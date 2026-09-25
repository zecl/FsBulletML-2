namespace FsBulletML2.MonoGame.Tests

open System
open System.Collections.Generic
open NUnit.Framework
open FsUnit
open Microsoft.Xna.Framework
open FsBulletML2.MonoGame

/// 区画分け の当たり判定 は、総当たり と同じ弾 に当たる。画面 の外 の弾 でも落ちない
[<TestFixture>]
[<NonParallelizable>]
type GridGate() =

    /// 画面（480 x 640）の外 も混ぜる。半径 は区画（10）より大きいもの も混ぜる
    let scatter (rnd: Random) (n: int) =
        [
            for _ in 1..n ->
                let x = float32 (rnd.NextDouble() * 680.0 - 100.0)
                let y = float32 (rnd.NextDouble() * 840.0 - 100.0)
                let b = StubBullet(x, y) :> IBullet
                b.Radius <- float32 (rnd.NextDouble() * 24.0)
                b
        ]

    /// 総当たり の答え。弾 を消さずに数える
    let bruteHits (pos: Vector2) (radius: float32) (targets: IBullet list) =
        targets
        |> List.filter (fun t -> Vector2.Distance(t.Pos, pos) < t.Radius + radius)
        |> List.map (fun t -> t.Pos.X, t.Pos.Y, t.Radius)
        |> List.sort

    [<SetUp>]
    member _.SetUp() = Manager.removeAll ()

    [<Test>]
    member _.``敵 と自機 の当たり は、総当たり と同じ弾 に当たる（3000 通り）``() =
        let rnd = Random(20260925)

        for round in 1..3000 do
            Manager.removeAll ()
            let players = scatter rnd (rnd.Next(0, 40))
            let enemies = scatter rnd (rnd.Next(0, 40))
            players |> List.iter Manager.addPlayerBullet
            enemies |> List.iter Manager.addEnemyBullet
            Manager.updateSpace ()

            let at =
                Vector2(float32 (rnd.NextDouble() * 680.0 - 100.0), float32 (rnd.NextDouble() * 840.0 - 100.0))

            let radius = float32 (rnd.NextDouble() * 40.0)

            let check (hit: Vector2 -> float32 -> (unit -> unit) -> unit) (targets: IBullet list) =
                let want = bruteHits at radius targets
                let mutable calls = 0
                hit at radius (fun () -> calls <- calls + 1)

                let got =
                    targets
                    |> List.filter (fun t -> not t.Used)
                    |> List.map (fun t -> t.Pos.X, t.Pos.Y, t.Radius)
                    |> List.sort

                (round, got) |> should equal (round, want)
                (round, calls) |> should equal (round, want.Length)

            check Manager.checkEnemyCollision players
            check Manager.checkPlayerCollision enemies

    [<Test>]
    member _.``画面 の遠く外 の弾 でも区画 に入れられる``() =
        for x, y in
            [
                (-5000.0f, -5000.0f)
                (5000.0f, 5000.0f)
                (-1.0f, 700.0f)
                (Single.NaN, 10.0f)
            ] do
            Manager.removeAll ()
            Manager.addEnemyBullet (StubBullet(x, y))
            Manager.addPlayerBullet (StubBullet(x, y))
            Manager.updateSpace ()
