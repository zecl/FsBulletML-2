namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
// Context の腕は表記に依らない（`Scan.fs`）。`contextAt` は表記のモジュールから引く
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.Languages.Fsb

/// カーソルがどこに居るかの判定、fsb の側。`XmlContext` / `SxmlContext` と対。
///
/// 同じ問いを 3 本 目 の表記で当てる —— 答えの型は同じ（`Context`）で、
/// 数え方だけが違う。
[<TestFixture>]
type FsbContext() =

  /// `|` の位置をカーソルとして読む。fsb に `|` は出てこない
  let at (marked: string) =
    let offset = marked.IndexOf '|'
    contextAt (marked.Replace("|", "")) offset

  [<Test>]
  member _.``根の外``() =
    at "|" |> should equal (InContent None)

  [<Test>]
  member _.``字下げが深いほうが中``() =
    at "bulletml\n    |" |> should equal (InContent(Some "bulletml"))
    at "bulletml\n    action\n        |" |> should equal (InContent(Some "action"))
    at "bulletml\n    action\n        fire\n            |"
    |> should equal (InContent(Some "fire"))

  [<Test>]
  member _.``字下げを戻すと 外へ戻る``() =
    // XML の閉じ札に当たるもの。 fsb には無いので、戻ったこと自体が閉じたこと
    at "bulletml\n    action\n        fire\n    |" |> should equal (InContent(Some "bulletml"))

  [<Test>]
  member _.``同じ字下げは兄弟``() =
    // `action` の中ではなく、`action` の隣
    at "bulletml\n    action\n    |" |> should equal (InContent(Some "bulletml"))

  [<Test>]
  member _.``名前を打っている途中は、置ける要素を出す側``() =
    // `        fi|` は行の上だが、出したいのは属性ではなく要素名の候補
    at "bulletml\n    action\n        fi|" |> should equal (InContent(Some "action"))

  [<Test>]
  member _.``字下げだけの行でも、打った空白の数で決まる``() =
    // 行に名前がまだ無い。 空白の数がそのまま字下げ
    at "bulletml\n    action\n        |" |> should equal (InContent(Some "action"))
    at "bulletml\n    action\n    |" |> should equal (InContent(Some "bulletml"))

  [<Test>]
  member _.``名前の後ろは属性名``() =
    at "bulletml\n    fire |" |> should equal (InStartTag "fire")

  [<Test>]
  member _.``属性名を打っている途中も属性名``() =
    at "bulletml\n    fire lab|" |> should equal (InStartTag "fire")

  [<Test>]
  member _.``引用符の中なら属性値``() =
    at "bulletml\n    direction type=\"|\"" |> should equal (InAttrValue("direction", "type"))

  [<Test>]
  member _.``閉じ引用符が無くても属性値``() =
    // 捨てると `type="` まで打った時点で候補が静かに消える（sxml と同じ理由）
    at "bulletml\n    direction type=\"|" |> should equal (InAttrValue("direction", "type"))

  [<Test>]
  member _.``本文の中は その要素の中身``() =
    // 式（`$rand`）を出す先。XML の `<wait>1|</wait>` に当たる
    at "bulletml\n    wait:\"|\"" |> should equal (InContent(Some "wait"))
    at "bulletml\n    wait:\"|" |> should equal (InContent(Some "wait"))

  [<Test>]
  member _.``属性値の中のコロンに騙されない``() =
    // `xmlns="http://…"` の `:` を本文の区切りと読むと、そこから先が
    // 丸ごと本文になる。根の宣言に必ず在る形なので、外すと全部 ずれる
    at "bulletml xmlns=\"http://a\" type=\"vertical\"\n    |"
    |> should equal (InContent(Some "bulletml"))
    at "bulletml xmlns=\"http://a\" ty|" |> should equal (InStartTag "bulletml")

  [<Test>]
  member _.``閉じ引用符の外は 本文ではない``() =
    // 本文の範囲を閉じ引用符で切っていることを見る。
    // ここは要素の行の上なので属性の場所として返る —— fsb の属性は本文より
    // 前に書くので厳密には打てない位置だが、出るだけで害は無い
    at "bulletml\n    wait:\"30\"|" |> should equal (InStartTag "wait")

  [<Test>]
  member _.``タブは字下げにならない``() =
    // パーサと揃える。 `Offside.fs` の字下げは `pchar ' '` だけ。
    // しかもタブの行はエラーにならず黙って捨てられる
    // （`Offside.parse` は `eof` を要求していない。`FsbReader` が当てている）——
    // ここで広く取ると、捨てられる行に補完だけが出る
    at "bulletml\n\taction\n\t\t|" |> should equal (InContent None)

  [<Test>]
  member _.``CRLF でも同じ``() =
    // 追跡ファイルは CRLF で入っている。`\r` を行に入れると桁が 1 ずれる
    at "bulletml\r\n    action\r\n        |" |> should equal (InContent(Some "action"))
    at "bulletml\r\n    direction type=\"|\"" |> should equal (InAttrValue("direction", "type"))
