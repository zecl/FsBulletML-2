namespace FsBulletML2.Front.Tests

open System
open System.Collections.Generic
open System.Text
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

/// 走らせている 1 弾。型の入れ子は書けないので namespace の高さに置く
[<Struct>]
type private Live =
  { mutable Run : BulletRun
    mutable X : float32
    mutable Y : float32
    mutable Alive : bool
    Id : int }

/// Front を通した走行と、Core を直に叩いた走行が同じ軌跡を出すこと。
///
/// Front が吸ったのは「同梱の 4 つ が同じ形で写していたもの」で、
/// 吸った時点で写しは消える。消したものと同じ答えを出しているかは、
/// ここでしか見られない —— 227 本 の突き合わせは Core の中を見ていて、
/// Front はその外に居る。
///
/// 2 本 走らせるので、乱数を二重に消費しない形にしてある。
/// 同じ列を独立に 2 本 用意して、それぞれに 1 本ずつ渡す。
[<TestFixture>]
type DriverEquiv() =

  /// 位置を持つだけの敵。一覧の要素
  let enemies : IReadOnlyList<struct (float32 * float32)> =
    [| struct (30.0f, -100.0f); struct (-60.0f, 40.0f); struct (5.0f, 5.0f) |] :> _

  let ex (struct (x, _): struct (float32 * float32)) = x
  let ey (struct (_, y): struct (float32 * float32)) = y

  let playerX = 20.0f
  let playerY = -80.0f

  /// 呼ぶたび進む決定的な列。同じ種から 2 本 作れば、同じ順で同じ値が出る
  let stream () =
    let mutable i = 0
    fun () ->
      i <- i + 1
      float32 ((i * 7919) % 1000) / 1000.0f

  /// Front の口を、一覧を持つフロントとして実装した側。
  /// `NearestEnemy` を弾 1 個 につき 1 個 持つ
  let front (rand: unit -> float32) =
    let near = NearestEnemy<struct (float32 * float32)>((fun () -> enemies), ex, ey)
    { new IFrontEnv with
        member _.Rand = rand
        member _.Rank = 0.5f
        member _.PlayerX = playerX
        member _.PlayerY = playerY
        member _.TryTargetFrom(x, y, tx, ty) = near.TryFrom(x, y, &tx, &ty)
        member _.TrySpawnTargetFrom(x, y, tx, ty) = near.TryNearest(x, y, &tx, &ty) }

  /// 直叩きの側が持つ、覚える相手。Front を使わずに同じ不変条件を書く
  let directTarget () =
    let mutable target = ValueNone
    fun (x: float32) (y: float32) ->
      match target with
      | ValueSome t -> ValueSome t
      | ValueNone ->
          let mutable best = Single.MaxValue
          let mutable found = ValueNone
          for i in 0 .. enemies.Count - 1 do
            let e = enemies.[i]
            let dx = ex e - x
            let dy = ey e - y
            let d = float32 (Math.Sqrt(float (dx * dx + dy * dy)))
            if best > d then
              best <- d
              found <- ValueSome e
          target <- found
          found

  let nearestNow (x: float32) (y: float32) =
    let mutable best = Single.MaxValue
    let mutable found = ValueNone
    for i in 0 .. enemies.Count - 1 do
      let e = enemies.[i]
      let dx = ex e - x
      let dy = ey e - y
      let d = float32 (Math.Sqrt(float (dx * dx + dy * dy)))
      if best > d then
        best <- d
        found <- ValueSome e
    found

  let atan2f (fx: float32) (fy: float32) (tx: float32) (ty: float32) (flip: bool) =
    let dy = ty - fy
    let dy = if flip then -dy else dy
    float32 (Math.Atan2(float (tx - fx), float dy))

  /// 直叩きの Env。Front が吸う前に同梱のフロントが書いていた形
  let directEnv (rand: unit -> float32) (target: float32 -> float32 -> ValueOption<struct (float32 * float32)>)
                (flip: bool) (spawnAtOrigin: bool) (x: float32) (y: float32) : Env =
    let sx, sy = if spawnAtOrigin then 0.0f, 0.0f else x, y
    let toEnemy =
      match target x y with
      | ValueSome e -> atan2f x y (ex e) (ey e) flip
      | ValueNone -> 0.0f
    let spawnToEnemy =
      match nearestNow sx sy with
      | ValueSome e -> atan2f sx sy (ex e) (ey e) flip
      | ValueNone -> 0.0f
    { Rand = rand
      Rank = 0.5f
      Aim = { ToPlayer = atan2f x y playerX playerY flip; ToEnemy = toEnemy }
      Spawn = { ToPlayer = atan2f sx sy playerX playerY flip; ToEnemy = spawnToEnemy } }


  /// Front を通した走行
  let runThroughFront (space: Space) (origin: SpawnOrigin) (rand: unit -> float32)
                      (xml: string) (frames: int) : string =
    let script = Runner.load rand 0.5f (Bulletml.readXmlString xml)
    let all = List<Live>()
    let worlds = List<IFrontEnv>()
    all.Add { Run = Runner.newRoot BulletType.Enemy script; X = 0.0f; Y = 0.0f; Alive = true; Id = 0 }
    worlds.Add(front rand)
    let sb = StringBuilder()
    for i in 0 .. frames - 1 do
      sb.AppendLine(sprintf "f%02d" i) |> ignore
      let liveCount = all.Count
      for j in 0 .. liveCount - 1 do
        let b = all.[j]
        if b.Alive then
          let w = worlds.[j]
          let motion = { b.Run.Motion with Pos = { X = b.X; Y = b.Y } }
          let f = Driver.step w space origin b.Run motion
          let mutable nb = b
          nb.X <- f.Run.Motion.Pos.X + f.Delta.X
          nb.Y <- f.Run.Motion.Pos.Y + f.Delta.Y
          nb.Run <- if f.Finished then Driver.restart w f.Run else f.Run
          if f.Vanished || f.Retired then nb.Alive <- false
          all.[j] <- nb
          sb.AppendLine(sprintf "  b%d x=%.6f y=%.6f d=%.6f s=%.6f%s"
                          nb.Id nb.X nb.Y f.Run.Motion.Dir f.Run.Motion.Speed
                          (if f.Vanished then " vanish" else "")) |> ignore
          for child in f.Spawned do
            all.Add { Run = child; X = child.Motion.Pos.X; Y = child.Motion.Pos.Y
                      Alive = true; Id = all.Count }
            worlds.Add(front rand)
    sb.ToString()

  /// Core を直に叩いた走行。Front を 1 行 も通らない
  let runDirect (flip: bool) (spawnAtOrigin: bool) (rand: unit -> float32)
                (xml: string) (frames: int) : string =
    let script = Runner.load rand 0.5f (Bulletml.readXmlString xml)
    let all = List<Live>()
    let targets = List<float32 -> float32 -> ValueOption<struct (float32 * float32)>>()
    all.Add { Run = Runner.newRoot BulletType.Enemy script; X = 0.0f; Y = 0.0f; Alive = true; Id = 0 }
    targets.Add(directTarget ())
    let sb = StringBuilder()
    let noAim : Env =
      { Rand = rand; Rank = 0.5f
        Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
        Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
    for i in 0 .. frames - 1 do
      sb.AppendLine(sprintf "f%02d" i) |> ignore
      let liveCount = all.Count
      for j in 0 .. liveCount - 1 do
        let b = all.[j]
        if b.Alive then
          let t = targets.[j]
          let motion = { b.Run.Motion with Pos = { X = b.X; Y = b.Y } }
          let env =
            if b.Run.HasNoScript then noAim
            else directEnv rand t flip spawnAtOrigin b.X b.Y
          let f = Runner.stepWith env b.Run motion
          let mutable nb = b
          nb.X <- f.Run.Motion.Pos.X + f.Delta.X
          nb.Y <- f.Run.Motion.Pos.Y + f.Delta.Y
          nb.Run <-
            if f.Finished then
              let renv =
                if f.Run.HasNoScript then noAim
                else directEnv rand t flip spawnAtOrigin nb.X nb.Y
              Runner.restart renv f.Run
            else f.Run
          if f.Vanished || f.Retired then nb.Alive <- false
          all.[j] <- nb
          sb.AppendLine(sprintf "  b%d x=%.6f y=%.6f d=%.6f s=%.6f%s"
                          nb.Id nb.X nb.Y f.Run.Motion.Dir f.Run.Motion.Speed
                          (if f.Vanished then " vanish" else "")) |> ignore
          for child in f.Spawned do
            all.Add { Run = child; X = child.Motion.Pos.X; Y = child.Motion.Pos.Y
                      Alive = true; Id = all.Count }
            targets.Add(directTarget ())
    sb.ToString()

  let compareAll (space: Space) (origin: SpawnOrigin) (flip: bool) (spawnAtOrigin: bool) =
    let files = CorpusData.uniqueSamples ()
    files |> List.length |> should be (greaterThan 100)
    let mutable compared = 0
    let mutable diverged = []
    for path in files do
      let xml = IO.File.ReadAllText path
      try
        let a = stream ()
        let b = stream ()
        let fa = runThroughFront space origin a xml 20
        let fb = runDirect flip spawnAtOrigin b xml 20
        compared <- compared + 1
        if fa <> fb then diverged <- path :: diverged
      with
      // 読む段で落ちる弾幕（DTD 違反）はここでも比べられない。
      // 数を下で押さえるので、握って進む
      | _ -> ()
    // 当てる先が本当に在るかを数で押さえる。
    // 全部 例外に吸われて「0 本 比べて緑」になっても気づけない
    compared |> should be (greaterThan 200)
    if not (List.isEmpty diverged) then
      Assert.Fail(sprintf "%d 本 で軌跡が割れた。最初: %s"
                    (List.length diverged) (List.head (List.rev diverged)))
    TestContext.WriteLine(sprintf "比べた台本: %d 本" compared)

  [<Test>]
  member _.``MonoGame の並び: Front 経由と直叩きが同じ軌跡``() =
    compareAll Space.YDown SpawnOrigin.AtOrigin true true

  [<Test>]
  member _.``Unity2D の並び: Front 経由と直叩きが同じ軌跡``() =
    compareAll Space.YUp SpawnOrigin.AtShooter false false

  /// 較正。 上の 2 本 が「何を入れても通る」門になっていないことを見る。
  /// 座標系を取り違えた組み合わせでは割れるはず
  [<Test>]
  member _.``較正: 座標系を取り違えると割れる``() =
    let files = CorpusData.uniqueSamples ()
    let mutable compared = 0
    let mutable diverged = 0
    for path in files do
      let xml = IO.File.ReadAllText path
      try
        let a = stream ()
        let b = stream ()
        let fa = runThroughFront Space.YDown SpawnOrigin.AtOrigin a xml 20
        // 直叩きの側だけ Unity2D の座標系にする
        let fb = runDirect false false b xml 20
        compared <- compared + 1
        if fa <> fb then diverged <- diverged + 1
      with
      | _ -> ()
    compared |> should be (greaterThan 200)
    // 全部 割れる必要は無い（aim を一度も読まない弾幕が在る）が、
    // 1 本 も割れないなら門が効いていない
    diverged |> should be (greaterThan 50)
    TestContext.WriteLine(sprintf "%d 本 中 %d 本 で割れた" compared diverged)
