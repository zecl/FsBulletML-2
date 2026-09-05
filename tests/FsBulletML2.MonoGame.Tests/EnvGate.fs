namespace FsBulletML2.MonoGame.Tests

open System
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Front
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
/// `Env` の欄は**どれも float32 か関数**なので、入れる場所を取り違えても
/// 型が止めなかった。**`AimDir` に `SpawnAimDir` を入れても通っていた。**
/// いまは `Aim` と `SpawnAim` が別の型なので、その軸の取り違えは
/// コンパイルで落ちる（C# の `new Env(...)` で 3 番目 と 4 番目 を
/// 入れ替えて CS1503 になることを確かめてある）。
///
/// **残っているのは `ToPlayer` と `ToEnemy` の軸**と、
/// **このフロントが `Space` と `SpawnOrigin` に何を選んでいるか。**
/// 式そのものは `FsBulletML2.Front` に移ったが、どちらを選ぶかはここの決めごとで、
/// 取り違えても型は通る。**この門が守るのはそこ。**

/// 位置だけを持つ IBullet。**Manager.enemies へ置くためだけのもの。**
/// オブジェクト式には member val を書けないので型にしてある
type private StubBullet(x: float32, y: float32) =
  let mutable pos = Microsoft.Xna.Framework.Vector2(x, y)
  interface IBullet with
    member _.Update() = ()
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

  let world () = MonoGameWorld () :> IWorld

  let envAt (x: float32) (y: float32) =
    FrontEnv.at (world ()) MonoGameFront.space MonoGameFront.origin x y

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(fixedManager ())
    // 敵の一覧はグローバル。前の試験の残りを持ち越さない
    Manager.removeAll ()

  [<Test>]
  member _.``自機を狙う向きは、弾の位置から引く。Y は反転する``() =
    // 弾 (10, 20) から自機 (30, 100) へ: Atan2(30-10, -(100-20)) = Atan2(20, -80)
    let expected = float32 (Math.Atan2(20.0, -80.0))
    (envAt 10.0f 20.0f).Aim.ToPlayer |> should (equalWithin 0.0001) expected

  /// **Unity2D は反転しない。** 同じ式に見えて座標系が逆なので、
  /// `Space` を取り違えると全弾幕の軌跡が割れる。ここで符号を固定する
  [<Test>]
  member _.``Y を反転しない式とは別の値になる``() =
    let notFlipped = float32 (Math.Atan2(20.0, 80.0))
    (envAt 10.0f 20.0f).Aim.ToPlayer |> should not' (equalWithin 0.0001 notFlipped)

  /// **これが本体。** 産まれる弾は原点に作るので、Spawn 側は
  /// 撃った側と別の値になる。同じ値だと、入れ替えても誰も気づかない ——
  /// `SpawnOrigin` を `AtShooter` に取り違えるとここで割れる
  [<Test>]
  member _.``Spawn 側の aim は原点から。撃った側とは別の値``() =
    let atOrigin = float32 (Math.Atan2(30.0, -100.0))
    let env = envAt 10.0f 20.0f
    env.Spawn.ToPlayer |> should (equalWithin 0.0001) atOrigin
    env.Spawn.ToPlayer |> should not' (equalWithin 0.0001 env.Aim.ToPlayer)

  /// 組み立ての 4 欄 が入れ替わっていないか。**敵を 1 体 だけ置いて、
  /// 撃った側と原点から見た向きが別の値になるところを選ぶ**
  [<Test>]
  member _.``Env の 4 本 の aim は、それぞれ別のところから来る``() =
    // 敵を 1 体 置く。置かないと enemy 側 2 本 が どちらも 0 になり、
    // 入れ替えても気づけない（下の対照がその条件を固定している）
    Manager.addEnemy (StubBullet(-40.0f, -60.0f))
    let env = envAt 10.0f 20.0f

    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f
    // 撃った側の位置から自機へ
    env.Aim.ToPlayer |> should (equalWithin 0.0001) (float32 (Math.Atan2(20.0, -80.0)))
    // 撃った側の位置から敵 (-40, -60) へ: Atan2(-50, 80)
    env.Aim.ToEnemy |> should (equalWithin 0.0001) (float32 (Math.Atan2(-50.0, 80.0)))
    // 原点から自機へ
    env.Spawn.ToPlayer |> should (equalWithin 0.0001) (float32 (Math.Atan2(30.0, -100.0)))
    // 原点から、いちばん近い敵 (-40, -60) へ: Atan2(-40, 60)
    env.Spawn.ToEnemy |> should (equalWithin 0.0001) (float32 (Math.Atan2(-40.0, 60.0)))
    // **4 本 とも別の値。** 1 つ でも同値だと入れ替えを当てられない
    [ env.Aim.ToPlayer; env.Aim.ToEnemy; env.Spawn.ToPlayer; env.Spawn.ToEnemy ]
    |> List.distinct |> List.length |> should equal 4

  /// aim を読まないコマの Env。
  ///
  /// **以前は同じ形が 3 か所 に在って、値が揃うことを門にしていた。**
  /// `Runner.load` が `Env` を取らなくなって 2 か所 になり、
  /// `FsBulletML2.Front` に吸われて 1 か所 になった。
  /// **検証をどこかへ移したのではなく、見張る対象が消えた。**
  /// いま見るのは「aim 4 本 が 0 で、乱数とランクは素通し」だけ
  [<Test>]
  member _.``aim を読まない Env は aim 4 本 が 0``() =
    let e = FrontEnv.noAim (world ())
    e.Rank |> should equal 0.25f
    e.Rand () |> should equal 0.5f
    e.Aim.ToPlayer |> should equal 0.0f
    e.Aim.ToEnemy |> should equal 0.0f
    e.Spawn.ToPlayer |> should equal 0.0f
    e.Spawn.ToEnemy |> should equal 0.0f

  /// 対照 —— **敵が居ないときは enemy 側が 0。**
  /// 上の門は敵を置いてから測っている。置き忘れると 0 どうしの一致になり、
  /// 入れ替えを当てられなくなるので、0 になる条件を字で固定しておく
  [<Test>]
  member _.``敵が 1 体 も居なければ、敵向きの aim は 0``() =
    let env = envAt 10.0f 20.0f
    env.Aim.ToEnemy |> should equal 0.0f
    env.Spawn.ToEnemy |> should equal 0.0f

  /// **選んだ相手を覚えること。** 覚えないと毎コマ 選び直して相手が
  /// 入れ替わり、軌跡が変わる。1 体 目 を置いて聞いたあと、より近い
  /// 2 体 目 を置いても答えが動かないことで見る
  [<Test>]
  member _.``一度 選んだ相手は、より近い敵が現れても入れ替わらない``() =
    Manager.addEnemy (StubBullet(-40.0f, -60.0f))
    let w = world ()
    let first = (FrontEnv.at w MonoGameFront.space MonoGameFront.origin 10.0f 20.0f).Aim.ToEnemy
    Manager.addEnemy (StubBullet(11.0f, 21.0f))
    let second = (FrontEnv.at w MonoGameFront.space MonoGameFront.origin 10.0f 20.0f).Aim.ToEnemy
    second |> should equal first
    // **較正。** 新しい世界なら近いほうを選ぶ —— 上の一致が
    // 「そもそも敵を見ていない」ことの結果ではないと分かる
    let fresh = (FrontEnv.at (world ()) MonoGameFront.space MonoGameFront.origin 10.0f 20.0f).Aim.ToEnemy
    fresh |> should not' (equal first)
