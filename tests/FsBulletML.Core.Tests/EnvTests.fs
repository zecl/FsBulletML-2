namespace FsBulletML.Core.Tests

open System
open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML
open FsBulletML.Domain
open FsBulletML.Processable

/// Env はレコード 1 行で作れること。17 メンバの Fake を書かずに済むことが要点。
[<TestFixture>]
[<NonParallelizable>]
type EnvTests() =

  [<Test>]
  member _.``Env はレコードリテラルで作れる``() =
    let env = { Rand = (fun () -> 0.5f); Rank = 0.25f; AimDir = 1.0f; EnemyAimDir = 2.0f }
    env.Rank |> should equal 0.25f
    env.Rand () |> should equal 0.5f

  [<Test>]
  member _.``Rand は呼ぶたびに読み直される``() =
    let mutable n = 0
    let env = { Rand = (fun () -> n <- n + 1; float32 n); Rank = 0.f; AimDir = 0.f; EnemyAimDir = 0.f }
    env.Rand () |> should equal 1.0f
    env.Rand () |> should equal 2.0f

  [<Test>]
  member _.``グローバルと弾から組んだ Env は、その弾の位置から aim を持つ``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.25f, 30.0f, 100.0f))
    let born = List<FakeBullet>()
    let b = FakeBullet(0, born) :> IBulletmlObject
    b.X <- 10.0f
    b.Y <- 20.0f
    let env =
      { Rand = BulletMLManager.GetRandom
        Rank = BulletMLManager.GetRank ()
        AimDir = b.GetAimDir ()
        EnemyAimDir = b.GetEnemyAimDir () }
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
