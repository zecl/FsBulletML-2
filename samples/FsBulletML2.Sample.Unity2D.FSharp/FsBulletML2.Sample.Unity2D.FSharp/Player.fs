namespace FsBulletML2.Sample.Unity2D.FSharp

open R3
// R3 のあとに開くこと（`Observable` が両方にある。FrameTicker の但し書き）
open FSharp.Control.R3
open UnityEngine
open FsBulletML2
open FsBulletML2.Unity2D

type Player () =
  inherit MonoBehaviour ()

  /// 受けたダメージ。増えたら爆風（購読は `Start`）。
  /// F# は let 束縛を val より前に置くので、ここに居る
  let damageRp = new ReactiveProperty<int>(0)

  [<DefaultValue>]val mutable public bulletObject : GameObject
  [<DefaultValue>]val mutable public bombType : GameObject
  [<DefaultValue>]val mutable public speed : float32
  [<DefaultValue>]val mutable public isBomb : bool

  [<DefaultValue>]val mutable private b2wayLeftBulletTask : BulletmlScript option
  [<DefaultValue>]val mutable private b2wayRightBulletTask : BulletmlScript option
  [<DefaultValue>]val mutable private hommingTask : BulletmlScript option

  interface IPlayerPosition with
    member this.PlayerPosX () = this.transform.position.x
    member this.PlayerPosY () = this.transform.position.y

  /// 受けたダメージ。Informations が読む
  member this.Damage = damageRp.Value
  member this.DamageRp = damageRp

  member this.Awake () =
    // Init が先。 読む段の Env は BulletMLManager から rand と rank を
    // 引くので、口を差し込む前に読むと NullReference になる
    BulletMLManager.Init(new BulletFunctions(this))
    this.b2wayLeftBulletTask <- Runner.load loadRand (loadRank ()) FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet |> Some
    this.b2wayRightBulletTask <- Runner.load loadRand (loadRank ()) FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayRightBullet |> Some
    this.hommingTask <- Runner.load loadRand (loadRank ()) FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.homing |> Some

  member this.X with get () = this.transform.position.x
                 and set (v) = this.transform.position <- Vector3(v, this.transform.position.y, this.transform.position.z)
  member this.Y with get () = this.transform.position.y
                 and set (v) = this.transform.position <- Vector3(this.transform.position.x, v, this.transform.position.z)

  /// 毎コマ の仕事を 4 本 の流れに割る。
  member this.Start () =
    let update = FrameTicker.Frames

    // 1. 移動。入力が入っているコマだけ通す
    update
    |> Observable.map (fun _ -> struct (Input.GetAxisRaw "Horizontal", Input.GetAxisRaw "Vertical"))
    |> Observable.filter (fun struct (x, y) -> x <> 0.0f || y <> 0.0f)
    |> subscribeUntilDestroy this (fun struct (x, y) -> this.ApplyMove x y)

    // 2. Z を押している間ずっと 2way
    update
    |> Observable.filter (fun _ -> Input.GetKey KeyCode.Z)
    |> subscribeUntilDestroy this (fun _ ->
        this.Shoot2WayLeftBullet ()
        this.Shoot2WayRightBullet ())

    // 3. ホーミングは 61 コマ に 1 回 だけ。
    update
    |> Observable.mapi (fun i _ -> i)
    |> Observable.filter (fun i -> i % 61 = 60 && Input.GetKey KeyCode.Z)
    |> subscribeUntilDestroy this (fun _ -> this.ShootHomingBullet ())

    // 4. ダメージが増えたら爆風。`skip 1` は初期値の 0 を捨てるため
    //    （ReactiveProperty は購読した瞬間に現在値を 1 個 流す）
    (damageRp :> Observable<int>)
    |> Observable.skip 1
    |> subscribeUntilDestroy this (fun _ ->
        if this.isBomb then Bomb.GenerateBomb(this.bombType, this.transform.position))

  member private this.ApplyMove (x: float32) (y: float32) =
    let mx = this.X + x / 100.f * this.speed
    if (mx >= 0.4f && mx <= 4.4f) then
        this.X <- mx

    let my = this.Y + y / 100.f * this.speed
    if (my > -6.0f && my <= -0.4f) then
        this.Y <- my

  /// 弾を 1 発 撃つ。prefab ではなく Entity を作る。
  /// 台本が無ければ何もしない（Awake が走る前に呼ばれた場合）
  member private this.Fire (position: Vector3) (script: BulletmlScript option) =
    match script with
    | Some s -> BulletEntityFactory.SpawnPlayer(position, s) |> ignore
    | None -> ()

  member this.Shoot2WayLeftBullet () =
    this.Fire (this.transform.position + new Vector3(-0.1f, 0.1f, 0.f)) this.b2wayLeftBulletTask

  member this.Shoot2WayRightBullet () =
    this.Fire (this.transform.position + new Vector3(0.1f, 0.1f, 0.f)) this.b2wayRightBulletTask

  /// 間隔の判定はここに無い。 持っているのは流れの側（`Start` の 3 番）
  member this.ShootHomingBullet () =
    this.Fire this.transform.position this.hommingTask

  /// 敵弾が当たった。
  /// 当たり判定は BulletEcsDriver がやる —— ECS の弾は Collider2D を持たないので、OnTriggerEnter2D は届かない。
  member this.HitByEnemyBullet () =
    damageRp.Value <- damageRp.Value + 1

  member this.OnTriggerEnter2D (collier:Collider2D) =
    // GameObject の弾（もう出ないが、prefab が残っている経路）向け
    this.HitByEnemyBullet()

  member this.OnDestroy () = damageRp.Dispose()
