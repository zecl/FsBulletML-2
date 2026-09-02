namespace FsBulletML2.Core.Tests
// 旧 API（IBulletmlObject）の Obsolete 警告を、**このファイルだけ**止める。
// ここは旧経路を意図して走らせる側だから（新旧を突き合わせる橋の材料）。
//
// プロジェクト単位（NoWarn）で止めない。止めると、**新しく書いた試験が
// うっかり旧 API を使っても警告が出なくなる**。
// 効きがファイル単位であることは較正済み —— nowarn を置いていない
// ファイルで旧 API に触ると FS0044 が出る。
#nowarn "44"


open NUnit.Framework
open FsBulletML2
open FsBulletML2.Processable

/// ref の param に入れた $rand / $rank が、走るたびに読み直されるか。
///
/// 直す前は、`BulletRunner.convertBulletmlTask` が
/// `IntermediateParser.existRandomParam` を見て、$rand を含む ref がひとつでもあれば
/// `BulletmlTask.Original` に生の XML を持たせ、`Init()` が毎周 作り直していた。
/// 探していたのは `$rand` の 5 文字だけで、`$rank` は見ていなかった。
///
/// そのあと 11 で直した。param を文字のまま子へ渡すようにしたので、
/// $rand を助けるためのこの迂回路は要らなくなり、`existRandomParam` は消してある。
/// `Original` は常に None で、`Init()` は既にある木の可変フラグを戻すだけ。
/// この doc は「何が在ってどう壊れていたか」の記録。控えは直したあとの姿。
[<TestFixture>]
[<NonParallelizable>]
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

  /// フレーム 4 で値を切り替える。前後で 2 発ずつ撃つ長さにしてある
  let runSwitching (xml: string) (switch: MutableManager -> unit) =
    let m = MutableManager(0.2f, 0.2f, 30.0f, 100.0f)
    BulletMLManager.Init(m)
    let hook i = if i = 4 then switch m
    Trace.runWith hook xml 9 |> firedSpeeds

  [<Test>]
  member _.``対照 1: param を通さない $rank は、走行中に変えると追随する``() =
    runSwitching (bml (bare "1+$rank*10")) (fun m -> m.Rank <- 0.7f)
    |> fun s -> s + "\n\n0.2 のとき 3.0、0.7 のとき 8.0。切り替えはフレーム 4"
    |> Golden.check "freeze-direct-rank"

  [<Test>]
  member _.``対照 2: param を通さない $rand も追随する``() =
    runSwitching (bml (bare "1+$rand*10")) (fun m -> m.Rand <- 0.7f)
    |> fun s -> s + "\n\n0.2 のとき 3.0、0.7 のとき 8.0。切り替えはフレーム 4"
    |> Golden.check "freeze-direct-rand"

  [<Test>]
  member _.``param に置いた $rank は追随するか``() =
    runSwitching (bml (viaParam "$rank")) (fun m -> m.Rank <- 0.7f)
    |> fun s -> s + "\n\n速さ = $1 = $rank。0.2 から 0.7 に変わるなら追随、0.2 のままなら焼き付け"
    |> Golden.check "freeze-param-rank"

  [<Test>]
  member _.``param に置いた $rand は追随するか``() =
    runSwitching (bml (viaParam "$rand")) (fun m -> m.Rand <- 0.7f)
    |> fun s -> s + "\n\n速さ = $1 = $rand。existRandomParam が守っているのはこちらだけ"
    |> Golden.check "freeze-param-rand"

  [<Test>]
  member _.``actionRef の先に直接書いた $rank は追随するか``() =
    runSwitching (bml (refNoParam "1+$rank*10")) (fun m -> m.Rank <- 0.7f)
    |> fun s -> s + "\n\nparam を通さず参照だけ挟んだ形。追随するなら、凍らせているのは param の置き換え"
    |> Golden.check "freeze-ref-noparam-rank"

  [<Test>]
  member _.``param に式ごと入れた $rank は追随するか``() =
    runSwitching (bml (viaParam "1+$rank*10")) (fun m -> m.Rank <- 0.7f)
    |> fun s -> s + "\n\n$rank 単体ではなく式ごと param に入れた形。3.0 のままなら焼き付け"
    |> Golden.check "freeze-param-rank-expr"

  /// existRandomParam が守るのは「top がひと回りしたとき作り直す」ところまで。
  /// ひと回りの中では展開は 1 回なので、$rand は 1 回しか転がらないはず
  [<Test>]
  member _.``ひと回りの中で 3 発撃つと、param の $rand は何回転がるか``() =
    let m = MutableManager(0.2f, 0.2f, 30.0f, 100.0f)
    BulletMLManager.Init(m)
    let hook i =
      if i = 1 then m.Rand <- 0.5f
      elif i = 2 then m.Rand <- 0.9f
    Trace.runWith hook (bml repeatViaParam) 8
    |> firedSpeeds
    |> fun s -> s + "\n\n毎フレーム rand を動かしている（f1 で 0.5、f2 で 0.9）。\n3 発とも同じなら、ひと回りの中では 1 回しか転がっていない"
    |> Golden.check "freeze-rand-within-loop"

  /// 挙動の手前で、機構そのものを直に測る。
  ///
  /// 11 を直す前は、$rand を含む ref があると convertBulletmlTask が Original に
  /// 生の XML を持たせ、Init() が毎周 作り直していた（param が展開のとき数へ
  /// 潰されるので、$rand だけを助けるための仕組み）。
  /// param を文字のまま渡すようにしたので、この仕組みは要らなくなった。
  /// existRandomParam は消してあり、Original は常に None になる
  [<Test>]
  member _.``param の中身と Original の関係``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))
    let probe (name: string) (expr: string) =
      let task = BulletRunner.convertBulletmlTask (readXmlString (bml (viaParam expr)))
      sprintf "  %-24s Original = %s" name (if task.Original.IsSome then "Some（作り直す）" else "None（持ち回る）")
    [ "param の中身と、その BulletML が Original を持たされるか"
      ""
      probe "<param>3</param>" "3"
      probe "<param>$rand</param>" "$rand"
      probe "<param>$rank</param>" "$rank"
      probe "<param>1+$rand*2</param>" "1+$rand*2"
      probe "<param>1+$rank*2</param>" "1+$rank*2"
      ""
      "11 を直したので、どの param でも Original は None。"
      "param は文字のまま子へ渡り、getValue が読む位置まで生き残る" ]
    |> String.concat "\n"
    |> Golden.check "freeze-original-flag"

  /// Original / Init は public であり（すぐ上の ResolveBulletRef 等が
  /// internal なのとは違う）、`t.Original <- Some xml2; t.Init(env)` は
  /// 公開面だけで組める。エンジン自身の経路（createTask / convertBulletmlTask）
  /// が Original を Some にしないことは、外から Some を書けないことを
  /// 意味しない。旧の Init の Some 腕（`this.Tasks <- toProcessable x`）が
  /// 実際に効くかを、ここで直に確かめる
  [<Test>]
  member _.``Original を書き換えて Init すると、次に走るのは書き換えた方``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))
    let xml1 = bml """<action label="top"><fire><direction type="absolute">0</direction><speed>1</speed><bullet/></fire></action>"""
    let xml2 = bml """<action label="top"><fire><direction type="absolute">90</direction><speed>9</speed><bullet/></fire></action>"""
    let born = System.Collections.Generic.List<FakeBullet>()
    let b = FakeBullet(0, born)
    let o = b :> IBulletmlObject
    o.Init()
    let task = BulletRunner.convertBulletmlTask (readXmlString xml1)
    o.Task <- Some task
    task.Original <- Some (readXmlString xml2)
    task.Init(BulletRunner.envOfGlobal o)
    BulletRunner.run o |> ignore
    Assert.That(born.Count, Is.EqualTo 1, "1 発 撃っているはず")
    let fired = born.[0] :> IBulletmlObject
    // xml1 のままなら d=0 s=1、xml2 に切り替わっていれば d=pi/2 s=9
    Assert.That(float fired.Speed, Is.EqualTo(9.0).Within(0.001), "xml1 のまま走っている（Original の書き換えが効いていない）")
    Assert.That(float fired.Dir, Is.EqualTo(System.Math.PI / 2.0).Within(0.001), "xml1 のまま走っている（Original の書き換えが効いていない）")

  /// 11 を直す代償を測る。
  ///
  /// mapEval を外して param を文字のまま渡すと、$rand / $rank は getValue まで
  /// 生き残る。そのかわり **$1 を何度も使う action では、使うたびに転がる**。
  /// 揃った扇がばらけるかどうかが、直すか決める材料になる。
  ///
  /// ここは 1 つの action の中で同じ $1 を 3 回 使い、3 発の向きが揃うかを見る
  [<Test>]
  member _.``同じ param を 1 つの action で何度も使う``() =
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, 30.0f, 100.0f))
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
    // 文字のまま渡っていれば 3 発ともばらける
    let mutable n = 0
    let hook _ =
      n <- n + 1
      BulletMLManager.Init(FixedManager(0.1f * float32 n, 0.5f, 30.0f, 100.0f))
    Trace.runWith hook xml 4
    |> fun t ->
        t.Split('\n')
        |> Array.filter (fun l -> l.Contains "  +b")
        |> Array.map (fun l -> l.Trim())
        |> String.concat "\n"
    |> fun s -> s + "\n\n同じ $1 を 3 回 使っている。向きが 3 発とも同じなら「揃った扇」、\nばらけていれば「使うたびに転がる」"
    |> Golden.check "freeze-param-reused"
