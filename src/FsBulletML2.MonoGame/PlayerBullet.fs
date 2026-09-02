namespace FsBulletML2.MonoGame

open System
open System.Collections.Generic
open System.Runtime.Serialization
open Microsoft.Xna.Framework
open FsBulletML2
 
type PlayerBullet () as this =
  inherit BaseBullet ()

  do 
    let self = this :> IBullet
    self.Init()
    self.IsBullet <- true
    self.BulletType <- BulletType.Player 

  /// 弾幕を割り当てる。根から始めるので実行状態は Core に作らせる
  member this.SetScript(script) =
    (this :> IBullet).SetScript(script, None)
