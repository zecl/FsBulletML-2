namespace FsBulletML2.Unity2D

open UnityEngine
open System
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

type DefaultBullet (transform:Transform) as this =

  /// 走らせている弾幕と、その実行状態。旧の Task option を 2 つ に割ったもの
  let mutable script : BulletmlScript option = None
  let mutable run : BulletRun option = None
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish の置き場所
  let mutable finished = false

  let self () = this :> IDefaultBullet

  /// この弾から見た世界。弾 1 個 につき 1 個。
  /// 狙う相手を覚えるのが弾ごとなので使い回せない
  let front = Unity2DEnv () :> IFrontEnv

  /// 撃たれた弾の実体を作る。旧 GetNewBullet が呼んでいたもの。
  /// 既定は null。null のときは実体を作らずに進む。
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
    member val Radius = 0.1f with get, set

    member _.Script = run |> Option.map (fun r -> r.Script)
    member _.Finished = finished

    /// 弾幕を割り当てて根から始める。
    /// BulletType と IsBullet はこれを呼ぶ前に立てておくこと。
    member _.SetScript (s) =
      finished <- false
      let me = self ()
      run <-
        s |> Option.map (fun sc ->
          if me.IsBullet then Runner.newShot me.BulletType sc
          else Runner.newRoot me.BulletType sc)

    /// 撃たれた弾を、エンジンから受け取った実行状態で始める。
    /// 弾幕を渡す口が無い —— `BulletRun` が親のものを持っている
    member _.SetRun (r: BulletRun) =
      finished <- false
      run <- Some r

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
      run <-
        run |> Option.map (fun r ->
          Driver.restart front r)

    member this.Update () =
      let me = this :> IDefaultBullet
      let apply x y =
        me.X <- me.X + (x / Settings.Display.PixcelsToUnits)
        me.Y <- me.Y - (y / Settings.Display.PixcelsToUnits)
        let angle = -(me.Dir) * Mathf.Rad2Deg
        transform.rotation <- Quaternion.AngleAxis(angle, new Vector3(0.f, 0.f, 1.f))

      this.RunTask(FSharpFunc.ToAction2 apply)


  /// 撃たれた弾を実体にする。旧 GetNewBullet ＋ applySpawn の合わせ。
  /// 実体が作れなければここで捨てる。エンジンには何も返さない。
  member private this.Spawn (child: BulletRun) =
    let newBullet = this.GetBulletPrefubInstance ()
    if newBullet :> obj <> null then
      let motion = child.Motion
      newBullet.Init ()
      newBullet.IsBullet <- true
      newBullet.BulletType <- (self ()).BulletType
      // 弾幕は親と同じものを引き継ぐ。引き継ぎ忘れる書き方がもう無い
      // —— BulletRun が弾幕を持っている
      newBullet.SetRun child
      newBullet.X <- motion.Pos.X
      newBullet.Y <- motion.Pos.Y
      newBullet.Dir <- motion.Dir
      newBullet.Speed <- motion.Speed

  member this.RunTask(apply:Action<_,_>) =
    let apply = Action.toFSharpFunc2 apply
    let me = self ()
    match run with
    | Some rn ->
        me.ShootingDirection <- rn.Script.ShootingDirection
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）
        let motion : Motion =
          { Pos = { X = me.X; Y = me.Y }
            Speed = me.Speed
            Dir = me.Dir
            Accel = { X = me.AccelerationX; Y = me.AccelerationY } }
        // 台本が無い弾は aim を読まない（BulletRun.HasNoScript の但し書き）
        // Env を組む位置も、台本が無い弾の枝も Driver が持っている
        let f = Driver.step front Unity2DFront.space Unity2DFront.origin rn motion
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
            Some (Driver.restart front f.Run)
          else Some f.Run
    | _ -> ()
