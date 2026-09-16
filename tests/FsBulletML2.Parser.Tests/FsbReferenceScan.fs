namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// 参照の欠けを数える側の目盛り、fsb の側。 `ReferenceScan` /
/// `SxmlReferenceScan` と対。
///
/// 数える中身（走る先の対、定義と参照の突き合わせ、並べ方）は
/// `References` に 1 本 しか無い。表記ごとなのは字の数え方だけで、
/// それを `FsbScan.tags` として渡している。
///
/// だからここで赤くなるのは `FsbScan` の側 —— 共通の側が壊れれば
/// XML と sxml の試験も一緒に赤くなる。
[<TestFixture>]
type FsbReferenceScan() =

  let missing = References.missing FsbScan.tags

  let rand () = 0.5f
  let rank = 0.5f
  let build (b: Bulletml) = Runner.load rand rank b |> ignore
  let read src = SourceReader.fsb.Apply build src
  let coreFails src = (read src).IsSome
  /// 本番と同じ道。`Main.fs` の `ApplySource` もこれを通る
  let explain src = References.explain FsbScan.tags (read src) src

  let corpus =
    lazy
      let root = Path.Combine(AppContext.BaseDirectory, "TestData", "fsb")
      if Directory.Exists root
      then Directory.EnumerateFiles(root, "*.fsb", SearchOption.AllDirectories) |> Seq.toArray
      else [||]

  /// 参照の label を 1 つ だけ、在り得ない名前に差し替える
  let refAttr = Regex("(actionRef|fireRef|bulletRef)\\s+label\\s*=\\s*\"([^\"]*)\"")

  // --- 挙げる / 挙げない ----------------------------------------------------

  [<Test>]
  member _.``定義が在れば挙げない``() =
    missing "bulletml\n    action label=\"a\"\n        wait:\"1\"\n    action label=\"top\"\n        actionRef label=\"a\"\n"
    |> should be Empty

  [<Test>]
  member _.``無い参照を 2 つ とも挙げる``() =
    let found =
      missing "bulletml\n    action label=\"top\"\n        actionRef label=\"a\"\n        actionRef label=\"b\"\n"
    found.Length |> should equal 2
    found |> List.forall (fun f -> f.Line > 0) |> should be True

  [<Test>]
  member _.``fire と bullet も見る``() =
    let found =
      missing "bulletml\n    action label=\"top\"\n        fireRef label=\"f\"\n        fire\n            bulletRef label=\"b\"\n"
    found.Length |> should equal 2
    found |> List.map (fun f -> f.Message.Split(' ').[0]) |> List.sort
    |> should equal [ "bulletRef"; "fireRef" ]

  // --- 位置 -----------------------------------------------------------------

  [<Test>]
  member _.``位置が label の値そのものを指す``() =
    // 3 行目 の `        actionRef label="a"`。`a` は 26 桁目、閉じ引用符が 27
    let src = "bulletml\n    action label=\"top\"\n        actionRef label=\"a\"\n"
    match missing src with
    | [ f ] ->
      f.Line |> should equal 3
      f.Column |> should equal 26
      f.EndColumn |> should equal 27
      f.Message |> should haveSubstring "a"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``行が動く``() =
    // いつも 1 を返す壊れ方が緑で通らないように、2 か所 の行を数える
    let src =
      "bulletml\n    action label=\"top\"\n        actionRef label=\"a\"\n        wait:\"1\"\n        actionRef label=\"b\"\n"
    missing src |> List.map (fun f -> f.Line) |> should equal [ 3; 5 ]

  // --- 飛ばすもの -----------------------------------------------------------

  [<Test>]
  member _.``本文の中は数えない``() =
    // `:"…"` の中は式。 数えると、式に書いた字が label に見える
    missing "bulletml\n    action label=\"top\"\n        wait:\"actionRef label=a\"\n"
    |> should be Empty

  [<Test>]
  member _.``名前の無い行は数えない``() =
    // XML の「宣言と閉じ札を積まない」／sxml の「名前の無い括弧」に当たる。
    // 空行と字下げだけの行を混ぜない
    FsbScan.tags "bulletml\n    \n\n    action label=\"top\"\n" |> List.map (fun t -> t.TagName)
    |> should equal [ "bulletml"; "action" ]

  [<Test>]
  member _.``閉じ引用符が無いときは、構文の理由だけを出す``() =
    // `FsbScan` は閉じ引用符の無い値を捨てない（カーソルの居場所に要る）。
    // そのぶん値が行末まで伸びるが、そこへは届かない ——
    // 閉じていなければ `Offside.parse` が位置つきで落ち、`explain` は
    // 位置が在る層をそのまま返して `missing` を呼ばない
    let src = "bulletml\n    action label=\"top\"\n        actionRef label=\"a"
    match explain src with
    | [ f ] ->
      f.Line |> should greaterThan 0
      f.Message |> should not' (haveSubstring "が指す")
    | other -> failwithf "1 本 のはずが %d 本" other.Length

  // --- コーパスと突き合わせる -----------------------------------------------

  [<Test>]
  member _.``コーパスが読めている``() =
    corpus.Value.Length |> should greaterThan 0

  [<Test>]
  member _.``Apply が通る弾幕には 1 本 も引かない``() =
    let noisy =
      corpus.Value
      |> Array.choose (fun file ->
          let src = File.ReadAllText file
          match explain src with
          | [] -> None
          | fs when coreFails src -> ignore fs; None   // 読めない形も置いてある並び
          | fs -> Some(sprintf "%s: %s" (Path.GetFileName file) fs.Head.Message))
      |> Array.toList
    noisy |> should be Empty

  [<Test>]
  member _.``参照を 1 つ 壊すと、Core が落ちて、同じ名前を挙げる``() =
    let mutable measured = 0
    let mutable skipped = 0
    let bad = ResizeArray<string>()
    for file in corpus.Value do
      let src = File.ReadAllText file
      if not (coreFails src) then
        for m in refAttr.Matches src do
          let broken =
            src.Substring(0, m.Groups.[2].Index)
            + "__nope__"
            + src.Substring(m.Groups.[2].Index + m.Groups.[2].Length)
          // 壊しても Core が通るのは、そこが到達しない枝だったとき
          if not (coreFails broken) then skipped <- skipped + 1
          else
            measured <- measured + 1
            let found = explain broken
            if not (found |> List.exists (fun f -> f.Message.EndsWith "__nope__")) then
              bad.Add(sprintf "%s: Core は落ちたのに挙げなかった（%d 本）" (Path.GetFileName file) found.Length)
    // 飛ばしてばかりだと、この試験は何も測っていない
    measured |> should greaterThan 0
    measured |> should greaterThan skipped
    bad |> List.ofSeq |> should be Empty
