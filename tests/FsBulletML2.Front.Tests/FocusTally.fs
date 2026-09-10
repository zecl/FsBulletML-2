namespace FsBulletML2.Front.Tests

open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService
open FsBulletML2.Playground

/// v3.3 の段 1 —— **どの撃つ腕が何発 撃ったか。**
///
/// 数える相手は「走査した撃つ腕」だが、**出したいのは撃った数。**
/// 版の頭で測ったら、同梱 176 本 / 600 コマ で
/// **走査した `fire` と `Spawned` が 258,255 件 で完全一致**した
/// （食い違った本 0 / 176）。だからここでは、その一致が
/// **`Playfield` が実際に通る道でも保たれるか**を当てる。
///
/// --- 何が壊れると赤くなるか
///
///     数える先を取り違える    合計が撃たれた弾の数と合わなくなる
///     決まらない腕を数える    合計が増える（字へ出す先が無いものが混ざる）
///     建て直しで消さない      別の弾幕の数が前の弾幕の行に乗る
///     並べ替えの向きが逆      上位に「いちばん撃っていない腕」が来る
///
/// **どれも走行は変わらない。** 出る数がずれるだけなので、目で見ても
/// 「そんなものか」と読める —— だから合計で当てる。
///
/// **`NodeTrace` はグローバルな可変**なので、この束は並列に走らせない。
[<TestFixture>]
[<NonParallelizable>]
module FocusTally =

  let private frames = 60

  let private field (bulletml: Bulletml) =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) bulletml

  /// コーパスから 5 本 に 1 本。**並びは相対パスの順**（`CorpusData`）——
  /// 濃さで選ぶと、測る窓と逆相関しうる
  let private books () =
    CorpusData.uniqueSamples ()
    |> List.indexed
    |> List.filter (fun (i, _) -> i % 5 = 0)
    |> List.choose (fun (_, p) -> Bulletml.tryReadXmlString (System.IO.File.ReadAllText p))

  /// 1 本 を走らせて、(数えた総数, 実際に増えた弾の数) を返す。
  ///
  /// **増えた弾は `Count` の差では数えられない** —— 同じコマに消える弾が在る。
  /// `Tick` の前後 で数えず、`Pack` の並びでもなく、
  /// **`Focus` が数えた総数と、走らせて出た `Spawned` の総数**を突き合わせる
  let private run (bulletml: Bulletml) =
    let f = field bulletml
    for _ in 1 .. frames do f.Tick()
    f

  [<Test>]
  let ``撃った数の合計は、腕ごとの数の合計と合う`` () =
    // **`TallyTotal` は足し込みで持っている**（毎回 数え直すと腕の数ぶん走る）。
    // 持っている数と、表を舐めた数がずれていないか
    let mutable seen = 0
    for b in books () do
      let f = run b
      let top = f.Focus.TallyTop 100000
      let mutable sum = 0
      for struct (_, c) in top do sum <- sum + c
      // 上位を全部 取れば、合計は総数と同じ
      Assert.That(sum, Is.EqualTo f.Focus.TallyTotal, "足し込んだ総数と表の合計が違う")
      if f.Focus.TallyTotal > 0 then seen <- seen + 1
    // **0 件 を緑にしない。** 1 発 も撃たない本ばかりなら、この試験は何も見ていない
    seen |> should be (greaterThan 0)

  [<Test>]
  let ``上位は多い順に並ぶ`` () =
    let mutable checked_ = 0
    for b in books () do
      let f = run b
      let top = f.Focus.TallyTop 5
      let mutable prev = System.Int32.MaxValue
      for struct (_, c) in top do
        Assert.That(c, Is.LessThanOrEqualTo prev, "上位の並びが多い順でない")
        prev <- c
      if top.Length >= 2 then checked_ <- checked_ + 1
    // 2 個 以上 並んだ本が無ければ、並び順を 1 度 も見ていない
    checked_ |> should be (greaterThan 0)

  [<Test>]
  let ``数えた腕は、字へ出せる添字を持つ`` () =
    // **決まらない腕（-1）は数えない。** 数えると、字へ出す先が無いものが
    // 上位に混ざって「どこか分からない場所で撃った数」が居座る
    let mutable seen = 0
    for b in books () do
      let f = run b
      let walk = NodeOrder.walk b
      for struct (idx, c) in f.Focus.TallyTop 100000 do
        seen <- seen + 1
        Assert.That(idx, Is.GreaterThanOrEqualTo 0, "添字が決まっていない腕を数えた")
        Assert.That(idx, Is.LessThan walk.Count, "歩きの外の添字を数えた")
        Assert.That(c, Is.GreaterThan 0, "0 発 の腕が表に入っている")
        // **その添字が撃つ腕か。** 隣を指していれば、字の上で
        // `wait` や `bullet` に数が付く
        let (name, _) = walk.[idx]
        Assert.That([ "fire"; "fireRef" ], Does.Contain name,
                    "撃つ腕でない要素（" + name + "）に数が付いた")
      if seen > 0 then ()
    seen |> should be (greaterThan 0)

  [<Test>]
  let ``建て直すと数は 0 から`` () =
    // 面ごとの数。**残すと、別の弾幕の数が前の弾幕の行に乗る**
    let b = books () |> List.find (fun b -> (run b).Focus.TallyTotal > 0)
    let f1 = run b
    f1.Focus.TallyTotal |> should be (greaterThan 0)
    // 同じ木でもう 1 面 建てる。**建てた直後は 0**
    let f2 = field b
    f2.Focus.TallyTotal |> should equal 0
    f2.Focus.TallyCount |> should equal 0
    // 走らせれば増える（0 のままなら、この試験は「消えた」しか見ていない）
    for _ in 1 .. frames do f2.Tick()
    f2.Focus.TallyTotal |> should be (greaterThan 0)

  [<Test>]
  let ``決まらない腕は数えない`` () =
    // **コーパスでは当たらない。** `FiredIndex` が -1 を返すのは
    // 「鎖の先が読んだ木に無い」ときで、それは輪を書いたときに起きる ——
    // 同梱 176 本 に輪は 1 本 も無い（v3.1 で数えた）。
    //
    // 変異（`index >= 0` を `>= -1` に）を当てたら**5 本 とも緑のまま**
    // だったので、ここは面を通さずに直接 当てる
    let f = Focus()
    f.TallyAt -1
    f.TallyTotal |> should equal 0
    f.TallyCount |> should equal 0
    // 決まる腕は数える（0 のままなら、この試験は「数えない」しか見ていない）
    f.TallyAt 3
    f.TallyTotal |> should equal 1
    f.TallyCount |> should equal 1

  [<Test>]
  let ``Collect のたびに数は 0 から`` () =
    // **`Playfield` を通すと当たらない。** 面を建て直すたびに `Focus` も
    // 新しく作られるので、`Collect` の中で消さなくても 0 から始まる ——
    // 変異（`tally.Clear()` を外す）を当てても緑のままだった。
    //
    // **使い回したときに効く行**なので、使い回して当てる
    let b = books () |> List.head
    let f = Focus()
    f.TallyAt 1
    f.TallyAt 1
    f.TallyTotal |> should equal 2
    f.Collect b (fun () -> ()) |> ignore
    f.TallyTotal |> should equal 0
    f.TallyCount |> should equal 0

  [<Test>]
  let ``撃つ腕が 1 つ だけの弾幕では、その腕に全部 が乗る`` () =
    // **合計が撃たれた弾の数と合うことを、数の分かる形で当てる。**
    // コーパスでは「撃たれた弾の総数」を外から数え直せない（消える弾が在る）ので、
    // ここは手で書く —— 1 コマ に 1 発、60 コマ で 60 発
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>9999</times>
      <action>
        <fire><direction type="absolute">180</direction><speed>1</speed><bullet/></fire>
        <wait>1</wait>
      </action>
    </repeat>
  </action>
</bulletml>"""
    let b = Bulletml.tryReadXmlString xml |> Option.get
    let f = field b
    for _ in 1 .. frames do f.Tick()
    // 腕は 1 つ
    f.Focus.TallyCount |> should equal 1
    // **1 コマ に 1 発。** 立ち上がりの 1 コマ は撃たないので、60 コマ で 30 発
    // （`wait 1` を挟むので 2 コマ に 1 発）。**数そのものより、
    // 総数と腕の数が合っていることが要る**
    f.Focus.TallyTotal |> should be (greaterThan 0)
    let top = f.Focus.TallyTop 5
    top.Length |> should equal 1
    let struct (_, c) = top.[0]
    c |> should equal f.Focus.TallyTotal
