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
#load "../../src/FsBulletML2.LanguageService/ShareLink.fs"
#load "../../src/FsBulletML2.LanguageService/Scan.fs"
#load "../../src/FsBulletML2.LanguageService/XmlScan.fs"
#load "../../src/FsBulletML2.LanguageService/SxmlScan.fs"
#load "../../src/FsBulletML2.LanguageService/FsbScan.fs"
#load "../../src/FsBulletML2.LanguageService/FsharpScan.fs"
// 意味の層（v2.3）。**Refs を先に読む** —— Semantics は対の形を借りる
#load "../../src/FsBulletML2.LanguageService/Refs.fs"
#load "../../src/FsBulletML2.LanguageService/Semantics.fs"
#load "../../src/FsBulletML2.LanguageService/Outline.fs"

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
  | "FsharpScan" ->
    FsharpScan.describe (c.GetProperty("src").GetString()) (c.GetProperty("cursor").GetInt32())
  | "SourceKind" ->
    SourceKind.describe (c.GetProperty("id").GetString())
  | "ShareLink" ->
    ShareLink.describe (c.GetProperty("fragment").GetString())
  | "Scan" ->
    Scan.describePosition (c.GetProperty("src").GetString()) (c.GetProperty("cursor").GetInt32())
  | "Semantics" ->
    Semantics.describe (c.GetProperty("src").GetString())
  | "Outline" ->
    Outline.describe (c.GetProperty("src").GetString())
  | "FsharpTags" ->
    // 表は case に在る。**器に書けない**（要素名が入るので門が当たる）
    let labels =
      c.GetProperty("labels").EnumerateArray()
      |> Seq.map (fun row ->
           let a = row.EnumerateArray() |> Seq.toArray
           a.[0].GetString(), a.[1].GetString(), a.[2].GetInt32(), a.[3].GetString())
      |> Seq.toArray
    FsharpScan.describeTags labels (c.GetProperty("attr").GetString()) (c.GetProperty("src").GetString())
  | "FsharpBlocks" ->
    FsharpScan.describeBlocks (c.GetProperty("src").GetString())
  | t ->
    eprintfn "表に知らない target が在る: %s" t
    exit 4

let doc = JsonDocument.Parse(File.ReadAllText casesPath)
doc.RootElement.EnumerateArray()
|> Seq.iteri (fun i c -> printfn "%d\t%s" i (answer (c.GetProperty("target").GetString()) c))
