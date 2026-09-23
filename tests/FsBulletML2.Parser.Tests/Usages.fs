namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// 同じ名前がどこに書いてあるか（rename の材料）の目盛り。
/// 中身は `UsagesAt` 1 本。要るのは `TokenAt` と `Tags` だけ。
[<TestFixture>]
type Usages() =

  static let xml = Languages.Xml.XmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let sxml = Languages.Sxml.SxmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsb = Languages.Fsb.FsbLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsharp = Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> ISourceLanguage

  /// `|` の位置をカーソルとして引く
  let at (lang: ISourceLanguage) (marked: string) =
    lang.Usages (marked.Replace("|", "")) (marked.IndexOf '|')

  let places (lang: ISourceLanguage) marked =
    at lang marked |> List.map (fun u -> u.Line, u.Column, u.EndColumn)

  static let corpus (ext: string) =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData", ext)
    if Directory.Exists root
    then Directory.EnumerateFiles(root, "*." + ext, SearchOption.AllDirectories) |> Seq.toArray
    else [||]

  /// その本文の中の label 値を全部 なめて、引けた数の並びを作る。
  /// 位置は表記ごとに違うので、比べられるのはこちら
  let profile (lang: ISourceLanguage) (tags: string -> TagHit list) (src: string) =
    tags src
    |> List.filter (fun t -> not t.Closing)
    |> List.collect (fun t -> t.Attrs)
    |> List.map (fun a -> (lang.Usages src a.ValueStart).Length)
    |> List.filter (fun n -> n > 0)
    |> List.sort

  // --- 引く / 引かない ------------------------------------------------------

  [<Test>]
  member _.``定義の上でも 参照の上でも 同じ並び``() =
    // 片方 だけだと「参照からしか直せない」ことになる
    let src = "<bulletml><action label=\"a\"/><actionRef label=\"a\"/></bulletml>"
    let fromDef = xml.Usages src (src.IndexOf "action label=\"a\"" + 14)
    let fromRef = xml.Usages src (src.IndexOf "actionRef label=\"a\"" + 17)
    fromDef.Length |> should equal 2
    fromDef |> should equal fromRef

  [<Test>]
  member _.``位置が値そのものを指す``() =
    // `<bulletml><action label="a"/></bulletml>` の `a` は 26 桁目、
    // 閉じ引用符が 27
    match places xml "<bulletml><action label=\"|a\"/></bulletml>" with
    | [ (line, col, endCol) ] ->
      line |> should equal 1
      col |> should equal 26
      endCol |> should equal 27
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``参照が無い定義でも 自分 1 つ は返る``() =
    // `label="top"` を直したいことは在る。0 件 にすると rename が始まらない
    at xml "<bulletml><action label=\"t|op\"/></bulletml>" |> List.length |> should equal 1

  [<Test>]
  member _.``別の名前は混ざらない``() =
    let src = "<bulletml><action label=\"a\"/><action label=\"b\"/><actionRef label=\"a\"/></bulletml>"
    xml.Usages src (src.IndexOf "label=\"a\"" + 7) |> List.length |> should equal 2

  [<Test>]
  member _.``別の種類の label は混ざらない``() =
    // `action` と `bullet` は別の対。同じ字でも一緒には直さない
    let src =
      "<bulletml><action label=\"a\"/><bullet label=\"a\"/><actionRef label=\"a\"/></bulletml>"
    let found = xml.Usages src (src.IndexOf "actionRef label=\"a\"" + 17)
    found.Length |> should equal 2
    // 3 件 なら bullet まで巻き込んでいる
    found |> List.map (fun u -> u.Text) |> List.distinct |> should equal [ "a" ]

  [<Test>]
  member _.``名前の上でなければ 空``() =
    at xml "<bulletml><ac|tion label=\"a\"/></bulletml>" |> should be Empty
    at xml "<bulletml><action la|bel=\"a\"/></bulletml>" |> should be Empty
    at xml "<bulletml><wait>1|</wait></bulletml>" |> should be Empty
    at xml "|<bulletml></bulletml>" |> should be Empty

  [<Test>]
  member _.``label でない属性値では 引かない``() =
    // `type="aim"` は参照ではない。対に載っていない属性は素通り
    at xml "<bulletml><direction type=\"a|im\">0</direction></bulletml>" |> should be Empty

  // --- 表記ごと -------------------------------------------------------------

  [<Test>]
  member _.``sxml でも同じように引ける``() =
    at sxml "(bulletml (action (@ (label \"a\"))) (actionRef (@ (label \"|a\"))))"
    |> List.length |> should equal 2

  [<Test>]
  member _.``fsb でも同じように引ける``() =
    at fsb "bulletml\n    action label=\"a\"\n    actionRef label=\"|a\"\n"
    |> List.length |> should equal 2

  [<Test>]
  member _.``F# の CE でも引く（名前は文字列の中）``() =
    // v1.9 で引けるようになった。CE の名前そのものの上では引かない ——
    // 名前は文字列の中に在る（詳しくは `FsharpUsages`）
    let src = "let x =\n  untyped \"a\" { top { actionRef \"b\" [] } }"
    fsharp.Usages src (src.IndexOf "actionRef") |> should be Empty
    fsharp.Usages src (src.IndexOf "\"b\"" + 1) |> List.length |> should equal 1

  [<Test>]
  member _.``表記ごとに 字の数え方が違う``() =
    // 同じ本文を 3 つ に通す。同じ答えが返ったら、どれかが effectively
    // 使われていない（`Shape.Tags` を引き違えている）
    let src = "bulletml\n    action label=\"a\"\n    actionRef label=\"a\"\n"
    let cursor = src.IndexOf "actionRef label=\"a\"" + 17
    fsb.Usages src cursor |> List.length |> should equal 2
    xml.Usages src cursor |> should be Empty
    sxml.Usages src cursor |> should be Empty

  // --- どちら側か（定義へ移動 / 参照）---------------------------------------

  [<Test>]
  member _.``定義側と参照側を 1 欄 で分ける``() =
    // 定義へ移動（F12）はこの欄だけで決まる。口を足していない
    let src = "<bulletml><action label=\"a\"/><actionRef label=\"a\"/></bulletml>"
    let found = xml.Usages src (src.IndexOf "actionRef label=\"a\"" + 17)
    found |> List.filter (fun u -> u.IsDefinition) |> List.length |> should equal 1
    found |> List.filter (fun u -> not u.IsDefinition) |> List.length |> should equal 1
    // 定義のほうが手前に在る。並びは本文の順
    (List.head found).IsDefinition |> should equal true

  [<Test>]
  member _.``定義が無ければ どれも定義ではない``() =
    // 参照だけ在る本文はふつうに書ける（そこは波線と Quick Fix の担当）。
    // 飛び先が無いことを、空で言う
    let src = "<bulletml><actionRef label=\"a\"/></bulletml>"
    let found = xml.Usages src (src.IndexOf "label=\"a\"" + 7)
    found |> List.length |> should equal 1
    found |> List.filter (fun u -> u.IsDefinition) |> should be Empty

  [<Test>]
  member _.``定義が 2 つ 在ることも在る``() =
    // 同じ label の定義を 2 つ 書ける。1 つ に決まると思ってはいけない
    let src =
      "<bulletml><action label=\"a\"/><action label=\"a\"/><actionRef label=\"a\"/></bulletml>"
    xml.Usages src (src.IndexOf "actionRef label=\"a\"" + 17)
    |> List.filter (fun u -> u.IsDefinition)
    |> List.length
    |> should equal 2

  [<Test>]
  member _.``4 表記 とも 同じ分け方``() =
    // 分けているのは表記を知らない側の 1 本。表記ごとに割れていない
    at sxml "(bulletml (action (@ (label \"a\"))) (actionRef (@ (label \"|a\"))))"
    |> List.map (fun u -> u.IsDefinition)
    |> should equal [ true; false ]
    at fsb "bulletml\n    action label=\"a\"\n    actionRef label=\"|a\"\n"
    |> List.map (fun u -> u.IsDefinition)
    |> should equal [ true; false ]

  [<Test>]
  member _.``定義側と参照側の要素名は重ならない``() =
    // これが崩れると 1 欄 では足りない —— 札の名前だけでは
    // どちら側か言えなくなるので、口を足すことになる
    let pairs =
      Refs.pairs (vocab.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    let refNames = pairs |> List.map (fun (r, _, _) -> r) |> Set.ofList
    let defNames = pairs |> List.map (fun (_, d, _) -> d) |> Set.ofList
    Set.intersect refNames defNames |> should be Empty
    // 0 組 なら上は「空と空が交わらない」だけ
    pairs |> should not' (be Empty)

  // --- 規則が 1 本 であること -----------------------------------------------

  [<Test>]
  member _.``走る先の対が host の波線と一致する``() =
    // `Refs.pairs` に 1 本しか無いことを、2 つの入口から確かめる。
    // 割れると「波線は出るのに rename は当たらない」。どちらも単独では正しく見える。
    let fromVocab =
      Refs.pairs (vocab.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
    fromVocab |> should equal (List.ofArray References.pairs)
    // 0 件 なら上は「空と空が一致した」だけ
    fromVocab |> should not' (be Empty)

  // --- コーパスと突き合わせる -----------------------------------------------

  [<Test>]
  member _.``3 表記 で 引ける数が一致する``() =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData")
    let mutable compared = 0
    let mutable withTargets = 0
    let bad = ResizeArray<string>()
    for xmlFile in corpus "xml" do
      let rel = xmlFile.Substring(Path.Combine(root, "xml").Length + 1)
      let others =
        [ "sxml", sxml, SxmlScan.tags; "fsb", fsb, FsbScan.tags ]
        |> List.choose (fun (ext, lang, tags) ->
             let f = Path.Combine(root, ext, Path.ChangeExtension(rel, "." + ext))
             if File.Exists f then Some(ext, profile lang tags (File.ReadAllText f)) else None)
      if others.Length = 2 then
        compared <- compared + 1
        let mine = profile xml XmlScan.tags (File.ReadAllText xmlFile)
        if not mine.IsEmpty then withTargets <- withTargets + 1
        for (ext, theirs) in others do
          if theirs <> mine then
            bad.Add(sprintf "%s: xml %A / %s %A" rel mine ext theirs)
    bad |> List.ofSeq |> should be Empty
    // 3 表記 そろった弾幕が無ければ、上は何も測っていない
    compared |> should greaterThan 100
    // 対象がどこにも無ければ、一致しているのは「全部 空」だけ
    withTargets |> should greaterThan 100
