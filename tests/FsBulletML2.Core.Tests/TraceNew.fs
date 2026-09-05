namespace FsBulletML2.Core.Tests

open System
open System.Collections.Generic
open System.Globalization
open System.Text
open FsBulletML2
open FsBulletML2.Domain

/// 新経路で、Trace と同じ書式の軌跡を作る。
///
/// 書式が違うと差分が全行に出て橋が使えない。Trace.fs の fmt と
/// 出力の並びをそのまま写してある。
module TraceNew =

  let private fmt (v: float32) =
    let r = Math.Round(float v, TraceFormat.digits)
    let r = if r = 0.0 then 0.0 else r
    r.ToString("F" + string TraceFormat.digits, CultureInfo.InvariantCulture)

  /// FakeBullet.GetAimDir と同じ式。ずれると全弾幕が割れる
  let private aimDir (px: float32) (py: float32) (pos: Vec2) =
    float32 (Math.Atan2(float (px - pos.X), float -(py - pos.Y)))

  let private enemyAimDir (pos: Vec2) =
    float32 (Math.Atan2(float (FakeEnemy.X - pos.X), -1.0 * float (FakeEnemy.Y - pos.Y)))

  type private Live =
    { mutable St : BulletState
      mutable Alive : bool
      mutable Vanished : int
      Id : int }

  let run (rand: unit -> float32) (rank: float32) (px: float32) (py: float32)
          (xml: string) (frames: int) : string =
    let bulletml = readXmlString xml
    let rec' = BulletmlRead.foldConstants bulletml
    let resolvers : Step.Resolvers =
      { Bullet = BulletmlOps.expandBulletRefOnce rec'
        Action = BulletmlOps.expandActionRefOnce rec' }
    // top* の並びは旧の toProcessable と同じ選び方
    let scripts =
      rec'
      |> BulletmlOps.getAction
      |> List.filter (function
        | ActionElm.Action (attrs, _) ->
            match attrs.actionLabel with
            | Some label -> (ActionLabel.text label).StartsWith "top"
            | None -> false
        | _ -> false)
      |> List.map (BulletmlOps.convertRefActionElm rec')

    // 根の Tops は Progress.initial では組めない。旧の toProcessable は
    // 木を組む段で wait の term だけをその場で引く（BulletmlRead.fs の
    // Action.Wait の腕、convertRecBulletmlEx から）。この段の Env は
    // 撃つ弾ごとの位置がまだ無いので AimDir / EnemyAimDir を 0 に固定し、
    // Rand / Rank はグローバルと同じ値を渡す（設計文書 5.6）。
    // accel / changeDirection / changeSpeed はこの段では引かないので、
    // resetChild ではなく rootProgress を通す
    // 産まれた弾がどこに出るかは、対になる FakeBullet.GetNewBullet が決める。
    // あちらは FakeBullet(id, born) を位置を入れずに作るので原点。
    // 同じ式に原点を入れた値を env に載せる（Fake.fs の GetSpawnAimDir と
    // 同じ値になるようにしてある。片方だけ直すと橋が割れる）
    let origin = { X = 0.f; Y = 0.f }
    let spawnAim = aimDir px py origin
    let spawnEnemyAim = enemyAimDir origin
    let rootEnv : Env =
      { Rand = rand; Rank = rank; AimDir = 0.f; EnemyAimDir = 0.f
        SpawnAimDir = spawnAim; SpawnEnemyAimDir = spawnEnemyAim }
    let initial =
      { Pos = { X = 0.f; Y = 0.f }
        Speed = 0.f
        Dir = 0.f
        Accel = { X = 0.f; Y = 0.f }
        Kind = BulletType.Enemy
        IsBullet = false
        HasFired = false
        Tops = scripts |> List.map (fun s -> s, Step.rootProgressActionElm rootEnv s, FireContext.zero) }

    let all = List<Live>()
    all.Add { St = initial; Alive = true; Vanished = 0; Id = 0 }
    let sb = StringBuilder()
    let mutable seen = 1

    for i in 0 .. frames - 1 do
      sb.AppendLine(sprintf "f%02d" i) |> ignore
      let liveCount = all.Count
      for j in 0 .. liveCount - 1 do
        let b = all.[j]
        if b.Alive then
          let env =
            { Rand = rand
              Rank = rank
              AimDir = aimDir px py b.St.Pos
              EnemyAimDir = enemyAimDir b.St.Pos
              SpawnAimDir = spawnAim
              SpawnEnemyAimDir = spawnEnemyAim }
          let r = Step.step resolvers env b.St
          let vanishedNow = r.Effects |> List.exists (fun e -> e = Vanished)
          let st = { r.State with Pos = { X = r.State.Pos.X + r.Delta.X
                                          Y = r.State.Pos.Y + r.Delta.Y } }
          // Trace は Processed のとき task.Init(envOfGlobal o) を呼んで
          // 回し直す。Original が None の task の Init は tasks を
          // Init(env) で歩くだけで、これは Progress.initial ではなく
          // Step.resetChild が写している（wait / changeDirection /
          // changeSpeed を引き直す。ruling 5.6 / 5.3 参照）。
          // 引き直しの Env は envOfGlobal と同じく、移動後の位置から組む
          let st =
            if r.Finished then
              let reinitEnv =
                { Rand = rand
                  Rank = rank
                  AimDir = aimDir px py st.Pos
                  EnemyAimDir = enemyAimDir st.Pos
                  SpawnAimDir = spawnAim
                  SpawnEnemyAimDir = spawnEnemyAim }
              { st with
                  Tops =
                    st.Tops
                    |> List.map (fun (s, _, fc) -> s, Step.resetChildActionElm reinitEnv s, fc) }
            else st
          b.St <- st
          if vanishedNow then
            b.Vanished <- b.Vanished + 1
            b.Alive <- false
          if r.Retired then b.Alive <- false
          let mark = if r.Finished then "P+" else "P-"
          sb.Append(sprintf "  b%d %s x=%s y=%s d=%s s=%s"
                      b.Id mark (fmt st.Pos.X) (fmt st.Pos.Y) (fmt st.Dir) (fmt st.Speed)) |> ignore
          if vanishedNow then sb.Append(" vanish") |> ignore
          sb.AppendLine() |> ignore
          // 撃たれた弾を並びへ足す。
          //
          // bullet 側の direction が aim 系のときも、stepFire が env.SpawnAimDir
          // で解決し終えている。ここで実体を見て仕上げる後処理は要らない
          for e in r.Effects do
            match e with
            | Spawn child -> all.Add { St = child; Alive = true; Vanished = 0; Id = all.Count }
            | Vanished -> ()
      while seen < all.Count do
        let b = all.[seen]
        sb.AppendLine(sprintf "  +b%d d=%s s=%s" b.Id (fmt b.St.Dir) (fmt b.St.Speed)) |> ignore
        seen <- seen + 1

    sb.ToString().Replace("\r\n", "\n")
