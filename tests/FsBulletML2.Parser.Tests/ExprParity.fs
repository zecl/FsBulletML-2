namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// **読めない式を字の上に出す**（v4.1）。
///
/// --- なぜ読み手が 2 本 在るのか
///
/// `FsBulletML2.LanguageService` は `ProjectReference` を 1 本 も持たない ——
/// **`Core/Expr.fs` はブラウザ側から見えない。** 打鍵ごとに式を見るなら
/// 器に読み手が要る。**値は出さない**（評価器が 2 本 に割れると、
/// 同じ式が 2 通り の値になる）。
///
/// **写しである以上、片方 だけ直した形が黙って残る。**
/// この門が、コーパスの全件 で 2 本 の答えを突き合わせる。
///
/// --- 版の頭で測ったこと
///
///     式が書ける要素        7 つ（DTD で #PCDATA を取るもの）
///     コーパス 173 本       式 427 件 / 読めない式 **0 件**
///     同梱 176 本           式 4,582 件 / 読めない式 **0 件**
///
/// **正しい弾幕には読めない式が 1 つ も無い** —— v2.3 が引いた線
/// （正しい弾幕にも在るものは出さない）に掛からない。
///
/// --- 壊れ方は 2 通り（測った）
///
///     <wait>1+*2</wait>       $ なし   **走らせると XPathException**
///     <wait>1+*$rank</wait>   $ あり   **走る。60 コマ で 61 発**（間 が空かない）
///
/// 割れ目は `BulletmlRead.foldConstants'` —— **`$` を含まない式だけ**を
/// 旧の評価器（XPath）で畳む。
///
/// --- 較正（1 か所 ずつ当てて、赤くなった点を数えた）
///
///   `'+' -> None` を「単項プラスを受ける」に   赤 1（器と Core の答えが同じ）
///   `skipWs s pos = s.Length` を `true` に     赤 4（後ろのゴミを許すと、位置も強さも崩れる）
///   `h.Start + readTo` を `h.Start` に         赤 1（読めたところの続きを指す）
///   `BadExpr(not (…"$"))` の `not` を落とす    赤 2（強さが 2 段）
///
/// **「式が書ける要素が引けている」は、この 4 つ では赤くならない。**
/// あれは当てる先の数を数える点で、**版の頭で「7 つ」と数えた誤りを拾った**
/// （`param` を落としていた）—— 門が測定を直した側。
[<TestFixture>]
type ExprParity() =

  /// `#PCDATA` を取る要素。**表を書かない** —— 語彙から引く
  static let exprNames =
    VocabForTests.vocab.Elements
    |> List.filter (fun e -> e.Text)
    |> List.map (fun e -> e.Name)

  static let corpus =
    Bullets.Dsl.All.bullets
    |> List.map (fun i -> i.Name, BulletmlWriter.toIndentedXml 4 i.Bulletml)

  /// Core の答え。**`Expr.parse` が `Invalid` を返さないこと**
  static let coreReadable (s: string) = Expr.parse s <> Expr.Node.Invalid

  /// 手で置いた式。**通る側と落ちる側を両方**（落ちる側だけ見ていると
  /// 「全部 落とす」壊れ方が緑のまま残る）
  static let handMade =
    [ "30"; "0"; "1.5"; ".5"; "1."; "-3"; "10-$rank*8"; "180+$rand*30"
      "$rand"; "$rank"; "$1"; "$"; "$12"; "(1+2)*3"; "1 + 2"; "3%2"; "4/2"
      "-(1+2)"; "--3"
      // 落ちる側
      "1+*2"; "abc"; "+1"; "1E-07"; "1e5"; "("; ")"; "()"; "1+"; "*2"; ""
      "1 2"; "$rando"; "1..2"; "."; "1+2)" ]

  static let writeAs (kind: SourceKind) (b: Bulletml) =
    match SourceWriter.tryFind kind with
    | None -> failwith "書く口が無い"
    | Some w -> match w.Write b with | Result.Ok s -> s | Result.Error e -> failwith e

  static let xmlWith (body: string) =
    "<?xml version=\"1.0\" ?>\n"
    + "<bulletml type=\"vertical\" xmlns=\"http://www.asahi-net.or.jp/~cs8k-cyu/bulletml\">\n"
    + "  <action label=\"top\">\n"
    + "    <wait>" + body + "</wait>\n"
    + "  </action>\n"
    + "</bulletml>"

  static let findingsFor (body: string) =
    let src = xmlWith body
    Semantics.exprFindings src (XmlScan.texts src exprNames)

  // --- 語彙が引けていること ---------------------------------------------------

  [<Test>]
  member _.``式が書ける要素が引けている``() =
    // **0 件 だと、下の点が全部「当てる先が無くて緑」になる**
    exprNames |> should not' (be Empty)
    // **8 つ。** 版の頭で「7 つ」と数えたのは誤りで、`param` を落としていた ——
    // **この点が拾った。** 増えたらまた赤くなる（DTD が動いた印）
    exprNames |> List.length |> should equal 8

  // --- 突き合わせ --------------------------------------------------------------

  [<Test>]
  member _.``手で置いた式で、器と Core の答えが同じ``() =
    handMade
    |> List.filter (fun s -> ExprCheck.readable s <> coreReadable s)
    |> should be Empty

  [<Test>]
  member _.``手で置いた式に、落ちる側が在る``() =
    // **材料が在ることを門が自分で数える。** 全部 読める式だと、
    // 上の点は当てる先を失って緑のまま通る
    handMade |> List.filter (ExprCheck.readable >> not) |> List.length
    |> should be (greaterThan 5)

  [<Test>]
  member _.``同梱 176 本 の式で、器と Core の答えが同じ``() =
    [ for (name, text) in corpus do
        for h in XmlScan.texts text exprNames do
          if ExprCheck.readable h.Text <> coreReadable h.Text then
            yield name, h.Text ]
    |> should be Empty

  [<Test>]
  member _.``同梱 176 本 に式が在る``() =
    // 上の点の当てる先。版の頭で 4,582 件 と数えた
    corpus |> List.sumBy (fun (_, t) -> (XmlScan.texts t exprNames).Length)
    |> should be (greaterThan 1000)

  // --- 偽陽性 ------------------------------------------------------------------

  [<Test>]
  member _.``正しい弾幕は 1 本 も光らない``() =
    [ for (name, text) in corpus do
        let fs = Semantics.exprFindings text (XmlScan.texts text exprNames)
        if not (List.isEmpty fs) then yield name ]
    |> should be Empty

  // --- 表記 --------------------------------------------------------------------

  [<Test>]
  member _.``xml と sxml と fsb で、取り出す式が揃う``() =
    [ for i in Bullets.Dsl.All.bullets do
        // **空白を全部 落として比べる。** 揃わないのは取り出しではなく**書く側**の
        // 都合が 2 つ ——
        //
        //     fsb は `120 + 120 * $rand` を `120+120*$rand` と焼く（空白を入れない）
        //     xml は長い式を**改行で折り返す**（1 本 で踏んだ。95 件 中 1 件）
        //
        // ここで見たいのは「同じ式を、同じ要素名で、同じ数だけ拾えるか」
        let squeeze (s: string) =
          s |> Seq.filter (System.Char.IsWhiteSpace >> not) |> Seq.toArray |> System.String
        let take (t: string) (g: string -> string list -> TextHit list) =
          g t exprNames
          |> List.map (fun h -> h.TagName, squeeze h.Text)
          |> List.sort
        let ax = take (writeAs SourceKind.Xml i.Bulletml) XmlScan.texts
        let asx = take (writeAs SourceKind.Sxml i.Bulletml) SxmlScan.texts
        let af = take (writeAs SourceKind.Fsb i.Bulletml) FsbScan.texts
        if ax <> asx || ax <> af then
          yield i.Name, List.length ax, List.length asx, List.length af ]
    |> should be Empty

  // --- 出し方 ------------------------------------------------------------------

  [<Test>]
  member _.``読める式には出ない``() =
    findingsFor "30-$rank*8" |> should be Empty

  [<Test>]
  member _.``$ を含まない式は「走らない」``() =
    match findingsFor "1+*2" with
    | [ f ] -> f.Kind |> should equal (Semantics.BadExpr true)
    | fs -> failwithf "1 件 でない: %d" (List.length fs)

  [<Test>]
  member _.``$ を含む式は「走るが値が出ない」``() =
    match findingsFor "1+*$rank" with
    | [ f ] -> f.Kind |> should equal (Semantics.BadExpr false)
    | fs -> failwithf "1 件 でない: %d" (List.length fs)

  [<Test>]
  member _.``読めたところの続きを指す``() =
    // `<wait>1+*2</wait>` は 4 行 目、`<wait>` は 5 桁 目 から。
    // 中身は 11 桁 目 から始まり、`1` まで読めるので指すのは 12 桁 目
    match findingsFor "1+*2" with
    | [ f ] ->
        f.Line |> should equal 4
        f.Column |> should equal 12
        f.EndColumn |> should equal 15
    | fs -> failwithf "1 件 でない: %d" (List.length fs)

  [<Test>]
  member _.``頭から読めない式は、字の頭 を指す``() =
    match findingsFor "abc" with
    | [ f ] ->
        f.Column |> should equal 11
        f.EndColumn |> should equal 14
    | fs -> failwithf "1 件 でない: %d" (List.length fs)
