namespace FsBulletML2.Core.Tests

open System.Collections.Generic
open FsBulletML2

// Reset() で明示的に頭へ戻す。片方だけ Reset を忘れると、頭出しがずれる。
type SharedRandomStream() =
  let mutable i = 0
  member _.Reset() = i <- 0
  member _.Next() : float32 =
    let v = float32 ((i * 37) % 101) / 101.0f
    i <- i + 1
    v

/// 敵を狙う向きの、狙う相手の位置。自機と区別できる場所に置く。
/// 原点に置くと根の弾も原点なので atan2(0, -0) = π になり、偽の弾の副作用が控えに出る。
module FakeEnemy =
  let X = -40.0f
  let Y = -60.0f

/// 位置と物理量を持つだけの弾。
type FakeBullet(id: int, born: List<FakeBullet>) =
  member val X = 0.0f with get, set
  member val Y = 0.0f with get, set
  member val Speed = 0.0f with get, set
  member val Dir = 0.0f with get, set
  member val Used = false with get, set
  member val IsBullet = false with get, set
  member _.Id = id
  /// 根の弾から数えて産まれた順。親子は問わず 1 本の並びに積む
  member _.Born = born
  /// 旧 IBulletmlObject.Init の写し。ベンチが弾を場に出すときに呼ぶ
  member this.Init() = this.Used <- true
