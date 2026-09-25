namespace FsBulletML2

open System.Runtime.InteropServices
open FParsec

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Bulletml =

    let private convertParsed (kind: string) (parsed: ParserResult<XmlNode, 'u>) : Bulletml =
        match parsed with
        | Success(r, _, _) -> r |> BulletmlRead.convertBulletmlFromXmlNode
        | Failure(_, _, _) -> failwith (kind + " parse error")

    let private tryParsed (parsed: ParserResult<XmlNode, 'u>) : Bulletml option =
        match parsed with
        | Success(r, _, _) -> r |> BulletmlRead.tryBulletmlFromXmlNode
        | Failure(_, _, _) -> None

    let readXmlString (xml: string) : Bulletml =
        XmlNode.ReadXmlString xml |> BulletmlRead.convertBulletmlFromXmlNode

    let tryReadXmlString (xml: string) : Bulletml option =
        XmlNode.ReadXmlString xml |> BulletmlRead.tryBulletmlFromXmlNode

    let readXml (xmlFile: string) : Bulletml =
        XmlNode.ReadXml xmlFile |> BulletmlRead.convertBulletmlFromXmlNode

    let tryReadXml (xmlFile: string) : Bulletml option =
        XmlNode.ReadXml xmlFile |> BulletmlRead.tryBulletmlFromXmlNode

    let readSxmlString (sxml: string) : Bulletml = Sxml.parse sxml |> convertParsed "sxml"

    let tryReadSxmlString (sxml: string) : Bulletml option = Sxml.parse sxml |> tryParsed

    let readSxml (sxmlFile: string) : Bulletml =
        Sxml.parseFromFile sxmlFile |> convertParsed "sxml"

    let tryReadSxml (sxmlFile: string) : Bulletml option =
        Sxml.parseFromFile sxmlFile |> tryParsed

    let readFsbString (fsb: string) : Bulletml =
        Offside.parse fsb |> convertParsed "fsb"

    let tryReadFsbString (fsb: string) : Bulletml option = Offside.parse fsb |> tryParsed

    let readFsb (fsbFile: string) : Bulletml =
        Offside.parseFromFile fsbFile |> convertParsed "fsb"

    let tryReadFsb (fsbFile: string) : Bulletml option =
        Offside.parseFromFile fsbFile |> tryParsed

    type Bulletml with

        [<CompiledName "ReadXmlString">]
        static member ReadXmlString(xml: string) : Bulletml = readXmlString xml

        [<CompiledName "TryReadXmlString">]
        static member TryReadXmlString(xml: string) : Bulletml option = tryReadXmlString xml

        [<CompiledName "ReadSxmlString">]
        static member ReadSxmlString(sxml: string) : Bulletml = readSxmlString sxml

        [<CompiledName "TryReadSxmlString">]
        static member TryReadSxmlString(sxml: string) : Bulletml option = tryReadSxmlString sxml

        [<CompiledName "ReadSxml">]
        static member ReadSxml(sxmlFile: string) : Bulletml = readSxml sxmlFile

        [<CompiledName "TryReadSxml">]
        static member TryReadSxml(sxmlFile: string) : Bulletml option = tryReadSxml sxmlFile

        [<CompiledName "ReadFsbString">]
        static member ReadFsbString(fsb: string) : Bulletml = readFsbString fsb

        [<CompiledName "TryReadFsbString">]
        static member TryReadFsbString(fsb: string) : Bulletml option = tryReadFsbString fsb

        [<CompiledName "ReadFsb">]
        static member ReadFsb(fsbFile: string) : Bulletml = readFsb fsbFile

        [<CompiledName "TryReadFsb">]
        static member TryReadFsb(fsbFile: string) : Bulletml option = tryReadFsb fsbFile

        // 走らせる木と同じ型になったので、foldConstants が返すのは
        // 定数を畳んだ Bulletml。書くのは BulletmlXml（DTD.fs）。
        // ここを member のままにできないのは、名前が同じで自分を呼ぶため
        member this.ToXmlString() =
            this
            |> BulletmlRead.foldConstants
            |> BulletmlXmlWrite.toXmlString EncodingAndDoctype.Nothing

        member this.ToXmlStringForTest() =
            this
            |> BulletmlRead.foldConstantsForTest
            |> BulletmlXmlWrite.toXmlString EncodingAndDoctype.Nothing

        member this.ToXmlString(?encodingAndDoctype) =
            let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing

            this
            |> BulletmlRead.foldConstants
            |> BulletmlXmlWrite.toXmlString encodingAndDoctype

        member this.ToXmlStringForTest(?encodingAndDoctype) =
            let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing

            this
            |> BulletmlRead.foldConstantsForTest
            |> BulletmlXmlWrite.toXmlString encodingAndDoctype

        member this.ToIndentedXmlString([<Optional; DefaultParameterValue(4)>] ?indentation: int, ?encodingAndDoctype) =
            let indentation = defaultArg indentation 4
            let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing

            this
            |> BulletmlRead.foldConstants
            |> BulletmlXmlWrite.toIndentedXmlString indentation encodingAndDoctype

        member this.ToIndentedXmlStringForTest
            ([<Optional; DefaultParameterValue(4)>] ?indentation: int, ?encodingAndDoctype)
            =
            let indentation = defaultArg indentation 4
            let encodingAndDoctype = defaultArg encodingAndDoctype EncodingAndDoctype.Nothing

            this
            |> BulletmlRead.foldConstantsForTest
            |> BulletmlXmlWrite.toIndentedXmlString indentation encodingAndDoctype
