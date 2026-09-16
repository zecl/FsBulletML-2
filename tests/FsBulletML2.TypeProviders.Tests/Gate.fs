namespace FsBulletML2.TypeProviders.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.TypeProviders

/// 型プロバイダが型プロバイダとして働くかを見る門。
///
/// 本体（src/FsBulletML2.TypeProviders）が sln に入っていて確かめられるのは
/// 「ビルドが通る」ことだけで、生成した型が使えるかを見ている門は無かった。
///
///     置いたとき        FS1108 24 件 / FS0039 12 件
///     SDK を 8.11.0 へ  FS1108 0 件。かわりに FS3033 が 16 件
///
/// 段が 2 つ あった。 型が解決できない段（FS1108。同梱 SDK が
///
/// を直したら、次に設計時に依存アセンブリを読めない段（FS3033。
module Docs =

  [<Literal>]
  let Xml = """<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical" name="門の弾"><action label="top"><fire><direction>0</direction><bullet/></fire></action></bulletml>"""

  [<Literal>]
  let Sxml = """(bulletml (@ (xmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml") (type "vertical") (name "門の弾"))
    (action (@ (label "top"))
        (fire
            (direction "0")
            (bullet)
        )
    )
)"""

  [<Literal>]
  let Fsb = """bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical" name="門の弾"
    action label="top"
        fire
            direction:"0"
            bullet"""

/// 型プロバイダは 6 本。出す型は 8 通り。全部 ここに載せる。
///
///     BulletML<s>              BulletMLTypeProvider          既定は Style.Xml
///     BulletML<s, Style.Sxml>  同上
///
/// 直すと使う側が壊れるので、いまは現状のまま門に載せて、ずれを型で固定しておく。
module Generated =

  type ViaStyleXml  = BulletML<Docs.Xml>
  type ViaStyleSxml = BulletML<Docs.Sxml, Style.Sxml>
  type ViaStyleFsb  = BulletML<Docs.Fsb, Style.Fsb>
  type ViaXml       = Xml.BulletML<Docs.Xml>
  type ViaSxml      = Sxml.BulletML<Docs.Sxml>
  type ViaFsb       = Fsb.BulletML<Docs.Fsb>
  type ViaXmlSingle = XML<Docs.Xml>
  type ViaFsbSingle = SXML<Docs.Fsb>

[<TestFixture>]
type TypeProviderGate() =

  /// 文字列を渡したときの名前は Bullet{i}（Impl.getBulletmlInfo）。
  /// 1 つ しか渡していないので i = 0
  [<Test>]
  member _.``BulletML<s>: 既定の Style.Xml で、生成した型からプロパティが読める``() =
    let g = Generated.ViaStyleXml()
    g.Bullet0.Name |> should equal (Some "門の弾")

  [<Test>]
  member _.``BulletML<s, Style.Sxml>: S 式 から読める``() =
    let g = Generated.ViaStyleSxml()
    g.Bullet0.Name |> should equal (Some "門の弾")

  [<Test>]
  member _.``BulletML<s, Style.Fsb>: fsb から読める``() =
    let g = Generated.ViaStyleFsb()
    g.Bullet0.Name |> should equal (Some "門の弾")

  [<Test>]
  member _.``Xml.BulletML<s>: 別名の型プロバイダからも読める``() =
    let g = Generated.ViaXml()
    g.Bullet0.Name |> should equal (Some "門の弾")

  [<Test>]
  member _.``Sxml.BulletML<s>: 別名の型プロバイダからも読める``() =
    let g = Generated.ViaSxml()
    g.Bullet0.Name |> should equal (Some "門の弾")

  [<Test>]
  member _.``Fsb.BulletML<s>: 別名の型プロバイダからも読める``() =
    let g = Generated.ViaFsb()
    g.Bullet0.Name |> should equal (Some "門の弾")

  /// XML<s> と SXML<s> はプロパティが Value 1 つ だけ（上の 6 通り とは形が違う）
  [<Test>]
  member _.``XML<s>: Value から読める``() =
    let g = Generated.ViaXmlSingle()
    g.Value.Name |> should equal (Some "門の弾")

  /// 名前は SXML だが、読むのは fsb。 FSBTypeProvider.fs の実装がそうなっている。
  /// ここに xml を渡すと落ちるのが正しい振る舞い
  [<Test>]
  member _.``SXML<s>: 名前に反して fsb を読む``() =
    let g = Generated.ViaFsbSingle()
    g.Value.Name |> should equal (Some "門の弾")
