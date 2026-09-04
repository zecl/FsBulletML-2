namespace FsBulletML2.Core.Tests

open NUnit.Framework

/// 骨が通るかを見る 1 本。
/// 公開 API で走らせるのでグローバル可変に触らない。並列に走る。
[<TestFixture>]
type Smoke() =

  [<Test>]
  member _.``fire と wait だけの top action が、控えどおりの軌跡になる``() =
    let xml = """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
<action label="top">
  <fire>
    <direction type="absolute">0</direction>
    <speed>2</speed>
    <bullet/>
  </fire>
  <wait>3</wait>
</action>
</bulletml>"""
    TraceRun.atOrigin xml 6 |> Golden.check "smoke-fire-wait"
