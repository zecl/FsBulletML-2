namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open R3
// R3 のあとに開くこと（`Observable` が両方にある。FrameTicker の但し書き）
open FSharp.Control.R3
open UnityEngine
open FsBulletML2
open FsBulletML2.Unity2D

type Enemy () =
  inherit BaseBullet ()

  /// ここが動くと弾幕が切り替わる。消す・読み直す・ライフを戻すは全部この購読。
  let bulletIndexRp = new ReactiveProperty<int>(0)
  /// ボスの残り。減ったら爆風、0 以下 で次の弾幕へ
  let lifeRp = new ReactiveProperty<int>(0)
  /// いまの弾幕の名前。Informations が読む
  let bulletNameRp = new ReactiveProperty<string>("")

  // Start で組み直すので直列化させない。再帰する木は深さ 10 で警告が出る。
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public bullets : BulletmlInfo list
  [<DefaultValue>]val mutable public bombType : GameObject
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public BulletmlInfo : BulletmlInfo
  /// 撃った弾幕の根。ひと回りしたかを見るのに持ち回る。
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public RootSim : BulletSim
  [<DefaultValue>]val mutable public MaxLife : int
  [<DefaultValue>]val mutable public isBomb : bool

  /// いまの弾幕の名前。Informations が読む
  member this.BulletName = bulletNameRp.Value
  member this.BulletNameRp = bulletNameRp
  /// ボスの残り。Informations が読む
  member this.Life
    with get () = lifeRp.Value
     and set (v) = lifeRp.Value <- v
  member this.LifeRp = lifeRp
  member this.BulletIndexRp = bulletIndexRp

  member this.Awake () =
    base.Awake ()
    Manager.addEnemy(this.GetDefaultBullet())

    let self = this.GetDefaultBullet ()
    self.Init()
    self.IsBullet <- false
    self.BulletType <- BulletType.Enemy

    // 0 のままだと尽きた購読が次の弾幕を呼び、また 0 を入れて止まらなくなる。
    if this.MaxLife <= 0 then this.MaxLife <- 2000

  /// 切り替えの合図は `bulletIndexRp` だけ。`Next` も `Prev` も番号を動かすだけ。
  member this.Start () =
    this.bullets <- FsBulletML2.Bullets.Dsl.All.bullets
    let update = FrameTicker.Frames
    let indexes = bulletIndexRp :> Observable<int>
    let lives = lifeRp :> Observable<int>

    // 1. 番号が動いたら弾幕を入れ替える（消す・読み直す・ライフを戻す）
    indexes
    |> subscribeUntilDestroy this (fun _ -> this.ApplyPattern ())

    // 2. 番号が動いたら名前も入れ替える
    indexes
    |> Observable.map (fun i -> this.bullets.[i].Name)
    |> subscribeUntilDestroy this (fun name -> bulletNameRp.Value <- name)

    // 撃つのは入れ替えの次のコマと、ひと回りしたコマ。同じコマだと順番で結果が変わる。
    let shootOnPattern =
      indexes
      |> Observable.map (fun _ -> update |> Observable.take 1)
      |> fun o -> o.Switch()

    let shootOnFinish = update |> Observable.filter (fun _ -> this.IsFinish ())

    // `merge` はタプル引数（`filter` や `take` と違ってパイプに乗らない）
    Observable.merge (shootOnPattern, shootOnFinish)
    |> subscribeUntilDestroy this (fun _ -> this.Shoot ())

    // 4. ライフが 1 減ったら爆風。戻したとき（MaxLife）には出さないので、
    //    値そのものではなく 1 つ 前との差を見る
    lives
    |> fun o -> o.Pairwise()
    |> Observable.filter (fun struct (previous, current) -> current = previous - 1)
    |> subscribeUntilDestroy this (fun _ ->
        if this.isBomb then Bomb.GenerateBomb(this.bombType, this.transform.position))

    // 5. 尽きたら次の弾幕へ
    lives
    |> Observable.filter (fun life -> life <= 0)
    |> subscribeUntilDestroy this (fun _ -> this.Next ())

    // 6. Enter で次の弾幕へ
    update
    |> Observable.filter (fun _ -> Input.GetKeyDown KeyCode.Return)
    |> subscribeUntilDestroy this (fun _ -> this.Next ())

    // 購読した瞬間に現在値が流れる。1 番 と 2 番 はここまでで走っている。
  member this.HitByPlayerBullet () =
    // 当たりは BulletEcsDriver。ライフを削るだけ。爆風と送りは Start の 4 番 と 5 番。
    lifeRp.Value <- lifeRp.Value - 1

  member this.OnTriggerEnter2D(collier:Collider2D) =
    // GameObject の弾（もう出ないが、prefab が残っている経路）向け
    this.HitByPlayerBullet()

  /// 弾は Entity。呼ばれても何も作らない。`bulletObject` は見本として Bootstrap が読む。
  override this.GetBulletPrefubInstance () = null

  /// ひと回りは `BulletSim.Finished` で見る。
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
      let script = FsBulletML2.Runner.load loadRand (loadRank ()) this.BulletmlInfo.Bulletml
      this.RootSim <- BulletEntityFactory.SpawnEnemy(this.transform.position, script, true)

  /// 次の弾幕へ。番号を動かすだけ —— 実際の入れ替えは購読（`Start` の 1 番）
  member this.Next () =
    let n = this.bullets.Length
    bulletIndexRp.Value <- (bulletIndexRp.Value + 1) % n

  /// 前の弾幕へ。番号を動かすだけ
  member this.Prev () =
    let n = this.bullets.Length
    bulletIndexRp.Value <- (bulletIndexRp.Value + n - 1) % n

  /// 呼ぶのは購読だけ。消す・読み直す・ライフを戻すを 1 か所 に揃える。
  member private this.ApplyPattern () =
    this.DestroyEnemyBullet()
    this.BulletmlInfo <- this.bullets.[bulletIndexRp.Value]
    lifeRp.Value <- this.MaxLife

  member this.DestroyEnemyBullet () =
    BulletEntityFactory.DestroyAllEnemy()
    this.RootSim <- Unchecked.defaultof<BulletSim>

  member this.OnDestroy () =
    bulletIndexRp.Dispose()
    bulletNameRp.Dispose()
    lifeRp.Dispose()
