namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// カーソルの下に在るものの判定、fsb の側。`XmlToken` / `SxmlToken` と対。
/// hover が引く。何でもなければ何も浮かない。
[<TestFixture>]
type FsbToken() =

    /// `|` の位置をカーソルとして読む
    let at (marked: string) =
        let offset = marked.IndexOf '|'
        FsbScan.tokenAt (marked.Replace("|", "")) offset

    [<Test>]
    member _.``要素名の上``() =
        at "bulletml\n    |action label=\"top\"" |> should equal (Element "action")
        // 最後の字も同じ要素
        at "bulletml\n    actio|n label=\"top\"" |> should equal (Element "action")

    [<Test>]
    member _.``属性名の上``() =
        at "bulletml\n    action la|bel=\"top\""
        |> should equal (Attribute("action", "label"))

    [<Test>]
    member _.``属性値の上``() =
        at "bulletml\n    direction type=\"a|im\""
        |> should equal (AttrValue("direction", "type", "aim"))

    [<Test>]
    member _.``引用符そのものは何でもない``() =
        at "bulletml\n    direction type=|\"aim\"" |> should equal Nothing
        at "bulletml\n    direction type=\"aim|\"" |> should equal Nothing

    [<Test>]
    member _.``等号は何でもない``() =
        at "bulletml\n    action label|=\"top\"" |> should equal Nothing

    [<Test>]
    member _.``字下げの空白は何でもない``() =
        at "bulletml\n  |  action" |> should equal Nothing

    [<Test>]
    member _.``本文は何でもない``() =
        // XML の #PCDATA と同じ扱い。候補は出るが、hover に出すものは無い
        at "bulletml\n    wait:\"3|0\"" |> should equal Nothing

    [<Test>]
    member _.``空行は何でもない``() =
        at "bulletml\n|\n    action" |> should equal Nothing

    [<Test>]
    member _.``本文の区切りに 名前を食われない``() =
        // `:` を名前の字として数えると、要素名が `direction:` になって
        // 語彙に無い名前になる（hover が静かに出なくなる）
        at "bulletml\n    dire|ction:\"-20\"" |> should equal (Element "direction")

        FsbScan.tags "bulletml\n    direction:\"-20\""
        |> List.map (fun t -> t.TagName)
        |> should equal [ "bulletml"; "direction" ]

    [<Test>]
    member _.``属性値の中のコロンに騙されない``() =
        at "bulletml xmlns=\"http:|//a\" type=\"vertical\""
        |> should equal (AttrValue("bulletml", "xmlns", "http://a"))

        at "bulletml xmlns=\"http://a\" ty|pe=\"vertical\""
        |> should equal (Attribute("bulletml", "type"))

    [<Test>]
    member _.``CRLF でも 値に改行が混ざらない``() =
        // `\r` を行に入れると、閉じ引用符の無い値がそれを飲む
        match FsbScan.tags "bulletml\r\n    action label=\"top\"\r\n" with
        | [ _; action ] ->
            action.Attrs |> List.map (fun a -> a.Value) |> should equal [ "top" ]
            action.Attrs |> List.map (fun a -> a.Line) |> should equal [ 2 ]
        | other -> failwithf "2 本 のはずが %d 本" other.Length
