namespace FsBulletML2.Core.Tests

open NUnit.Framework

/// 中身の無い BulletML を食わせたときにどうなるか。
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
