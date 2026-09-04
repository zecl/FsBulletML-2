namespace FsBulletML2
// **nowarn "44" は外した。** このファイルはもう旧 API を持っていない。
// 外しておくと、うっかり旧経路を足したときに FS0044 が出る（門になる）。

open System.Globalization

/// 式の値を出すところ。
///
/// **名前が中身と合っていない。** もとは旧 API のシム（`IBulletmlObject` /
/// `BulletmlTask`、走らせる木に mutable を埋めた `Processable*` の一族）が
/// ここに同居していて、それが名前の由来だった。旧面を落としたので、
/// いま残っているのは式の評価 2 本 だけ。
///
/// **置き場所を決め直すのは別の手**（`BulletMLManager` を `Manager.fs` へ
/// 出したのと同じ形の判断）。`getValue` はホットパスで `Step.fs` が呼ぶので、
/// 動かすときは確保を測ってから。
[<AutoOpen>]
module Processable =

  /// 式の値。走行中はここを通る。
  ///
  /// 木は Expr.NumExpr が読んだ時点で組んであるので、ここは評価するだけ。
  /// **乱数は式の中身によらず 1 回 だけ引く**（$rand が何個 あっても、
  /// 1 個 も無くても 1 回）。引く回数は乱数の並びを進めるので、
  /// 下の getValueByXPath と揃っていなければ全弾幕の軌跡がずれる
  let getValue (env: Domain.Env) (e: Expr.NumExpr) =
    Expr.NumExpr.eval env.Rand env.Rank e

  /// 旧実装。文字列を毎回 XPath で評価する。
  ///
  /// **走行はもうここを通らない。** 残してあるのは ExprTests が
  /// 「木が同じ値を返すか」を突き合わせる相手として要るから。消すと、
  /// 木が正しいことを確かめる基準が無くなる。
  ///
  /// この実装には穴が 2 つ ある（どちらも ExprTests が名指しで固定している）。
  ///   - $rand / $rank が 1e-4 未満だと ToString が "1E-07" を吐き、
  ///     xpathNumber の空白入れがそれを割って XPathException になる
  ///   - 読めない式で例外になる（木のほうは NaN）
  let getValueByXPath (env: Domain.Env) (s: string) =
    let rand = env.Rand ()
    let rank = env.Rank
    let s = s.Replace("$rand", rand.ToString(CultureInfo.InvariantCulture))
             .Replace("$rank", rank.ToString(CultureInfo.InvariantCulture))
    // 置き換え残りの $N を 0 に潰す。\d が \$d* と書かれていて、
    // $ だけが 0 になり数字が残っていた（$1 が "01" = 1 になる）
    let s = System.Text.RegularExpressions.Regex.Replace(s, "\$\d*", "0")
    TryParse.eval s
