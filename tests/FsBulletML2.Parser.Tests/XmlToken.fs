namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// カーソルの下に在るものの判定。hover の中身はこれで決まる。
///
/// `contextAt`（補完）とは向きが違う。あちらは「そこで何を打てるか」なので
/// 手前だけを見るが、こちらは「いま何の上に居るか」なので語の途中でもその語を返す。
///
/// どの変異でも緑のまま —— 境目を入れていなければ全部 通っていた。
[<TestFixture>]
type XmlToken() =

  /// `@` をカーソルの位置として読む。その位置に在る 1 文字を指す
  let at (marked: string) =
    XmlScan.tokenAt (marked.Replace("@", "")) (marked.IndexOf '@')

  [<Test>]
  member _.``要素名の上``() =
    at "<@fire label=\"a\"/>" |> should equal (Element "fire")
    at "<fi@re label=\"a\"/>" |> should equal (Element "fire")
    at "<fir@e label=\"a\"/>" |> should equal (Element "fire")

  [<Test>]
  member _.``開き括弧の上は 何でもない``() =
    // 名前の 1 文字 手前。 ここを名前に含めると、本文の `<` でも
    // 要素の仕様が出る
    at "@<fire label=\"a\"/>" |> should equal Nothing

  [<Test>]
  member _.``名前の 1 文字 先は 何でもない``() =
    // `<fire@ ...` —— 空白の上
    at "<fire@ label=\"a\"/>" |> should equal Nothing

  [<Test>]
  member _.``閉じ括弧の上は 何でもない``() =
    at "<fire label=\"a\"/@>" |> should equal Nothing
    at "<fire label=\"a\"/>@" |> should equal Nothing

  [<Test>]
  member _.``閉じ札の名前も その要素``() =
    // `</fire>` に触っても仕様が出てほしい
    at "<fire></f@ire>" |> should equal (Element "fire")

  [<Test>]
  member _.``属性名の上``() =
    at "<fire l@abel=\"a\"/>" |> should equal (Attribute("fire", "label"))
    at "<fire @label=\"a\"/>" |> should equal (Attribute("fire", "label"))
    at "<fire labe@l=\"a\"/>" |> should equal (Attribute("fire", "label"))

  [<Test>]
  member _.``等号の上は 何でもない``() =
    at "<fire label@=\"a\"/>" |> should equal Nothing

  [<Test>]
  member _.``引用符そのものの上は 何でもない``() =
    // 開きの `"`
    at "<fire label=@\"abc\"/>" |> should equal Nothing
    // 閉じの `"`
    at "<fire label=\"abc@\"/>" |> should equal Nothing

  [<Test>]
  member _.``属性値の上``() =
    at "<fire label=\"@abc\"/>" |> should equal (AttrValue("fire", "label", "abc"))
    at "<fire label=\"ab@c\"/>" |> should equal (AttrValue("fire", "label", "abc"))

  [<Test>]
  member _.``値が空なら 指せる字が無い``() =
    at "<fire label=\"@\"/>" |> should equal Nothing

  [<Test>]
  member _.``2 つ目 の属性も引ける``() =
    at "<direction type=\"a@im\">0</direction>"
    |> should equal (AttrValue("direction", "type", "aim"))

  [<Test>]
  member _.``本文の字は 何でもない``() =
    at "<wait>1@2</wait>" |> should equal Nothing

  [<Test>]
  member _.``タグの外の空白は 何でもない``() =
    at "<a/>@ <b/>" |> should equal Nothing

  [<Test>]
  member _.``コメントの中は 何でもない``() =
    // 中に `>` を先に置く。 置かないと変異が `<!` の枝に吸われて、
    // コメントを飛ばしていなくても同じ答えになる
    at "<bulletml><!-- x > <fi@re/> --></bulletml>" |> should equal Nothing

  [<Test>]
  member _.``CDATA の中は 何でもない``() =
    at "<wait><![CDATA[x > <fi@re/>]]></wait>" |> should equal Nothing

  [<Test>]
  member _.``宣言の中は 何でもない``() =
    at "<?xml ver@sion=\"1.0\"?><bulletml/>" |> should equal Nothing

  [<Test>]
  member _.``引用符の中の 大なり に騙されない``() =
    // 値の中の `>` でタグが終わったことにすると、次の属性の位置がずれる
    at "<action label=\"a>b\" x@=\"1\"><wait>1</wait></action>"
    |> should equal Nothing
    at "<action label=\"a>b\" @x=\"1\"><wait>1</wait></action>"
    |> should equal (Attribute("action", "x"))

  [<Test>]
  member _.``改行をまたいでも位置が合う``() =
    at "<bulletml>\n  <action la@bel=\"top\">\n  </action>\n</bulletml>"
    |> should equal (Attribute("action", "label"))

  [<Test>]
  member _.``空の本文``() =
    XmlScan.tokenAt "" 0 |> should equal Nothing

  [<Test>]
  member _.``本文の外を指しても落ちない``() =
    // Monaco が渡す位置が本文より後ろになることがある
    XmlScan.tokenAt "<fire/>" 999 |> should equal Nothing
    XmlScan.tokenAt "<fire/>" -5 |> should equal Nothing
