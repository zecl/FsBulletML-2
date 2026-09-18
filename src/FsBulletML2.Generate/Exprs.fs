/// 軸 -> 式 の字。純粋関数 だけ。
///
/// 数 を直書き せず `$rank` を含む 式 を出す。同梱 で `$rank` が掛かる 先 と向き（実測）:
///
///     speed 増える 188 対 18 / wait 減る 19 対 201 / repeat 増える 137 対 25
///     sequence 減る 8 対 20 / changeDirectionAim 減る 1 対 8
module FsBulletML2.Generate.Exprs

open System
open FsBulletML2
open FsBulletML2.Generate

/// Score の生値 を段 に落とす。丸め方 を 1 か所 に持つ
let step (v: float) = int (round v)

/// 式 の字 を、その `$rank` で評価 する。`$rand` は 0.5 に固定（上界 を測る のが用途）
let evalAt (rank: float) (expr: string) : float =
  let e = Expr.NumExpr.ofString expr
  float (Expr.evalWithValues 0.5f (float32 rank) e.Ast)

/// 腕 の数。最小 は 1 本
let armsExpr (d: PatternSpec) =
  sprintf "%d + %d * $rank" (1 + step d.Symmetry * 2) (2 + step d.Symmetry * 3)

/// 腕 の角度 の刻み。`armsExpr` から 引く —— 2 か所 で別 に計算 する と渦 が閉じない
let armStep (d: PatternSpec) =
  let jitter =
    match step d.Jitter with
    | 0 -> ""
    | 1 -> " + $rand * 4 - 2"
    | _ -> " + $rand * 20 - 10"
  sprintf "360 / (%s)%s" (armsExpr d) jitter

/// 自機 の周り に散らす 角度。`Aimed` の腕 は `armStep` を使えない ——
/// `aim` は前 の弾 を見ない ので、毎回 同じ N を渡す と n 発 が同じ 方向 に重なる
let aimSpread (d: PatternSpec) =
  let w = 20 + step d.Symmetry * 25
  sprintf "$rand * %d - %d - $rank * %d" (w * 2) w (w / 3)

let speedExpr (d: PatternSpec) =
  sprintf "%.1f + $rank * %.1f" (1.0 + d.Speed * 0.5) (0.5 + d.Speed * 0.5)

/// 間隔。`$rank = 1.0` でも 正 に保つ —— 負 の `wait` は 上界 の計算（÷ 間隔）ごと 壊す。
///
/// `i` は波 の中 の何 回目 か。`WaitScale` は `Bound.fit` が入れる 係数（既定 1.0）
let waitExpr (d: PatternSpec) (i: int) =
  let slow = 14 - step d.Density * 3
  let kinds = 1 + step d.Rhythm
  let shift = (i % kinds) * 3
  let v = max 2 (int (float ((slow + shift - kinds) * (1 + step d.Cascade)) * d.WaitScale))
  sprintf "%d - %d * $rank" v (v / 2)

/// 波 の回数。`$rank = 1.0` で `MAX_REPEAT` を越えない ように 頭 を押さえる
let wavesExpr (d: PatternSpec) =
  let want = 60 + step d.Density * 40
  let grow = 40 + step d.Density * 40
  let n = min want Consts.MAX_REPEAT
  let g = min grow (Consts.MAX_REPEAT - n)
  sprintf "%d + %d * $rank" n g

/// 中間 の `repeat` の回数。段 が深い ほど 1 段 の回数 を減らす
let midExpr (d: PatternSpec) =
  let n = max 2 (12 - step d.Depth * 3)
  sprintf "%d + %d * $rank" n (n / 2)

/// 波 の合間 の「間」。60 コマ 以上（同梱 61% が持つ）
let pauseExpr (d: PatternSpec) =
  let hold = 150 - step d.Rhythm * 25
  sprintf "%d - %d * $rank" hold (hold / 3)

/// 1 発 が増える 総量 の予算。`lv` を引く だけ では 足りない ——
/// 15 / 12 / 9 で 1 発 が 1,620 発 になり 同時 21,517 発 だった。
///
/// 3 段 なら 各 3 発（3^3 = 27）、1 段 なら 27 発
let [<Literal>] BURST_BUDGET = 27.0

/// `n` と `g` を別々 に頭打ち する と、`$rank = 1.0` の実効 が `n + g` になって
/// 予算 を超える。
///
/// `Math.Pow` の誤差 も足す —— `64 ** (1/3)` が 3.9999… で `floor` が 3 を返し、
/// 予算 64 のつもり が 27 で動いて いた
let scatterExpr (d: PatternSpec) (lv: int) =
  let total = max 1 (step d.Cascade)
  let perLevel = int (floor (Math.Pow(BURST_BUDGET, 1.0 / float total) + 1e-9))
  let n = max 2 (min perLevel (3 + step d.BulletKinds * 4 - lv * 2))
  let g = max 1 (min (max 1 (perLevel - n)) (3 + step d.BulletKinds * 3 - lv * 2))
  sprintf "%d + %d * $rank" n g

let scatterStep (d: PatternSpec) (lv: int) = sprintf "360 / (%s)" (scatterExpr d lv)

let subSpeedExpr (d: PatternSpec) (lv: int) =
  let f = 0.9 - float lv * 0.15
  sprintf "%.1f + $rank * %.1f" (f + d.Speed * 0.3) (0.8 + d.Speed * 0.3)

let levelName (lv: int) = if lv = 0 then "core" else sprintf "core%d" lv

let layerBullet (lv: int) = sprintf "layer%d" lv
