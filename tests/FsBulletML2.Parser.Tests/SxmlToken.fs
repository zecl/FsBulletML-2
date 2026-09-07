namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// **カーソルの下に在るもの**、sxml の側。`XmlToken` と対。hover が引く。
///
/// `Context` とは向きが違う —— あちらは「そこで何を打てるか」なので手前だけを
/// 見て、名前を打っている途中なら本文扱いにする。こちらは「いま何の上に
/// 居るか」なので、語の途中でもその語を返す。
[<TestFixture>]
type SxmlToken() =

  /// `|` の位置をカーソルとして読む（`@` は属性ブロックの印なので使えない）
  let at (marked: string) =
    SxmlScan.tokenAt (marked.Replace("|", "")) (marked.IndexOf '|')

  [<Test>]
  member _.``要素名の上``() =
    at "(bulletml (|action))" |> should equal (Element "action")
    at "(bulletml (act|ion))" |> should equal (Element "action")
    // 最後の字の上でも返る。`p < NameStop` なので、`n` の位置が境目
    at "(bulletml (actio|n))" |> should equal (Element "action")

  [<Test>]
  member _.``括弧そのものの上は 何でもない``() =
    at "|(fire)" |> should equal Nothing
    at "(fire|)" |> should equal Nothing

  [<Test>]
  member _.``属性名の上``() =
    at "(bulletml (direction (@ (ty|pe \"aim\")) \"1\"))"
    |> should equal (Attribute("direction", "type"))

  [<Test>]
  member _.``属性値の上``() =
    at "(bulletml (direction (@ (type \"a|im\")) \"1\"))"
    |> should equal (AttrValue("direction", "type", "aim"))

  [<Test>]
  member _.``引用符そのものの上は 何でもない``() =
    at "(bulletml (direction (@ (type |\"aim\")) \"1\"))" |> should equal Nothing

  [<Test>]
  member _.``本文の字は 何でもない``() =
    // 属性値と同じ `"…"` だが、**属性ブロックの外**なので何の上でもない
    at "(bulletml (action (wait \"1|2\")))" |> should equal Nothing

  [<Test>]
  member _.``括弧の中の空白は 何でもない``() =
    at "(bulletml (action| (wait \"1\")))" |> should equal Nothing

  [<Test>]
  member _.``内側の括弧を返す``() =
    // **入れ子は位置で覆われている。** 覆っているものを先着で採ると
    // どこに触っても根（bulletml）が当たり、名前の範囲から外れて
    // 「何でもない」になる —— 25 は bullet の名前の中
    SxmlScan.tokenAt "(bulletml (action (fire (bullet))))" 25 |> should equal (Element "bullet")

  [<Test>]
  member _.``空と 範囲の外``() =
    SxmlScan.tokenAt "" 0 |> should equal Nothing
    SxmlScan.tokenAt "(fire)" 999 |> should equal Nothing
    SxmlScan.tokenAt "(fire)" -5 |> should equal Nothing
