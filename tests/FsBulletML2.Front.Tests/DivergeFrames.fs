namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Playground

/// **2 つ の面が分かれたコマを出す**（v3.8）。
///
/// --- 数だけ見ると遅れる
///
/// 版の頭で同梱 176 本 を数えた —— 種を変えたとき、**座標が分かれるのは
/// 中央 6 コマ 目 なのに、弾の数が食い違うのは中央 134 コマ 目**（22.3 倍）。
/// だから `Playfield.Differ` は座標まで見る。
///
/// **ここで当てるのもそこ。** 使う 2 本 は「弾の出方は同じで速さだけ違う」ので、
/// **どのコマでも弾の数が等しい** —— 数だけ見る実装では 1 度 も分かれない。
///
/// --- コマ数は焼き込まない
///
/// 「n コマ 目 で分かれる」と書くと、弾幕の書き方を 1 文字 直しただけで
/// 赤くなる。代わりに**その数の意味**を当てる ——
/// **止まったコマの 1 つ 手前 では分かれていない。**
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   Differ から座標の比較を外す      数が同じでも分かれる
///   Differ を数だけにする            同上
///   差を進める手前で見る              1 コマ は進む
///   予算を見ずに回し切る              切れたら 1 フレームでは終わらない
///   found でも idle に戻さない        見つけたらそこで止まる
///   start の丸めを外す                0 以下 は走らない / 上限で丸める
[<TestFixture>]
type DivergeFrames() =

  /// 速さだけ違う 1 本。**弾の出方は同じ**なので、どのコマでも弾数が等しい
  [<Literal>]
  let Fast = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>30</times>
      <action>
        <fire>
          <direction type="sequence">17</direction>
          <speed>3.0</speed>
          <bullet/>
        </fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  let slow () = DeterministicField.create ()

  let fast () =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString Fast)

  /// 予算が尽きない
  let never = fun () -> 0.0

  /// `k` 回 目 の問い合わせで尽きる
  let after (k: int) =
    let mutable i = 0
    fun () ->
      i <- i + 1
      if i >= k then Diverge.BudgetMs else 0.0

  /// 2 つ の面を同じコマで進めて、分かれるまで送る。
  /// 戻りは（分かれたか, 進めたコマ数, フレーム数）
  let run (a: Playfield) (b: Playfield) (look: int) (budget: unit -> float) =
    let tick = fun () ->
      a.Tick()
      b.Tick()
    let differs = fun () -> Playfield.Differ a b
    let mutable d = Diverge.start look
    let mutable moved = 0
    let mutable found = false
    let mutable frames = 0
    // **上限を置く。** 進まなくなったら赤ではなく止まらなくなる
    while Diverge.isRunning d && frames < 10000 do
      let struct (n, hit, next) = Diverge.step tick budget differs d
      moved <- moved + n
      if hit then found <- true
      d <- next
      frames <- frames + 1
    (found, moved, frames)

  [<Test>]
  member _.``速さだけ違う 2 面 は、弾の数が同じまま座標で分かれる``() =
    let a = slow ()
    let b = fast ()
    let mutable split = -1
    let mutable i = 0
    // **数では 1 度 も分かれない**ことを、同じ走行の中で数える
    let mutable countEverDiffered = false
    while split < 0 && i < 200 do
      a.Tick()
      b.Tick()
      i <- i + 1
      if a.Count <> b.Count then countEverDiffered <- true
      if Playfield.Differ a b then split <- i
    split |> should be (greaterThan 0)
    countEverDiffered |> should equal false

  [<Test>]
  member _.``分かれたら、その 1 つ 手前 では分かれていない``() =
    let (found, moved, _) = run (slow ()) (fast ()) 200 never
    found |> should equal true
    moved |> should be (greaterThan 0)
    // 手前 まで進め直して、分かれていないことを見る
    let a = slow ()
    let b = fast ()
    for _ in 1 .. moved - 1 do
      a.Tick()
      b.Tick()
    Playfield.Differ a b |> should equal false
    a.Tick()
    b.Tick()
    Playfield.Differ a b |> should equal true

  [<Test>]
  member _.``同じ弾幕の 2 面 は分かれない。見るコマ数を使い切って止まる``() =
    let (found, moved, _) = run (slow ()) (slow ()) 120 never
    found |> should equal false
    moved |> should equal 120

  [<Test>]
  member _.``予算で切れても、続きから同じコマで見つける``() =
    let (foundA, movedA, framesA) = run (slow ()) (fast ()) 200 never
    let (foundB, movedB, framesB) = run (slow ()) (fast ()) 200 (after 1)
    foundA |> should equal true
    foundB |> should equal true
    movedB |> should equal movedA
    // **切れたら 1 フレームでは終わらない**（切れない側は 1 フレーム）
    framesA |> should equal 1
    framesB |> should be (greaterThan 1)

  [<Test>]
  member _.``差は 1 コマ 進めたあとに見る``() =
    // 押した時点で既に分かれている 2 面。**0 コマ で返さない** ——
    // 返すと「どのコマで分かれたか」が言えない
    let a = slow ()
    let b = fast ()
    for _ in 1 .. 60 do
      a.Tick()
      b.Tick()
    Playfield.Differ a b |> should equal true
    let tick = fun () ->
      a.Tick()
      b.Tick()
    let struct (n, found, next) = Diverge.step tick never (fun () -> Playfield.Differ a b) (Diverge.start 100)
    found |> should equal true
    n |> should equal 1
    Diverge.isRunning next |> should equal false

  [<Test>]
  member _.``0 以下 では走らない。上限で丸める``() =
    Diverge.isRunning (Diverge.start 0) |> should equal false
    Diverge.isRunning (Diverge.start -5) |> should equal false
    (Diverge.start (Diverge.Max + 1000)).Left |> should equal Diverge.Max
    (Diverge.start 7).Left |> should equal 7