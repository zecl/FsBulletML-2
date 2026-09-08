namespace FsBulletML2.Front.Tests

open System.Runtime.InteropServices
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Playground

/// 決まった走りをする面。**進め方を測る側と、飛び方を測る側が同じ 1 本 を使う。**
///
/// 写しを 2 つ 置くと、片方 だけ直したときに「どちらが正か」が言えなくなる。
///
/// --- 乱数を決め打つ
///
/// `System.Random` を使う `BrowserEnv` では、2 本 の走りが別の並びを引いて、
/// 測っているもの（進める回数 / 飛んだ先）と関係なく位置が割れる。
///
/// --- 自機は動かさない
///
/// 倍速のときは N 回 の Tick が同じ自機の位置を見る。等速で N フレーム
/// 進めたものとは、自機が動いていれば別のものになる。ここでは止めて測る。
module internal DeterministicField =

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

  /// 弾が出て、消えずに残る弾幕。**出ないと 2 つ の走りが両方 空で一致する**
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

  let create () = Playfield.Create (env (stream ())) (Bulletml.readXmlString Xml)

  /// 弾の位置。**`Pack` を通す** —— ブラウザが読むのと同じ道
  let snapshot (f: Playfield) =
    let n = f.Pack()
    let buf = Array.zeroCreate<float32> (max 1 (n * 2))
    if n > 0 then Marshal.Copy(nativeint (int64 f.PackedPtr), buf, 0, n * 2)
    n, List.ofArray buf
