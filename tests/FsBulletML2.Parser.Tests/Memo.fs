namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 覚える口が嘘をつかないか（v4.9）。
/// 打鍵ごとに `Tags` を焼き直すと、同じ走査を繰り返す。直前の 1 本だけ覚える。
[<TestFixture>]
type Memo() =

  static let vocab = VocabForTests.vocab

  static let fresh () : SourceLanguage.ISourceLanguage =
    Languages.Xml.XmlLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

  static let freshCe () : SourceLanguage.ISourceLanguage =
    Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

  /// 参照が 2 つ と、呼ばれない定義が 1 つ
  static let a = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <actionRef label="shot"/>
    <actionRef label="shot"/>
  </action>
  <action label="shot">
    <fire><direction type="aim">0+$rank*2</direction><speed>2</speed><bullet/></fire>
  </action>
  <action label="dead">
    <wait>1</wait>
  </action>
</bulletml>"""

  /// 中身が違う。 参照が 1 つ、呼ばれない定義が 0、式も別
  static let b = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <actionRef label="shot"/>
    <wait>10-$rank</wait>
  </action>
  <action label="shot">
    <fire><direction type="absolute">180</direction><speed>3*$rank</speed><bullet/></fire>
  </action>
</bulletml>"""

  /// 交互に渡して、毎回 新しい器 と突き合わせる。
  /// 覚えた側 を返していたら、2 巡目 の a か b のどちらかが割れる
  static let alternates (of_: SourceLanguage.ISourceLanguage -> string -> 'T) =
    let one = fresh ()
    let want = [ of_ (fresh ()) a; of_ (fresh ()) b; of_ (fresh ()) a; of_ (fresh ()) b ]
    let got = [ of_ one a; of_ one b; of_ one a; of_ one b ]
    // 当てる先が在ることを、門が自分で数える ——
    // a と b の答えが同じなら、この点は何も見ていない
    List.item 0 want |> should not' (equal (List.item 1 want))
    got |> should equal want

  [<Test>]
  member _.``Lenses は本文を取り違えない``() =
    alternates (fun l s -> l.Lenses s |> List.map (fun x -> x.Name, x.Title, x.Line))

  [<Test>]
  member _.``Findings は本文を取り違えない``() =
    alternates (fun l s -> l.Findings s |> List.map (fun x -> x.Kind, x.Name, x.Line))

  [<Test>]
  member _.``Outline は本文を取り違えない``() =
    alternates (fun l s -> l.Outline s |> List.map (fun x -> x.Name, x.Line, x.EndLine))

  [<Test>]
  member _.``Hints は本文を取り違えない``() =
    alternates (fun l s -> l.Hints s |> List.map (fun x -> x.Text, x.Line, x.Column))

  /// `Findings` と `Hints` は同じ名前の並びを見る。
  /// 覚えているのは Texts の結果 1 つ。口ごとに答えが混ざらないこと。
  [<Test>]
  member _.``式の取り出しは、渡した名前ごとに覚える``() =
    let one = fresh ()
    let wantF = (fresh ()).Findings a |> List.map (fun x -> x.Kind, x.Name)
    let wantH = (fresh ()).Hints a |> List.map (fun x -> x.Text)
    // 同じ本文で往復する —— 本文が変わらないので、鍵が本文だけだと当たる
    one.Findings a |> List.map (fun x -> x.Kind, x.Name) |> should equal wantF
    one.Hints a |> List.map (fun x -> x.Text) |> should equal wantH
    one.Findings a |> List.map (fun x -> x.Kind, x.Name) |> should equal wantF
    one.Hints a |> List.map (fun x -> x.Text) |> should equal wantH
    wantH |> should not' (be Empty)

  /// F# の CE も同じ形で覚える。語彙が入れ替わるのは起動時の 1 度だけ。
  /// 試験の中に 2 つ目を作ると、本番に無い形を固定することになる。
  [<Test>]
  member _.``CE も本文を取り違えない``() =
    let write (kind: SourceKind) (b: Bulletml) =
      match SourceWriter.tryFind kind with
      | None -> failwith "書く口が無い"
      | Some w -> match w.Write b with | Result.Ok s -> s | Result.Error e -> failwith e
    let two =
      Bullets.Dsl.All.bullets
      |> List.truncate 2
      |> List.map (fun i -> write SourceKind.FSharpDsl i.Bulletml)
    match two with
    | [ x; y ] ->
      let one = freshCe ()
      let of_ (l: SourceLanguage.ISourceLanguage) s = l.Lenses s |> List.map (fun n -> n.Name, n.Title)
      let want = [ of_ (freshCe ()) x; of_ (freshCe ()) y; of_ (freshCe ()) x ]
      want.[0] |> should not' (equal want.[1])
      [ of_ one x; of_ one y; of_ one x ] |> should equal want
    | _ -> failwith "同梱が 2 本 無い"

/// 位置から行桁を引く 3 本が、同じ答えを返すか（v4.9）。
/// `lineColumn` は 1 回につき頭から走る。昇順の並びは 1 巡。
[<TestFixture>]
type LineColumns() =

  static let src =
    Bullets.Dsl.All.bullets
    |> List.map (fun i -> BulletmlWriter.toIndentedXml 4 i.Bulletml)
    |> List.maxBy String.length

  [<Test>]
  member _.``当てる先が在る``() =
    // 0 件 は緑にしない —— 本文が短いと、行をまたぐ点が 1 つ も無い
    src.Length |> should be (greaterThan 10000)
    (src |> Seq.filter (fun c -> c = '\n') |> Seq.length) |> should be (greaterThan 100)

  [<Test>]
  member _.``表で引いても、頭から走っても同じ``() =
    let lookup = Scan.lineColumnLookup src
    let mutable bad = 0
    for i in 0 .. src.Length do
      if lookup i <> Scan.lineColumn src i then bad <- bad + 1
    bad |> should equal 0

  [<Test>]
  member _.``昇順で 1 巡 しても同じ``() =
    // 100 字 おきに（全部 だと昇順の 1 巡 が長くなるだけで、見るものは同じ）
    let offs = [ for i in 0 .. 100 .. src.Length -> i ]
    let ascending = Scan.lineColumnsAscending src offs
    let one = offs |> List.map (Scan.lineColumn src)
    List.length ascending |> should be (greaterThan 100)
    ascending |> should equal one

  /// 昇順が崩れていても正しい答えを返す（但し書きに書いてある）——
  /// 遅くなるだけ、を字で固定する
  [<Test>]
  member _.``昇順が崩れても答えは変わらない``() =
    let offs = [ 5000; 100; 9000; 200 ]
    Scan.lineColumnsAscending src offs |> should equal (offs |> List.map (Scan.lineColumn src))
