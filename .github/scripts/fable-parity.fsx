// .NET の側。**ソースをそのまま `#load` する** ——
// 焼いた JS と突き合わせる相手が「元のソース」であってほしいので、
// build 済みの dll ではなくここから引く。
//
// 答えを組み立てるのは `XmlScan.describe`。node 側と同じ 1 本。
#load "../../src/FsBulletML2.Playground/fable/XmlScan.fs"

open System
open System.IO
open System.Text.Json
open FsBulletML2.Playground

let casesPath =
  match fsi.CommandLineArgs |> Array.tryItem 1 with
  | Some p -> p
  | None ->
    eprintfn "usage: dotnet fsi fable-parity.fsx <cases.json>"
    exit 2

let doc = JsonDocument.Parse(File.ReadAllText casesPath)
doc.RootElement.EnumerateArray()
|> Seq.iteri (fun i c ->
    let src = c.GetProperty("src").GetString()
    let cursor = c.GetProperty("cursor").GetInt32()
    printfn "%d\t%s" i (XmlScan.describe src cursor))
