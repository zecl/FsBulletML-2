namespace FsBulletML2.Benchmarks

open FsBulletML2

/// $rand / $rank / 自機位置を固定する。`BulletMLManager` は使う側ごとに Init し直す。
/// 弾の実体は tests の FakeBullet をリンクしたまま。写すと片方が古びる。
type FixedManager(rand: float32, rank: float32, playerX: float32, playerY: float32) =
  interface IBulletMLManager with
    member _.GetRandom() = rand
    member _.GetRank() = rank
    member _.GetPlayerPosX() = playerX
    member _.GetPlayerPosY() = playerY
