namespace FsBulletML2.Benchmarks

open FsBulletML2

/// $rand / $rank / 自機位置を固定する。`BulletMLManager` は static mutable なので
/// 使う側ごとに Init し直すこと。
///
/// **元は tests/FsBulletML2.Core.Tests/Fake.fs に在って、ここへリンクしていた。**
/// `BulletMLManager` が `FsBulletML2.Front` へ移り、Core.Tests は Front を
/// 参照しない（「公開だけで書けているか」を測る役目が消えるため）ので、
/// この 1 本 だけこちらへ移した。**弾の実体（FakeBullet）はいまも
/// tests 側の 1 本 をリンクしている** —— あちらは同じ形を 2 つ 持つと
/// 片方が古びるので、実体を 1 つ に保つほうが効く。
type FixedManager(rand: float32, rank: float32, playerX: float32, playerY: float32) =
  interface IBulletMLManager with
    member _.GetRandom() = rand
    member _.GetRank() = rank
    member _.GetPlayerPosX() = playerX
    member _.GetPlayerPosY() = playerY
