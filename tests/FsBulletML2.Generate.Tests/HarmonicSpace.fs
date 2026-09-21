namespace FsBulletML2.Generate.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 種 18 通り が「ダメ」に 落ちて いない こと。美しい は 測れない ので、弾く のは ダメ の側 だけ。
///
/// 閾値 と、決めた とき の 実測 の 幅（速さ と 深さ を 4 通り 振った 18 x 4 = 72 通り）。
/// 良い 側 が 全部 通る いちばん 厳しい 値 から、ゆるい ほう へ 2 割
///
///   外周 の 山     実測 5 以上             ->  2 以上（1 は 真円）
///   外/内          実測 2.02 .. 4.91       ->  1.3 .. 6.0
///   標本           実測 8 .. 15            ->  Arms / Folds2 >= 4
///   床 / 天井      実測 0                  ->  0（潰れた 弾 が 1 発 も 無い）
///   規則           実測 0.680 以上         ->  0.55 以上
///   字             実測 最大 27,801 バイト ->  40,000 未満（札 x 枚数 x 種 216 通り）
///   上界           実測 最大 898           ->  MAX_ALIVE 以下（fit 後）
///
/// 外/内 の 上限 は 4 に しない。第 2 の波 の 無い 深い 花（Speed 1 / Amplitude 2）が
/// 元 から 4.9 あり、4 で 切る と いま の 花 を 弾く。
/// 余白（一面 に 埋まる）は 測って いない —— 72 通り 全部 が 空き 6 割 超 で、落ちる 例 が 無く 較正 できなかった。
/// 床 / 天井 を 「3 割 未満」に して いた ときは、足す 式 に 戻して クリップ が 出て も 3 割 に 届かず 素通り した
///
/// --- 較正（当てた変異 と、赤 になった 行）
///
///   Arms を 層 で 割る に 戻す                標本
///   a1 を a に して 足す 式 に戻す            床・外/内 の 上限・規則
///   r0 の 傾き を 0.5 から 1.0 に             天井
///   振幅 を 0 に（真円）                      山（外/内 の 下限 は その 後ろ で 届かない）
///   種 の 重ね の 位相 0.5 を 0.37 に          規則
///   Arms を 対称 の 倍数 に 丸めない           字
[<TestFixture>]
type HarmonicSpace() =

  /// 速さ と 深さ。床 と 天井 は 速くて 深い 札 で 初めて 当たる 材料 が 出る
  static let FEEL = [ 2.0, 1.2; 3.0, 2.0; 1.0, 2.0; 0.0, 1.2 ]

  static let specOf (spd: float) (amp: float) (seed: int) =
    HarmonicSpec.create (fun a -> { a with Folds = 5; Speed = spd; Amplitude = amp; Seed = seed })

  /// 撃った コマ の 向き と 速さ で 測る。Positions は 上 に飛んだ 弾 が 80 px で 間引かれる
  static let waveOf (h: HarmonicSpec) =
    Harmonic.generate h |> fun i -> Felt.run 90 i.Bulletml |> Felt.Draw.fullest

  /// 向き ごと に いちばん 速い 弾 だけ 残した 外周。
  /// 層 を 重ねた 輪郭 に そのまま 正弦 を 当てる と、半周期 ずらした 層 が 第 1 の波 を
  /// ちょうど 裏返して 打ち消し合い 0 に なる（層 1 枚 ずつ は 規則的 なのに）
  static let envelope (s: Felt.Snapshot) : Felt.Snapshot =
    let hs, vs =
      List.zip s.Headings s.Speeds
      |> List.groupBy (fun (th, _) -> System.Math.Round(th, 4))
      |> List.map (fun (_, g) -> g |> List.maxBy snd)
      |> List.unzip
    { s with Headings = hs; Speeds = vs }

  /// 角 の順 に並べた 速さ の 山 の数。輪 なので 頭 と尻 を繋ぐ
  static let peaks (s: Felt.Snapshot) =
    let v = List.zip s.Headings s.Speeds |> List.sortBy fst |> List.map snd |> Array.ofList
    let n = v.Length
    [ 0 .. n - 1 ]
    |> List.filter (fun i -> v.[i] > v.[(i + n - 1) % n] && v.[i] >= v.[(i + 1) % n])
    |> List.length

  static let ratio (s: Felt.Snapshot) = List.max s.Speeds / List.min s.Speeds

  /// 外周 に 当てる 回数 は 第 1 の波・第 2 の波・層 の 数 倍 の うち 最大
  static let regularity (h: HarmonicSpec) (s: Felt.Snapshot) =
    let e = envelope s
    [ h.Folds; h.Folds2; h.Folds * h.Layers; h.Folds2 * h.Layers ]
    |> List.filter (fun k -> k > 0)
    |> List.map (fun k -> Felt.fitScore k id e)
    |> List.max

  /// Felt は $rank = 1 で走る（Felt.fs の runWith）ので、返る 速さ は 基準値 の 1.3 倍。
  /// 基準値 の まま 書く と 床 は 1 発 も 当たらず、天井 は 健全 な 弾 まで 拾う
  static let FLOOR = 0.3 * 1.3
  static let CEIL = float Consts.MAX_SPEED

  static let share (pred: float -> bool) (s: Felt.Snapshot) =
    float (s.Speeds |> List.filter pred |> List.length) / float s.Speeds.Length

  [<Test>]
  [<TestCase(1)>] [<TestCase(2)>] [<TestCase(3)>] [<TestCase(4)>] [<TestCase(5)>] [<TestCase(6)>]
  [<TestCase(7)>] [<TestCase(8)>] [<TestCase(9)>] [<TestCase(10)>] [<TestCase(11)>] [<TestCase(12)>]
  [<TestCase(13)>] [<TestCase(14)>] [<TestCase(15)>] [<TestCase(16)>] [<TestCase(17)>] [<TestCase(18)>]
  member _.``種 が ダメ に 落ちて いない``(seed: int) =
    for spd, amp in FEEL do
      let h = specOf spd amp seed
      let w = waveOf h
      // 潰れ を 先 に 見る。潰れる と 外/内 も 規則 も 崩れる ので、形 を 先 に 見る と
      // そちら が 先 に 落ちて 床 と 天井 の 行 に 届かない（足す 式 で 踏んだ）
      if h.Folds2 > 0 then
        float h.Arms / float h.Folds2 |> should be (greaterThanOrEqualTo 4.0)
      share (fun v -> v <= FLOOR + 0.01) w |> should equal 0.0
      share (fun v -> v >= CEIL - 0.01) w |> should equal 0.0
      peaks (envelope w) |> should be (greaterThanOrEqualTo 2)
      ratio w |> should be (greaterThanOrEqualTo 1.3)
      ratio w |> should be (lessThanOrEqualTo 6.0)
      regularity h w |> should be (greaterThanOrEqualTo 0.55)

  [<Test>]
  member _.``札 x 枚数 x 種 の 字 と 上界 が 予算 に 収まる``() =
    // 畳めない 形 で 層 を 重ねる と 字 が 層 の 数 だけ 膨らむ（丸める 前 は k = 7 と
    // ハート が 3 枚 で 82,842 バイト）
    for fig in [ Petal; Star; Heart ] do
      for k in [ 3; 5; 7; 8 ] do
        for seed in 1 .. 18 do
          let h =
            HarmonicSpec.create (fun a -> { a with Figure = fig; Folds = k; Speed = 2.0; Amplitude = 1.2; Seed = seed })
          (BulletmlWriter.toIndentedXml 2 (Harmonic.generate h).Bulletml).Length |> should be (lessThan 40000)
          Harmonic.aliveBound (Harmonic.fit h) |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE))

  [<Test>]
  member _.``重い 種 でも 同時 に MAX_ALIVE 以下``() =
    // 上界 は 見積もり。走らせて 数えた 同時数 も 見る —— 重ね 3 枚 と 入れ子 3 枚 が いちばん 重い
    for seed in [ 10; 13 ] do
      let alive =
        Harmonic.generate (specOf 3.0 2.0 seed) |> fun i -> Felt.run 400 i.Bulletml
        |> List.map (fun s -> s.Positions.Length)
        |> List.max
      alive |> should be (greaterThan 0)
      alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)
