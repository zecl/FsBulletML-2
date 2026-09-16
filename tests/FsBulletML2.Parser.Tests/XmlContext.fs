namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
// Context の腕は `XmlScan`（字を数える 1 本）が持つ。
// `contextAt` は表記のモジュールから引く
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.Languages.Xml

/// カーソルがどこに居るかの判定。補完の候補はこれで決まる。
///
/// ブラウザで当てることはできるが、CI では回らない（rAF も layout も
/// 止まっている背面タブでしか見られない）。ここが唯一 機械が回す道。
///
/// 実際に 1 個 出た —— 走査がカーソルを越えて末尾まで進み、後ろの閉じタグまで
/// 札を下ろしていた。属性の補完は当たるので、本文の補完だけ静かに外れる
/// という形だった。数を見ないと気づけない。
///
/// 打っている途中の XML は必ず壊れているので、閉じていない形も見る。
[<TestFixture>]
type XmlContext() =

  /// `@` の位置をカーソルとして読む
  let at (marked: string) =
    let offset = marked.IndexOf '@'
    contextAt (marked.Replace("@", "")) offset

  [<Test>]
  member _.``根の外``() =
    at "@" |> should equal (InContent None)

  [<Test>]
  member _.``開いている要素を返す``() =
    at "<bulletml>\n  @\n</bulletml>" |> should equal (InContent(Some "bulletml"))
    at "<bulletml>\n<action>\n  @\n</action>\n</bulletml>" |> should equal (InContent(Some "action"))
    at "<bulletml>\n<action>\n<fire>\n  @\n</fire>\n</action>\n</bulletml>"
    |> should equal (InContent(Some "fire"))

  [<Test>]
  member _.``閉じたら 1 つ 外へ戻る``() =
    at "<bulletml>\n<action>\n<fire></fire>\n@\n</action>\n</bulletml>"
    |> should equal (InContent(Some "action"))

  [<Test>]
  member _.``自己閉じは積まない``() =
    // ここを見落とすと、`<bullet/>` から先がずっと bullet の中に居ることになる
    at "<bulletml>\n<action>\n<bullet/>\n@\n</action>\n</bulletml>"
    |> should equal (InContent(Some "action"))

  [<Test>]
  member _.``閉じていない途中でも止まらない``() =
    at "<bulletml>\n<action>\n<fire>\n  @" |> should equal (InContent(Some "fire"))

  [<Test>]
  member _.``要素名を打っている途中は、置ける要素を出す側``() =
    // `<act|` は「タグの中」だが、出したいのは属性ではなく要素名の候補
    at "<bulletml>\n<action>\n<fi@" |> should equal (InContent(Some "action"))

  [<Test>]
  member _.``タグの中なら属性名``() =
    at "<bulletml>\n<action>\n<direction @>1</direction>\n</action>\n</bulletml>"
    |> should equal (InStartTag "direction")

  [<Test>]
  member _.``引用符の中なら属性値``() =
    at "<bulletml>\n<action>\n<direction type=\"@\">1</direction>\n</action>\n</bulletml>"
    |> should equal (InAttrValue("direction", "type"))

  // `>` は属性値の中に書ける。 タグの終わりを探すときに引用符を見ないと、
  // そこで終わったことになる。
  //
  // 下の 2 点 は、引用符を見なくしても答えが変わらない形を先に書いてしまい、
  // 変異が緑のまま通ったので置き直したもの。カーソルが「はみ出した `>` より
  // 先」に居る形でないと当たらない。
  [<Test>]
  member _.``属性値の中の 山括弧 でタグが終わったことにしない``() =
    // 終わったことにすると、まだタグの中に居るカーソルが「本文」に見える
    at "<bulletml>\n<action label=\"a>b\" @>\n</action>\n</bulletml>"
    |> should equal (InStartTag "action")

  [<Test>]
  member _.``属性値の中の スラッシュ山括弧 を自己閉じと読まない``() =
    // 自己閉じと読むと札に積まれず、中に居るのに外に居ることになる
    at "<bulletml>\n<action label=\"x/>\">\n@\n</action>\n</bulletml>"
    |> should equal (InContent(Some "action"))

  [<Test>]
  member _.``宣言とコメントは積まない``() =
    at "<?xml version=\"1.0\" ?>\n<!-- memo -->\n<bulletml>\n@\n</bulletml>"
    |> should equal (InContent(Some "bulletml"))
