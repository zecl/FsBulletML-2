namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// **F# の CE を読む口の目盛り。**
///
/// --- 何を真とするか
///
/// 弾幕 176 本 は `FsBulletML2.Bullets.Dsl` に**値として在る**（F# コンパイラが
/// 組んだもの）。同じ本文を `FsharpCe.read` に通して、**同じ値になるかを見る。**
///
/// これは目盛りとしていちばん強い形 —— 期待値を手で書いていないので、
/// **書き間違えようがない。** ずれたらこちらが間違っている。
///
/// --- 二重化していること
///
/// `FsharpCe` は CE の意味論を持たない（歩いたあとに `Dsl` を呼ぶ）が、
/// **「名前 -> どの呼び出しか」の振り分けは持っている。** そこが唯一 の
/// 二重化で、この試験が当てているのはそこ。
///
/// --- 材料
///
/// `Bullets.Dsl` の `.fs` を試験の出力へ写している（fsproj の `Content`）。
/// **写せていなければ 0 本 で緑になる**ので、本数そのものを先に見る。
[<TestFixture>]
type FsharpCeCorpus() =

  /// `  let <名> =` から次の同じ深さの `  let ` までを 1 本 とする
  static let sources =
    lazy
      let root = Path.Combine(AppContext.BaseDirectory, "BulletsDsl")
      if not (Directory.Exists root) then dict []
      else
        let head = Regex(@"^  let\s+(\w+)\s*=", RegexOptions.Multiline)
        let table = System.Collections.Generic.Dictionary<string, string>()
        for file in Directory.EnumerateFiles(root, "*.fs", SearchOption.AllDirectories) do
          if Path.GetFileName file <> "All.fs" then
            let text = File.ReadAllText file
            let ms = head.Matches text |> Seq.toArray
            for i in 0 .. ms.Length - 1 do
              let from = ms.[i].Index
              let till = if i + 1 < ms.Length then ms.[i + 1].Index else text.Length
              table.[ms.[i].Groups.[1].Value] <- text.Substring(from, till - from)
        table :> Collections.Generic.IDictionary<string, string>

  let catalog = Bullets.Dsl.All.bullets

  // --- 材料が読めているか ---------------------------------------------------

  [<Test>]
  member _.``コーパスの本文が写せている``() =
    // **0 本 なら下の点は全部「1 本 も回さずに緑」になる**
    sources.Value.Count |> should greaterThan 100

  [<Test>]
  member _.``同梱カタログが 176 本``() =
    catalog.Length |> should equal 176

  // --- 本題 -----------------------------------------------------------------

  [<Test>]
  member _.``本文はすべて読める``() =
    let bad =
      [ for KeyValue (name, src) in sources.Value do
          match FsharpCe.read src with
          | Error (line, col, message) -> yield sprintf "%s: %d:%d %s" name line col message
          | Ok _ -> () ]
    bad |> should be Empty

  [<Test>]
  member _.``読んだ値が、F# コンパイラの組んだ値と数まで一致する``() =
    // **名前で引き当てない。** カタログの `Name` は CE の説明文字列で、
    // **同じ説明の弾幕が 3 組 在る**（`let` の名前は 179 本 すべて一意）。
    // 名前を鍵にすると、その 3 組 が取り違わって「値が違う」に見える
    // （最初それで 3 本 外した）。
    //
    // だから鍵を使わず、**値の多重集合そのもの**を突き合わせる。
    let parsed =
      [ for KeyValue (_, src) in sources.Value do
          match FsharpCe.read src with
          | Ok b -> yield b
          | Error _ -> () ]
    let parsedCount = parsed |> List.countBy id |> dict
    let short (b: Bulletml) = defaultArg b.Name "（名前なし）"
    let bad =
      catalog
      |> List.countBy (fun i -> i.Bulletml)
      |> List.choose (fun (value, want) ->
          match parsedCount.TryGetValue value with
          | true, got when got >= want -> None
          | true, got -> Some(sprintf "%s: 正本 %d 本 / 読めたのは %d 本" (short value) want got)
          | false, _ -> Some(sprintf "%s: 読んだ側に無い" (short value)))
    bad |> should be Empty
    // **0 本 を緑にしない。** 上は「読んだ側が空」でも空の一覧を返しうる
    parsed.Length |> should greaterThanOrEqualTo catalog.Length

  // --- 読めない形は、位置つきで断る -----------------------------------------

  [<Test>]
  member _.``F# として壊れていれば 位置が出る``() =
    match FsharpCe.read "let x =\n  vertical \"a\" {\n    wait \"1\"\n" with
    | Ok _ -> failwith "読めてしまった"
    | Error (line, col, msg) ->
      line |> should greaterThan 0
      col |> should greaterThan 0
      msg |> should not' (equal "")

  [<Test>]
  member _.``知らない名前は 位置つきで断る``() =
    // **黙って落とさない。** コーパスに無い書き方は「読めない」と言う
    match FsharpCe.read "let x =\n  vertical \"a\" {\n    top {\n      nope \"1\"\n    }\n  }\n" with
    | Ok _ -> failwith "読めてしまった"
    | Error (line, col, msg) ->
      line |> should equal 4
      col |> should greaterThan 0
      msg |> should haveSubstring "nope"

  [<Test>]
  member _.``弾幕が 1 本 も無ければ 断る``() =
    match FsharpCe.read "// 何も書いていない\n" with
    | Ok _ -> failwith "読めてしまった"
    | Error (_, _, msg) -> msg |> should haveSubstring "1 本 も"

  // --- いま出さないと決めたもの ---------------------------------------------

  [<Test>]
  member _.``CE は候補を出さない（置ける場所が入れ子の型で決まるため）``() =
    // **黙って空なのではなく、ここで空だと決めている。**
    //
    // v1.6 まで hover もここで空だった。**その点は位置を外していた** ——
    // 当てていた添字が引用符の上で、名前の上ではなかったので、
    // hover を実装しても緑のまま通った。hover の点は `FsharpHover` へ移した
    let lang = Languages.Fsharp.FsharpLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage
    lang.Kind |> should equal SourceKind.FSharpDsl
    lang.EditorLanguageId |> should equal "fsharp"
    lang.TriggerCharacters |> should be Empty
    lang.Complete "let x =\n  untyped \"a\" {\n    top {\n      w" 40 |> should be Empty

  [<Test>]
  member _.``CE も参照の波線を出す（v1.9）``() =
    // v1.6 まで `Tags` が空で、出るのは Core が落ちた理由（位置なし）だけだった。
    // **無いのは要素名であって名前ではない** —— いまは位置つきで出る
    let src =
      "let x =\n  untyped \"a\" {\n    top {\n      actionRef \"nope\" []\n    }\n  }\n"
    SourceReader.fsharp.Tags src |> should not' (be Empty)
    let build (b: Bulletml) = Runner.load (fun () -> 0.5f) 0.5f b |> ignore
    match References.explain SourceReader.fsharp.Tags (SourceReader.fsharp.Apply build src) src with
    | [ f ] ->
      // 位置は名前そのもの（引用符の内側）
      f.Line |> should equal 4
      f.Column |> should equal 18
      f.EndColumn |> should equal 22
    | other -> failwithf "1 本 のはずが %d 本" other.Length

  [<Test>]
  member _.``読める CE には波線を出さない``() =
    // 上の点は「1 本 出る」ことしか見ていない。**出しすぎていないこと**を
    // 別に置く —— 参照が埋まっている本文で 1 本 でも出たら、それは嘘
    let src =
      "let x =\n  untyped \"a\" {\n    top {\n      actionRef \"loop\" []\n    }\n    defAction \"loop\" { wait \"1\" }\n  }\n"
    let build (b: Bulletml) = Runner.load (fun () -> 0.5f) 0.5f b |> ignore
    References.explain SourceReader.fsharp.Tags (SourceReader.fsharp.Apply build src) src
    |> should be Empty

  [<Test>]
  member _.``createBulletmlInfo が無くても読める``() =
    // 人がエディタで書くときは `createBulletmlInfo <|` を書かないほうが自然
    match FsharpCe.read "let x =\n  untyped \"a\" {\n    top {\n      wait \"1\"\n    }\n  }\n" with
    | Error (l, c, m) -> failwithf "読めなかった（%d:%d %s）" l c m
    | Ok bulletml -> bulletml.Name |> should equal (Some "a")
