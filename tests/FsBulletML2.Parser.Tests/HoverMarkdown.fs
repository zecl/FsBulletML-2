namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Xml

/// **hover が返す markdown の字。**
///
/// Monaco を通さずに当てる。ブラウザでしか見られないのは「浮く位置・幅・
/// 改行の入り方」で、**中身は .NET で全部 当てられる。**
///
/// --- コードフェンスを見る
///
/// markdown は `<` をタグとして食うので、`<!ELEMENT ...>` を素で渡すと
/// 丸ごと消える。**消えても hover は浮く**ので、目でも「出ていない」には
/// 見えない。だからフェンスの**内側**に在ることまで当てる。
///
/// --- 重複させない
///
/// 「置ける子」も「取る値」も「既定」も DTD の行が言っているので、
/// 散文の隣に並べ直さない。属性値のときだけ「省いたときはこれ」を足す
/// （あの行は属性の hover にしか出ないので）。
///
/// --- 語彙は本番と同じ道
///
/// `Vocabulary`（Core の DTD 由来）から作る。JSON は経由しない ——
/// 通しているのは「語彙 -> hover」の道だけ。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   コードフェンスを外す                DTD の行はフェンスの内側に在る
///   要素の hover から ELEMENT を落とす  同上
///   属性の hover から ATTLIST を落とす  属性は ATTLIST の行を出す
///   既定の注記を落とす                  既定の値には 省いたときはこれ が付く
///   何の上でもなくても字を返す          何の上でもなければ 出さない
///   要素の散文を落とす                  20 要素 の全部 で 空でない hover が出る
///
/// **6 通り とも、素が緑のまま自分だけ赤くなる。**
/// フェンスを外す変異は 3 点 を赤にする —— markdown が `<` を食うので、
/// DTD の行がどこからも見えなくなる。
[<TestFixture>]
type HoverMarkdown() =

  // 語彙は `VocabForTests` の 1 本。**ここに写しを持たない** ——
  // 字まで同じものが 2 つ 在ると、片方 だけ古びる
  static let lang = XmlLanguage(fun () -> vocab)

  /// `@` をカーソルの位置として読む
  let hover (marked: string) =
    lang.HoverAt(marked.Replace("@", ""), marked.IndexOf '@')

  let must (marked: string) =
    match hover marked with
    | Some md -> md
    | None -> failwithf "hover が出なかった: %s" marked

  /// フェンスの内側だけを集める。``` で割って、奇数番が内側
  let fenced (md: string) =
    md.Split([| "```" |], System.StringSplitOptions.None)
    |> Array.mapi (fun i s -> i, s)
    |> Array.filter (fun (i, _) -> i % 2 = 1)
    |> Array.map snd
    |> String.concat "\n"

  [<Test>]
  member _.``要素は 散文と DTD の行を出す``() =
    let md = must "<bulletml><fi@re/></bulletml>"
    md |> should haveSubstring "弾を 1 つ 撃つ"
    md |> should haveSubstring "<!ELEMENT fire"

  [<Test>]
  member _.``DTD の行はフェンスの内側に在る``() =
    // **素で渡すと markdown が `<` を食って丸ごと消える。**
    // 消えても hover は浮くので、目では気づけない
    let md = must "<bulletml><fi@re/></bulletml>"
    fenced md |> should haveSubstring "<!ELEMENT fire"

  [<Test>]
  member _.``要素の hover は その属性の ATTLIST も出す``() =
    let md = must "<bulletml><fi@re/></bulletml>"
    fenced md |> should haveSubstring "<!ATTLIST fire label"

  [<Test>]
  member _.``属性は ATTLIST の行を出す``() =
    let md = must "<direction ty@pe=\"aim\">0</direction>"
    md |> should haveSubstring "向きの決め方"
    fenced md |> should haveSubstring "<!ATTLIST direction type"

  [<Test>]
  member _.``属性値は その値の散文を出す``() =
    let md = must "<direction type=\"a@im\">0</direction>"
    md |> should haveSubstring "自機のいる向き"

  [<Test>]
  member _.``既定の値には 省いたときはこれ が付く``() =
    must "<direction type=\"a@im\">0</direction>" |> should haveSubstring "省いたときはこれ"

  [<Test>]
  member _.``既定でない値には 付かない``() =
    // 付いたままだと、どれが既定か読めなくなる
    let md = must "<direction type=\"rel@ative\">0</direction>"
    md |> should haveSubstring "いまの弾の向き"
    md.Contains "省いたときはこれ" |> should equal false

  [<Test>]
  member _.``同じ綴りでも 型が違えば違う散文``() =
    // absolute は direction では画面の絶対角、speed ではその速さ
    let d = must "<direction type=\"abso@lute\">0</direction>"
    let s = must "<speed type=\"abso@lute\">2</speed>"
    d |> should haveSubstring "画面の絶対角"
    s |> should haveSubstring "その速さにする"
    d |> should not' (equal s)

  [<Test>]
  member _.``閉じ札に触っても その要素``() =
    must "<bulletml><fire/></bullet@ml>" |> should haveSubstring "弾幕 1 つ の根"

  [<Test>]
  member _.``何の上でもなければ 出さない``() =
    // **空の字を返さない。** 空でも枠が浮くので、出ていないことと
    // 見分けがつかなくなる
    hover "<wait>1@2</wait>" |> should equal None
    hover "<fire/@>" |> should equal None
    hover "<bulletml><!-- x > <fi@re/> --></bulletml>" |> should equal None

  [<Test>]
  member _.``語彙に無い名前では 出さない``() =
    hover "<bulletml><zz@z/></bulletml>" |> should equal None

  [<Test>]
  member _.``20 要素 の全部 で 空でない hover が出る``() =
    // **0 件 を緑にしない。** 上の点は 1 つ ずつしか見ていないので、
    // 書き忘れた要素が在っても通る
    let blank =
      [ for e in Vocabulary.elements do
          let src = sprintf "<%s/>" e.Name
          match lang.HoverAt(src, 1) with
          | Some md when md.Trim() <> "" && md.Contains e.Spec && e.Spec <> "" -> ()
          | _ -> yield e.Name ]
    blank |> should be Empty
    Vocabulary.elements.Length |> should greaterThan 0
