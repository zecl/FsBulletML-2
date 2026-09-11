namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front
open FsBulletML2.Playground

/// **台本の無い弾は、エンジンを呼ばずに位置を足すだけ**（v4.9.1）。
///
/// --- なぜ言えるのか
///
/// 1 コマ の差分は `Accel + (速さ x 向き)` の 1 本 で、`Speed` / `Dir` /
/// `Accel` を変えるのは命令だけ（`changeSpeed` / `changeDirection` / `accel`）。
/// 台本が 1 本 も無ければ命令は 1 つ も走らない ——
/// **その弾が消えるまで同じ値**（`BulletRun.ConstantDelta`）。
///
/// --- 見るのは「答えが変わらないこと」
///
/// 速くなったかは実機で測る。ここで見るのは**座標がビット一致すること**。
///
/// **相手は独立に書いた 1 本。** 面の中に「速い道を切る」旗を置くと、
/// 本番に無い形を門が固定することになる —— 代わりに
/// **毎コマ エンジンを呼ぶ面をここに書いて、2 つ を突き合わせる。**
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   `ConstantDelta` の `Sin` を `Cos` に          赤 4
///   `ConstantDelta` を いつも `ValueSome` に      赤 5
///   面の速い道から 面の外 の判定を落とす           赤 6
///   速い道で 再開点 を戻さない                     赤 1
///   速い道へ移すときの `not it.IsRoot` を外す      **赤 0**
///
/// **最後の 1 つ は冗長な守り**と分かった —— 台本を持たない根は速さが 0 で、
/// 差分が (0, 0) になる。`Playfield.fs` の但し書きに残してある。
///
/// --- 段 2（成分の配列）の較正
///
///   末尾の弾を死んだ枠へ移さない                 赤 1
///   差分の y を x と同じにする                   赤 2
///   撃った腕の添字を詰めた並びへ書かない          赤 4
///   伸ばすときに中身を写さない                   赤 1
///   追っている弾の添字を追随させない              赤 1（**はじめ 0 点**）
///   消した枠の参照を落とさない                   **赤 0**
///
/// **追随の変異は、はじめ 0 点 だった** —— 詰めが起きる形を作れていなかった
/// （末尾を選んでも、次のコマの産まれた弾が後ろ に付くので末尾でなくなる）。
/// **速い弾と遅い弾を 1 発 ずつ撃つ本文**に替えて、狙って起こした。
///
/// 最後の 1 つ は振る舞いに出ない（GC の話）。こちらも但し書きに残した。
///
/// --- v4.9.2（`wait` で止まっている弾）の較正
///
///   `repeat` の中の wait を静かと言う           赤 1
///   飛ばすぶんの wait を消化しない               赤 2
///   速い道の残りを減らさない                     赤 5
///   終わった子を飛ばさず、頭の子だけ見る          **赤 0**
///   `changeSpeed` の途中 を静かと言う            **赤 0**
///   `accel` の途中 を静かと言う                  **赤 0**
///   1 ずつでなく まとめて引く                    **赤 0**
///
/// **空振り 4 件 の理由は 1 つ。**
/// `Step.quietFirst` が**現在位置だけを見る**形になったので、
/// **`QBusy` と `QNone` は同じ効果**（どちらも「静かと言わない」）。
/// 木を全部 なめていた頃 は、後ろ に在る進行中の変化を拾うために
/// `QBusy` が要った —— **その形が 136 本 を割ったので捨てた。**
///
/// 腕は残してある。**「`accel` は静かではない」を字で言う場所**が要るのと、
/// 走査を全なめに戻した日に、腕が無いと静かに壊れるため。
///
/// 「1 ずつ引く」も同じで、同梱 176 本 では丸めの差が出なかった ——
/// **出うるのは事実**なので残す（`Step.minusOnes` の但し書き）。
///
/// --- v4.9.3（一定の割合で変わる弾）の較正
///
///   `changeSpeed` の終わるコマ も飛ばす        赤 2
///   `changeDirection` を線形と言う             赤 1
///   `wait` で止まることを要る形に戻す           赤 2
///   `Speed` を 1 回 でまとめて足す             赤 3
///   向きを float32 で持つ                      赤 1
///   詰めるときに姿を写さない                    赤 1
///   線形の残りを減らさない                      赤 2
///   `accel` の残りを 1 少なくする              **赤 0**
///
/// **空振りは 1 件 だけ。** `accel` を 1 コマ 短く飛ばすのは
/// **保守的に外れる**変異で、答えは変わらない（乗る弾が減るだけ）。
///
/// 「向きを float32 で持つ」が赤 1 なのが効いている ——
/// `Step.fs` の出口 は `float32 (Math.Sin dir * speed)` と
/// **double で掛けてから落とす**ので、**持ち方を 1 段 落とすと 1 ビット ずれる。**
////// 「終わった子を飛ばさず」は**保守的に外れる**変異で、答えは変わらない
/// （現在位置でない子は `QNone` になり、その弾が素の道へ落ちるだけ）。
/// **速さは落ちるが、赤くはならない。**
[<TestFixture>]
type ConstStep() =

  /// **毎コマ エンジンを呼ぶ面。** `Playfield.Tick` と同じ順で回す ——
  /// 弾を回し、差分を足し、`Finished` なら走らせ直し、産まれた弾は次のコマから
  static let reference (front: IFrontEnv) (bulletml: Bulletml) (frames: int) =
    let script = Runner.load front.Rand front.Rank bulletml
    let field = Stage.ofDirection script.ShootingDirection
    // (run, x, y, isRoot)
    let live = ResizeArray<BulletRun * float32 * float32 * bool>()
    live.Add(Runner.newRoot BulletType.Enemy script, field.EnemyX, field.EnemyY, true)
    let spawned = ResizeArray<BulletRun * float32 * float32 * bool>()
    let shots = ResizeArray<(float32 * float32)[]>()
    for _ in 1 .. frames do
      spawned.Clear()
      let mutable i = 0
      while i < live.Count do
        let (run, x, y, isRoot) = live.[i]
        let m0 = run.Motion
        let motion = { Pos = { X = x; Y = y }; Speed = m0.Speed; Dir = m0.Dir; Accel = m0.Accel }
        let f = Driver.step front Space.YDown SpawnOrigin.AtShooter run motion
        let nx = if isRoot then x else x + f.Delta.X
        let ny = if isRoot then y else y + f.Delta.Y
        let run2 = if f.Finished then Driver.restart front f.Run else f.Run
        for child in f.Spawned do
          let p = child.Motion.Pos
          spawned.Add(child, p.X, p.Y, false)
        let dead =
          not isRoot && (
            f.Vanished || f.Retired
            || nx < 0.0f || nx > field.Width
            || ny < 0.0f || ny > field.Height)
        if dead then
          let last = live.Count - 1
          if i <> last then live.[i] <- live.[last]
          live.RemoveAt last
        else
          live.[i] <- (run2, nx, ny, isRoot)
          i <- i + 1
      live.AddRange spawned
      shots.Add [| for (_, x, y, _) in live -> (x, y) |]
    shots

  /// 本番の面。**`Pack` を通さない** —— あちらは 3 つ 組 で、
  /// ここで見たいのは座標そのもの
  static let actual (front: IFrontEnv) (bulletml: Bulletml) (frames: int) =
    let f = Playfield.Create front bulletml
    let shots = ResizeArray<(float32 * float32)[]>()
    for _ in 1 .. frames do
      f.Tick()
      let n = f.Pack()
      let a = f.Packed
      shots.Add [| for k in 0 .. n - 1 -> (a.[k * 3], a.[k * 3 + 1]) |]
    shots

  static let sameShots (a: ResizeArray<(float32 * float32)[]>) (b: ResizeArray<(float32 * float32)[]>) =
    if a.Count <> b.Count then Some -1
    else
      let mutable bad = None
      let mutable i = 0
      while bad.IsNone && i < a.Count do
        if a.[i].Length <> b.[i].Length then bad <- Some i
        else
          let mutable k = 0
          while bad.IsNone && k < a.[i].Length do
            let (x1, y1) = a.[i].[k]
            let (x2, y2) = b.[i].[k]
            // **ビット一致で見る。** 近いかではなく同じか
            if x1 <> x2 || y1 <> y2 then bad <- Some i
            k <- k + 1
        i <- i + 1
      bad

  [<Literal>]
  static let Frames = 120

  /// 弾が 100 発 以上 出て、台本の無い弾が残る本文
  static let manyXml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>60</times>
      <action>
        <fire><direction type="sequence">13</direction><speed>1.3</speed><bullet/></fire>
        <fire><direction type="sequence">31</direction><speed>1.7</speed><bullet/></fire>
        <wait>1</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  /// **弾の action に長い `wait` を置く**（v4.9.2）。
  ///
  /// `repeat` の中では速い道に乗らない（`Step.quietOf` の但し書き ——
  /// `repeat` は毎コマ `times` を評価するので `$rand` を読みうる）。
  /// ここで乗せたいのは**弾が持つ台本の wait** なので、repeat の外 に置く。
  ///
  /// `changeSpeed` を挟んであるのは、**その途中 を静かと言わないこと**を
  /// 同じ本文で踏むため —— あれは action をブロックせず、
  /// 止まって見えるまま速さが動く
  static let waitXml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>20</times>
      <action>
        <fire><direction type="sequence">17</direction><speed>1.5</speed>
          <bullet>
            <action>
              <wait>40</wait>
              <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
              <wait>100</wait>
            </action>
          </bullet>
        </fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>300</wait>
  </action>
</bulletml>"""

  /// **`accel` だけを持つ弾**（v4.9.3）。
  ///
  /// 同梱でいちばん多かった形がこれ ——
  /// `wait` が 1 つ も無く、`accel` が `Continue` を返し続けるので
  /// **action は同じ位置に留まる。**
  /// 「`wait` で止まるまで」を条件にしていたときは 1 発 も乗らなかった
  static let accelXml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>20</times>
      <action>
        <fire><direction type="sequence">17</direction><speed>1</speed>
          <bullet>
            <action>
              <accel><vertical>6</vertical><term>200</term></accel>
            </action>
          </bullet>
        </fire>
        <wait>3</wait>
      </action>
    </repeat>
    <wait>300</wait>
  </action>
</bulletml>"""

  [<Test>]
  member _.``台本が空なら、差分が出る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString manyXml
    let script = Runner.load front.Rand front.Rank bulletml
    let root = Runner.newRoot BulletType.Enemy script
    // **根はまだ台本を持っている**
    root.HasNoScript |> should equal false
    root.ConstantDelta.IsNone |> should equal true
    // 撃たれた弾を 1 発 拾う
    let mutable run = root
    let mutable found = ValueNone
    let mutable f = 0
    while found.IsNone && f < 10 do
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
      run <- fr.Run
      for c in fr.Spawned do
        if found.IsNone && c.HasNoScript then found <- ValueSome c
      f <- f + 1
    match found with
    | ValueNone -> failwith "台本の無い弾が出ない"
    | ValueSome c ->
      // **エンジンが返す差分と一致する。** 式を 2 か所 に書いているので、
      // 片方 だけ直したら割れる
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion
      c.ConstantDelta.IsSome |> should equal true
      c.ConstantDelta.Value.X |> should equal fr.Delta.X
      c.ConstantDelta.Value.Y |> should equal fr.Delta.Y

  /// **その差分が生涯 変わらない。** 変わるなら速い道は嘘になる
  [<Test>]
  member _.``台本が空の弾は、何コマ 進めても差分が変わらない``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString manyXml
    let script = Runner.load front.Rand front.Rank bulletml
    let mutable run = Runner.newRoot BulletType.Enemy script
    let mutable found = ValueNone
    let mutable f = 0
    while found.IsNone && f < 10 do
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
      run <- fr.Run
      for c in fr.Spawned do
        if found.IsNone && c.HasNoScript then found <- ValueSome c
      f <- f + 1
    match found with
    | ValueNone -> failwith "台本の無い弾が出ない"
    | ValueSome c0 ->
      c0.ConstantDelta.IsSome |> should equal true
      let fx = c0.ConstantDelta.Value.X
      let fy = c0.ConstantDelta.Value.Y
      let mutable c = c0
      for _ in 1 .. 100 do
        let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion
        // **撃たないし消えない**
        fr.Spawned |> should be Empty
        fr.Vanished |> should equal false
        fr.Finished |> should equal true
        fr.Delta.X |> should equal fx
        fr.Delta.Y |> should equal fy
        c <- (if fr.Finished then Driver.restart front fr.Run else fr.Run)
        c.ConstantDelta.IsSome |> should equal true
        c.ConstantDelta.Value.X |> should equal fx
        c.ConstantDelta.Value.Y |> should equal fy

  /// **いちばん強い点。** 同梱を 2 通り の面で走らせて、座標を突き合わせる
  [<Test>]
  member _.``同梱 全部 で、毎コマ 呼ぶ面と座標がビット一致する``() =
    // **同梱の弾幕は `samples` の xml から読む** ——
    // このプロジェクトは `Bullets.Dsl` を引いていない（公開だけで書く決めごと）
    let books = CorpusData.uniqueSamples ()
    let mutable compared = 0
    // **読めない本は数えて外す。** `samples` には DTD を通らない字も混じる
    // （`repeat` の中に action が 2 つ 在るもの）—— **黙って落とさない**
    let mutable unreadable = 0
    let bad = ResizeArray<string * int>()
    for path in books do
      let parsed =
        try ValueSome (Bulletml.readXmlString (System.IO.File.ReadAllText path))
        with _ -> ValueNone
      match parsed with
      | ValueNone -> unreadable <- unreadable + 1
      | ValueSome bulletml ->
        let a = reference (DeterministicField.env (DeterministicField.stream ())) bulletml Frames
        let b = actual (DeterministicField.env (DeterministicField.stream ())) bulletml Frames
        compared <- compared + 1
        match sameShots a b with
        | Some f -> bad.Add(System.IO.Path.GetFileName path, f)
        | None -> ()
    // **当てる先が在ることを、門が自分で数える**
    compared |> should be (greaterThan 100)
    // 外した数が増え続けたら、この点の分母が静かに痩せる
    unreadable |> should be (lessThan (compared / 4))
    bad |> List.ofSeq |> should be Empty

  /// **速い道を通る弾コマ が在ること**を数える ——
  /// 0 件 なら、上の点は「速い道を 1 度 も通らずに一致した」だけ
  [<Test>]
  member _.``同梱で、速い道を通る弾が在る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let f = Playfield.Create front (Bulletml.readXmlString manyXml)
    let mutable withConst = 0
    let mutable all = 0
    for _ in 1 .. Frames do
      f.Tick()
      all <- all + f.BulletCount
      withConst <- withConst + f.ConstCount
    all |> should be (greaterThan 1000)
    // 版の頭で数えた同梱の割合は 84.0%。ここは 1 本 の本文なので下限だけ見る
    withConst |> should be (greaterThan (all / 2))

  /// **追っている弾が速い道に乗っても、再開点は「無い」に戻る**（v3.1）。
  ///
  /// 素の道では `Begin` -> step -> `End` を通り、台本が空の弾は受け口を
  /// 1 度 も鳴らさないので `End` が全部 消していた ——
  /// 速い道でそこを飛ばすと**前のコマの再開点が残る**。
  /// **この点だけが、それを見ている**（座標は変わらないので上の点は緑のまま）
  [<Test>]
  member _.``速い道の弾を追っても、再開点が残らない``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let f = Playfield.Create front (Bulletml.readXmlString manyXml)
    // 弾が出て、速い道に乗るまで進める
    let mutable n = 0
    while n < 30 && f.ConstCount = 0 do
      f.Tick()
      n <- n + 1
    f.ConstCount |> should be (greaterThan 0)
    // **台本を持つ弾を先に追って、再開点を立てる** ——
    // 立っていないと、この点は「もともと空だった」を見るだけになる
    let mutable lit = false
    let mutable k = 0
    while not lit && k < f.Count do
      f.Pick k
      f.Tick()
      if f.Focus.Name <> "" then lit <- true else k <- k + 1
    lit |> should equal true
    // **速い道の弾へ移す。** `Pack` の並びで `From` が 0 以上 のものを探す
    let pickFast () =
      let cnt = f.Pack()
      let a = f.Packed
      let mutable found = -1
      let mutable i = 0
      while found < 0 && i < cnt do
        // 撃たれた弾（根でない）を選ぶ
        if a.[i * 3 + 2] >= 0.0f then found <- i
        i <- i + 1
      found
    let idx = pickFast ()
    idx |> should be (greaterThanOrEqualTo 0)
    f.Pick idx
    f.Tick()
    // **その弾が速い道に居るなら、再開点は空**
    if f.ConstCount > 0 then
      f.Focus.Name |> should equal ""

  /// **根は速い道へ移さない。** 移すと動き出す ——
  /// 素の道は `IsRoot` のとき差分を足さない。
  ///
  /// `top` で始まる名前の定義が 1 つ も無い弾幕なら、**根は最初から
  /// 台本を持たない**（`Semantics.NoEntryPoint` が波線を出す形）——
  /// この本文でだけ、その守りに当たる先ができる
  [<Test>]
  member _.``台本を持たない根は、動かない``() =
    let noTop = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="tsukawanai">
    <fire><direction type="absolute">180</direction><speed>2</speed><bullet/></fire>
  </action>
</bulletml>"""
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString noTop
    let script = Runner.load front.Rand front.Rank bulletml
    // **当てる先が在ることを、門が自分で数える**
    (Runner.newRoot BulletType.Enemy script).HasNoScript |> should equal true
    let f = Playfield.Create front bulletml
    let at () =
      let n = f.Pack()
      n |> should equal 1
      (f.Packed.[0], f.Packed.[1])
    let before = at ()
    for _ in 1 .. 60 do f.Tick()
    at () |> should equal before

  /// **追っている弾の添字が、消しで詰めたときに追随するか**（v4.9.1 の段 2）。
  ///
  /// 段 2 で `picked` を参照から**添字**に替えた。v3.1 の但し書きは
  /// 「添字を覚えると、消しで詰めたコマに黙って別の弾を指す」——
  /// **消すところが 1 か所 しか無いので、そこで直せば足りる**という賭け。
  ///
  /// **詰めが起きる形を狙って作る。** 速い弾が先に面から出て、
  /// そのあとの枠へ**末尾の遅い弾が移る** ——
  /// 追っているのがその遅い弾なら、添字は動かなければならない。
  ///
  /// 見るのは 2 つ
  ///
  ///     添字が動いたこと          動かなければ、この点は何も見ていない
  ///     座標が跳ばないこと        別の弾を指した瞬間、面の端から端へ跳ぶ
  [<Test>]
  member _.``追っている弾は、消しで詰めても同じ弾のまま``() =
    // **速い弾を先に、遅い弾を後に撃つ。** 撃つのは 1 度 だけ
    let twoSpeeds = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">180</direction><speed>30</speed><bullet/></fire>
    <fire><direction type="absolute">0</direction><speed>0.5</speed><bullet/></fire>
    <wait>500</wait>
  </action>
</bulletml>"""
    let front = DeterministicField.env (DeterministicField.stream ())
    let f = Playfield.Create front (Bulletml.readXmlString twoSpeeds)
    f.Tick()
    // 根 + 速い弾 + 遅い弾
    f.Count |> should equal 3
    // **末尾（遅い弾）を選ぶ**
    f.Pick 2
    f.PickedIndex |> should equal 2
    let at () =
      let n = f.Pack()
      let i = f.PickedIndex
      if i < 0 || i >= n then ValueNone
      else ValueSome (struct (f.Packed.[i * 3], f.Packed.[i * 3 + 1]))
    let mutable prev = at ()
    let mutable jumped = 0
    let mutable shifted = 0
    let mutable prevIdx = f.PickedIndex
    for _ in 1 .. 60 do
      f.Tick()
      let now = at ()
      match prev, now with
      | ValueSome (struct (x0, y0)), ValueSome (struct (x1, y1)) ->
          // 遅い弾は 1 コマ に 0.5 px。**5 px も動いたら別の弾**
          if abs (x1 - x0) > 5.0f || abs (y1 - y0) > 5.0f then jumped <- jumped + 1
      | _ -> ()
      let idx = f.PickedIndex
      if idx >= 0 && prevIdx >= 0 && idx <> prevIdx then shifted <- shifted + 1
      prevIdx <- idx
      prev <- now
    // **当てる先が在ることを、門が自分で数える** ——
    // 速い弾が消えて、遅い弾がその枠へ移ったコマが在る
    shifted |> should equal 1
    jumped |> should equal 0
    // 遅い弾はまだ生きていて、追えている
    f.PickedIndex |> should be (greaterThanOrEqualTo 0)

  // --- v4.9.2 —— `wait` で止まっているだけの弾も速い道へ ----------------------

  /// **答えが変わらないこと。** 速い道は近似ではない
  [<Test>]
  member _.``wait で止まる本文でも、毎コマ 呼ぶ面と座標がビット一致する``() =
    let a = reference (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString waitXml) Frames
    let b = actual (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString waitXml) Frames
    sameShots a b |> should equal None

  /// **その道を通ったことを、門が自分で数える** ——
  /// 0 件 なら、上の点は「段 2 を 1 度 も通らずに一致した」だけ。
  ///
  /// **`ConstCount` と分けて数える。** 同じ配列の別の意味なので、
  /// 片方 だけ見ていると、もう片方 が死んでいても緑のまま
  [<Test>]
  member _.``wait で止まる弾が、速い道に居る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let f = Playfield.Create front (Bulletml.readXmlString waitXml)
    let mutable withQuiet = 0
    let mutable all = 0
    for _ in 1 .. Frames do
      f.Tick()
      all <- all + f.BulletCount
      withQuiet <- withQuiet + f.QuietCount
    all |> should be (greaterThan 500)
    withQuiet |> should be (greaterThan (all / 4))

  /// **速さが動く本文で、言ったことが嘘でないか。**
  ///
  /// `changeSpeed` は `wait` と違って action をブロックせず、
  /// **その先の `wait` まで進んでから**毎コマ 速さを動かす ——
  /// 木には `[CS(進行中); W(true, 199)]` が並ぶ。
  /// **wait だけを見て「199 コマ 静か」と読むと、位置は動かないのに
  /// 速さが変わる**（同梱 176 本 のうち 136 本 で座標が割れた形）。
  ///
  /// **「0 と言うこと」は見ない。** 撃たれた弾は `resetChild` を通るので、
  /// 木の形が素の `initial` と違う —— この本文では 0 にならないことがある。
  /// 見るのは**言った数だけ進めて姿が動かないこと**で、
  /// **0 と言っても、170 と言っても、嘘でなければ緑**。
  /// 嘘を言った瞬間 だけ赤くなる形にしてある
  [<Test>]
  member _.``速さが動く本文でも、静かと言ったなら嘘でない``() =
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">180</direction><speed>1</speed>
      <bullet>
        <action>
          <changeSpeed><speed>5</speed><term>30</term></changeSpeed>
          <wait>200</wait>
        </action>
      </bullet>
    </fire>
    <wait>500</wait>
  </action>
</bulletml>"""
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString xml
    let script = Runner.load front.Rand front.Rank bulletml
    let run = Runner.newRoot BulletType.Enemy script
    let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
    let child = fr.Spawned |> List.head
    let c1 = (Driver.step front Space.YDown SpawnOrigin.AtShooter child child.Motion).Run
    // **当てる先が在ることを、門が自分で数える** ——
    // この本文の弾は `changeSpeed` を持っていて、速さが動く
    let speedMoves =
      let mutable c = c1
      let s0 = c.Motion.Speed
      let mutable moved = false
      for _ in 1 .. 40 do
        c <- (Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion).Run
        if c.Motion.Speed <> s0 then moved <- true
      moved
    speedMoves |> should equal true
    // **言った数だけ進めて、姿が動かないことを見る。**
    //
    // 0 と言うなら「言えないことを言わなかった」で、それも正しい ——
    // **見たいのは「言ったことが嘘でない」ほう。**
    // 速さが動くあいだ 静かと言えば、ここが赤くなる
    let c2 = (Driver.step front Space.YDown SpawnOrigin.AtShooter child child.Motion).Run
    let q = c2.QuietFrames
    let m0 = c2.Motion
    let mutable c = c2
    for _ in 1 .. q do
      let step = Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion
      step.Spawned |> should be Empty
      step.Vanished |> should equal false
      c <- step.Run
      c.Motion.Speed |> should equal m0.Speed
      c.Motion.Dir |> should equal m0.Dir

  /// **言った「あと n コマ」のあいだ、エンジンは何も起こさない。**
  ///
  /// 位置・撃つ・消える だけでなく **`Motion` が動かないこと**まで見る ——
  /// 速い道は差分を据え置くので、速さや向きが動いたら答えが割れる
  [<Test>]
  member _.``静かと言ったあいだ、撃たず消えず 姿も動かない``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString waitXml
    let script = Runner.load front.Rand front.Rank bulletml
    let mutable run = Runner.newRoot BulletType.Enemy script
    let mutable found = ValueNone
    let mutable f = 0
    while found.IsNone && f < 60 do
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
      run <- fr.Run
      for c in fr.Spawned do
        if found.IsNone then found <- ValueSome c
      f <- f + 1
    match found with
    | ValueNone -> failwith "弾が出ない"
    | ValueSome c0 ->
      // 1 コマ 進めて wait を始めさせる
      let fr0 = Driver.step front Space.YDown SpawnOrigin.AtShooter c0 c0.Motion
      let mutable c = fr0.Run
      let q = c.QuietFrames
      // **当てる先が在ることを、門が自分で数える**
      q |> should be (greaterThan 5)
      let m0 = c.Motion
      let d0 = fr0.Delta
      for _ in 1 .. q do
        let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion
        fr.Spawned |> should be Empty
        fr.Vanished |> should equal false
        fr.Retired |> should equal false
        fr.Delta.X |> should equal d0.X
        fr.Delta.Y |> should equal d0.Y
        c <- fr.Run
        c.Motion.Speed |> should equal m0.Speed
        c.Motion.Dir |> should equal m0.Dir

  /// **飛ばしたぶんの `wait` を消化してあるか。**
  ///
  /// 速い道はエンジンを呼ばないので、放っておくと `wait` が減らない ——
  /// 素の道へ戻った弾が n コマ 余分 に待つ（同梱 176 本 のうち
  /// 118 本 で座標が割れた形）。**`SkipQuiet` を通した Run は、
  /// 毎コマ 進めた Run と同じ所に居る**
  [<Test>]
  member _.``SkipQuiet を通した弾は、毎コマ 進めた弾と同じ所に居る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString waitXml
    let script = Runner.load front.Rand front.Rank bulletml
    let mutable run = Runner.newRoot BulletType.Enemy script
    let mutable found = ValueNone
    let mutable f = 0
    while found.IsNone && f < 60 do
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
      run <- fr.Run
      for c in fr.Spawned do
        if found.IsNone then found <- ValueSome c
      f <- f + 1
    match found with
    | ValueNone -> failwith "弾が出ない"
    | ValueSome c0 ->
      let start = (Driver.step front Space.YDown SpawnOrigin.AtShooter c0 c0.Motion).Run
      let q = start.QuietFrames
      q |> should be (greaterThan 5)
      // 毎コマ 進めた側
      let mutable slow = start
      for _ in 1 .. q do
        slow <- (Driver.step front Space.YDown SpawnOrigin.AtShooter slow slow.Motion).Run
      // 飛ばした側
      let fast = start.SkipQuiet q
      // **次の 1 コマ が一致する。** ここがずれると、その弾だけ余分 に待つ
      let a = Driver.step front Space.YDown SpawnOrigin.AtShooter slow slow.Motion
      let b = Driver.step front Space.YDown SpawnOrigin.AtShooter fast fast.Motion
      b.Delta.X |> should equal a.Delta.X
      b.Delta.Y |> should equal a.Delta.Y
      b.Run.QuietFrames |> should equal a.Run.QuietFrames
      (List.length b.Spawned) |> should equal (List.length a.Spawned)

  // --- v4.9.3 —— 一定の割合で変わる弾も速い道へ ------------------------------

  /// **答えが変わらないこと。**
  [<Test>]
  member _.``accel だけの弾でも、毎コマ 呼ぶ面と座標がビット一致する``() =
    let a = reference (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString accelXml) Frames
    let b = actual (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString accelXml) Frames
    sameShots a b |> should equal None

  /// **その道を通ったことを、門が自分で数える。**
  /// `ConstCount` / `QuietCount` とは別の口で見る
  [<Test>]
  member _.``accel で変わる弾が、速い道に居る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let f = Playfield.Create front (Bulletml.readXmlString accelXml)
    let mutable withLin = 0
    let mutable all = 0
    for _ in 1 .. Frames do
      f.Tick()
      all <- all + f.BulletCount
      withLin <- withLin + f.LinearCount
    all |> should be (greaterThan 500)
    withLin |> should be (greaterThan (all / 4))

  /// **飛ばした後 の姿が、毎コマ 進めた姿と同じか。**
  ///
  /// `SkipLinear` は実行位置の残りを引くだけでなく
  /// **`Speed` と `Accel` を n 回 分 進める** —— 1 回 で
  /// `speed + step * n` とすると、素の道（毎コマ 足す）と丸めが合わない
  [<Test>]
  member _.``SkipLinear を通した弾は、毎コマ 進めた弾と同じ所に居る``() =
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString accelXml
    let script = Runner.load front.Rand front.Rank bulletml
    let mutable run = Runner.newRoot BulletType.Enemy script
    let mutable found = ValueNone
    let mutable k = 0
    while found.IsNone && k < 60 do
      let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
      run <- fr.Run
      for c in fr.Spawned do
        if found.IsNone then found <- ValueSome c
      k <- k + 1
    match found with
    | ValueNone -> failwith "弾が出ない"
    | ValueSome c0 ->
      let start = (Driver.step front Space.YDown SpawnOrigin.AtShooter c0 c0.Motion).Run
      match start.LinearPlan with
      | ValueNone -> failwith "線形の弾にならない"
      | ValueSome (struct (nn, _, _, _)) ->
        // **当てる先が在ることを、門が自分で数える**
        nn |> should be (greaterThan 5)
        let mutable slow = start
        for _ in 1 .. nn do
          slow <- (Driver.step front Space.YDown SpawnOrigin.AtShooter slow slow.Motion).Run
        let fast = start.SkipLinear nn
        // **姿がビット で一致する**
        fast.Motion.Speed |> should equal slow.Motion.Speed
        fast.Motion.Accel.X |> should equal slow.Motion.Accel.X
        fast.Motion.Accel.Y |> should equal slow.Motion.Accel.Y
        // **次の 1 コマ も一致する**（実行位置の残りまで合っているか）
        let x = Driver.step front Space.YDown SpawnOrigin.AtShooter slow slow.Motion
        let y = Driver.step front Space.YDown SpawnOrigin.AtShooter fast fast.Motion
        y.Delta.X |> should equal x.Delta.X
        y.Delta.Y |> should equal x.Delta.Y
        (List.length y.Spawned) |> should equal (List.length x.Spawned)

  /// **`changeDirection` が混じる形は乗らない。**
  /// `Dir` が動くと `sin dir` になり、一定の割合では変わらない
  [<Test>]
  member _.``changeDirection が進行中 なら、線形と言わない``() =
    let xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">180</direction><speed>1</speed>
      <bullet>
        <action>
          <changeDirection><direction type="absolute">90</direction><term>100</term></changeDirection>
          <wait>200</wait>
        </action>
      </bullet>
    </fire>
    <wait>500</wait>
  </action>
</bulletml>"""
    let front = DeterministicField.env (DeterministicField.stream ())
    let bulletml = Bulletml.readXmlString xml
    let script = Runner.load front.Rand front.Rank bulletml
    let run = Runner.newRoot BulletType.Enemy script
    let fr = Driver.step front Space.YDown SpawnOrigin.AtShooter run run.Motion
    let child = fr.Spawned |> List.head
    let mutable c = (Driver.step front Space.YDown SpawnOrigin.AtShooter child child.Motion).Run
    // **当てる先が在ることを、門が自分で数える** —— 向きが動く
    let d0 = c.Motion.Dir
    let mutable moved = false
    for _ in 1 .. 30 do
      c <- (Driver.step front Space.YDown SpawnOrigin.AtShooter c c.Motion).Run
      if c.Motion.Dir <> d0 then moved <- true
      // 向きが動いているあいだ、線形とは言わない
      c.LinearPlan.IsNone |> should equal true
    moved |> should equal true
