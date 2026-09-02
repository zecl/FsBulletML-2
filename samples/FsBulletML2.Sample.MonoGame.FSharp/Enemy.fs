namespace FsBulletML2.Sample.MonoGame.FSharp

open System
open System.Collections.Generic
open System.Runtime.Serialization
open Microsoft.Xna.Framework
open FsBulletML2
open FsBulletML2.MonoGame
 
type Enemy (life) as this =
  inherit BaseBullet ()
  [<DefaultValue>]val mutable private self : IEnemy
  [<DefaultValue>]val mutable private timer : int
  [<DefaultValue>]val mutable private bulletName : string
  [<DefaultValue>]val mutable private bulletBulletmlInfo : BulletmlInfo
  [<DefaultValue>]val mutable private second : bool
  [<DefaultValue>]val mutable private bullet : EnemyBullet
  [<DefaultValue>]val mutable private life : int32
    
  new() = Enemy(2000)
  do 
    this.self <- this :> IEnemy
    this.self.BulletType <- BulletType.Enemy 
    this.self.Init()
    this.self.IsBullet <- false
    this.self.Radius <- 18.f
    this.life <- life

  member this.Timer with get () = this.timer     

  interface IEnemy with
    member this.Life with get () = this.life
                      and set (v) = this.life <- v      

    member this.Shoot () =
      let self = this :> IEnemy
      if self.Used then
        this.bullet <- new EnemyBullet()
        (this.bullet :> IBullet).IsBullet <- true
        Manager.addEnemyBulletPos(this.bullet, Vector2(self.X, self.Y))
        this.bullet.SetScript(Some (this.bulletBulletmlInfo.Script (loadEnv ())))

    member this.Update () = 
      this.timer <- this.timer + 1       

      let finish =
        if this.bullet :> obj = null then false
        else (this.bullet :> IBullet).Finished
      if not this.second || finish then
        this.second <- true
        this.timer <- 0
        let self = this :> IEnemy
        self.Shoot()
    
      let apply x y = this.self.X <- this.self.X + x; this.self.Y <- this.self.Y + y
      base.RunTask(System.Action<_,_>(apply))

  member this.SetMoveBulletmlInfo(bulletmlInfo:BulletmlInfo) =
    (this :> IEnemy).SetScript(Some (bulletmlInfo.Script (loadEnv ())), None)

  member this.SetBulletTask(bulletName, bulletmlInfo) = 
    this.bulletName <- bulletName
    this.bulletBulletmlInfo <- bulletmlInfo

module EnemyControl =
  let bullets = [ FsBulletML2.Bullets.EnemyBullet.Sdmkun.Guwange.round_2_boss_circle_fire
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Psyvariar.b4_D_boss_MZIQ
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Strikers1999.hanabi
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.b10flower_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.dis_bee_1
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.dis_bee_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.dis_bee_3
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.roll_misago
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.slow_move
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.OtakuTwo.circle_fireworks2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.DragonBlaze.nebyurosu_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.GWange._roll_gara
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.knight_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.GWange.round_trip_bit
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.b88way
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.bit
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.rollbar
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Tenmado.b5_boss_1
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Tenmado.b5_boss_3
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Dodonpachi.hibachi
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Dodonpachi.kitiku_1 
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Dodonpachi.kitiku_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.hibachi_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.hibachi_3
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.EspRade.round_5_boss_gara_4
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.EspRade.round_5_boss_gara_3
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.EspRade.round_5_boss_gara_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.EspRade.round_5_boss_gara_1_a
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.EspRade.round_123_boss_izuna_hakkyou
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_1_boss
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_3_boss
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_3_boss_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_3_boss_last
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_4_boss
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.StormCalibar.last_boss_double_roll_bullets
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.acc_n_dec
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.circular_model
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.wind_cl
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.gnnnyari
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.mossari
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.MAD.double_w
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.star_in_the_sky
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.guruguru
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.backfire 
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.yokokasoku
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.entangled_space
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.ellipse_bomb
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.gyakuhunsya
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.fujin_ranbu_true
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Bulletsmorph.double_seduction
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.accusation
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.XiiStag.b3b
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.KetuiLt.b2boss_winder_crash
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.ChaosSeed.big_monkey_boss
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Guwange.round_4_boss_eye_ball
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_4_boss_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_4_boss_4
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_4_boss_5
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_5_boss_1
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_5_boss_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_6_boss_1
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_6_boss_2
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_6_boss_3
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_6_boss_4
                  FsBulletML2.Bullets.EnemyBullet.Sdmkun.Daiouzyou.round_6_boss_5
                ]