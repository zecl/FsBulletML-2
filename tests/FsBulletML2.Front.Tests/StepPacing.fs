namespace FsBulletML2.Front.Tests

open System.Runtime.InteropServices
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Playground

/// **1 コマ送りと倍速が、進める回数だけを変えているか。**
///
/// 倍速は「1 フレームに N 回 進める」であって「1 回 進めて N 倍 動かす」ではない。
/// 後者にすると弾の軌跡そのものが変わるが、**絵はそれらしく見える**ので
/// 目では気づけない。だから弾の位置を 1 つ ずつ突き合わせる。
///
/// --- なぜここに居るか
///
/// 進め方を決めるのは `Pacing`、進む先は `Playfield`。どちらも純 .NET だが
/// `Playfield` は `Front` を要る。**`Parser.Tests` は Front を参照していない**
/// ので、ここが Link で借りられる唯一の場所。
///
/// `Main.fs`（Bolero を参照する）には判断を置いていない —— あちらは
/// `Pacing.advance` を呼んで、返った回数だけ `Tick` するだけ。
///
/// --- 自機は動かさない
///
/// 倍速のときは N 回 の Tick が**同じ自機の位置**を見る。等速で N フレーム
/// 進めたものとは、自機が動いていれば別のものになる。**これは仕様**
/// （直すには自機の位置を Tick ごとに補間することになり、「補間しない」に反する）。
/// ここでは止めた自機で測って、進める回数だけを見る。
///
/// --- 0 件 を緑にしない
///
/// 弾が 1 つ も出ない弾幕だと、**両方 空で一致する。** 弾数を別の点で見る。
/// それと、比べている 2 つ が本当に違う走りであること（倍速 2 の 10 コマ が
/// 等速 10 コマ とは違うこと）も置く —— 同じなら一致は何も言っていない。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   いつも 1 回 だけ進める        進む回数の並び / 倍速 / 速さを変えると変わる
///   倍速を 1 つ 多く回す          上の 3 点 と 1 コマ送り / スロー
///   スローの残りを 1 つ ずらす    スロー / 進む回数の並び
///   0 を通す                      0 は等速に倒す
///   上限を外す                    上限で丸める
///   数えるが呼ばない              1 コマ送り / 速さを変えると変わる
///   弾の出ない弾幕に差し替える    弾が出ている / 速さを変えると変わる
///
/// **7 通り とも、素が緑のまま赤くなる。**
///
/// 最後の 1 つ で、この門の穴が 1 つ 出た。**「弾が 0 個 より多い」では
/// 見張れない** —— `Pack` は撃つ側（根）も 1 個 と数えるので、弾が 1 つ も
/// 出ない弾幕でもそこは満たされる。走らせる前より増えていることを見る形に直した。
[<TestFixture>]
type StepPacing() =

  /// **決まった並びの乱数。** `System.Random` を使う `BrowserEnv` では
  /// 2 本 の走りが別の並びを引いて、進め方と関係なく位置が割れる
  let stream () =
    let mutable i = 0
    fun () ->
      i <- i + 1
      float32 ((i * 7919) % 1000) / 1000.0f

  let env (rand: unit -> float32) =
    { new IFrontEnv with
        member _.Rand = rand
        member _.Rank = 0.5f
        member _.PlayerX = Stage.PlayerX0
        member _.PlayerY = Stage.PlayerY0
        member _.TryTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false
        member _.TrySpawnTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false }

  /// 弾が出て、消えずに残る弾幕。**出ないと両方 空で一致する**
  [<Literal>]
  let Xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>30</times>
      <action>
        <fire>
          <direction type="sequence">17</direction>
          <speed>1.6</speed>
          <bullet/>
        </fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  let field () = Playfield.Create (env (stream ())) (Bulletml.readXmlString Xml)

  /// 弾の位置。**`Pack` を通す** —— ブラウザが読むのと同じ道
  let snapshot (f: Playfield) =
    let n = f.Pack()
    let buf = Array.zeroCreate<float32> (max 1 (n * 2))
    if n > 0 then Marshal.Copy(nativeint (int64 f.PackedPtr), buf, 0, n * 2)
    n, List.ofArray buf

  /// `rate` で `frames` フレーム 走らせた結果。
  /// **`Main.fs` の `StepFrame` と同じ 1 本 を通る**（あちらも `Pacing.step`）
  let run (rate: int) (frames: int) =
    let f = field ()
    let tick = fun () -> f.Tick()
    let mutable p = Pacing.withRate rate
    for _ in 1 .. frames do p <- Pacing.step tick p
    snapshot f

  /// 1 コマ送り（`StepOnce`）を `n` 回。**進め方を通さず直に Tick する側**
  let stepped (n: int) =
    let f = field ()
    for _ in 1 .. n do f.Tick()
    snapshot f

  [<Test>]
  member _.``弾が出ている``() =
    // 下の突き合わせは、弾が 1 つ も出なければ「両方 空で一致」になる。
    //
    // **`0 個 より多い` では見張れない。** `Pack` は撃つ側（根）も 1 個 と
    // 数えるので、弾が 1 つ も出ない弾幕でもそこは満たされる（較正で踏んだ）。
    // 走らせる前より増えていることを見る
    let n0, _ = run 1 0
    let n, _ = run 1 40
    n |> should greaterThan n0

  [<Test>]
  member _.``1 コマ送り N 回 は、等速 N フレーム と同じ``() =
    run 1 40 |> should equal (stepped 40)

  [<Test>]
  member _.``倍速 N で M フレーム は、等速 N x M フレーム と同じ``() =
    run 2 20 |> should equal (run 1 40)
    run 4 10 |> should equal (run 1 40)

  [<Test>]
  member _.``スロー N で N x M フレーム は、等速 M フレーム と同じ``() =
    run -2 40 |> should equal (run 1 20)
    run -4 40 |> should equal (run 1 10)

  [<Test>]
  member _.``速さを変えると走りが変わる``() =
    // 上の一致は、速さが何にも効いていなくても緑になる
    run 2 20 |> should not' (equal (run 1 20))
    run -2 20 |> should not' (equal (run 1 20))

  [<Test>]
  member _.``進む回数の並び``() =
    let counts (rate: int) (frames: int) =
      let mutable p = Pacing.withRate rate
      [ for _ in 1 .. frames do
          let struct (n, next) = Pacing.advance p
          p <- next
          yield n ]
    counts 1 5 |> should equal [ 1; 1; 1; 1; 1 ]
    counts 4 3 |> should equal [ 4; 4; 4 ]
    counts -3 7 |> should equal [ 1; 0; 0; 1; 0; 0; 1 ]

  [<Test>]
  member _.``0 は等速に倒す``() =
    // 止めるのは Pause の仕事。ここで 0 を通すと、Play のまま止まって見える
    (Pacing.withRate 0).Rate |> should equal 1

  [<Test>]
  member _.``上限で丸める``() =
    // 1 フレームで回す回数が、そのままブラウザの止まる時間になる
    (Pacing.withRate 999).Rate |> should equal Pacing.Max
    (Pacing.withRate -999).Rate |> should equal -Pacing.Max
