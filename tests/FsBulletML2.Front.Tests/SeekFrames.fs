namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.Playground

/// **飛んだ先が、そこまで進めた先と同じか。**
///
/// 飛ぶのは「1 フレームに 1 回」を「1 フレームに入るだけ」に変えるだけで、
/// 走り方は変えていない。だから **N コマ 飛んだ面と、N 回 Tick した面は
/// 弾の位置まで同じ**でなければならない。絵はそれらしく見えるので目では
/// 気づけない —— 位置を 1 つ ずつ突き合わせる。
///
/// --- 予算の切れ方は測らない
///
/// 何 ms で切るかは走る機械で変わる。ここで見るのは
/// **切れても着く / 切れたら 1 フレームでは着かない**の 2 つ だけで、
/// 時間そのものは測らない（測ると台の速さで色が変わる）。
/// 予算の代わりに「何回 目 で超えたことにするか」を渡す。
///
/// --- 0 件 を緑にしない
///
/// 飛んだ先で弾が 1 つ も出ていなければ、突き合わせは両方 空で一致する。
/// **`Pack` は撃つ側（根）も 1 個 と数える**ので「0 個 より多い」では
/// 見張れない —— 走らせる前より増えていることを見る。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   予算を見ずに 1 回 で回し切る      切れたら 1 フレームでは着かない
///   予算を進める手前で見る            予算が尽きていても 1 コマ は進む
///   着いても idle に戻さない          着いたら飛ぶのをやめる
///   目的が手前でも建て直さない        戻るには建て直しが要る
///   目的と同じコマでも建て直す        同上
///   丸めを外す                        負は頭へ / 上限で丸める
///   もう着いていても進める            もう着いているなら進めない
///   Frame を数えない                  コマ数は Tick の回数 ほか
///
/// 最後の 1 つ で、この門の穴が 1 つ 出た。**コマ数が進まなくなると、
/// 飛ばす側が回り続けて赤ではなく固まる。** `flyTo` に上限を置いた ——
/// 赤くならない門より、止まる門のほうが見つけにくい。
[<TestFixture>]
type SeekFrames() =

  let field () = DeterministicField.create ()
  let snapshot (f: Playfield) = DeterministicField.snapshot f

  /// 予算が尽きない
  let never = fun () -> 0.0

  /// 手前で既に尽きている
  let always = fun () -> Seek.BudgetMs

  /// `k` 回 目 の問い合わせで尽きる
  let after (k: int) =
    let mutable i = 0
    fun () ->
      i <- i + 1
      if i >= k then Seek.BudgetMs else 0.0

  /// 飛ばして、着くまでのフレーム数と面を返す。
  /// **面は建て直さない** —— 前へ飛ぶぶんだけをここで見る。
  ///
  /// **上限を置く。** コマ数が進まなくなる壊れ方だと、上限が無いと
  /// 赤ではなく回り続ける（較正で踏んだ）。1 フレームに最低 1 コマ は
  /// 進むので、着くなら `target` フレーム 以内 に着く
  let flyTo (target: int) (elapsed: unit -> float) =
    let f = field ()
    let tick = fun () -> f.Tick()
    let mutable s = Seek.toFrame target
    let mutable frames = 0
    while Seek.isRunning s && frames <= target do
      let struct (_, next) = Seek.step f.Frame tick elapsed s
      s <- next
      frames <- frames + 1
    Seek.isRunning s |> should equal false
    f, frames

  let ticked (n: int) =
    let f = field ()
    for _ in 1 .. n do f.Tick()
    f

  [<Test>]
  member _.``コマ数は Tick の回数``() =
    let f = field ()
    f.Frame |> should equal 0
    f.Tick()
    f.Tick()
    f.Frame |> should equal 2

  [<Test>]
  member _.``建て直すとコマ数は 0``() =
    // Reset / Apply / 弾幕の選び直しは、どれも面を建て直す道
    let f = ticked 10
    f.Frame |> should equal 10
    (field ()).Frame |> should equal 0

  [<Test>]
  member _.``飛んだ先で弾が出ている``() =
    // これが無いと、下の突き合わせは「両方 空で一致」でも緑になる。
    // **0 個 より多いでは見張れない** —— 根も 1 個 と数える
    let n0, _ = snapshot (field ())
    let f, _ = flyTo 40 never
    let n, _ = snapshot f
    n |> should greaterThan n0

  [<Test>]
  member _.``N コマ 飛んだ面は、N 回 進めた面と同じ``() =
    let f, _ = flyTo 40 never
    snapshot f |> should equal (snapshot (ticked 40))
    f.Frame |> should equal 40

  [<Test>]
  member _.``予算で切れても、着く先は変わらない``() =
    // 切れ方を変えても行き先は同じ。**フレームの数だけが変わる**
    let f, _ = flyTo 40 (after 3)
    snapshot f |> should equal (snapshot (ticked 40))

  [<Test>]
  member _.``切れたら 1 フレームでは着かない``() =
    // 上の一致は、予算を 1 度 も見ていなくても緑になる
    let _, many = flyTo 40 (after 3)
    let _, once = flyTo 40 never
    once |> should equal 1
    many |> should greaterThan 1

  [<Test>]
  member _.``予算が尽きていても 1 コマ は進む``() =
    // 進める手前で予算を見ると、毎フレーム 0 コマ で戻ってきて着かない
    let f = field ()
    let tick = fun () -> f.Tick()
    let struct (n, next) = Seek.step f.Frame tick always (Seek.toFrame 40)
    n |> should equal 1
    Seek.isRunning next |> should equal true

  [<Test>]
  member _.``着いたら飛ぶのをやめる``() =
    let f = field ()
    let tick = fun () -> f.Tick()
    let struct (_, next) = Seek.step f.Frame tick never (Seek.toFrame 3)
    Seek.isRunning next |> should equal false

  [<Test>]
  member _.``飛んでいなければ進めない``() =
    let f = field ()
    let tick = fun () -> f.Tick()
    let struct (n, next) = Seek.step f.Frame tick never Seek.idle
    n |> should equal 0
    Seek.isRunning next |> should equal false
    f.Frame |> should equal 0

  [<Test>]
  member _.``もう着いているなら進めない``() =
    let f = ticked 10
    let tick = fun () -> f.Tick()
    let struct (n, next) = Seek.step f.Frame tick never (Seek.toFrame 10)
    n |> should equal 0
    Seek.isRunning next |> should equal false
    f.Frame |> should equal 10

  [<Test>]
  member _.``戻るには建て直しが要る``() =
    // 面は逆再生できない。建て直すのは面を持っている側の仕事なので、
    // ここは「要るかどうか」だけを言う
    Seek.needsRestart 100 (Seek.toFrame 40) |> should equal true
    Seek.needsRestart 100 (Seek.toFrame 100) |> should equal false
    Seek.needsRestart 100 (Seek.toFrame 200) |> should equal false
    Seek.needsRestart 100 Seek.idle |> should equal false

  [<Test>]
  member _.``負は頭へ倒す``() =
    // 飛んでいない印は -1 なので、負をそのまま入れると飛ばなくなる
    (Seek.toFrame -5).Target |> should equal 0
    Seek.isRunning (Seek.toFrame -5) |> should equal true

  [<Test>]
  member _.``上限で丸める``() =
    // JSInvokable なので UI に無い値も来うる
    (Seek.toFrame 999999).Target |> should equal Seek.Max
