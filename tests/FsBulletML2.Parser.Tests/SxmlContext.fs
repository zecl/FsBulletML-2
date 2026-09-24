namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
// Context の腕は表記に依らない（`Scan.fs`）。`contextAt` は表記のモジュールから引く
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.Languages.Sxml

/// カーソルがどこに居るかの判定、sxml の側。`XmlContext` と対。
/// 答えの型は同じで、数え方だけが違う。閉じていない形も見る。
[<TestFixture>]
type SxmlContext() =

    /// `|` の位置をカーソルとして読む。
    /// `@` は属性ブロックの印。同じ印にすると本文そのものが変わる。
    let at (marked: string) =
        let offset = marked.IndexOf '|'
        contextAt (marked.Replace("|", "")) offset

    [<Test>]
    member _.``根の外``() = at "|" |> should equal (InContent None)

    [<Test>]
    member _.``開いている括弧を返す``() =
        at "(bulletml\n  |\n)" |> should equal (InContent(Some "bulletml"))
        at "(bulletml\n(action\n  |\n)\n)" |> should equal (InContent(Some "action"))

        at "(bulletml\n(action\n(fire\n  |\n)\n)\n)"
        |> should equal (InContent(Some "fire"))

    [<Test>]
    member _.``閉じたら 1 つ 外へ戻る``() =
        // XML の「自己閉じは積まない」に当たる形。 sxml は括弧 1 組 が
        // 開きも閉じも兼ねるので、`(bullet)` を積むかどうかの分かれ道が無い
        at "(bulletml\n(action\n(fire)\n|\n)\n)"
        |> should equal (InContent(Some "action"))

        at "(bulletml\n(action\n(bullet)\n|\n)\n)"
        |> should equal (InContent(Some "action"))

    [<Test>]
    member _.``閉じていない途中でも止まらない``() =
        at "(bulletml\n(action\n(fire\n  |" |> should equal (InContent(Some "fire"))

    [<Test>]
    member _.``名前を打っている途中は、置ける要素を出す側``() =
        // `(fi|` は「括弧の中」だが、出したいのは属性ではなく要素名の候補
        at "(bulletml\n(action\n(fi|" |> should equal (InContent(Some "action"))

    [<Test>]
    member _.``開き括弧の直後も、置ける要素を出す側``() =
        // 打った瞬間に候補が出る位置。`(` の次も、空白を挟んだ次も同じ
        at "(bulletml\n(action\n(|" |> should equal (InContent(Some "action"))
        at "(bulletml\n(action\n( |" |> should equal (InContent(Some "action"))

    [<Test>]
    member _.``属性ブロックの中なら属性名``() =
        at "(bulletml\n(action\n(direction (@ |) \"1\")\n)\n)"
        |> should equal (InStartTag "direction")

    [<Test>]
    member _.``属性の括弧を開けたところも属性名``() =
        at "(bulletml\n(action\n(direction (@ (ty|)) \"1\")\n)\n)"
        |> should equal (InStartTag "direction")

    [<Test>]
    member _.``引用符の中なら属性値``() =
        at "(bulletml\n(action\n(direction (@ (type \"|\")) \"1\")\n)\n)"
        |> should equal (InAttrValue("direction", "type"))

    [<Test>]
    member _.``閉じ引用符が無くても属性値``() =
        // XML 側と分かれる唯一の形。捨てると `(type "` まで打った時点で候補が静かに消える。
        at "(bulletml\n(action\n(direction (@ (type \"|"
        |> should equal (InAttrValue("direction", "type"))

    [<Test>]
    member _.``属性ブロックを閉じたら本文へ戻る``() =
        at "(bulletml\n(action\n(direction (@ (type \"aim\")) |)\n)\n)"
        |> should equal (InContent(Some "direction"))

    [<Test>]
    member _.``本文の文字列の中は その要素の中身``() =
        // 式（`$rand`）を出す先。XML の `<wait>1|</wait>` に当たる
        at "(bulletml\n(action\n(wait \"|\")\n)\n)"
        |> should equal (InContent(Some "wait"))

    [<Test>]
    member _.``文字列の中の括弧に騙されない``() =
        // 式は `(180-45)*$rand` のように括弧を書ける。文字列の中を数えると
        // そこで括弧が開いたことになり、以降ずっと中に居ることになる
        at "(bulletml\n(action\n(fire (direction \"(1+2\") (bullet))\n|\n)\n)"
        |> should equal (InContent(Some "action"))
