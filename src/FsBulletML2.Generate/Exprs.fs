/// 軸 -> 式 の字。純粋関数 だけ。数 を直書き せず `$rank` を含む 式 を出す
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

/// 腕 の数。最小 は 1 本。名指し（「3way」）が在れば 定数 で置き、`$rank` を掛けない
/// 刻み は どれ も ここ から 引く ので、本数 を名指し する と 角度 も 一緒 に追随 する
let armsExpr (d: PatternSpec) =
    if d.Ways > 0 then
        string d.Ways
    else
        sprintf "%d + %d * $rank" (1 + step d.Symmetry * 2) (2 + step d.Symmetry * 3)

/// 撃つ 向き の基準 角。180 度 が 自機 の方向。`Around` は ここ を通らない（呼ぶ側 が `facingGiven` で 分ける）
let facingBase (d: PatternSpec) =
    match d.Facing with
    | Forward -> 180
    | Backward -> 0
    | Sideways -> 90
    | Around -> 0

/// 向き が名指し されて いるか。名指し が無ければ、型 が決める 向き
/// （`Spiral` は回る / `Aimed` は狙う）を 上書き しない
let facingGiven (d: PatternSpec) = d.Facing <> Around

/// 名指し された 向き の字。横 は 右 の 90 度 から 始める（BulletML の式 に 比較 が無い ので 左 へ 分けない）
let facingExpr (d: PatternSpec) = string (facingBase d)

let private jitterOf (d: PatternSpec) (small: int) (large: int) =
    match step d.Jitter with
    | 0 -> ""
    | 1 -> sprintf " + $rand * %d - %d" (small * 2) small
    | _ -> sprintf " + $rand * %d - %d" (large * 2) large

/// 腕 の角度 の刻み。`armsExpr` から 引く —— 2 か所 で別 に計算 する と渦 が閉じない
let armStep (d: PatternSpec) =
    sprintf "360 / (%s)%s" (armsExpr d) (jitterOf d 2 10)

/// 中心 ± `half` 度 の弧 を腕 の数 で割った 刻み。頭 を `中心 - half` に置けば `中心 + half` で閉じる
let private arcStep (half: int) (jitter: string) (d: PatternSpec) =
    sprintf "%d / (%s)%s" (half * 2) (armsExpr d) jitter

/// 頭 を 1 本目 に数える 型。n 本 = 頭 ＋ 腕 n - 1 本 で、頭 の後 に n 本 撃つ 型 より 1 発 少ない
let headIsArm (d: PatternSpec) =
    match d.Kind with
    | Radial
    | Aimed
    | Spread -> true
    | Spiral
    | Curtain -> false

/// 本数 を整数 に切った 字。`repeat` は times を int に切る（`Step.fs`）ので、
/// 刻み の分母 も 同じ 数 で割らない と 輪 も弧 も閉じない
let wholeArms (d: PatternSpec) =
    if d.Ways > 0 then
        string d.Ways
    else
        let a = armsExpr d
        sprintf "(%s) - (%s) %% 1" a a

/// `headIsArm` の型 が 頭 の後 に撃つ 腕 の数
let restArms (d: PatternSpec) =
    if d.Ways > 0 then
        string (d.Ways - 1)
    else
        sprintf "%s - 1" (wholeArms d)

/// 放射 の刻み。頭 ＋ n - 1 回 なので、n 本 目 が頭 に重ならない
let ringStep (d: PatternSpec) =
    sprintf "360 / (%s)%s" (wholeArms d) (jitterOf d 2 10)

/// 両端 を含めて n 本 で ± `half` 度 を割る ので 刻み は 2 half / (n - 1)
let private fanStep (half: int) (d: PatternSpec) =
    sprintf "%d / (%s)%s" (half * 2) (restArms d) (jitterOf d 2 10)

/// 扇 の頭（左端）。1 本 なら 中心 に置く。本数 が `$rank` の式 の とき は、
/// 式 に比較 が無い ので `1 % n`（1 本 で 0、2 本 以上 で 1）で分ける
let fanHead (center: string) (half: int) (d: PatternSpec) =
    if d.Ways = 1 then
        center
    elif d.Ways > 1 then
        sprintf "%s - %d" center half
    else
        sprintf "%s - %d * (1 %% (%s))" center half (wholeArms d)

/// 扇 の片側 の開き。全幅 は 60〜120 度
let spreadSpan (d: PatternSpec) = 30 + step d.Symmetry * 10

let spreadStep (d: PatternSpec) = fanStep (spreadSpan d) d

/// 狙い の扇 の片側 の開き。自機 の方位 を中心 に 全幅 20〜80 度
let aimSpan (d: PatternSpec) = 10 + step d.Symmetry * 10

let aimStep (d: PatternSpec) = fanStep (aimSpan d) d

/// 波 ごと の回転（`Spiral` だけ）。腕 の刻み の 1/4 で、4 波 で 腕 1 本 ぶん 回る
/// 無い と 同じ 向き の リング が 重なる だけ で、渦 に見えない
let spinExpr (d: PatternSpec) = sprintf "360 / ((%s) * 4)" (armsExpr d)

/// 幕 の横幅。下向き（180 度）を中心 に 左右 へ何度 ずつ 開く か
/// 開きすぎる と 端 の弾 が真横 へ出て すぐ 消え、幕 でなく 扇 に見える
let curtainSpan (d: PatternSpec) = 20 + step d.Symmetry * 10

/// 幕 の頭 の向き（帯 の左端）。名指し が無ければ 自機 の方向（180 度）を中心 に
/// `absolute` で置く のが要。毎波 ここ へ戻る ので、後ろ の `sequence` が累積 しても 帯 が回り出さない
let curtainHead (d: PatternSpec) =
    let center = if facingGiven d then facingBase d else 180
    sprintf "%d - %d" center (curtainSpan d)

/// 幕 の 1 発 の刻み。帯 を腕 の数 で割った 等間隔
/// `$rand` で散らす と 雨 に見える。同じ コマ の弾 が 1 本 の弧 を作る には 角度 が揃う 必要 が在る
let curtainStep (d: PatternSpec) =
    arcStep (curtainSpan d) (jitterOf d 1 3) d

let speedExpr (d: PatternSpec) =
    sprintf "%.1f + $rank * %.1f" (1.0 + d.Speed * 0.5) (0.5 + d.Speed * 0.5)

/// レーザー が伸びる 先 の速さ。素 の 3 倍（2 倍 では 速い 弾 と 見分け が つかない）
let laserSpeed (d: PatternSpec) =
    sprintf "%.1f + $rank * %.1f" (3.0 + d.Speed * 1.5) (1.5 + d.Speed * 1.5)

/// ミサイル の加速。下（自機 の方向）へ 押す
/// `accel` は 面 の縦横 で効く ので、向き を変えた あと でも 同じ 向き に押す
let missileAccel (d: PatternSpec) =
    sprintf "%.1f + $rank * 0.6" (0.4 + d.Speed * 0.2)

/// 間隔。`$rank = 1.0` でも 正 に保つ（負 の `wait` は 上界 の計算（÷ 間隔）ごと 壊す）
/// `i` は波 の中 の何 回目 か。`WaitScale` は `Bound.fit` が入れる 係数（既定 1.0）
let waitExpr (d: PatternSpec) (i: int) =
    let slow = 14 - step d.Density * 3
    let kinds = 1 + step d.Rhythm
    let shift = (i % kinds) * 3

    let v =
        max 2 (int (float ((slow + shift - kinds) * (1 + step d.Cascade)) * d.WaitScale))

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

/// 波 の合間 の「間」。60 コマ 以上
let pauseExpr (d: PatternSpec) =
    let hold = 150 - step d.Rhythm * 25
    sprintf "%d - %d * $rank" hold (hold / 3)

/// 1 発 が増える 総量 の予算。3 段 なら 各 3 発（3^3 = 27）、1 段 なら 27 発
/// `lv` を引く だけ では 総量 が抑えられない
[<Literal>]
let BURST_BUDGET = 27.0

/// `n` と `g` を別々 に頭打ち する と、`$rank = 1.0` の実効 `n + g` が 予算 を超える
/// `Math.Pow` の誤差 を足す。64 の 1/3 乗 が 3.9999… で `floor` が 3 を返す
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

let levelName (lv: int) =
    if lv = 0 then "core" else sprintf "core%d" lv

let layerBullet (lv: int) = sprintf "layer%d" lv
