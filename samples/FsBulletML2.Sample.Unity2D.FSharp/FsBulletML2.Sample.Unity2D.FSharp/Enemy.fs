namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open System.Collections.Generic
open R3
// **R3 のあとに開くこと**（`Observable` が両方にある。FrameTicker の但し書き）
open FSharp.Control.R3
open UnityEngine
open FsBulletML2
open FsBulletML2.Bullets
open FsBulletML2.Unity2D

type Enemy () =
  inherit BaseBullet ()

  /// いま何番目 の弾幕か。**ここが動くと弾幕が切り替わる** ——
  /// 消す・読み直す・ライフを戻す、は全部 これの購読（`Start`）。
  /// **F# は let 束縛を val より前に置く**ので、ここに居る
  let bulletIndexRp = new ReactiveProperty<int>(0)
  /// ボスの残り。減ったら爆風、0 以下 で次の弾幕へ
  let lifeRp = new ReactiveProperty<int>(0)
  /// いまの弾幕の名前。**Informations が読む**
  let bulletNameRp = new ReactiveProperty<string>("")

  // Start で組み直すので Unity に直列化させない。BulletmlInfo は
  // 弾幕の木を持っており、action が action を含む再帰なので、
  // Unity の直列化は深さ 10 で打ち切って警告を出す
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public bullets : BulletmlInfo list
  [<DefaultValue>]val mutable public bombType : GameObject
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public BulletmlInfo : BulletmlInfo
  /// 撃った弾幕の根。**GameObject ではなく Entity になった。**
  /// ひと回りしたかを見るのに持ち回る
  [<System.NonSerialized>]
  [<DefaultValue>]val mutable public RootSim : BulletSim
  [<DefaultValue>]val mutable public MaxLife : int
  [<DefaultValue>]val mutable public isBomb : bool

  /// いまの弾幕の名前。**Informations が読む**
  member this.BulletName = bulletNameRp.Value
  member this.BulletNameRp = bulletNameRp
  /// ボスの残り。**Informations が読む**
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

    // **0 なら既定に落とす。** ライフは `ApplyPattern` が `MaxLife` から
    // 入れ直すので、0 のままだと「尽きた」の購読（`Start` の 5 番）が
    // すぐ次の弾幕を呼び、それがまた 0 を入れる —— 止まらなくなる。
    // 旧は当たったときにしか見ていなかったので、この形は出なかった。
    // 同梱の C# サンプルは欄の既定値が 2000 で、そちらに合わせてある
    if this.MaxLife <= 0 then this.MaxLife <- 2000

  /// 弾幕の切り替えと発射を 5 本 の流れに割る。
  ///
  /// **旧は Update の中に畳んであった** —— 「初回か、ひと回りしたら撃つ」を
  /// `Second` という bool で持ち、切り替えは `Next` / `Prev` が手で
  /// 「消す・読み直す・ライフを戻す」を並べていた。
  /// **切り替えの経路が 2 本 あって、片方だけ直す形になっていた。**
  ///
  /// いまは `bulletIndexRp` が動いたことが唯一の合図で、
  /// `Next` も `Prev` も番号を動かすだけ。
  member this.Start () =
    this.bullets <- this.GetBulletml() |> Seq.toList
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

    // 3. 撃つ合図は 2 つ。**入れ替えた次のコマ**と、**ひと回りしたコマ**。
    //
    //    入れ替えを 1 コマ 遅らせるのは、`ApplyPattern` が同じコマで
    //    弾を消しているため（消した直後に撃つと、消す側と撃つ側の
    //    順番で結果が変わる）。`take 1` した update を `Switch` で
    //    差し替えるので、**入れ替えが続けて起きても撃つのは 1 回**
    let shootOnPattern =
      indexes
      |> Observable.map (fun _ -> update |> Observable.take 1)
      |> fun o -> o.Switch()

    let shootOnFinish = update |> Observable.filter (fun _ -> this.IsFinish ())

    // **`merge` はタプル引数**（`filter` や `take` と違ってパイプに乗らない）
    Observable.merge (shootOnPattern, shootOnFinish)
    |> subscribeUntilDestroy this (fun _ -> this.Shoot ())

    // 4. ライフが 1 減ったら爆風。**戻したとき（MaxLife）には出さない**ので、
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

    // **最初の 1 回 を自分で呼ぶ必要は無い。** ReactiveProperty は
    // 購読した瞬間に現在値を流すので、1 番 と 2 番 はここまでで走っている
    // （`bullets` はこのメソッドの先頭で入れてある）

  /// 自機弾が当たった。**当たり判定は BulletEcsDriver がやる** ——
  /// ECS の弾は Collider2D を持たないので、OnTriggerEnter2D は届かない。
  ///
  /// **爆風も次の弾幕への送りもここには無い。** ライフを削るだけで、
  /// 出すのも送るのも `Start` の 4 番 と 5 番
  member this.HitByPlayerBullet () =
    lifeRp.Value <- lifeRp.Value - 1

  member this.OnTriggerEnter2D(collier:Collider2D) =
    // GameObject の弾（もう出ないが、prefab が残っている経路）向け
    this.HitByPlayerBullet()

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

  /// 次の弾幕へ。**番号を動かすだけ** —— 実際の入れ替えは購読（`Start` の 1 番）
  member this.Next () =
    let n = this.bullets.Length
    bulletIndexRp.Value <- (bulletIndexRp.Value + 1) % n

  /// 前の弾幕へ。**番号を動かすだけ**
  member this.Prev () =
    let n = this.bullets.Length
    bulletIndexRp.Value <- (bulletIndexRp.Value + n - 1) % n

  /// いまの番号の弾幕に入れ替える。**呼ぶのは購読だけ。**
  /// 消す・読み直す・ライフを戻す、が 1 か所 に揃っている
  member private this.ApplyPattern () =
    this.DestroyEnemyBullet()
    this.BulletmlInfo <- this.bullets.[bulletIndexRp.Value]
    lifeRp.Value <- this.MaxLife

  member this.DestroyEnemyBullet () =
    BulletEntityFactory.DestroyAllEnemy()
    this.RootSim <- Unchecked.defaultof<BulletSim>

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

  member this.OnDestroy () =
    bulletIndexRp.Dispose()
    bulletNameRp.Dispose()
    lifeRp.Dispose()
