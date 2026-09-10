namespace FsBulletML2.Playground

/// **2 つ の面が分かれるまで進める**だけを決める（v3.8）。
/// 弾幕も描画も Bolero も知らない —— `Seek` / `Pacing` と同じ理由でここに居る。
///
/// --- なぜ「分かれた最初のコマ」か
///
/// v2.0 で「同じ種なら同じ走り」を、v2.1 で「並べて見る」を作った。
/// **並べても、どこで分かれたかは目で探すしかない。**
///
/// --- 版の頭で数えた（同梱 176 本 / 上限 900 コマ）
///
///     軸        分かれる本   1 コマ目 で分かれる   分かれるコマの中央
///     種         104 / 176          0 本                6 コマ
///     難度 0/1   172 / 176         13 本               14 コマ
///
/// **1 コマ目 で分かれる本が多いなら、この道具は意味を持たない**（設計書の問い）。
/// 種では 0 本。**成立する。**
///
/// --- 数では見ない。位置で見る
///
/// 同じ 176 本 で、**弾の数が食い違うのは位置より遅れる** ——
///
///     軸        位置で分かれる   数で分かれる
///     種          中央  6 コマ    中央 134 コマ   **22.3 倍 遅れる**
///     難度 0/1    中央 14 コマ    中央  31 コマ     2.2 倍
///
/// 数だけ見ると、**位置は 6 コマ 目 から違うのに 134 コマ 目 まで気づかない。**
/// だから判定は座標まで見る（`Playfield.Differ`）。
///
/// --- 待たせない
///
/// `Seek` と同じ —— 1 フレームに使う時間を切って、着くまで何フレームかに分ける。
/// 進んでいることはコマ数の表示に出る。
[<Struct>]
type Diverge =
  { /// あと何コマ 見るか。**走っていなければ 0**
    Left: int }

module Diverge =

  /// 見るコマ数の上限。**測った最大は 437 コマ**（難度の軸）なので
  /// 900 で足りるが、人が打った弾幕はそこに収まらない
  [<Literal>]
  let Max = 3600

  /// 既定で見るコマ数。**測った最大の 2 倍 強**
  [<Literal>]
  let Default = 900

  /// 1 フレームで進めるのに使ってよい時間。**`Seek` と同じ 8 ms**
  [<Literal>]
  let BudgetMs = 8.0

  /// 走っていない。
  let idle = { Left = 0 }

  /// 何コマ 見るかを決める。**0 以下 は走らない**（`idle`）。上限で丸める。
  /// `JSInvokable` なので UI に無い値も来うる
  let start (n: int) = if n <= 0 then idle else { Left = min Max n }

  let isRunning (d: Diverge) = d.Left > 0

  /// この 1 フレームで進める。**予算を使い切るか、分かれたか、
  /// 見るコマ数を使い切ったら止まる。**
  ///
  /// 戻りは（進めた回数, 分かれたか, 次のフレームへ持ち越す状態）。
  ///
  /// `tick` / `elapsedMs` / `differs` は呼ぶ側が 1 個 だけ作って持ち回ること
  /// （`Seek.step` と同じ理由 —— 毎フレーム作るとヒープに乗る）。
  ///
  /// **`differs` は 1 コマ 進めたあとに見る。** 手前で見ると、
  /// 押した時点で既に分かれている面が **0 コマ 進んだまま**「分かれた」と出て、
  /// どのコマで分かれたのかが言えない。
  ///
  /// **予算も 1 コマ 進めたあとに測る**（`Seek.step` と同じ。予算が 0 のときに
  /// 1 コマ も進まないまま毎フレーム戻ってくるのを避ける）
  let step
    (tick: unit -> unit)
    (elapsedMs: unit -> float)
    (differs: unit -> bool)
    (d: Diverge)
    : struct (int * bool * Diverge) =
    if d.Left <= 0 then struct (0, false, idle)
    else
      let mutable n = 0
      let mutable found = false
      let mutable go = true
      while go do
        tick ()
        n <- n + 1
        if differs () then
          found <- true
          go <- false
        elif n >= d.Left then go <- false
        elif elapsedMs () >= BudgetMs then go <- false
      let left = d.Left - n
      struct (n, found, (if found || left <= 0 then idle else { Left = left }))