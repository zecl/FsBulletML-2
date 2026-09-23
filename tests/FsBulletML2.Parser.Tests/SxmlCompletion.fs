namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Sxml

/// 候補と hover、sxml の側。 `XmlCompletion` / `HoverMarkdown` と対。
/// 語彙は XML と同じ。変わるのは書き方だけで、それがここで当てているもの。
[<TestFixture>]
type SxmlCompletion() =

  static let lang = SxmlLanguage(fun () -> vocab)

  /// `|` の位置をカーソルとして候補を出す（`@` は属性ブロックの印）
  let complete (marked: string) =
    lang.Candidates(marked.Replace("|", ""), marked.IndexOf '|')

  let labels marked =
    complete marked
    // 雛形（v2.6）は外す。 ここが数えているのは「その場所に置ける要素」で、
    // 形の候補はその上に載る別の並び（`Frames.fs` が持ち、`Frames` が当てる）
    |> List.filter (fun c -> not c.IsFrame)
    |> List.map (fun c -> c.Label)
    |> List.sort

  let hover (marked: string) =
    lang.HoverAt(marked.Replace("|", ""), marked.IndexOf '|')

  // --- 候補 -----------------------------------------------------------------

  [<Test>]
  member _.``本文では置ける子要素``() =
    labels "(bulletml\n|\n)" |> should equal [ "action"; "bullet"; "fire" ]
    labels "(bulletml\n(action\n(fire\n|\n)\n)\n)"
    |> should equal [ "bullet"; "bulletRef"; "direction"; "speed" ]

  [<Test>]
  member _.``根の外は bulletml だけ``() =
    labels "|" |> should equal [ "bulletml" ]

  [<Test>]
  member _.``XML と同じ並びが出る``() =
    // 語彙は表記に依らない。 ここが割れたら、どちらかの表記だけが
    // 語彙を持ち始めたということ
    let asXml =
      FsBulletML2.LanguageService.Languages.Xml.XmlLanguage(fun () -> vocab)
        .Candidates("<bulletml>\n\n</bulletml>", 11)
      // 比べる相手も揃える。 `labels` は雛形を外しているので、
      // ここだけ入れると「表記が割れた」でなく「絞り方が割れた」で赤くなる
      |> List.filter (fun c -> not c.IsFrame)
      |> List.map (fun c -> c.Label)
      |> List.sort
    labels "(bulletml\n|\n)" |> should equal asXml

  [<Test>]
  member _.``打っている名前のぶんを置き換える``() =
    complete "(bulletml\n(action\n(fi|" |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 2 ]
    complete "(bulletml\n(action\n(|" |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 0 ]

  [<Test>]
  member _.``属性名は 括弧ごと入れる``() =
    let items = complete "(bulletml\n(action\n(direction (@ |) \"1\")\n)\n)"
    items |> List.map (fun c -> c.Label) |> should equal [ "type" ]
    // `$0` はカーソルの置き場
    items |> List.map (fun c -> c.Insert) |> should equal [ "(type \"$0\")" ]
    items |> List.forall (fun c -> c.Snippet) |> should be True

  [<Test>]
  member _.``属性の置き換えは 開き括弧 を含む``() =
    // 含めないと `((type "")` になる。 `$rand` の `$` と同じ形
    complete "(bulletml\n(action\n(direction (@ (ty|)) \"1\")\n)\n)"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 3 ]
    // 括弧を開けただけなら 1 文字
    complete "(bulletml\n(action\n(direction (@ (|)) \"1\")\n)\n)"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 1 ]
    // 括弧がまだ無ければ 0
    complete "(bulletml\n(action\n(direction (@ |) \"1\")\n)\n)"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 0 ]

  [<Test>]
  member _.``属性値は語彙の並び``() =
    labels "(bulletml\n(action\n(direction (@ (type \"|\")) \"1\")\n)\n)"
    |> should equal [ "absolute"; "aim"; "relative"; "sequence" ]

  [<Test>]
  member _.``中身を取る要素では式も出る``() =
    labels "(bulletml\n(action\n(wait \"|\")\n)\n)" |> should equal [ "$rand"; "$rank" ]

  [<Test>]
  member _.``式の置き換えは ドル記号 を含む``() =
    complete "(bulletml\n(action\n(wait \"$r|\")\n)\n)"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 2 ]
    complete "(bulletml\n(action\n(wait \"3+$ra|\")\n)\n)"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 3 ]

  // --- hover ----------------------------------------------------------------

  [<Test>]
  member _.``要素の hover は sxml の書き方で見出しを出す``() =
    match hover "(bulletml (fi|re))" with
    | None -> failwith "hover が出なかった"
    | Some md ->
      md |> should haveSubstring "(fire)"
      md |> should haveSubstring "弾を 1 つ 撃つ"
      // DTD の行は表記に依らない。 語彙の正本は Core/DTD.fs で、
      // sxml で書いても DTD は DTD のまま
      md |> should haveSubstring "<!ELEMENT fire"

  [<Test>]
  member _.``属性値の hover は sxml の書き方``() =
    match hover "(bulletml (direction (@ (type \"a|im\")) \"1\"))" with
    | None -> failwith "hover が出なかった"
    | Some md ->
      md |> should haveSubstring "(type \"aim\")"
      md |> should haveSubstring "自機のいる向き"

  [<Test>]
  member _.``何の上でもなければ 出さない``() =
    hover "(bulletml (action (wait \"1|2\")))" |> should equal None
    hover "(bulletml (fire|))" |> should equal None

  [<Test>]
  member _.``20 要素 の全部 で 空でない hover が出る``() =
    // 0 件 を緑にしない。 上の点は 1 つ ずつしか見ていない
    let blank =
      [ for e in Vocabulary.elements do
          match lang.HoverAt(sprintf "(%s)" e.Name, 1) with
          | Some md when md.Trim() <> "" && md.Contains e.Spec && e.Spec <> "" -> ()
          | _ -> yield e.Name ]
    blank |> should be Empty
    Vocabulary.elements.Length |> should greaterThan 0
