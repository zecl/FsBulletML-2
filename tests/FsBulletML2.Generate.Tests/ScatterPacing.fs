module FsBulletML2.Generate.Tests.ScatterPacing

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 散らした 弾幕 を 走らせて 数える ところ。
///
/// `ScatterSeeds`（Core.Tests）は 木 の 字 を見る。ここ は 走行 を見る ——
/// 1 コマ の ずれ も、余計 に待つ 20 コマ も、XML には 出ない。
///
/// --- 較正（当てた変異 と、赤くなった点）
///
///   `changeSpeed` を `Wait term` に          散らして も 1 周 が変わらない（1 コマ ずれる）
///   `accel` を `Wait term` に                散らして も 1 周 が変わらない（余計 に待つ）
///   `ghost` を いつも `Some []` に           1 周 が変わらない ／ 弾数 が跳ねない
///
/// 上 の 3 つ は **XML を見る 門 では 1 つ も 赤 に ならない** ——
/// 1 コマ の ずれ も、余計 に待つ 20 コマ も、字 には 出ない。
///
/// `split` の `spends g` を true に固定／`spends` の再帰 を落とす は ここ では
/// 赤 に ならない。`paced` も 生成 した 型 も 写し が 上っ面 に 待ち を持つ ので 変わらない ——
/// どちら も `ScatterSeeds`（Core.Tests）の `waitless` と `nested` が 捕まえる。
///
/// **弾数 の比 だけ では 足りない。** 散らなく なった とき も 比 は 1 倍 前後 に収まる ので、
/// `places` の assert を 先 に置く
[<TestFixture>]
type ScatterPacing() =

  static let read (x: string) =
    Bulletml.ReadXmlString("<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + x + "</bulletml>")

  static let peak (frames: int) (b: Bulletml) =
    Felt.run frames b |> List.map (fun s -> s.Positions.Length) |> List.max

  /// 1 発 目 と 2 発 目 の あいだ の コマ 数。撒く 側 の 1 周 を測る
  static let cycleOf (b: Bulletml) =
    Felt.run 400 b
    |> List.mapi (fun i s -> i, s.Positions.Length)
    |> List.fold (fun (prev, acc) (i, n) -> n, (if n > prev then i :: acc else acc)) (0, [])
    |> snd
    |> List.rev
    |> function
       | a :: c :: _ -> c - a
       | _ -> -1

  /// `changeSpeed` と `accel` を 1 つ ずつ 挟んだ 輪。
  /// 実測: wait 20 は 20 コマ / changeSpeed term 20 は 19 コマ / accel term 20 は 1 コマ
  static let paced =
    """<action label="top"><repeat><times>9</times><action>
         <wait>4</wait>
         <fire><bulletRef label="mark"/></fire>
         <changeSpeed><speed>0.02</speed><term>20</term></changeSpeed>
         <accel><vertical>1</vertical><term>20</term></accel>
       </action></repeat></action>
       <bullet label="mark"><speed>0.01</speed></bullet>"""

  /// 散らして も 撒く 側 の 1 周 は 変わらない。
  /// 写し は 間合い を 構造 ごと 運ぶ ので、数 を 1 つ も 計算 して いない
  [<Test>]
  member _.``散らして も 1 周 の コマ 数 が変わらない``() =
    let b = read paced
    Scatter.places 3 b |> should equal 3
    cycleOf (Scatter.apply 3 b) |> should equal (cycleOf b)

  /// 撒く 側 に 間合い が戻った ので、面 が 走らせ直して も 毎コマ 茎 を撒き 直さない。
  ///
  /// `Depth` を 2.0 にする のは、ここ が 散らせなかった 軸 だから ——
  /// 直す 前 は 5 つ の型 とも depth 1.0 以上 で 1 か所 のまま だった
  [<Test>]
  member _.``散らして も 弾数 が 跳ねない``() =
    for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
      let spec =
        PatternSpec.create (fun a ->
          { a with Kind = kind; Speed = 1.0; Density = 1.5; Depth = 2.0; Cascade = 2.0; Layers = 2.0 })
      let plain = (Generate.generateTo 900.0 spec).Bulletml
      Scatter.places 3 plain |> should equal 3
      let three = Scatter.apply 3 (Generate.generateTo 300.0 spec).Bulletml
      // 直す 前 は 708 倍。取り分 を 3 で割って いる ので 本当 は 1 倍 前後
      float (peak 300 three) / float (peak 300 plain) |> should be (lessThan 3.0)
