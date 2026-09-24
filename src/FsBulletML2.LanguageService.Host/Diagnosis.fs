namespace FsBulletML2.LanguageService

open System
open System.Xml
open FsBulletML2

/// 読めなかった理由。
/// `Line = 0` は位置が無い。`EndColumn = 0` は終わりが分からない。混ぜるな。
type Failure =
    {
        Line: int
        Column: int
        EndColumn: int
        Message: string
    }

/// Apply が落ちたときに、どこまで分かるかを分ける。
/// 位置が在るのは構文の層だけ。同じ label を本文から推定して指すな。
/// 輪と重複と top 無しはここを素通りする。落ちるのは走行。
module Diagnosis =

    /// 位置なしの理由
    let plain message =
        {
            Line = 0
            Column = 0
            EndColumn = 0
            Message = message
        }

    /// 例外を層に分ける。位置が在るのは `XmlException` だけ。
    /// 行 0 桁 0 は 1 に丸める。位置なし（`Line = 0`）と混ぜるな。
    let ofException (ex: exn) : Failure =
        match ex with
        | :? XmlException as x ->
            {
                Line = max 1 x.LineNumber
                Column = max 1 x.LinePosition
                // 「そこから先が読めない」しか言わないので、終わりは分からない
                EndColumn = 0
                Message = x.Message
            }
        | _ -> plain ex.Message

    /// 読んで、載せる。載せるところは呼ぶ側が渡す。`None` なら成功。
    /// 試験と本番で分け方を分けるな。
    let apply (build: Bulletml -> unit) (xml: string) : Failure option =
        try
            match tryReadXmlString xml with
            | None -> Some(plain "BulletML として読めなかった")
            | Some bulletml ->
                build bulletml
                None
        with ex ->
            Some(ofException ex)

    /// FParsec の文面から、待っていたものを言う行だけ足す。
    /// 飾り。取れなくても波線の位置は変わらない。
    let private expectation (message: string) =
        message.Split('\n')
        |> Array.map (fun l -> l.Trim())
        |> Array.tryFind (fun l -> l.StartsWith("Expecting:", StringComparison.Ordinal))

    /// F# の CE を読んで、載せる。parse だけ。型検査しない。
    /// 知らない名前にも位置が在る。ほかの表記と違うところ。
    let applyFsharp (build: Bulletml -> unit) (source: string) : Failure option =
        try
            match FsharpCe.read source with
            // 位置の無い理由（`0` は `FsharpCe` 側の印）
            | Error(0, _, message) -> Some(plain message)
            | Error(line, column, message) ->
                Some
                    {
                        Line = max 1 line
                        Column = max 1 column
                        // 「そこで詰まった」しか言わないので、終わりは分からない
                        EndColumn = 0
                        Message = message
                    }
            | Ok bulletml ->
                build bulletml
                None
        with ex ->
            Some(ofException ex)

    /// sxml を読んで、載せる。`tryReadSxmlString` は使うな。
    /// あちらは FParsec の位置を捨てている。
    let applySxml (build: Bulletml -> unit) (sxml: string) : Failure option =
        try
            match Sxml.parse sxml with
            | FParsec.CharParsers.Failure(message, error, _) ->
                let where = "S 式として読めなかった"

                Some
                    { // 空文字を読ませても 1 起点 で返ってくるが、丸めておく ——
                        // 0 のまま渡すと Monaco の範囲が壊れる
                        Line = max 1 (int error.Position.Line)
                        Column = max 1 (int error.Position.Column)
                        // 「そこから先が読めない」しか言わないので、終わりは分からない
                        EndColumn = 0
                        Message =
                            match expectation message with
                            | Some e -> where + "。" + e
                            | None -> where
                    }
            | FParsec.CharParsers.Success(node, _, _) ->
                match BulletmlRead.tryBulletmlFromXmlNode node with
                | None -> Some(plain "BulletML として読めなかった")
                | Some bulletml ->
                    build bulletml
                    None
        with ex ->
            Some(ofException ex)

    /// インデント記法（fsb）を読んで、載せる。`tryReadFsbString` は使うな。
    /// あちらも位置を捨てている。
    let applyFsb (build: Bulletml -> unit) (fsb: string) : Failure option =
        try
            match Offside.parse fsb with
            | FParsec.CharParsers.Failure(message, error, _) ->
                let where = "インデント記法として読めなかった"

                Some
                    { // 空文字を読ませても 1 起点 で返ってくるが、丸めておく
                        Line = max 1 (int error.Position.Line)
                        Column = max 1 (int error.Position.Column)
                        // 「そこから先が読めない」しか言わないので、終わりは分からない
                        EndColumn = 0
                        Message =
                            match expectation message with
                            | Some e -> where + "。" + e
                            | None -> where
                    }
            | FParsec.CharParsers.Success(node, _, _) ->
                match BulletmlRead.tryBulletmlFromXmlNode node with
                | None -> Some(plain "BulletML として読めなかった")
                | Some bulletml ->
                    build bulletml
                    None
        with ex ->
            Some(ofException ex)
