namespace FsBulletML2.Core.Tests
// 旧 API（IBulletmlObject）の Obsolete 警告を、**このファイルだけ**止める。
// ここは旧経路を意図して走らせる側だから（新旧を突き合わせる橋の材料）。
//
// プロジェクト単位（NoWarn）で止めない。止めると、**新しく書いた試験が
// うっかり旧 API を使っても警告が出なくなる**。
// 効きがファイル単位であることは較正済み —— nowarn を置いていない
// ファイルで旧 API に触ると FS0044 が出る。
#nowarn "44"


open System
open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Processable

/// Env はレコード 1 行で作れること。17 メンバの Fake を書かずに済むことが要点。
[<TestFixture>]
[<NonParallelizable>]
type EnvTests() =

  [<Test>]
  member _.``Env はレコードリテラルで作れる``() =
    let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; AimDir = 1.0f; EnemyAimDir = 2.0f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }
    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f

  [<Test>]
  member _.``Rand は呼ぶたびに読み直される``() =
    let mutable n = 0
    let env = { Rand = (fun () -> n <- n + 1; float32 n); Rank = 0.f; AimDir = 0.f; EnemyAimDir = 0.f; SpawnAimDir = 0.f; SpawnEnemyAimDir = 0.f }
    env.Rand () |> should equal 1.0f
    env.Rand () |> should equal 2.0f

  [<Test>]
  member _.``グローバルと弾から組んだ Env は、その弾の位置から aim を持つ``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.25f, 30.0f, 100.0f))
    let born = List<FakeBullet>()
    let b = FakeBullet(0, born) :> IBulletmlObject
    b.X <- 10.0f
    b.Y <- 20.0f
    // 組むのは envOfGlobal 1 本だけ。ここで自前のレコードを組むと、
    // envOfGlobal が field を埋め忘れていても門が緑になる
    let env = BulletRunner.envOfGlobal b
    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f
    // 弾の位置 (10, 20) から、自機位置 (30, 100) への向きを手計算で検証。
    // GetAimDir: Atan2(playerX - x, -(playerY - y)) = Atan2(20, -80)
    let expectedAimDir = float32 (Math.Atan2(20.0, -80.0))
    env.AimDir |> should (equalWithin 0.0001) expectedAimDir
    env.AimDir |> should (equalWithin 0.0001) (b.GetAimDir())
    // 弾の位置 (10, 20) から、敵位置 (-40, -60) への向きを手計算で検証。
    // GetEnemyAimDir: Atan2(enemyX - x, -1.0 * (enemyY - y)) = Atan2(-50, 80)
    let expectedEnemyAimDir = float32 (Math.Atan2(-50.0, 80.0))
    env.EnemyAimDir |> should (equalWithin 0.0001) expectedEnemyAimDir
    env.EnemyAimDir |> should (equalWithin 0.0001) (b.GetEnemyAimDir())

  /// 産まれる弾の向きは、撃った側ではなく「産まれる弾の位置」から決まる。
  ///
  /// FakeBullet.GetNewBullet は原点に弾を作るので、SpawnAimDir は原点から
  /// 自機への向きになり、撃った側の位置 (10, 20) から出る AimDir とは別の値
  /// になる。両者が同じ値だと、取り違えても誰も気づかない
  [<Test>]
  member _.``Env の Spawn 側の aim は、産まれる弾の位置から決まる``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.25f, 30.0f, 100.0f))
    let born = List<FakeBullet>()
    let b = FakeBullet(0, born) :> IBulletmlObject
    b.X <- 10.0f
    b.Y <- 20.0f
    let env = BulletRunner.envOfGlobal b
    // 原点 (0, 0) から自機 (30, 100) へ: Atan2(30, -100)
    let expectedSpawnAim = float32 (Math.Atan2(30.0, -100.0))
    env.SpawnAimDir |> should (equalWithin 0.0001) expectedSpawnAim
    // 原点 (0, 0) から敵 (-40, -60) へ: Atan2(-40, 60)
    let expectedSpawnEnemyAim = float32 (Math.Atan2(-40.0, 60.0))
    env.SpawnEnemyAimDir |> should (equalWithin 0.0001) expectedSpawnEnemyAim
    // 撃った側の値と別物であること。ここが同じなら上の 2 つは何も担保しない
    env.SpawnAimDir |> should not' (equalWithin 0.0001 env.AimDir)
    env.SpawnEnemyAimDir |> should not' (equalWithin 0.0001 env.EnemyAimDir)
