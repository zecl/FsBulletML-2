namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.Playground

/// **面から JS へ渡す並びは 1 点 3 つ**（v4.7）—— x / y / 撃った腕の添字。
///
/// 3 つ 目 を足したのは「字から弾へ」のため。**系譜は 1 度 も辿らない** ——
/// `Live.From` が撃った腕の添字を既に持っている（v3.2）。
///
/// --- 足したのに速くなった
///
///     前  0.01344 ms
///     後  0.0096〜0.0119 ms（1,481 点。別々 の走行）
///
/// 書く数は 2 つ から 3 つ に増えたが、`live.[i]` を 2 度 引くのを
/// **1 度 に畳んだ**ぶんのほうが大きい（`ResizeArray` の添字は境界を見る）。
///
/// --- 較正（1 か所 ずつ当てて、赤くなった点を数えた）
///
///   `float32 it.From` を `0.0f` に      赤 3
///   `need = n * 3` を `n * 2` に        赤 6
///   `it.Y` を `it.X` に                 赤 1
///
/// **下の 2 つ は、はじめ 0 点 だった** —— 弾が 20 発 では最初の確保（256）で
/// 足りてしまい、x と y が両方 面の中なら「同じ値」でも通る。
/// **弾を 100 発 以上 出す本文に替えて、x と y が割れることを見る点を足した。**
///
/// --- 突き合わせる相手は `AliveByFire`
///
/// **同じことを 2 通り の道で数える。** 並びの 3 つ 目 を数えたものと、
/// `AliveByFire`（腕ごとに束ねる別の口）が同じ答えを出す ——
/// 片方 だけ壊れたら割れる。
[<TestFixture>]
type PackTriple() =

  /// **撃つ腕が 2 つ 在る本文。** 添字が別々 に入ることを見る。
  ///
  /// **弾を 100 発 以上 出す。** 少ないと確保が最初の 256 で足りてしまい、
  /// 「3 つ 分 を確保する」を 2 つ 分 に戻しても赤くならない（較正で 0 点 だった）
  static let two = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>120</times>
      <action>
        <fire><direction type="absolute">150</direction><speed>1</speed><bullet/></fire>
        <fire><direction type="absolute">210</direction><speed>1</speed><bullet/></fire>
        <wait>2</wait>
      </action>
    </repeat>
    <wait>200</wait>
  </action>
</bulletml>"""

  static let field (xml: string) =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) (FsBulletML2.Bulletml.readXmlString xml)

  static let ran () =
    let f = field two
    for _ in 1 .. 120 do f.Tick()
    f

  [<Test>]
  member _.``点 1 つ につき 3 つ ぶん の場所が在る``() =
    let f = ran ()
    let n = f.Pack()
    // **最初の確保（256）を超えるところまで出す。** 超えないと、
    // 3 つ 分 を 2 つ 分 に戻しても足りてしまう
    n |> should be (greaterThan 100)
    // **足りなければここで落ちる**（歩幅を戻すと、末尾の点がはみ出す）
    (f.Packed).Length |> should be (greaterThanOrEqualTo (n * 3))

  [<Test>]
  member _.``x と y は面の中に入っている``() =
    let f = ran ()
    let n = f.Pack()
    let a = f.Packed
    for i in 0 .. n - 1 do
      a.[i * 3] |> should be (greaterThanOrEqualTo 0.0f)
      a.[i * 3] |> should be (lessThanOrEqualTo f.Field.Width)
      a.[i * 3 + 1] |> should be (greaterThanOrEqualTo 0.0f)
      a.[i * 3 + 1] |> should be (lessThanOrEqualTo f.Field.Height)

  /// **x と y が同じ値ばかりではない。** 片方 をもう片方 で埋める壊し方は
  /// 「面の中に入っている」だけでは捕まらない（較正で 0 点 だった）——
  /// 2 つ の腕は 150 度 と 210 度 へ撃つので、x と y は必ず割れる
  [<Test>]
  member _.``x と y は別の値``() =
    let f = ran ()
    let n = f.Pack()
    let a = f.Packed
    let same = [ for i in 0 .. n - 1 do if a.[i * 3] = a.[i * 3 + 1] then yield i ]
    List.length same |> should be (lessThan (n / 2))

  [<Test>]
  member _.``3 つ 目 を数えると AliveByFire と同じ``() =
    let f = ran ()
    let n = f.Pack()
    let a = f.Packed
    let byPack =
      [ for i in 0 .. n - 1 -> int a.[i * 3 + 2] ]
      |> List.filter (fun v -> v >= 0)
      |> List.countBy id
      |> List.sort
    let byArm =
      f.AliveByFire()
      |> Array.map (fun (struct (idx, c)) -> idx, c)
      |> Array.toList
      |> List.sort
    // **当てる先が在ることを、門が自分で数える**
    byArm |> should not' (be Empty)
    byPack |> should equal byArm

  [<Test>]
  member _.``撃つ腕が 2 つ 在れば、添字も 2 通り 出る``() =
    // **1 通り しか出ないと、上の点は「全部 同じ添字」でも緑になる**
    let f = ran ()
    let n = f.Pack()
    let a = f.Packed
    [ for i in 0 .. n - 1 -> int a.[i * 3 + 2] ]
    |> List.filter (fun v -> v >= 0)
    |> List.distinct
    |> List.length
    |> should be (greaterThanOrEqualTo 2)

  [<Test>]
  member _.``撃たれていない弾は -1``() =
    // 根の敵。**JS 側はそこを外す**（塗るのは添字が 0 以上 のときだけ）
    let f = ran ()
    let n = f.Pack()
    let a = f.Packed
    let negatives = [ for i in 0 .. n - 1 -> int a.[i * 3 + 2] ] |> List.filter (fun v -> v < 0)
    // **`AliveUnattributed` は根を含む**（あちらの但し書き）——
    // 1 を足すと 2 重 に数える
    negatives |> List.length |> should equal f.AliveUnattributed
    // 当てる先が在ることを数える（根が居るので必ず 1 以上）
    negatives |> should not' (be Empty)
