namespace FsBulletML2.MonoGame

open System
open System.Collections.Generic 
open Microsoft.Xna.Framework
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2

type BaseBullet () as this =
  [<DefaultValue>]val mutable pos : Vector2
  [<DefaultValue>]val mutable private self : IBullet
  
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
    member val BulletRoot = false with get, set
    member val BulletType = BulletType.Enemy with get, set
    // Core の既定（BulletRunner.convertBulletmlTask）と揃えてある。
    // run が <bulletml type> を届けるので、ふつうはすぐ上書きされる。
    // この値が出るのは、まだ 1 度も run を通していない弾だけ
    member val ShootingDirection = ShootingDirection.BulletVertical with get, set
    member val Task = None with get, set
    member val TargetEnemy = defaultof<IBullet> with get, set
    member val Radius = 0.f with get, set
    member this.Vanish () = this.self.Used <- false
    member this.GetNewBullet() = 
      this.self.BulletRoot <- true

      let newBullet = new BaseBullet() :> IBullet
      newBullet.IsBullet <- true
      newBullet.Radius <- 4.5f
      newBullet.BulletType <- this.self.BulletType
      match newBullet.BulletType with
      | Player -> Manager.addPlayerBullet(newBullet)
      | Enemy -> Manager.addEnemyBullet(newBullet)
      newBullet :> IBulletmlObject

    member this.GetAimDir () : float32 =
      let dir = Math.Atan2( float (BulletMLManager.GetPlayerPosX() - this.self.X),float -(BulletMLManager.GetPlayerPosY() - this.self.Y))
      float32 dir

    /// GetNewBullet は new BaseBullet() を位置を入れずに返すので、産まれた弾は
    /// 原点に居る。GetAimDir と同じ式に、その弾の位置として (0, 0) を入れる
    member this.GetSpawnAimDir () : float32 =
      let dir = Math.Atan2( float (BulletMLManager.GetPlayerPosX() - 0.f), float -(BulletMLManager.GetPlayerPosY() - 0.f))
      float32 dir

    /// 同上。産まれたばかりの弾は TargetEnemy を持たない（new BaseBullet() の
    /// 既定が null）ので、旧はその場で Manager.enemies から原点に最も近い敵を
    /// 選んでいた。同じ選び方をここで行う。
    ///
    /// 旧はそのとき newBullet.TargetEnemy に選んだ相手を書き込んでいたが、
    /// ここには書き込む先の弾がまだ無いので、その代入だけは起きない。
    /// 次のコマでその弾が GetEnemyAimDir を通れば、同じ探し方でまた選ばれる
    member this.GetSpawnEnemyAimDir () : float32 =
      if ((Manager.enemies) :> seq<_>) |> Seq.length <= 0 then 0.f
      else
        let mutable md = Single.MaxValue
        let mutable target = defaultof<IBullet>
        for enemy in Manager.enemies do
          let d = Vector2.Distance (Vector2(0.f, 0.f), Vector2(enemy.X, enemy.Y))
          if md > d then
            target <- enemy
            md <- d
        float32 (Math.Atan2( float (target.X - 0.f), -1. * float (target.Y - 0.f)))

    member this.GetEnemyAimDir() : float32 = 
      let mutable md = Single.MaxValue 
      if this.self.TargetEnemy :> obj <> null then
        let dir = Math.Atan2( float (this.self.TargetEnemy.X - this.self.X), -1. * float (this.self.TargetEnemy.Y - this.self.Y))
        float32 dir
      else
        if  ((Manager.enemies) :> seq<_>) |> Seq.length <= 0 then
          0.f
        else
          for enemy in Manager.enemies do
            let d = Vector2.Distance (Vector2(this.self.X, this.self.Y), Vector2(enemy.X, enemy.Y))
            if md > d then
              this.self.TargetEnemy <- enemy
              md <- d
          let dir = Math.Atan2( float (this.self.TargetEnemy.X - this.self.X), -1. * float (this.self.TargetEnemy.Y - this.self.Y))
          float32 dir

    member this.Init () = 
      this.self.Used <- true
      this.self.BulletRoot <- false

    member this.Update () = 
      let apply x y = this.self.X <- this.self.X + x; this.self.Y <- this.self.Y + y
      this.RunTask(FSharpFunc.ToAction2 apply)

  member this.RunTask(apply:Action<_,_>) =
    let apply = Action.toFSharpFunc2 apply
    match this.self.Task with
    | None -> ()
    | Some task -> 
      let result = BulletRunner.run this
      if result.Processed then
        apply result.X result.Y
        // 位置を更新したあとの this.self から組む（envOfGlobal は位置を読むので、
        // apply より前の値を使い回すと aim がずれる）
        this.self.Task |> Option.iter (fun x -> x.Init(BulletRunner.envOfGlobal this.self))
      else apply result.X result.Y

    if (this.pos.X < 0.f || this.pos.X > Settings.Display.Width || this.pos.Y < 0.f || this.pos.Y > Settings.Display.Height) then
      this.self.Used <- false

  do
    this.self <- this :> IBullet
    this.self.Init()