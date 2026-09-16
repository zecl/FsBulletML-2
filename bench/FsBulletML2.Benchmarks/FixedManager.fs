namespace FsBulletML2.Benchmarks

open FsBulletML2

/// $rand / $rank / 自機位置を固定する。`BulletMLManager` は static mutable なので
/// 使う側ごとに Init し直すこと。
///
/// 元は tests/FsBulletML2.Core.Tests/Fake.fs に在って、ここへリンクしていた。
///
/// 片方が古びるので、実体を 1 つ に保つほうが効く。
type FixedManager(rand: float32, rank: float32, playerX: float32, playerY: float32) =
  interface IBulletMLManager with
    member _.GetRandom() = rand
    member _.GetRank() = rank
    member _.GetPlayerPosX() = playerX
    member _.GetPlayerPosY() = playerY
