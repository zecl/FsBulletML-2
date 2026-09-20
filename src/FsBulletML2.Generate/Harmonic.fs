namespace FsBulletML2.Generate

open System
open FsBulletML2
open FsBulletML2.Dsl
open FsBulletML2.Generate.Consts

/// 花 の軸 の生値。`HarmonicSpec.create` に渡す 途中 の形 で、clamp を通って いない。
///
/// 放射 の速さ を角 で変調 する —— speed_i = r0 + A sin(k θ_i + φ)。
/// 同じ コマ に出た 弾 が k 枚 の花弁 の輪郭 を描いて 広がる
type HarmonicAxes =
  { /// 花弁 の数 k。3 / 5 / 7 / 8 の外 は 5 に倒す
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
    Vanishing: bool }

/// 花 の仕様。`private` なので `HarmonicSpec.create` を通らず に作れない
type HarmonicSpec =
  private
    { Axes_: HarmonicAxes
      WaitScale_: float }

  member this.Folds = this.Axes_.Folds
  member this.Speed = this.Axes_.Speed
  member this.Amplitude = this.Axes_.Amplitude
  member this.Phase = this.Axes_.Phase
  member this.Spin = this.Axes_.Spin
  member this.Arms = this.Axes_.Arms
  member this.Blooms = this.Axes_.Blooms
  member this.Density = this.Axes_.Density
  member this.Vanishing = this.Axes_.Vanishing
  member this.WaitScale = this.WaitScale_

[<RequireQualifiedAccess>]
module HarmonicSpec =

  let private onScale (steps: int) (v: float) = max 0.0 (min (float steps - 1.0) v)

  let zero =
    { Folds = 5
      Speed = 0.0
      Amplitude = 0.0
      Phase = 0.0
      Spin = 0.0
      Arms = 0
      Blooms = 1
      Density = 0.0
      Vanishing = false }

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
    { Axes_ =
        { a with
            Folds = folds
            Speed = onScale 4 a.Speed
            Amplitude = max 0.0 (min 2.0 a.Amplitude)
            Phase = (let r = a.Phase % twoPi in if r < 0.0 then r + twoPi else r)
            Spin = max 0.0 (min 2.0 a.Spin)
            // 名指し して も 咲かせる 数 で頭打ち。1 波 は まるごと 同時 に居る ので、
            // ここ を割らない と 160 x 8 が 1 コマ に出て、`fit` は `wait` しか 伸ばせず 直せない
            Arms = (let cap = max 8 (160 / blooms)
                    if a.Arms = 0 then min cap (max 16 (folds * 24 / blooms)) else max 8 (min cap a.Arms))
            Blooms = blooms
            Density = onScale 4 a.Density }
      WaitScale_ = 1.0 }

  /// `Harmonic.fit` だけ が呼ぶ
  let internal withWaitScale (scale: float) (s: HarmonicSpec) = { s with WaitScale_ = scale }

module Harmonic =

  let private step (v: float) = int (round v)

  let private r0 (h: HarmonicSpec) = 1.0 + h.Speed * 0.5

  /// 振幅 は r0 - 0.5 で止める。越える と 谷 の弾 が負 の速さ で逆走 し、0.3 では 敵 の近く に居座った
  let private amp (h: HarmonicSpec) = min (h.Amplitude * 0.8) (r0 h - 0.5)

  /// 頭 から i 本 目 の角（度）と速さ（`$rank = 0`）
  let private petal (h: HarmonicSpec) (i: int) =
    let theta = float i * 360.0 / float h.Arms
    theta, r0 h + amp h * sin (float h.Folds * theta * Math.PI / 180.0 + h.Phase)

  let private rankFactor = 0.3

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
  let private ring (h: HarmonicSpec) : Action list =
    let gap = 360.0 / float h.Arms
    [ for i in 0 .. h.Arms - 1 do
        let _, spd = petal h i
        if i = 0 then
          if h.Spin > 0.0 then
            let head = sprintf "%.2f + %.3f * (%s)" gap (turnDeg h / 2.0) (waitExpr h)
            fire { sequence head; speed (speedOf spd); refBullet "core" [] }
          else
            fire { absolute "0"; speed (speedOf spd); refBullet "core" [] }
        else
          fire { sequence (sprintf "%.2f" gap); speed (speedOf spd); refBullet "core" [] } ]

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
    let perWave =
      ([ 0 .. h.Arms - 1 ] |> List.sumBy (fun i -> lifeOf (snd (petal h i)))) * float h.Blooms
    max (perWave / wait1) (float (h.Arms * h.Blooms)) * Bound.SAFETY

  /// 越える なら `wait` を伸ばす。花弁 の数 と Arms は形 そのもの なので 減らさない
  let fit (h: HarmonicSpec) : HarmonicSpec =
    let rec go (s: HarmonicSpec) (tries: int) =
      let need = aliveBound s / float MAX_ALIVE
      if need <= 1.0 || tries <= 0 then s
      else go (HarmonicSpec.withWaitScale (s.WaitScale * max 2.0 (ceil need)) s) (tries - 1)
    go h 12

  let generate (spec: HarmonicSpec) : BulletmlInfo =
    let h = fit spec

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
