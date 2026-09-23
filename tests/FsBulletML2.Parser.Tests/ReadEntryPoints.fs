namespace FsBulletML2.Parser.Tests

open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2

/// 読む口 12 本の門。中身の parser ではなく、その手前の糊を見る。
/// 繋ぎ間違えても型は通る。`read*` は例外、`tryRead*` は None。
[<TestFixture>]
type ReadEntryPoints() =

  let path (ext: string) =
    resolveTestPath (sprintf @"..\..\..\TestData\%s\accel\elements\success\accel-horizontal-exist.%s" ext ext)

  let text (ext: string) = File.ReadAllText(path ext)

  /// 読めるが DTD に反する（accel に term が無い）。XML としては正しいので、
  /// XML の構文エラーではなく「木にできない」側で落ちる
  let dtdViolation =
    """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical">
  <action label="top"><accel><horizontal>2</horizontal></accel></action>
</bulletml>"""

  [<Test>]
  member _.``3 つ の書式が同じ木に着く（文字列から）``() =
    let xml = Bulletml.readXmlString (text "xml")
    Bulletml.readSxmlString (text "sxml") |> should equal xml
    Bulletml.readFsbString (text "fsb") |> should equal xml

  [<Test>]
  member _.``3 つ の書式が同じ木に着く（ファイルから）``() =
    let xml = Bulletml.readXmlString (text "xml")
    Bulletml.readXml (path "xml") |> should equal xml
    Bulletml.readSxml (path "sxml") |> should equal xml
    Bulletml.readFsb (path "fsb") |> should equal xml

  [<Test>]
  member _.``tryRead は同じ木を Some で返す``() =
    let xml = Bulletml.readXmlString (text "xml")
    Bulletml.tryReadXmlString (text "xml") |> should equal (Some xml)
    Bulletml.tryReadSxmlString (text "sxml") |> should equal (Some xml)
    Bulletml.tryReadFsbString (text "fsb") |> should equal (Some xml)
    Bulletml.tryReadXml (path "xml") |> should equal (Some xml)
    Bulletml.tryReadSxml (path "sxml") |> should equal (Some xml)
    Bulletml.tryReadFsb (path "fsb") |> should equal (Some xml)

  /// read と tryRead の分かれ目。 ここが崩れると、読めない弾幕を
  /// 黙って None にする経路と、落として知らせる経路が入れ替わる
  [<Test>]
  member _.``読めない XML は read が落ち、tryRead は None``() =
    Assert.Catch(fun () -> Bulletml.readXmlString dtdViolation |> ignore) |> ignore
    Bulletml.tryReadXmlString dtdViolation |> should equal None

  [<Test>]
  member _.``読めない sxml と fsb も同じ分かれ方``() =
    let garbage = "(this is not sxml"
    Assert.Catch(fun () -> Bulletml.readSxmlString garbage |> ignore) |> ignore
    Bulletml.tryReadSxmlString garbage |> should equal None
    Assert.Catch(fun () -> Bulletml.readFsbString garbage |> ignore) |> ignore
    Bulletml.tryReadFsbString garbage |> should equal None

  /// ファイルから読む側の失敗枝。文字列版とは別の関数なので、
  /// 片方だけ try を取り違えても文字列版の門は緑のまま
  [<Test>]
  member _.``ファイルから読めないときも read は落ち、tryRead は None``() =
    let tmp = Path.Combine(Path.GetTempPath(), "fsbulletml2-broken.txt")
    File.WriteAllText(tmp, "(this is not sxml")
    try
      Assert.Catch(fun () -> Bulletml.readSxml tmp |> ignore) |> ignore
      Bulletml.tryReadSxml tmp |> should equal None
      Assert.Catch(fun () -> Bulletml.readFsb tmp |> ignore) |> ignore
      Bulletml.tryReadFsb tmp |> should equal None
    finally
      File.Delete tmp

  /// C# から見える口。module の関数とは別の実体（型拡張の static member）
  /// なので、片方だけ繋ぎ変えても気づけない
  [<Test>]
  member _.``static member は module の関数と同じ答えを返す``() =
    let xml = Bulletml.readXmlString (text "xml")
    Bulletml.ReadXmlString (text "xml") |> should equal xml
    Bulletml.TryReadXmlString (text "xml") |> should equal (Some xml)
    Bulletml.ReadSxmlString (text "sxml") |> should equal xml
    Bulletml.TryReadSxmlString (text "sxml") |> should equal (Some xml)
    Bulletml.ReadSxml (path "sxml") |> should equal xml
    Bulletml.TryReadSxml (path "sxml") |> should equal (Some xml)
    Bulletml.ReadFsbString (text "fsb") |> should equal xml
    Bulletml.TryReadFsbString (text "fsb") |> should equal (Some xml)
    Bulletml.ReadFsb (path "fsb") |> should equal xml
    Bulletml.TryReadFsb (path "fsb") |> should equal (Some xml)

  /// 書き出しの往復。完全に戻るのは ForTest だけ。
  /// `ToXmlString` は定数を畳むので木が変わる。取り違えると往復の意味が変わる。
  [<Test>]
  member _.``往復で完全に戻るのは ForTest のほう``() =
    let xml = Bulletml.readXmlString (text "xml")
    Bulletml.readXmlString (xml.ToXmlStringForTest()) |> should equal xml
    Bulletml.readXmlString (xml.ToIndentedXmlStringForTest()) |> should equal xml
    let folded = Bulletml.readXmlString (xml.ToXmlString())
    folded |> should not' (equal xml)
    // 畳んだ結果をもう一度 畳んでも動かない
    Bulletml.readXmlString (folded.ToXmlString()) |> should equal folded

  /// DOCTYPE を出す枝。既定では出ないので、切り替えが効いていることを見る
  [<Test>]
  member _.``EncodingAndDoctype を Exist にすると DOCTYPE が出る``() =
    let xml = Bulletml.readXmlString (text "xml")
    xml.ToXmlStringForTest() |> should not' (haveSubstring "DOCTYPE")
    let withDoctype = xml.ToXmlStringForTest(EncodingAndDoctype.Exist)
    withDoctype |> should haveSubstring "DOCTYPE"
    withDoctype |> should haveSubstring "bulletml.dtd"
    // 読み直せることも見る。DOCTYPE を付けた結果が読めなければ意味が無い
    Bulletml.readXmlString withDoctype |> should equal xml

  /// 省略引数つきの多重定義。引数なしの版とは別の実体で、既定値を
  /// 埋める行がそれぞれに居る
  [<Test>]
  member _.``省略引数つきの ToXmlString も同じものを書く``() =
    let xml = Bulletml.readXmlString (text "xml")
    xml.ToXmlString(EncodingAndDoctype.Nothing) |> should equal (xml.ToXmlString())
    xml.ToIndentedXmlString(4, EncodingAndDoctype.Nothing)
    |> should equal (xml.ToIndentedXmlString())
    // Exist を渡した側だけ DOCTYPE が出る
    xml.ToXmlString(EncodingAndDoctype.Exist) |> should haveSubstring "DOCTYPE"
    xml.ToIndentedXmlString(2, EncodingAndDoctype.Exist) |> should haveSubstring "DOCTYPE"
