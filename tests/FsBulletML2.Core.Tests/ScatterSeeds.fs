module FsBulletML2.Core.Tests.ScatterSeeds

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 1 波 を 種 に載せ、離れた 場所 で 咲かせる ところ。
///
/// 当てる のは 花 でない 弾幕（刻み で撒く 輪）—— 中身 を読まない こと が この 操作 の値打ち なので、
/// 花 で測る と「花 だから 効いた」と 区別 が つかない。
///
/// --- 較正（当てた変異 と、赤くなった点）
///
///   `apply` の `count <= 1` を落とす        1 は そのまま
///   `stemBullet` の撒き を 1 発 に固定   撒いた 数 だけ 種 が出る
///   `vertexDeg` の 0.5 を落とす             真上 に 種 を飛ばさない
///   `stemFire` の 180 を 0 に               茎 は 真下 へ伸びる
///   `stemBullet` の `Vanish` を落とす        撒き終えた 茎 は 消える
///   `split` の `wave` を空 に               1 波 は 種 の中 へ移る
///   `splitTailWaits` を素通し に            間合い は 撒く 側 に残る
///   `split` で `Repeat` を 種 へ移す        繰り返し は 撒く 側 に残る
///   `seedBullet` の `Vanish` を落とす       咲いた 種 は 消える
///   `freeName` を素通し に                  名前 がぶつかったら 番号 を足す
///   `split` の `None` を落とす              待ち が残らない 形 は 散らさない ／ 咲く 数 は 1
///   `places` を count 固定 に               咲く 数 は 1
///   `places` を 1 固定 に                   咲く 数 は 1
///
/// 「散らした なら 撒く 側 は 待つ」は 書いて 落とした ——
/// 単独 で赤 に する 変異 が 無い。`ring` の 撒く 側 は
/// 「間合い と 繰り返し は 撒く 側 に残る」が `<wait>4</wait>` で もっと 強く 見て いる
[<TestFixture>]
type ScatterSeeds() =

  static let read (xml: string) : Bulletml =
    Bulletml.ReadXmlString("<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + xml + "</bulletml>")

  static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

  static let count (needle: string) (s: string) =
    (s.Length - s.Replace(needle, "").Length) / needle.Length

  /// 刻み で 1 周 する 輪。花 ではない
  static let ring =
    """<action label="top"><repeat><times>8</times><action>
         <fire><direction type="sequence">13</direction><bulletRef label="core"/></fire>
         <wait>4</wait>
       </action></repeat></action>
       <bullet label="core"><speed>2</speed></bullet>"""

  /// 待ち が 内側 の `repeat` の中 にしか 無い 輪。外 の繰り返し の末尾 は `repeat`
  static let nested =
    """<action label="top"><repeat><times>180</times><action>
         <repeat><times>13</times><action>
           <fire><direction type="sequence">13</direction><bulletRef label="core"/></fire>
           <wait>90</wait>
         </action></repeat>
       </action></repeat></action>
       <bullet label="core"><speed>2</speed></bullet>"""

  /// 待ち が 1 つ も無い 台本
  static let waitless =
    """<action label="top"><fire><bulletRef label="core"/></fire></action>
       <bullet label="core"><speed>2</speed></bullet>"""

  /// 種 の定義 から 後ろ。撒く 側（`top`）や 茎 と 見分ける ため
  static let seedPart (s: string) =
    let i = s.IndexOf "<bullet label=\"scattered-seed"
    if i < 0 then "" else s.Substring i

  /// 茎 の定義 だけ。撒く 向き は ここ に在る
  static let stemPart (s: string) =
    let i = s.IndexOf "<bullet label=\"scattered-stem"
    let j = s.IndexOf "<bullet label=\"scattered-seed"
    if i < 0 || j < i then "" else s.Substring(i, j - i)

  static let topPart (s: string) =
    let i = s.IndexOf "<bullet label=\"scattered-stem"
    if i < 0 then s else s.Substring(0, i)

  [<Test>]
  member _.``1 以下 は そのまま``() =
    let b = read ring
    xml (Scatter.apply 1 b) |> should equal (xml b)
    xml (Scatter.apply 0 b) |> should equal (xml b)
    xml (Scatter.apply -3 b) |> should equal (xml b)

  /// 撒く のは 茎 の先。`top` から 出る のは 茎 1 本 だけ
  [<Test>]
  member _.``撒いた 数 だけ 種 を撃つ``() =
    for n in [ 2; 3; 5; 8 ] do
      let s = xml (Scatter.apply n (read ring))
      count "<bulletRef label=\"scattered-stem\"" (topPart s) |> should equal 1
      count "<bulletRef label=\"scattered-seed\"" (stemPart s) |> should equal n

  /// 敵 (240, 80) を中心 に する と、上 の頂点 が y = 20 に来て 咲いた 弾幕 の上半分 が 天井 で切れた
  [<Test>]
  member _.``茎 が n 角形 の中心 を 下 へ運ぶ``() =
    let s = xml (Scatter.apply 3 (read ring))
    topPart s |> should haveSubstring "<direction type=\"absolute\">180.00</direction>"
    stemPart s |> should haveSubstring "<vanish"
    // 中心 は 敵 の下 に来て、いちばん 上 の頂点 も 敵 より 下
    for n in [ 2; 3; 5; 8 ] do
      let up = [ 0 .. n - 1 ] |> List.map (fun j -> Scatter.REACH * cos (Scatter.vertexDeg n j * System.Math.PI / 180.0))
      80.0 + Scatter.DROP - List.max up |> should be (greaterThanOrEqualTo 80.0)

  /// 真上 は いつも 辺 の真ん中。頂点 を 真上 に置く と、その 種 だけ 面 の外 で消えて
  /// n か所 の うち 1 つ が咲かない
  [<Test>]
  member _.``n 角形 の頂点 へ撒き、真上 を避ける``() =
    Scatter.vertexDeg 3 0 |> should (equalWithin 1e-9) 60.0
    Scatter.vertexDeg 3 1 |> should (equalWithin 1e-9) 180.0
    Scatter.vertexDeg 3 2 |> should (equalWithin 1e-9) 300.0
    Scatter.vertexDeg 2 0 |> should (equalWithin 1e-9) 90.0
    Scatter.vertexDeg 4 0 |> should (equalWithin 1e-9) 45.0
    let fromUp (d: float) = min d (360.0 - d)
    for n in [ 2; 3; 4; 5; 8 ] do
      [ 0 .. n - 1 ]
      |> List.map (Scatter.vertexDeg n >> fromUp)
      |> List.min
      |> should be (greaterThanOrEqualTo (180.0 / float n - 1e-9))
    xml (Scatter.apply 3 (read ring)) |> should haveSubstring "180.00"

  [<Test>]
  member _.``1 波 は 種 の中 へ移り、撒く 側 から 消える``() =
    let s = xml (Scatter.apply 3 (read ring))
    topPart s |> should not' (haveSubstring "sequence")
    seedPart s |> should haveSubstring "sequence"
    seedPart s |> should haveSubstring "core"

  [<Test>]
  member _.``間合い と 繰り返し は 撒く 側 に残る``() =
    let s = xml (Scatter.apply 3 (read ring))
    topPart s |> should haveSubstring "<wait>4</wait>"
    topPart s |> should haveSubstring "<times>8</times>"
    seedPart s |> should not' (haveSubstring "<wait>4</wait>")
    seedPart s |> should not' (haveSubstring "<times>8</times>")

  /// 面 は 台本 を終えた 弾 を 頭 から 走らせ直す。消さない と 同じ 種 が 何度 も 咲く
  [<Test>]
  member _.``咲いた 種 は 消える``() =
    seedPart (xml (Scatter.apply 3 (read ring))) |> should haveSubstring "<vanish"

  [<Test>]
  member _.``名前 がぶつかったら 番号 を足す``() =
    let taken =
      ring + """<bullet label="scattered-seed"><speed>1</speed></bullet>"""
    let s = xml (Scatter.apply 2 (read taken))
    s |> should haveSubstring "label=\"scattered-seed2\""
    count "<bulletRef label=\"scattered-seed2\"" s |> should equal 2

  /// 撃つ もの が無い 台本 は そのまま。「作れなかった」に しない
  [<Test>]
  member _.``撒く もの が無ければ そのまま``() =
    let idle = read """<action label="top"><wait>10</wait></action>"""
    xml (Scatter.apply 3 idle) |> should equal (xml idle)

  /// 撒く 側 に 待ち が 1 つ も残らない と、その 台本 は 1 コマ で終わる ——
  /// 面 は 終えた 弾 を 頭 から 走らせ直す ので、毎コマ 茎 を撒き 直す。
  ///
  /// `nested` を 3 か所 に散らした 木 を Felt で 100 コマ 走らせる と 221,390 発 まで 増えた
  /// （散らさなければ 1,482 発）。待ち は 内側 の `repeat` の中 に在り、外 の末尾 には 1 つ も無い
  [<Test>]
  member _.``撒く 側 に 待ち が残らない 形 は 散らさない``() =
    for src in [ nested; waitless ] do
      let b = read src
      xml (Scatter.apply 3 b) |> should equal (xml b)

  /// 散らせない 木 に `count` を答える と、取り分 を割る 側 が 弾 を 1/n に薄める
  [<Test>]
  member _.``散らせない 木 の 咲く 数 は 1``() =
    Scatter.places 3 (read ring) |> should equal 3
    Scatter.places 3 (read nested) |> should equal 1
    Scatter.places 3 (read waitless) |> should equal 1
    Scatter.places 1 (read ring) |> should equal 1
