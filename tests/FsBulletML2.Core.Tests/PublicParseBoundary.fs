namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 公開の読み取り口が「読めなかった」をどう返すか。
///
/// **いまの振る舞いを固定するための門で、これが正しいと言っているわけではない。**
/// 二重木（公開の Bulletml と Rec*）を畳むときにここは必ず触るので、
/// 触ったことが分かるように留めてある。
///
/// いまの形（IntermediateParser の但し書きにも書いてある）:
///
///   convertBulletmlFromXmlNode は「読めなかった」を NotCommand という**値**で返す。
///   tryBulletmlFromXmlNode は**例外だけ**を option に畳むので、読めなかったときは
///   None ではなく Some NotCommand が返る ——「成功したが中身が無い」と
///   見分けが付かない。
///
/// 畳むときに戻り値を option か Result へ変えるなら、**この門が赤くなるのが
/// 正しい。** 赤くなったら、変えたことを確かめてから書き換えること
/// （黙って通らないように、期待値を「NotCommand が返る」と字で書いてある）。
[<TestFixture>]
type PublicParseBoundary() =

  [<Test>]
  member _.``PCData を渡すと NotCommand が返る``() =
    IntermediateParser.convertBulletmlFromXmlNode (PCData "ただの文字")
    |> should equal NotCommand

  [<Test>]
  member _.``根が bulletml でない要素を渡すと NotCommand が返る``() =
    IntermediateParser.convertBulletmlFromXmlNode (Element ("foo", [], []))
    |> should equal NotCommand

  /// **ここが穴。** 読めていないのに Some が返る
  [<Test>]
  member _.``tryBulletmlFromXmlNode は、読めなくても None ではなく Some を返す``() =
    let r = IntermediateParser.tryBulletmlFromXmlNode (PCData "ただの文字")
    r |> should not' (equal None)
    r |> should equal (Some NotCommand)

  /// 対照 —— 読める入力では中身のある木が返る。
  /// **これが無いと、上の 3 本 は「常に NotCommand」でも緑になる**
  [<Test>]
  member _.``読める bulletml では、NotCommand ではない木が返る``() =
    let xml =
      """<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><fire><direction>0</direction><bullet/></fire></action>
</bulletml>"""
    let parsed = readXmlString xml
    parsed |> should not' (equal NotCommand)
    match parsed with
    | Bulletml (_, elms) -> elms |> should not' (be Empty)
    | other -> Assert.Fail (sprintf "Bulletml でない腕が返った: %A" other)
