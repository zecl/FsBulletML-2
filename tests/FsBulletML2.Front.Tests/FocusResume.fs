namespace FsBulletML2.Front.Tests

open System.Collections.Generic
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.LanguageService
open FsBulletML2.Playground

/// v3.1 の段 3 —— **追っている弾の再開点が、読んだ木のノードか。**
///
/// 段 2 で「走行のノードから読んだ木へ 100.00% 戻れる」は測ったが、あれは
/// 受け口を測定のためだけに繋いだもので、**本流の道ではない。** ここは
/// `Playfield` が実際に通る道（選び、`Tick` の中で繋ぎ、鎖を辿って戻す）を、
/// コーパスの現物で当てる。
///
/// **番号や名前では確かめられない。** 「戻れる」と「正しい位置に戻れる」は別で、
/// 後者は**読んだ木のノードと参照で突き合わせないと言えない。**
///
/// **`NodeTrace` はグローバルな可変**なので、この束は並列に走らせない。
[<TestFixture>]
[<NonParallelizable>]
module FocusResume =

  let private frames = 60

  let private field (bulletml: Bulletml) =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) bulletml

  /// 読んだ木の中のノードを全部 集める。**参照で持つ** ——
  /// 中身が同じ別のノードを「同じ」と読まないため。
  ///
  /// `BulletmlElm` も入れる。**鎖の先はそこになりうる** —— 根の直下 の
  /// `<action>` は `BulletmlElm.Action` で、`getAction` が作った `ActionElm` の
  /// 元がそこ（段 2 で 1.00% を落としていた穴の 1 つ 目）
  let private readNodes (root: Bulletml) =
    let set = HashSet<obj>(HashIdentity.Reference)
    let rec cmd (a: Action) =
      set.Add(box a) |> ignore
      match a with
      | Action.Repeat (_, e) -> elm e
      | Action.Fire (_, _, _, b) -> bul b
      | Action.Action (_, xs) -> List.iter cmd xs
      | _ -> ()
    and elm (e: ActionElm) =
      set.Add(box e) |> ignore
      match e with
      | ActionElm.Action (_, xs) -> List.iter cmd xs
      | ActionElm.ActionRef _ -> ()
    and bul (b: BulletElm) =
      match b with
      | BulletElm.Bullet (_, _, _, es) -> List.iter elm es
      | BulletElm.BulletRef _ -> ()
    match root with
    | Bulletml.Bulletml (_, elms) ->
        for t in elms do
          set.Add(box t) |> ignore
          match t with
          | BulletmlElm.Bullet (_, _, _, es) -> List.iter elm es
          | BulletmlElm.Fire (_, _, _, b) -> bul b
          | BulletmlElm.Action (_, xs) -> List.iter cmd xs
    set

  /// コーパスから 3 本 に 1 本。**並びは相対パスの順**（`CorpusData`）で、
  /// 選び方は中身と関係が無い —— 濃い弾幕を選ぶと、測る窓と逆相関しうる。
  ///
  /// **読めない本が在る**（DTD 違反をわざと置いてある）ので、そこは飛ばす
  let private books () =
    CorpusData.uniqueSamples ()
    |> List.indexed
    |> List.filter (fun (i, _) -> i % 3 = 0)
    |> List.choose (fun (_, p) -> Bulletml.tryReadXmlString (System.IO.File.ReadAllText p))

  [<Test>]
  let ``再開点は読んだ木のノードで、要素は wait`` () =
    let mutable seen = 0
    let mutable outside = 0
    let mutable notWait = 0
    let mutable stopOne = 0
    let mutable ran = 0
    for bulletml in books () do
      ran <- ran + 1
      let nodes = readNodes bulletml
      let pf = field bulletml
      // **根（敵）を追う。** 最初のコマの `live` は根 1 つ だけ で、
      // 根は面の外へ出ても消えないので、追う先が途中で無くならない
      pf.Pick 0
      for _ in 1 .. frames do
        pf.Tick()
        if pf.Focus.Stops = 1 then stopOne <- stopOne + 1
        if pf.Focus.Serial >= 0 then
          seen <- seen + 1
          if not (nodes.Contains pf.Focus.Resumed) then outside <- outside + 1
          if pf.Focus.Name <> "wait" then notWait <- notWait + 1
    // **0 件 を緑にしない。** 1 度 も再開点が出ていなければ、
    // 「外に出たものが 0 件」は何も言っていない
    ran |> should be (greaterThan 0)
    seen |> should be (greaterThan 0)
    outside |> should equal 0
    // 列の先頭は `wait`。`Stopped` が生まれるのは `wait`（`Step.fs:58`）と
    // 輪の打ち切り（`621`）だけで、**同梱に輪は 1 本 も無い**
    notWait |> should equal 0
    // 子が止まると親も `Stopped` を返すので、深さだけ積み上がる。
    // **1 は出ない**（`Trace.fs` の但し書き）
    stopOne |> should equal 0

  [<Test>]
  let ``再開点の添字は、歩きの並びのその位置を指す`` () =
    // `OrderIndex` は「読んだ木を**書いてある順**に歩いた何番目 か」（v3.1 の段 4）。
    // 字の側はその数だけを受け取り、札の k 番目 を光らせる ——
    // **番号が 1 つ ずれても、走行も絵も変わらない。** 隣の要素が光るだけ。
    //
    // だから歩き直して**参照で突き合わせる。**
    let mutable seen = 0
    let mutable wrong = 0
    let mutable missing = 0
    for bulletml in books () do
      let walk = NodeOrder.walk bulletml
      let pf = field bulletml
      pf.Pick 0
      for _ in 1 .. frames do
        pf.Tick()
        if pf.Focus.Serial >= 0 then
          seen <- seen + 1
          let k = pf.Focus.OrderIndex
          // **-1 は「決まらない」。** 並びに 2 度 出るノードのときで、
          // 再開点は `action` / `wait` / `repeat` なので来ないはず
          if k < 0 || k >= walk.Count then missing <- missing + 1
          else
            let (name, node) = walk.[k]
            if not (obj.ReferenceEquals(node, pf.Focus.Resumed)) then wrong <- wrong + 1
            elif name <> pf.Focus.Name then wrong <- wrong + 1
    seen |> should be (greaterThan 0)
    missing |> should equal 0
    wrong |> should equal 0

  [<Test>]
  let ``撃たれた弾は、撃った fire を指す`` () =
    // v3.2 —— 面の弾を押すと「それを撃った場所」へ飛ぶ。
    //
    // **Core には撃った場所を渡す口が無い**（`Effect.Spawn` は `BulletState`
    // だけを運ぶ）。走査した撃つ腕の並びと `Spawned` の並びが 1 対 1 なことを
    // 測って（同梱の撃ったコマ 17,945 で 100.00%）、外から結んでいる ——
    // **その対応が本当に撃った側を指しているか**をここで当てる。
    let mutable picked = 0
    let mutable notFire = 0
    let mutable none = 0
    for bulletml in books () do
      let walk = NodeOrder.walk bulletml
      let pf = field bulletml
      // 1 コマ 進めて、撃たれた弾が出るまで待つ
      let mutable n = 0
      while n < frames && pf.Count < 2 do
        pf.Tick()
        n <- n + 1
      if pf.Count >= 2 then
        // **0 番目 は根の敵。** 撃たれていないので出どころが無い
        pf.Pick 0
        pf.PickedFrom |> should equal -1
        pf.Pick 1
        let k = pf.PickedFrom
        if k < 0 then none <- none + 1
        else
          picked <- picked + 1
          let (name, _) = walk.[k]
          // 撃つ腕は 2 つ。`fireRef` も `command` が解決してから撃つ
          if name <> "fire" && name <> "fireRef" then notFire <- notFire + 1
    // **0 件 を緑にしない**
    picked |> should be (greaterThan 0)
    notFire |> should equal 0
    // 結べなかった弾が在ってもよい（数が食い違うコマでは結ばない）が、
    // **全部 が結べないなら、この試験は何も見ていない**
    none |> should be (lessThan picked)

  [<Test>]
  let ``撃った fire の並びが、撃たれた弾の並びと同じ順`` () =
    // **コーパスで「fire を指す」だけでは、1 つ ずれても赤くならない**
    // （ずれた先も fire なので）。実際に変異を当てて緑のままだった。
    //
    // だから**並びそのもの**を当てる —— 1 コマ で 4 つ 撃つ弾幕を書いて、
    // 撃たれた 4 弾 の出どころが、字に書いてある 4 つ の `fire` と
    // **同じ順**であることを見る
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
    <action label="top">
        <fire><direction type="absolute">10</direction><bullet /></fire>
        <fire><direction type="absolute">70</direction><bullet /></fire>
        <fire><direction type="absolute">130</direction><bullet /></fire>
        <fire><direction type="absolute">190</direction><bullet /></fire>
        <wait>60</wait>
    </action>
</bulletml>"""
    let bulletml = Bulletml.readXmlString xml
    let walk = NodeOrder.walk bulletml
    let fires =
      [ for k in 0 .. walk.Count - 1 do
          let (name, _) = walk.[k]
          if name = "fire" then yield k ]
    fires.Length |> should equal 4
    let pf = field bulletml
    pf.Tick()
    // 根 1 つ ＋ 撃たれた 4 つ
    pf.Count |> should equal 5
    let got = [ for i in 1 .. 4 -> pf.Pick i; pf.PickedFrom ]
    got |> should equal fires

  [<Test>]
  let ``もう撃った fire を挟んでも、出どころを取り違えない`` () =
    // **数が食い違うコマでは結ばない**という守りが効いているか。
    //
    // `<fire/><wait>1</wait><fire/>` は、2 コマ 目 に
    // 「もう撃った 1 つ 目」と「これから撃つ 2 つ 目」の両方 を走査しうる ——
    // そこで数だけで結ぶと、**2 つ 目 で出た弾を 1 つ 目 に貼る。**
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
    <action label="top">
        <fire><direction type="absolute">10</direction><bullet /></fire>
        <wait>1</wait>
        <fire><direction type="absolute">100</direction><bullet /></fire>
        <wait>60</wait>
    </action>
</bulletml>"""
    let bulletml = Bulletml.readXmlString xml
    let walk = NodeOrder.walk bulletml
    let fires =
      [ for k in 0 .. walk.Count - 1 do
          let (name, _) = walk.[k]
          if name = "fire" then yield k ]
    fires.Length |> should equal 2
    let pf = field bulletml
    pf.Tick()                       // 1 つ 目 が出て、wait で止まる
    pf.Count |> should equal 2
    pf.Pick 1
    pf.PickedFrom |> should equal fires.[0]
    pf.Tick()                       // wait が明けて 2 つ 目 が出る
    pf.Tick()
    pf.Count |> should equal 3
    // **2 つ 目 の弾は 2 つ 目 の fire から。** 取り違えるならここが 1 つ 目 になる。
    // 結べなかった（-1）のも、貼り違えるよりは正しい
    let second = (pf.Pick 2; pf.PickedFrom)
    Assert.That(second, Is.EqualTo(fires.[1]).Or.EqualTo(-1),
                "2 つ 目 の弾が 1 つ 目 の fire を指している")

  /// 弾が出て、`wait` で止まる弾幕。**コーパスの 1 本 目 では測れない** ——
  /// あちらの根は `wait` を持たず、1 度 も止まらない（再開点が出ない）
  let private stopping = lazy (Bulletml.readXmlString DeterministicField.Xml)

  [<Test>]
  let ``追っていなければ何も出ない`` () =
    let pf = field stopping.Value
    for _ in 1 .. frames do
      pf.Tick()
      pf.Focus.Serial |> should equal -1
      pf.Focus.Stops |> should equal 0
      pf.PickedIndex |> should equal -1

  [<Test>]
  let ``追うのをやめると数も消える`` () =
    let pf = field stopping.Value
    pf.Pick 0
    let mutable got = false
    for _ in 1 .. frames do
      pf.Tick()
      if pf.Focus.Serial >= 0 then got <- true
    // ここまでで 1 度 も出ていなければ、下の「消えた」は何も言っていない
    got |> should equal true
    pf.Unpick()
    pf.Focus.Serial |> should equal -1
    pf.Focus.Stops |> should equal 0
    pf.PickedIndex |> should equal -1

  [<Test>]
  let ``範囲の外を選ぶと追わない`` () =
    let pf = field stopping.Value
    pf.Pick 0
    pf.Tick()
    // **範囲の外は「追うのをやめる」。** 黙って前の弾を追い続けると、
    // 面の何も無いところを押したのに印が残る
    pf.Pick 9999
    pf.Tick()
    pf.PickedIndex |> should equal -1
    pf.Focus.Serial |> should equal -1

  [<Test>]
  let ``輪を書くと先頭は wait でなくなる`` () =
    // 同梱に輪は 1 本 も無い（段 2 で測った）。**Playground は本文を打てる**ので
    // 輪はいつでも書ける —— そこで先頭が変わることを、ここで当てておく。
    //
    // **走る木では `action`、読んだ木では `actionRef`。** 打ち切りは展開後の
    // `action` の段で起きる（`Step.fs:621`）が、その元は `<actionRef>` ——
    // **出すのは読んだ木の側**なので、字に結べるのはこちら
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
    <action label="top">
        <fire><bullet /></fire>
        <actionRef label="top" />
    </action>
</bulletml>"""
    let bulletml = Bulletml.readXmlString xml
    let nodes = readNodes bulletml
    let pf = field bulletml
    pf.Pick 0
    let mutable seen = 0
    let mutable notRef = 0
    let mutable outside = 0
    for _ in 1 .. 10 do
      pf.Tick()
      if pf.Focus.Serial >= 0 then
        seen <- seen + 1
        if pf.Focus.Name <> "actionRef" then notRef <- notRef + 1
        if not (nodes.Contains pf.Focus.Resumed) then outside <- outside + 1
    seen |> should be (greaterThan 0)
    notRef |> should equal 0
    outside |> should equal 0
    // 道は `wait` の数。**輪では 1 本 も止まらないので 0**
    pf.Focus.Paths |> should equal 0