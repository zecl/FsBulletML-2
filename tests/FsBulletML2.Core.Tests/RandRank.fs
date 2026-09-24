namespace FsBulletML2.Core.Tests

open NUnit.Framework

/// $rand と $rank。
[<TestFixture>]
type RandRank() =

    let bml body =
        """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
"""
        + body
        + "\n</bulletml>"

    let randXml =
        """<action label="top">
  <fire>
    <direction type="absolute">$rand*90</direction>
    <speed>1+$rand*2</speed>
    <bullet/>
  </fire>
  <wait>2</wait>
</action>"""

    [<Test>]
    member _.``rand が 0 のとき``() =
        TraceRun.withRandRank 0.0f 0.5f (bml randXml) 6 |> Golden.check "rand-0"

    [<Test>]
    member _.``rand が 1 のとき``() =
        TraceRun.withRandRank 1.0f 0.5f (bml randXml) 6 |> Golden.check "rand-1"

    [<Test>]
    member _.``rank が式に入る``() =
        bml
            """<action label="top">
  <fire>
    <direction type="absolute">0</direction>
    <speed>1+$rank*4</speed>
    <bullet/>
  </fire>
  <wait>2</wait>
</action>"""
        |> fun x -> TraceRun.withRandRank 0.5f 0.8f x 6 |> Golden.check "rank-in-expr"
