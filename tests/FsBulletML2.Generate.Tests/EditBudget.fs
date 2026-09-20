module FsBulletML2.Generate.Tests.EditBudget

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// `Edit` を通した あと、弾数 が どれだけ 増える か。
///
/// `Consts.MAX_ALIVE`（900）は `Generate` が組んだ 木 についての 約束 で、
/// `Edit` の層 は 誰 も 約束 して いない —— **約束 が無い こと を 数 で 固定 する。**
///
/// ここ を 門 に する のは、増えて も 気づかない から。
/// 面 の側 は 別 の天井（DanmakuLab の `FIELD_ALIVE`）で 受ける ので、
/// その数 を決め直す とき に この 倍率 が 材料 になる。
///
/// 300 コマ の 最大 同時数 を 5 つ の型 で測った 実測:
///
///            素     段 1     倍     段 2     倍    層 1    倍
///   Spiral   427    2890   6.77   10204  23.90     427   1.00
///   Radial   400    2775   6.94    9893  24.73     400   1.00
///   Aimed    620    2881   4.65   10597  17.09     620   1.00
///   Spread   562    2872   5.11   10535  18.75     562   1.00
///   Curtain  623    2993   4.80   11100  17.82     623   1.00
///
/// 床 は 実測 の 最大 に 3 割 の余白。**推測 で置かない** ——
/// 6 と 20 で書いたら 6.77 と 23.90 で 落ちた。
///
/// 層 は 木 を変える（字 が 1,476 から 2,197 へ）のに 同時数 が 1.00 倍。
/// 空振り ではない こと は 字 の長さ で確かめた。なぜ 増えない か は 測って いない
///
/// --- 較正（当てた変異 と、赤くなった点）
///
///   `splitBranch` の `times` を 4 から 8 に     段 1 つ ／ 段 2 つ
///   `applySteps` を 素通し に                   3 点 とも
///   `layer` を `axis` から 外す                 層 は 同時数 を増やさない
///
/// `MAX_SPLIT` を 3 から 5 に上げて も 赤 に ならない —— `split` は 2 まで しか
/// 来ない（jev の `steps` が 0..2）ので、**深さ 3 の頭打ち に 当たる 材料 が無い**。
///
/// `MAX_SPLIT` を 3 から 1 に下げる のも 赤 に ならない。倍率 が **減る** 向き なので、
/// 上限 だけ 見る 門 は 通す
[<TestFixture>]
type EditBudget() =

  static let peak (b: Bulletml) =
    Felt.run 300 b |> List.map (fun s -> s.Positions.Length) |> List.max

  /// 素 の生成 は 上界 を ほぼ 守る。`Edit` を当てる 前 の 足場
  static let plain (kind: PatternKind) =
    PatternSpec.create (fun a ->
      { a with Kind = kind; Speed = 1.0; Density = 1.5; Symmetry = 1.5; Layers = 1.0 })

  /// 段 は 葉 を 4 発 に割って 親 を消す。実測 4.65〜6.94 倍
  [<Test>]
  member _.``段 を 1 つ 足す と 弾数 が 9 倍 を超えない``() =
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let tree = (Generate.generate (plain kind)).Bulletml
      let bare = peak tree
      let grown = peak (Tune.applySteps [ "split", 1 ] tree)
      bare |> should be (greaterThan 0)
      // 割った 先 が 1 発 も増えて いない なら、段 が当たって いない
      grown |> should be (greaterThan bare)
      float grown / float bare |> should be (lessThan 9.0)

  /// 段 を 2 つ 足して も 天井 は 在る。`MAX_SPLIT` が 3 で頭打ち にする。実測 17.09〜24.73 倍
  [<Test>]
  member _.``段 を 2 つ 足して も 弾数 が 32 倍 を超えない``() =
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let tree = (Generate.generate (plain kind)).Bulletml
      let bare = peak tree
      let grown = peak (Tune.applySteps [ "split", 2 ] tree)
      grown |> should be (greaterThan bare)
      float grown / float bare |> should be (lessThan 32.0)

  /// 層 は 木 を変える が 同時数 を増やさない（5 型 とも 1.00 倍）。
  /// 増やす ように なったら ここ が 赤 に なる
  [<Test>]
  member _.``層 は 同時数 を増やさない``() =
    let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 2 b
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let tree = (Generate.generate (plain kind)).Bulletml
      let layered = Tune.applySteps [ "layer", 1 ] tree
      // 空振り では ない こと を 字 で見る。同時数 だけ 見る と 素通り する
      xml layered |> should not' (equal (xml tree))
      float (peak layered) / float (peak tree) |> should be (lessThan 1.5)
