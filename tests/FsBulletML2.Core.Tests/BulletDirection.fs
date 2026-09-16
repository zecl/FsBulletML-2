namespace FsBulletML2.Core.Tests

open System.Text.RegularExpressions
open NUnit.Framework

/// bullet 要素の中に書いた direction が、弾の向きに入るか。
///
/// ここは「正しい姿」ではなく「いまの姿」を記録している。
/// 測った結果はこう。
[<TestFixture>]
type BulletDirection() =

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  let firstBullet (trace: string) =
    let m = Regex.Match(trace, @"\+b1 d=([-\d.]+) s=([-\d.]+)")
    if m.Success then sprintf "d=%s s=%s" m.Groups.[1].Value m.Groups.[2].Value else "撃っていない"

  let inlineLiteral = bml """<action label="top">
  <fire><bullet><direction type="absolute">30</direction><speed>2</speed></bullet></fire>
  <wait>10</wait>
</action>"""

  let refLiteral = bml """<action label="top">
  <fire><bulletRef label="b"/></fire>
  <wait>10</wait>
</action>
<bullet label="b"><direction type="absolute">30</direction><speed>2</speed></bullet>"""

  let refParam = bml """<action label="top">
  <fire><bulletRef label="b"><param>30</param><param>2</param></bulletRef></fire>
  <wait>10</wait>
</action>
<bullet label="b"><direction type="absolute">$1</direction><speed>$2</speed></bullet>"""

  let fireDir = bml """<action label="top">
  <fire><direction type="absolute">30</direction><bullet><speed>2</speed></bullet></fire>
  <wait>10</wait>
</action>"""

  [<Test>]
  member _.``direction をどこに書いたかで、弾の向きがどうなるか``() =
    // 30 度 = 0.524。自機 (30,100) を狙う aim = 2.850
    [ sprintf "inline literal (bullet 内)  %s" (TraceRun.std inlineLiteral 4 |> firstBullet)
      sprintf "ref literal    (bullet 内)  %s" (TraceRun.std refLiteral 4 |> firstBullet)
      sprintf "ref param      (bullet 内)  %s" (TraceRun.std refParam 4 |> firstBullet)
      sprintf "fire 側        (fire 内)    %s" (TraceRun.std fireDir 4 |> firstBullet)
      "30 度 = 0.524 / aim = 2.850" ]
    |> String.concat "\n"
    |> Golden.check "bullet-direction-where"
