namespace FsBulletML2.MonoGame.Tests

open System
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.MonoGame

/// このフロントが `Env` を組むところの門。
///
/// **同梱フロントに門が 1 つ も無かった。** 旧 API を廃止すると
/// フロント 2 つ が唯一の公開 API 利用者になるので、ここが空なのは穴。
/// `TraceApi.fs`（Core 側）自身が「公開が足りているかの本当の門は
/// フロントを移したとき」と書いている。
///
/// **Core.Tests へは置けない。** あちらは `InternalsVisibleTo` に入っていて
/// internal が見えるので、フロントの門をあちらに置くと「公開だけで書けるか」
/// の検査が効かなくなる。参照も張っていない。
///
/// ## 何を守るのか
///
/// `Env` の 6 欄 は**どれも float32 か関数**なので、入れる場所を取り違えても
/// 型が止めない。**`AimDir` に `SpawnAimDir` を入れても通る。**
/// 元は Core.Tests の `EnvTests` 2 本 が旧 `BulletRunner.envOfGlobal` に
/// 対して見ていたが、あれは**このフロントの規約**を測っていた
/// （Unity2D は Spawn 側と `AimDir` が同値なのが正しい。座標系も Y が逆）。
/// 式の在る場所へ門を移した。

/// 位置だけを持つ IBullet。**Manager.enemies へ置くためだけのもの。**
/// オブジェクト式には member val を書けないので型にしてある
type private StubBullet(x: float32, y: float32) =
  let mutable pos = Microsoft.Xna.Framework.Vector2(x, y)
  interface IBullet with
    member _.Update() = ()
    member val TargetEnemy = Unchecked.defaultof<IBullet> with get, set
    member _.Pos with get () = pos and set v = pos <- v
    member _.X with get () = pos.X and set v = pos <- Microsoft.Xna.Framework.Vector2(v, pos.Y)
    member _.Y with get () = pos.Y and set v = pos <- Microsoft.Xna.Framework.Vector2(pos.X, v)
    member val Radius = 1.0f with get, set
    member val Speed = 0.0f with get, set
    member val Dir = 0.0f with get, set
    member val AccelerationX = 0.0f with get, set
    member val AccelerationY = 0.0f with get, set
    member val Used = true with get, set
    member val IsBullet = false with get, set
    member val BulletType = BulletType.Enemy with get, set
    member val ShootingDirection = ShootingDirection.BulletVertical with get, set
    member _.Init() = ()
    member _.Vanish() = ()
    member _.SetScript(_, _) = ()
    member _.Script = None
    member _.Finished = false

/// 下が本体の門。上の doc（このフロントが Env を組むところ）を参照
[<TestFixture>]
[<NonParallelizable>]
type EnvGate() =

  /// 自機 (30, 100)、rand 0.5、rank 0.25
  let fixedManager () =
    { new IBulletMLManager with
        member _.GetRandom() = 0.5f
        member _.GetRank() = 0.25f
        member _.GetPlayerPosX() = 30.0f
        member _.GetPlayerPosY() = 100.0f }

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(fixedManager ())
    // 敵の一覧はグローバル。前の試験の残りを持ち越さない
    Manager.removeAll ()

  [<Test>]
  member _.``自機を狙う向きは、弾の位置から引く。Y は反転する``() =
    // 弾 (10, 20) から自機 (30, 100) へ: (30-10, -(100-20)) = (20, -80)
    FrontEnv.aimAtPlayer 10.0f 20.0f |> should equal { X = 20.0f; Y = -80.0f }

  /// **Unity2D は反転しない。** 同じ式に見えて座標系が逆なので、
  /// 片方をもう片方へ寄せると全弾幕の軌跡が割れる。ここで符号を固定する
  [<Test>]
  member _.``Y を反転しない式とは別の値になる``() =
    FrontEnv.aimAtPlayer 10.0f 20.0f |> should not' (equal { X = 20.0f; Y = 80.0f })

  /// **これが (b) の本体。** 産まれる弾は原点に作るので、Spawn 側は
  /// 撃った側と別の値になる。同じ値だと、入れ替えても誰も気づかない
  [<Test>]
  member _.``Spawn 側の aim は原点から。撃った側とは別の値``() =
    FrontEnv.spawnAimAtPlayer () |> should equal { X = 30.0f; Y = -100.0f }
    FrontEnv.spawnAimAtPlayer ()
    |> should not' (equal (FrontEnv.aimAtPlayer 10.0f 20.0f))

  /// 組み立ての 4 欄 が入れ替わっていないか。**enemyAimAt は差し込めるので、
  /// 弾の位置がそのまま出る stub を渡して、どの欄へ入ったかを見る**
  [<Test>]
  member _.``Env の 4 本 の aim は、それぞれ別のところから来る``() =
    // 敵を 1 体 置く。置かないと enemy 側 2 本 が どちらも 0 になり、
    // 入れ替えても気づけない（下の対照がその条件を固定している）
    Manager.addEnemy (StubBullet(-40.0f, -60.0f))

    // 弾の位置を「そのまま」返す stub。EnemyAimVec の欄にだけ出るはず
    let enemyAimStub (x: float32) (_y: float32) : Vec2 = { X = x * 1000.0f; Y = 1.0f }
    let env = FrontEnv.at enemyAimStub 10.0f 20.0f

    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f
    // 撃った側の位置から
    env.AimDir |> should (equalWithin 0.0001) (float32 (Math.Atan2(20.0, -80.0)))
    // 差し込んだ stub から。**AimVec と入れ替わっていたらここで割れる**
    env.EnemyAimVec |> should equal { X = 10000.0f; Y = 1.0f }
    // 原点から
    env.SpawnAimDir |> should (equalWithin 0.0001) (float32 (Math.Atan2(30.0, -100.0)))
    // 原点から、いちばん近い敵 (-40, -60) へ: Atan2(-40, 60)
    env.SpawnEnemyAimDir |> should (equalWithin 0.0001) (float32 (Math.Atan2(-40.0, 60.0)))

  /// aim を読まないコマの Env。**3 か所 に同じ形が居るので、値が揃うことを見る**
  /// （`FrontEnv.noAim` / `BulletmlLoad.loadEnv` / `BulletmlLoad.noAimEnv`）
  [<Test>]
  member _.``aim を読まない Env は 3 か所 とも同じ``() =
    let a = FrontEnv.noAim ()
    let b = BulletmlLoad.loadEnv ()
    let c = BulletmlLoad.noAimEnv ()
    for e in [ a; b; c ] do
      e.Rank |> should equal 0.25f
      e.Rand () |> should equal 0.5f
      e.AimDir |> should equal 0.0f
      e.EnemyAimDir |> should equal 0.0f
      e.SpawnAimDir |> should equal 0.0f
      e.SpawnEnemyAimDir |> should equal 0.0f

  /// 対照 —— **敵が居ないときは enemy 側が 0。**
  /// 上の門は敵を置いてから測っている。置き忘れると 0 どうしの一致になり、
  /// 入れ替えを当てられなくなるので、0 になる条件を字で固定しておく
  [<Test>]
  member _.``敵が 1 体 も居なければ、敵向きの aim は 0``() =
    FrontEnv.spawnAimAtEnemy () |> should equal { X = 0.0f; Y = 0.0f }
