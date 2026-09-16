namespace FsBulletML2.Core.Tests

open System.Text.RegularExpressions
open NUnit.Framework

/// wait が何フレーム効くかを、発射の間隔として固める。
///
/// ここは「正しい姿」ではなく「いまの姿」を記録している。
/// 実装には次の 2 つの癖がある（測ったもの）。
[<TestFixture>]
type Timing() =

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  /// 何フレーム目で弾が産まれたかだけを取り出す
  let fireFrames (trace: string) =
    let mutable frame = -1
    [ for line in trace.Split('\n') do
        let m = Regex.Match(line, @"^f(\d+)$")
        if m.Success then frame <- int m.Groups.[1].Value
        elif line.Contains "  +b" then yield frame ]

  let inRepeat wait =
    bml (sprintf """<action label="top">
  <repeat><times>4</times>
    <action>
      <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
      <wait>%d</wait>
    </action>
  </repeat>
  <wait>30</wait>
</action>""" wait)

  let noRepeat wait =
    bml (sprintf """<action label="top">
  <fire><direction type="absolute">0</direction><speed>2</speed><bullet/></fire>
  <wait>%d</wait>
</action>""" wait)

  [<Test>]
  member _.``wait を 1..4 で振った発射フレームと間隔``() =
    let gaps (xs: int list) = xs |> List.pairwise |> List.map (fun (p, q) -> q - p)
    let lines =
      [ for w in 1 .. 4 do
          let a = TraceRun.atOrigin (inRepeat w) 24 |> fireFrames
          let b = TraceRun.atOrigin (noRepeat w) 24 |> fireFrames
          yield sprintf "wait=%d repeat=on  fires=%A gaps=%A" w a (gaps a)
          yield sprintf "wait=%d repeat=off fires=%A gaps=%A" w b (gaps b) ]
    String.concat "\n" lines |> Golden.check "wait-intervals"
