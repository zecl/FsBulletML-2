namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// **sxml を読む口の目盛り。**
///
/// --- なぜ 2 本 目 を書いているのか
///
/// `Parser` には `tryReadSxmlString` が既に在る。**それでも `Diagnosis` は
/// 使っていない** —— あちらは
///
///     | Failure (_,_,_) -> None
///
/// で、**FParsec が持っている行と桁を捨てている。** 波線を引くにはそれが要る。
/// だから `Sxml.parse` を直に呼ぶ筋をもう 1 本 書いた。
///
/// **口が在ることと、その口が要るものを返すことは別。**
///
/// --- 2 本 書いた以上は突き合わせる
///
/// 読める / 読めないの判定が `tryReadSxmlString` とずれたら、それは
/// こちらの写し間違い。コーパス全部 で 1 本 ずつ当てる。
///
/// --- xml とも突き合わせる
///
/// コーパスは同じ名前で xml と sxml が対になっている。**同じ弾幕なので
/// 判定も同じはず** —— 割れたら、どちらかの表記だけが読めなくなっている。
[<TestFixture>]
type SxmlReader() =

  let corpus (ext: string) =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData", ext)
    if Directory.Exists root
    then Directory.EnumerateFiles(root, "*." + ext, SearchOption.AllDirectories) |> Seq.toArray
    else [||]

  let sxmlCorpus = lazy corpus "sxml"

  /// **載せるところは通さない。** ここで見ているのは「読めるか」だけで、
  /// 走らせて落ちる層（輪、`top` が無い）は別の試験が見ている
  let reads (src: string) = (SourceReader.sxml.Apply ignore src).IsNone

  [<Test>]
  member _.``コーパスが読めている``() =
    // 下の点は「1 本 も読めなくても緑」になる
    sxmlCorpus.Value.Length |> should greaterThan 0

  [<Test>]
  member _.``読める / 読めない が tryReadSxmlString と一致する``() =
    let split =
      sxmlCorpus.Value
      |> Array.choose (fun file ->
          let src = File.ReadAllText file
          let mine = reads src
          let theirs = (try (tryReadSxmlString src).IsSome with _ -> false)
          if mine = theirs then None
          else Some(sprintf "%s: こちら %b / Parser %b" (Path.GetFileName file) mine theirs))
      |> Array.toList
    split |> should be Empty

  [<Test>]
  member _.``読めるものと読めないものが 両方 在る``() =
    // **上の点は「全部 読めない」でも緑。** 並びの中身そのものを見る
    let ok = sxmlCorpus.Value |> Array.filter (fun f -> reads (File.ReadAllText f))
    ok.Length |> should greaterThan 0
    ok.Length |> should lessThan sxmlCorpus.Value.Length

  [<Test>]
  member _.``同じ名前の xml と 判定が一致する``() =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData")
    let mutable pairs = 0
    let split =
      sxmlCorpus.Value
      |> Array.choose (fun file ->
          let rel = file.Substring(Path.Combine(root, "sxml").Length + 1)
          let asXml = Path.Combine(root, "xml", Path.ChangeExtension(rel, ".xml"))
          if not (File.Exists asXml) then None
          else
            pairs <- pairs + 1
            let a = reads (File.ReadAllText file)
            let b = (SourceReader.xml.Apply ignore (File.ReadAllText asXml)).IsNone
            if a = b then None
            else Some(sprintf "%s: sxml %b / xml %b" rel a b))
      |> Array.toList
    split |> should be Empty
    // 対が 1 組 も無ければ、上は何も測っていない
    pairs |> should greaterThan 0

  // --- 落ちたときに、どこまで分かるか ---------------------------------------

  [<Test>]
  member _.``構文が壊れていれば 位置が出る``() =
    // **これがこの版の本題。** `tryReadSxmlString` を通すとここが 0 になる
    let cases =
      [ "(bulletml (action (fire", 1
        "(bulletml\n(action\n(fire \"\n)\n)", 3 ]
    for (src, line) in cases do
      match SourceReader.sxml.Apply ignore src with
      | None -> failwithf "読めてしまった: %s" src
      | Some f ->
        f.Line |> should equal line
        f.Column |> should greaterThan 0
        f.Message |> should not' (equal "")

  [<Test>]
  member _.``桁が動く``() =
    // いつも 1 を返す壊れ方が緑で通らないように、同じ行の 2 か所 を数える
    let columnOf (src: string) =
      match SourceReader.sxml.Apply ignore src with
      | Some f -> f.Column
      | None -> failwithf "読めてしまった: %s" src
    columnOf "(bulletml (action (fire" |> should not' (equal (columnOf "(bulletml (fire"))

  [<Test>]
  member _.``空の本文でも 位置が 1 以上``() =
    // **0 のまま渡すと Monaco の範囲が壊れる。** 位置なし（Line = 0）とも混ざる
    match SourceReader.sxml.Apply ignore "" with
    | None -> failwith "空が読めてしまった"
    | Some f ->
      f.Line |> should greaterThanOrEqualTo 1
      f.Column |> should greaterThanOrEqualTo 1

  [<Test>]
  member _.``BulletML でない S 式は 位置なし``() =
    // S 式としては読めるが BulletML ではない。**位置は無い** ——
    // 字は全部 読めているので、どこが悪いとは言えない
    match SourceReader.sxml.Apply ignore "(nope (inner \"1\"))" with
    | None -> failwith "読めてしまった"
    | Some f -> f.Line |> should equal 0

  // --- 束ねている側 ---------------------------------------------------------

  [<Test>]
  member _.``読める表記が 2 つ 登録されている``() =
    SourceReader.all |> List.map (fun r -> r.Kind.Id) |> should equal [ "xml"; "sxml" ]
    (SourceReader.tryFind SourceKind.Xml).IsSome |> should be True
    (SourceReader.tryFind SourceKind.Sxml).IsSome |> should be True

  [<Test>]
  member _.``まだ読めない表記は 引けない``() =
    // **`SourceKind.all` と揃っていない。** 揃っていないことを人へ見せるのは
    // `ApplySource` の側（`未対応: …`）
    (SourceReader.tryFind SourceKind.Fsb).IsNone |> should be True
    (SourceReader.tryFind SourceKind.FSharpDsl).IsNone |> should be True

  [<Test>]
  member _.``拡張子は 表記ごとに違い、id から導けない``() =
    // **`"." + Id` で作れる形に見える。** 3 つ までは合うが F# の CE だけ
    // ずれる（id は `fsharp`、拡張子は `.fsx`）—— 導く形にすると
    // **そこだけ静かに嘘になる**。ブラウザ側の Open が名前で表記を決めるので、
    // 嘘だと「開いたのに読めない」になる
    SourceKind.all |> List.map (fun k -> k.FileExtension)
    |> should equal [ ".xml"; ".sxml"; ".fsb"; ".fsx" ]
    SourceKind.all |> List.map (fun k -> k.FileExtension) |> List.distinct |> List.length
    |> should equal SourceKind.all.Length
    SourceKind.FSharpDsl.FileExtension |> should not' (equal ("." + SourceKind.FSharpDsl.Id))

  [<Test>]
  member _.``表記ごとに 字の数え方が違う``() =
    // 同じ本文を両方 に通す。**同じ答えが返ったら、どちらかが effectively
    // 使われていない**（表を引き違えている）
    let src = "(bulletml (action (@ (label \"top\"))))"
    (SourceReader.sxml.Tags src |> List.map (fun t -> t.TagName))
    |> should equal [ "bulletml"; "action" ]
    SourceReader.xml.Tags src |> should be Empty
