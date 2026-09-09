namespace FsBulletML2.Parser.Tests

open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 読んだ木のノードと札を順番で結ぶ（v3.1 の段 4）。
///
/// **木は字の位置を持たない**ので、「このノードは字のどこか」は順番でしか
/// 言えない。v2.9 で測ってある（5b / 5c。3 表記 で 176 / 176、F# の CE は
/// 結べない）が、**あれは測定であって門ではない** —— 段 4 が乗るので、
/// ここで固定する。
///
/// --- 何が壊れると赤くなるか
///
///     歩きの腕が減る          札のほうが多くなって並びがずれる
///     ノードでない名前を拾う  木より札が多くなる
///     `names` が腕とずれる    落とす名前が変わって、添字が黙ってずれる
///
/// **どれも走行は変わらない。** 光る場所がずれるだけなので、目にも出ない
/// （字は在るし、印も出る。ただ 1 つ 隣を指す）。
[<TestFixture>]
type NodeOrderTests() =

  static let catalog = Bullets.Dsl.All.bullets

  static let writeAs (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> None
    | Some w -> match w.Write b with Result.Ok t -> Some t | Result.Error _ -> None

  static let kinds =
    [ "xml", SourceKind.Xml, (XmlScan.tags: string -> TagHit list)
      "sxml", SourceKind.Sxml, SxmlScan.tags
      "fsb", SourceKind.Fsb, FsbScan.tags ]

  /// 開き札だけ、しかも**ノードになる名前だけ**。
  /// 落とす側（`times` / `param` など）を数えない —— 名前は `NodeOrder.names`
  static let openNodeTags (tags: TagHit list) =
    let ok = HashSet<string>(NodeOrder.names)
    tags |> List.filter (fun t -> not t.Closing && ok.Contains t.TagName)
  /// 並びに 2 度 出る腕を数える。戻りは (名前 -> 件数, 2 度 出た本の数)
  static let dupsOf (books: Bulletml list) =
    let names = Dictionary<string, int>()
    let mutable books2 = 0
    for b in books do
      let seen = HashSet<obj>(HashIdentity.Reference)
      let mutable any = false
      for (name, node) in NodeOrder.walk b do
        if not (seen.Add node) then
          any <- true
          names.[name] <- (match names.TryGetValue name with | true, v -> v | _ -> 0) + 1
      if any then books2 <- books2 + 1
    names, books2

  [<Test>]
  member _.``ノードになる名前は腕から引けている``() =
    // **0 件 を緑にしない。** reflection が効いていなければ空になる
    NodeOrder.names.Length |> should be (greaterThan 0)
    // 落とす側の名前が混ざっていたら、札を絞りすぎて添字がずれる
    for v in [ "times"; "direction"; "speed"; "horizontal"; "vertical"; "term"; "param" ] do
      NodeOrder.names |> should not' (contain v)
    // 歩きが出す名前は、全部 この並びの中に在る
    let ok = HashSet<string>(NodeOrder.names)
    let mutable outside = 0
    for info in catalog do
      for (name, _) in NodeOrder.walk info.Bulletml do
        if not (ok.Contains name) then outside <- outside + 1
    outside |> should equal 0

  [<Test>]
  member _.``木の k 番目 と札の k 番目 が 3 表記 で揃う``() =
    let mutable ran = 0
    for (name, kind, tagsOf) in kinds do
      let mutable same = 0
      let mutable total = 0
      for info in catalog do
        match writeAs kind info.Bulletml with
        | None -> ()
        | Some src ->
            total <- total + 1
            let fromTags = openNodeTags (tagsOf src) |> List.map (fun t -> t.TagName)
            let fromAst = NodeOrder.walk info.Bulletml |> Seq.map fst |> List.ofSeq
            if fromTags = fromAst then same <- same + 1
      // 0 件 を緑にしない
      total |> should be (greaterThan 0)
      ran <- ran + 1
      Assert.That(same, Is.EqualTo total, name + " で割れた本が在る")
    ran |> should equal 3


  [<Test>]
  member _.``並びに 2 度 出るのは vanish と bullet だけ``() =
    // **添字で引くので、参照が 2 度 出るノードは引けない。**
    // 2 通り の理由で起きる ——
    //
    //     vanish   引数なしの腕は singleton。同じ物が何度でも返る
    //     bullet   **CE の木だけ。** 同梱カタログは空の `bullet` を共有していて、
    //              XML へ書いて読み直すと別の物になる
    //
    // **どちらも段 3 の再開点には来ない**（`stop` に渡ってくるのは
    // `action` / `wait` / `repeat` の 3 腕 だけ。段 3 で 989,269 件 数えた）。
    // だから段 4 の光る先は決まる —— **決まる理由がこの 2 つ に閉じている**
    // ことを、ここで固定する。
    let ceNames, ceBooks = dupsOf (catalog |> List.map (fun i -> i.Bulletml))
    // **0 件 を緑にしない。** 重なりが 1 件 も無ければ、この試験は何も見ていない
    ceBooks |> should be (greaterThan 0)
    for kv in ceNames do
      Assert.That([ "vanish"; "bullet" ], Does.Contain kv.Key,
                  "CE の木に " + kv.Key + " の重なりが出た")

  [<Test>]
  member _.``XML から読み直すと重なるのは vanish だけ``() =
    // **Playground は 2 通り の木を走らせる** —— プルダウンで選ぶと CE の木、
    // Apply すると字から読んだ木。**共有の度合いが違う**ので、片方 で測った
    // 「一意だ」をもう片方 へ広げない
    let read =
      catalog
      |> List.choose (fun i ->
          match writeAs SourceKind.Xml i.Bulletml with
          | None -> None
          | Some src -> Bulletml.tryReadXmlString src)
    read.Length |> should be (greaterThan 0)
    let names, books2 = dupsOf read
    books2 |> should be (greaterThan 0)
    for kv in names do
      Assert.That(kv.Key, Is.EqualTo "vanish",
                  "字から読んだ木に " + kv.Key + " の重なりが出た")