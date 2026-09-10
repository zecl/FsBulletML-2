namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Playground

/// **走った字ごとの弾コマ**（v3.9）。
///
/// 数えるのは**延べ** —— 500 発 が 1 コマ 同じ行を通ったら 500。
/// 「どこに時間が居るか」を見る軸なので、1 コマ と数えると
/// **いちばん重い行がいちばん軽く見える。**
///
/// --- 畳むところが本体
///
/// 走る木のノードは**同じ行へ 2 つ 以上 戻る**（`bulletRef` の展開など）。
/// 畳まずに並べると、1 行 に `32%` と `31%` が別々 に付いて
/// **本当は 63% だと読めない** —— 実機で見て気づいた。
///
/// **予測が外れたのは、測る側のバグだった。** 版の頭で
/// 「上位 5 行 で 7 割」と凍結して 45% と出たが、畳んだら 77.8% になった。
///
/// --- 較正（`Focus.SpanTop` の中だけに 1 か所 当てて、赤くなった門を数えた）
///
/// **`TallyTop` に同じ 2 行 が在る**（並べ替えと切り出し）。全文で置換すると
/// 2 か所 に当たり、赤くなったのがどちらの門かが読めない ——
/// **当てた箇所の数を 1 で止めてから回す。**
///
///   `if i >= 0` を `if true`                失敗 2（出すのは決まった行だけ /
///                                           決まらない行は、数には居るが出さない）
///   `sortByDescending` を `sortBy`          失敗 1（多い順に返る）
///   `|> Seq.truncate n` を消す              失敗 1（n を超えて返さない）
///   `byIndex.[i] <- 足す` を `<- 上書き`     失敗 1（畳んだ数は、畳む前の合計と変わらない）
///   `if countSpans` を `if true`            失敗 1（数えるのを切ると 0 のまま）
///
/// --- 較正（v4.0.2。切り替えと捨てる口）
///
/// **`spans.Clear()` は 2 か所 に在った**（面を建て直すところと、捨てる口）——
/// 全文で置換すると 2 か所 に当たるので、`clearSpans` に畳んでから当てた。
///
///   `spans.Clear()` を `spans.Count |> ignore`     失敗 1（捨てると、それまでの数は残らない）
///   `spanTotal <- 0` を `<- spanTotal + 0`         失敗 1（同上）
///   `countSpans <- v` を `<- countSpans && v`      失敗 1（切ってから入れ直すと、また数え始める）
///
/// **1 つ 目 は、はじめ緑のまま通った。** `Shared` にも同梱の弾幕にも
/// 添字が決まらない行が 1 つ も無く、**当てる材料が入力に無かった** ——
/// `vanish` を 2 か所 に書いた `Ambiguous` を足して赤になる
[<TestFixture>]
type SpanWeight() =

  /// **同じ `bullet` を 2 か所 から撃つ。** 走る木では 2 つ の台本になるが、
  /// 読んだ木では 1 つ —— **畳まないと同じ行が 2 度 出る**
  [<Literal>]
  let Shared = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">170</direction><speed>1.4</speed><bulletRef label="b"/></fire>
    <fire><direction type="absolute">190</direction><speed>1.4</speed><bulletRef label="b"/></fire>
    <wait>200</wait>
  </action>
  <bullet label="b">
    <action>
      <repeat><times>60</times>
        <action><wait>1</wait></action>
      </repeat>
    </action>
  </bullet>
</bulletml>"""

  /// **`vanish` を 2 か所 に書く。** 引数なしの腕は singleton なので、
  /// 読んだ木では**同じ参照が並びに 2 度 出る** —— 添字が決まらず、
  /// `SpanTop` は出さない（`Focus.indexOfRead` が -1 を返す）。
  ///
  /// **この本が要るのは、材料が無いと門が緑のままだから。** `Shared` にも
  /// 同梱の弾幕にも決まらない行が 1 つ も無く、「決まらない行も出す」変異を
  /// 当てても赤くならなかった
  [<Literal>]
  let Ambiguous = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">180</direction><speed>1</speed>
      <bullet><action><wait>3</wait><vanish/></action></bullet></fire>
    <fire><direction type="absolute">170</direction><speed>1</speed>
      <bullet><action><wait>5</wait><vanish/></action></bullet></fire>
    <wait>200</wait>
  </action>
</bulletml>"""

  let field (xml: string) =
    Playfield.Create (DeterministicField.env (DeterministicField.stream ())) (Bulletml.readXmlString xml)

  let runFor (f: Playfield) (n: int) = for _ in 1 .. n do f.Tick()

  [<Test>]
  member _.``走らせると、走った字に弾コマ が付く``() =
    let f = field DeterministicField.Xml
    f.Focus.SpanTotal |> should equal 0
    runFor f 60
    f.Focus.SpanTotal |> should be (greaterThan 0)
    f.Focus.SpanCount |> should be (greaterThan 0)

  [<Test>]
  member _.``同じ行は 1 つ に畳む``() =
    let f = field Shared
    runFor f 60
    let top = f.Focus.SpanTop 50
    top.Length |> should be (greaterThan 0)
    let ids = top |> Array.map (fun (struct (i, _)) -> i)
    // **同じ添字が 2 度 出ない。** 走る木では 2 つ でも、字の上では 1 行
    Array.distinct ids |> Array.length |> should equal ids.Length

  [<Test>]
  member _.``畳んだ数は、畳む前の合計と変わらない``() =
    let f = field Shared
    runFor f 60
    // **落ちるのは「添字が決まらない行」だけ。** それ以外 は合計が保たれる
    let placed = f.Focus.SpanTop 100000 |> Array.sumBy (fun (struct (_, v)) -> v)
    placed |> should be (greaterThan 0)
    // **Shared には決まらない行が無い。** 落ちる行が在る本は下の門で見る
    placed |> should equal f.Focus.SpanTotal

  [<Test>]
  member _.``多い順に返る``() =
    let f = field Shared
    runFor f 90
    let top = f.Focus.SpanTop 50 |> Array.map (fun (struct (_, v)) -> v)
    top |> Array.pairwise |> Array.forall (fun (a, b) -> a >= b) |> should equal true

  [<Test>]
  member _.``出すのは決まった行だけ``() =
    let f = field Ambiguous
    runFor f 60
    f.Focus.SpanTop 100000
    |> Array.forall (fun (struct (i, _)) -> i >= 0)
    |> should equal true

  /// **材料が在ることを、門が自分で数える。** 決まらない行が 1 つ も無い本を
  /// 渡すと、上の門は当てる先を失って緑のまま通る ——
  /// **落ちた数が 1 以上** であることを見て、それを防ぐ
  [<Test>]
  member _.``決まらない行は、数には居るが出さない``() =
    let f = field Ambiguous
    runFor f 60
    let placed = f.Focus.SpanTop 100000 |> Array.sumBy (fun (struct (_, v)) -> v)
    placed |> should be (lessThan f.Focus.SpanTotal)

  [<Test>]
  member _.``n を超えて返さない``() =
    let f = field Shared
    runFor f 60
    (f.Focus.SpanTop 2).Length |> should be (lessThanOrEqualTo 2)
    (f.Focus.SpanTop 0).Length |> should equal 0
    (f.Focus.SpanTop -1).Length |> should equal 0

  [<Test>]
  member _.``数えるのを切ると 0 のまま``() =
    let f = field DeterministicField.Xml
    f.Focus.SetCountSpans false
    runFor f 60
    f.Focus.SpanTotal |> should equal 0
    f.Focus.SpanCount |> should equal 0

  /// **切ったあとも入れ直せる**（v4.0.2）。印のチェックは何度でも押せるので、
  /// 片道 の切り替えだと 2 度 目 から効かない
  [<Test>]
  member _.``切ってから入れ直すと、また数え始める``() =
    let f = field DeterministicField.Xml
    f.Focus.SetCountSpans false
    runFor f 60
    f.Focus.SpanTotal |> should equal 0
    f.Focus.SetCountSpans true
    runFor f 60
    f.Focus.SpanTotal |> should be (greaterThan 0)

  /// **入れ直したら数え直す**（v4.0.2）。捨てないと、切っているあいだの穴が
  /// 空いた数を「その行に居た弾コマ の割合」と呼ぶことになる ——
  /// 穴の大きさを決めるのは人（いつ入れ直したか）なので、
  /// **同じ弾幕・同じ種でも出る数が違ってしまう**
  [<Test>]
  member _.``捨てると、それまでの数は残らない``() =
    let whole = field DeterministicField.Xml
    runFor whole 120
    let a = field DeterministicField.Xml
    runFor a 60
    a.Focus.ResetSpans()
    a.Focus.SpanTotal |> should equal 0
    a.Focus.SpanCount |> should equal 0
    runFor a 60
    // 捨てたあとも数え続ける（切ったわけではない）
    a.Focus.SpanTotal |> should be (greaterThan 0)
    // **前の 60 コマ ぶんは足されていない。** 同じ走りを通しで数えたほうが多い
    a.Focus.SpanTotal |> should be (lessThan whole.Focus.SpanTotal)