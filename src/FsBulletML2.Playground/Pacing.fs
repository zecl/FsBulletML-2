namespace FsBulletML2.Playground

/// **1 フレームに Tick を何回 呼ぶか**だけを決める。
///
/// 弾幕も描画も Bolero も知らない。だから `Front.Tests` が `Link` で借りて
/// .NET でそのまま当てられる —— **本番と同じ 1 本 を通す**（`Main.fs` は
/// これを呼ぶだけで、進め方の判断を持たない）。
///
/// --- 補間しない
///
/// 倍速は「1 フレームに N 回 進める」であって、「1 回 進めて N 倍 の距離を
/// 動かす」ではない。後者は弾の軌跡そのものが変わる。
///
/// --- 整数倍だけ
///
/// 1.5 倍 は「3 フレームに 2 回」のような形になり、コマの粒が不均一になる。
///
/// --- 自機の位置は 1 フレームに 1 回 しか変わらない
///
/// 倍速のときは、N 回 の Tick が**同じ自機の位置**を見る。等速で N フレーム
/// 進めたものとは、自機が動いていれば別のものになる。**これは仕様** ——
/// 直すには自機の位置を Tick ごとに補間することになり、上の「補間しない」に反する。
[<Struct>]
type Pacing =
  { /// 正なら倍速（1 フレームに n 回）、負ならスロー（-n フレームに 1 回）。
    /// **0 は無い** —— 止めるのは Pause の仕事
    Rate: int
    /// スローの残り。倍速のときは使わない
    Left: int }

module Pacing =

  /// **上限を置く。** 1 フレームで回す回数が、そのままブラウザの止まる時間になる。
  /// `SetRate` は JSInvokable なので、UI に無い値も来うる
  [<Literal>]
  let Max = 8

  let normal = { Rate = 1; Left = 0 }

  /// 0 は等速に倒し、上限で丸める。**残りは持ち越さない** ——
  /// 速さを変えた次のフレームから効く
  let withRate (n: int) =
    let n = if n = 0 then 1 else max -Max (min Max n)
    { Rate = n; Left = 0 }

  /// この 1 フレームで Tick を呼ぶ回数と、次のフレームへ持ち越す状態。
  ///
  /// **確保しない。** 弾 1 個 ごとではなく 1 フレーム ごとだが、
  /// ここは毎コマ必ず通るので struct のまま返す
  let advance (p: Pacing) : struct (int * Pacing) =
    if p.Rate > 0 then struct (p.Rate, p)
    else
      let left = p.Left - 1
      if left <= 0 then struct (1, { p with Left = -p.Rate })
      else struct (0, { p with Left = left })

  /// 1 フレーム ぶん進める。**数えるのも呼ぶのもここ。**
  ///
  /// 呼ぶ側（`Main.fs`）に `for` を書くと、あのファイルは Bolero を参照するので
  /// **その `for` だけが門の外に出る**（回数が合っていても呼んでいない、が通る）。
  ///
  /// `tick` は呼ぶ側が 1 個 だけ作って持ち回ること。毎コマ作ると、
  /// 1 フレーム につき 1 個 の閉包がヒープに乗る
  let step (tick: unit -> unit) (p: Pacing) : Pacing =
    let struct (n, next) = advance p
    for _ in 1 .. n do tick ()
    next
