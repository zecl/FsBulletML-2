namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 表記を書き分ける口の目盛り。
///
/// v1.3 まで、書けるのは XML だけだった。Playground が「表記を変えても本文は
/// 触らない」と決めていたのは、決めたのではなく できなかったから。
[<TestFixture>]
type Transcode() =

  static let catalog = Bullets.Dsl.All.bullets

  /// その表記で書いて、読み直す。本番と同じ口（`SourceWriter` / `SourceReader`）
  static let through (kind: SourceKind) (b: Bulletml) : Result<Bulletml, string> =
    match SourceWriter.tryFind kind, SourceReader.tryFind kind with
    | Some writer, Some reader ->
      match writer.Write b with
      | Result.Error why -> Result.Error("書けない: " + why)
      | Result.Ok text ->
        let mutable got = None
        match reader.Apply (fun x -> got <- Some x) text with
        | Some f -> Result.Error(sprintf "読めない: %s" f.Message)
        | None ->
          match got with
          | Some x -> Result.Ok x
          | None -> Result.Error "読めたが値が取れない"
    | _ -> Result.Error "口が無い"

  static let kinds = [ SourceKind.Xml; SourceKind.Sxml; SourceKind.Fsb; SourceKind.FSharpDsl ]

  // --- 材料 -----------------------------------------------------------------

  [<Test>]
  member _.``同梱カタログが 176 本``() =
    // 0 本 なら下の点は全部 空回りで緑になる
    catalog.Length |> should equal 176

  [<Test>]
  member _.``書く口が 4 つ、読む口と同じ並び``() =
    SourceWriter.all |> List.map (fun w -> w.Kind.Id)
    |> should equal (SourceReader.all |> List.map (fun r -> r.Kind.Id))
    SourceWriter.all |> List.map (fun w -> w.Kind.Id)
    |> should equal [ "xml"; "sxml"; "fsb"; "fsharp" ]

  // --- 本題 -----------------------------------------------------------------

  [<Test>]
  member _.``4 表記 すべてで書けて、読める``() =
    let bad =
      [ for kind in kinds do
          for info in catalog do
            match through kind info.Bulletml with
            | Result.Error why -> yield sprintf "%s / %s: %s" kind.Id (defaultArg info.Bulletml.Name "?") why
            | Result.Ok _ -> () ]
    bad |> should be Empty

  [<Test>]
  member _.``1 回 通したあとは、もう一度 通しても動かない``() =
    // ここが「落ち着いている」の中身。 2 回 目 で動くなら、
    // 表記を行き来するたびに中身が変わる
    let bad =
      [ for kind in kinds do
          for info in catalog do
            match through kind info.Bulletml with
            | Result.Ok once ->
              match through kind once with
              | Result.Ok twice when twice = once -> ()
              | Result.Ok _ -> yield sprintf "%s / %s: 2 回 目 で動いた" kind.Id (defaultArg info.Bulletml.Name "?")
              | Result.Error why -> yield sprintf "%s / %s: %s" kind.Id (defaultArg info.Bulletml.Name "?") why
            | Result.Error _ -> () ]
    bad |> should be Empty

  [<Test>]
  member _.``どの表記を通っても、そのあと XML を通して動かない``() =
    // この版の本体。
    //
    // 素の値と比べない。 fsb は 1 回 目 で式の空白を落とすので、
    // そこは必ず違う（違わなければ書けていない）。見たいのはその先 ——
    // fsb が作った値を XML に通しても動かないなら、
    // 2 つ の表記は同じものを指している。
    //
    // 4 表記 とも同じことが言えれば、表記を行き来しても中身が変わらない。
    let bad =
      [ for kind in kinds do
          for info in catalog do
            match through kind info.Bulletml with
            | Result.Error why -> yield sprintf "%s: %s" kind.Id why
            | Result.Ok once ->
              match through SourceKind.Xml once with
              | Result.Ok again when again = once -> ()
              | Result.Ok _ ->
                yield sprintf "%s / %s: XML を通すと動く" kind.Id (defaultArg info.Bulletml.Name "?")
              | Result.Error why -> yield sprintf "%s -> xml: %s" kind.Id why ]
    bad |> should be Empty

  [<Test>]
  member _.``名前の無い弾幕でも 4 表記 で往復する``() =
    // 同梱カタログは全部 name つき（CE で書かれているので、根が名前を要る）。
    // だがそれは本家の弾幕の姿ではない —— TestData の xml 173 本 は
    // 1 本 も name を持っていない。
    //
    // カタログだけで測っていたとき、CE は名前の無い弾幕を書けなかった
    // （`Dsl` の根が必ず名前を取る）。開いたファイルではほぼ必ず落ちる形で、
    // ここを足して初めて出た。
    let root = Path.Combine(AppContext.BaseDirectory, "TestData", "xml")
    let files =
      if Directory.Exists root
      then Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories) |> Seq.toArray
      else [||]
    files.Length |> should greaterThan 100
    let mutable measured = 0
    let bad = ResizeArray<string>()
    for file in files do
      let src = File.ReadAllText file
      let mutable got = None
      match SourceReader.xml.Apply (fun b -> got <- Some b) src with
      | Some _ -> ()   // 読めない形も置いてある並び
      | None ->
        match got with
        | None -> ()
        | Some b ->
          // 名前を持っていないことを、この点自身が確かめる
          if b.Name.IsNone then
            measured <- measured + 1
            for kind in kinds do
              match through kind b with
              | Result.Error why -> bad.Add(sprintf "%s / %s: %s" kind.Id (Path.GetFileName file) why)
              | Result.Ok once ->
                match through SourceKind.Xml once with
                | Result.Ok again when again = once -> ()
                | _ -> bad.Add(sprintf "%s / %s: XML を通すと動く" kind.Id (Path.GetFileName file))
    // 名前の無い弾幕が 1 本 も無ければ、この点は何も測っていない
    measured |> should greaterThan 100
    bad |> List.ofSeq |> should be Empty

  // --- インデント記法だけの事情 ---------------------------------------------

  [<Test>]
  member _.``fsb は式の空白を落とす``() =
    // 落とさないと書けない（`Offside.fs` の本文は空白を通さない）。
    // 黙って落としているのではなく、そう決めている
    match SourceWriter.fsb.Write (Bulletml.readXmlString "<bulletml><action label=\"top\"><wait>90 * 2</wait></action></bulletml>") with
    | Result.Error why -> failwithf "書けなかった: %s" why
    | Result.Ok text ->
      text |> should haveSubstring "wait:\"90*2\""
      text |> should not' (haveSubstring "90 * 2")

  [<Test>]
  member _.``空白を落として 語がくっつく式は カタログに無い``() =
    // `1 2` を `12` にすると意味が変わる。そういう式が在れば、この直しは嘘。
    // 同梱カタログの本文 1,194 種類 で 0 件 だった（v1.4 の頭で測った）——
    // 増えたらここが赤くなる
    let fused =
      [ for info in catalog do
          let xml = SourceWriter.xml.Write info.Bulletml
          match xml with
          | Result.Ok text ->
            for m in Text.RegularExpressions.Regex.Matches(text, ">([^<>]+)<") do
              let v = m.Groups.[1].Value
              if Text.RegularExpressions.Regex.IsMatch(v, @"\w\s+\w") then
                yield sprintf "%s: %s" (defaultArg info.Bulletml.Name "?") v
          | Result.Error _ -> () ]
    fused |> should be Empty

  // --- 書けないものは、そう言う -------------------------------------------

  [<Test>]
  member _.``sxml で通せない字は 書けないと言う``() =
    // 属性値に `<` は通らない。黙って落とすと、読み直したときに値が変わる
    let src = "<bulletml><action label=\"a&lt;b\"/></bulletml>"
    match SourceWriter.sxml.Write (Bulletml.readXmlString src) with
    | Result.Ok text -> failwithf "書けてしまった: %s" text
    | Result.Error why -> why |> should haveSubstring "sxml で書けない"

  [<Test>]
  member _.``角括弧は sxml で書ける``() =
    // 同梱カタログの 3 本 が `bulletmls/[Progear]_…` というラベルを持つ。
    // v1.4 で文法を広げた —— 引用符の中なので曖昧にならない
    let src = "<bulletml><action label=\"a[b]c\"/></bulletml>"
    match SourceWriter.sxml.Write (Bulletml.readXmlString src) with
    | Result.Error why -> failwithf "書けなかった: %s" why
    | Result.Ok text ->
      text |> should haveSubstring "a[b]c"
      (Bulletml.tryReadSxmlString text).IsSome |> should be True

  [<Test>]
  member _.``既存の sxml コーパスは そのまま読める``() =
    // 文法を広げた側の当て先。 受け入れを増やしただけなら、既存は全部 通る
    let root = Path.Combine(AppContext.BaseDirectory, "TestData", "sxml")
    let files =
      if Directory.Exists root
      then Directory.EnumerateFiles(root, "*.sxml", SearchOption.AllDirectories) |> Seq.toArray
      else [||]
    files.Length |> should greaterThan 100
    let readable = files |> Array.filter (fun f -> (SourceReader.sxml.Apply ignore (File.ReadAllText f)).IsNone)
    // 読めない形も置いてある並びなので、本数そのものではなく「読めるものが在る」を見る
    readable.Length |> should greaterThan 100

  [<Test>]
  member _.``CE で description は 書けないと言う``() =
    // `Dsl` に口が無い。同梱カタログには 1 件 も無いが、XML からは持ってこられる
    let src = "<bulletml description=\"あ\"><action label=\"top\"><wait>1</wait></action></bulletml>"
    match SourceWriter.fsharp.Write (Bulletml.readXmlString src) with
    | Result.Ok text -> failwithf "書けてしまった: %s" text
    | Result.Error why -> why |> should haveSubstring "description"

  [<Test>]
  member _.``中身の無い action も 4 表記 で往復する``() =
    // 空の `{ }` は F# では書けない（記録式に見える）ので `()` を置いている。
    // sxml と fsb も、本文が空だと読み直したときに消える形が在る
    let src = "<bulletml><action label=\"top\"><action/></action></bulletml>"
    let b = Bulletml.readXmlString src
    for kind in kinds do
      match through kind b with
      | Result.Error why -> failwithf "%s: %s" kind.Id why
      | Result.Ok back ->
        match through SourceKind.Xml b with
        | Result.Ok viaXml -> back |> should equal viaXml
        | Result.Error why -> failwith why
