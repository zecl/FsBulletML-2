namespace FsBulletML2.TypeProviders.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.TypeProviders

/// 型プロバイダが**型プロバイダとして働く**かを見る門。
///
/// 本体（src/FsBulletML2.TypeProviders）が sln に入っていて確かめられるのは
/// 「ビルドが通る」ことだけで、**生成した型が使えるかを見ている門は無かった。**
/// nuspec と nuget/*.package.bat がある配布物なのに。
///
/// **いまは緑。** 置いたときは赤で、その赤を消すために SDK を差し替えた。
/// 経緯は samples/FsBulletML2.Sample.TypeProviders.Debug/README.md。
///
/// 赤から緑へ動いた段を、数で控えてある。**次に赤くなったとき、どの段まで
/// 戻ったかを引き算で言うため。**
///
///     置いたとき        FS1108 24 件 / FS0039 12 件
///     SDK を 8.11.0 へ  FS1108 0 件。かわりに FS3033 が 16 件
///     probing を直した  0 件。テスト 6 本 緑
///
/// **段が 2 つ あった。** 型が解決できない段（FS1108。同梱 SDK が
/// .NET Framework 時代のもので、返す型が実行時アセンブリの String を指していた）
/// を直したら、次に**設計時に依存アセンブリを読めない段**（FS3033。
/// FParsec.dll が型プロバイダの隣に無く、probing も効いていなかった）が出た。
///
/// 1 段目 だけ見て「直った」と言える形ではなかった。**門を先に置いて
/// 数を控えていたから、2 段目 が「新しい壊れ」ではなく「次の段」だと分かった。**
///
/// **ファイルを置いていない。** 型プロバイダは拡張子で終わらない文字列を
/// 「BulletML の本体そのもの」として受けるので、ここでは文字列リテラルを渡す。
/// TypeProviderConfig.ResolutionFolder の解決を門の対象から外して、
/// **型が生成されるかどうかだけ**を見るため。ファイル解決は別の門の仕事。
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

/// **型プロバイダは 6 本。出す型は 8 通り。全部 ここに載せる。**
///
///     BulletML<s>              BulletMLTypeProvider          既定は Style.Xml
///     BulletML<s, Style.Sxml>  同上
///     BulletML<s, Style.Fsb>   同上
///     Xml.BulletML<s>          BulletMLFromXmlTypeProvider
///     Sxml.BulletML<s>         BulletMLFromSxmlTypeProvider
///     Fsb.BulletML<s>          BulletMLFromFsbTypeProvider
///     XML<s>                   XMLTypeProvider.fs            Value を 1 つ 出す
///     SXML<s>                  FSBTypeProvider.fs            **中身は fsb**
///
/// 最後の 1 行 は打ち間違いではない。FSBTypeProvider.fs が出す型の名前が SXML で、
/// 読むのは .fsb（EndsWith ".fsb" / ReadFsbString）。**名前と実装がずれている。**
/// 直すと使う側が壊れるので、いまは現状のまま門に載せて、ずれを型で固定しておく。
///
/// **下の 2 本 は、少し前まで存在しない型だった。** XMLTypeProvider.fs と
/// FSBTypeProvider.fs はディレクトリに在るのに fsproj の <Compile Include> に
/// 入っておらず、この門に書いたら「型 'XML' が定義されていません」で落ちた。
/// ビルドに入れたときに 3 か所 直している ——
///
///     読む API が古いまま      FsBulletML2.Xml.Bulletml.readXmlString (xml, None)
///     読まれない束縛が 1 つ    Bulletml(...) を組んで捨てるだけの bulletml2
///     HideObjectMethods <-     新 SDK では読み取り専用。引数で渡すほうへ
///
/// **コンパイルされないファイルは、周りが動いても古びたまま残る。**
/// いま門に載っているので、次にずれたらここが赤くなる。
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

  /// **名前は SXML だが、読むのは fsb。** FSBTypeProvider.fs の実装がそうなっている。
  /// ここに xml を渡すと落ちるのが正しい振る舞い
  [<Test>]
  member _.``SXML<s>: 名前に反して fsb を読む``() =
    let g = Generated.ViaFsbSingle()
    g.Value.Name |> should equal (Some "門の弾")
