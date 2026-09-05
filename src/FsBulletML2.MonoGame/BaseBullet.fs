namespace FsBulletML2.MonoGame

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front

type BaseBullet () as this =
  [<DefaultValue>]val mutable pos : Vector2
  [<DefaultValue>]val mutable private self : IBullet

  /// 走らせている弾幕と、その実行状態。旧の Task option を 2 つ に割ったもの
  let mutable script : BulletmlScript option = None
  let mutable run : BulletRun option = None
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish の置き場所
  let mutable finished = false

  /// この弾から見た世界。**弾 1 個 につき 1 個。**
  /// 狙う相手を覚えるのが弾ごとなので使い回せない
  let world = MonoGameWorld () :> IWorld

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
    member val Radius = 0.f with get, set

    member _.Script = script
    member _.Finished = finished

    /// 弾幕を割り当てる。`r` が None なら根から始める。
    ///
    /// **根の立場（狙う先と、撃たれた弾か）はここで 1 回 だけ決まる。**
    /// Core へは毎コマ渡らないので、BulletType と IsBullet はこれを呼ぶ前に
    /// 立てておくこと（同梱の弾はどれもコンストラクタで立てている）
    member this.SetScript (s, r) =
      script <- s
      finished <- false
      let self = this.self
      run <- match r with
             | Some _ -> r
             | None ->
                 s |> Option.map (fun sc ->
                   if self.IsBullet then Runner.newShot self.BulletType sc
                   else Runner.newRoot self.BulletType sc)

    member this.Vanish () = this.self.Used <- false

    member this.Init () =
      this.self.Used <- true

    member this.Update () =
      let apply x y = this.self.X <- this.self.X + x; this.self.Y <- this.self.Y + y
      this.RunTask(FSharpFunc.ToAction2 apply)


  /// 撃たれた弾を実体にする。旧 GetNewBullet ＋ applySpawn の合わせ。
  ///
  /// 旧はエンジンが GetNewBullet を呼び返して、返ってきた実体へ位置・向き・
  /// 速さを書き込んでいた。いまは Frame.Spawned で値として受け取るので、
  /// フロントが自分の都合で実体を作って値を移すだけ
  member private this.Spawn (child: BulletRun) =
    let motion = child.Motion
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
    newBullet.X <- motion.Pos.X
    newBullet.Y <- motion.Pos.Y
    newBullet.Dir <- motion.Dir
    newBullet.Speed <- motion.Speed

  member this.RunTask(apply:Action<_,_>) =
    let apply = Action.toFSharpFunc2 apply
    match script, run with
    | Some sc, Some rn ->
        this.self.ShootingDirection <- sc.ShootingDirection
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）
        let motion : Motion =
          { Pos = { X = this.self.X; Y = this.self.Y }
            Speed = this.self.Speed
            Dir = this.self.Dir
            Accel = { X = this.self.AccelerationX; Y = this.self.AccelerationY } }
        // Env を組む位置も、台本が無い弾の枝も Driver が持っている
        let f = Driver.step sc world MonoGameFront.space MonoGameFront.origin rn motion
        let after = f.Run.Motion
        this.self.Speed <- after.Speed
        this.self.Dir <- after.Dir
        this.self.AccelerationX <- after.Accel.X
        this.self.AccelerationY <- after.Accel.Y
        apply f.Delta.X f.Delta.Y
        finished <- f.Finished
        for child in f.Spawned do this.Spawn child
        if f.Vanished then this.self.Vanish ()
        if f.Retired then this.self.Used <- false
        // 走らせ直しは、位置を更新したあとの自分から組む
        // （旧 BaseBullet が apply のあとで envOfGlobal を呼ぶのと同じ順）。
        // **呼ぶ / 呼ばないはこのフロントの決めごと** —— Driver は既定を作らない
        run <-
          if f.Finished then
            Some (Driver.restart world MonoGameFront.space MonoGameFront.origin
                    f.Run this.self.X this.self.Y)
          else Some f.Run
    | _ -> ()

    if (this.pos.X < 0.f || this.pos.X > Settings.Display.Width || this.pos.Y < 0.f || this.pos.Y > Settings.Display.Height) then
      this.self.Used <- false

  do
    this.self <- this :> IBullet
    this.self.Init()
