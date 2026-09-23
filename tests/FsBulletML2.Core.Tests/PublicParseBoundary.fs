namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 公開の読み取り口が「読めなかった」をどう返すか。
[<TestFixture>]
type PublicParseBoundary() =

  [<Test>]
  member _.``PCData を渡すと上がる``() =
    Assert.Throws<FsBulletML2.Exception.BulletmlDTDViolationException>(fun () ->
      BulletmlRead.convertBulletmlFromXmlNode (PCData "ただの文字") |> ignore)
    |> ignore

  [<Test>]
  member _.``根が bulletml でない要素を渡すと上がる``() =
    Assert.Throws<FsBulletML2.Exception.BulletmlDTDViolationException>(fun () ->
      BulletmlRead.convertBulletmlFromXmlNode (Element ("foo", [], [])) |> ignore)
    |> ignore

  /// ここが穴だった。 読めていないのに Some が返っていた
  [<Test>]
  member _.``tryBulletmlFromXmlNode は、読めなければ None を返す``() =
    BulletmlRead.tryBulletmlFromXmlNode (PCData "ただの文字")
    |> should equal (None: Bulletml option)

  [<Test>]
  member _.``tryBulletmlFromXmlNode は、根が bulletml でなくても None を返す``() =
    BulletmlRead.tryBulletmlFromXmlNode (Element ("foo", [], []))
    |> should equal (None: Bulletml option)

  /// 対照 —— 読める入力では中身のある木が返る。
  /// これが無いと、上の 4 本 は「常に上がる / 常に None」でも緑になる
  [<Test>]
  member _.``読める bulletml では、中身のある木が返る``() =
    let xml =
      """<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top"><fire><direction>0</direction><bullet/></fire></action>
</bulletml>"""
    // 腕は bulletml 1 つ だけなので、ここに | other -> は書けない
    let (Bulletml (_, elms)) = readXmlString xml
    elms |> should not' (be Empty)

  /// 空の bulletml は「読めなかった」ではない。
  /// 中身 0 個 の木が返る —— この段では欠落を弾かない、という線引き。
  [<Test>]
  member _.``中身が空の bulletml は、上がらずに中身 0 個 の木が返る``() =
    let xml =
      """<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"></bulletml>"""
    let (Bulletml (_, elms)) = readXmlString xml
    elms |> should be Empty

  /// bulletml の子になれるのは bullet / fire / action だけ。
  /// この 2 本 が無いと、位置の検査は誰にも見られていなかった。
  [<TestCase("wait", "<wait>1</wait>")>]
  [<TestCase("repeat", "<repeat><times>1</times><action><wait>1</wait></action></repeat>")>]
  [<TestCase("actionRef", """<actionRef label="top"/>""")>]
  [<TestCase("vanish", "<vanish/>")>]
  member _.``bulletml の子に、位置の違う命令が来ると上がる``(name: string, child: string) =
    let xml =
      sprintf """<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">%s</bulletml>""" child
    Assert.Throws<FsBulletML2.Exception.BulletmlDTDViolationException>(fun () ->
      readXmlString xml |> ignore)
    |> ignore

  /// 対照 —— 命令ですらない節は上がらずに落ちる。
  /// これが無いと、上の 1 本 は「bulletml の子を全部 上げる」でも緑になる
  [<TestCase("<direction>0</direction>")>]
  [<TestCase("<speed>1</speed>")>]
  [<TestCase("<term>1</term>")>]
  member _.``bulletml の子に、命令でない節が来ても上がらずに落ちる``(child: string) =
    let xml =
      sprintf """<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">%s</bulletml>""" child
    let (Bulletml (_, elms)) = readXmlString xml
    elms |> should be Empty
