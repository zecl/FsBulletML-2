module FsBulletML2.Core.Tests.TuneThin

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 終点 の弾 に 寿命 を付けて 間引く ところ。
[<TestFixture>]
type TuneThin() =

  static let wrap (body: string) =
    "<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + body + "</bulletml>"

  static let read (src: string) = Bulletml.ReadXmlString(wrap src)

  static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

  static let press (knob: Tune.Knob) (n: int) (b: Bulletml) =
    List.fold (fun x _ -> Tune.apply knob x) b [ 1 .. n ]

  /// 段 を 1 つ 持つ 弾幕。終点 は 内側 の弾
  static let deep =
    """<action label="top">
         <fire><bullet><action>
           <wait>20</wait>
           <fire><bullet/></fire>
         </action></bullet></fire>
       </action>"""

  static let plain =
    """<action label="top"><fire><speed>2</speed><bullet/></fire></action>"""

  [<Test>]
  member _.``薄く する と 終点 の弾 に 寿命 が付く``() =
    let got = xml (Tune.apply Tune.Thinner (read plain))
    got |> should haveSubstring "tuned-thin"
    got |> should haveSubstring "<wait>60</wait>"
    got |> should haveSubstring "<vanish"

  /// 撃つ 枝 を持つ 弾 は 段 の途中。寿命 を付ける と
  /// その先 の段 が 丸ごと 出なく なる
  [<Test>]
  member _.``寿命 が付く のは 終点 の弾 だけ``() =
    let got = xml (Tune.apply Tune.Thinner (read deep))
    let marks = System.Text.RegularExpressions.Regex.Matches(got, "tuned-thin").Count
    marks |> should equal 1
    // 外側 の弾 の直下 に 印 が来て いない こと ——
    // 来る と 内側 の `fire` が 走る 前 に 消える
    got.IndexOf "tuned-thin" |> should be (greaterThan (got.IndexOf "<wait>20</wait>"))

  [<Test>]
  member _.``もう 1 段 薄く する と 寿命 が短く なる``() =
    let got = xml (press Tune.Thinner 2 (read plain))
    got |> should haveSubstring "(60) * 0.8"
    System.Text.RegularExpressions.Regex.Matches(got, "tuned-thin").Count |> should equal 1

  /// 押し戻す と 元 の字 に戻る。倍率 だけ 消す と 印 が残る
  [<Test>]
  member _.``押し戻す と 元 の字 に戻る``() =
    let b = read plain
    xml (Tune.apply Tune.Thicker (Tune.apply Tune.Thinner b)) |> should equal (xml b)
    xml (press Tune.Thicker 3 (press Tune.Thinner 3 b)) |> should equal (xml b)

  /// 印 が無い 弾 に `Thicker` を当てて 寿命 が生える と、
  /// 「長く 生きる ように する」が 逆 に弾 を消す
  [<Test>]
  member _.``印 が無い とき の 濃く する は 何 も しない``() =
    let b = read plain
    xml (Tune.apply Tune.Thicker b) |> should equal (xml b)
    xml (press Tune.Thicker 4 b) |> should equal (xml b)

  [<Test>]
  member _.``軸 の名 から 引ける``() =
    Tune.axis "thin" |> should equal (Some(Tune.Thinner, Tune.Thicker))
    xml (Tune.applySteps [ "thin", 1 ] (read plain))
      |> should equal (xml (Tune.apply Tune.Thinner (read plain)))
    xml (Tune.applySteps [ "thin", -1 ] (Tune.apply Tune.Thinner (read plain)))
      |> should equal (xml (read plain))

  /// 寿命 は 終点 の弾 に乗る ので、先 に乗せる と 足した 段 が 消えない
  [<Test>]
  member _.``段 を足して から 間引く``() =
    let b = read plain
    let both = xml (Tune.applySteps [ "thin", 1; "split", 1 ] b)
    both |> should equal (xml (Tune.apply Tune.Thinner (Tune.apply Tune.AddSplit b)))
    both |> should not' (equal (xml (Tune.apply Tune.AddSplit (Tune.apply Tune.Thinner b))))

  /// 先 に間引く と、寿命 にも 密度 の倍率 が掛かる ——
  /// 寿命 は 何コマ で消える か で、密度 とは 別 の軸
  [<Test>]
  member _.``数 を振って から 間引く``() =
    let b = read plain
    let both = xml (Tune.applySteps [ "thin", 1; "density", 1 ] b)
    both |> should equal (xml (Tune.apply Tune.Thinner (Tune.apply Tune.Denser b)))
    both |> should not' (equal (xml (Tune.apply Tune.Denser (Tune.apply Tune.Thinner b))))

  /// 同時 に居る 弾 が減る こと を 走行 で言う。
  /// 字 が変わる だけ では「効いた」と言えない
  [<Test>]
  member _.``薄く する と 同時 に居る 弾 が減る``() =
    let src =
      """<action label="top">
           <repeat><times>200</times><action>
             <fire><direction type="sequence">17</direction><speed>1.5</speed><bullet/></fire>
             <wait>2</wait>
           </action></repeat>
         </action>"""

    let aliveAt (b: Bulletml) =
      let t = TraceRun.std (xml b) 300
      let lines = t.Split('\n')
      match lines |> Array.tryFindIndexBack (fun l -> l.StartsWith "f") with
      | Some i -> lines.[i + 1 ..] |> Array.filter (fun l -> l.StartsWith "  b") |> Array.length
      | None -> 0

    let before = aliveAt (read src)
    let after = aliveAt (Tune.apply Tune.Thinner (read src))

    // 当てる 先 が 本当 に在る か を 先 に見る。0 発 なら 緑 にしない
    if before = 0 then Assert.Fail "素 の弾幕 が 1 発 も 残って いません。この点 は 何 も 測って いません"
    after |> should be (lessThan before)

    TestContext.Out.WriteLine(sprintf "300 コマ 後 の同時数 %d -> %d 発" before after)
