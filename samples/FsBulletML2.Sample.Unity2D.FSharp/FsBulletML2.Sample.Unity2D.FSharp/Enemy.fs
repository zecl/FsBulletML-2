namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Collections.Generic
open System.Runtime.Serialization
open UnityEngine
open FsBulletML2
open FsBulletML2.Bullets 
open FsBulletML2.Unity2D 
 
type Enemy () =
  inherit BaseBullet ()
  // Start で組み直すので Unity に直列化させない。BulletmlInfo は
  // 弾幕の木を持っており、action が action を含む再帰なので、
  // Unity の直列化は深さ 10 で打ち切って警告を出す
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public bullets : BulletmlInfo list
  [<DefaultValue>]val mutable public bombType : GameObject
  [<DefaultValue>]val mutable public BulletName : string
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public BulletmlInfo : BulletmlInfo
  /// 撃った弾幕の根。**GameObject ではなく Entity になった。**
  /// ひと回りしたかを見るのに持ち回る
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public RootSim : BulletSim
  [<DefaultValue>]val mutable public Life : int
  [<DefaultValue>]val mutable public MaxLife : int
  [<DefaultValue>]val mutable public isBomb : bool
  [<DefaultValue>]val mutable private BulletIndex : int
  [<DefaultValue>]val mutable private Second : bool

  member this.Awake () =
    base.Awake ()
    Manager.addEnemy(this.GetDefaultBullet())

    let self = this.GetDefaultBullet ()
    self.Init()
    self.IsBullet <- false
    self.BulletType <- BulletType.Enemy 
    this.BulletName <- ""

  member this.Start () = 
    this.bullets <- this.GetBulletml() |> Seq.toList  
    this.SetBulletmlInfo()

  /// 自機弾が当たった。**当たり判定は BulletEcsDriver がやる** ——
  /// ECS の弾は Collider2D を持たないので、OnTriggerEnter2D は届かない
  member this.HitByPlayerBullet () =
    if (this.isBomb) then Bomb.GenerateBomb(this.bombType, this.transform.position)
    this.Life <- this.Life - 1
    if (this.Life <= 0) then
      this.Next()

  member this.OnTriggerEnter2D(collier:Collider2D) =
    // GameObject の弾（もう出ないが、prefab が残っている経路）向け
    this.HitByPlayerBullet()

  override this.Update () = 
    if (Input.GetKeyDown(KeyCode.Return)) then
        this.Next()

    if (not this.Second || this.IsFinish()) then
        this.Second <- true
        this.Shoot()

    base.Update()

  /// **もう prefab を実体化しない。** 弾は Entity になった。
  /// GameObject 側の口は残してあるが、呼ばれても何も作らない
  /// （`bulletObject` は弾の見た目の見本として Bootstrap が読む）
  override this.GetBulletPrefubInstance () = null

  /// 撃った弾幕がひと回りしたか。旧は GameObject の DefaultBullet.Finished を
  /// 見ていた。ECS では Entity が控えている（`BulletSim.Finished`）
  member this.IsFinish () =
    if isNull (box this.RootSim) then false
    elif this.RootSim.Finished then
      BulletEntityFactory.DestroySim this.RootSim
      this.RootSim <- Unchecked.defaultof<BulletSim>
      true
    else false

  member this.Shoot () =
    let self = this.GetDefaultBullet ()
    if (self.Used) then
      // 弾幕は撃つたびに読み直す。読む段の Env は aim を読まない
      // （撃つ弾ごとの位置がまだ無い）
      let script = FsBulletML2.Runner.load (FrontEnv.Load()) this.BulletmlInfo.Bulletml
      this.RootSim <- BulletEntityFactory.SpawnEnemy(this.transform.position, script, true)

  member this.Next () = 
    this.DestroyEnemyBullet()
    if (this.BulletIndex + 1 >= this.bullets.Length) then
      this.BulletIndex <- 0
    else
      this.BulletIndex <- this.BulletIndex + 1

    this.Life <- this.MaxLife
    this.Second <- false
    this.SetBulletmlInfo();

  member this.Prev () = 
    this.DestroyEnemyBullet()
    if (this.BulletIndex = 0) then
      this.BulletIndex <- this.bullets.Length - 1
    else
      this.BulletIndex <- this.BulletIndex - 1

    this.Life <- this.MaxLife
    this.Second <- false
    this.SetBulletmlInfo();


  member this.DestroyEnemyBullet () =
    BulletEntityFactory.DestroyAllEnemy()
    this.RootSim <- Unchecked.defaultof<BulletSim>

  member this.SetBulletmlInfo () =
    let bulletmlInfo = this.bullets.[this.BulletIndex]
    this.BulletName <- bulletmlInfo.Name
    this.BulletmlInfo <- bulletmlInfo

  member this.GetBulletml() : seq<BulletmlInfo> =
    seq {
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.Strikers1999.hanabi
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.DragonBlaze.nebyurosu_2
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.GWange._roll_gara
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.Original.knight_2
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.GWange.round_trip_bit
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.b88way
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.bit
        yield FsBulletML2.Bullets.EnemyBullet.Sdmkun.Noiz2sa.rollbar
    }

