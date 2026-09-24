namespace FsBulletML2
// nowarn "44" は外した。 このファイルはもう旧 API を持っていない。
// 外しておくと、うっかり旧経路を足したときに FS0044 が出る（門になる）。

/// 式の値を出すところ。compile 順（Domain のあと、BulletmlRead の前）を動かすと inline が変わりうる。
[<AutoOpen>]
module Eval =

    /// 式の値。乱数は式の中身によらず 1 回 だけ引く。回数を変えると軌跡がずれる。
    let getValue (env: Domain.Env) (e: Expr.NumExpr) = Expr.NumExpr.eval env.Rand env.Rank e
