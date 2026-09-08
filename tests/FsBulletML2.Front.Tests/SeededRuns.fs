namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Playground

/// **同じ種なら同じ走り。** 種は Share URL に乗るので、
/// リンクが指しているのは「その走り」——
/// 並びが 1 か所 でも動けば、リンクは別の絵を開く。
///
/// --- なぜ `System.Random` を使わないか
///
/// 同じ種で同じ並びを返すことは測った（下の点でも見ている）。
/// それでも使わないのは、**契約が「同じ実装なら」だから** ——
/// runtime が上がった時点で、配ったリンクが別の絵になる。
///
/// --- 版の頭で測ったこと
///
///     同じ種で 2 回 走らせて 1 コマ も違わない   176 / 176 本
///     種を変えると走りが変わる                 86 本（残りは $rand を使わない）
///     $rank を 0 と 1 に振ると走りが変わる       171 / 176 本
///
/// ここでは同じことを 1 本 の弾幕で、**面を通して**当てる。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   `Restart` を何もしない            建て直しても同じ走り
///   種を捨てて固定の並びにする        種を変えると走りが変わる
///   `clamp` の 0 を通す               0 は 1 に倒す
///   rank を使わない                   難度で走りが変わる
[<TestFixture>]
type SeededRuns() =

  let env (seed: int) (rank: float32) =
    let r = SeededRandom(seed)
    let rand = fun () -> r.Next()
    r,
    { new IFrontEnv with
        member _.Rand = rand
        member _.Rank = rank
        member _.PlayerX = Stage.portrait.PlayerX
        member _.PlayerY = Stage.portrait.PlayerY
        member _.TryTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false
        member _.TrySpawnTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false }

  /// `$rand` と `$rank` の両方 を使う弾幕。**片方 だけだと、
  /// もう片方 の点が「変わらなくて緑」になる**
  [<Literal>]
  let Xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>20 + 30 * $rank</times>
      <action>
        <fire>
          <direction type="absolute">360 * $rand</direction>
          <speed>1 + 2 * $rank</speed>
          <bullet/>
        </fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  let bulletml = Bulletml.readXmlString Xml

  /// `frames` コマ 回して、弾数の並びを返す
  let run (seed: int) (rank: float32) (frames: int) =
    let _, e = env seed rank
    let f = Playfield.Create e bulletml
    [ for _ in 1 .. frames do
        f.Tick()
        yield f.Count ]

  [<Test>]
  member _.``同じ種なら同じ走り``() =
    run 12345 0.5f 200 |> should equal (run 12345 0.5f 200)

  [<Test>]
  member _.``種を変えると走りが変わる``() =
    // 上の一致は、**種が何にも効いていなくても緑**になる
    run 1 0.5f 200 |> should not' (equal (run 999 0.5f 200))

  [<Test>]
  member _.``難度で走りが変わる``() =
    // 版の頭で測ったら 176 本 中 171 本 で変わった。**0.5 に固定していた軸**
    run 1 0.0f 200 |> should not' (equal (run 1 1.0f 200))

  [<Test>]
  member _.``建て直しても同じ走り``() =
    // **並びを頭へ戻さないと、同じ種でも建て直したあとが別の走りになる。**
    // Reset も Apply も飛ぶのも、すべて建て直しを通る
    let r, e = env 4242 0.5f
    let first =
      let f = Playfield.Create e bulletml
      [ for _ in 1 .. 120 do
          f.Tick()
          yield f.Count ]
    r.Restart()
    let second =
      let f = Playfield.Create e bulletml
      [ for _ in 1 .. 120 do
          f.Tick()
          yield f.Count ]
    second |> should equal first

  [<Test>]
  member _.``戻さないと別の走りになる``() =
    // 上の点は、**`Restart` が何もしなくても「たまたま同じ」なら緑**になる。
    // 戻さない側を並べて、本当に違うことを見る
    let _, e = env 4242 0.5f
    let once () =
      let f = Playfield.Create e bulletml
      [ for _ in 1 .. 120 do
          f.Tick()
          yield f.Count ]
    let first = once ()
    let second = once ()
    second |> should not' (equal first)

  [<Test>]
  member _.``弾が出ている``() =
    // 上の突き合わせは、弾が 1 つ も出なければ「両方 空で一致」になる
    let counts = run 1 0.5f 120
    List.max counts |> should greaterThan 1

  // --- 並びそのもの ---------------------------------------------------------

  [<Test>]
  member _.``同じ種の 2 本 は同じ並び``() =
    let a = SeededRandom(7)
    let b = SeededRandom(7)
    [ for _ in 1 .. 50 -> a.Next() ] |> should equal [ for _ in 1 .. 50 -> b.Next() ]

  [<Test>]
  member _.``0 から 1 の中に入る``() =
    let r = SeededRandom(99)
    for _ in 1 .. 1000 do
      let v = r.Next()
      v |> should be (greaterThanOrEqualTo 0.0f)
      v |> should be (lessThan 1.0f)

  [<Test>]
  member _.``同じ値ばかり返さない``() =
    // 上の 2 点 は、**いつも 0 を返しても緑**になる
    let r = SeededRandom(99)
    [ for _ in 1 .. 100 -> r.Next() ] |> List.distinct |> List.length |> should greaterThan 50

  [<Test>]
  member _.``0 は 1 に倒す``() =
    // xorshift は 0 から動かない。**倒さないと並びが全部 0 になる**
    SeededRandom.clamp 0 |> should equal 1
    SeededRandom.clamp -5 |> should equal 1
    [ for _ in 1 .. 10 -> SeededRandom(0).Next() ] |> List.distinct |> should not' (equal [ 0.0f ])

  [<Test>]
  member _.``上限で丸める``() =
    SeededRandom.clamp 9999999 |> should equal SeededRandom.Max
    SeededRandom.clamp SeededRandom.Max |> should equal SeededRandom.Max

  [<Test>]
  member _.``時計から作る種は範囲の中``() =
    for _ in 1 .. 20 do
      let n = SeededRandom.fromClock ()
      n |> should be (greaterThanOrEqualTo 1)
      n |> should be (lessThanOrEqualTo SeededRandom.Max)
