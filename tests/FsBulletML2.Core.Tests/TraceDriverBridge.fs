namespace FsBulletML2.Core.Tests
// 旧 API（IBulletmlObject）の Obsolete 警告を、**このファイルだけ**止める。
// ここは新旧の駆動を並べて突き合わせる側なので、旧を呼ぶのが仕事。
#nowarn "44"


open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Processable

/// **走行の途中で値が動く走行**で、新旧の駆動が同じ軌跡を出すか。
///
/// 旧 API 廃止の下ごしらえ。`Trace.runWith`（旧、`BulletMLManager` を走行の
/// 途中で差し替える）に乗っている試験を `TraceApi` へ移すには、
/// **移す前に「同じものを出す」ことを見ておく必要がある。**
///
/// 値を動かさない走行なら、橋 227 本 と Golden の大半がすでに押さえている。
/// **押さえていないのは hook が効く走行のほうで、そこがこの門。**
///
/// 2 本 に分けてある。動かすものが違うと、通る道が違う。
///
///     rank を動かす     Env.Rank だけ。aim は動かない
///     自機を動かす      AimDir と **SpawnAimDir** が動く
///
/// 後者を別に置いたのは、`TraceApi` が spawn 側の aim を走行前に 1 回 だけ
/// 計算していたため。対になる `FakeBullet.GetSpawnAimDir` は呼ばれるたびに
/// 自機の位置を読むので、**自機が動く走行でだけ割れる**。
/// 毎コマ 組み直す形に直したが、rank を動かすだけの門ではその直しに届かない。
[<TestFixture>]
[<NonParallelizable>]
type TraceDriverBridge() =

  let bml (body: string) =
    sprintf """<?xml version="1.0" ?>
<bulletml xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" type="vertical">
%s
</bulletml>""" body

  /// rank を読む。撃つ速さに出る
  let rankScript =
    bml """<action label="top">
  <repeat><times>4</times>
    <action>
      <fire><direction type="absolute">0</direction><speed>1+$rank*10</speed><bullet/></fire>
      <wait>2</wait>
    </action>
  </repeat>
  <wait>2</wait>
</action>"""

  /// 撃つ弾の向きを aim で決める。**産まれる弾の位置から見た向き**
  /// （SpawnAimDir）を読むので、自機が動くとここに出る
  let spawnAimScript =
    bml """<action label="top">
  <repeat><times>4</times>
    <action>
      <fire><bullet><direction type="aim">0</direction><speed>1</speed></bullet></fire>
      <wait>2</wait>
    </action>
  </repeat>
  <wait>2</wait>
</action>"""

  [<Test>]
  member _.``走行の途中で rank を動かしても、新旧の駆動が同じ軌跡を出す``() =
    let frames = 10
    let m = MutableManager(0.2f, 0.2f, 30.0f, 100.0f)
    BulletMLManager.Init(m)
    let oldTrace = Trace.runWith (fun i -> if i = 4 then m.Rank <- 0.7f) rankScript frames

    let mutable rank = 0.2f
    let newTrace =
      TraceApi.runWithParams
        (fun i -> if i = 4 then rank <- 0.7f)
        (fun () -> 0.2f) (fun () -> rank) (fun () -> 30.0f) (fun () -> 100.0f)
        rankScript frames

    newTrace |> should equal oldTrace

  [<Test>]
  member _.``走行の途中で自機を動かしても、新旧の駆動が同じ軌跡を出す``() =
    let frames = 10
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))
    let oldTrace =
      Trace.runWith
        (fun i -> if i = 4 then BulletMLManager.Init(FixedManager(0.5f, 0.5f, -80.0f, 20.0f)))
        spawnAimScript frames

    let mutable px = 30.0f
    let mutable py = 100.0f
    let newTrace =
      TraceApi.runWithParams
        (fun i -> if i = 4 then px <- -80.0f; py <- 20.0f)
        (fun () -> 0.5f) (fun () -> 0.5f) (fun () -> px) (fun () -> py)
        spawnAimScript frames

    newTrace |> should equal oldTrace

  /// 対照 —— **上の 2 本 は、hook が何もしなくても緑になる。**
  /// 動かした値がそもそも軌跡に出ていなければ、新旧が一致しても
  /// 何も確かめたことにならない。動かす前と後で軌跡が違うことを見ておく。
  [<Test>]
  member _.``対照: 動かした値は、どちらの台本でも軌跡に出ている``() =
    let frames = 10
    let still =
      TraceApi.runWithParams ignore
        (fun () -> 0.2f) (fun () -> 0.2f) (fun () -> 30.0f) (fun () -> 100.0f)
        rankScript frames
    let mutable rank = 0.2f
    let moved =
      TraceApi.runWithParams (fun i -> if i = 4 then rank <- 0.7f)
        (fun () -> 0.2f) (fun () -> rank) (fun () -> 30.0f) (fun () -> 100.0f)
        rankScript frames
    moved |> should not' (equal still)

    let stillAim =
      TraceApi.runWithParams ignore
        (fun () -> 0.5f) (fun () -> 0.5f) (fun () -> 30.0f) (fun () -> 100.0f)
        spawnAimScript frames
    let mutable px = 30.0f
    let mutable py = 100.0f
    let movedAim =
      TraceApi.runWithParams (fun i -> if i = 4 then px <- -80.0f; py <- 20.0f)
        (fun () -> 0.5f) (fun () -> 0.5f) (fun () -> px) (fun () -> py)
        spawnAimScript frames
    movedAim |> should not' (equal stillAim)
