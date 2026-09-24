namespace FsBulletML2.Sample.MonoGame.FSharp

open System.Collections.Generic
open Microsoft.Xna.Framework
open FsBulletML2
open FsBulletML2.MonoGame

type Enemy(life) as this =
    inherit BaseBullet()

    [<DefaultValue>]
    val mutable private self: IEnemy

    [<DefaultValue>]
    val mutable private timer: int

    [<DefaultValue>]
    val mutable private bulletName: string

    [<DefaultValue>]
    val mutable private bulletBulletmlInfo: BulletmlInfo

    [<DefaultValue>]
    val mutable private second: bool

    [<DefaultValue>]
    val mutable private bullet: EnemyBullet

    [<DefaultValue>]
    val mutable private life: int32

    new() = Enemy(2000)

    do
        this.self <- this :> IEnemy
        this.self.BulletType <- BulletType.Enemy
        this.self.Init()
        this.self.IsBullet <- false
        this.self.Radius <- 18.f
        this.life <- life

    member this.Timer = this.timer

    interface IEnemy with
        member this.Life
            with get () = this.life
            and set (v) = this.life <- v

        member this.Shoot() =
            let self = this :> IEnemy

            if self.Used then
                this.bullet <- new EnemyBullet()
                (this.bullet :> IBullet).IsBullet <- true
                Manager.addEnemyBulletPos (this.bullet, Vector2(self.X, self.Y))
                this.bullet.SetScript(Some(this.bulletBulletmlInfo.Script(loadRand, loadRank ())))

        member this.Update() =
            this.timer <- this.timer + 1

            let finish =
                if this.bullet :> obj = null then
                    false
                else
                    (this.bullet :> IBullet).Finished

            if not this.second || finish then
                this.second <- true
                this.timer <- 0
                let self = this :> IEnemy
                self.Shoot()

            let apply x y =
                this.self.X <- this.self.X + x
                this.self.Y <- this.self.Y + y

            base.RunTask(System.Action<_, _>(apply))

    member this.SetMoveBulletmlInfo(bulletmlInfo: BulletmlInfo) =
        (this :> IEnemy).SetScript(Some(bulletmlInfo.Script(loadRand, loadRank ())))

    member this.SetBulletTask(bulletName, bulletmlInfo) =
        this.bulletName <- bulletName
        this.bulletBulletmlInfo <- bulletmlInfo

module EnemyControl =
    let bullets = FsBulletML2.Bullets.Dsl.All.bullets
