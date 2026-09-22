/// 同時 に居る 弾 の上界 と、越えた とき に絞る 仕組み。
module FsBulletML2.Generate.Bound

open FsBulletML2.Generate
open FsBulletML2.Generate.Consts
open FsBulletML2.Generate.Exprs

/// 式 が見て いない ぶん の余裕。大きく する と `fit` が `wait` を伸ばして 密度 が死ぬ
let [<Literal>] SAFETY = 1.3

/// 弾 が生きて いる コマ数。`wait 180` で消える のと、画面 から 出る のと、早い ほう
/// 180 固定 で見積もる と 速い 弾 の上界 が過大 になり、`fit` が `wait` を伸ばしすぎる
let private lifeOf (d: PatternSpec) : float =
  // いちばん 遅い 弾 で見る —— 段 の子 は 親 より 遅い（`subSpeedExpr`）ので、
  // 親 の速度 だけ 見る と 短命 に見積もって 上界 が破れる
  let slowest =
    [ yield evalAt 1.0 (speedExpr d)
      // `Breathe` は `changeSpeed "0.25"` で 60 コマ ほど 溜める。
      // その間 ほとんど 進まない ので、見ない と 上界 が破れる
      if d.Breathe then yield 0.25
      for lv in 0 .. step d.Cascade - 1 -> evalAt 1.0 (subSpeedExpr d lv) ]
    |> List.min
    |> max 0.25

  min BULLET_LIFE (FIELD_SPAN / slowest)

/// 1 回 の腕 で `top` が出す 弾（`$rank = 1.0`）。`Parametrized` は `arm` を 0 度 と 180 度 で 2 回 呼ぶ ので
/// 倍 になる。どちら にも 速度 の起点 の 1 発 が付く。幕 は `arm` を通らない（`Generate.arms`）
let perTurn (d: PatternSpec) : float =
  let arms = evalAt 1.0 (armsExpr d)
  if d.Parametrized && d.Kind <> Curtain then 3.0 + arms * 2.0
  elif headIsArm d then arms
  else 1.0 + arms

/// `$rank = 1.0` で `(top の発射率 × Σ段 ごと の撒く数 の積 ＋ 層 の発射率) × 寿命`
/// `repeat` の回数 は掛けない（発射 の回数 と 同じ だけ 時間 も 伸びる ので 単位時間 あたり には効かない）
let aliveBound (d: PatternSpec) : float =
  let arms = evalAt 1.0 (armsExpr d)
  let perTurn = perTurn d

  // `Depth` 段 の中間 `repeat` は、1 波 の中 で腕 と wait を mid^Depth 回 繰り返す。
  // 分子 と分母 の両方 に掛かる ので、`Pause` が無ければ 約分 されて 消える
  let reps = (evalAt 1.0 (midExpr d)) ** float (step d.Depth)
  let wait0 = max 1.0 (evalAt 1.0 (waitExpr d 0))
  let pause = if d.Pause then evalAt 1.0 (pauseExpr d) else 0.0
  let topRate = perTurn * reps / (wait0 * reps + pause)

  // 層 は `top` と別 の間隔 を使い、「間」を持たない。撒かない ので burst も掛からない
  let wait1 = max 1.0 (evalAt 1.0 (waitExpr d 1))
  let layerRate = float (step d.Layers) * arms / wait1

  let burst =
    let mutable acc = 1.0
    let mutable total = 1.0
    for lv in 0 .. step d.Cascade - 1 do
      acc <- acc * evalAt 1.0 (scatterExpr d lv)
      total <- total + acc
    total

  let steady = (topRate * burst + layerRate) * lifeOf d

  // `wait` が寿命 を超える と 発射率 で割る のが 無意味 になる。1 回 の塊 が丸ごと 同時 に生きる
  let perBurst = perTurn * burst + arms * float (step d.Layers)

  max steady perBurst * SAFETY

/// 越える なら まず `wait` を伸ばす。腕 や段 を減らす と 形 が変わる が、これ なら ゆっくり になる だけ
/// `waitExpr` は `max 2` と 整数 への丸め が在る ので、入れた 後 に もう一度 測る。
let private byWait (budget: float) (d: PatternSpec) : PatternSpec =
  let rec go (s: PatternSpec) (tries: int) =
    if tries <= 0 then s
    else
      let need = aliveBound s / budget
      if need <= 1.0 then s
      else
        let next = PatternSpec.withWaitScale (s.WaitScale * max 2.0 (ceil need)) s
        if aliveBound next >= aliveBound s then next else go next (tries - 1)

  go d 12

/// `wait` で絞りきれない なら 段 を 1 つ ずつ 落とす（1 回 の塊 は `wait` で減らない）
/// 上限 は 引数 で、呼ぶ側 が 取り分 を渡す。0 以下 は 1 に倒す（呼ぶ側 の割り算 で 0 になりうる）
let fitTo (budget: float) (d: PatternSpec) : PatternSpec =
  let budget = max 1.0 budget
  let rec go (s: PatternSpec) =
    let w = byWait budget s
    if aliveBound w <= budget || step w.Cascade = 0 then w
    else go (PatternSpec.withCascade (w.Cascade - 1.0) w)

  go d

let fit (d: PatternSpec) : PatternSpec = fitTo (float MAX_ALIVE) d
