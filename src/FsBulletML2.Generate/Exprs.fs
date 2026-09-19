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

/// 腕 の数。最小 は 1 本。
///
/// 名指し（「3way」）が在れば そちら を 定数 で置く —— `$rank` を掛けない。
/// 3way は どの難度 でも 3 本 で、増えたら 3way ではない。
///
/// 刻み（`armStep` / `spinExpr` / `curtainStep`）は どれ も ここ から 引く ので、
/// 本数 を名指し する と 角度 も 一緒 に追随 する
let armsExpr (d: PatternSpec) =
  if d.Ways > 0 then string d.Ways
  else sprintf "%d + %d * $rank" (1 + step d.Symmetry * 2) (2 + step d.Symmetry * 3)

/// 撃つ 向き の基準 角。面 は 縦 で、180 度 が 自機 の方向。
///
/// `Around` は ここ を通らない —— 呼ぶ側 が `facingGiven` で 分ける
let facingBase (d: PatternSpec) =
  match d.Facing with
  | Forward -> 180
  | Backward -> 0
  | Sideways -> 90
  | Around -> 0

/// 向き が名指し されて いるか。名指し が無ければ、型 が決める 向き
/// （`Spiral` は回る / `Aimed` は狙う）を 上書き しない
let facingGiven (d: PatternSpec) = d.Facing <> Around

/// 名指し された 向き の字。
///
/// 横 は 右 の 90 度 から 始める。左 へ 分けない のは、BulletML の式 に
/// 比較 が無い から —— 腕 が 2 本 以上 あれば `armStep` が 全周 に散らす ので、
/// 起点 が右 でも 左 にも 出る
let facingExpr (d: PatternSpec) = string (facingBase d)

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

/// 波 ごと の回転。`Spiral` だけ が持つ。
///
/// `sequence` は波 を跨いで 累積 する ので、頭 の 1 発 に刻み を渡せば
/// リング が毎波 少しずつ 回る —— これ が無い と 同じ 向き の リング が
/// 重なる だけ で、渦 に見えない（実機 で踏んだ）。
///
/// 腕 の刻み の 1/4。4 波 で 腕 1 本 ぶん 回る
let spinExpr (d: PatternSpec) =
  sprintf "360 / ((%s) * 4)" (armsExpr d)

/// 幕 の横幅。下向き（180 度）を中心 に 左右 へ何度 ずつ 開く か。
///
/// 面 は 480x640 で、敵 は (240, 80)、自機 は (240, 600)。
/// 横 の端 まで 届く のに要る のは `atan(240 / 520)` ＝ 25 度 ほど なので、
/// それ を少し 越える ところ で 止める ——
/// 90 度 まで 開いた とき、端 の弾 が真横 へ出て すぐ 画面 から 消え、
/// 幕 でなく 扇 に見えた（実機 で踏んだ）
let curtainSpan (d: PatternSpec) = 20 + step d.Symmetry * 10

/// 幕 の頭 の向き。帯 の左端。
///
/// `absolute` で置く のが要 —— 毎波 ここ へ戻る ので、後ろ の `sequence` が
/// 累積 しても 帯 が回り出さない
/// 向き が名指し されて いれば そちら を中心 に。既定 は 自機 の方向（180 度）
let curtainHead (d: PatternSpec) =
  let center = if facingGiven d then facingBase d else 180
  sprintf "%d - %d" center (curtainSpan d)

/// 幕 の 1 発 の刻み。帯 を腕 の数 で割った 等間隔。
///
/// `$rand` で散らして いた とき、雨 にしか 見えなかった（実機 で踏んだ）——
/// 幕 は「同じ コマ に出た 弾 が 1 本 の弧 を作る」形 なので、
/// 角度 が揃って いない と 線 にならない。
///
/// `sequence` は直前 の fire の向き から の差分（`Step.fs`）。
/// 頭 が毎波 `absolute` で左端 に戻る ので、掃く のは 帯 の中 だけ
let curtainStep (d: PatternSpec) =
  let jitter =
    match step d.Jitter with
    | 0 -> ""
    | 1 -> " + $rand * 2 - 1"
    | _ -> " + $rand * 6 - 3"
  sprintf "%d / (%s)%s" (curtainSpan d * 2) (armsExpr d) jitter

let speedExpr (d: PatternSpec) =
  sprintf "%.1f + $rank * %.1f" (1.0 + d.Speed * 0.5) (0.5 + d.Speed * 0.5)

/// レーザー が伸びる 先 の速さ。素 の 3 倍 —— 2 倍 では 速い 弾 と
/// 見分け が つかず、`Speed` 軸 を上げた だけ に見えた
let laserSpeed (d: PatternSpec) =
  sprintf "%.1f + $rank * %.1f" (3.0 + d.Speed * 1.5) (1.5 + d.Speed * 1.5)

/// ミサイル の加速。下（自機 の方向）へ 押す。
///
/// `accel` は 面 の縦横 で効く ので、向き を変えた あと でも 同じ 向き に押す ——
/// 曲がり ながら 加速 する のが ミサイル の 見え方
let missileAccel (d: PatternSpec) = sprintf "%.1f + $rank * 0.6" (0.4 + d.Speed * 0.2)

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
