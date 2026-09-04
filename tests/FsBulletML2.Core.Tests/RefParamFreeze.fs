namespace FsBulletML2.Core.Tests
// **nowarn "44" は外した。** このファイルはもう旧 API を通らない。
// 外しておくと、うっかり旧経路へ戻したときに FS0044 が出る（門になる）。

open NUnit.Framework

/// ref の param に入れた $rand / $rank が、走るたびに読み直されるか。
///
/// 直す前は、`BulletRunner.convertBulletmlTask` が
/// `IntermediateParser.existRandomParam` を見て、$rand を含む ref がひとつでもあれば
/// `BulletmlTask.Original` に生の XML を持たせ、`Init()` が毎周 作り直していた。
/// 探していたのは `$rand` の 5 文字だけで、`$rank` は見ていなかった。
///
/// そのあと 11 で直した。param を文字のまま子へ渡すようにしたので、
/// $rand を助けるためのこの迂回路は要らなくなり、`existRandomParam` は消してある。
/// この doc は「何が在ってどう壊れていたか」の記録。控えは直したあとの姿。
///
/// **旧 API だけの 2 本 は消した。** どちらも `BulletmlTask.Original` を
/// 毎周 作り直していた不具合の見張りで、**新 API に Original が無いので
/// その不具合の形が作れない**（param は文字のまま子へ渡り、`getValue` が
/// 読む位置まで生き残る）。
///
///     param の中身と Original の関係
///     Original を書き換えて Init すると、次に走るのは書き換えた方
///
/// 控えも一緒に落とした（`freeze-original-flag`）。
/// 「param が凍らない」ことそのものは、残した 8 本 が軌跡で見ている。
[<TestFixture>]
type RefParamFreeze() =

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  /// 撃った弾の速さだけを抜く。位置は本題ではないので落とす
  let firedSpeeds (trace: string) =
    trace.Split('\n')
    |> Array.filter (fun l -> l.Contains "  +b")
    |> Array.map (fun l -> l.Trim())
    |> String.concat "\n"

  /// top がひと回りするたびに 1 発だけ撃つ。速さは param 経由
  let viaParam expr =
    sprintf """<action label="top">
  <actionRef label="shoot"><param>%s</param></actionRef>
</action>

<action label="shoot">
  <fire><direction type="absolute">0</direction><speed>$1</speed><bullet/></fire>
  <wait>2</wait>
</action>""" expr

  /// 同じ形で param を通さないもの。これが対照
  let bare expr =
    sprintf """<action label="top">
  <fire><direction type="absolute">0</direction><speed>%s</speed><bullet/></fire>
  <wait>2</wait>
</action>""" expr

  /// param を通さず、参照した先に直接 $rank を書く。
  /// actionRef を挟んだこと自体が凍らせるのか、param の置き換えが凍らせるのかを割る
  let refNoParam expr =
    sprintf """<action label="top">
  <actionRef label="shoot"/>
</action>

<action label="shoot">
  <fire><direction type="absolute">0</direction><speed>%s</speed><bullet/></fire>
  <wait>2</wait>
</action>""" expr

  /// ひと回りの中で 3 発。param 経由なので、展開が 1 回なら 3 発とも同じ値になる
  let repeatViaParam =
    """<action label="top">
  <repeat><times>3</times>
    <action><actionRef label="shoot"><param>$rand</param></actionRef></action>
  </repeat>
  <wait>6</wait>
</action>

<action label="shoot">
  <fire><direction type="absolute">0</direction><speed>$1</speed><bullet/></fire>
  <wait>1</wait>
</action>"""

  /// フレーム 4 で値を切り替える。前後で 2 発ずつ撃つ長さにしてある。
  ///
  /// 旧は MutableManager（グローバル）を走行の途中で差し替えていた。
  /// **新 API はフロントが毎コマ Env を渡すので、ふつうの mutable でよい**
  /// —— 差し替えるグローバルが要らない。移植が正しいことは、
  /// 下の Golden が 1 バイト も動かないことで押さえている。
  let runSwitching (xml: string) (rand: unit -> float32) (rank: unit -> float32)
                   (switch: unit -> unit) =
    TraceApi.runWithParams (fun i -> if i = 4 then switch ())
      rand rank (fun () -> 30.0f) (fun () -> 100.0f) xml 9
    |> firedSpeeds

  [<Test>]
  member _.``対照 1: param を通さない $rank は、走行中に変えると追随する``() =
    let mutable rank = 0.2f
    runSwitching (bml (bare "1+$rank*10")) (fun () -> 0.2f) (fun () -> rank) (fun () -> rank <- 0.7f)
    |> fun s -> s + "\n\n0.2 のとき 3.0、0.7 のとき 8.0。切り替えはフレーム 4"
    |> Golden.check "freeze-direct-rank"

  [<Test>]
  member _.``対照 2: param を通さない $rand も追随する``() =
    let mutable rand = 0.2f
    runSwitching (bml (bare "1+$rand*10")) (fun () -> rand) (fun () -> 0.2f) (fun () -> rand <- 0.7f)
    |> fun s -> s + "\n\n0.2 のとき 3.0、0.7 のとき 8.0。切り替えはフレーム 4"
    |> Golden.check "freeze-direct-rand"

  [<Test>]
  member _.``param に置いた $rank は追随するか``() =
    let mutable rank = 0.2f
    runSwitching (bml (viaParam "$rank")) (fun () -> 0.2f) (fun () -> rank) (fun () -> rank <- 0.7f)
    |> fun s -> s + "\n\n速さ = $1 = $rank。0.2 から 0.7 に変わるなら追随、0.2 のままなら焼き付け"
    |> Golden.check "freeze-param-rank"

  [<Test>]
  member _.``param に置いた $rand は追随するか``() =
    let mutable rand = 0.2f
    runSwitching (bml (viaParam "$rand")) (fun () -> rand) (fun () -> 0.2f) (fun () -> rand <- 0.7f)
    |> fun s -> s + "\n\n速さ = $1 = $rand。existRandomParam が守っているのはこちらだけ"
    |> Golden.check "freeze-param-rand"

  [<Test>]
  member _.``actionRef の先に直接書いた $rank は追随するか``() =
    let mutable rank = 0.2f
    runSwitching (bml (refNoParam "1+$rank*10")) (fun () -> 0.2f) (fun () -> rank) (fun () -> rank <- 0.7f)
    |> fun s -> s + "\n\nparam を通さず参照だけ挟んだ形。追随するなら、凍らせているのは param の置き換え"
    |> Golden.check "freeze-ref-noparam-rank"

  [<Test>]
  member _.``param に式ごと入れた $rank は追随するか``() =
    let mutable rank = 0.2f
    runSwitching (bml (viaParam "1+$rank*10")) (fun () -> 0.2f) (fun () -> rank) (fun () -> rank <- 0.7f)
    |> fun s -> s + "\n\n$rank 単体ではなく式ごと param に入れた形。3.0 のままなら焼き付け"
    |> Golden.check "freeze-param-rank-expr"

  /// existRandomParam が守るのは「top がひと回りしたとき作り直す」ところまで。
  /// ひと回りの中では展開は 1 回なので、$rand は 1 回しか転がらないはず
  [<Test>]
  member _.``ひと回りの中で 3 発撃つと、param の $rand は何回転がるか``() =
    let mutable rand = 0.2f
    let hook i =
      if i = 1 then rand <- 0.5f
      elif i = 2 then rand <- 0.9f
    TraceApi.runWithParams hook (fun () -> rand) (fun () -> 0.2f)
      (fun () -> 30.0f) (fun () -> 100.0f) (bml repeatViaParam) 8
    |> firedSpeeds
    |> fun s -> s + "\n\n毎フレーム rand を動かしている（f1 で 0.5、f2 で 0.9）。\n3 発とも同じなら、ひと回りの中では 1 回しか転がっていない"
    |> Golden.check "freeze-rand-within-loop"

  /// 11 を直す代償を測る。
  ///
  /// mapEval を外して param を文字のまま渡すと、$rand / $rank は getValue まで
  /// 生き残る。そのかわり **$1 を何度も使う action では、使うたびに転がる**。
  /// 揃った扇がばらけるかどうかが、直すか決める材料になる。
  ///
  /// ここは 1 つの action の中で同じ $1 を 3 回 使い、3 発の向きが揃うかを見る
  [<Test>]
  member _.``同じ param を 1 つの action で何度も使う``() =
    let xml =
      bml """<action label="top">
  <actionRef label="fan"><param>$rand*100</param></actionRef>
  <wait>10</wait>
</action>
<action label="fan">
  <fire><direction type="absolute">$1</direction><speed>1</speed><bullet/></fire>
  <fire><direction type="absolute">$1</direction><speed>2</speed><bullet/></fire>
  <fire><direction type="absolute">$1</direction><speed>3</speed><bullet/></fire>
</action>"""
    // 毎フレーム rand を動かす。param が数へ潰されていれば 3 発とも同じ向き、
    // 文字のまま渡っていれば 3 発ともばらける。
    // **木を組む段は 0.5**（旧はループの前に FixedManager(0.5f, ...) が
    // 入っていた）。hook はコマの頭で呼ばれるので、f0 からは 0.1 x n
    let mutable n = 0
    let mutable rand = 0.5f
    let hook _ =
      n <- n + 1
      rand <- 0.1f * float32 n
    TraceApi.runWithParams hook (fun () -> rand) (fun () -> 0.5f)
      (fun () -> 30.0f) (fun () -> 100.0f) xml 4
    |> fun t ->
        t.Split('\n')
        |> Array.filter (fun l -> l.Contains "  +b")
        |> Array.map (fun l -> l.Trim())
        |> String.concat "\n"
    |> fun s -> s + "\n\n同じ $1 を 3 回 使っている。向きが 3 発とも同じなら「揃った扇」、\nばらけていれば「使うたびに転がる」"
    |> Golden.check "freeze-param-reused"
