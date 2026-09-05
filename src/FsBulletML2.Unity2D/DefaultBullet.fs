namespace FsBulletML2.Unity2D

open UnityEngine
open System
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2
open FsBulletML2.Domain

type DefaultBullet (transform:Transform) as this =

  /// 走らせている弾幕と、その実行状態。旧の Task option を 2 つ に割ったもの
  let mutable script : BulletmlScript option = None
  let mutable run : BulletRun option = None
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish の置き場所
  let mutable finished = false

  let self () = this :> IDefaultBullet

  /// 撃たれた弾の実体を作る。旧 GetNewBullet が呼んでいたもの。
  ///
  /// **既定は null を返す**（サンプルの未実装）。旧はエンジンがここを
  /// 呼び返して、null なら「撃たなかったこと」にして fire の累積
  /// （SrcSpeed / SpeedInit）を巻き戻していた。新 API は撃つ弾を値で
  /// 返しきるので、その巻き戻しは無い —— null のときは実体を作らずに
  /// 進む。同梱サンプル 4 つ は全部 override しているので届かない経路で、
  /// override し忘れた状態は弾が 1 発 も出ないので動くゲームでは観測できない。
  abstract member GetBulletPrefubInstance: unit -> IDefaultBullet
  default this.GetBulletPrefubInstance () = defaultof<IDefaultBullet>

  interface IDefaultBullet with
    member val Root = false with get, set
    member this.Pos with get () = transform.position
                       and set (v) = transform.position <- v
    member this.X with get () = transform.position.x
                    and set (v) = transform.position <- Vector3(v, transform.position.y, transform.position.z)
    member this.Y with get () = transform.position.y
                    and set (v) = transform.position <- Vector3(transform.position.x, v, transform.position.z)
    member val AccelerationX = 0.f with get, set
    member val AccelerationY = 0.f with get, set
    member val Speed = 0.f with get, set
    member val Dir = 0.f with get, set
    member val Used = false with get, set
    member val IsBullet = false with get, set
    member val BulletRoot = false with get, set
    member val BulletType = BulletType.Enemy with get, set
    // Core の既定（Runner.load）と揃えてある。走らせた弾は
    // script.ShootingDirection で上書きされるので、この値が出るのは
    // まだ 1 度も走らせていない弾だけ
    member val ShootingDirection = ShootingDirection.BulletVertical with get, set
    member val TargetEnemy = defaultof<IDefaultBullet> with get, set
    member val Radius = 0.1f with get, set

    member _.Script = script
    member _.Finished = finished

    /// 弾幕を割り当てる。`r` が None なら根から始める。
    ///
    /// **根の立場（狙う先と、撃たれた弾か）はここで 1 回 だけ決まる。**
    /// Core へは毎コマ渡らないので、BulletType と IsBullet はこれを呼ぶ前に
    /// 立てておくこと（同梱の弾はどれも Awake で立てている）
    member _.SetScript (s, r) =
      script <- s
      finished <- false
      let me = self ()
      run <- match r with
             | Some _ -> r
             | None ->
                 s |> Option.map (fun sc ->
                   if me.IsBullet then Runner.newShot me.BulletType sc
                   else Runner.newRoot me.BulletType sc)

    member this.Vanish () = (this :> IDefaultBullet).Used <- false

    member this.Init () =
      let me = this :> IDefaultBullet
      me.Root <- false
      me.Used <- true
      me.BulletRoot <- false
      me.AccelerationX <- 0.f
      me.AccelerationY <- 0.f
      me.Speed <- 0.f
      me.Dir <- 0.f
      me.IsBullet <- true

      // 旧はここで task.Init(envOfGlobal self) を呼んで木を歩き直していた。
      // この時点の位置（fire からの呼び出しでは、まだ親の位置へ移す前）で
      // 組むところも旧のまま
      run <- run |> Option.map (fun r -> Runner.restart (this.EnvNow ()) r)

    member this.Update () =
      let me = this :> IDefaultBullet
      let apply x y =
        me.X <- me.X + (x / Settings.Display.PixcelsToUnits)
        me.Y <- me.Y - (y / Settings.Display.PixcelsToUnits)
        let angle = -(me.Dir) * Mathf.Rad2Deg
        transform.rotation <- Quaternion.AngleAxis(angle, new Vector3(0.f, 0.f, 1.f))

      this.RunTask(FSharpFunc.ToAction2 apply)

  /// 自機を狙う向き。旧 GetAimDir の式そのまま
  member private this.AimDir () =
    let me = self ()
    float32 (Math.Atan2(float (BulletMLManager.GetPlayerPosX() - me.X),
                        float (BulletMLManager.GetPlayerPosY() - me.Y)))

  /// いちばん近い敵を狙う向き。旧 GetEnemyAimDir の式そのまま。
  /// 選んだ相手を TargetEnemy に覚えるところも旧と同じ
  member private this.EnemyAimDir () =
    let me = self ()
    if me.TargetEnemy :> obj <> null then
      Mathf.Atan2((me.TargetEnemy.X - me.X), 1.f * (me.TargetEnemy.Y - me.Y))
    elif ((Manager.enemies) :> seq<_>) |> Seq.length <= 0 then 0.f
    else
      let mutable md = Single.MaxValue
      for enemy in Manager.enemies do
        let d = Vector3.Distance (me.Pos, Vector3(enemy.X, enemy.Y))
        if md > d then
          me.TargetEnemy <- enemy
          md <- d
      Mathf.Atan2((me.TargetEnemy.X - me.X), 1.f * (me.TargetEnemy.Y - me.Y))

  /// このコマの Env。旧 BulletRunner.envOfGlobal の写し。
  ///
  /// **産まれる弾の向きが MonoGame と違う。** このフロントの
  /// GetBulletPrefubInstance は撃った側と同じ場所に作るので、
  /// Spawn は Aim と同じ値になる（MonoGame は原点に作るので別式）。
  /// 旧 GetSpawnAimDir / GetSpawnEnemyAimDir の但し書きをそのまま写した
  member private this.EnvNow () : Env =
    let aim = this.AimDir ()
    let enemyAim = this.EnemyAimDir ()
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      Aim = { ToPlayer = aim; ToEnemy = enemyAim }
      Spawn = { ToPlayer = aim; ToEnemy = enemyAim } }

  /// 撃たれた弾を実体にする。旧 GetNewBullet ＋ applySpawn の合わせ
  ///
  /// **実体が作れなければ、ここで捨てる。エンジンには何も返さない。**
  /// エンジンの側では撃った扱いのままで、fire の sequence の累積も進んでいる
  /// （Frame.Spawned に入った時点で確定している）。
  ///
  /// 旧 API はここで累積を巻き戻していたが、**参照実装 2 本 のどちらにも無い
  /// 振る舞い**だったので新 API へは持ってきていない
  /// （根拠は Core の Api.fs、Frame.Spawned の但し書き）。
  /// 弾プールの尽きは、こちら側の都合として こちら側で終わらせる。
  member private this.Spawn (child: BulletRun) =
    let newBullet = this.GetBulletPrefubInstance ()
    if newBullet :> obj <> null then
      let motion = child.Motion
      newBullet.Init ()
      newBullet.IsBullet <- true
      newBullet.BulletType <- (self ()).BulletType
      // 弾幕は親と同じものを引き継ぐ。引き継がないと、弾の中に残った
      // bulletRef / actionRef を誰も解けない
      newBullet.SetScript (script, Some child)
      newBullet.X <- motion.Pos.X
      newBullet.Y <- motion.Pos.Y
      newBullet.Dir <- motion.Dir
      newBullet.Speed <- motion.Speed

  member this.RunTask(apply:Action<_,_>) =
    let apply = Action.toFSharpFunc2 apply
    let me = self ()
    match script, run with
    | Some sc, Some rn ->
        me.ShootingDirection <- sc.ShootingDirection
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）
        let motion : Motion =
          { Pos = { X = me.X; Y = me.Y }
            Speed = me.Speed
            Dir = me.Dir
            Accel = { X = me.AccelerationX; Y = me.AccelerationY } }
        // 台本が無い弾は aim を読まない（BulletRun.HasNoScript の但し書き）
        let env = if rn.HasNoScript then noAimEnv () else this.EnvNow ()
        let f = Runner.stepWith sc env rn motion
        let after = f.Run.Motion
        me.Speed <- after.Speed
        me.Dir <- after.Dir
        me.AccelerationX <- after.Accel.X
        me.AccelerationY <- after.Accel.Y
        apply f.Delta.X f.Delta.Y
        finished <- f.Finished
        for child in f.Spawned do this.Spawn child
        if f.Vanished then me.Vanish ()
        if f.Retired then me.Used <- false
        // 走らせ直しの Env は、位置を更新したあとの自分から組む
        // （旧 DefaultBullet が apply のあとで envOfGlobal を呼ぶのと同じ順）
        run <-
          if f.Finished then
            let renv = if f.Run.HasNoScript then noAimEnv () else this.EnvNow ()
            Some (Runner.restart renv f.Run)
          else Some f.Run
    | _ -> ()
