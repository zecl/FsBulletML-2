namespace FsBulletML2

open System.Xml
open System.Runtime.InteropServices
open FParsec

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Bulletml = 

  let readXmlString (xml : string) : Bulletml = 
    use reader = new System.IO.StringReader(xml)
    use reader = XmlReader.Create(reader, readerSettingsIndented) 
    XmlNode.Read(xml, reader) |> BulletmlRead.convertBulletmlFromXmlNode

  let tryReadXmlString (xml : string) : Bulletml option = 
    use reader = new System.IO.StringReader(xml)
    use reader = XmlReader.Create(reader, readerSettingsIndented) 
    XmlNode.Read(xml, reader) |> BulletmlRead.tryBulletmlFromXmlNode

  let readXml (xmlFile : string) : Bulletml =
    use reader = XmlReader.Create((xmlFile:string), readerSettingsIndented) 
    XmlNode.Read(xmlFile, reader) |> BulletmlRead.convertBulletmlFromXmlNode

  let tryReadXml (xmlFile : string) : Bulletml option =
    use reader = XmlReader.Create((xmlFile:string), readerSettingsIndented) 
    XmlNode.Read(xmlFile, reader) |> BulletmlRead.tryBulletmlFromXmlNode

  let readSxmlString (sxml : string) : Bulletml =
    match Sxml.parse sxml with 
    | Success (r,_,_) -> r |> BulletmlRead.convertBulletmlFromXmlNode 
    | Failure (_,_,_) -> failwith "sxml parse error"

  let tryReadSxmlString (sxml : string) : Bulletml option =
    match Sxml.parse sxml with 
    | Success (r,_,_) -> r |> BulletmlRead.tryBulletmlFromXmlNode 
    | Failure (_,_,_) -> None

  let readSxml (sxmlFile : string) : Bulletml =
    match Sxml.parseFromFile sxmlFile with 
    | Success (r,_,_) -> r |> BulletmlRead.convertBulletmlFromXmlNode 
    | Failure (_,_,_) -> failwith "sxml parse error"

  let tryReadSxml (sxmlFile : string) : Bulletml option =
    match Sxml.parseFromFile sxmlFile with 
    | Success (r,_,_) -> r |> BulletmlRead.tryBulletmlFromXmlNode 
    | Failure (_,_,_) -> None

  let readFsbString (fsb: string) : Bulletml =
    match Offside.parse fsb with 
    | Success (r,_,_) -> r |> BulletmlRead.convertBulletmlFromXmlNode
    | Failure (_,_,_) -> failwith "fsb parse error"

  let tryReadFsbString (fsb: string) : Bulletml option =
    match Offside.parse fsb with 
    | Success (r,_,_) -> r |> BulletmlRead.tryBulletmlFromXmlNode
    | Failure (_,_,_) -> None

  let readFsb (fsbFile : string) : Bulletml =
    match Offside.parseFromFile fsbFile with 
    | Success (r,_,_) -> r |> BulletmlRead.convertBulletmlFromXmlNode
    | Failure (_,_,_) -> failwith "fsb parse error"

  let tryReadFsb (fsbFile : string) : Bulletml option =
    match Offside.parseFromFile fsbFile with 
    | Success (r,_,_) -> r |> BulletmlRead.tryBulletmlFromXmlNode
    | Failure (_,_,_) -> None

  type Bulletml with

    [<CompiledName "ReadXmlString">]
    static member ReadXmlString (xml : string) : Bulletml = readXmlString xml

    [<CompiledName "TryReadXmlString">]
    static member TryReadXmlString (xml : string) : Bulletml option = tryReadXmlString xml

    [<CompiledName "ReadSxmlString">]
    static member ReadSxmlString (sxml : string) : Bulletml = readSxmlString sxml

    [<CompiledName "TryReadSxmlString">]
    static member TryReadSxmlString (sxml : string) : Bulletml option = tryReadSxmlString sxml

    [<CompiledName "ReadSxml">]
    static member ReadSxml (sxmlFile : string) : Bulletml = readSxml sxmlFile

    [<CompiledName "TryReadSxml">]
    static member TryReadSxml (sxmlFile : string) : Bulletml option = tryReadSxml sxmlFile

    [<CompiledName "ReadFsbString">]
    static member ReadFsbString (fsb: string) : Bulletml = readFsbString fsb

    [<CompiledName "TryReadFsbString">]
    static member TryReadFsbString (fsb: string) : Bulletml option = tryReadFsbString fsb

    [<CompiledName "ReadFsb">]
    static member ReadFsb (fsbFile : string) : Bulletml = readFsb fsbFile

    [<CompiledName "TryReadFsb">]
    static member TryReadFsb (fsbFile : string) : Bulletml option = tryReadFsb fsbFile

    // 走らせる木と同じ型になったので、foldConstants が返すのは
    // **定数を畳んだ Bulletml**。書くのは BulletmlXml（DTD.fs）。
    // ここを member のままにできないのは、名前が同じで自分を呼ぶため
    member this.ToXmlString() =
      this |> BulletmlRead.foldConstants
           |> BulletmlXml.toXmlString EncodingAndDoctype.Nothing

    member this.ToXmlStringForTest() =
      this |> BulletmlRead.foldConstantsForTest
           |> BulletmlXml.toXmlString EncodingAndDoctype.Nothing

    member this.ToXmlString(?encodingAndDoctype) =
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing
      this |> BulletmlRead.foldConstants
           |> BulletmlXml.toXmlString encodingAndDoctype

    member this.ToXmlStringForTest(?encodingAndDoctype) =
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing
      this |> BulletmlRead.foldConstantsForTest
           |> BulletmlXml.toXmlString encodingAndDoctype

    member this.ToIndentedXmlString([<Optional; DefaultParameterValue(4)>]?indentation : int, ?encodingAndDoctype) =
      let indentation = defaultArg indentation 4
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing
      this |> BulletmlRead.foldConstants
           |> BulletmlXml.toIndentedXmlString indentation encodingAndDoctype

    member this.ToIndentedXmlStringForTest([<Optional; DefaultParameterValue(4)>]?indentation : int, ?encodingAndDoctype) =
      let indentation = defaultArg indentation 4
      let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing
      this |> BulletmlRead.foldConstantsForTest
           |> BulletmlXml.toIndentedXmlString indentation encodingAndDoctype
