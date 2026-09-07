namespace FsBulletML2.Parser.Tests

open System
open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// **fsb を読む口の目盛り。** `SxmlReader` と対。
///
/// --- なぜ 2 本 目 を書いているのか
///
/// `Parser` には `tryReadFsbString` が既に在る。**それでも `Diagnosis` は
/// 使っていない** —— あちらは
///
///     | Failure (_,_,_) -> None
///
/// で、**FParsec が持っている行と桁を捨てている。** 波線を引くにはそれが要る。
/// だから `Offside.parse` を直に呼ぶ筋をもう 1 本 書いた
/// （sxml とまったく同じ形だった。v1.1 の頭で測った）。
///
/// **口が在ることと、その口が要るものを返すことは別。**
///
/// --- 2 本 書いた以上は突き合わせる
///
/// 読める / 読めないの判定が `tryReadFsbString` とずれたら、それは
/// こちらの写し間違い。コーパス全部 で 1 本 ずつ当てる。
[<TestFixture>]
type FsbReader() =

  let corpus (ext: string) =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData", ext)
    if Directory.Exists root
    then Directory.EnumerateFiles(root, "*." + ext, SearchOption.AllDirectories) |> Seq.toArray
    else [||]

  let fsbCorpus = lazy corpus "fsb"

  /// **載せるところは通さない。** ここで見ているのは「読めるか」だけ
  let reads (src: string) = (SourceReader.fsb.Apply ignore src).IsNone

  [<Test>]
  member _.``コーパスが読めている``() =
    // 下の点は「1 本 も読めなくても緑」になる
    fsbCorpus.Value.Length |> should greaterThan 0

  [<Test>]
  member _.``読める / 読めない が tryReadFsbString と一致する``() =
    let split =
      fsbCorpus.Value
      |> Array.choose (fun file ->
          let src = File.ReadAllText file
          let mine = reads src
          let theirs = (try (tryReadFsbString src).IsSome with _ -> false)
          if mine = theirs then None
          else Some(sprintf "%s: こちら %b / Parser %b" (Path.GetFileName file) mine theirs))
      |> Array.toList
    split |> should be Empty

  [<Test>]
  member _.``読めるものと読めないものが 両方 在る``() =
    // **上の点は「全部 読めない」でも緑。** 並びの中身そのものを見る
    let ok = fsbCorpus.Value |> Array.filter (fun f -> reads (File.ReadAllText f))
    ok.Length |> should greaterThan 0
    ok.Length |> should lessThan fsbCorpus.Value.Length

  [<Test>]
  member _.``同じ名前の xml と 判定が一致する``() =
    let root = Path.Combine(AppContext.BaseDirectory, "TestData")
    let mutable pairs = 0
    let split =
      fsbCorpus.Value
      |> Array.choose (fun file ->
          let rel = file.Substring(Path.Combine(root, "fsb").Length + 1)
          let asXml = Path.Combine(root, "xml", Path.ChangeExtension(rel, ".xml"))
          if not (File.Exists asXml) then None
          else
            pairs <- pairs + 1
            let a = reads (File.ReadAllText file)
            let b = (SourceReader.xml.Apply ignore (File.ReadAllText asXml)).IsNone
            if a = b then None
            else Some(sprintf "%s: fsb %b / xml %b" rel a b))
      |> Array.toList
    split |> should be Empty
    // 対が 1 組 も無ければ、上は何も測っていない
    pairs |> should greaterThan 0

  // --- 落ちたときに、どこまで分かるか ---------------------------------------

  [<Test>]
  member _.``構文が壊れていれば 位置が出る``() =
    // **これがこの版の本題。** `tryReadFsbString` を通すとここが 0 になる
    let cases =
      [ "bulletml\n    action label=top", 2
        "bulletml\n    action\n        fire\n            direction:\"", 4 ]
    for (src, line) in cases do
      match SourceReader.fsb.Apply ignore src with
      | None -> failwithf "読めてしまった: %s" src
      | Some f ->
        f.Line |> should equal line
        f.Column |> should greaterThan 0
        f.Message |> should not' (equal "")

  [<Test>]
  member _.``桁が動く``() =
    // いつも 1 を返す壊れ方が緑で通らないように、同じ行の 2 か所 を数える
    let columnOf (src: string) =
      match SourceReader.fsb.Apply ignore src with
      | Some f -> f.Column
      | None -> failwithf "読めてしまった: %s" src
    columnOf "bulletml\n    action label=top"
    |> should not' (equal (columnOf "bulletml\n    fire label=top"))

  [<Test>]
  member _.``空の本文でも 位置が 1 以上``() =
    // **0 のまま渡すと Monaco の範囲が壊れる。** 位置なし（Line = 0）とも混ざる
    match SourceReader.fsb.Apply ignore "" with
    | None -> failwith "空が読めてしまった"
    | Some f ->
      f.Line |> should greaterThanOrEqualTo 1
      f.Column |> should greaterThanOrEqualTo 1

  [<Test>]
  member _.``BulletML でない木は 位置なし``() =
    // 字は全部 読めているので、どこが悪いとは言えない
    match SourceReader.fsb.Apply ignore "nope\n    inner:\"1\"" with
    | None -> failwith "読めてしまった"
    | Some f -> f.Line |> should equal 0

  [<Test>]
  member _.``読める形が 途中で終わっていても通る``() =
    // **`Offside.parse` は `eof` を要求していない。** 根の要素を読み終えた
    // ところで止まり、**残りを黙って捨てる。**
    //
    // タブで字下げした行がその形 —— エラーにならず、子が 1 つ も付かない。
    // **補完の側（`FsbScan`）がタブを字下げに数えないのは、これに揃えたから**
    // （`FsbContext` の「タブは字下げにならない」）。
    //
    // ここを直すのは Parser の仕事で、この版の範囲ではない。
    // **黙って捨てていることを、字で残しておく**
    let mutable loaded = None
    SourceReader.fsb.Apply (fun b -> loaded <- Some b) "bulletml\n\taction label=\"top\""
    |> should equal None
    match loaded with
    | None -> failwith "載らなかった"
    | Some (Bulletml (_, elements)) -> elements |> should be Empty

  // --- 束ねている側 ---------------------------------------------------------

  [<Test>]
  member _.``表記ごとに 字の数え方が違う``() =
    // 同じ本文を 3 つ に通す。**同じ答えが返ったら、どれかが effectively
    // 使われていない**（表を引き違えている）
    let src = "bulletml\n    action label=\"top\""
    (SourceReader.fsb.Tags src |> List.map (fun t -> t.TagName))
    |> should equal [ "bulletml"; "action" ]
    SourceReader.xml.Tags src |> should be Empty
    SourceReader.sxml.Tags src |> should be Empty
