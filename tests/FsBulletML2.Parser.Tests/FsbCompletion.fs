namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Fsb

/// **候補と hover、fsb の側。** `XmlCompletion` / `SxmlCompletion` と対。
///
/// 語彙は XML / sxml と同じ `Vocabulary`（Core の DTD 由来）をそのまま渡す ——
/// **表記が変わっても要素と属性は変わらない。** 変わるのは書き方だけで、
/// それがここで当てているもの。
///
/// --- この試験が本当に見ているもの
///
/// 候補を作る中身は `Languages/Lookup.fs` に在って**表記を知らない**。
/// だからここで赤くなるのは `Languages/Fsb.fs` の `shape` か
/// `FsbScan` のどちらか —— **共通の側が壊れれば XML と sxml も一緒に赤くなる。**
[<TestFixture>]
type FsbCompletion() =

  static let lang = FsbLanguage(fun () -> vocab)

  /// `|` の位置をカーソルとして候補を出す
  let complete (marked: string) =
    lang.Candidates(marked.Replace("|", ""), marked.IndexOf '|')

  let labels marked = complete marked |> List.map (fun c -> c.Label) |> List.sort

  let hover (marked: string) =
    lang.HoverAt(marked.Replace("|", ""), marked.IndexOf '|')

  // --- 候補 -----------------------------------------------------------------

  [<Test>]
  member _.``根の外では 根になれる要素``() =
    labels "|" |> should equal [ "bulletml" ]

  [<Test>]
  member _.``字下げの先では 置ける子要素``() =
    labels "bulletml\n    |" |> should equal [ "action"; "bullet"; "fire" ]

  [<Test>]
  member _.``字下げを戻すと 外の子要素``() =
    labels "bulletml\n    action\n        fire\n    |"
    |> should equal [ "action"; "bullet"; "fire" ]

  [<Test>]
  member _.``名前の後ろでは 属性名``() =
    labels "bulletml\n    fire |" |> should equal [ "label" ]

  [<Test>]
  member _.``属性値の中では 取れる値``() =
    labels "bulletml\n    direction type=\"|\""
    |> should equal [ "absolute"; "aim"; "relative"; "sequence" ]

  [<Test>]
  member _.``本文の中では 式も出る``() =
    // `#PCDATA` を取る要素の中だけ。XML の `<wait>$|</wait>` に当たる
    labels "bulletml\n    action\n        wait:\"|\"" |> should contain "$rand"
    labels "bulletml\n    action\n        wait:\"|\"" |> should contain "$rank"

  [<Test>]
  member _.``式を取らない要素の中では 式は出ない``() =
    labels "bulletml\n    action\n        fire |" |> should not' (contain "$rand")

  // --- 入れる字 -------------------------------------------------------------

  [<Test>]
  member _.``属性は 値の引用符まで入れて カーソルを中へ``() =
    // **XML と同じ形。** 括弧が無いので手前 を食う必要も無い
    match complete "bulletml\n    fire |" with
    | [ c ] ->
      c.Insert |> should equal "label=\"$0\""
      c.Snippet |> should be True
      c.Replace |> should equal 0
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``打ちかけの属性名は 置き換える``() =
    match complete "bulletml\n    fire lab|" with
    | [ c ] -> c.Replace |> should equal 3
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``式は ドルまで置き換える``() =
    // 食わないと `$` + `$rand` で `$$rand` になる
    let c =
      complete "bulletml\n    action\n        wait:\"$|\""
      |> List.find (fun c -> c.Label = "$rand")
    c.Replace |> should equal 1

  // --- hover ----------------------------------------------------------------

  [<Test>]
  member _.``要素の hover は その表記の書き方で``() =
    // **札にも括弧にもならない。** fsb では名前をそのまま打つ
    match hover "bulletml\n    fi|re" with
    | None -> failwith "浮かなかった"
    | Some md ->
      md |> should startWith "**`fire`**"
      md |> should haveSubstring "```xml"

  [<Test>]
  member _.``属性値の hover は その表記の書き方で``() =
    match hover "bulletml\n    direction type=\"a|im\"" with
    | None -> failwith "浮かなかった"
    | Some md -> md |> should startWith "**`type=\"aim\"`**"

  [<Test>]
  member _.``語彙に無い名前では 浮かない``() =
    hover "bulletml\n    no|pe" |> should equal None
