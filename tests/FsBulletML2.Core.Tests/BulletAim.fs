namespace FsBulletML2.Core.Tests

open System
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit

/// final review 3: <bullet><direction type="aim"> は、撃った側ではなく
/// 撃たれた新しい弾自身の位置から見た向きで解決する。
[<TestFixture>]
type BulletAim() =

  /// TraceRun.std が渡す自機の位置と同じ値。片方だけ動かすと期待値が割れる
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

  [<Test>]
  member _.``撃った側が原点から動いていても、bullet 側の aim は原点基準のまま動かない``() =
    // b1: 根から絶対方向 0・速さ 5 で撃たれた弾（この時点は原点 (0,0)）。
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
    let trace = TraceRun.std xml 4
    match firedDir 2 trace with
    | Some d -> d |> should (equalWithin 0.002f) originAim
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)

  [<Test>]
  member _.``fire 側の aim は、bullet 側と違って撃った側の位置を正しく使う（対照）``() =
    // 上と同じ移動のさせ方で、今度は fire 側に type="aim" を書く
    // （bullet 側は無指定）。
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
    let trace = TraceRun.std xml 4
    let firerAim = float32 (Math.Atan2(float px, float (-(py - -5.0f))))
    match firedDir 2 trace with
    | Some d ->
        d |> should (equalWithin 0.002f) firerAim
        d |> should not' (equalWithin 0.002f originAim)
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)

  [<Test>]
  member _.``1 段深くても同じ: 撃たれた弾が撃った bullet 側 aim も原点基準``() =
    // b1（根から）-> 1 フレーム待って移動 -> b2（type="aim" の bullet で撃たれる）
    // -> b2 も 1 フレーム待って移動 -> b3（type="aim" の bullet で撃たれる）。
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
    let trace = TraceRun.std xml 6
    match firedDir 2 trace, firedDir 3 trace with
    | Some d2, Some d3 ->
        d2 |> should (equalWithin 0.002f) originAim
        d3 |> should (equalWithin 0.002f) originAim
    | _ -> Assert.Fail (sprintf "b2 / b3 が撃たれていない:\n%s" trace)

  /// 較正: fire 側の aim に戻すと、1 本めの門が割れる。
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
    let trace = TraceRun.std xml 4
    let buggyValue = float32 (Math.Atan2(float px, float (-(py - -5.0f))))
    match firedDir 2 trace with
    | Some d ->
        d |> should not' (equalWithin 0.002f buggyValue)
        d |> should (equalWithin 0.002f) originAim
    | None -> Assert.Fail (sprintf "b2 が撃たれていない:\n%s" trace)
