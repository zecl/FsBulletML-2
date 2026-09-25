namespace FsBulletML2.MonoGame.Tests

open NUnit.Framework
open FsUnit
open Microsoft.Xna.Framework
open FsBulletML2.MonoGame

/// 当たり判定。1 発 当たったら `cont` は 1 回、当たった弾 は消える
[<TestFixture>]
[<NonParallelizable>]
type CollisionGate() =

    let shot (x: float32) (y: float32) =
        let b = StubBullet(x, y) :> IBullet
        b.Radius <- 1.0f
        b

    [<SetUp>]
    member _.SetUp() = Manager.removeAll ()

    [<Test>]
    member _.``敵 に自機 の弾 が 1 発 当たると cont は 1 回、弾 は消える``() =
        let b = shot 100.0f 100.0f
        Manager.addPlayerBullet b
        Manager.updateSpace ()
        let mutable hits = 0
        Manager.checkEnemyCollision (Vector2(100.0f, 100.0f)) 5.0f (fun () -> hits <- hits + 1)
        hits |> should equal 1
        b.Used |> should equal false

    [<Test>]
    member _.``自機 に敵 の弾 が 1 発 当たると cont は 1 回、弾 は消える``() =
        let b = shot 100.0f 100.0f
        Manager.addEnemyBullet b
        Manager.updateSpace ()
        let mutable hits = 0
        Manager.checkPlayerCollision (Vector2(100.0f, 100.0f)) 5.0f (fun () -> hits <- hits + 1)
        hits |> should equal 1
        b.Used |> should equal false

    [<Test>]
    member _.``届かない弾 には当たらない``() =
        let b = shot 100.0f 100.0f
        Manager.addPlayerBullet b
        Manager.updateSpace ()
        let mutable hits = 0
        Manager.checkEnemyCollision (Vector2(100.0f, 110.0f)) 5.0f (fun () -> hits <- hits + 1)
        hits |> should equal 0
        b.Used |> should equal true
