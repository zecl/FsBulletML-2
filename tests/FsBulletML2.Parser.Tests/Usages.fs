namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// **同じ名前がどこに書いてあるか（rename の材料）の目盛り。**
///
/// --- 表記を知らない側に在る
///
/// 中身は `Languages/Lookup.fs` の `UsagesAt` 1 本 で、要るのは
/// `TokenAt`（いま何の上に居るか）と `Tags`（本文に何が在るか）だけ。
/// **どちらも表記ごとの 1 本 を指しているだけ。**
///
/// だからここは 4 表記 を同じ形で当てる —— **共通の側が壊れれば全部 赤くなる。**
///
/// --- いちばん強い点は「3 表記 で数が一致する」
///
/// コーパスは同じ弾幕が xml / sxml / fsb で対になっている。**同じ弾幕なら
/// 書いてある場所の数も同じはず** —— 位置は表記ごとに違うが、数は違わない。
/// v1.2 の頭で probe に当てたものを、そのまま門にしてある。
[<TestFixture>]
type Usages() =

  static let xml = Languages.Xml.XmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let sxml = Languages.Sxml.SxmlLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsb = Languages.Fsb.FsbLanguage(fun () -> vocab) :> ISourceLanguage
  static let fsharp = Languages.Fsharp.FsharpLanguage() :> ISourceLanguage

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

  /// その本文の中の label 値を全部 なめて、**引けた数の並び**を作る。
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
    // **片方 だけだと「参照からしか直せない」ことになる**
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
    // `label="top"` を直したいことは在る。**0 件 にすると rename が始まらない**
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
    // `type="aim"` は参照ではない。**対に載っていない属性は素通り**
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
  member _.``F# の CE では 引かない``() =
    // **黙って空なのではなく、ここで空だと決めている。**
    // CE の label は DSL の名前で、「要素名 + label 属性」の形ではない
    fsharp.Usages "let x =\n  untyped \"a\" { top { actionRef \"b\" [] } }" 30 |> should be Empty

  [<Test>]
  member _.``表記ごとに 字の数え方が違う``() =
    // 同じ本文を 3 つ に通す。**同じ答えが返ったら、どれかが effectively
    // 使われていない**（`Shape.Tags` を引き違えている）
    let src = "bulletml\n    action label=\"a\"\n    actionRef label=\"a\"\n"
    let cursor = src.IndexOf "actionRef label=\"a\"" + 17
    fsb.Usages src cursor |> List.length |> should equal 2
    xml.Usages src cursor |> should be Empty
    sxml.Usages src cursor |> should be Empty

  // --- 規則が 1 本 であること -----------------------------------------------

  [<Test>]
  member _.``走る先の対が host の波線と一致する``() =
    // **`Refs.pairs` に 1 本 しか無い**ことを、2 つ の入口から確かめる。
    // 割れると「波線は出るのに rename は当たらない」になり、
    // **どちらも単独では正しく見える**
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
