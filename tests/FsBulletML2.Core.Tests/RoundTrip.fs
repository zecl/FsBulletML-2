namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsBulletML2
open FsBulletML2.BulletmlRead

/// XML を読んで書き戻したとき、属性が生き残るか。
[<TestFixture>]
type RoundTrip() =

    let ns = "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"

    /// 2 つの経路を別々に見る。`Parser.Tests` はこの両方を一致で当てている
    let roundTrip (source: string) =
        let xml = XmlNode.ReadXmlString source
        let bml = xml |> convertBulletmlFromXmlNode
        let viaDtd = bml.ToXmlStringForTest()
        let viaXmlNode = xml.ToXmlString()

        sprintf
            "  入力      %s\n  DTD 経由  %s  %s\n  XmlNode   %s  %s"
            source
            viaDtd
            (if viaDtd = source then "一致" else "ちがう")
            viaXmlNode
            (if viaXmlNode = source then "一致" else "ちがう")

    [<Test>]
    member _.``bulletml の属性は往復で生き残るか``() =
        let attrs =
            [
                ""
                " name=\"No Name\""
                " description=\"a sample\""
                " name=\"No Name\" description=\"a sample\""
            ]

        [
            yield "既存の TestCase 2 本が持つのは xmlns と type だけ。足すとどうなるか"
            yield ""
            for a in attrs do
                yield roundTrip (sprintf """<bulletml xmlns="%s" type="vertical"%s><bullet /></bulletml>""" ns a)
                yield ""
        ]
        |> String.concat "\n"
        |> Golden.check "now-roundtrip-bulletml-attrs"
