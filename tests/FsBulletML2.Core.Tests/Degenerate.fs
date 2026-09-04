namespace FsBulletML2.Core.Tests
// **nowarn "44" は外した。** このファイルはもう旧 API を通らない。
// 外しておくと、うっかり旧経路へ戻したときに FS0044 が出る（門になる）。

open NUnit.Framework

/// 中身の無い BulletML を食わせたときにどうなるか。
///
/// **旧 API の `run` の戻り値を測っていた 2 本 は消した。**
/// あれは「`run` が X / Y を差分ではなく絶対値で返す枝に本当に入れるのか」
/// を見るもので、`BulletRunner.run` ごと廃止で消える。
/// 新 API の `Frame.Delta` は名前で差分だと言っているので、
/// 「どちらを返しているか」という問い自体が無い。
///
/// 消したのは
///
///     Tasks が空のとき、run は差分を返すか絶対値を返すか
///     Task が None のとき run が何を返すか
///
/// 控えも一緒に落とした（`run-returns-delta-or-absolute` /
/// `run-branch-task-none`）。
///
/// 残した 2 本 は台本そのものの縮退（top が無い / action が無い）で、
/// **新 API でそのまま意味がある**。
[<TestFixture>]
type Degenerate() =

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  [<Test>]
  member _.``top ラベルの action が無い BulletML``() =
    bml """<action label="notTop">
  <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
  <wait>5</wait>
</action>"""
    |> fun x -> TraceRun.std x 4 |> Golden.check "no-top-action"

  [<Test>]
  member _.``action が 1 つも無い BulletML``() =
    bml """<bullet label="b"><speed>2</speed></bullet>"""
    |> fun x -> TraceRun.std x 4 |> Golden.check "no-action-at-all"
