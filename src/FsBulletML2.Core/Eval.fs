namespace FsBulletML2
// nowarn "44" は外した。 このファイルはもう旧 API を持っていない。
// 外しておくと、うっかり旧経路を足したときに FS0044 が出る（門になる）。

/// 式の値を出すところ。
///
/// compile の順は動かさないこと（`Domain` のあと、`BulletmlRead` の前）
/// —— `getValue` はホットパスで `Step.fs` が呼ぶので、位置を変えると
/// inline の条件が変わりうる。
[<AutoOpen>]
module Eval =

  /// 式の値。走行中はここを通る。
  ///
  /// 木は `Expr.NumExpr` が読んだ時点で組んであるので、ここは評価するだけ。
  /// 乱数は式の中身によらず 1 回 だけ引く（`$rand` が何個 あっても、
  /// 1 個 も無くても 1 回）。引く回数は乱数の並びを進めるので、
  /// ここを写し違えると弾幕の軌跡が丸ごとずれる。
  let getValue (env: Domain.Env) (e: Expr.NumExpr) =
    Expr.NumExpr.eval env.Rand env.Rank e
