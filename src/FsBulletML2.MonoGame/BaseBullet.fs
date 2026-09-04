namespace FsBulletML2.MonoGame

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2
open FsBulletML2.Domain

type BaseBullet () as this =
  [<DefaultValue>]val mutable pos : Vector2
  [<DefaultValue>]val mutable private self : IBullet

  /// 走らせている弾幕と、その実行状態。旧の Task option を 2 つ に割ったもの
  let mutable script : BulletmlScript option = None
  let mutable run : BulletRun option = None
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish の置き場所
  let mutable finished = false

  // 自機・産まれる弾の向きを出す式と、Env の組み立ては FrontEnv.fs に出した。
  // **散らしておくと、4 本 の aim の取り違えを門で当てられない**
  // （型はどれも float32 なので入れ替えても通る）

  interface IBullet with
    member this.Pos with get () = this.pos
                     and set (v) = this.pos <- v
    member this.X with get () = this.pos.X
                    and set (v) = this.pos.X <- v
    member this.Y with get () = this.pos.Y
                    and set (v) = this.pos.Y <- v
    member val AccelerationX = 0.f with get, set
    member val AccelerationY = 0.f with get, set
    member val Speed = 0.f with get, set
    member val Dir = 0.f with get, set
    member val Used = false with get, set
    member val IsBullet = false with get, set
    member val BulletType = BulletType.Enemy with get, set
    // Core の既定（Runner.load）と揃えてある。走らせた弾は
    // script.ShootingDirection で上書きされるので、この値が出るのは
    // まだ 1 度も走らせていない弾だけ
    member val ShootingDirection = ShootingDirection.BulletVertical with get, set
    member val TargetEnemy = defaultof<IBullet> with get, set
    member val Radius = 0.f with get, set

    member _.Script = script
    member _.Finished = finished

    member _.SetScript (s, r) =
      script <- s
      finished <- false
      run <- match r with
             | Some _ -> r
             | None -> s |> Option.map Runner.newRoot

    member this.Vanish () = this.self.Used <- false

    member this.Init () =
      this.self.Used <- true

    member this.Update () =
      let apply x y = this.self.X <- this.self.X + x; this.self.Y <- this.self.Y + y
      this.RunTask(FSharpFunc.ToAction2 apply)

  /// いちばん近い敵を狙う向き。旧 GetEnemyAimDir の式そのまま。
  /// 選んだ相手を TargetEnemy に覚えるところも旧と同じ
  /// （覚えないと毎コマ選び直して相手が入れ替わり、軌跡が変わる）
  member private this.EnemyAimDirAt (x: float32) (y: float32) =
    if this.self.TargetEnemy :> obj <> null then
      float32 (Math.Atan2(float (this.self.TargetEnemy.X - x),
                          -1.0 * float (this.self.TargetEnemy.Y - y)))
    elif ((Manager.enemies) :> seq<_>) |> Seq.length <= 0 then 0.0f
    else
      let mutable md = Single.MaxValue
      for enemy in Manager.enemies do
        let d = Vector2.Distance (Vector2(x, y), Vector2(enemy.X, enemy.Y))
        if md > d then
          this.self.TargetEnemy <- enemy
          md <- d
      float32 (Math.Atan2(float (this.self.TargetEnemy.X - x),
                          -1.0 * float (this.self.TargetEnemy.Y - y)))

  /// このコマの Env を、いまの位置から組む。中身は FrontEnv.at。
  /// **組む位置が変わると aim がずれる**ので、step の直前（差分を足す前）に組む
  member private this.EnvAt (x: float32) (y: float32) : Env =
    FrontEnv.at this.EnemyAimDirAt x y

  /// 撃たれた弾を実体にする。旧 GetNewBullet ＋ applySpawn の合わせ。
  ///
  /// 旧はエンジンが GetNewBullet を呼び返して、返ってきた実体へ位置・向き・
  /// 速さを書き込んでいた。いまは Frame.Spawned で値として受け取るので、
  /// フロントが自分の都合で実体を作って値を移すだけ
  member private this.Spawn (child: BulletRun) =
    let body = child.Body
    let newBullet = new BaseBullet() :> IBullet
    newBullet.Init ()
    newBullet.IsBullet <- true
    newBullet.Radius <- 4.5f
    newBullet.BulletType <- this.self.BulletType
    match newBullet.BulletType with
    | Player -> Manager.addPlayerBullet(newBullet)
    | Enemy -> Manager.addEnemyBullet(newBullet)
    // 弾幕は親と同じものを引き継ぐ。引き継がないと、弾の中に残った
    // bulletRef / actionRef を誰も解けない
    newBullet.SetScript (script, Some child)
    newBullet.X <- body.Pos.X
    newBullet.Y <- body.Pos.Y
    newBullet.Dir <- body.Dir
    newBullet.Speed <- body.Speed

  member this.RunTask(apply:Action<_,_>) =
    let apply = Action.toFSharpFunc2 apply
    match script, run with
    | Some sc, Some rn ->
        this.self.ShootingDirection <- sc.ShootingDirection
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）
        let body =
          { rn.Body with
              Pos = { X = this.self.X; Y = this.self.Y }
              Speed = this.self.Speed
              Dir = this.self.Dir
              Accel = { X = this.self.AccelerationX; Y = this.self.AccelerationY }
              Kind = this.self.BulletType
              IsBullet = this.self.IsBullet }
        // 台本が無い弾は aim を読まない（BulletRun.HasNoScript の但し書き）。
        // 旧 BulletRunner.envWithoutAim と同じ狙いで、段階 4 で Env を組む
        // 責任がフロントへ移ったぶん、判断もフロントに来た
        let env = if rn.HasNoScript then noAimEnv () else this.EnvAt this.self.X this.self.Y
        let f = Runner.stepWith sc env rn body
        let after = f.Run.Body
        this.self.Speed <- after.Speed
        this.self.Dir <- after.Dir
        this.self.AccelerationX <- after.Accel.X
        this.self.AccelerationY <- after.Accel.Y
        apply f.Delta.X f.Delta.Y
        finished <- f.Finished
        for child in f.Spawned do this.Spawn child
        if f.Vanished then this.self.Vanish ()
        if f.Retired then this.self.Used <- false
        // 走らせ直しの Env は、位置を更新したあとの自分から組む
        // （旧 BaseBullet が apply のあとで envOfGlobal を呼ぶのと同じ順）
        run <-
          if f.Finished then
            let renv =
              if f.Run.HasNoScript then noAimEnv ()
              else this.EnvAt this.self.X this.self.Y
            Some (Runner.restart renv f.Run)
          else Some f.Run
    | _ -> ()

    if (this.pos.X < 0.f || this.pos.X > Settings.Display.Width || this.pos.Y < 0.f || this.pos.Y > Settings.Display.Height) then
      this.self.Used <- false

  do
    this.self <- this :> IBullet
    this.self.Init()
