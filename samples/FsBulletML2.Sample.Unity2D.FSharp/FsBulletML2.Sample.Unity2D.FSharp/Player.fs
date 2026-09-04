namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Collections.Generic
open System.Runtime.Serialization
open UnityEngine
open FsBulletML2
open FsBulletML2.Unity2D 
 
type Player () =
  inherit MonoBehaviour ()
  [<DefaultValue>]val mutable public bulletObject : GameObject
  [<DefaultValue>]val mutable public bombType : GameObject
  [<DefaultValue>]val mutable public speed : float32
  [<DefaultValue>]val mutable public isBomb : bool
  [<DefaultValue>]val mutable public Damage : int

  [<DefaultValue>]val mutable private counter : int
  [<DefaultValue>]val mutable private b2wayLeftBulletTask : BulletmlScript option
  [<DefaultValue>]val mutable private b2wayRightBulletTask : BulletmlScript option
  [<DefaultValue>]val mutable private hommingTask : BulletmlScript option

  interface IPlayerPosition with
    member this.PlayerPosX () = this.transform.position.x
    member this.PlayerPosY () = this.transform.position.y

  member this.Awake () =
    // **Init が先。** 読む段の Env は BulletMLManager から rand と rank を
    // 引くので、口を差し込む前に読むと NullReference になる
    BulletMLManager.Init(new BulletFunctions(this))
    this.b2wayLeftBulletTask <- Runner.load (FrontEnv.Load()) FsBulletML2.Bullets.PlayerBullet.PlayerBullet.b2wayLeftBullet |> Some
    this.b2wayRightBulletTask <- Runner.load (FrontEnv.Load()) FsBulletML2.Bullets.PlayerBullet.PlayerBullet.b2wayRightBullet |> Some
    this.hommingTask <- Runner.load (FrontEnv.Load()) FsBulletML2.Bullets.PlayerBullet.PlayerBullet.homing |> Some

  member this.X with get () = this.transform.position.x 
                 and set (v) = this.transform.position <- Vector3(v, this.transform.position.y, this.transform.position.z) 
  member this.Y with get () = this.transform.position.y
                 and set (v) = this.transform.position <- Vector3(this.transform.position.x, v, this.transform.position.z) 

  member this.Update () = 

    let x = Input.GetAxisRaw("Horizontal")
    let y = Input.GetAxisRaw("Vertical")

    let mx = this.X + x / 100.f * this.speed
    if (mx >= 0.4f && mx <= 4.4f) then
        this.X <- mx

    let my = this.Y + y / 100.f * this.speed
    if (my > -6.0f && my <= -0.4f) then
        this.Y <- my

    this.counter <- this.counter + 1
    if (Input.GetKey(KeyCode.Z)) then
      this.Shoot2WayLeftBullet()
      this.Shoot2WayRightBullet()
      this.ShootHomingBullet()

    if (this.counter > 60) then
        this.counter <- 0

  /// 弾を 1 発 撃つ。**prefab ではなく Entity を作る。**
  /// 台本が無ければ何もしない（Awake が走る前に呼ばれた場合）
  member private this.Fire (position: Vector3) (script: BulletmlScript option) =
    match script with
    | Some s -> BulletEntityFactory.SpawnPlayer(position, s) |> ignore
    | None -> ()

  member this.Shoot2WayLeftBullet () =
    this.Fire (this.transform.position + new Vector3(-0.1f, 0.1f, 0.f)) this.b2wayLeftBulletTask

  member this.Shoot2WayRightBullet () =
    this.Fire (this.transform.position + new Vector3(0.1f, 0.1f, 0.f)) this.b2wayRightBulletTask

  member this.ShootHomingBullet () =
    if this.counter > 60 then
      this.Fire this.transform.position this.hommingTask

  /// 敵弾が当たった。**当たり判定は BulletEcsDriver がやる** ——
  /// ECS の弾は Collider2D を持たないので、OnTriggerEnter2D は届かない
  member this.HitByEnemyBullet () =
    if (this.isBomb) then Bomb.GenerateBomb(this.bombType, this.transform.position)
    this.Damage <- this.Damage + 1

  member this.OnTriggerEnter2D (collier:Collider2D) =
    // GameObject の弾（もう出ないが、prefab が残っている経路）向け
    this.HitByEnemyBullet()
