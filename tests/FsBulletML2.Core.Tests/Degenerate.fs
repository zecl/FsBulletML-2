namespace FsBulletML2.Core.Tests

open NUnit.Framework

/// 中身の無い BulletML を食わせたときにどうなるか。
///
/// 旧 API の `run` の戻り値を測っていた 2 本 は消した。
/// あれは「`run` が X / Y を差分ではなく絶対値で返す枝に本当に入れるのか」
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
