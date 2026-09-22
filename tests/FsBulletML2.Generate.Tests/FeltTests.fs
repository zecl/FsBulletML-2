module FsBulletML2.Generate.Tests.FeltTests

open NUnit.Framework
open FsUnit
open FsBulletML2.Generate

module Dsl = FsBulletML2.Dsl

[<TestFixture>]
type FeltTests() =

  static let twoPi = 2.0 * System.Math.PI

  static let shot (heads: float list) (speeds: float list) : Felt.Snapshot =
    { Felt.Frame = 1; Felt.Positions = []; Felt.Born = []; Felt.Headings = heads; Felt.Speeds = speeds }

  static let cohort (frame: int) (born: int) (pts: (float * float) list) : Felt.Snapshot =
    { Felt.Frame = frame; Felt.Positions = pts; Felt.Born = List.replicate pts.Length born
      Felt.Headings = []; Felt.Speeds = [] }

  /// n 本 を 0..2π に等間隔。ビン の境 に乗らない よう 半刻み ずらす
  static let even (n: int) =
    [ for i in 0 .. n - 1 -> (float i + 0.5) * twoPi / float n ]

  static let circle (n: int) (r: float) =
    [ for i in 0 .. n - 1 ->
        let t = float i * twoPi / float n
        240.0 + r * sin t, 300.0 + r * cos t ]

  static let spiral () =
    PatternSpec.create (fun a ->
      { a with Kind = Spiral; Speed = 1.0; Density = 1.0; Symmetry = 2.0; Vanishing = true })

  /// 型 を見比べる 素。乱れ を足す と 幕 が雨 に戻る ので 0
  static let namedWays ways kind facing =
    PatternSpec.create (fun a ->
      { a with
          Kind = kind
          Speed = 1.0; Density = 1.5; Symmetry = 2.0
          Jitter = 0.0; Layers = 0.0; Cascade = 0.0; Depth = 0.0
          Vanishing = true; Aiming = false
          Facing = facing; Motion = Plain; Ways = ways })

  static let named kind facing = namedWays 8 kind facing

  static let runOf frames kind facing = Felt.run frames (Generate.generate (named kind facing)).Bulletml

  static let deg (d: float) = d * System.Math.PI / 180.0

  /// 0.5 度 以内 に在る 発射角 の数
  static let near (target: float) (hs: float list) =
    hs |> List.filter (fun h -> abs (System.Math.IEEERemainder(h - target, twoPi)) <= deg 0.5) |> List.length

  /// 発射角 の円 平均
  static let centerOf (s: Felt.Snapshot) =
    atan2 (s.Headings |> List.averageBy sin) (s.Headings |> List.averageBy cos)

  /// いちばん 多く 撃った コマ（先 に来た もの）
  static let fullest (snaps: Felt.Snapshot list) = snaps |> List.maxBy (fun s -> s.Headings.Length)

  /// 帯 は 群 が 8 コマ 飛んだ あと にしか 測れない ので、全 コマ の最大 を採る
  static let bestBand (snaps: Felt.Snapshot list) = snaps |> List.map Felt.bandScore |> List.max

  /// 0.5 度 以内 の発射角 は 1 本 に数える（0 と 2π の継ぎ目 も跨ぐ）
  static let distinct (hs: float list) =
    let tol = deg 0.5
    let a = hs |> List.sort |> Array.ofList
    if a.Length = 0 then 0
    else
      let inner = a |> Array.pairwise |> Array.filter (fun (p, q) -> q - p > tol) |> Array.length
      let seam = if a.[0] + twoPi - a.[a.Length - 1] > tol then 1 else 0
      max 1 (inner + seam)

  /// すべて の発射角 を含む 最小 の弧 の幅（度）
  static let arcDeg (hs: float list) =
    let a = hs |> List.sort |> Array.ofList
    let gaps = Array.append [| a.[0] + twoPi - a.[a.Length - 1] |] (a |> Array.pairwise |> Array.map (fun (p, q) -> q - p))
    (twoPi - Array.max gaps) * 180.0 / System.Math.PI

  [<Test>]
  member _.``60 コマ 走せて弾が居る``() =
    let spec =
      PatternSpec.create (fun a ->
        { a with Kind = Spiral; Speed = 1.0; Density = 1.0; Symmetry = 2.0; Vanishing = true })
    let snaps = Felt.run 60 (Generate.generate spec).Bulletml
    snaps |> List.exists (fun s -> s.Positions.Length > 0) |> should equal true

  /// 1 コマ目 は根 が撃つ だけ で、撃たれた 弾 は まだ 回って いない。根 を数える と 1 になる
  [<Test>]
  member _.``根 は Positions に入らない``() =
    let first = Felt.run 1 (Generate.generate (spiral ())).Bulletml |> List.head
    first.Headings |> should not' (be Empty)
    first.Positions |> should be Empty

  [<Test>]
  member _.``Born は Positions と同じ 長さ で、面 の外 は間引く``() =
    let snaps = Felt.run 120 (Generate.generate (spiral ())).Bulletml
    for s in snaps do
      s.Born.Length |> should equal s.Positions.Length
      for (x, y) in s.Positions do
        (x >= 0.0 && x <= 480.0 && y >= 0.0 && y <= 640.0) |> should equal true
      for b in s.Born do
        (b >= 1 && b < s.Frame) |> should equal true

  /// 渦 の sequence は 波 を跨いで 増え続ける
  [<Test>]
  member _.``発射角 は 0..2π に畳む``() =
    let heads = Felt.run 90 (Generate.generate (spiral ())).Bulletml |> List.collect (fun s -> s.Headings)
    heads |> should not' (be Empty)
    for h in heads do
      (h >= 0.0 && h < twoPi) |> should equal true

  [<Test>]
  member _.``等間隔 の発射角 は ring が高く fan が低い``() =
    let s = shot (even 12) []
    Felt.ringScore s |> should be (greaterThan 0.9)
    Felt.fanScore s |> should be (lessThan 0.1)

  [<Test>]
  member _.``狭い 弧 は fan が高く ring が低い``() =
    let s = shot [ for i in 0 .. 8 -> System.Math.PI - 0.2 + float i * 0.05 ] []
    Felt.fanScore s |> should be (greaterThan 0.9)
    Felt.ringScore s |> should be (lessThan 0.2)

  [<Test>]
  member _.``完全 な 3・4・8 方向 は ring が高い``() =
    for n in [ 3; 4; 8 ] do
      Felt.ringScore (shot [ for i in 0 .. n - 1 -> float i * twoPi / float n ] []) |> should be (greaterThan 0.9)

  [<Test>]
  member _.``90 度 の扇 は ring が低い``() =
    Felt.ringScore (shot [ for i in 0 .. 8 -> float i * System.Math.PI / 16.0 ] []) |> should be (lessThan 0.3)

  [<Test>]
  member _.``1 本 だけ なら ring は 0``() =
    Felt.ringScore (shot [ 1.0 ] []) |> should equal 0.0

  /// 半周 を越える 弧 は 扇 と呼ばない
  [<Test>]
  member _.``半周 を越える 弧 は fan が 0``() =
    let s = shot [ for i in 0 .. 20 -> float i * 0.2 ] []
    Felt.fanScore s |> should equal 0.0

  [<Test>]
  member _.``横 一列 の群 は band が高く、輪 の群 は低い``() =
    let line = cohort 20 5 [ for i in 0 .. 9 -> 100.0 + float i * 30.0, 300.0 ]
    Felt.bandScore line |> should be (greaterThan 0.9)
    Felt.bandScore (cohort 20 5 (circle 12 60.0)) |> should be (lessThan 0.1)

  [<Test>]
  member _.``band は 生まれて 8 コマ 未満 の群 と 8 発 未満 の群 を見ない``() =
    let line n = [ for i in 0 .. n - 1 -> 100.0 + float i * 30.0, 300.0 ]
    Felt.bandScore (cohort 12 5 (line 10)) |> should equal 0.0
    Felt.bandScore (cohort 13 5 (line 10)) |> should be (greaterThan 0.9)
    Felt.bandScore (cohort 20 5 (line 7)) |> should equal 0.0

  /// 群 を混ぜる と 波 ごと の縦 のずれ が var(y) に入って 帯 が消える
  [<Test>]
  member _.``band は 群 を混ぜない``() =
    let rows =
      [ for b in 1 .. 5 do
          for i in 0 .. 9 -> (100.0 + float i * 30.0, 100.0 + float b * 80.0), b ]
    let s : Felt.Snapshot =
      { Felt.Frame = 20; Felt.Positions = List.map fst rows; Felt.Born = List.map snd rows
        Felt.Headings = []; Felt.Speeds = [] }
    Felt.bandScore s |> should be (greaterThan 0.9)

  [<Test>]
  member _.``狙い の方位 に揃う と aim が高く、真裏 は 0``() =
    Felt.aimScore 240.0 600.0 (shot (List.replicate 5 System.Math.PI) []) |> should be (greaterThan 0.99)
    Felt.aimScore 240.0 600.0 (shot (List.replicate 5 0.0) []) |> should equal 0.0

  [<Test>]
  member _.``中央値 が同じ 向き に進む と rotation が 1、止まる と 0``() =
    let waves (step: float) =
      [ for w in 0 .. 9 ->
          let c = float w * step
          { (shot [ c; c + 0.1; c + 0.2 ] []) with Felt.Frame = w * 4 + 1 } ]
    Felt.rotationScore (waves 0.1) |> should equal 1.0
    Felt.rotationScore (waves 0.0) |> should equal 0.0

  /// 6.2 から 0.1 へ は 最短 で +0.18 rad。素 の差 だと -6.1 で 逆回り に数える
  [<Test>]
  member _.``rotation は 2π を跨いでも 同じ 向き``() =
    let snaps =
      [ for w in 0 .. 9 ->
          let h = (5.9 + float w * 0.15) % twoPi
          { (shot [ h ] []) with Felt.Frame = w + 1 } ]
    Felt.rotationScore snaps |> should equal 1.0

  [<Test>]
  member _.``rotation は 撃って いない コマ を飛ばす``() =
    let snaps =
      [ for f in 1 .. 20 ->
          let heads = if f % 4 = 1 then [ float f * 0.05 ] else []
          { (shot heads []) with Felt.Frame = f } ]
    Felt.rotationScore snaps |> should equal 1.0

  [<Test>]
  member _.``速さ が 1 + sin 5θ なら fold 5 が高く fold 3 が低い``() =
    let heads = even 36
    let s = shot heads (heads |> List.map (fun t -> 1.0 + sin (5.0 * t)))
    Felt.foldScore 5 s |> should be (greaterThan 0.99)
    Felt.foldScore 3 s |> should be (lessThan 0.05)

  /// 分散 0 は 0/0。NaN は どの 比較 も偽 なので、振幅 0 の門 が 間違った 理由 で赤 になる
  [<Test>]
  member _.``速さ が一定 なら fold は 0``() =
    let heads = even 36
    Felt.foldScore 5 (shot heads (List.replicate 36 2.0)) |> should equal 0.0
    Felt.recipScore 5 (shot heads (List.replicate 36 2.0)) |> should equal 0.0

  /// 逆数 が正弦 の形。星 の式 `r0 sqrt(1-α²) / (1 + α cos kθ)` を 素 で置いた もの
  [<Test>]
  member _.``速さ が 1 除 1 + 0.85 cos 5θ なら recip 5 だけ が高い``() =
    let heads = even 36
    let s = shot heads (heads |> List.map (fun t -> 1.0 / (1.0 + 0.85 * cos (5.0 * t))))
    Felt.recipScore 5 s |> should be (greaterThan 0.99)
    Felt.foldScore 5 s |> should be (lessThan 0.8)

  /// ほぼ 0 の速さ は 逆数 が 巨大 に なって 当てはまり を 潰す
  /// ちょうど 0 は `IsFinite` の ふるい が 落とす ので、見張り を外して も 緑 の まま（門 に ならない）
  [<Test>]
  member _.``recip は ほぼ 0 の弾 を 数 から 落とす``() =
    let heads = even 36
    let speeds = heads |> List.mapi (fun i t -> if i < 3 then 1e-9 else 1.0 / (1.0 + 0.85 * cos (5.0 * t)))
    Felt.recipScore 5 (shot heads speeds) |> should be (greaterThan 0.99)

  [<Test>]
  member _.``渦 は回る``() =
    Felt.rotationScore (runOf 180 Spiral Around) |> should be (greaterThan 0.6)

  /// 幕 は自機 のほう へ降りる ので aim も高い。それ は正しい ので門 にしない
  [<Test>]
  member _.``幕 は帯 で、放射 ではない``() =
    let snaps = runOf 90 Curtain Around
    bestBand snaps |> should be (greaterThan 0.6)
    Felt.ringScore (fullest snaps) |> should be (lessThan 0.5)

  /// 渦 も ring は高い。分ける のは 回転 だけ
  [<Test>]
  member _.``放射 は全周 で、幕 ではない``() =
    let snaps = runOf 90 Radial Around
    Felt.ringScore (fullest snaps) |> should be (greaterThan 0.6)
    bestBand snaps |> should be (lessThan 0.5)
    Felt.rotationScore snaps |> should be (lessThan 0.5)
    // 頭 と最後 の腕 が同じ 角 に重なる と 9 発 で 8 方向 になる
    let s = fullest snaps
    s.Headings.Length |> should equal 8
    distinct s.Headings |> should equal 8

  /// 全腕 が 1 つ の角 に重なって いても fan は高い。本数 と開き で縮退 を落とす
  [<Test>]
  member _.``扇 は弧 に集まり、全周 ではない``() =
    let s = fullest (runOf 90 Spread Backward)
    Felt.fanScore s |> should be (greaterThan 0.6)
    Felt.ringScore s |> should be (lessThan 0.5)
    distinct s.Headings |> should be (greaterThanOrEqualTo 8)
    arcDeg s.Headings |> should be (greaterThanOrEqualTo 30.0)
    s.Headings.Length |> should equal 8

  /// 名指し も狙い も無い 扇 は 下（自機 の側）へ開く。撃つ側 の向き は 敵 なら 0（真上）
  [<Test>]
  member _.``扇 は 名指し が無ければ 下 へ開く``() =
    let c = centerOf (fullest (runOf 90 Spread Around))
    abs (System.Math.IEEERemainder(c - System.Math.PI, twoPi)) |> should be (lessThanOrEqualTo (deg 0.5))

  /// `$rand` が 0.5 固定 だと 腕 が全部 同じ 角 に落ちて aim だけ 高く出る
  [<Test>]
  member _.``狙い は自機 方位 に寄る``() =
    let s = fullest (runOf 90 Aimed Around)
    Felt.aimScore 240.0 600.0 s |> should be (greaterThan 0.6)
    distinct s.Headings |> should be (greaterThanOrEqualTo 3)
    // 3way は 3 本 で、真ん中 が自機 の方位（敵 の真下 = π）
    let three = fullest (Felt.run 90 (Generate.generate (namedWays 3 Aimed Around)).Bulletml)
    three.Headings.Length |> should equal 3
    distinct three.Headings |> should equal 3
    near System.Math.PI three.Headings |> should equal 1

  /// 自機 が真下 に居る と 幕 も狙い も 下向き の弧 で、aim では割れない。
  /// 自機 を (240, 600) から (60, 600) へ動かした とき の 発射 の中心角 の ずれ で見る。敵 (240, 80) から 見て atan(180 / 520) = 19.1 度
  [<Test>]
  member _.``狙い は自機 を追い、幕 は追わない``() =
    let centerAt px kind = centerOf (fullest (Felt.runWith px 600.0 90 (Generate.generate (named kind Around)).Bulletml))
    let shift kind = abs (System.Math.IEEERemainder(centerAt 60.0 kind - centerAt 240.0 kind, twoPi))
    abs (shift Aimed - atan (180.0 / 520.0)) |> should be (lessThanOrEqualTo (deg 1.0))
    shift Curtain |> should be (lessThanOrEqualTo (deg 0.5))
    shift Spread |> should be (lessThanOrEqualTo (deg 0.5))

  /// 難度 を 下げる と 撃つ 数 が 減る。`$rank` を 読む 木 で 見る —— 読まない 木 では 同じ
  [<Test>]
  member _.``runAt は 難度 を 渡す``() =
    let b =
      Dsl.vertical "rank" {
        Dsl.top {
          Dsl.repeat "1 + 4 * $rank" {
            Dsl.fire { absolute "180"; speed "1"; plain }
          }
          // 終えた 台本 は 頭 から 走り 直す ので、止めない と コマ 数 だけ 撃つ
          Dsl.wait "9999"
        }
      }
    let shots rank = Felt.runAt rank 3 b |> List.sumBy (fun s -> s.Headings.Length)
    shots 0.0f |> should equal 1
    shots 1.0f |> should equal 5
    (Felt.run 3 b |> List.sumBy (fun s -> s.Headings.Length)) |> should equal 5
