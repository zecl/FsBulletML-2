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
/// ここは差し替えの前後で比べる相手として置く。**いまは赤でよい** ——
/// 同梱の ProvidedTypes.fs が .NET Framework 時代の SDK なので、型を使う側が
/// FS1108（System.Private.CoreLib への参照が要る）で落ちる。
/// 経緯と直す順番は samples/FsBulletML2.Sample.TypeProviders.Debug/README.md。
///
/// **測り方**（ビルドが通らないので dotnet test では走らない）:
///
///     dotnet build tests/FsBulletML2.TypeProviders.Tests -c Release
///
/// **いまの赤の内訳**（差し替え後にここが 0 になれば成功）:
///
///     FS1108   24 件   型プロバイダが返した型が実行時アセンブリの String を指す
///     FS0039   12 件   その帰結。型生成に失敗して obj に落ちるので Bullet0 が無い
///
/// 2 種類 出るが原因は 1 つ。**片方だけ残ったら、残ったほうは別の原因。**
/// 数まで書いてあるのは、減ったのか形が変わっただけなのかを区別するため。
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

/// **ビルドに入っている型プロバイダは 4 本。** 出す型は 6 通り。
///
///     BulletML<s>              BulletMLTypeProvider          既定は Style.Xml
///     BulletML<s, Style.Sxml>  同上
///     BulletML<s, Style.Fsb>   同上
///     Xml.BulletML<s>          BulletMLFromXmlTypeProvider
///     Sxml.BulletML<s>         BulletMLFromSxmlTypeProvider
///     Fsb.BulletML<s>          BulletMLFromFsbTypeProvider
///
/// **XML<s> と SXML<s> はここに無い。**
/// src/FsBulletML2.TypeProviders に XMLTypeProvider.fs と FSBTypeProvider.fs が
/// 置いてあり、それぞれ XML / SXML という型を出す形に書かれているが、
/// **どちらも fsproj の <Compile Include> に入っていない** ——
/// ディレクトリに .fs が 11 本、fsproj が読むのは 9 本。2 本 が孤児。
///
/// 最初この門に XML<Docs.Xml> / SXML<Docs.Fsb> を書いて、
/// 「型 'XML' が定義されていません」で落ちた。**ディレクトリに在るファイルと
/// build が読むファイルは別の集合**で、孤児のほうは現行の API と合っているかも
/// 確かめられていない（コンパイルされないので）。
///
/// ついでに、その孤児の FSBTypeProvider.fs が出す型の名前は SXML なのに
/// 読むのは .fsb（EndsWith ".fsb" / ReadFsbString）。**名前と実装がずれている。**
/// ビルドに入れるかどうかは配布する API が増える話なので、ここでは触らない。
module Generated =

  type ViaStyleXml  = BulletML<Docs.Xml>
  type ViaStyleSxml = BulletML<Docs.Sxml, Style.Sxml>
  type ViaStyleFsb  = BulletML<Docs.Fsb, Style.Fsb>
  type ViaXml       = Xml.BulletML<Docs.Xml>
  type ViaSxml      = Sxml.BulletML<Docs.Sxml>
  type ViaFsb       = Fsb.BulletML<Docs.Fsb>

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
