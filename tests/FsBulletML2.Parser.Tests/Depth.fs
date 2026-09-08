namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 入れ子の深さ（v2.4）。**知り方は表記ごとに違うが、答えは同じ。**
///
///     xml    開始札で増やし、閉じ札で減らす
///     sxml   括弧を読む再帰の段
///     fsb    字下げの段（**幅を決め打たない**）
///     F# CE  `{ }` の積み
///
/// --- 版の頭で測ったこと
///
/// `Stop` の意味を揃える案は**倒れた** —— 各 Scan の `contextAt` は
/// `cursor <= t.Stop` で「開始札の中に居るか」を見ていて、`Stop` が
/// 要素の終わり（子を含む）になると中身でも開始札の中と読む。
///
/// 一方、入れ子は `TagHit` だけからは組めなかった ——
///
///     xml    開始/閉じ の対応で組める（深さ 6・要素 662 個・割れ 0）
///     sxml   Start..Stop の包含で組める（176 / 176 本）
///     fsb    **組めない**（`Stop` が行末なので全部 兄弟に見える）
///     F# CE  **組めない**（名前しか返さない）
///
/// だから `Stop` は揃えず、**数のほうを持たせた。**
///
/// 同梱 176 本 を 3 表記 に通したら、(名前, 深さ) の並びが
/// **176 / 176 本 一致**（深さは最大 15 段、根は必ず 1 個）。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   xml の `Closing` で減らさない        3 表記 で深さが一致する
///   sxml の再帰に `+ 1` を渡さない       同上
///   fsb の字下げを積まない               同上
///   xml の深さを負に落とす               閉じすぎても負にしない
[<TestFixture>]
type Depth() =

  static let catalog = Bullets.Dsl.All.bullets

  static let writeAs (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> None
    | Some w -> match w.Write b with Result.Ok t -> Some t | Result.Error _ -> None

  /// 開始札だけ、(名前, 深さ) の並び。**閉じ札は XML にしか無い**
  static let shapeOf (tags: string -> TagHit list) (src: string) =
    tags src |> List.filter (fun t -> not t.Closing) |> List.map (fun t -> t.TagName, t.Depth)

  /// F# の CE も同じ形で数える。**表は語彙から**（本番と同じ 1 本）
  static let fsharpTags (src: string) =
    let v = VocabForTests.vocab
    let labels =
      v.CeLabels |> List.map (fun c -> c.Name, c.Element, c.LabelArg, c.Fixed) |> List.toArray
    let attr =
      Refs.pairs (v.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      |> List.tryHead
      |> Option.map (fun (_, _, a) -> a)
      |> Option.defaultValue ""
    FsharpScan.tags labels attr src

  static let kinds =
    [ "xml", SourceKind.Xml, (XmlScan.tags: string -> TagHit list)
      "sxml", SourceKind.Sxml, SxmlScan.tags
      "fsb", SourceKind.Fsb, FsbScan.tags ]

  /// `label` を持つ要素だけ、(名前, 深さ)。
  /// **CE はそこしか返さない**ので、比べるならこちらに揃える。
  ///
  /// はじめ「xml のうち CE にも在る名前」で絞ったら数が合わなかった ——
  /// 同じ名前が何度も出るので、名前で絞っても同じ並びにならない
  static let labeledOf (tags: string -> TagHit list) (src: string) =
    tags src
    |> List.filter (fun t ->
         not t.Closing && t.Attrs |> List.exists (fun a -> a.AttrName = "label"))
    |> List.map (fun t -> t.TagName, t.Depth)

  // --- 3 表記 の突き合わせ ----------------------------------------------------

  [<Test>]
  member _.``3 表記 で名前と深さの並びが一致する``() =
    // **知り方が 3 通り でも答えは 1 つ。** ここが割れたら、
    // アウトラインが表記を切り替えるたびに別の木になる
    let broken =
      catalog
      |> List.choose (fun info ->
           let per =
             kinds
             |> List.map (fun (label, kind, tags) ->
                  label, (writeAs kind info.Bulletml |> Option.map (shapeOf tags) |> Option.defaultValue []))
           match per with
           | (_, a) :: rest when rest |> List.forall (fun (_, b) -> b = a) -> None
           | _ -> Some info.Name)
    broken |> should be Empty

  [<Test>]
  member _.``深さが 1 段 でも在る``() =
    // 上の点は、**全部 0 でも「一致」で緑**になる
    let src = writeAs SourceKind.Xml (List.head catalog).Bulletml |> Option.get
    shapeOf XmlScan.tags src |> List.map snd |> List.max |> should greaterThan 0

  [<Test>]
  member _.``根は 1 つ``() =
    catalog
    |> List.choose (fun info ->
         let n =
           writeAs SourceKind.Xml info.Bulletml
           |> Option.map (fun s -> shapeOf XmlScan.tags s |> List.filter (fun (_, d) -> d = 0) |> List.length)
           |> Option.defaultValue -1
         if n = 1 then None else Some(info.Name, n))
    |> should be Empty

  [<Test>]
  member _.``F# の CE は名前の並びだけ一致する``() =
    // **4 表記 目。** あちらは `{ }` を数えるので、ほかの 3 つ と
    // 知り方がいちばん遠い —— そして**深さは一致しない**（下の点）。
    // 名前の並びは一致する
    let broken =
      catalog
      |> List.choose (fun info ->
           let xml =
             writeAs SourceKind.Xml info.Bulletml
             |> Option.map (labeledOf XmlScan.tags >> List.map fst)
             |> Option.defaultValue []
           let fs =
             writeAs SourceKind.FSharpDsl info.Bulletml
             |> Option.map (labeledOf fsharpTags >> List.map fst)
             |> Option.defaultValue []
           if fs = xml then None else Some info.Name)
    broken |> should be Empty

  [<Test>]
  member _.``F# の CE の深さは要素の入れ子と一致しない``() =
    // **測って分かったこと。** CE の `{ }` は要素と 1 対 1 ではない ——
    //
    //     refBullet "cross" [ ]      `◯◯Ref` は **`{ }` を開かない**
    //     doActs (body { ... })      要素に当たらない包みが 1 段 増える
    //
    // だから CE のアウトラインは**「CE の構造」を映す。それが正しい** ——
    // あちらで見ているのは CE の本文であって XML ではない。
    //
    // **数を固定しておく。** 書き手（`Dsl`）を変えたときにここが動いて、
    // 「揃った」のか「別の形で割れた」のかを見に行ける。
    //
    // 残る 31 本 は `◯◯Ref` も `doActs` も持たない形で、そこは一致する ——
    // **「全部 割れる」でも「全部 揃う」でもない**のがこの数の意味
    let differ =
      catalog
      |> List.filter (fun info ->
           let xml =
             writeAs SourceKind.Xml info.Bulletml
             |> Option.map (labeledOf XmlScan.tags)
             |> Option.defaultValue []
           let fs =
             writeAs SourceKind.FSharpDsl info.Bulletml
             |> Option.map (labeledOf fsharpTags)
             |> Option.defaultValue []
           fs <> xml)
      |> List.length
    differ |> should equal 145

  // --- 端 ---------------------------------------------------------------------

  [<Test>]
  member _.``閉じすぎても負にしない``() =
    // 打っている途中は、閉じ札だけが先に在る形が普通に起きる
    XmlScan.tags "</a></b><c/>" |> List.map (fun t -> t.Depth) |> List.min
    |> should be (greaterThanOrEqualTo 0)

  [<Test>]
  member _.``閉じ札は開いていた側と同じ深さ``() =
    let ts = XmlScan.tags "<a><b/></a>"
    let a = ts |> List.filter (fun t -> t.TagName = "a")
    a |> List.map (fun t -> t.Depth) |> List.distinct |> should equal [ 0 ]

  [<Test>]
  member _.``自己閉じは中を作らない``() =
    let ts = XmlScan.tags "<a><b/><c/></a>"
    ts
    |> List.filter (fun t -> not t.Closing)
    |> List.map (fun t -> t.TagName, t.Depth)
    |> should equal [ "a", 0; "b", 1; "c", 1 ]

  [<Test>]
  member _.``fsb は字下げの幅を決め打たない``() =
    // 2 でも 4 でも、同じ木になる
    let two = FsbScan.tags "a\n  b\n    c\n  d"
    let four = FsbScan.tags "a\n    b\n        c\n    d"
    let shape (ts: TagHit list) = ts |> List.map (fun t -> t.TagName, t.Depth)
    shape two |> should equal [ "a", 0; "b", 1; "c", 2; "d", 1 ]
    shape four |> should equal (shape two)
