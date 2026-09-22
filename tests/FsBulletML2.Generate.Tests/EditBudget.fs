module FsBulletML2.Generate.Tests.EditBudget

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// `Edit` を通した あと、弾数 が どれだけ 増える か。`Edit` の層 には 約束 が無い こと を 数 で 固定 する
/// 床 は 300 コマ の 最大 同時数 の実測 の 最大 に 3 割 の余白（推測 で置かない）
[<TestFixture>]
type EditBudget() =

  static let peak (b: Bulletml) =
    Felt.run 300 b |> List.map (fun s -> s.Positions.Length) |> List.max

  /// 素 の生成 は 上界 を ほぼ 守る。`Edit` を当てる 前 の 足場
  static let plain (kind: PatternKind) =
    PatternSpec.create (fun a ->
      { a with Kind = kind; Speed = 1.0; Density = 1.5; Symmetry = 1.5; Layers = 1.0 })

  /// 段 は 葉 を 4 発 に割って 親 を消す
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

  /// 段 を 2 つ 足して も 天井 は 在る。`MAX_SPLIT` が 3 で頭打ち にする
  [<Test>]
  member _.``段 を 2 つ 足して も 弾数 が 32 倍 を超えない``() =
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let tree = (Generate.generate (plain kind)).Bulletml
      let bare = peak tree
      let grown = peak (Tune.applySteps [ "split", 2 ] tree)
      grown |> should be (greaterThan bare)
      float grown / float bare |> should be (lessThan 32.0)

  /// 層 は 木 を変える が 同時数 を増やさない。増やす ように なったら ここ が 赤 に なる
  [<Test>]
  member _.``層 は 同時数 を増やさない``() =
    let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 2 b
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let tree = (Generate.generate (plain kind)).Bulletml
      let layered = Tune.applySteps [ "layer", 1 ] tree
      // 空振り では ない こと を 字 で見る。同時数 だけ 見る と 素通り する
      xml layered |> should not' (equal (xml tree))
      float (peak layered) / float (peak tree) |> should be (lessThan 1.5)
