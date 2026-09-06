// **`namespace` の手前 に `///` は置けない**（FS3520 が出る）。
// 型の doc は型に付けてある
namespace FsBulletML2.LanguageService

open System
open System.Text

/// どの表記で書かれているか。
///
/// **同じ字が 2 か所 に書いてあった。** ブラウザ側（Fable）が
/// `SourceKind.Xml.Id` で `"xml"` を作り、host 側（`Main.fs`）が
/// `match kind with | "xml" ->` で受けていた。**片方 だけ変えても
/// build も試験も落ちない** —— 走らせて「未対応: xml」が出て初めて分かる。
///
/// ここに 1 本 置いて、両方 が引く形にした。
///
/// ## 表記そのものは、まだ 1 つ しか読めない
///
/// 並びに 4 つ 在るのは、口を先に開けてあるため。`Parser` には
/// `tryReadSxmlString` / `tryReadFsbString` が既に在り、載せるのは
/// host 側に腕を 1 本 足すだけ。**ここは読める / 読めないを持たない** ——
/// 持つと「どの表記か」と「いま読めるか」が同じ型に乗り、
/// 読めるようになったときに両方 直すことになる。
///
/// ## `Fable.Core` に依存しない
///
/// このソースは host（.NET）と ブラウザ側（Fable が焼いた JS）の
/// **両方 で compile される**。依存を入れると host 側が壊れる。
/// 答えが 2 つ の runtime で一致することは `guard-fable-parity.ps1` が
/// 走行で見ている（`describe` がその口）。
type SourceKind =
  /// BulletML の XML
  | Xml
  /// S 式
  | Sxml
  /// 短い記法
  | Fsb
  /// F# の computation expression
  | FSharpDsl

  /// 表記を指す字。**host と ブラウザ側 が渡し合う鍵**で、
  /// `ApplySource` の第 1 引数 がこれ
  member this.Id =
    match this with
    | Xml -> "xml"
    | Sxml -> "sxml"
    | Fsb -> "fsb"
    | FSharpDsl -> "fsharp"

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SourceKind =

  /// 並び。**候補を出す側も、知らない字を断る側も、ここを引く**
  let all = [ Xml; Sxml; Fsb; FSharpDsl ]

  /// 字から戻す。**大小を区別する** —— `Id` が作る字は小文字だけなので、
  /// `"XML"` を通すと「作った側が知らない字が通る」ことになる
  let tryParse (id: string) : SourceKind option =
    if isNull id then None
    else all |> List.tryFind (fun k -> String.Equals(k.Id, id, StringComparison.Ordinal))

  /// 2 つ の runtime で同じ答えが返ることを見る口。
  /// **組み立てはここ 1 か所。** node 側 と .NET 側 で別々に組むと、
  /// 組み方のほうが食い違って「中身は同じなのに赤」になる
  let describe (id: string) : string =
    let sb = StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    add (if isNull id then "null" else string id.Length)
    add ":"
    match tryParse id with
    | None -> add "-"
    | Some k ->
      // 腕の名。**`Id` とは別の道**で、Fable の union の toString を通る
      add (string k)
      add "/"
      add k.Id
    add " all="
    add (all |> List.map (fun k -> k.Id) |> String.concat ",")
    sb.ToString()
