namespace FsBulletML2.Core.Tests

open System
open System.Collections.Generic
open System.Globalization
open System.Text
open FsBulletML2
open FsBulletML2.Domain

/// 公開 API（Runner / BulletmlScript / BulletRun / Body / Frame / Env）だけで、
/// Trace と同じ書式の軌跡を作る。
module TraceApi =

  let private fmt (v: float32) =
    let r = Math.Round(float v, TraceFormat.digits)
    let r = if r = 0.0 then 0.0 else r
    r.ToString("F" + string TraceFormat.digits, CultureInfo.InvariantCulture)

  /// FakeBullet.GetAimDir と同じ式。ずれると全弾幕が割れる
  let private aimDir (px: float32) (py: float32) (x: float32) (y: float32) =
    float32 (Math.Atan2(float (px - x), float -(py - y)))

  let private enemyAimDir (x: float32) (y: float32) =
    float32 (Math.Atan2(float (FakeEnemy.X - x), -1.0 * float (FakeEnemy.Y - y)))

  type private Live =
    { mutable Run : BulletRun
      mutable X : float32
      mutable Y : float32
      mutable Alive : bool
      Id : int }

  /// `run` の、走行の途中で rank や自機の位置を動かせる形。
  let runDetailed (rootKind: BulletType) (onFrame: int -> unit)
                  (rand: unit -> float32) (rank: unit -> float32)
                  (px: unit -> float32) (py: unit -> float32)
                  (xml: string) (frames: int) : string * BulletRun list =
    // 産まれた弾がどこに出るかは、対になる FakeBullet.GetNewBullet が決める。
    // あちらは位置を入れずに作るので原点。同じ式に原点を入れた値を載せる
    let spawnAim () = aimDir (px ()) (py ()) 0.0f 0.0f
    let spawnEnemyAim () = enemyAimDir 0.0f 0.0f
    let envAt (x: float32) (y: float32) : Env =
      { Rand = rand
        Rank = rank ()
        Aim = { ToPlayer = aimDir (px ()) (py ()) x y; ToEnemy = enemyAimDir x y }
        Spawn = { ToPlayer = spawnAim (); ToEnemy = spawnEnemyAim () } }

    // 木を組む段。撃つ弾ごとの位置がまだ無いので aim は 0 で組む
    let rootEnv : Env =
      { Rand = rand; Rank = rank (); Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }; Spawn = { ToPlayer = spawnAim (); ToEnemy = spawnEnemyAim () } }

    /// aim を読まないと分かっているコマの Env。同梱フロントの noAimEnv と
    /// 同じ形（aim 4 本 を 0 に、Rand / Rank はそのまま）
    let noAimEnv () : Env =
      { Rand = rand; Rank = rank (); Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }; Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
    let script = Runner.load rand (rank ()) (readXmlString xml)

    let all = List<Live>()
    // 根の種別はここで 1 回 だけ決まる。撃たれた弾は Core が親から継ぐ
    all.Add { Run = Runner.newRoot rootKind script; X = 0.0f; Y = 0.0f; Alive = true; Id = 0 }
    // 産まれた弾を産まれた順に控える。軌跡の文字列には出ない面
    // （BulletType など）を見る試験のため
    let spawnedRuns = List<BulletRun>()
    let sb = StringBuilder()
    let mutable seen = 1

    for i in 0 .. frames - 1 do
      onFrame i
      sb.AppendLine(sprintf "f%02d" i) |> ignore
      let liveCount = all.Count
      for j in 0 .. liveCount - 1 do
        let b = all.[j]
        if b.Alive then
          // 物理量はフロントが持っている。毎コマ入れ直す（旧の stateOfBullet）。
          // 種別はもう渡らない —— 根は newRoot で、撃たれた弾は親から継ぐ
          let motion = { b.Run.Motion with Pos = { X = b.X; Y = b.Y } }
          // 同梱フロントと同じ skip をここでも通す。
          let env = if b.Run.HasNoScript then noAimEnv () else envAt b.X b.Y
          let f = Runner.stepWith env b.Run motion
          b.X <- f.Run.Motion.Pos.X + f.Delta.X
          b.Y <- f.Run.Motion.Pos.Y + f.Delta.Y
          // 走らせ直しの Env は、移動したあとの位置から組む（旧 BaseBullet が
          // apply のあとで envOfGlobal を呼ぶのと同じ順）
          b.Run <-
            if f.Finished then
              let renv = if f.Run.HasNoScript then noAimEnv () else envAt b.X b.Y
              Runner.restart renv f.Run
            else f.Run
          if f.Vanished || f.Retired then b.Alive <- false
          let mark = if f.Finished then "P+" else "P-"
          sb.Append(sprintf "  b%d %s x=%s y=%s d=%s s=%s"
                      b.Id mark (fmt b.X) (fmt b.Y)
                      (fmt f.Run.Motion.Dir) (fmt f.Run.Motion.Speed)) |> ignore
          if f.Vanished then sb.Append(" vanish") |> ignore
          sb.AppendLine() |> ignore
          for child in f.Spawned do
            spawnedRuns.Add child
            all.Add { Run = child
                      X = child.Motion.Pos.X
                      Y = child.Motion.Pos.Y
                      Alive = true
                      Id = all.Count }
      while seen < all.Count do
        let b = all.[seen]
        sb.AppendLine(sprintf "  +b%d d=%s s=%s" b.Id (fmt b.Run.Motion.Dir) (fmt b.Run.Motion.Speed)) |> ignore
        seen <- seen + 1

    sb.ToString().Replace("\r\n", "\n"), List.ofSeq spawnedRuns

  /// 根は敵。軌跡だけ要るとき
  let runWithParams (onFrame: int -> unit)
                    (rand: unit -> float32) (rank: unit -> float32)
                    (px: unit -> float32) (py: unit -> float32)
                    (xml: string) (frames: int) : string =
    runDetailed BulletType.Enemy onFrame rand rank px py xml frames |> fst

  /// 根の種別を変えて回し、産まれた弾だけを産まれた順に出す。
  let runSpawnedAs (kind: BulletType) (rand: unit -> float32) (rank: float32)
                   (px: float32) (py: float32) (xml: string) (frames: int) : string =
    let _, runs =
      runDetailed kind ignore rand (fun () -> rank) (fun () -> px) (fun () -> py) xml frames
    runs
    |> List.mapi (fun i (r: BulletRun) ->
        sprintf "+b%d d=%s s=%s type=%A" (i + 1) (fmt r.Motion.Dir) (fmt r.Motion.Speed) r.Kind)
    |> String.concat "\n"
    |> fun s -> if s = "" then s else s + "\n"

  /// 値を動かさない走行。
  let run (rand: unit -> float32) (rank: float32) (px: float32) (py: float32)
          (xml: string) (frames: int) : string =
    runWithParams ignore rand (fun () -> rank) (fun () -> px) (fun () -> py) xml frames
