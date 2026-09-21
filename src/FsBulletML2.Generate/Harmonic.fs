namespace FsBulletML2.Generate

open System
open FsBulletML2
open FsBulletML2.Dsl
open FsBulletML2.Generate.Consts

/// 輪郭 の形。速さ を角 で変調 する 式 だけ が違う。
///
/// `Petal` は 山 も 谷 も 丸い。`Star` は 逆数 余弦 で 山 が尖り 谷 が抉れる。
/// `Heart` は k を 輪郭 に使わず、1 周 に 1 つ の くびれ。
///
/// 薔薇 は 落とした。中心 から 見て どの 角 にも 半径 が 1 つ の 輪郭（`r = f(θ)`）しか
/// 描けない ので、渦 を巻いて 花びら が 重なる 薔薇模様 は 式 を どう 変えて も 出ない ——
/// `|cos(kθ/2)|` は 花 と 見分け が つかず、2 乗 は `(1 + cos kθ)/2` で 花 そのもの、
/// 全角 の 2k 枚 は 薔薇 でなく 菊 の 座 に なった
type Figure =
  | Petal
  | Star
  | Heart

[<RequireQualifiedAccess>]
module Figure =

  /// 知らない 字 は `Petal`。`figure` を書かない 呼び手 は 今 の花 の まま。
  ///
  /// `cardioid` は `heart` の 旧名 として 受ける —— 素 の r = 1 - cos θ は
  /// 卵 に しか 見えなかった ので 式 を替えた が、字 で名指し して いた 側 を落とさない
  let ofString (s: string) =
    match s with
    | "star" -> Star
    | "heart" | "cardioid" -> Heart
    | _ -> Petal

  let toString (f: Figure) =
    match f with
    | Petal -> "petal"
    | Star -> "star"
    | Heart -> "heart"

/// 花 の軸 の生値。`HarmonicSpec.create` に渡す 途中 の形 で、clamp を通って いない。
///
/// 放射 の速さ を角 で変調 する —— speed_i = r0 + A sin(k θ_i + φ)。
/// 同じ コマ に出た 弾 が k 枚 の花弁 の輪郭 を描いて 広がる
type HarmonicAxes =
  { /// 輪郭 の式。`Petal` が既定
    Figure: Figure
    /// 花弁 の数 k。3 / 5 / 7 / 8 の外 は 5 に倒す
    Folds: int
    /// r0。`PatternSpec` と同じ 4 段
    Speed: float
    /// 0..2。0 は真円（花 に見えない）
    Amplitude: float
    /// ラジアン。0..2π に畳む
    Phase: float
    /// 0..2。0 は回らない
    Spin: float
    /// 1 波 の弾 の数。8..160、0 は 花弁 x 24 を `Blooms` で割った 数（上限 160、下限 16）。
    /// 24 発 では 広がる と点 が離れ、同じ 向き の点 が線 に並んで 花 でなく 放射 の線 に見えた
    Arms: int
    /// 何 か所 で咲かせる か。1..8。
    ///
    /// ここ が決める のは 弾数 の割り当て だけ で、撒く のは `FsBulletML2.Scatter.apply` ——
    /// あちら は Core しか 見ない ので 花 に限らず 使える。
    /// 2 以上 に した まま 撒かない と、腕 が割られた だけ の まばらな 1 輪 が出る
    Blooms: int
    /// `wait` に効く。4 段
    Density: float
    Vanishing: bool
    /// 第 2 の波 の 回数。0 は 足さない。`Folds` の 倍数 なら 対称 が 残って 畳めるが、
    /// 互いに素 だと 畳めず 字 が 4 倍 に なる
    Folds2: int
    /// 深さ の うち 第 2 の波 が 取る 割合 0..1。足す と 山 が 天井 に、谷 が 床 に クリップ する
    /// ので 分け合う。0 は 第 2 の波 が 無い のと 同じ（`Folds2` も 0 に 倒す）
    Amplitude2: float
    /// 同じ 波 で撃つ 輪郭 の 枚数。0 は 未指定 で 1 と同じ。上限 3
    Layers: int
    /// 層 の 位相 ずれ。輪郭 の 1 周期 に対する 比 0..1
    LayerPhase: float
    /// 上 の 4 本 を 表 から 一括 で引く 番号。0 は 引かない
    Seed: int }

/// 花 の仕様。`private` なので `HarmonicSpec.create` を通らず に作れない
type HarmonicSpec =
  private
    { Axes_: HarmonicAxes
      WaitScale_: float }

  member this.Figure = this.Axes_.Figure
  member this.Folds = this.Axes_.Folds
  member this.Speed = this.Axes_.Speed
  member this.Amplitude = this.Axes_.Amplitude
  member this.Phase = this.Axes_.Phase
  member this.Spin = this.Axes_.Spin
  member this.Arms = this.Axes_.Arms
  member this.Blooms = this.Axes_.Blooms
  member this.Density = this.Axes_.Density
  member this.Vanishing = this.Axes_.Vanishing
  member this.Folds2 = this.Axes_.Folds2
  member this.Amplitude2 = this.Axes_.Amplitude2
  member this.Layers = this.Axes_.Layers
  member this.LayerPhase = this.Axes_.LayerPhase
  member this.Seed = this.Axes_.Seed
  member this.WaitScale = this.WaitScale_

[<RequireQualifiedAccess>]
module HarmonicSpec =

  let private onScale (steps: int) (v: float) = max 0.0 (min (float steps - 1.0) v)

  /// 整数比 の 相手。3:5 / 5:8 / 8:13 は フィボナッチ の隣 で 比 が 黄金比 1.618 の まわり。
  /// 7 は フィボナッチ の外 なので 11 —— 11/7 = 1.571 で、12/7 = 1.714 より 近い
  let private GOLDEN_PARTNER = dict [ 3, 5; 5, 8; 7, 11; 8, 13 ]

  /// 比 と 層 の 組。比 の 0 と 1 は 倍音 で 対称 が 残り、2 は 互いに素 で 畳めない。
  ///
  /// 比 3 x 層 3 を 独立 に 振らない —— `Arms` を 層 で割る ので `Folds2` が 大きい と
  /// 1 周期 の 点 が 足りなく なり、Folds 8 で (3k, 3 枚) が Arms / Folds2 = 2.21、
  /// (2k, 3 枚) が 3.31、(3k, 2 枚) が 3.33 に 落ちた。残る 6 組 は 4 つ の Folds 全部 で 4 以上
  let private RATIO_LAYERS =
    [ 0, 1, 0.0
      0, 2, 0.5
      1, 1, 0.0
      2, 1, 0.0
      2, 2, 0.5
      2, 3, 1.0 / 3.0 ]

  /// 種 が引く 18 通り。比 と 層 の 6 組 x 深さ 3
  let private SEED_TABLE =
    [| for ratio, layers, phase in RATIO_LAYERS do
         for depth in [ 0.25; 0.4; 0.6 ] -> ratio, depth, layers, phase |]

  let zero =
    { Figure = Petal
      Folds = 5
      Speed = 0.0
      Amplitude = 0.0
      Phase = 0.0
      Spin = 0.0
      Arms = 0
      Blooms = 1
      Density = 0.0
      Vanishing = false
      Folds2 = 0
      Amplitude2 = 0.0
      Layers = 0
      LayerPhase = 0.0
      Seed = 0 }

  let create (f: HarmonicAxes -> HarmonicAxes) : HarmonicSpec =
    let a = f zero
    let twoPi = 2.0 * Math.PI
    let folds =
      match a.Folds with
      | 3 | 5 | 7 | 8 -> a.Folds
      | _ -> 5
    // 花 を 2 つ 以上 咲かせて も 同時 に居られる 数 は変わらない。
    // 名指し が無い とき は 1 輪 ぶん を 数 で割る —— 割らない と `fit` が `wait` を伸ばし、
    // 輪 と輪 の間 が 3 秒 空いて「まばらな花」になる
    let blooms = max 1 (min 8 a.Blooms)
    // 種 は 名指し の 無い 欄 だけ を埋める。`Arms = 0` が 既定 を引く のと 同じ 決め方
    let folds2, amp2, layers0, layerPhase =
      if a.Seed <= 0 then a.Folds2, a.Amplitude2, a.Layers, a.LayerPhase
      else
        let ratio, depth, l, p = SEED_TABLE.[(a.Seed - 1) % SEED_TABLE.Length]
        let k2 =
          match ratio with
          | 0 -> 2 * folds
          | 1 -> 3 * folds
          | _ -> GOLDEN_PARTNER.[folds]
        (if a.Folds2 <> 0 then a.Folds2 else k2),
        (if a.Amplitude2 > 0.0 then a.Amplitude2 else depth),
        (if a.Layers > 0 then a.Layers else l),
        (if a.LayerPhase > 0.0 then a.LayerPhase else p)
    let layers = max 1 (min 3 layers0)
    let amp2 = max 0.0 (min 1.0 amp2)
    // 取り分 0 は 第 2 の波 が 無い のと 同じ。ここ で 0 に倒さない と symmetryOf が
    // gcd を取って 畳み が 外れる —— Folds2 8・Amplitude2 0 は 見た目 が 5 回 対称 の まま
    // gcd(5, 8) = 1 で 畳めず、字 が 4 倍 に なる
    let folds2 = if amp2 <= 0.0 then 0 else max 0 folds2
    { Axes_ =
        { a with
            Folds = folds
            Speed = onScale 4 a.Speed
            Amplitude = max 0.0 (min 2.0 a.Amplitude)
            Phase = (let r = a.Phase % twoPi in if r < 0.0 then r + twoPi else r)
            Spin = max 0.0 (min 2.0 a.Spin)
            // 名指し して も 咲かせる 数 で頭打ち。1 波 は まるごと 同時 に居る ので、
            // ここ を割らない と 160 x 8 が 1 コマ に出て、`fit` は `wait` しか 伸ばせず 直せない。
            // 層 は 同じ 角 に重ねる ので 層 でも 割る —— 割らない と 層 を足した だけ で 3 倍
            Arms = (let per = blooms * layers
                    let cap = max 8 (160 / per)
                    if a.Arms = 0 then min cap (max 16 (folds * 24 / per)) else max 8 (min cap a.Arms))
            Blooms = blooms
            Density = onScale 4 a.Density
            Folds2 = folds2
            Amplitude2 = amp2
            Layers = layers
            LayerPhase = max 0.0 (min 1.0 layerPhase)
            Seed = max 0 a.Seed }
      WaitScale_ = 1.0 }

  /// `Harmonic.fit` だけ が呼ぶ
  let internal withWaitScale (scale: float) (s: HarmonicSpec) = { s with WaitScale_ = scale }

module Harmonic =

  let private step (v: float) = int (round v)

  let private r0 (h: HarmonicSpec) = 1.0 + h.Speed * 0.5

  /// 振幅 は r0 - 0.5 で止める。越える と 谷 の弾 が負 の速さ で逆走 し、0.3 では 敵 の近く に居座った
  let private amp (h: HarmonicSpec) = min (h.Amplitude * 0.8) (r0 h - 0.5)

  let rec private gcd a b = if b = 0 then a else gcd b (a % b)

  /// 輪郭 が 1 周 に 何回 繰り返す か。畳み の 回数 と、層 の 位相 の 周期 に なる。
  /// 第 2 の波 が入る と 対称 は 2 つ の 回数 の 最大公約数 に落ちる
  let private symmetryOf (h: HarmonicSpec) =
    match h.Figure with
    | Heart -> 1
    | Star -> h.Folds
    | Petal -> if h.Folds2 = 0 then h.Folds else gcd h.Folds h.Folds2

  /// 層 j の 位相 の ずれ。割る のは `Folds` でなく `symmetryOf` ——
  /// ハート は Folds を 輪郭 に使わず 周期 が 2π なので、Folds で割る と
  /// 半周期 の つもり が 1/10 しか ずれない
  let private layerShift (h: HarmonicSpec) (j: int) =
    if j = 0 then 0.0 else float j * h.LayerPhase * 2.0 * Math.PI / float (symmetryOf h)

  let private rankFactor = 0.3

  /// 正 星型 多角形 の 内/外。0.382 は 正 五芒星（辺 を 伸ばす と 隣 の 頂点 に当たる 比）
  let private STAR_INNER = 0.382

  /// 外 の頂点 と 内 の頂点 を 直線 で結んだ 輪郭。t = 0 が 外 の頂点。
  ///
  /// 逆数 余弦（r = sqrt(1-α²)/(1 + α cos kθ)）は 使わない —— α を どこ に振って も
  /// ★ に ならなかった。0.85 では 細い トゲ 5 本 と 中心 の ダマ（外/内 12.3 の 閃光）、
  /// 0.45 では 外/内 が ★ と同じ 2.6 でも 山 が 丸い 5 弁 の花。
  /// 滑らかな 曲線 から 直線 の 辺 は 出ない
  let private starAt (k: float) (t: float) =
    let b = Math.PI / k
    let u = ((t % (2.0 * b)) + 2.0 * b) % (2.0 * b)
    let a = if u <= b then u else 2.0 * b - u
    STAR_INNER * sin b / (sin a + STAR_INNER * sin (b - a))

  /// `starAt` の 1 周 平均。k で変わる ので 先 に 4 通り 持つ（`Folds` は 3 / 5 / 7 / 8）
  let private starMean =
    let n = 720
    [ 3; 5; 7; 8 ]
    |> List.map (fun k ->
        k, (Seq.init n (fun i -> starAt (float k) (float i * 2.0 * Math.PI / float n)) |> Seq.average))
    |> Map.ofList

  /// ハート の くびれ。1.0 で 谷 が 床 に着き、0 は 真円。
  /// 割る 1.5 は 星 と同じ 都合 —— 面 が返す 1.2 では 0.6 に しか ならない
  let private betaOf (h: HarmonicSpec) = min 1.0 (h.Amplitude / 1.5)

  /// ハート の 素 の輪郭。谷 が 0、山 が 4。t = 0 が くびれ（真上）。
  ///
  /// 素 の カージオイド（r = 1 - cos θ）は 使わない —— 尖点 は 在る が 二つ山 に ならず、
  /// 96 発 で描く と 上 が へこんだ 卵 に見えた（実測。床 から 立ち上げて 外/内 を 9 倍 に
  /// しても 形 は 変わらなかった ので、深さ ではなく 式 の問題）
  let private heartAt (t: float) =
    let p = Math.PI / 2.0 - t
    let s = sin p
    max 0.0 (2.0 - 2.0 * s + s * sqrt (abs (cos p)) / (s + 1.4))

  /// `heartAt` の 1 周 平均。割って 平均 を 1 に戻す ——
  /// 割らない と ハート だけ が 他 の 3 札 より 2 倍 速い
  let private HEART_MEAN =
    let n = 720
    Seq.init n (fun i -> heartAt (float i * 2.0 * Math.PI / float n)) |> Seq.average

  /// 速さ の床。割る と 敵 の近く に居座る。星 の内 の頂点 は ここ に当たる ——
  /// r0 = 1・α = 0.85 で 素 の谷 は 0.285 で、床 で止めて も 外/内 は 11.7 倍 残る
  let private SPEED_LO = 0.3

  /// 速さ の天井。`$rank = 1` で `rankFactor` 倍 されて `MAX_SPEED` に当たる 手前。
  /// `Speed` の高い 札 で 星 の山 が ここ に当たり、尖り が平ら に潰れる ——
  /// 潰れた こと は 弾数 にも 形 の門 にも 出ない ので、速さ の最大 を見る 門 で止める
  let private speedHi = MAX_SPEED / (1.0 + rankFactor)

  /// 角 t（ラジアン）での 速さ（`$rank = 0`）。振幅 0 は どの 札 でも 真円。
  ///
  /// `Star` は 相乗 でなく 算術 の平均 が r0 —— 1/(1 + α cos) の 平均 は 1/sqrt(1-α²) なので、
  /// sqrt(1-α²) を掛ける と 平均 が r0 に戻る。これ で 山 と谷 の比 が `Speed` から 外れる。
  /// 素 の `r0 / (1 + α cos)` だと 山 が 6.67 r0 まで 伸びて、`Speed` が高い 札 は 天井 で 全部 潰れた
  let private speedAt (h: HarmonicSpec) (j: int) (t0: float) =
    let t = t0 + layerShift h j
    let r = r0 h
    let a = amp h / r
    let k = float h.Folds
    let raw =
      match h.Figure with
      // 第 2 の波 が無い 花 は いま の 1 行 を そのまま 通る。`+ 0.0` で 済ませない ——
      // x + 0.0 は x = -0.0 の とき だけ 答え が変わる 上 に、字 で読めなく なる
      | Petal when h.Folds2 = 0 -> r * (1.0 + a * sin (k * t + h.Phase))
      | Petal ->
        // 深さ は 2 つ で 分け合う。足す と amp が 守って いる 不変（谷 >= 0.5・山 <= 天井）が
        // 壊れる —— Speed 3 / Amplitude 2 / Amplitude2 1 で 山 5.70・谷 -0.70 の 両方 が クリップ した
        let a1 = a * (1.0 - h.Amplitude2)
        let a2 = a * h.Amplitude2
        let k2 = float h.Folds2
        r * (1.0 + a1 * sin (k * t + h.Phase) + a2 * sin (k2 * t + h.Phase))
      | Star ->
        // 振幅 は 星 らしさ。0 は 真円、1 で 正 星型 多角形 —— 素 の 多角形 は
        // 内 と外 が 同じ とき でも 2k 角形 で、真円 に ならない
        let w = min 1.0 (h.Amplitude / 1.5)
        let m = defaultArg (Map.tryFind h.Folds starMean) 1.0
        r * ((1.0 - w) + w * starAt k (t + h.Phase) / m)
      | Heart ->
        let b = betaOf h
        SPEED_LO + (r - SPEED_LO) * ((1.0 - b) + b * heartAt (t + h.Phase) / HEART_MEAN)
    max SPEED_LO (min speedHi raw)

  /// 頭 から i 本 目、層 j の 角（度）と速さ（`$rank = 0`）
  let private armOf (h: HarmonicSpec) (j: int) (i: int) =
    let theta = float i * 360.0 / float h.Arms
    theta, speedAt h j (theta * Math.PI / 180.0)

  let private speedOf (spd: float) = sprintf "%.2f + $rank * %.2f" spd (spd * rankFactor)

  /// 弾 が毎コマ 曲がる 角（度）。1 輪 の弾 が同じ 角 で曲がる と、位置 は全員 に共通 の因子 が掛かる だけ なので
  /// 花 は形 を保った まま ω/2 で回り ながら 広がる（曲がる 120 コマ で Spin 1 は 45 度、2 は 60 度）。
  /// 曲げる ほど 広がり が縮む（真っすぐ の 2 sin(ωt/2) / ωt 倍。Spin 1 の 120 コマ で 90%）ので 1 度 で止める
  ///
  /// 撃つ 向き だけ を輪 ごと に回して いた とき、花 1 輪 は向き を変えず に広がり、
  /// 隣 の輪 と少し ずれて 見える だけ で 回転 に見えなかった
  let private turnDeg (h: HarmonicSpec) = if h.Spin > 0.0 then 0.5 + 0.25 * h.Spin else 0.0

  /// 曲がる の は ここ まで。以後 は真っすぐ 飛ぶ ので 花 は回り終えた 向き の まま 広がり、面 の外 へ出る。
  /// 曲がり 続ける と 遅い 弾 は半径 v / ω の円 を描いて 出て行かない
  let [<Literal>] private TURN_TERM = 120

  /// 輪 と輪 の間 を空ける（40 / 32 / 24 / 16 コマ）。詰める と 1 輪 ずつ の輪郭 が重なって 読めない
  let private waitOf (h: HarmonicSpec) =
    let v = max 2 (int (float (40 - step h.Density * 8) * h.WaitScale))
    v, v / 3

  let private waitExpr (h: HarmonicSpec) =
    let v, g = waitOf h
    sprintf "%d - %d * $rank" v g

  let private wavesExpr (h: HarmonicSpec) =
    let n = min (20 + step h.Density * 10) MAX_REPEAT
    let g = min (10 + step h.Density * 10) (MAX_REPEAT - n)
    sprintf "%d + %d * $rank" n g

  /// 1 波。頭 の後 は `sequence` の刻み で 1 周 する。
  ///
  /// 回す とき は頭 も `sequence` にして `360 / Arms` に 輪 の間 に花 が回る 角（ω/2 x wait）を足す ——
  /// 前 の波 の最後 の弾 は 頭 から 1 刻み 手前 なので、刻み を 1 つ 足す と 頭 に戻る。
  /// 先 に出た 花 と向き が揃い、何重 の花 が 1 つ として 回る。
  /// `defAction` に θ0 を `$1` で渡す 形 は 波 を跨いで 足し込めない（`repeat` は毎回 同じ 引数 を渡す）
  /// 1/n 周 の 腕 の数。ここ が `Some m` なら 畳める。
  ///
  /// 畳める のは 輪郭 が n 回 対称 で、`Arms` が n で 割り切れる とき だけ ——
  /// ハート は 1 回 対称 で 畳めず、k = 7 は `Arms` が 160 で 割り切れない（160 / 7）。
  /// 4 つ の k の うち 3 つ（3 / 5 / 8）が 畳まる
  let private foldBy (h: HarmonicSpec) =
    let n = symmetryOf h
    if n >= 2 && h.Arms % n = 0 && h.Arms / n >= 2 then Some(h.Arms / n) else None

  /// 1 波 で撃つ (腕, 層) の組。畳む と 頭 と 最後 の 1 発 が 同じ 向き に なる ので 1 発 多い ——
  /// 頭 は 1 波 に 1 度 しか 置けず（`absolute 0` か、回す とき の 足し込み）、
  /// 残り を n 回 の `repeat` で 割る と 1/n 周 x n = 1 周 ちょうど で 頭 に戻る。
  /// 120 発 の うち 1 発 が 重なる だけ なので、字 が 5 倍 小さく なる 代金 として 払う
  let private firedShots (h: HarmonicSpec) =
    let arms =
      match foldBy h with
      | Some m -> 0 :: List.collect (fun _ -> [ 1 .. m ]) [ 1 .. symmetryOf h ]
      | None -> [ 0 .. h.Arms - 1 ]
    [ for i in arms do
        for j in 0 .. h.Layers - 1 -> i, j ]

  /// 1 波 で出る 弾 の数。畳んだ とき は `(Arms + 1) x Layers`
  let shotsPerWave (h: HarmonicSpec) = (firedShots h).Length

  let private ring (h: HarmonicSpec) : Action list =
    let gap = 360.0 / float h.Arms
    let shot (d: string) (spd: float) =
      fire { sequence d; speed (speedOf spd); refBullet "core" [] }
    // 層 は 同じ 向き に重ねる。2 枚 目 以降 は `sequence "0.00"` —— 角 を進めず 速さ だけ 変える
    let withLayers (i: int) (first: Action) =
      first :: [ for j in 1 .. h.Layers - 1 -> shot "0.00" (snd (armOf h j i)) ]
    let step i = withLayers i (shot (sprintf "%.2f" gap) (snd (armOf h 0 i)))
    let head =
      let _, spd = armOf h 0 0
      if h.Spin > 0.0 then
        let d = sprintf "%.2f + %.3f * (%s)" gap (turnDeg h / 2.0) (waitExpr h)
        withLayers 0 (shot d spd)
      else
        withLayers 0 (fire { absolute "0"; speed (speedOf spd); refBullet "core" [] })
    match foldBy h with
    | Some m ->
      // 1/n 周 だけ 書いて 回す。`sequence` の刻み は `repeat` を跨いで 足し込まれる ので、
      // n 回 で ちょうど 1 周 する。腕 m の 速さ は 対称 から 腕 0 と同じ
      [ yield! head
        repeat (string (symmetryOf h)) { yield! List.collect step [ 1 .. m ] } ]
    | None -> head @ List.collect step [ 1 .. h.Arms - 1 ]

  /// 同時 に居る 弾 の上界（`$rank = 1.0`）。
  ///
  /// 弾 ごと に速さ が違う ので、寿命 も弾 ごと に取って 足す。`Vanishing` が無ければ 画面 を抜ける まで 生きる
  let aliveBound (h: HarmonicSpec) : float =
    let v, g = waitOf h
    let wait1 = max 1.0 (float (v - g))
    let lifeOf spd =
      let fly = FIELD_SPAN / (spd * (1.0 + rankFactor))
      if h.Vanishing then min BULLET_LIFE fly else fly
    // 種 は 1 波 に `Blooms` 発 しか 出ず、咲いたら 消える ので 数 に入れない
    let shots = firedShots h
    let perWave = (shots |> List.sumBy (fun (i, j) -> lifeOf (snd (armOf h j i)))) * float h.Blooms
    max (perWave / wait1) (float (shots.Length * h.Blooms)) * Bound.SAFETY

  /// 越える なら `wait` を伸ばす。花弁 の数 と Arms は形 そのもの なので 減らさない
  /// 上限 は 引数 —— 混ぜ の相手 が居る とき は 半分 になる
  let fitTo (budget: float) (h: HarmonicSpec) : HarmonicSpec =
    let budget = max 1.0 budget
    let rec go (s: HarmonicSpec) (tries: int) =
      let need = aliveBound s / budget
      if need <= 1.0 || tries <= 0 then s
      else go (HarmonicSpec.withWaitScale (s.WaitScale * max 2.0 (ceil need)) s) (tries - 1)
    go h 12

  let fit (h: HarmonicSpec) : HarmonicSpec = fitTo (float MAX_ALIVE) h

  let generateTo (budget: float) (spec: HarmonicSpec) : BulletmlInfo =
    let h = fitTo budget spec

    createBulletmlInfo
    <| vertical "harmonic" {
         top {
           repeat (wavesExpr h) {
             yield! ring h
             wait (waitExpr h)
           }
         }

         defBullet "core" {
           doActs (
             body {
               if h.Spin > 0.0 then
                 changeDirectionSeq (sprintf "%.2f" (turnDeg h)) (string TURN_TERM)
                 // 台本 を終わらせない。面 は終えた 弾 を頭 から走らせ直す ので、ここで終わる と また 曲がり出す
                 if not h.Vanishing then
                   wait "9999"
               if h.Vanishing then
                 wait "180"
                 vanish
             })
         }
       }

  let generate (spec: HarmonicSpec) : BulletmlInfo = generateTo (float MAX_ALIVE) spec
