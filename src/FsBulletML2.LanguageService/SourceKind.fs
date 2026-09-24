// `namespace` の手前 に `///` は置けない（FS3520 が出る）。
// 型の doc は型に付けてある
namespace FsBulletML2.LanguageService

open System
open System.Text

/// どの表記で書かれているか。
/// 読める / 読めないは持たない。持つと次の表記を足した瞬間に割れる。
type SourceKind =
    /// BulletML の XML
    | Xml
    /// S 式
    | Sxml
    /// 短い記法
    | Fsb
    /// F# の computation expression
    | FSharpDsl

    /// 表記を指す字。host と ブラウザ側 が渡し合う鍵で、
    /// `ApplySource` の第 1 引数 がこれ
    member this.Id =
        match this with
        | Xml -> "xml"
        | Sxml -> "sxml"
        | Fsb -> "fsb"
        | FSharpDsl -> "fsharp"

    /// 開いたファイルの拡張子。`Id` から作らない。
    /// F# の CE は `.fsx`。`Id` の `"fsharp"` とはずれる。
    member this.FileExtension =
        match this with
        | Xml -> ".xml"
        | Sxml -> ".sxml"
        | Fsb -> ".fsb"
        | FSharpDsl -> ".fsx"

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SourceKind =

    /// 並び。候補を出す側も、知らない字を断る側も、ここを引く
    let all = [ Xml; Sxml; Fsb; FSharpDsl ]

    /// 字から戻す。大小を区別する —— `Id` が作る字は小文字だけなので、
    /// `"XML"` を通すと「作った側が知らない字が通る」ことになる
    let tryParse (id: string) : SourceKind option =
        if isNull id then
            None
        else
            all |> List.tryFind (fun k -> String.Equals(k.Id, id, StringComparison.Ordinal))

    /// 2 runtime で同じ答えが返る口。組み立てはここ 1 か所。
    let describe (id: string) : string =
        let sb = StringBuilder()
        let add (s: string) = sb.Append s |> ignore
        add (if isNull id then "null" else string id.Length)
        add ":"

        match tryParse id with
        | None -> add "-"
        | Some k ->
            // 腕の名。`Id` とは別の道で、Fable の union の toString を通る
            add (string k)
            add "/"
            add k.Id
            add "/"
            add k.FileExtension

        add " all="
        add (all |> List.map (fun k -> k.Id) |> String.concat ",")
        add " ext="
        add (all |> List.map (fun k -> k.FileExtension) |> String.concat ",")
        sb.ToString()
