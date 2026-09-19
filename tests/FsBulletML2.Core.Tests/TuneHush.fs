module FsBulletML2.Core.Tests.TuneHush

open NUnit.Framework
open FsUnit
open FsBulletML2

/// いちばん 深い 段 を 黙らせる ところ。
///
/// --- 較正（当てた変異 と、赤 になった 点数）
///
///   1 段 の弾幕 も 黙らせる          1 点
///   いちばん 浅い 段 を黙らせる        2 点
///   times 0 でなく 1 で包む        2 点
///   hushCount が 印 の中 を見ない    1 点（2 段 目 の包み が 1 段 目 を含む）
///   deepest が 黙らせた 枝 も数える    1 点
///   unhush が 最後 でなく 最初 を外す   1 点
///   hush の順 を 数 と同じ に         1 点
[<TestFixture>]
type TuneHush() =

  static let wrap (body: string) =
    "<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + body + "</bulletml>"

  static let read (src: string) = Bulletml.ReadXmlString(wrap src)

  static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

  static let press (knob: Tune.Knob) (n: int) (b: Bulletml) =
    List.fold (fun x _ -> Tune.apply knob x) b [ 1 .. n ]

  static let marks (s: string) =
    System.Text.RegularExpressions.Regex.Matches(s, "tuned-hush").Count

  /// 3 段。根 の fire が 1 段 目
  static let three =
    """<action label="top">
         <fire><bullet><action>
           <wait>10</wait>
           <fire><bullet><action>
             <wait>10</wait>
             <fire><bullet/></fire>
           </action></bullet></fire>
         </action></bullet></fire>
       </action>"""

  /// 1 段 だけ。黙らせる と 弾幕 が丸ごと 止まる
  static let flat =
    """<action label="top"><fire><speed>2</speed><bullet/></fire></action>"""

  [<Test>]
  member _.``黙らせる と いちばん 深い 段 が 包まれる``() =
    let got = xml (Tune.apply Tune.Hush (read three))
    marks got |> should equal 1
    got |> should haveSubstring "<times>0</times>"
    // 3 段 目 が包まれる。2 つ 目 の wait より 後ろ
    got.IndexOf "tuned-hush" |> should be (greaterThan (got.LastIndexOf "<wait>10</wait>"))

  /// 根 の `fire` を黙らせる と 弾幕 が丸ごと 止まる
  [<Test>]
  member _.``1 段 しか 無い 弾幕 は 触らない``() =
    xml (Tune.apply Tune.Hush (read flat)) |> should equal (xml (read flat))

  [<Test>]
  member _.``もう 1 段 黙らせる と 上 の段 も 包まれる``() =
    let got = xml (press Tune.Hush 2 (read three))
    marks got |> should equal 2
    got |> should haveSubstring "tuned-hush1"
    got |> should haveSubstring "tuned-hush2"

  [<Test>]
  member _.``押し戻す と 元 の字 に戻る``() =
    let b = read three
    xml (Tune.apply Tune.Unhush (Tune.apply Tune.Hush b)) |> should equal (xml b)
    xml (press Tune.Unhush 2 (press Tune.Hush 2 b)) |> should equal (xml b)

  [<Test>]
  member _.``包んで いない とき の 押し戻し は 何 も しない``() =
    let b = read three
    xml (Tune.apply Tune.Unhush b) |> should equal (xml b)

  [<Test>]
  member _.``軸 の名 から 引ける``() =
    Tune.axis "hush" |> should equal (Some(Tune.Hush, Tune.Unhush))
    xml (Tune.applySteps [ "hush", 1 ] (read three))
      |> should equal (xml (Tune.apply Tune.Hush (read three)))

  /// 先 に包む と `times 0` に 倍率 が掛かる
  [<Test>]
  member _.``数 を振って から 黙らせる``() =
    let b = read three
    let both = xml (Tune.applySteps [ "hush", 1; "arms", 1 ] b)
    both |> should equal (xml (Tune.apply Tune.Hush (Tune.apply Tune.MoreArms b)))
    both |> should not' (equal (xml (Tune.apply Tune.MoreArms (Tune.apply Tune.Hush b))))

  /// 字 が変わる だけ では「効いた」と言えない
  [<Test>]
  member _.``黙らせる と 撃つ 弾 が減る``() =
    let src =
      """<action label="top">
           <repeat><times>20</times><action>
             <fire><direction type="sequence">30</direction><speed>2</speed>
               <bullet><action>
                 <wait>15</wait>
                 <repeat><times>6</times><action>
                   <fire><direction type="sequence">60</direction><speed>1.5</speed><bullet/></fire>
                 </action></repeat>
               </action></bullet>
             </fire>
             <wait>8</wait>
           </action></repeat>
         </action>"""

    let firedBy (b: Bulletml) =
      let t = TraceRun.std (xml b) 200
      t.Split('\n') |> Array.filter (fun l -> l.Contains "  +b") |> Array.length

    let before = firedBy (read src)
    let after = firedBy (Tune.apply Tune.Hush (read src))

    if before = 0 then Assert.Fail "素 の弾幕 が 1 発 も 撃って いません。この点 は 何 も 測って いません"
    after |> should be (lessThan before)

    TestContext.Out.WriteLine(sprintf "200 コマ で 撃った 弾 %d -> %d 発" before after)
