namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Playground

/// **参照の欠けを、本文の字から全部 数える側の目盛り。**
///
/// Core は最初の 1 件 で `raise` して止まり、位置も持たない。だから
/// 「無い label」を全部 波線にするには本文の側から数えるしかなく、
/// **Core の判定をもう 1 本 書いている**ことになる。
///
/// **`missing` は Core より広い。** Core は `top` から到達した要素しか
/// 展開しないので、誰も参照していない枝の中の壊れた参照を素通りする
/// （コーパスの `readTest.xml` が実際にそう ——
/// `<bulletml>` 直下の `<fire label="topFire">` が `top` から辿れない）。
/// **だから「Core が落ちる ⇔ ここが挙げる」ではない。**
///
/// 割れる向きを、本番の道（`References.explain`）で塞ぐ。
///
///   嘘の波線  Apply が通る弾幕に 1 本 でも引いたら嘘
///             -> コーパス全部 を `explain` に通して 0 本
///   見落とし  Core が落ちる名前を挙げない
///             -> 参照を 1 つ ずつ壊して、Core が落ちた回だけ突き合わせる
///
/// 走る先（`actionRef` -> `action` など）も表を持たず語彙から導いている。
/// **導けなければ 1 件 も挙げない**ので、0 件 が緑にならないよう
/// 対の数そのものを見る点を先に置く。
[<TestFixture>]
type ReferenceScan() =

  let rand () = 0.5f
  let rank = 0.5f
  let build (b: Bulletml) = Runner.load rand rank b |> ignore
  let coreFails xml = (Diagnosis.apply build xml).IsSome
  /// 本番と同じ道。`Main.fs` の `ApplySource` もこれを通る
  let explain xml = References.explain (Diagnosis.apply build xml) xml

  let corpus =
    lazy
      let root = Path.Combine(AppContext.BaseDirectory, "TestData", "xml")
      if Directory.Exists root
      then Directory.EnumerateFiles(root, "*.xml", SearchOption.AllDirectories) |> Seq.toArray
      else [||]

  /// 参照の label を 1 つ だけ、在り得ない名前に差し替える。
  /// `[^>]*?` なのでタグを跨がない
  let refAttr = Regex("<(actionRef|fireRef|bulletRef)([^>]*?)label\\s*=\\s*\"([^\"]*)\"")

  // --- 走る先が導けているか -------------------------------------------------

  [<Test>]
  member _.``対が語彙から導けている``() =
    // ここが 0 だと下の点は全部「挙げないから緑」になる
    References.pairs.Length |> should greaterThan 0
    let names = References.pairs |> Array.map (fun (r, d, a) -> sprintf "%s->%s@%s" r d a) |> Array.sort
    names |> should equal [| "actionRef->action@label"; "bulletRef->bullet@label"; "fireRef->fire@label" |]

  // --- 挙げる / 挙げない ----------------------------------------------------

  [<Test>]
  member _.``定義が在れば挙げない``() =
    References.missing "<bulletml><action label=\"a\"><wait>1</wait></action><action label=\"top\"><actionRef label=\"a\"/></action></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``無い参照を 2 つ とも挙げる``() =
    // **これが本題。** Core はここで 1 件 目 しか言わない
    let found =
      References.missing "<bulletml><action label=\"top\"><actionRef label=\"a\"/><actionRef label=\"b\"/></action></bulletml>"
    found.Length |> should equal 2
    found |> List.forall (fun f -> f.Line > 0) |> should be True

  [<Test>]
  member _.``同じ名前を 2 回 参照したら 2 本``() =
    // どちらも直す先なので、1 本 に畳まない
    (References.missing "<bulletml><action label=\"top\"><actionRef label=\"a\"/><actionRef label=\"a\"/></action></bulletml>").Length
    |> should equal 2

  [<Test>]
  member _.``fire と bullet も見る``() =
    let found =
      References.missing
        "<bulletml><action label=\"top\"><fireRef label=\"f\"/><fire><bulletRef label=\"b\"/></fire></action></bulletml>"
    found.Length |> should equal 2
    found |> List.map (fun f -> f.Message.Split(' ').[0]) |> List.sort
    |> should equal [ "bulletRef"; "fireRef" ]

  // --- 位置 -----------------------------------------------------------------

  [<Test>]
  member _.``位置が label の値そのものを指す``() =
    // 3 行目 の `<actionRef label="a"/>`。`a` は 19 桁目、閉じ引用符が 20
    let xml = "<bulletml>\n<action label=\"top\">\n<actionRef label=\"a\"/>\n</action>\n</bulletml>"
    match References.missing xml with
    | [ f ] ->
      f.Line |> should equal 3
      f.Column |> should equal 19
      f.EndColumn |> should equal 20
      f.Message |> should haveSubstring "a"
    | other -> failwithf "1 件 のはずが %d 件" other.Length

  [<Test>]
  member _.``行が動く``() =
    // いつも 1 を返す壊れ方が緑で通らないように、2 か所 の行を数える
    let xml =
      "<bulletml>\n<action label=\"top\">\n<actionRef label=\"a\"/>\n<wait>1</wait>\n<actionRef label=\"b\"/>\n</action>\n</bulletml>"
    References.missing xml |> List.map (fun f -> f.Line) |> should equal [ 3; 5 ]

  // --- 飛ばすもの -----------------------------------------------------------

  [<Test>]
  member _.``コメントの中は数えない``() =
    // **コメントの中に `>` を先に置く。** 置かないと、コメント専用の飛ばしを
    // 外しても `<!` の枝が `>` まで食って同じ結果になり、変異が当たらない
    References.missing "<bulletml><action label=\"top\"><!-- x > <actionRef label=\"a\"/> --><wait>1</wait></action></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``CDATA の中は数えない``() =
    // コメントと同じで、中に `>` を先に置かないと変異が `<!` の枝に吸われる
    References.missing "<bulletml><action label=\"top\"><wait><![CDATA[x > <actionRef label=\"a\"/>]]></wait></action></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``宣言と閉じ札を積まない``() =
    // `<?xml ...?>` と `</action>` をタグとして拾うと、名前が空の札が混ざる
    References.tags "<?xml version=\"1.0\"?><bulletml><action label=\"top\"></action></bulletml>"
    |> List.map (fun t -> t.TagName)
    |> should equal [ "bulletml"; "action" ]

  [<Test>]
  member _.``引用符の中の 大なり に騙されない``() =
    // 値の中の `>` でタグが終わったことにすると、次の属性を取り落とす
    let tags = References.tags "<action label=\"a>b\" x=\"1\"><actionRef label=\"a>b\"/></action>"
    tags |> List.map (fun t -> t.TagName) |> should equal [ "action"; "actionRef" ]
    tags.Head.Attrs |> List.length |> should equal 2
    // 定義側も参照側も同じ値なので、欠けは無い
    References.missing "<bulletml><action label=\"a>b\"><wait>1</wait></action><action label=\"top\"><actionRef label=\"a>b\"/></action></bulletml>"
    |> should be Empty

  [<Test>]
  member _.``閉じ引用符が無い属性は捨てる``() =
    // **位置が本文とずれるものを挙げない。** 読めていないまま挙げると嘘の波線
    References.missing "<bulletml><action label=\"top\"><actionRef label=\"a/></action></bulletml>"
    |> should be Empty

  // --- コーパスと突き合わせる -----------------------------------------------

  [<Test>]
  member _.``コーパスが読めている``() =
    // 下の 2 点 は「1 本 も読めなくても緑」になる
    corpus.Value.Length |> should greaterThan 0

  [<Test>]
  member _.``Apply が通る弾幕には 1 本 も引かない``() =
    // **本番の道で見る。** `missing` 単独だと、到達しない枝の壊れた参照を
    // 挙げてしまう（Core より広い）。`explain` は Core が通れば呼ばない
    let noisy =
      corpus.Value
      |> Array.choose (fun file ->
          let xml = File.ReadAllText file
          match explain xml with
          | [] -> None
          | fs when coreFails xml -> ignore fs; None   // 読めない形も置いてある並び
          | fs -> Some(sprintf "%s: %s" (Path.GetFileName file) fs.Head.Message))
      |> Array.toList
    noisy |> should be Empty

  [<Test>]
  member _.``構文が壊れているときは、構文の理由だけを出す``() =
    // 本文が読めていないので、字から数えた位置は当てにならない。
    // **参照の欠けが本文に在っても、そちらへ乗り換えない**
    let xml = "<bulletml><action label=\"top\"><actionRef label=\"nope\"/><fire>"
    (References.missing xml).Length |> should equal 1   // 単独なら挙げる
    match explain xml with
    | [ f ] ->
      f.Line |> should greaterThan 0
      f.Message |> should not' (haveSubstring "nope")
    | other -> failwithf "1 本 のはずが %d 本" other.Length

  [<Test>]
  member _.``到達しない枝の壊れた参照は、Apply が通るので波線にならない``() =
    // **Core と食い違う唯一 の向きを、字で固定する。** ここが赤くなったら
    // Core の展開が変わったということ（到達を見るようになった、など）
    let xml =
      "<bulletml><action label=\"top\"><wait>1</wait></action>"
      + "<fire label=\"orphan\"><bulletRef label=\"nope\"/></fire></bulletml>"
    coreFails xml |> should be False
    // `missing` 単独では挙げる。**広いこと自体は間違いではない**
    (References.missing xml).Length |> should equal 1
    // 本番の道では出ない
    explain xml |> should be Empty

  [<Test>]
  member _.``参照を 1 つ 壊すと、Core が落ちて、同じ名前を挙げる``() =
    // 見落とし。**Core が真**で、こちらは位置を足すだけ
    let mutable measured = 0
    let mutable skipped = 0
    let bad = ResizeArray<string>()
    for file in corpus.Value do
      let xml = File.ReadAllText file
      if not (coreFails xml) then
        for m in refAttr.Matches xml do
          let broken =
            xml.Substring(0, m.Groups.[3].Index)
            + "__nope__"
            + xml.Substring(m.Groups.[3].Index + m.Groups.[3].Length)
          // 壊しても Core が通るのは、そこが到達しない枝だったとき。
          // **測定材料にならないので飛ばす**（飛ばした数は下で見る）
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
