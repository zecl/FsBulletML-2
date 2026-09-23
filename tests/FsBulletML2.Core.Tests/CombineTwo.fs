module FsBulletML2.Core.Tests.CombineTwo

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 2 つ の弾幕 を 1 つ に混ぜる ところ。
[<TestFixture>]
type CombineTwo() =

  static let read (xml: string) : Bulletml =
    Bulletml.ReadXmlString("<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + xml + "</bulletml>")

  static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

  static let count (needle: string) (s: string) =
    (s.Length - s.Replace(needle, "").Length) / needle.Length

  /// どちら も `top` と `core` を持つ。素 で繋ぐ と 参照 が迷子 になる 形
  static let a =
    """<action label="top"><repeat><times>8</times><action>
         <fire><direction type="sequence">13</direction><bulletRef label="core"/></fire>
         <wait>4</wait>
       </action></repeat></action>
       <bullet label="core"><speed>2</speed></bullet>"""

  static let b =
    """<action label="top"><repeat><times>5</times><action>
         <fire><direction type="absolute">90</direction><bulletRef label="core"/></fire>
         <wait>9</wait>
       </action></repeat></action>
       <bullet label="core"><speed>1</speed></bullet>"""

  /// 弾 が また 撃つ 形。借りる とき に 撃つ 枝 が落ちる か を見る ——
  /// 上 の `b` は 終点弾 が 空 なので、素通し しても 差 が出ない
  static let firing =
    """<action label="top"><fire><bulletRef label="core"/></fire></action>
       <bullet label="core"><speed>1</speed><action>
         <changeSpeed><speed>3</speed><term>10</term></changeSpeed>
         <fire><bullet/></fire>
       </action></bullet>"""

  /// 絶対角 と 刻み の両方 を持ち、弾 の中 にも action が在る 形。
  /// 上 の `b` では 回す 変異 も 待つ 変異 も 当てる 材料 が無い
  static let bRich =
    """<action label="top"><repeat><times>5</times><action>
         <fire><direction type="absolute">90</direction><bulletRef label="core"/></fire>
         <fire><direction type="sequence">7</direction><bulletRef label="core"/></fire>
         <actionRef label="sub"/>
         <wait>9</wait>
       </action></repeat></action>
       <action label="sub"><wait>3</wait></action>
       <bullet label="core"><speed>1</speed><action><wait>5</wait></action></bullet>"""
  [<Test>]
  member _.``繋ぎ方 の字 が 5 通り 別 に読める``() =
    Combine.ofString "beside" |> should equal (Some Combine.Beside)
    Combine.ofString "mirror" |> should equal (Some Combine.Mirror)
    Combine.ofString "after" |> should equal (Some Combine.After)
    Combine.ofString "inside" |> should equal (Some Combine.Inside)
    Combine.ofString "borrow" |> should equal (Some Combine.Borrow)
    Combine.ofString "mixed" |> should equal None

  /// top は「面 1 枚 ぶん」で、外側 に 何十波 の繰り返し が付いて いる。
  /// 残した まま A の弾 1 つ ごと に走らせる と、別 の弾幕 が丸ごと 生える
  [<Test>]
  member _.``咲く のは B の 1 波``() =
    let got = xml (Combine.apply Combine.Inside (read a) (read b))
    got |> should not' (haveSubstring "<times>5</times>")
    // A の側 の繰り返し は そのまま。剥がす のは 咲く 側 だけ
    got |> should haveSubstring "<times>8</times>"

  /// 1 波 に絞って も、A の終点弾 の数 だけ 咲く ので 掛け算 が残る。
  [<Test>]
  member _.``咲く のは 4 発 に 1 発``() =
    let got = xml (Combine.apply Combine.Inside (read a) (read b))
    got |> should haveSubstring Combine.BLOOM
    // 引く 口 は 1 つ。包み を外す と ここ が 裸 の `actionRef` に戻る
    count ("<times>" + Combine.BLOOM + "</times>") got |> should equal 1

  /// 咲いた 弾 に 寿命 を付ける と、A が 何発 撃とう と 同時数 の上 が 決まる。
  [<Test>]
  member _.``咲いた 弾 は 消える``() =
    let got = xml (Combine.apply Combine.Inside (read a) (read b))
    // 付く のは B の葉 に だけ。A の終点 は `branch` の側 で 消える
    count ("<wait>" + Combine.LIFE + "</wait>") got |> should equal 1
    got |> should haveSubstring "<wait>30</wait>"

  /// 借りる のは 弾 の形 だけ。B の撃つ 枝 を持ち込む と
  /// `Inside` と同じ 掛け算 に戻る
  [<Test>]
  member _.``弾 を借りる と 撒き方 は A のまま``() =
    let got = xml (Combine.apply Combine.Borrow (read a) (read b))
    count "<fire>" got |> should equal (count "<fire>" (xml (read a)))
    // B の要素 は 1 つ も残らない。残す と そちら が 並行 に走る
    got |> should not' (haveSubstring "b-")
    // A の終点弾 が B の速さ を持つ。A の 2 は 上書き される
    got |> should haveSubstring "<speed>1</speed>"
    got |> should not' (haveSubstring "<speed>2</speed>")

  /// B の弾 が また 撃つ 形。動き（加速・曲がり）は 借りる が、
  /// 撃つ 枝 は落とす —— 持ち込む と `Inside` と同じ 掛け算 に戻る
  [<Test>]
  member _.``借りる のは 動き だけ``() =
    let got = xml (Combine.apply Combine.Borrow (read a) (read firing))
    got |> should haveSubstring "<changeSpeed>"
    count "<fire>" got |> should equal (count "<fire>" (xml (read a)))

  /// 名前 を付け直さない と、あと から 引いた ほう が
  /// 両方 の参照 を持って いく（`BulletmlOps` は名前 で引く）
  [<Test>]
  member _.``同じ 名前 がぶつからない``() =
    let got = xml (Combine.apply Combine.Beside (read a) (read b))
    // 定義 は 2 つ。同じ 名前 の弾 が 2 つ 並ばない
    count "<bullet label=\"core\"" got |> should equal 1
    count "<bullet label=" got |> should equal 2
    // 相手 の参照 も 付け直って いる。片方 だけ だと 迷子 になる
    count "label=\"b-core\"" got |> should equal 2

  /// 走らせる 側 は「名前 が `top` で始まる `action`」を 並行 に走らせる。
  /// 相手 の top を `b-top` にする と 走らなく なる
  [<Test>]
  member _.``重ねる と 相手 も 走る``() =
    let got = xml (Combine.apply Combine.Beside (read a) (read b))
    let tops =
      System.Text.RegularExpressions.Regex.Matches(got, "<action label=\"(top[^\"]*)\"")
      |> Seq.map (fun m -> m.Groups.[1].Value)
      |> List.ofSeq
    tops.Length |> should equal 2
    tops |> List.distinct |> List.length |> should equal 2
    for t in tops do
      t.StartsWith "top" |> should equal true

  /// 弾 を B にする とき、B の top を そのまま 置く と
  /// 並行 でも 終点 でも 走って 二重 になる
  [<Test>]
  member _.``弾 を B にする と 二重 に走らない``() =
    let got = xml (Combine.apply Combine.Inside (read a) (read b))
    count "<action label=\"top\"" got |> should equal 1
    System.Text.RegularExpressions.Regex.Matches(got, "<action label=\"top[^\"]*\"").Count
    |> should equal 1
    // 引ける ままで 在る。引けない と B が 1 発 も出ない
    got |> should haveSubstring ("<actionRef label=\"" + Combine.MARK)

  /// A の終点 の弾 に入る。撃つ 枝 を持つ 弾 には 入れない
  [<Test>]
  member _.``弾 を B にする と 終点 で咲く``() =
    let got = xml (Combine.apply Combine.Inside (read a) (read b))
    got |> should haveSubstring ("<action label=\"" + Combine.MARK + "\"")
    got |> should haveSubstring "<vanish"

  /// 混ぜた もの に 印 が付く。外す とき に 元々 在った 要素 まで 削らない
  [<Test>]
  member _.``混ぜた ところ に 印 が付く``() =
    xml (Combine.apply Combine.Inside (read a) (read b))
    |> should haveSubstring Combine.MARK

  /// A の向き が正。`<bulletml type>` が 2 つ 在って も 面 は 1 つ
  [<Test>]
  member _.``面 の向き は A のもの``() =
    let sideways =
      Bulletml.ReadXmlString
        "<?xml version=\"1.0\" ?><bulletml type=\"horizontal\"><action label=\"top\"><fire><bullet/></fire></action></bulletml>"
    let got = xml (Combine.apply Combine.Beside (read a) sideways)
    got |> should haveSubstring "type=\"vertical\""
    got |> should not' (haveSubstring "horizontal")

  /// A が 既 に `b-` を持って いたら 別 の頭 を選ぶ。
  /// 選ばない と 付け直した 先 で また ぶつかる
  [<Test>]
  member _.``頭 が塞がって いたら ずらす``() =
    let taken =
      """<action label="top"><fire><bulletRef label="core"/></fire></action>
         <bullet label="core"><speed>2</speed></bullet>
         <action label="b-top"><fire><bullet/></fire></action>"""
    let got = xml (Combine.apply Combine.Beside (read taken) (read b))
    count "<action label=\"b-top\"" got |> should equal 1
    got |> should haveSubstring "b2-"

  /// 元 の要素 は 1 つ も消えない。混ぜる のは 足す 操作
  [<Test>]
  member _.``元 の 2 つ が どちら も残る``() =
    let got = xml (Combine.apply Combine.Beside (read a) (read b))
    // A の刻み と B の絶対角 が どちら も 在る
    got |> should haveSubstring ">13<"
    got |> should haveSubstring ">90<"
    count "<fire>" got |> should equal (count "<fire>" (xml (read a)) + count "<fire>" (xml (read b)))

  /// 回す のは 絶対角 だけ。`sequence` と `relative` は 直前 から の差分 で、
  /// `aim` は 自機 を見る —— どれ も 回す と 形 が壊れる
  [<Test>]
  member _.``鏡 は B の絶対角 だけ を回す``() =
    let got = xml (Combine.apply Combine.Mirror (read a) (read bRich))
    // B の 90 度 が 270 度 の向き へ。字 は 式 のまま 残す
    got |> should haveSubstring "(90) + 180"
    // B の刻み（sequence）は そのまま。回す と 渦 が壊れる
    got |> should haveSubstring ">7<"
    got |> should not' (haveSubstring "(7) + 180")
    // A の刻み も そのまま
    got |> should haveSubstring ">13<"

  /// 回す のは B だけ。A は 混ぜる 先 なので そのまま 残す
  [<Test>]
  member _.``鏡 でも A は動かない``() =
    let sided =
      """<action label="top"><fire><direction type="absolute">45</direction><bullet/></fire></action>"""
    let got = xml (Combine.apply Combine.Mirror (read sided) (read b))
    got |> should haveSubstring ">45<"
    got |> should not' (haveSubstring "(45) + 180")

  /// 同時 に出す と 1 層 に見える。時差 で割る
  [<Test>]
  member _.``ずらし は B の top を遅らせる``() =
    let got = xml (Combine.apply Combine.After (read a) (read b))
    got |> should haveSubstring ("<wait>" + Combine.LAG + "</wait>")
    // 重ねる 側 には 入らない
    xml (Combine.apply Combine.Beside (read a) (read b))
    |> should not' (haveSubstring ("<wait>" + Combine.LAG + "</wait>"))

  /// 当てる のは `top` だけ。弾 の中 の action に入れる と
  /// 1 発 ごと に 遅れて 形 が崩れる
  [<Test>]
  member _.``ずらし は 弾 の中 に入らない``() =
    let got = xml (Combine.apply Combine.After (read a) (read bRich))
    count ("<wait>" + Combine.LAG + "</wait>") got |> should equal 1
    // 弾 の中 と、引かれる 側 の action は そのまま。
    // ここ に入る と 1 発 ごと・1 回 ごと に 遅れて 形 が崩れる
    got |> should haveSubstring "<wait>5</wait>"
    got |> should haveSubstring "<wait>3</wait>"

  /// 根 の下 へ置く 3 つ は どれ も 足し算。要素 の数 が変わらない
  [<Test>]
  member _.``並べる 3 つ は 要素 の数 が同じ``() =
    let tops (j: Combine.Join) =
      match Combine.apply j (read a) (read b) with
      | Bulletml(_, elms) -> List.length elms
    tops Combine.Mirror |> should equal (tops Combine.Beside)
    tops Combine.After |> should equal (tops Combine.Beside)
