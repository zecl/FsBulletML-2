namespace FsBulletML2

open System.IO
open System.Text
open System.Xml

/// 弾幕の木を XML の字にするところ。
/// 歩きは Core のまま。System.Xml は Fable に無いので、ここへ置いてある。
[<AutoOpen>]
module internal BulletmlXmlWrite =

  /// XML の受け口。`XmlWriter` を包むだけ ——
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
    // DOCTYPE の名前と SYSTEM id は `Xml.fs` の [<Literal>] を使う。
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

/// 弾幕の木を歩いて、受け口へ流す 1 本。表記を知らない。
/// 違うのは受け口だけ。F# の CE だけはこの形に乗らない。
module BulletmlWriter =

  let writeTo (sink: DTD.IBulletmlSink) (bulletml: Bulletml) =
    DTD.BulletmlXml.writeContentTo sink bulletml

  /// XML の字にする。定数を畳まない。
  /// 畳む側は 8 が 8.0000000000 になり、表記を行き来すると字が化ける。
  let toIndentedXml (indentation: int) (bulletml: Bulletml) =
    BulletmlXmlWrite.toIndentedXmlString indentation EncodingAndDoctype.Nothing bulletml
