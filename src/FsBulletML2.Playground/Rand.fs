namespace FsBulletML2.Playground

/// **種から決まる乱数の並び。** 弾幕も描画も Bolero も知らない ——
/// `Pacing` / `Seek` と同じ理由でここに居る（`Front.Tests` が `Link` で借りる）。
///
/// --- なぜ `System.Random` を使わないか
///
/// 種を Share URL に乗せる。**並びを決めるのが runtime だと、絵は
/// 「同じ種で、同じ .NET で」しか揃わない** —— リンクは残るが、
/// 走らせる側の runtime が上がった時点で別の絵になる。
/// **リンクが指しているのは「その走り」なので、並びはこちらが決める。**
///
/// `System.Random(seed)` が同じ種で同じ並びを返すことは測った（返す）。
/// それでも使わないのは、**契約が「同じ実装なら」だから。**
///
/// --- xorshift32
///
/// 3 行。速さも状態も、弾幕 1 コマ の中では見えない大きさ
/// （1 コマ の値段は弾数で決まる。v1.7 で測った）。
///
/// **0 を種にしない。** xorshift は 0 から動かない —— 入ってきたら 1 に倒す。
[<Sealed>]
type SeededRandom(seed: int) =

  let start = if seed = 0 then 1u else uint seed
  let mutable state = start

  /// **同じ種の 2 本 は、同じ並びを返す。**
  member _.Next() : float32 =
    state <- state ^^^ (state <<< 13)
    state <- state ^^^ (state >>> 17)
    state <- state ^^^ (state <<< 5)
    // `[0, 1)` に落とす。**`% 1000000` で刻む** —— 割り切れる形にして、
    // 2 つ の runtime で同じ数が出る
    float32 (state % 1000000u) / 1000000.0f

  /// 並びを頭へ戻す。**面を建て直すときに呼ぶ** ——
  /// 戻さないと、同じ種でも「建て直したあと」が別の走りになる
  member _.Restart() = state <- start

  member _.Seed = seed

module SeededRandom =

  /// 種の上限。**リンクに乗るので短いほうがよい。** 6 桁 は
  /// 100 万 通り —— 「同じ走りをもう一度」に足りて、字が長くならない
  [<Literal>]
  let Max = 999999

  /// 種を丸める。**負も 0 も 1 に倒す**（xorshift は 0 から動かない）
  let clamp (n: int) = if n <= 0 then 1 elif n > Max then Max else n

  /// 時計から種を作る。**起動のたびに違う走りにする**ための入口で、
  /// ここから先は種が決める
  let fromClock () =
    clamp (int (System.DateTime.UtcNow.Ticks % int64 Max) + 1)
