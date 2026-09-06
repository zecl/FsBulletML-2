namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Playground
open FsBulletML2.Playground.SourceLanguage
open FsBulletML2.Playground.Languages.Xml

/// **候補づくりを機械で回す。** ブラウザでも当てられるが、そこは CI では
/// 動かない（背面タブは rAF も layout も止まる）ので、ここが唯一の道。
///
/// 語彙は `Vocabulary`（Core の DTD 由来）をそのまま渡す。JSON は経由しない ——
/// **通しているのは「語彙 -> 候補」の道だけ。** JSON の往復は別で、
/// 形が食い違えば候補が 0 になり起動時に赤が出る。
[<TestFixture>]
type XmlCompletion() =

  static let vocab: Vocab =
    { Elements =
        Vocabulary.elements
        |> Array.toList
        |> List.map (fun e ->
             { Name = e.Name
               Children = List.ofArray e.Children
               Text = e.Text
               Attrs =
                 e.Attrs
                 |> Array.toList
                 |> List.map (fun a -> { Name = a.Name; Values = List.ofArray a.Values }) })
      Expressions = List.ofArray Vocabulary.expressions }

  static let lang = XmlLanguage(fun () -> vocab)

  /// `@` の位置をカーソルとして候補を出す
  let complete (marked: string) =
    lang.Candidates(marked.Replace("@", ""), marked.IndexOf '@')

  let labels marked = complete marked |> List.map (fun c -> c.Label) |> List.sort

  [<Test>]
  member _.``本文では置ける子要素``() =
    labels "<bulletml>\n@\n</bulletml>" |> should equal [ "action"; "bullet"; "fire" ]
    labels "<bulletml>\n<action>\n<fire>\n@\n</fire>\n</action>\n</bulletml>"
    |> should equal [ "bullet"; "bulletRef"; "direction"; "speed" ]

  [<Test>]
  member _.``根の外は bulletml だけ``() =
    labels "@" |> should equal [ "bulletml" ]

  [<Test>]
  member _.``打っている名前のぶんを置き換える``() =
    // **Monaco の語の定義に頼らない。** ここが 0 のままだと、
    // 打った字の後ろに候補が継ぎ足される（`<fifire>`）
    complete "<bulletml>\n<action>\n<fi@" |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 2 ]
    complete "<bulletml>\n<action>\n@" |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 0 ]

  [<Test>]
  member _.``属性名は 等号と引用符 まで入れる``() =
    let items = complete "<bulletml>\n<action>\n<direction @>1</direction>\n</action>\n</bulletml>"
    items |> List.map (fun c -> c.Label) |> should equal [ "type" ]
    // `$0` はカーソルの置き場。名前だけ入れると必ず手で 3 文字 足すことになる
    items |> List.map (fun c -> c.Insert) |> should equal [ "type=\"$0\"" ]
    items |> List.forall (fun c -> c.Snippet) |> should be True

  [<Test>]
  member _.``属性値は語彙の並び``() =
    labels "<bulletml>\n<action>\n<direction type=\"@\">1</direction>\n</action>\n</bulletml>"
    |> should equal [ "absolute"; "aim"; "relative"; "sequence" ]

  [<Test>]
  member _.``中身を取る要素では式も出る``() =
    // wait は子を持たないので、出るのは式だけ
    labels "<bulletml>\n<action>\n<wait>@</wait>\n</action>\n</bulletml>"
    |> should equal [ "$rand"; "$rank" ]
    // param も #PCDATA（型は string list なので reflection では見えない）
    labels "<bulletml>\n<action>\n<actionRef label=\"a\"><param>@</param></actionRef>\n</action>\n</bulletml>"
    |> should equal [ "$rand"; "$rank" ]

  [<Test>]
  member _.``中身を取らない要素では式は出ない``() =
    labels "<bulletml>\n@\n</bulletml>" |> should not' (contain "$rand")

  [<Test>]
  member _.``式の置き換えは ドル記号 を含む``() =
    // 含めないと `$` の後ろに `$rand` が付いて `$$rand` になる
    complete "<bulletml>\n<action>\n<wait>$r@</wait>\n</action>\n</bulletml>"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 2 ]
    complete "<bulletml>\n<action>\n<wait>3+$ra@</wait>\n</action>\n</bulletml>"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 3 ]
    complete "<bulletml>\n<action>\n<wait>$@</wait>\n</action>\n</bulletml>"
    |> List.map (fun c -> c.Replace) |> List.distinct
    |> should equal [ 1 ]

  [<Test>]
  member _.``式の候補は Parser が読める字である``() =
    // **綴りは DU から引けない**（読む側が文字で持っている）ので、
    // 表と Parser がずれていないかをここで当てる
    for token in Vocabulary.expressions do
      let e = Expr.NumExpr.ofString token
      e.Ast |> should not' (equal Expr.Node.Invalid)
      (e.NeedRand || e.NeedRank) |> should be True

  [<Test>]
  member _.``読めない字は Parser が弾く（上の点が当たっている証拠）``() =
    // 上の点が「何を入れても緑」でないこと
    let e = Expr.NumExpr.ofString "$rnd"
    (e.Ast = Expr.Node.Invalid || not (e.NeedRand || e.NeedRank)) |> should be True
