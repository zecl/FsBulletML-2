namespace FsBulletML2.Core.Tests

open System
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Processable

/// final review 3: <bullet><direction type="aim"> は、撃った側ではなく
/// 撃たれた新しい弾自身の位置から見た向きで解決する。
///
/// 旧 createTask (bulletElm) (bulletmlTask) (bullet: IBulletmlObject) は
/// fireCommand から newBullet を渡されて呼ばれ、GetNewBullet() 直後・
/// 位置をコピーする前（まだ (0, 0)）の newBullet 自身で GetAimDir() /
/// GetEnemyAimDir() を読んでいた。fire 側の aim（撃った側の位置に依る）と
/// bullet 側の aim（常に原点）は別の値になる —— 撃った側が原点から
/// 動いていれば、この 2 つは違う数になる。
///
/// 根の top action の直後には必ず長い <wait> を置いてある。根は
/// IsBullet = false なので Retired にならず、Finished が立つたびに
/// Trace.fs が task.Init(envOfGlobal o) を呼んで木を丸ごと引き直す
/// （設計文書 5.3「走らせ直す」）。<wait> を置かずに <fire> 1 本だけで
/// action を終わらせると、根は毎フレーム Init され直して撃ち直してしまい、
/// 見たい弾（b2 / b3）の番号がずれる
[<TestFixture>]
[<NonParallelizable>]
type BulletAim() =

  let px, py = 30.0f, 100.0f

  let bml body =
    """<?xml version="1.0" ?>
<!DOCTYPE bulletml SYSTEM "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml/bulletml.dtd">
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
""" + body + "\n</bulletml>"

  // 名前を問わず、指定した番号の弾が生まれた行から d= を取り出す
  let firedDir (bulletNo: int) (trace: string) =
    let m = Regex.Match(trace, sprintf @"\+b%d d=([-\d.]+)" bulletNo)
    if m.Success then Some (float32 m.Groups.[1].Value) else None

  // 原点（新しい弾が生まれた直後、まだ位置をコピーする前の位置）から見た
  // 自機への向き。撃った側の位置に関わらず一定
  let originAim = float32 (Math.Atan2(float px, float -py))

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(FixedManager(0.5f, 0.5f, px, py))

  [<Test>]
  member _.``撃った側が原点から動いていても、bullet 側の aim は原点基準のまま動かない``() =
    // b1: 根から絶対方向 0・速さ 5 で撃たれた弾（この時点は原点 (0,0)）。
    // 1 フレーム待ってから（この間に (0,0) -> (0,-5) へ動く）、
    // 自分の位置から type="aim" の弾（b2）を撃つ。
    // 旧の壊れ方を写すと b2 の向きは b1 の "今の" 位置 (0,-5) を基準にした
    // aim になるが、正しくは新しい弾（b2）自身の原点基準の aim になるはず
    let xml =
      bml """<action label="top">
  <fire>
    <bullet>
      <direction type="absolute">0</direction>
      <speed type="absolute">5</speed>
      <action>
        <wait>1</wait>
        <fire><bullet><direction type="aim">0</direction></bullet></fire>
      </action>
    </bullet>
  </fire>
  <wait>100</wait>
</action>"""
    let trace = Trace.run xml 4
    match firedDir 2 trace with
    | Some d -> d |> should (equalWithin 0.002f) originAim
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)

  [<Test>]
  member _.``fire 側の aim は、bullet 側と違って撃った側の位置を正しく使う（対照）``() =
    // 上と同じ移動のさせ方で、今度は fire 側に type="aim" を書く
    // （bullet 側は無指定）。こちらは撃った側 (0,-5) の aim になるはずで、
    // 原点基準の値とは違う数になる —— 混ざっていないことの確認
    let xml =
      bml """<action label="top">
  <fire>
    <bullet>
      <direction type="absolute">0</direction>
      <speed type="absolute">5</speed>
      <action>
        <wait>1</wait>
        <fire><direction type="aim">0</direction><bullet/></fire>
      </action>
    </bullet>
  </fire>
  <wait>100</wait>
</action>"""
    let trace = Trace.run xml 4
    let firerAim = float32 (Math.Atan2(float px, float (-(py - -5.0f))))
    match firedDir 2 trace with
    | Some d ->
        d |> should (equalWithin 0.002f) firerAim
        // 原点基準の値とは別の数になっていること（混同していないことの確認）
        d |> should not' (equalWithin 0.002f originAim)
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)

  [<Test>]
  member _.``1 段深くても同じ: 撃たれた弾が撃った bullet 側 aim も原点基準``() =
    // b1（根から）-> 1 フレーム待って移動 -> b2（type="aim" の bullet で撃たれる）
    // -> b2 も 1 フレーム待って移動 -> b3（type="aim" の bullet で撃たれる）。
    // b2 も b3 も、それぞれ「撃たれた瞬間の自分の位置」ではなく原点基準の
    // aim になるはず（b2 は既に原点基準で撃たれているので、b2 が積む
    // 移動と無関係に b3 もまた原点基準になる）
    let xml =
      bml """<action label="top">
  <fire>
    <bullet>
      <direction type="absolute">0</direction>
      <speed type="absolute">5</speed>
      <action>
        <wait>1</wait>
        <fire>
          <bullet>
            <direction type="aim">0</direction>
            <speed type="absolute">3</speed>
            <action>
              <wait>1</wait>
              <fire><bullet><direction type="aim">0</direction></bullet></fire>
            </action>
          </bullet>
        </fire>
      </action>
    </bullet>
  </fire>
  <wait>100</wait>
</action>"""
    let trace = Trace.run xml 6
    match firedDir 2 trace, firedDir 3 trace with
    | Some d2, Some d3 ->
        d2 |> should (equalWithin 0.002f) originAim
        d3 |> should (equalWithin 0.002f) originAim
    | _ -> Assert.Fail (sprintf "b2 / b3 が撃たれていない:\n%s" trace)

  /// 較正: PendingBulletAim を無視して、常に fire 側と同じ aim（撃った側の
  /// 位置基準）を bullet 側にも使うよう戻すと、1 本めの門が割れることを
  /// Step レベルで確かめる（StepFire.fs の対応するテストを参照）。
  /// ここでは HEAD の壊れた値（撃った側の位置 (0,-5) 基準）を、
  /// 直した後の値と並べて書いておく
  [<Test>]
  member _.``較正: 直す前の値は撃った側の位置基準になっていたはず``() =
    let xml =
      bml """<action label="top">
  <fire>
    <bullet>
      <direction type="absolute">0</direction>
      <speed type="absolute">5</speed>
      <action>
        <wait>1</wait>
        <fire><bullet><direction type="aim">0</direction></bullet></fire>
      </action>
    </bullet>
  </fire>
  <wait>100</wait>
</action>"""
    let trace = Trace.run xml 4
    let buggyValue = float32 (Math.Atan2(float px, float (-(py - -5.0f))))
    match firedDir 2 trace with
    | Some d ->
        // 直したあとの値は、旧の壊れ方（撃った側の位置基準）とは異なる
        d |> should not' (equalWithin 0.002f buggyValue)
        d |> should (equalWithin 0.002f) originAim
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)
