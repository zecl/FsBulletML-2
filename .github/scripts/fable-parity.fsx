// .NET の側。**ソースをそのまま `#load` する** ——
// 焼いた JS と突き合わせる相手が「元のソース」であってほしいので、
// build 済みの dll ではなくここから引く。
//
// **`#load` が `Fable.Core` への依存も見張っている。** LanguageService の
// ソースは host（.NET）でも compile されるので、Fable 専用の物を入れると
// ここが解決できずに落ちる。門を別に置かなくてよい。
//
// 答えを組み立てるのは各 target の `describe`。node 側と同じ 1 本。
#load "../../src/FsBulletML2.LanguageService/SourceKind.fs"
#load "../../src/FsBulletML2.LanguageService/Scan.fs"
#load "../../src/FsBulletML2.LanguageService/XmlScan.fs"
#load "../../src/FsBulletML2.LanguageService/SxmlScan.fs"
#load "../../src/FsBulletML2.LanguageService/FsbScan.fs"

open System
open System.IO
open System.Text.Json
open FsBulletML2.LanguageService

let casesPath =
  match fsi.CommandLineArgs |> Array.tryItem 1 with
  | Some p -> p
  | None ->
    eprintfn "usage: dotnet fsi fable-parity.fsx <cases.json>"
    exit 2

/// 引数の取り出しだけ target ごとに分ける。**答えの組み立ては describe の中。**
/// node 側にも同じ振り分けが在り、取り違えれば答えがずれるので
/// 突き合わせ自身がそこも見ている
let answer (target: string) (c: JsonElement) =
  match target with
  | "XmlScan" ->
    XmlScan.describe (c.GetProperty("src").GetString()) (c.GetProperty("cursor").GetInt32())
  | "SxmlScan" ->
    SxmlScan.describe (c.GetProperty("src").GetString()) (c.GetProperty("cursor").GetInt32())
  | "FsbScan" ->
    FsbScan.describe (c.GetProperty("src").GetString()) (c.GetProperty("cursor").GetInt32())
  | "SourceKind" ->
    SourceKind.describe (c.GetProperty("id").GetString())
  | t ->
    eprintfn "表に知らない target が在る: %s" t
    exit 4

let doc = JsonDocument.Parse(File.ReadAllText casesPath)
doc.RootElement.EnumerateArray()
|> Seq.iteri (fun i c -> printfn "%d\t%s" i (answer (c.GetProperty("target").GetString()) c))
