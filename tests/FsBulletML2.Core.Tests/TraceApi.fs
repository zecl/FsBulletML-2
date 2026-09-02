namespace FsBulletML2.Core.Tests

open System
open System.Collections.Generic
open System.Globalization
open System.Text
open FsBulletML2
open FsBulletML2.Domain

/// 公開 API（Runner / BulletmlScript / BulletRun / Body / Frame / Env）だけで、
/// Trace と同じ書式の軌跡を作る。**旧経路と一致するかを見るのがここの仕事。**
///
/// **「internal を使っていない」はここでは強制されない。**
/// このアセンブリは InternalsVisibleTo に入っているので、うっかり
/// BulletState や Progress を触ってもコンパイラは通してしまう。
/// 強制が掛かるのはフロント 2 つ（MonoGame / Unity2D。どちらも
/// InternalsVisibleTo に入っていない）を新 API へ移したときで、
/// **そちらが「公開が足りているか」の本当の門。**
/// ここは「足りている公開だけで書いたとき、値が旧と合うか」を見る。
///
/// open しているのは FsBulletML2 と FsBulletML2.Domain の 2 つ だけ。
/// Domain には internal も同居しているが、ここで使うのは Env / Vec2 /
/// FireContext（どれも公開）。3 つめ を open したくなったら、
/// 公開の置き場所を見直す印。
///
/// 書式は Trace.fs の fmt と並びをそのまま写す。違うと差分が全行に出て
/// 橋が使えない。
module TraceApi =

  let private fmt (v: float32) =
    let r = Math.Round(float v, Trace.digits)
    let r = if r = 0.0 then 0.0 else r
    r.ToString("F" + string Trace.digits, CultureInfo.InvariantCulture)

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

  let run (rand: unit -> float32) (rank: float32) (px: float32) (py: float32)
          (xml: string) (frames: int) : string =
    // 産まれた弾がどこに出るかは、対になる FakeBullet.GetNewBullet が決める。
    // あちらは位置を入れずに作るので原点。同じ式に原点を入れた値を載せる
    let spawnAim = aimDir px py 0.0f 0.0f
    let spawnEnemyAim = enemyAimDir 0.0f 0.0f
    let envAt (x: float32) (y: float32) : Env =
      { Rand = rand
        Rank = rank
        AimDir = aimDir px py x y
        EnemyAimDir = enemyAimDir x y
        SpawnAimDir = spawnAim
        SpawnEnemyAimDir = spawnEnemyAim }

    // 木を組む段。撃つ弾ごとの位置がまだ無いので aim は 0 で組む
    let rootEnv : Env =
      { Rand = rand; Rank = rank; AimDir = 0.0f; EnemyAimDir = 0.0f
        SpawnAimDir = spawnAim; SpawnEnemyAimDir = spawnEnemyAim }

    /// aim を読まないと分かっているコマの Env。同梱フロントの noAimEnv と
    /// 同じ形（aim 4 本 を 0 に、Rand / Rank はそのまま）
    let noAimEnv () : Env =
      { Rand = rand; Rank = rank; AimDir = 0.0f; EnemyAimDir = 0.0f
        SpawnAimDir = 0.0f; SpawnEnemyAimDir = 0.0f }
    let script = Runner.load rootEnv (readXmlString xml)

    let all = List<Live>()
    all.Add { Run = Runner.newRoot script; X = 0.0f; Y = 0.0f; Alive = true; Id = 0 }
    let sb = StringBuilder()
    let mutable seen = 1

    for i in 0 .. frames - 1 do
      sb.AppendLine(sprintf "f%02d" i) |> ignore
      let liveCount = all.Count
      for j in 0 .. liveCount - 1 do
        let b = all.[j]
        if b.Alive then
          // 物理量はフロントが持っている。毎コマ入れ直す（旧の stateOfBullet）
          let body = { b.Run.Body with Pos = { X = b.X; Y = b.Y } }
          // **同梱フロントと同じ skip をここでも通す。** 通さないと、この橋は
          // 本番と違う経路を見ることになり、skip の条件が間違っていても
          // 227 本 が緑のまま通ってしまう（BulletRun.HasNoScript の但し書き）
          let env = if b.Run.HasNoScript then noAimEnv () else envAt b.X b.Y
          let f = Runner.stepWith script env b.Run body
          b.X <- f.Run.Body.Pos.X + f.Delta.X
          b.Y <- f.Run.Body.Pos.Y + f.Delta.Y
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
                      (fmt f.Run.Body.Dir) (fmt f.Run.Body.Speed)) |> ignore
          if f.Vanished then sb.Append(" vanish") |> ignore
          sb.AppendLine() |> ignore
          for child in f.Spawned do
            all.Add { Run = child
                      X = child.Body.Pos.X
                      Y = child.Body.Pos.Y
                      Alive = true
                      Id = all.Count }
      while seen < all.Count do
        let b = all.[seen]
        sb.AppendLine(sprintf "  +b%d d=%s s=%s" b.Id (fmt b.Run.Body.Dir) (fmt b.Run.Body.Speed)) |> ignore
        seen <- seen + 1

    sb.ToString().Replace("\r\n", "\n")
