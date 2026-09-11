namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 意味の層（v2.3）。**読めて・組めても、走らないものが在る。**
///
/// --- 版の頭で測って、出すものを決めた
///
/// 候補は 4 つ 在って、同梱 176 本 に当てたらこうなった ——
///
///     top が無い            当たり 0 / 176 本   偽陽性なし。**出す**
///     呼ばれない定義         当たり 6 / 176 本   正しい弾幕でも光る。**強さを下げて出す**
///     参照の循環            当たり 2 / 176 本   正しい弾幕に在る。**出さない**
///     repeat の times が 0   当たり 0 / 176 本   字から取れない。**出さない**
///
/// 循環の 2 件 は `top -> top` と `topt -> gurugurup -> guru2 -> guru2` で、
/// どちらも同梱＝正しく動く弾幕。BulletML では `actionRef` の輪は正常
/// （無限に繰り返す弾幕がそう書く）。Core が「1 段 だけ解く」入口を
/// 持っているのは、まさにそれを前提にしている。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   `noEntry` を返さない              入口が無ければ光る
///   入口を「呼ばれない定義」に数える    6 点（同梱の 6 個 を含む）
///   `unused` を並べ替えない            本文の順で返る
///
/// --- 較正（v4.6。同じ名前の定義）
///
///   `if not (seen.Add …)` を「1 つ 目 から出す」に   赤 9
///   `seen` を対をまたいで共有する                    赤 2
///   並べ直しを消す                                  赤 1
///
/// **1 つ 目 が 9 点 に当たるのは、定義が全部 光るから** ——
/// ほかの検査の点も、余分な指摘が混ざって赤くなる。
///
/// **「語彙が引けなければ黙る」は、変異を当てても赤くならない。**
/// `Semantics` の `topPrefix = ""` の守りは冗長で、外しても答えが同じ
/// （`StartsWith ""` が全部 当たるので、結局 入口が在ることになる）。
/// あちらの但し書きに書いてある —— 点は残すが、**これは門ではなく
/// 「そう決めた」の記録**だと読むこと。
[<TestFixture>]
type Semantics() =

  static let catalog = Bullets.Dsl.All.bullets

  /// 本番と同じ対の引き方。**表を書かない**（`Refs.pairs` が語彙から作る）
  static let pairs =
    Refs.pairs
      (VocabForTests.vocab.Elements
       |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))

  static let top = VocabForTests.vocab.TopPrefix

  static let findingsOf (src: string) = Semantics.findings pairs top (XmlScan.tags src)

  /// 同梱 176 本 を XML の字にしたもの。
  /// **定数を畳まない側で焼く**（`Main.fs` の `InitialSource` と同じ）
  static let corpus =
    catalog
    |> List.map (fun i -> i.Name, BulletmlWriter.toIndentedXml 4 i.Bulletml)

  // --- 語彙が引けていること ---------------------------------------------------

  [<Test>]
  member _.``対が引けている``() =
    // **0 件 だと、下の点が全部「当たりが無くて緑」になる**
    pairs |> should not' (be Empty)

  [<Test>]
  member _.``top の綴りが引けている``() =
    // 空だと `NoEntryPoint` を 1 度 も出さない（そう作ってある）
    top |> should not' (equal "")

  // --- 入口が無い -------------------------------------------------------------

  // --- 同じ名前の定義（v4.6）-------------------------------------------------

  [<Test>]
  member _.``同じ名前の定義は 2 つ 目 から光る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="x"/></action>
  <action label="x"><wait>1</wait></action>
  <action label="x"><wait>2</wait></action>
</bulletml>"""
    let dup = findingsOf src |> List.filter (fun f -> f.Kind = Semantics.DuplicateDefinition)
    dup |> List.length |> should equal 1
    // **光るのは負けるほう**（走るのは先に書いたほう。実機で数えた）——
    // 5 行 目 の `x`
    (List.head dup).Line |> should equal 5

  [<Test>]
  member _.``3 つ 在れば 2 件 出る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="x"/></action>
  <action label="x"><wait>1</wait></action>
  <action label="x"><wait>2</wait></action>
  <action label="x"><wait>3</wait></action>
</bulletml>"""
    findingsOf src
    |> List.filter (fun f -> f.Kind = Semantics.DuplicateDefinition)
    |> List.length
    |> should equal 2

  [<Test>]
  member _.``要素が違えば別の名前``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="x"/><bulletRef label="x"/></action>
  <action label="x"><wait>1</wait></action>
  <bullet label="x"><action><wait>1</wait></action></bullet>
</bulletml>"""
    findingsOf src
    |> List.filter (fun f -> f.Kind = Semantics.DuplicateDefinition)
    |> should be Empty

  [<Test>]
  member _.``同梱 176 本 に同じ名前の定義は 1 件 も無い``() =
    // **偽陽性が無いから出せる**（v2.3 の `NoEntryPoint` と同じ形）。
    // ここが赤くなったら、出すかどうかの判断ごと見直す
    [ for (name, text) in corpus do
        for f in findingsOf text do
          if f.Kind = Semantics.DuplicateDefinition then yield name, f.Name ]
    |> should be Empty

  [<Test>]
  member _.``入口が無ければ光る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="fan">
    <fire><speed>2</speed><bullet/></fire>
  </action>
</bulletml>"""
    findingsOf src
    |> List.filter (fun f -> f.Kind = Semantics.NoEntryPoint)
    |> List.length
    |> should equal 1

  [<Test>]
  member _.``入口が在れば光らない``() =
    // 上の点は、**いつでも光っていれば緑**になる
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><speed>2</speed><bullet/></fire>
  </action>
</bulletml>"""
    findingsOf src
    |> List.filter (fun f -> f.Kind = Semantics.NoEntryPoint)
    |> should be Empty

  [<Test>]
  member _.``top で始まれば入口``() =
    // `top` そのものでなくてよい（Core は `StartsWith`）
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top2"><wait>1</wait></action>
</bulletml>"""
    findingsOf src
    |> List.filter (fun f -> f.Kind = Semantics.NoEntryPoint)
    |> should be Empty

  [<Test>]
  member _.``語彙が引けなければ黙る``() =
    // **「走らない」と言うほうが害が大きい。** 語彙が来ていないだけかもしれない
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="fan"><wait>1</wait></action>
</bulletml>"""
    Semantics.findings pairs "" (XmlScan.tags src)
    |> List.filter (fun f -> f.Kind = Semantics.NoEntryPoint)
    |> should be Empty

  [<Test>]
  member _.``対が無ければ黙る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="fan"><wait>1</wait></action>
</bulletml>"""
    Semantics.findings [] top (XmlScan.tags src) |> should be Empty

  // --- 呼ばれない定義 ---------------------------------------------------------

  [<Test>]
  member _.``呼ばれない定義が光る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="used"/></action>
  <action label="used"><wait>1</wait></action>
  <action label="dead"><wait>1</wait></action>
</bulletml>"""
    let unused =
      findingsOf src |> List.filter (fun f -> f.Kind = Semantics.UnusedDefinition)
    unused |> List.map (fun f -> f.Name) |> should equal [ "dead" ]

  [<Test>]
  member _.``入口は呼ばれなくても光らない``() =
    // **根から走るので、参照が無いのは正常。** ここを数えると
    // 同梱 176 本 のほぼ全部 が光る
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><wait>1</wait></action>
</bulletml>"""
    findingsOf src |> should be Empty

  [<Test>]
  member _.``位置はその名前の値の上``() =
    // 波線を引く先。**要素の頭でも属性の名前でもなく、値そのもの** ——
    // `Refs.missing` が `Hit.Column` / `EndColumn` をそのまま波線にしていて、
    // あれは `AttrHit` の値の範囲。**同じ作法に揃える**（片方 だけ違うと、
    // 参照の波線と定義の波線で引く先が食い違う）
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><wait>1</wait></action>
  <action label="dead"><wait>1</wait></action>
</bulletml>"""
    let f = findingsOf src |> List.exactlyOne
    f.Line |> should equal 4
    src.Split('\n').[f.Line - 1].Substring(f.Column - 1, f.EndColumn - f.Column)
    |> should equal "dead"

  [<Test>]
  member _.``要素名はその定義のもの``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><wait>1</wait></action>
  <bullet label="dead"><speed>1</speed></bullet>
</bulletml>"""
    let f = findingsOf src |> List.exactlyOne
    f.Element |> should equal "bullet"

  [<Test>]
  member _.``本文の順で返る``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><wait>1</wait></action>
  <bullet label="b1"><speed>1</speed></bullet>
  <action label="a1"><wait>1</wait></action>
  <bullet label="b2"><speed>1</speed></bullet>
</bulletml>"""
    // **対ごとに走るので、素では action がまとまって出る。** そこを並べ直している
    findingsOf src |> List.map (fun f -> f.Name) |> should equal [ "b1"; "a1"; "b2" ]

  // --- 同梱 176 本 ------------------------------------------------------------

  [<Test>]
  member _.``同梱に入口の無い本は 1 本 も無い``() =
    // **同梱は全部 走るもの。** ここが 0 でなければ、検査のほうが間違っている
    corpus
    |> List.filter (fun (_, src) ->
         findingsOf src |> List.exists (fun f -> f.Kind = Semantics.NoEntryPoint))
    |> List.map fst
    |> should be Empty

  [<Test>]
  member _.``同梱の呼ばれない定義は 6 個``() =
    // **正しい弾幕でも光る**（だから強さを下げて出す）。
    // 数を固定しておくと、検査を広げたときにここが動いて気づける
    let dead =
      corpus
      |> List.collect (fun (_, src) ->
           findingsOf src |> List.filter (fun f -> f.Kind = Semantics.UnusedDefinition))
    dead |> List.length |> should equal 6
    dead |> List.map (fun f -> f.Element) |> List.distinct |> List.sort
    |> should equal [ "action"; "bullet" ]

  // --- 定義に無い参照 ---------------------------------------------------------

  [<Test>]
  member _.``無い参照が光る``() =
    // **Apply は通る**（実測）—— 解けない参照は黙って無視されるだけで、
    // Core は落ちない。だからここで出さないと誰も言わない
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="nope"/></action>
</bulletml>"""
    let f =
      findingsOf src
      |> List.filter (fun f -> f.Kind = Semantics.MissingRef)
      |> List.exactlyOne
    f.Name |> should equal "nope"
    f.Element |> should equal "actionRef"

  [<Test>]
  member _.``在る参照は光らない``() =
    let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><actionRef label="ok"/></action>
  <action label="ok"><wait>1</wait></action>
</bulletml>"""
    findingsOf src |> should be Empty

  [<Test>]
  member _.``同梱に無い参照は 1 件 も無い``() =
    // **同梱は全部 走るもの。** ここが 0 でなければ、検査のほうが間違っている
    corpus
    |> List.collect (fun (n, src) ->
         findingsOf src
         |> List.filter (fun f -> f.Kind = Semantics.MissingRef)
         |> List.map (fun f -> n, f.Name))
    |> should be Empty

  [<Test>]
  member _.``同梱は全部 読めている``() =
    // 上の 2 点 は、**字が 1 つ も拾えていなくても緑**になる
    corpus
    |> List.filter (fun (_, src) -> (XmlScan.tags src).IsEmpty)
    |> List.map fst
    |> should be Empty
