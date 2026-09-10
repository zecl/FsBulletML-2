namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService
open FsBulletML2.Playground

/// 弾の系譜（v3.6）—— **この弾を撃った弾を、根まで辿る。**
///
/// v3.2 の `From` は「撃った `fire` の書いてある順の添字」で、
/// **同じ `fire` から撃たれた弾は全部 同じ数になる** —— 親の弾は指していない。
/// だから親そのものを持たせて、辿る。
///
/// --- 何が壊れると赤くなるか
///
///     親を繋がない          系譜が 1 段 で止まる
///     並びが逆              根に近い順 でなくなる（読むと逆から辿ることになる）
///     -1 を混ぜる           撃たれていない段（根）が並びに出る
///     深さと長さがずれる    「決まらなかった段」を落とし忘れる
///
/// **走行は変わらない。** 出る数がずれるだけなので、目で見ても読めない。
///
/// **`NodeTrace` はグローバルな可変**なので、この束は並列に走らせない。
[<TestFixture>]
[<NonParallelizable>]
module BulletLineage =

  /// 2 段 に撃つ弾幕。**速さ 0** で置いたままにする ——
  /// 動くと画面の外へ出て消え（`Playfield` の消し）、選べなくなる。
  ///
  /// `wait` を長く取るのは、撃ったあとも生きていてほしいから
  let private xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire>
      <speed>0</speed>
      <bullet>
        <action>
          <wait>2</wait>
          <fire>
            <speed>0</speed>
            <bullet/>
          </fire>
          <wait>300</wait>
        </action>
      </bullet>
    </fire>
    <wait>300</wait>
  </action>
</bulletml>"""

  let private book () = Bulletml.tryReadXmlString xml |> Option.get

  let private field () =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) (book ())

  /// 書いてある順に歩いたとき、`fire` が何番目 か（外側 -> 内側）
  let private fireIndexes () =
    let walk = NodeOrder.walk (book ())
    [ for k in 0 .. walk.Count - 1 do
        let (name, _) = walk.[k]
        if name = "fire" then yield k ]

  /// n コマ 走らせて、深さ d の弾を 1 つ 選ぶ。**無ければ None**
  let private pickAtDepth (f: Playfield) (d: int) =
    let mutable found = -1
    let mutable i = 0
    while found < 0 && i < f.Count do
      f.Pick i
      if f.PickedDepth = d then found <- i
      i <- i + 1
    if found < 0 then (f.Unpick(); None) else (f.Pick found; Some found)

  [<Test>]
  let ``根の弾は系譜を持たない`` () =
    let f = field ()
    // 建てた直後は根だけ
    f.Count |> should equal 1
    f.Pick 0 |> ignore
    f.PickedDepth |> should equal 1
    f.PickedLineage.Length |> should equal 0

  [<Test>]
  let ``撃たれた弾の系譜は、撃った fire を 1 つ`` () =
    let fires = fireIndexes ()
    fires.Length |> should equal 2
    let f = field ()
    for _ in 1 .. 2 do f.Tick()
    match pickAtDepth f 2 with
    | None -> Assert.Fail "深さ 2 の弾が出ていない"
    | Some _ ->
        f.PickedLineage |> should equal [| List.head fires |]

  [<Test>]
  let ``孫の系譜は、根に近い順で 2 つ`` () =
    // **ここが本体。** 1 段 で止まっていないこと、順が逆でないことを同時に見る
    let fires = fireIndexes ()
    let f = field ()
    for _ in 1 .. 8 do f.Tick()
    match pickAtDepth f 3 with
    | None -> Assert.Fail "深さ 3 の弾が出ていない"
    | Some _ ->
        let got = f.PickedLineage
        got.Length |> should equal 2
        // **外側 の fire が先。** 逆だと「そこから、ここから」と読むことになる
        got |> should equal [| fires.[0]; fires.[1] |]

  [<Test>]
  let ``系譜の長さは、辿れた段より 1 つ 少ない`` () =
    // 根は撃たれていないので `From` を持たない —— **並びに -1 を混ぜない**
    let f = field ()
    for _ in 1 .. 8 do f.Tick()
    let mutable seen = 0
    for i in 0 .. f.Count - 1 do
      f.Pick i
      let d = f.PickedDepth
      let n = f.PickedLineage.Length
      d |> should be (greaterThan 0)
      Assert.That(n, Is.EqualTo(d - 1), "深さ " + string d + " の弾の系譜が " + string n)
      seen <- seen + 1
    f.Unpick()
    // 0 件 を緑にしない
    seen |> should be (greaterThan 1)

  [<Test>]
  let ``追うのをやめると系譜も消える`` () =
    let f = field ()
    for _ in 1 .. 8 do f.Tick()
    pickAtDepth f 3 |> ignore
    f.PickedLineage.Length |> should be (greaterThan 0)
    f.Unpick()
    f.PickedLineage.Length |> should equal 0
    f.PickedDepth |> should equal 0
