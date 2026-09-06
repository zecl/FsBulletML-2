namespace FsBulletML2.Unity2D.Tests

open System
open NUnit.Framework
open FsUnit
open UnityEngine
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Unity2D

/// このフロントが `Env` を組むところの門。**MonoGame 側の `EnvGate` と対。**
///
/// 何を守るのかは向こうの doc に書いてある。ここに要るのは
/// **こちらの決めごとが向こうと違うこと**で、
///
///     Space.YUp        —— Y を反転しない
///     SpawnOrigin.AtShooter —— 産まれる弾は撃った側と同じ場所
///     産まれる弾の相手は、撃った側が覚えている相手と同じ
///
/// **均すと全弾幕の軌跡が割れる。** 3 つ とも取り違えても型は通るので、
/// 字で固定する。

/// 位置だけを持つ IDefaultBullet。**`Manager.enemies` へ置くためだけのもの。**
/// オブジェクト式には member val を書けないので型にしてある
type private StubBullet(x: float32, y: float32) =
  let mutable pos = Vector3(x, y, 0.0f)
  interface IDefaultBullet with
    member _.Update() = ()
    member _.Pos with get () = pos and set v = pos <- v
    member _.X with get () = pos.x and set v = pos <- Vector3(v, pos.y, pos.z)
    member _.Y with get () = pos.y and set v = pos <- Vector3(pos.x, v, pos.z)
    member val Radius = 1.0f with get, set
    member val Root = false with get, set
    member val Speed = 0.0f with get, set
    member val Dir = 0.0f with get, set
    member val AccelerationX = 0.0f with get, set
    member val AccelerationY = 0.0f with get, set
    member val Used = true with get, set
    member val IsBullet = false with get, set
    member val BulletRoot = false with get, set
    member val BulletType = BulletType.Enemy with get, set
    member val ShootingDirection = ShootingDirection.BulletVertical with get, set
    member _.Init() = ()
    member _.Vanish() = ()
    member _.SetScript(_) = ()
    member _.SetRun(_) = ()
    member _.Script = None
    member _.Finished = false

/// 下が本体の門。上の doc（このフロントが Env を組むところ）を参照
[<TestFixture>]
[<NonParallelizable>]
type EnvGate() =

  /// 自機 (3, 10)、rand 0.5、rank 0.25。**Unity の単位**なので値が小さい
  let fixedManager () =
    { new IBulletMLManager with
        member _.GetRandom() = 0.5f
        member _.GetRank() = 0.25f
        member _.GetPlayerPosX() = 3.0f
        member _.GetPlayerPosY() = 10.0f }

  let front () = Unity2DEnv () :> IFrontEnv

  let envOf (w: IFrontEnv) (x: float32) (y: float32) =
    FrontEnv.at w Unity2DFront.space Unity2DFront.origin x y

  let envAt (x: float32) (y: float32) = envOf (front ()) x y

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(fixedManager ())
    // 敵の一覧はグローバル。前の試験の残りを持ち越さない。
    // **`Manager.removeAll` では抜けない** —— あれは `Used` を寝かせるだけで、
    // 一覧から抜くのは `Manager.free`。そちらは MonoBehaviour へのキャストを
    // 通すので、位置だけのスタブは落ちる。ここは一覧を直に空ける
    Manager.enemies.Clear()
    Manager.rootBullets.Clear()
    Manager.enemyBullets.Clear()
    Manager.playerBullets.Clear()

  [<Test>]
  member _.``自機を狙う向きは、弾の位置から引く。Y は反転しない``() =
    // 弾 (1, 2) から自機 (3, 10) へ: Atan2(3-1, 10-2) = Atan2(2, 8)
    let expected = float32 (Math.Atan2(2.0, 8.0))
    (envAt 1.0f 2.0f).Aim.ToPlayer |> should (equalWithin 0.0001) expected

  /// **MonoGame は反転する。** 同じ式に見えて座標系が逆なので、
  /// `Space` を取り違えると全弾幕の軌跡が割れる。ここで符号を固定する
  [<Test>]
  member _.``Y を反転する式とは別の値になる``() =
    let flipped = float32 (Math.Atan2(2.0, -8.0))
    (envAt 1.0f 2.0f).Aim.ToPlayer |> should not' (equalWithin 0.0001 flipped)

  /// **これが本体。** 産まれる弾は撃った側と同じ場所に作るので、
  /// Spawn 側は撃った側と同じ値になる —— `SpawnOrigin` を `AtOrigin` に
  /// 取り違えるとここで割れる
  [<Test>]
  member _.``Spawn 側の aim は撃った側と同じ場所から``() =
    let env = envAt 1.0f 2.0f
    env.Spawn.ToPlayer |> should (equalWithin 0.0001) env.Aim.ToPlayer
    // **較正。** 原点から引いた値は別 —— 上の一致が
    // 「どちらも 0 だった」ことの結果ではないと分かる
    let atOrigin = float32 (Math.Atan2(3.0, 10.0))
    env.Aim.ToPlayer |> should not' (equalWithin 0.0001 atOrigin)

  /// 組み立ての 4 欄 が入れ替わっていないか。
  /// **自機と敵を別のところに置いて、値が 4 本 とも分かれる形にする**
  [<Test>]
  member _.``Env の 4 本 の aim は、それぞれ別のところから来る``() =
    // 敵を 1 体 置く。置かないと enemy 側 2 本 が どちらも 0 になり、
    // 入れ替えても気づけない（下の対照がその条件を固定している）
    Manager.addEnemy (StubBullet(-4.0f, -6.0f))
    let env = envOf (front ()) 1.0f 2.0f

    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f
    // 撃った側 (1, 2) から自機 (3, 10) へ
    env.Aim.ToPlayer |> should (equalWithin 0.0001) (float32 (Math.Atan2(2.0, 8.0)))
    // 撃った側 (1, 2) から敵 (-4, -6) へ: Atan2(-5, -8)
    env.Aim.ToEnemy |> should (equalWithin 0.0001) (float32 (Math.Atan2(-5.0, -8.0)))
    // **player 側 と enemy 側 は別。** 同値だと入れ替えを当てられない
    env.Aim.ToPlayer |> should not' (equalWithin 0.0001 env.Aim.ToEnemy)
    // **Spawn 側 は撃った側と同じ。** こちらは同じであることが決めごと
    env.Spawn.ToPlayer |> should (equalWithin 0.0001) env.Aim.ToPlayer
    env.Spawn.ToEnemy |> should (equalWithin 0.0001) env.Aim.ToEnemy

  /// aim を読まないコマの Env
  [<Test>]
  member _.``aim を読まない Env は aim 4 本 が 0``() =
    let e = FrontEnv.noAim (front ())
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
    let env = envAt 1.0f 2.0f
    env.Aim.ToEnemy |> should equal 0.0f
    env.Spawn.ToEnemy |> should equal 0.0f

  /// **産まれる弾の相手は、撃った側が覚えている相手と同じ。**
  /// このフロントは弾を撃った側と同じ場所に作るので、産まれる弾から見た
  /// 相手も撃った側と同じ。**MonoGame は原点に作るので選び直す** ——
  /// そちらの振る舞いをこちらへ持ってくるとここで割れる。
  ///
  /// **変異で穴が見つかって足した。** `TrySpawnTargetFrom` を
  /// 選び直すほう（`TryNearest`）に差し替えても、通しが緑のまま通った
  [<Test>]
  member _.``産まれる弾の相手は、撃った側が覚えている相手と同じ``() =
    // 遠い E1 だけ置いて 1 回 引く。ここで E1 を覚える
    Manager.addEnemy (StubBullet(5.0f, 8.0f))
    let w = front ()
    let first = envOf w 1.0f 2.0f
    // より近い E2 を足す。**選び直すなら Spawn 側はこちらを向く**
    Manager.addEnemy (StubBullet(1.5f, 2.5f))
    let second = envOf w 1.0f 2.0f

    let toE1 = float32 (Math.Atan2(4.0, 6.0))
    second.Aim.ToEnemy |> should (equalWithin 0.0001) toE1
    second.Aim.ToEnemy |> should (equalWithin 0.0001) first.Aim.ToEnemy
    // **ここが本体。** 産まれる弾も、覚えている E1 を向く
    second.Spawn.ToEnemy |> should (equalWithin 0.0001) toE1

    // **較正。** 新しい世界なら近い E2 を選ぶ —— 上の一致が
    // 「そもそも敵を見ていない」ことの結果ではないと分かる
    let fresh = envOf (front ()) 1.0f 2.0f
    fresh.Spawn.ToEnemy |> should (equalWithin 0.0001) (float32 (Math.Atan2(0.5, 0.5)))
    fresh.Spawn.ToEnemy |> should not' (equalWithin 0.0001 toE1)

  /// **選んだ相手を覚えること。** 覚えないと毎コマ 選び直して相手が
  /// 入れ替わり、軌跡が変わる
  [<Test>]
  member _.``一度 選んだ相手は、より近い敵が現れても入れ替わらない``() =
    Manager.addEnemy (StubBullet(-4.0f, -6.0f))
    let w = front ()
    let first = (envOf w 1.0f 2.0f).Aim.ToEnemy
    Manager.addEnemy (StubBullet(1.1f, 2.1f))
    let second = (envOf w 1.0f 2.0f).Aim.ToEnemy
    second |> should equal first
    // **較正。** 新しい世界なら近いほうを選ぶ
    let fresh = (envOf (front ()) 1.0f 2.0f).Aim.ToEnemy
    fresh |> should not' (equal first)

  /// **世界は弾 1 個 につき 1 個。** 使い回すと別の弾が選んだ相手を
  /// 引き継ぐ。`Forget` で捨てられることを見る
  [<Test>]
  member _.``Forget すると相手を選び直す``() =
    Manager.addEnemy (StubBullet(-4.0f, -6.0f))
    let w = Unity2DEnv ()
    let iw = w :> IFrontEnv
    let first = (envOf iw 1.0f 2.0f).Aim.ToEnemy
    Manager.addEnemy (StubBullet(1.1f, 2.1f))
    w.Forget ()
    let second = (envOf iw 1.0f 2.0f).Aim.ToEnemy
    second |> should not' (equal first)
    second |> should (equalWithin 0.0001) (float32 (Math.Atan2(0.1, 0.1)))
