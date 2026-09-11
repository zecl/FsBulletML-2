namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService

/// **式の横に値を出す先**（v4.4）。
///
/// --- 出すのは 3 つ を通ったものだけ
///
///     読める            読めない式は波線の担当（v4.1）
///     ただの数でない     `<wait>30</wait>` に `= 30` は要らない
///     `$rand` を含まない **毎回 変わる**ので、字の横に固定の数を出すと嘘になる
///
/// 同梱 176 本 の式 6,042 件 を数えたら ——
///
///     ただの数        4,075 件（67.4%）
///     $rand を含む     349 件（5.8%）
///     読めない           0 件
///     **出す         1,618 件（26.8%）**
///
/// 1 本 あたり 中央 5 / 9 割 19 / 最大 71。**1 つ も出ない本は 6 / 176。**
///
/// --- 値はここに入らない
///
/// **器は評価器を持たない。** 畳むのは面（`Main.EvalExprs` -> Core の `Expr`）——
/// 評価器を 2 本 持つと同じ式が 2 通り の値になる（`ExprCheck` と同じ理由）。
///
/// --- 較正（1 か所 ずつ当てて、赤くなった点を数えた）
///
///   `ExprCheck.plainNumber` の除外を落とす     赤 3
///   `$rand` の除外を落とす                     赤 2
///   `lineColumnsAscending` の巻き戻しを消す    赤 1
[<TestFixture>]
type Hints() =

  static let vocab = VocabForTests.vocab

  static let lang : SourceLanguage.ISourceLanguage =
    Languages.Xml.XmlLanguage(fun () -> vocab) :> SourceLanguage.ISourceLanguage

  static let corpus =
    Bullets.Dsl.All.bullets
    |> List.map (fun i -> i.Name, BulletmlWriter.toIndentedXml 4 i.Bulletml)

  static let src = """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>4+8*$rank</times>
      <action>
        <fire>
          <direction type="absolute">360/(4+8*$rank)</direction>
          <speed>2</speed>
          <bullet/>
        </fire>
        <wait>60-30*$rand</wait>
      </action>
    </repeat>
  </action>
</bulletml>"""

  // --- ただの数 --------------------------------------------------------------

  [<Test>]
  member _.``ただの数を見分ける``() =
    for s in [ "30"; "0"; "1.5"; "-3"; ".5"; "1."; " 30 " ] do
      ExprCheck.plainNumber s |> should equal true
    for s in [ "4+8*$rank"; "$rank"; "(1)"; "1+2"; "abc"; ""; "-"; "1 2" ] do
      ExprCheck.plainNumber s |> should equal false

  [<Test>]
  member _.``ただの数は読める``() =
    // **2 つ の口が食い違わない。** ただの数だと言うなら、読めなければ嘘
    for s in [ "30"; "0"; "1.5"; "-3"; ".5" ] do
      ExprCheck.readable s |> should equal true

  // --- 出す先 ----------------------------------------------------------------

  [<Test>]
  member _.``畳んで数になる式にだけ出す``() =
    let hs = lang.Hints src
    hs |> List.map (fun h -> h.Text) |> should equal [ "4+8*$rank"; "360/(4+8*$rank)" ]

  [<Test>]
  member _.``ただの数には出ない``() =
    lang.Hints src |> List.exists (fun h -> h.Text = "2") |> should equal false

  [<Test>]
  member _.``$rand を含む式には出ない``() =
    lang.Hints src |> List.exists (fun h -> h.Text.Contains "$rand") |> should equal false

  [<Test>]
  member _.``読めない式には出ない``() =
    let broken = src.Replace("4+8*$rank", "4+*8")
    lang.Hints broken |> List.exists (fun h -> h.Text = "4+*8") |> should equal false

  [<Test>]
  member _.``位置は式の終わりの次``() =
    match lang.Hints src with
    | first :: _ ->
        // `    <repeat><times>4+8*$rank</times>` は 4 行 目。
        // `<times>` の `>` が 19 桁 目 なので中身は 20 桁 目 から 9 文字 ——
        // 指すのは**閉じ札の `<` と同じ 29 桁 目**（式の終わりの次）
        first.Line |> should equal 4
        first.Column |> should equal 29
    | [] -> failwith "1 つ も出ない"

  // --- 行桁を 1 巡 で出す -----------------------------------------------------

  [<Test>]
  member _.``行桁は 1 つ ずつ引いても並びで引いても同じ``() =
    for (_, text) in corpus |> List.truncate 40 do
      let names = vocab.Elements |> List.filter (fun e -> e.Text) |> List.map (fun e -> e.Name)
      let stops = XmlScan.texts text names |> List.map (fun h -> h.Stop)
      let one = stops |> List.map (Scan.lineColumn text)
      let many = Scan.lineColumnsAscending text stops
      many |> should equal one

  [<Test>]
  member _.``昇順が崩れていても正しい``() =
    // **崩れたら頭から数え直す。** 黙って間違えるより遅いほうを選ぶ
    let text = "a\nbb\nccc\ndddd"
    let offs = [ 10; 2; 6; 0 ]
    Scan.lineColumnsAscending text offs
    |> should equal (offs |> List.map (Scan.lineColumn text))

  // --- 同梱 ------------------------------------------------------------------

  [<Test>]
  member _.``同梱で出す先が在る``() =
    // **0 件 だと、上の点が全部「当てる先が無くて緑」になる**
    corpus |> List.sumBy (fun (_, t) -> List.length (lang.Hints t))
    |> should be (greaterThan 500)

  [<Test>]
  member _.``同梱では、出すのは式の 4 分の 1 くらい``() =
    let names = vocab.Elements |> List.filter (fun e -> e.Text) |> List.map (fun e -> e.Name)
    let all = corpus |> List.sumBy (fun (_, t) -> List.length (XmlScan.texts t names))
    let shown = corpus |> List.sumBy (fun (_, t) -> List.length (lang.Hints t))
    // 版の頭で 26.8% と数えた。**大きく動いたらここが赤くなる**
    let pct = 100 * shown / all
    pct |> should be (greaterThan 15)
    pct |> should be (lessThan 40)
