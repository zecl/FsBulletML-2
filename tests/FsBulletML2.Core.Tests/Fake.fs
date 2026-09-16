namespace FsBulletML2.Core.Tests

open System.Collections.Generic
open FsBulletML2

// グローバルの口（IBulletMLManager / BulletMLManager）は FsBulletML2.Front へ
// 移した。この試験プロジェクトはもうそこを触らない ——
// 新 API は Env を引数で受けるので、走らせる側にグローバルは要らない。
//
// Reset() で明示的に頭へ戻す形のほうが、片方だけ Reset を忘れる・違う
// 頭出しをする、という取り違いが構造的に起きない
type SharedRandomStream() =
  let mutable i = 0
  member _.Reset() = i <- 0
  member _.Next() : float32 =
    let v = float32 ((i * 37) % 101) / 101.0f
    i <- i + 1
    v

/// 敵を狙う向きの、狙う相手の位置。自機（FixedManager が持つ）と区別できる場所に置く。
/// 原点に置くと根の弾も原点なので atan2(0, -0) = π になり、
/// ライブラリの値ではなく偽の弾の副作用が控えに出る。
module FakeEnemy =
  let X = -40.0f
  let Y = -60.0f

/// 位置と物理量を持つだけの弾。いまはベンチ専用。
///
/// 旧 API を落とす前は `IBulletmlObject` の 19 メンバ を実装していて、
/// エンジンがここを呼び返していた。新 API は値の受け渡しだけなので、
/// 呼び返される面は 1 つ も要らなくなった —— 残っているのは
/// 「フロントが持っている物理量」に当たる箱だけ。
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
