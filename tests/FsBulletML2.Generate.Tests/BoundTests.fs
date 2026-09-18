module FsBulletML2.Generate.Tests.BoundTests

open NUnit.Framework
open FsUnit
open FsBulletML2.Generate
open FsBulletML2.Generate.Bound
open FsBulletML2.Generate.Consts
open FsBulletML2.Generate.Exprs

[<TestFixture>]
type BoundTests() =

  /// 軸 の数。9 つ の段 ＋ 5 つ の真偽
  static let AXES = 14

  /// 各軸 の 最小 と最大 の 2 値 だけ を回す（2^14 = 16,384 通り）。
  /// 掛け算 は端 で最大 になる ので、中 を回して も 上界 は破れない
  static let allExtremes () =
    let at (bits: int) (i: int) (hi: float) = if (bits >>> i) &&& 1 = 0 then 0.0 else hi
    let flag (bits: int) (i: int) = (bits >>> i) &&& 1 = 1
    seq {
      for bits in 0 .. (1 <<< AXES) - 1 ->
        PatternSpec.create Spiral
          (at bits 0 3.0)   // speed
          (at bits 1 3.0)   // density
          (at bits 2 3.0)   // symmetry
          (at bits 3 2.0)   // layers
          (at bits 4 2.0)   // jitter
          (at bits 5 2.0)   // rhythm
          (at bits 6 2.0)   // depth
          (at bits 7 2.0)   // bulletKinds
          (at bits 8 3.0)   // cascade
          (flag bits 9)     // breathe
          (flag bits 10)    // vanishing
          (flag bits 11)    // aiming
          (flag bits 12)    // pause
          (flag bits 13)    // parametrized
          0.8
    }

  static let baseSpec () =
    PatternSpec.create Spiral 1.0 1.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0
                       false true false false false 0.8

  static let withAxis kind =
    match kind with
    | "cascade" -> PatternSpec.create Spiral 1.0 1.0 1.0 0.0 0.0 0.0 0.0 1.0 2.0 false true false false false 0.8
    | "layers" -> PatternSpec.create Spiral 1.0 1.0 1.0 2.0 0.0 0.0 0.0 1.0 0.0 false true false false false 0.8
    | "depth" -> PatternSpec.create Spiral 1.0 1.0 1.0 0.0 0.0 0.0 2.0 1.0 0.0 false true false false false 0.8
    | "symmetry" -> PatternSpec.create Spiral 1.0 1.0 3.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false 0.8
    | _ -> failwithf "知らない 軸: %s" kind

  /// 端 を 1 つ も 落として いない —— 畳んだ 添字 がずれる と 数 が減る
  [<Test>]
  member _.``端 の組み合わせ を全部 回す``() =
    allExtremes () |> Seq.length |> should equal 16384
    allExtremes () |> Seq.filter (fun s -> s.Cascade = 3.0) |> Seq.length |> should equal 8192
    allExtremes () |> Seq.filter (fun s -> s.Pause) |> Seq.length |> should equal 8192

  /// 軸 ごと の clamp は「1 本 の式 が暴れない」ことしか 保証 しない。実機 で確かめた:
  ///
  ///     Cascade 3 段   同時 21,517 発（MAX_ALIVE の 24 倍）
  ///     Cascade 無し   同時  1,070 発（それでも 破れる）
  [<Test>]
  member _.``fit を通せば どの 組み合わせ でも 上界 の中``() =
    let bad =
      allExtremes ()
      |> Seq.map fit
      |> Seq.filter (fun s -> aliveBound s > float MAX_ALIVE)
      |> Seq.truncate 3
      |> List.ofSeq

    bad |> List.length |> should equal 0

  /// 実機 で 1,958 -> 662 に絞った 後 も、腕 18 本 のリング も 3 層 の重なり も残った。
  /// `Cascade` だけ は最後 の手段 で落ちる（下 の 2 本 が見る）
  [<Test>]
  member _.``fit は形 を壊さない``() =
    for s in allExtremes () |> Seq.truncate 2000 do
      let f = fit s
      f.Symmetry |> should equal s.Symmetry
      f.Layers |> should equal s.Layers
      f.Depth |> should equal s.Depth
      f.BulletKinds |> should equal s.BulletKinds
      f.Kind |> should equal s.Kind

  /// 段 を落とす のは `wait` で絞りきれない とき だけ。
  /// 収まって いる 仕様 の段 を削る と、頼んだ 個性 が黙って 消える
  [<Test>]
  member _.``収まる 仕様 の段 は落とさない``() =
    let ok =
      allExtremes ()
      |> Seq.filter (fun s -> aliveBound s <= float MAX_ALIVE)
      |> Seq.truncate 3000
      |> List.ofSeq

    ok |> List.isEmpty |> should equal false
    for s in ok do
      (fit s).Cascade |> should equal s.Cascade

  /// `wait` を伸ばして も 1 回 の塊 は減らない ので、段 を落とす 経路 が要る ——
  /// 全軸 最大 は wait 278 倍 でも 同時 3,351 発 だった
  [<Test>]
  member _.``塊 が越える 仕様 では 段 が落ちる``() =
    let heavy =
      PatternSpec.create Spiral 3.0 3.0 3.0 2.0 2.0 2.0 2.0 2.0 3.0
                         true true true true true 0.8
    (fit heavy).Cascade |> should be (lessThan heavy.Cascade)

  [<Test>]
  member _.``余裕 が在れば WaitScale は 1 のまま``() =
    let s =
      PatternSpec.create Spiral 1.0 0.0 0.0 0.0 0.0 0.0 0.0 0.0 0.0
                         false true false true false 0.8
    (fit s).WaitScale |> should equal 1.0

  [<Test>]
  member _.``重い 仕様 では WaitScale が伸びる``() =
    let heavy =
      PatternSpec.create Spiral 3.0 3.0 3.0 2.0 2.0 2.0 2.0 2.0 3.0
                         true true true true true 0.8
    (fit heavy).WaitScale |> should be (greaterThan 1.0)

  // ------------------------------------------------------------------
  // `aliveBound (fit s) <= MAX_ALIVE` だけ を見る と、両側 が一緒 にずれて
  // 緑 のまま になる —— 掛け算 の項 を落とす 変異 が 4 本 とも 空振り した。
  // だから 軸 を 1 つ 上げた ら 上界 が増える を直 に見る
  // ------------------------------------------------------------------

  [<Test>]
  member _.``Cascade を上げる と 上界 が増える``() =
    aliveBound (withAxis "cascade") |> should be (greaterThan (aliveBound (baseSpec ())))

  [<Test>]
  member _.``Layers を上げる と 上界 が増える``() =
    aliveBound (withAxis "layers") |> should be (greaterThan (aliveBound (baseSpec ())))

  /// `Depth` は上界 に効かない —— 中間 `repeat` は発射 の回数 を増やす が
  /// 同じ だけ 時間 も 伸びる。掛けた とき 上界 5,100 万 発 が出た
  [<Test>]
  member _.``Depth は上界 に効かない``() =
    aliveBound (withAxis "depth") |> should (equalWithin 0.001) (aliveBound (baseSpec ()))

  [<Test>]
  member _.``Symmetry を上げる と 上界 が増える``() =
    aliveBound (withAxis "symmetry") |> should be (greaterThan (aliveBound (baseSpec ())))

  /// 値 を手 で固定 する —— 不等式 で書く と 緩すぎて、項 を 1 つ だけ
  /// `$rank = 0.5` にした 変異 が通り抜けた。
  ///
  ///     armsExpr 8 腕 ＋ 速度 の起点 1 発 = 9   waitExpr 5 間隔
  ///     Depth 0 / Cascade 0 / Layers 0 / Pause 無し
  ///     9 ÷ 5 × 180 × 1.5 = 486
  [<Test>]
  member _.``上界 は いちばん 重い側 の値 になる``() =
    evalAt 1.0 (armsExpr (baseSpec ())) |> should (equalWithin 0.001) 8.0
    evalAt 1.0 (waitExpr (baseSpec ()) 0) |> should (equalWithin 0.001) 5.0
    aliveBound (baseSpec ()) |> should (equalWithin 0.001) 486.0

  /// Core で 10 通り 走らせて 校正 した（2026-09-19、`$rank = 1.0`、種 3 通り の最大、
  /// 1,800 コマ）。ブラウザ では 背面タブ で コマ が間引かれて 数 が取れない ——
  /// 弾数 は論理値 なので 同じ Core を .NET で回す。
  ///
  ///     仕様                      予測   実測    比
  ///     a-spiral-2層-間            707    614   1.15
  ///     b-radial-parametrized      878    624   1.41
  ///     c-aimed-cascade3           883    680   1.30
  ///     d-全軸最大                  638    516   1.24
  ///     e-最小                     154    104   1.48
  ///     f-間なし-密                 630    434   1.45
  ///     g-間あり-密                  37     28   1.32
  ///     h-depth2                   486    333   1.46
  ///     i-layers2                  675    475   1.42
  ///     j-spread-breathe           569    406   1.40
  ///
  /// 素 の式（`SAFETY = 1.0`）は 0.77〜1.00 で **全部 実測 を下回って いた** ——
  /// 上界 として 破れて いる。`SAFETY` は いちばん 外す ところ の逆数 で決める
  [<Test>]
  member _.``SAFETY は いちばん 外す ところ を覆う``() =
    SAFETY |> should be (greaterThanOrEqualTo (1.0 / 0.77))
    // 大きすぎる と 収まって いる 仕様 まで 絞る
    SAFETY |> should be (lessThanOrEqualTo 2.0)
