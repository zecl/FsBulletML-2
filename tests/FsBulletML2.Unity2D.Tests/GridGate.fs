namespace FsBulletML2.Unity2D.Tests

open System
open NUnit.Framework
open FsUnit
open UnityEngine
open FsBulletML2.Unity2D

/// 区画分け の当たり判定 は、総当たり と同じ弾 に当たる。画面 の外 の弾 でも落ちない。
/// MonoGame 側 と対。こちら は Unity の単位（画面 4.8 x 6.4、区画 0.05）
[<TestFixture>]
[<NonParallelizable>]
type GridGate() =

    /// `removeAll` は印 を付けるだけ で一覧 を空 にしない（`free` が片付ける）
    let reset () =
        [
            Manager.enemies
            Manager.rootBullets
            Manager.enemyBullets
            Manager.playerBullets
        ]
        |> List.iter (fun l -> l.Clear())

        Manager.updateSpace ()

    /// 画面 の外 も混ぜる。半径 は区画 より大きいもの も混ぜる
    let scatter (rnd: Random) (n: int) =
        [
            for _ in 1..n ->
                let x = float32 (rnd.NextDouble() * 6.8 - 1.0)
                let y = float32 (rnd.NextDouble() * 8.4 - 1.0)
                let b = StubBullet(x, y) :> IDefaultBullet
                b.Radius <- float32 (rnd.NextDouble() * 0.24)
                b
        ]

    let key (t: IDefaultBullet) = t.Pos.x, t.Pos.y, t.Radius

    [<SetUp>]
    member _.SetUp() = reset ()

    [<Test>]
    member _.``敵 と自機 の当たり は、総当たり と同じ弾 に当たる（3000 通り）``() =
        let rnd = Random(20260925)

        for round in 1..3000 do
            reset ()
            let players = scatter rnd (rnd.Next(0, 40))
            let enemies = scatter rnd (rnd.Next(0, 40))
            players |> List.iter Manager.addPlayerBullet
            enemies |> List.iter Manager.addEnemyBullet
            Manager.updateSpace ()

            let at =
                Vector2(float32 (rnd.NextDouble() * 6.8 - 1.0), float32 (rnd.NextDouble() * 8.4 - 1.0))

            let radius = float32 (rnd.NextDouble() * 0.4)

            let check (hit: Vector2 -> float32 -> (unit -> unit) -> unit) (targets: IDefaultBullet list) =
                let want =
                    targets
                    |> List.filter (fun t -> Vector2.Distance(Vector2(t.Pos.x, t.Pos.y), at) < t.Radius + radius)
                    |> List.map key
                    |> List.sort

                let mutable calls = 0
                hit at radius (fun () -> calls <- calls + 1)
                let got = targets |> List.filter (fun t -> not t.Used) |> List.map key |> List.sort
                (round, got) |> should equal (round, want)
                (round, calls) |> should equal (round, want.Length)

            check Manager.checkEnemyCollision players
            check Manager.checkPlayerCollision enemies

    [<Test>]
    member _.``画面 の遠く外 の弾 でも区画 に入れられる``() =
        for x, y in [ (-50.0f, -50.0f); (50.0f, 50.0f); (-0.01f, 7.0f); (Single.NaN, 1.0f) ] do
            reset ()
            Manager.addEnemyBullet (StubBullet(x, y))
            Manager.addPlayerBullet (StubBullet(x, y))
            Manager.updateSpace ()
