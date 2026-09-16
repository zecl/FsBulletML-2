namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Bullets.Dsl
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

/// F# の CE でも、名前がどこに書いてあるかを引けるか。
///
/// v1.6 まで、この表記だけ `Usages` も `Fixes` も空だった。
/// 理由は「CE には要素名が無いから」と書いてあったが、無いのは要素名で
/// あって名前ではない —— `defAction "x"` の `x` は `<action label="x">` の
/// `x` そのもので、数え方が違うだけだった。
[<TestFixture>]
type FsharpUsages() =

  static let fsharp = Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> ISourceLanguage
  static let xml = Languages.Xml.XmlLanguage(fun () -> vocab) :> ISourceLanguage

  /// `|` の位置をカーソルとして引く
  let at (marked: string) =
    fsharp.Usages (marked.Replace("|", "")) (marked.IndexOf '|')

  let fixesAt (marked: string) =
    fsharp.Fixes (marked.Replace("|", "")) (marked.IndexOf '|')

  let write (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> Result.Error "書く口が無い"
    | Some w -> w.Write b

  /// 参照 1 つ と 定義 1 つ
  [<Literal>]
  let Src = """let x =
  untypedXmlns "u" "n" {
    top {
      actionRef "loop" []
    }
    defAction "loop" {
      wait "1"
    }
  }
"""

  // --- 引く / 引かない -------------------------------------------------------

  [<Test>]
  member _.``定義の上でも 参照の上でも 同じ並び``() =
    let src = Src
    let fromRef = fsharp.Usages src (src.IndexOf "actionRef \"loop\"" + 11)
    let fromDef = fsharp.Usages src (src.IndexOf "defAction \"loop\"" + 11)
    fromRef.Length |> should equal 2
    fromRef |> should equal fromDef

  [<Test>]
  member _.``定義側と参照側を 1 欄 で分ける``() =
    let src = Src
    let found = fsharp.Usages src (src.IndexOf "actionRef \"loop\"" + 11)
    found |> List.map (fun u -> u.IsDefinition) |> should equal [ false; true ]

  [<Test>]
  member _.``位置が名前そのものを指す``() =
    // `      actionRef "loop" []` の `loop` は 18 桁目 から、閉じ引用符が 22
    let src = Src
    match fsharp.Usages src (src.IndexOf "actionRef \"loop\"" + 11) with
    | first :: _ ->
      first.Line |> should equal 4
      first.Column |> should equal 18
      first.EndColumn |> should equal 22
      first.Text |> should equal "loop"
    | [] -> failwith "1 件 も引けない"

  [<Test>]
  member _.``名前の上でなければ 空``() =
    // CE の名前そのものの上（`actionRef` の a）は名前ではない
    at "let x =\n  untyped \"n\" { top { a|ctionRef \"b\" [] } }" |> should be Empty
    // 引数の並びの中
    at "let x =\n  untyped \"n\" { top { actionRef \"b\" [|] } }" |> should be Empty

  [<Test>]
  member _.``引数を取らない名前``() =
    // `top` は `<action label="top">`。引数が無いので名前そのものを指す
    let src = "let x =\n  untyped \"n\" {\n    t|op { wait \"1\" }\n  }\n"
    match at src with
    | [ u ] ->
      u.Text |> should equal "top"
      u.IsDefinition |> should equal true
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``名前は 2 番目 の文字列のことも在る``() =
    // `repeatRef "8" "loop" []` —— 手前 に回数が在る。
    // 0 番目 を取ると回数を名前だと思う
    let src = "let x =\n  untyped \"n\" {\n    top { repeatRef \"8\" \"loop\" [] }\n    defAction \"loop\" { wait \"1\" }\n  }\n"
    let found = fsharp.Usages src (src.IndexOf "\"loop\"" + 2)
    found.Length |> should equal 2
    found |> List.map (fun u -> u.Text) |> List.distinct |> should equal [ "loop" ]

  [<Test>]
  member _.``文字列の中の似た字は拾わない``() =
    // 弾幕の名前に CE の名前と同じ綴りが入っていても、名前ではない
    let src = "let x =\n  untyped \"defAction top actionRef\" {\n    top { wait \"1\" }\n  }\n"
    // 名前として数えたのは `top` の 1 つ だけ
    let found = fsharp.Usages src (src.IndexOf "    top" + 5)
    found.Length |> should equal 1

  // --- 直し方 ---------------------------------------------------------------

  [<Test>]
  member _.``無い参照の上で 近い定義を出す``() =
    // `top` も定義。 引数を取らないだけで、`<action label="top">` を
    // 決めている —— `lop` からは `top`（1 置換）も `loop`（1 挿入）も近い
    let src = "let x =\n  untyped \"n\" {\n    top { actionRef \"lop\" [] }\n    defAction \"loop\" { wait \"1\" }\n  }\n"
    fsharp.Fixes src (src.IndexOf "\"lop\"" + 2)
    |> List.filter (fun f -> f.EndColumn > f.Column)
    |> List.map (fun f -> f.Title)
    |> List.sort
    |> should equal [ "lop を loop に直す"; "lop を top に直す" ]

  [<Test>]
  member _.``無い参照の上で 定義を作れる``() =
    let src = "let x =\n  untyped \"n\" {\n    top { actionRef \"loop\" [] }\n  }\n"
    match fsharp.Fixes src (src.IndexOf "\"loop\"" + 2) |> List.filter (fun f -> f.EndColumn = f.Column) with
    | [ f ] ->
      // 見出しはその表記で打つ字。要素名ではない
      f.Title |> should equal "loop の defAction を作る"
      // 中身を空にできない。 F# の CE は `{ }` の中に何か要る
      f.Text |> should equal "    defAction \"loop\" {\n        ()\n    }\n"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``作った定義を当てると 参照が埋まる``() =
    let src = "let x =\n  untyped \"n\" {\n    top { actionRef \"loop\" [] }\n  }\n"
    match fsharp.Fixes src (src.IndexOf "\"loop\"" + 2) |> List.filter (fun f -> f.EndColumn = f.Column) with
    | [ f ] ->
      let lines = src.Replace("\r\n", "\n").Split('\n')
      let line = lines.[f.Line - 1]
      lines.[f.Line - 1] <- line.Substring(0, f.Column - 1) + f.Text + line.Substring(f.EndColumn - 1)
      let after = System.String.Join("\n", lines)
      // 当てたあとは、その名前の書いてあるところが 2 つ になる
      let found = fsharp.Usages after (after.IndexOf "\"loop\"" + 2)
      found.Length |> should equal 2
      found |> List.filter (fun u -> u.IsDefinition) |> List.length |> should equal 1
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``作った定義を当てた本文が 読める``() =
    // 数だけ見ていると出ない。 参照が埋まっても、その字が読めるとは
    // 限らない —— CE は `{ }` の中に何か要るので、空の定義は読めない
    // （ブラウザで Apply して初めて出た）
    let src = "let x =\n  vertical \"n\" {\n    top {\n      actionRef \"loop\" []\n    }\n  }\n"
    match fsharp.Fixes src (src.IndexOf "\"loop\"" + 2) |> List.filter (fun f -> f.EndColumn = f.Column) with
    | [ f ] ->
      let lines = src.Replace("\r\n", "\n").Split('\n')
      let line = lines.[f.Line - 1]
      lines.[f.Line - 1] <- line.Substring(0, f.Column - 1) + f.Text + line.Substring(f.EndColumn - 1)
      let after = System.String.Join("\n", lines)
      let reader = (SourceReader.tryFind SourceKind.FSharpDsl).Value
      let mutable got = false
      // 当てる前は読める（読めない本文と比べていない）
      reader.Apply (fun _ -> ()) src |> should equal None
      match reader.Apply (fun _ -> got <- true) after with
      | Some failure -> failwithf "当てた本文が読めない: %s" failure.Message
      | None -> got |> should equal true
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``根が閉じていなければ 作らない``() =
    // 打っている途中の本文はふつうに閉じていない
    fixesAt "let x =\n  untyped \"n\" {\n    top { actionRef \"l|oop\" [] }\n"
    |> List.filter (fun f -> f.EndColumn = f.Column)
    |> should be Empty

  // --- コーパスと突き合わせる -----------------------------------------------

  [<Test>]
  member _.``同梱 176 本 で CE と XML の数が一致する``() =
    // 位置は表記ごとに違うが、数は違わない。
    // v1.2 で 3 表記 に当てたのと同じ測り方を、4 つ 目 に当てる
    let mutable compared = 0
    let mutable withTargets = 0
    let bad = ResizeArray<string>()
    for info in All.bullets do
      match write SourceKind.FSharpDsl info.Bulletml, write SourceKind.Xml info.Bulletml with
      | Result.Ok ce, Result.Ok x ->
        compared <- compared + 1
        // その本文の中の名前を全部 なめて、引けた数の並びを作る
        let profile (lang: ISourceLanguage) (tags: string -> TagHit list) (src: string) =
          tags src
          |> List.filter (fun t -> not t.Closing)
          |> List.collect (fun t -> t.Attrs)
          |> List.map (fun a -> (lang.Usages src a.ValueStart).Length)
          |> List.filter (fun n -> n > 0)
          |> List.sort
        // 名前を載せる属性は語彙から引く（字を書かない）
        let attr =
          Refs.pairs (vocab.Elements |> List.map (fun e -> e.Name, e.Attrs |> List.map (fun a -> a.Name)))
          |> List.head
          |> fun (_, _, a) -> a
        let ceTags =
          FsharpScan.tags
            (vocab.CeLabels |> List.map (fun c -> c.Name, c.Element, c.LabelArg, c.Fixed) |> List.toArray)
            attr
        let mine = profile fsharp ceTags ce
        let theirs = profile xml XmlScan.tags x
        if not mine.IsEmpty then withTargets <- withTargets + 1
        if mine <> theirs then
          bad.Add(sprintf "%s: CE %A / XML %A" info.Name mine theirs)
      | _ -> ()
    bad |> List.ofSeq |> should be Empty
    // 1 本 も比べていなければ、上は何も測っていない
    compared |> should greaterThan 100
    // 対象がどこにも無ければ、一致しているのは「全部 空」だけ
    withTargets |> should greaterThan 100
