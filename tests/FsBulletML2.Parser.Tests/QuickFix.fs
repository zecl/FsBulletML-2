namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// **「無い参照をどう直すか」の目盛り。** 直し方は 2 通り ——
///
///     綴りを直す    近い定義へ名前を書き換える（幅の在る範囲の置き換え）
///     定義を作る    根の直下 に空の定義を挿す（**幅 0 の範囲**）
///
/// **同じ型で足りる。** `Fix` の `Column = EndColumn` が挿し込みの印で、
/// 口を分けていない。
///
/// --- 表記を知らない側に在る
///
/// 中身は `Languages/Lookup.fs` の `FixesAt` 1 本。綴りの直しに要るのは
/// `Tags`（本文に何が在るか）と語彙だけで、**そこは `Languages/*.fs` の
/// 差分が 0 行。** 挿す場所だけは表記ごと（`Shape.DefinitionAt`）——
/// 閉じ札の在る XML、閉じ括弧の sxml、行の字下げだけの fsb で、
/// **「根の直下」の指し方が揃わない。**
///
/// --- 波線には紐づけていない
///
/// 波線は 1 文字 打った時点で消える（印は文字に追随しないので、そこで下ろすのが
/// 正しい）。紐づけると**直し方が Apply の直後の窓でしか出ない** ——
/// だから本文から数え直している。ここもそう当てる（波線を一度も出さない）。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   定義を作るほうを出さない        6 点
///   字下げを測らずに 4 で決め打つ    字下げは本文に合わせる
///   根が閉じていなくても作る        根が閉じていなければ 作らない
///   sxml の挿し先を 1 文字 ずらす    当てた本文が字まで合う
///   fsb の字下げを根に合わせる      同上
///   行の頭を 1 つ ずらす            12 点 と 突き合わせ
///
/// **後ろの 2 つ は、はじめ赤くならなかった。**
/// 「参照が埋まった」だけを見ていたのが穴 —— `References.missing` は
/// **入れ子を見ない**ので、根の外へ挿しても・字下げを間違えても 0 件 になる。
/// 当てた本文そのものを字で見る点を足して、両方 赤くした。
[<TestFixture>]
type QuickFix() =

  static let xml = Languages.Xml.XmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let sxml = Languages.Sxml.SxmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsb = Languages.Fsb.FsbLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsharp = Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> ISourceLanguage

  /// `|` の位置をカーソルとして直し方を引く
  let at (lang: ISourceLanguage) (marked: string) =
    lang.Fixes (marked.Replace("|", "")) (marked.IndexOf '|')

  /// 綴りを直すほうだけ。**幅の在る範囲が置き換えの印**
  let renames lang marked = at lang marked |> List.filter (fun f -> f.EndColumn > f.Column)

  /// 定義を作るほうだけ。**幅 0 が挿し込みの印**
  let creates lang marked = at lang marked |> List.filter (fun f -> f.EndColumn = f.Column)

  let titles lang marked = renames lang marked |> List.map (fun f -> f.Title) |> List.sort

  /// 直し方を当てた本文。**挿し込みは行と桁で入る**ので、
  /// ここで本文へ戻して「読める字になったか」を見る
  let applied (src: string) (f: Fix) =
    let lines = src.Replace("\r\n", "\n").Split('\n')
    let line = lines.[f.Line - 1]
    lines.[f.Line - 1] <- line.Substring(0, f.Column - 1) + f.Text + line.Substring(f.EndColumn - 1)
    String.Join("\n", lines)

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
  member _.``遠い名前には 綴りの直しを出さない``() =
    // 嘘の直し方を出さない。押した人は直ったと思う。
    // **「定義を作る」は出る** —— あちらは嘘ではない（まだ書いていないだけ）
    renames xml "<bulletml><action label=\"top\"/><actionRef label=\"n|ope\"/></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``定義が在るなら 何も出さない``() =
    // **どちらも出さない。** 参照が欠けていなければ直すものが無い
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
  member _.``綴りの直しの範囲が 値そのものを指す``() =
    // `<bulletml><action label="top"/><actionRef label="tp"/></bulletml>`
    // の `tp` は 50 桁目 から、閉じ引用符が 52
    match renames xml "<bulletml><action label=\"top\"/><actionRef label=\"t|p\"/></bulletml>" with
    | [ f ] ->
      f.Line |> should equal 1
      f.Column |> should equal 50
      f.EndColumn |> should equal 52
      f.Text |> should equal "top"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  // --- 定義を作る -------------------------------------------------------------

  [<Test>]
  member _.``無い参照の上で 定義を作れる``() =
    match creates xml "<bulletml>\n    <actionRef label=\"t|op\"/>\n</bulletml>" with
    | [ f ] ->
      f.Title |> should equal "top の <action> を作る"
      // **幅 0。** 置き換えではなく挿し込み
      f.Column |> should equal f.EndColumn
      f.Text |> should equal "    <action label=\"top\">\n    </action>\n"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``作った定義を当てると 参照が埋まる``() =
    let src = "<bulletml>\n    <actionRef label=\"top\"/>\n</bulletml>"
    let cursor = src.IndexOf "label=\"top\"" + 8
    match xml.Fixes src cursor |> List.filter (fun f -> f.EndColumn = f.Column) with
    | [ f ] ->
      let after = applied src f
      References.missing XmlScan.tags after |> should be Empty
      // 当てる前は 1 件。**0 件 と 0 件 を比べていない**
      References.missing XmlScan.tags src |> List.length |> should equal 1
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``字下げは本文に合わせる``() =
    // 書き手は 4 で焼くが、人が 2 で書き直していることは在る
    match creates xml "<bulletml>\n  <actionRef label=\"t|op\"/>\n</bulletml>" with
    | [ f ] -> f.Text |> should equal "  <action label=\"top\">\n  </action>\n"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``根が閉じていなければ 作らない``() =
    // 打っている途中の本文はふつうに閉じていない。
    // **そこで場所を決め打つと本文の外に出る**
    creates xml "<bulletml>\n    <actionRef label=\"t|op\"/>\n" |> should be Empty

  [<Test>]
  member _.``綴りの直しが在っても 作るほうも出す``() =
    // 近い名前が在ることと、その名前を使いたいことは別
    let marked = "<bulletml>\n    <action label=\"top\"/>\n    <actionRef label=\"t|p\"/>\n</bulletml>"
    renames xml marked |> List.length |> should equal 1
    creates xml marked |> List.length |> should equal 1

  [<Test>]
  member _.``4 表記 とも 当てた本文が字まで合う``() =
    // **「参照が埋まった」だけでは足りない。** `References.missing` は入れ子を
    // 見ないので、根の外へ挿しても・字下げを間違えても 0 件 になる
    // （較正で 2 通り 踏んだ）。当てた本文そのものを見る。
    //
    // **閉じるものが行頭に在る形を通す。** 1 行 に畳んだ本文だけだと、
    // 「行の頭に挿す」枝を 1 度 も通らない
    let cases =
      [ xml, "<bulletml>\n    <actionRef label=\"top\"/>\n</bulletml>",
        "<bulletml>\n    <actionRef label=\"top\"/>\n    <action label=\"top\">\n    </action>\n</bulletml>"
        sxml, "(bulletml\n    (actionRef (@ (label \"top\")))\n)",
        "(bulletml\n    (actionRef (@ (label \"top\")))\n    (action (@ (label \"top\")))\n)"
        fsb, "bulletml\n    actionRef label=\"top\"\n",
        "bulletml\n    actionRef label=\"top\"\n    action label=\"top\"\n" ]
    for (lang: ISourceLanguage, src, expected) in cases do
      let cursor = src.IndexOf "top"
      match lang.Fixes src cursor |> List.filter (fun f -> f.EndColumn = f.Column) with
      | [ f ] -> applied src f |> should equal expected
      | other -> failwithf "%s: 1 件 のはずが %d 件" lang.Kind.Id other.Length

  [<Test>]
  member _.``作った定義を当てた本文が 読める``() =
    // **数だけ見ていると出ない。** 参照が埋まっても、その字が読めるとは
    // 限らない —— F# の CE ではそこで落ちていた（`FsharpUsages`）。
    // ここは残る 3 表記 を同じ形で当てる
    let cases =
      [ SourceKind.Xml, xml,
        "<bulletml type=\"vertical\" xmlns=\"http://www.asahi-net.or.jp/~cs8k-cyu/bulletml\">\n    <action label=\"top\"><actionRef label=\"loop\"/></action>\n</bulletml>"
        SourceKind.Sxml, sxml,
        "(bulletml (@ (xmlns \"http://www.asahi-net.or.jp/~cs8k-cyu/bulletml\") (type \"vertical\"))\n    (action (@ (label \"top\")) (actionRef (@ (label \"loop\"))))\n)"
        SourceKind.Fsb, fsb,
        "bulletml xmlns=\"http://www.asahi-net.or.jp/~cs8k-cyu/bulletml\" type=\"vertical\"\n    action label=\"top\"\n        actionRef label=\"loop\"\n" ]
    for (kind, lang: ISourceLanguage, src) in cases do
      let reader = (SourceReader.tryFind kind).Value
      // 当てる前は読める。**読めない本文と比べていない**
      reader.Apply (fun _ -> ()) src |> should equal None
      match lang.Fixes src (src.IndexOf "\"loop\"" + 2) |> List.filter (fun f -> f.EndColumn = f.Column) with
      | [ f ] ->
        let after = applied src f
        match reader.Apply (fun _ -> ()) after with
        | Some failure -> failwithf "%s: 当てた本文が読めない: %s" kind.Id failure.Message
        | None -> ()
      | other -> failwithf "%s: 1 件 のはずが %d 件" kind.Id other.Length

  [<Test>]
  member _.``sxml と fsb でも 参照が埋まる``() =
    let cases =
      [ sxml, SxmlScan.tags, "(bulletml\n    (actionRef (@ (label \"top\")))\n)"
        fsb, FsbScan.tags, "bulletml\n    actionRef label=\"top\"\n" ]
    for (lang: ISourceLanguage, tags, src) in cases do
      let cursor = src.IndexOf "top"
      match lang.Fixes src cursor |> List.filter (fun f -> f.EndColumn = f.Column) with
      | [ f ] ->
        References.missing tags (applied src f) |> should be Empty
        References.missing tags src |> List.length |> should equal 1
      | other -> failwithf "%s: 1 件 のはずが %d 件" lang.Kind.Id other.Length

  [<Test>]
  member _.``F# の CE でも作る（v1.9）``() =
    // v1.6 まで空だった。**カーソルが名前の上に無ければ空**なのは同じ
    let src = "let x =\n  untyped \"a\" { top { actionRef \"b\" [] } }"
    fsharp.Fixes src (src.IndexOf "actionRef") |> should be Empty
    fsharp.Fixes src (src.IndexOf "\"b\"" + 1) |> should not' (be Empty)

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
  member _.``F# の CE でも出る（v1.9）``() =
    // 綴りの直しも同じ 1 本 を通る（詳しくは `FsharpUsages`）
    titles fsharp "let x =\n  untyped \"a\" {\n    defAction \"loop\" { wait \"1\" }\n    nestAs \"z\" { actionRef \"l|op\" [] }\n  }\n"
    |> should equal [ "lop を loop に直す" ]

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
