namespace FsBulletML2.Parser.Tests

open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages

/// 雛形（v2.6）。**骨は 1 つ、字にする手は表記ごと。**
///
/// `Shape.WriteFrame` は `SourceWriter` と別に「骨から字を作る」ので、
/// **同じことを言う手が 2 か所 に在る**。片方 だけ直すのを止めるのがここ。
///
/// 当てるのは 3 つ ——
///
///     1  書いた字が、その表記のパーサで読める
///     2  3 表記 が**同じ木**になる（表記の差ではなく骨の差だけが残る）
///     3  書き直すと、`SourceWriter` が焼いた字と一致する
///
/// **2 がいちばん効く。** 1 だけだと、片方 の表記で属性を落としても
/// 「読めた」で通る。3 は `WriteFrame` と `SourceWriter` を直に突き合わせる。
///
/// --- 較正（当てた変異と、赤くなった点の数）
///
///   xml の属性を落とす      3 点
///   sxml の属性を落とす     3 点
///   xml の字下げを 2 にする  1 点
///   fsb の字下げを 2 にする  1 点
///   JSON の鍵を変える     1 点
///
/// **字下げの変異は 1 点 しか赤くならない。** 木を見る 2 つ は字下げでは
/// 割れず（fsb は字下げが構造なので割れる）、当たるのは字を突き合わせる側だけ。
/// **点が少ないのは弱いのではなく、当たる先が 1 つ しか無いということ。**
[<TestFixture>]
type Frames() =

  /// snippet の穴を既定値に潰す。**`${1:8}` -> `8`**
  ///
  /// 穴をそのまま読ませない —— `$1` は BulletML の式（param の参照）として
  /// **正しく読めてしまう**ので、潰さないと「読めた」が嘘になる
  static let fill (s: string) = Regex.Replace(s, @"\$\{\d+:([^}]*)\}", "$1")

  /// 雛形を、その表記で読める形に包む。**包むのも `WriteFrame`** ——
  /// 包み方を表記ごとに書くと、そこがまた 2 か所 目 になる
  static let wrap (snippet: FrameSnippet) =
    let inner =
      if List.contains "action" snippet.In then
        { Element = "action"
          Attrs = [ "label", "top" ]
          Text = ""
          Children = [ snippet.Frame ] }
      else snippet.Frame
    { Element = "bulletml"
      Attrs = [ "type", "vertical" ]
      Text = ""
      Children = [ inner ] }

  static let shapes =
    [ "xml", Xml.shape; "sxml", Sxml.shape; "fsb", Fsb.shape ]

  static let readWith (kind: SourceKind) (text: string) =
    match SourceReader.tryFind kind with
    | None -> Error("reader が無い: " + string kind)
    | Some r ->
      let mutable got = None
      match r.Apply (fun b -> got <- Some b) text with
      | Some f -> Error(sprintf "%A" f)
      | None ->
        match got with
        | Some b -> Ok b
        | None -> Error "読めたのに木が来ない"

  [<Test>]
  member _.``雛形が 1 つ も無ければ、この門は何も見ていない``() =
    // **0 件 は違反 0 件 と同じ顔をする**
    Frames.all |> List.length |> should greaterThan 3

  [<Test>]
  member _.``どの雛形も、置き場を持っている``() =
    Frames.all |> List.filter (fun s -> List.isEmpty s.In) |> should be Empty

  [<Test>]
  member _.``書いた字は、その表記で読める``() =
    let mutable n = 0
    for snippet in Frames.all do
      for (name, shape) in shapes do
        let text = fill (shape.WriteFrame(wrap snippet))
        match readWith shape.Kind text with
        | Ok _ -> n <- n + 1
        | Error e -> failwithf "%s / %s が読めない: %s\n%s" snippet.Label name e text
    n |> should equal (List.length Frames.all * List.length shapes)

  [<Test>]
  member _.``3 表記 が同じ木になる``() =
    for snippet in Frames.all do
      let trees =
        shapes
        |> List.map (fun (name, shape) ->
             name, readWith shape.Kind (fill (shape.WriteFrame(wrap snippet))))
      let ok =
        trees
        |> List.map (fun (name, r) ->
             match r with
             | Ok b -> name, sprintf "%A" b
             | Error e -> failwithf "%s / %s: %s" snippet.Label name e)
      let first = snd ok.Head
      for (name, s) in ok do
        if s <> first then
          failwithf "%s —— %s だけ木が違う\n%s\n----\n%s" snippet.Label name (fst ok.Head + ":\n" + first) (name + ":\n" + s)

  [<Test>]
  member _.``WriteFrame の字は、SourceWriter が焼いた字と一致する``() =
    // **2 か所 を直に突き合わせる。** 読んで木にしてから焼き直せば、
    // `SourceWriter` の側の字になる —— そこと `WriteFrame` の字が同じなら、
    // 骨から字を作る手が 2 つ とも同じことを言っている
    for snippet in Frames.all do
      for (name, shape) in shapes do
        let mine = fill (shape.WriteFrame(wrap snippet))
        match readWith shape.Kind mine with
        | Error e -> failwithf "%s / %s: %s" snippet.Label name e
        | Ok tree ->
          match SourceWriter.tryFind shape.Kind with
          | None -> failwithf "writer が無い: %s" name
          | Some w ->
            match w.Write tree with
            | Result.Error e -> failwithf "%s / %s は焼けない: %s" snippet.Label name e
            | Result.Ok theirs ->
              let norm (s: string) = s.Replace("\r\n", "\n").TrimEnd('\n')
              if norm mine <> norm theirs then
                failwithf "%s / %s で字が割れた\nWriteFrame:\n%s\n----\nSourceWriter:\n%s"
                  snippet.Label name (norm mine) (norm theirs)

  [<Test>]
  member _.``穴は既定値を持っている``() =
    // 穴を Tab で埋めずに走らせても壊れないこと。**`$1` は param の参照**
    // として読めてしまうので、既定値の無い穴を作らない
    let rec holes (f: Frame) =
      let here =
        (Regex.Matches(f.Text, @"\$\{?\d+") |> Seq.map (fun m -> m.Value) |> List.ofSeq)
        @ (f.Attrs |> List.collect (fun (_, v) ->
             Regex.Matches(v, @"\$\{?\d+") |> Seq.map (fun m -> m.Value) |> List.ofSeq))
      here @ (f.Children |> List.collect holes)
    let bare =
      Frames.all
      |> List.collect (fun s -> holes s.Frame)
      |> List.filter (fun h -> not (h.StartsWith "${"))
    bare |> should be Empty

  [<Test>]
  member _.``host が焼いた JSON に雛形が載っている``() =
    // **器は JSON でしか受け取らない。** ここが落ちると、試験は緑のまま
    // ブラウザにだけ雛形が届かない（`VocabForTests` は `Frames.all` を
    // 直に渡すので、JSON の往復を 1 度 も通らない）
    let json = FsBulletML2.LanguageService.Vocabulary.toJson ()
    use doc = System.Text.Json.JsonDocument.Parse json
    let mutable frames = Unchecked.defaultof<System.Text.Json.JsonElement>
    doc.RootElement.TryGetProperty("frames", &frames) |> should equal true
    let arr = frames.EnumerateArray() |> Seq.toList
    arr |> List.length |> should equal (List.length Frames.all)
    // 骨が空でないこと。**数だけ合っていても中身が空なら届いていない**
    for f in arr do
      f.GetProperty("label").GetString() |> should not' (equal "")
      f.GetProperty("frame").GetProperty("element").GetString() |> should not' (equal "")
      f.GetProperty("in").EnumerateArray() |> Seq.length |> should greaterThan 0