namespace FsBulletML2.Playground

/// **目的のコマまで進める**だけを決める。弾幕も描画も Bolero も知らない。
/// `Pacing` と同じ理由でここに居る —— `Front.Tests` が `Link` で借りて
/// .NET でそのまま当てられる（`Main.fs` は呼ぶだけで判断を持たない）。
///
/// --- 待たせない
///
/// 同梱 176 本 を 3600 コマ ずつ回して測った（.NET）——
///
///     1 コマ の値段   中央 0.08 ms / 9 割 0.27 ms / 最大 3.67 ms
///     600 コマ 飛ぶ   中央 47 ms / いちばん重いもので 2205 ms
///
/// **いちばん重いもので 2.2 秒。** 物が走るのは WASM なのでこれは下限で、
/// その場で回し切ると画面が固まる。だから **1 フレームに使う時間を切って、
/// 着くまで何フレームかに分ける。** 進んでいることはコマ数の表示に出る。
///
/// ブラウザでも測った（同梱の 1 本、着いた時点で弾 337 個）——
///
///     600 コマ 飛ぶ   1366 ms / `StepFrame` 141 回（1 回 あたり 9.7 ms）
///
/// **その場で回し切っていたら 1.4 秒 固まっていた。** 1 回 が 9.7 ms なのは
/// 予算 8 ms を超えた 1 コマ ぶんと、JS との往復が乗るから。
/// この 1 本 では .NET の同じ弾数のものより 1 コマ が 20 倍 ほど高かった
/// （**1 本 での比なので、倍率そのものは当てにしない**）。
///
/// --- 前へしか進めない
///
/// 面は逆再生できないので、戻るには建て直して 0 から進め直すしかない。
/// 建て直すのは面を持っている側（`Main.fs`）の仕事なので、ここは
/// **建て直しが要るかどうかだけ**を言う（`needsRestart`）。
///
/// **建て直すと乱数の並びも建て直る。** `$rand` を使う弾幕は、戻った先が
/// さっき見た絵と同じにならない。種を持つのは別の版の話。
///
/// --- 着いたら止まる
///
/// 1 コマ送りと同じ —— 見たいコマで止まる。走ったままだと、着いた瞬間に
/// 流れてしまって見えない。
[<Struct>]
type Seek =
  { /// 目的のコマ。**飛んでいなければ -1**（0 は「頭へ飛ぶ」で、別の意味）
    Target: int }

module Seek =

  /// 飛び先の上限。**待たせないので、大きくても画面は止まらない** ——
  /// それでも置くのは、重い弾幕で人が待つ時間そのものが伸びるから。
  /// 36000 は 60 コマ/秒 で 10 分 ぶん
  [<Literal>]
  let Max = 36000

  /// 1 フレームで進めるのに使ってよい時間。**16.6 ms の内側に収める** ——
  /// 残りで描画と、ブラウザ自身の仕事が要る
  [<Literal>]
  let BudgetMs = 8.0

  /// 飛んでいない。
  let idle = { Target = -1 }

  /// 飛び先を決める。負は頭へ倒し、上限で丸める。
  /// **`JSInvokable` なので UI に無い値も来うる**
  let toFrame (n: int) = { Target = max 0 (min Max n) }

  let isRunning (s: Seek) = s.Target >= 0

  /// 戻るには建て直しが要る。**`Target = current` は要らない** ——
  /// もう着いている
  let needsRestart (current: int) (s: Seek) = s.Target >= 0 && s.Target < current

  /// この 1 フレームで進める。**予算を使い切るか、着いたら止まる。**
  ///
  /// 返すのは「進めた回数」と、次のフレームへ持ち越す状態。着いたら `idle`。
  ///
  /// `tick` と `elapsedMs` は呼ぶ側が 1 個 だけ作って持ち回ること
  /// （`Pacing.step` と同じ理由 —— 毎フレーム作るとヒープに乗る）。
  /// `elapsedMs` は**このフレームが始まってからの ms**。
  ///
  /// **予算を測るのは 1 コマ 進めたあと。** 手前で測ると、予算が 0 のときに
  /// 1 コマ も進まないまま毎フレーム戻ってきて、いつまでも着かない
  let step (current: int) (tick: unit -> unit) (elapsedMs: unit -> float) (s: Seek) : struct (int * Seek) =
    if s.Target < 0 || current >= s.Target then struct (0, (if s.Target < 0 then s else idle))
    else
      let mutable n = 0
      let mutable go = true
      while go do
        tick ()
        n <- n + 1
        if current + n >= s.Target then go <- false
        elif elapsedMs () >= BudgetMs then go <- false
      struct (n, (if current + n >= s.Target then idle else s))
