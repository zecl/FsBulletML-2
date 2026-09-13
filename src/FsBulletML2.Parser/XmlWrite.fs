namespace FsBulletML2

open System.IO
open System.Text
open System.Xml

/// **弾幕の木を XML の字にするところ。**
///
/// 以前は `Core` の `DTD.fs` に居た（`BulletmlXml.XmlSink` ほか）。こちらへ移したのは
/// 2 つ の理由が同じ向きを指したから ——
///
///   - **受け口はその表記を読むパーサと同じところに置く**（`DTD.IBulletmlSink` の
///     コメントに書いてある決め）。sxml の受け口は `Sxml.fs`、fsb の受け口は
///     `Offside.fs` に在って、xml だけが Core に残っていた
///   - **`System.Xml` は Fable に無い。** Fable は proj まるごとしか焼けないので、
///     Core に 1 か所 でも在ると Core ごと焼けなくなる
///
/// 歩き（`DTD.BulletmlXml.writeContentTo`）は Core のまま。**あちらは表記を知らない。**
[<AutoOpen>]
module internal BulletmlXmlWrite =

  /// XML の受け口。**`XmlWriter` を包むだけ** ——
  /// エスケープも字下げもあちらが持っている
  type private XmlSink(writer: XmlWriter) =
    interface DTD.IBulletmlSink with
      member _.Start name = writer.WriteStartElement name
      member _.Attr(name, value) = writer.WriteAttributeString(name, value)
      member _.Text value = writer.WriteString value
      member _.End() = writer.WriteEndElement()

  let getXmlString formatting (encdoc: EncodingAndDoctype) indentation (this: Bulletml) =
    let output = new StringBuilder()
    let sw =
      { new StringWriter(output) with
        override this.Encoding with get () = Encoding.UTF8 }
    sw.NewLine <- "\r\n"

    use writer = new XmlTextWriter(sw, Formatting=formatting, Indentation = indentation)
    encdoc |> function
    | Nothing -> ()
    // **DOCTYPE の名前と SYSTEM id は `Xml.fs` の [<Literal>] を使う。**
    // Core に在ったころは同じ字がここにも書いてあった —— 同じ値が 2 か所 に
    // 在ると、片方 だけ直したときに黙って食い違う
    | Exist -> writer.WriteStartDocument()
               writer.WriteDocType(Xml.docType, null, Xml.sysid, null)

    DTD.BulletmlXml.writeContentTo (XmlSink(writer)) this
    output.ToString()

  let toXmlString (encodingAndDoctype: EncodingAndDoctype) (this: Bulletml) =
    getXmlString Formatting.None encodingAndDoctype 0 this

  let toIndentedXmlString (indentation: int) (encodingAndDoctype: EncodingAndDoctype) (this: Bulletml) =
    getXmlString Formatting.Indented encodingAndDoctype indentation this

/// **弾幕の木を歩いて、受け口へ流す 1 本。表記を知らない。**
///
/// XML を書くのも S 式 を書くのも インデント記法 を書くのも、歩きはこれ 1 本。
/// 違うのは受け口だけ —— **表記が増えても、ここは増えない。**
///
/// F# の CE だけはこの形に乗らない（要素名ではなく DSL の名前で書くので、
/// 木の形がそのまま字にならない）。あちらは別の口。
///
/// 歩きの実体は `Core` の `DTD.BulletmlXml.writeContentTo`。**この module が
/// Core ではなくここに在るのは、下の `toIndentedXml` が xml を書くから。**
module BulletmlWriter =

  let writeTo (sink: DTD.IBulletmlSink) (bulletml: Bulletml) =
    DTD.BulletmlXml.writeContentTo sink bulletml

  /// XML の字にする。**定数を畳まない。**
  ///
  /// `Bulletml.ToIndentedXmlString`（`Parser.fs`）は `foldConstants` を通す ——
  /// `8` が `8.0000000000` になる。**表記を行き来する用途ではそれが困る**
  /// （`<wait>8</wait>` を sxml にして戻すと字が化ける）。
  ///
  /// 人が書いた字を保つ側が要るので、こちらを開けてある。
  /// 同梱カタログを焼くのは畳む側のまま（v1.4 より前 からの挙動）。
  let toIndentedXml (indentation: int) (bulletml: Bulletml) =
    BulletmlXmlWrite.toIndentedXmlString indentation EncodingAndDoctype.Nothing bulletml
