namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// F# の CE の hover。 `HoverMarkdown`（XML 側）と対。
/// 出す字はほかの表記と同じ。違うのは、名前が BulletML の何を作るかを 1 段挟むところ。
[<TestFixture>]
type FsharpHover() =

  static let lang =
    Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> ISourceLanguage

  /// `@` の位置をカーソルとして引く
  let hover (marked: string) =
    lang.Hover (marked.Replace("@", "")) (marked.IndexOf '@')

  let must (marked: string) =
    match hover marked with
    | Some md -> md
    | None -> failwithf "hover が出なかった: %s" marked

  /// 語彙から引いた散文。期待値を手で書かない ——
  /// 書くと `Spec.fs` を直したときにこちらだけ古びる
  let specOf (element: string) =
    (vocab.Elements |> List.find (fun e -> e.Name = element)).Spec

  let valueSpecOf (element: string) (attr: string) (value: string) =
    let a =
      (vocab.Elements |> List.find (fun e -> e.Name = element)).Attrs
      |> List.find (fun a -> a.Name = attr)
    a.ValueSpecs |> List.find (fun (v, _) -> v = value) |> snd

  // --- 引けること -----------------------------------------------------------

  [<Test>]
  member _.``要素を作る名前で出る``() =
    let md = must "let x =\n  untyped \"a\" { top { w@ait \"1\" } }"
    md |> should haveSubstring "wait"
    md |> should haveSubstring (specOf "wait")
    // 見出しは打った字と、それが作るもの。 片方 だけだと、
    // `aim` のような名前で「何の話か」が分からない
    md |> should haveSubstring "wait → <wait>"

  [<Test>]
  member _.``要素名と綴りが違う名前でも出る``() =
    // ここが CE の本題 —— `defAction` は語彙に無い綴り
    let md = must "let x =\n  untyped \"a\" { defAc@tion \"go\" { wait \"1\" } }"
    md |> should haveSubstring "defAction → <action>"
    md |> should haveSubstring (specOf "action")

  [<Test>]
  member _.``属性値を固定する名前は 要素と値の 両方 が出る``() =
    // 値の散文だけにしない。 それだと「どの要素の話か」「ほかに
    // どんな属性が在るか」が落ちて、名前によって出る量が変わる
    let md = must "let x =\n  untyped \"a\" { top { fire { a@im \"0\" } } }"
    md |> should haveSubstring "aim → <direction>"
    md |> should haveSubstring (specOf "direction")
    md |> should haveSubstring "aim → <direction type=\"aim\">"
    md |> should haveSubstring (valueSpecOf "direction" "type" "aim")

  [<Test>]
  member _.``要素と属性値の 2 つ を指す名前は 2 つ 出る``() =
    let md = must "let x =\n  untyped \"a\" { top { changeDirection@Abs \"0\" \"1\" } }"
    md |> should haveSubstring (specOf "changeDirection")
    md |> should haveSubstring (valueSpecOf "direction" "type" "absolute")

  [<Test>]
  member _.``同じ綴りで別のものを指す名前は 両方 出る``() =
    // `vertical` は根の型でもあり、accel の中の要素でもある。
    // どちらか に決めない —— 決めるには入れ子の型を追うことになる
    let md = must "let x =\n  ver@tical \"a\" { top { wait \"1\" } }"
    md |> should haveSubstring (valueSpecOf "bulletml" "type" "vertical")
    md |> should haveSubstring (specOf "vertical")

  [<Test>]
  member _.``DTD の行がコードフェンスに入る``() =
    // markdown は `<` をタグとして食う。素で渡すと丸ごと消えて、
    // それでも hover は浮くので、目でも「出ていない」に見えない
    let md = must "let x =\n  untyped \"a\" { top { w@ait \"1\" } }"
    md |> should haveSubstring "```xml"
    md |> should haveSubstring "<!ELEMENT wait"

  // --- 引かないこと ---------------------------------------------------------

  [<Test>]
  member _.``文字列の中では出ない``() =
    // `wait "aim"` の `aim` は値であって CE の名前ではない
    hover "let x =\n  untyped \"a\" { top { wait \"a@im\" } }" |> should equal None

  [<Test>]
  member _.``弾幕の名前の中では出ない``() =
    hover "let x =\n  untyped \"w@ait\" { top { wait \"1\" } }" |> should equal None

  [<Test>]
  member _.``行コメントの中では出ない``() =
    hover "let x =\n  // w@ait は待つ\n  untyped \"a\" { top { wait \"1\" } }"
    |> should equal None

  [<Test>]
  member _.``入れ子のコメントの中では出ない``() =
    hover "let x =\n  (* (* w@ait *) *)\n  untyped \"a\" { top { wait \"1\" } }"
    |> should equal None

  [<Test>]
  member _.``CE でない語では出ない``() =
    hover "l@et x =\n  untyped \"a\" { top { wait \"1\" } }" |> should equal None

  [<Test>]
  member _.``名前の上でなければ出ない``() =
    hover "let x =\n  untyped \"a\" {@ top { wait \"1\" } }" |> should equal None

  [<Test>]
  member _.``空の本文で落ちない``() =
    lang.Hover "" 0 |> should equal None
    lang.Hover "wait" 99 |> should equal None

  // --- 覆っていること -------------------------------------------------------

  [<Test>]
  member _.``同梱カタログの CE の名前を 1 つ も取りこぼさない``() =
    // 点を 1 つ ずつ書かない。 本物の本文を通して、そこに在る名前の
    // ぜんぶ で出ることを見る —— 表に 1 行 足し忘れれば、その名前で赤くなる
    let root = Path.Combine(AppContext.BaseDirectory, "BulletsDsl")
    let sources =
      if not (Directory.Exists root) then [||]
      else Directory.EnumerateFiles(root, "*.fs", SearchOption.AllDirectories) |> Seq.toArray
    // 0 本 なら下は空回りで緑になる
    sources.Length |> should greaterThan 0
    let known = vocab.Ce |> List.map (fun c -> c.Name) |> Set.ofList
    let word = Regex(@"[A-Za-z_][A-Za-z0-9_']*")
    let mutable hit = 0
    let bad = ResizeArray<string>()
    for file in sources do
      let text = File.ReadAllText file
      for m in word.Matches text do
        if known.Contains m.Value then
          // 字を数える側で「本文の外」を落とす。 文字列の中の
          // 同じ綴りは名前ではないので、そこは数えない
          match FsharpScan.wordAt text m.Index with
          | Some w when w = m.Value ->
            hit <- hit + 1
            if (lang.Hover text m.Index).IsNone then
              bad.Add(sprintf "%s: %s" (Path.GetFileName file) w)
          | _ -> ()
    bad |> Seq.distinct |> Seq.toList |> should be Empty
    // 当たった数そのものも見る。0 件 で緑にしない
    hit |> should greaterThan 1000
