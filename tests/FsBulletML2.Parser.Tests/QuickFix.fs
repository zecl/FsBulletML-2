namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// **「無い参照を、綴りの近い定義へ直す」の目盛り。**
///
/// --- 表記を知らない側に在る
///
/// 中身は `Languages/Lookup.fs` の `FixesAt` 1 本 で、要るのは
/// `Tags`（本文に何が在るか）と語彙だけ。**`Languages/*.fs` の差分は 0 行。**
///
/// --- 波線には紐づけていない
///
/// 波線は 1 文字 打った時点で消える（印は文字に追随しないので、そこで下ろすのが
/// 正しい）。紐づけると**直し方が Apply の直後の窓でしか出ない** ——
/// だから本文から数え直している。ここもそう当てる（波線を一度も出さない）。
[<TestFixture>]
type QuickFix() =

  static let xml = Languages.Xml.XmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let sxml = Languages.Sxml.SxmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsb = Languages.Fsb.FsbLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsharp = Languages.Fsharp.FsharpLanguage() :> ISourceLanguage

  /// `|` の位置をカーソルとして直し方を引く
  let at (lang: ISourceLanguage) (marked: string) =
    lang.Fixes (marked.Replace("|", "")) (marked.IndexOf '|')

  let titles lang marked = at lang marked |> List.map (fun f -> f.Title) |> List.sort

  static let corpus =
    lazy
      let root = Path.Combine(AppContext.BaseDirectory, "TestData", "xml")
      if Directory.Exists root
      then Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories) |> Seq.toArray
      else [||]

  // --- 綴りの近さ そのもの ---------------------------------------------------

  [<Test>]
  member _.``同じ字は 近い``() =
    Distance.within1 "top" "top" |> should be True
    Distance.within1 "" "" |> should be True

  [<Test>]
  member _.``1 文字 の 置換・削除・挿入 は 近い``() =
    Distance.within1 "tap" "top" |> should be True    // 置換
    Distance.within1 "tp" "top" |> should be True     // 削除
    Distance.within1 "toop" "top" |> should be True   // 挿入
    Distance.within1 "" "a" |> should be True

  [<Test>]
  member _.``2 文字 違えば 近くない``() =
    // **1 か所 だけ違うものと並べる。** 片方 だけ見ると、
    // 「近い」を返しすぎているのか足りないのかが分からない
    Distance.within1 "tip" "top" |> should be True     // 1 か所 の置換
    Distance.within1 "toe" "tip" |> should be False    // 2 か所
    Distance.within1 "taip" "top" |> should be False   // 1 文字 落としても合わない
    Distance.within1 "abc" "cba" |> should be False
    Distance.within1 "ab" "" |> should be False

  [<Test>]
  member _.``入れ替えは 近くない``() =
    // `ab` -> `ba` は 2 回 の置換。**1 回 では作れない**
    Distance.within1 "ba" "ab" |> should be False

  // --- 直し方を出す / 出さない ----------------------------------------------

  [<Test>]
  member _.``無い参照の上で 近い定義を出す``() =
    titles xml "<bulletml><action label=\"top\"/><actionRef label=\"t|p\"/></bulletml>"
    |> should equal [ "tp を top に直す" ]

  [<Test>]
  member _.``遠い名前は出さない``() =
    // 嘘の直し方を出さない。押した人は直ったと思う
    at xml "<bulletml><action label=\"top\"/><actionRef label=\"n|ope\"/></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``定義が在るなら 出さない``() =
    at xml "<bulletml><action label=\"top\"/><actionRef label=\"t|op\"/></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``カーソルが その名前の上でなければ 出さない``() =
    // 本文の全部 を出さない。直すのはいま見ているところ
    let src = "<bulletml><action label=\"top\"/><actionRef label=\"tp\"/></bulletml>"
    xml.Fixes src (src.IndexOf "action label") |> should be Empty
    xml.Fixes src 0 |> should be Empty

  [<Test>]
  member _.``別の種類の定義は 候補にしない``() =
    // `bullet label="tp"` が在っても、`actionRef` の直し方にはならない
    titles xml
      ("<bulletml><action label=\"top\"/><bullet label=\"tq\"/>"
       + "<actionRef label=\"t|p\"/></bulletml>")
    |> should equal [ "tp を top に直す" ]

  [<Test>]
  member _.``近い定義が 2 つ 在れば 2 つ 出す``() =
    // **選ぶのは人。** 片方 を勝手に選ばない
    titles xml
      ("<bulletml><action label=\"tap\"/><action label=\"top\"/>"
       + "<actionRef label=\"t|p\"/></bulletml>")
    |> should equal [ "tp を tap に直す"; "tp を top に直す" ]

  [<Test>]
  member _.``範囲が 値そのものを指す``() =
    // `<bulletml><action label="top"/><actionRef label="tp"/></bulletml>`
    // の `tp` は 50 桁目 から、閉じ引用符が 52
    match at xml "<bulletml><action label=\"top\"/><actionRef label=\"t|p\"/></bulletml>" with
    | [ f ] ->
      f.Line |> should equal 1
      f.Column |> should equal 50
      f.EndColumn |> should equal 52
      f.Text |> should equal "top"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  // --- 表記ごと -------------------------------------------------------------

  [<Test>]
  member _.``sxml でも同じように出る``() =
    titles sxml
      "(bulletml (action (@ (label \"top\"))) (actionRef (@ (label \"t|p\"))))"
    |> should equal [ "tp を top に直す" ]

  [<Test>]
  member _.``fsb でも同じように出る``() =
    titles fsb "bulletml\n    action label=\"top\"\n    actionRef label=\"t|p\"\n"
    |> should equal [ "tp を top に直す" ]

  [<Test>]
  member _.``F# の CE では 出ない``() =
    // **黙って空なのではなく、ここで空だと決めている**
    fsharp.Fixes "let x =\n  untyped \"a\" { top { actionRef \"b\" [] } }" 30 |> should be Empty

  // --- コーパスと突き合わせる -----------------------------------------------

  [<Test>]
  member _.``コーパスが読めている``() =
    corpus.Value.Length |> should greaterThan 0

  [<Test>]
  member _.``参照を 1 文字 落とすと、元の名前が候補に出る``() =
    // **この版の本題。** 打ち間違いを作って、直し方が出るかを本文で当てる。
    // 落とす位置は**全部** 試す
    let mutable measured = 0
    let bad = ResizeArray<string>()
    for file in corpus.Value do
      let src = File.ReadAllText file
      let tags = XmlScan.tags src |> List.filter (fun t -> not t.Closing)
      let pairs =
        Refs.pairs (vocab.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
      // 定義が在る参照だけを壊す（もともと無いものは、この点の材料ではない）
      let broken =
        [ for (refName, defName, attr) in pairs do
            let defined =
              tags
              |> List.filter (fun t -> t.TagName = defName)
              |> List.choose (fun t -> t.Attrs |> List.tryFind (fun a -> a.AttrName = attr))
              |> List.map (fun a -> a.Value)
              |> List.distinct
            for t in tags do
              if t.TagName = refName then
                match t.Attrs |> List.tryFind (fun a -> a.AttrName = attr) with
                | Some a when List.contains a.Value defined ->
                  for i in 0 .. a.Value.Length - 1 do
                    let typo = a.Value.Remove(i, 1)
                    if not (List.contains typo defined) then
                      yield a.ValueStart, a.ValueStop, a.Value, typo
                | _ -> () ]
      for (from, till, original, typo) in broken do
        let source = src.Substring(0, from) + typo + src.Substring(till)
        measured <- measured + 1
        let found = xml.Fixes source from |> List.map (fun f -> f.Text)
        if not (List.contains original found) then
          bad.Add(sprintf "%s: %s -> %s の直し方に %s が無い（%A）"
                    (Path.GetFileName file) original typo original found)
    // 1 本 も壊せていなければ、この点は何も測っていない
    measured |> should greaterThan 20
    bad |> List.ofSeq |> should be Empty

  [<Test>]
  member _.``壊していない本文には 1 つ も出さない``() =
    // **全部 の label の上で引く。** 読める弾幕に直し方が出たら、それは嘘
    let noisy =
      corpus.Value
      |> Array.choose (fun file ->
          let src = File.ReadAllText file
          let found =
            XmlScan.tags src
            |> List.filter (fun t -> not t.Closing)
            |> List.collect (fun t -> t.Attrs)
            |> List.collect (fun a -> xml.Fixes src a.ValueStart)
          // 参照が本当に欠けている弾幕は、コーパスに置いてある
          if found.IsEmpty || (References.missing XmlScan.tags src).Length > 0 then None
          else Some(sprintf "%s: %s" (Path.GetFileName file) found.Head.Title))
      |> Array.toList
    noisy |> should be Empty
