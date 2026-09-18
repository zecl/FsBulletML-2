namespace FsBulletML2.Generate

/// 弾幕 の型。知らない 字 は `Radial` に倒す（`PatternSpec.kindOfString`）
type PatternKind =
  | Spiral
  | Radial
  | Aimed
  | Spread

/// 弾幕 の仕様。`private` なので `PatternSpec.create` を通らず に作れない ——
/// 公開 すると `{ spec with Speed = 99.0 }` が書けて clamp を抜ける。
type PatternSpec =
  private
    { Kind_: PatternKind
      // --- 4 段 ---
      Speed_: float
      Density_: float
      Symmetry_: float
      Cascade_: float
      // --- 3 段 ---
      Layers_: float
      Jitter_: float
      Rhythm_: float
      Depth_: float
      BulletKinds_: float
      // --- 真偽 ---
      Breathe_: bool
      Vanishing_: bool
      Aiming_: bool
      Pause_: bool
      Parametrized_: bool
      /// `kind` の確からしさ。軸 ではない —— 画面 に出す 値 で、生成器 は読まない
      KindConfidence_: float
      /// `Bound.fit` が上界 を超えた ときに 入れる 係数（既定 1.0）。
      /// 仕様 の中 に持つ —— 外 で掛ける と、上界 を計算 した 式 と
      /// 実際 に出る 式 が割れる
      WaitScale_: float }

  member this.Kind = this.Kind_
  member this.Speed = this.Speed_
  member this.Density = this.Density_
  member this.Symmetry = this.Symmetry_
  member this.Cascade = this.Cascade_
  member this.Layers = this.Layers_
  member this.Jitter = this.Jitter_
  member this.Rhythm = this.Rhythm_
  member this.Depth = this.Depth_
  member this.BulletKinds = this.BulletKinds_
  member this.Breathe = this.Breathe_
  member this.Vanishing = this.Vanishing_
  member this.Aiming = this.Aiming_
  member this.Pause = this.Pause_
  member this.Parametrized = this.Parametrized_
  member this.KindConfidence = this.KindConfidence_
  member this.WaitScale = this.WaitScale_

/// 仕様 を作る 口。ここ だけ が値 を作れる。
[<RequireQualifiedAccess>]
module PatternSpec =

  /// 目盛り の外 に出た値 を戻す。呼ぶ側 が段階数 を守る 保証 は無い
  let private onScale (steps: int) (v: float) = max 0.0 (min (float steps - 1.0) v)

  /// 知らない 字 は `Radial` に倒す
  let kindOfString (s: string) =
    match s with
    | "spiral" -> Spiral
    | "aimed" -> Aimed
    | "spread" -> Spread
    | _ -> Radial

  /// 唯一 の入口。 JSON を受け取らない —— 呼ぶ側 の都合 を持ち込まない
  let create
    (kind: PatternKind)
    (speed: float)
    (density: float)
    (symmetry: float)
    (layers: float)
    (jitter: float)
    (rhythm: float)
    (depth: float)
    (bulletKinds: float)
    (cascade: float)
    (breathe: bool)
    (vanishing: bool)
    (aiming: bool)
    (pause: bool)
    (parametrized: bool)
    (kindConfidence: float)
    : PatternSpec =
    { Kind_ = kind
      Speed_ = onScale 4 speed
      Density_ = onScale 4 density
      Symmetry_ = onScale 4 symmetry
      Cascade_ = onScale 4 cascade
      Layers_ = onScale 3 layers
      Jitter_ = onScale 3 jitter
      Rhythm_ = onScale 3 rhythm
      Depth_ = onScale 3 depth
      BulletKinds_ = onScale 3 bulletKinds
      Breathe_ = breathe
      Vanishing_ = vanishing
      Aiming_ = aiming
      Pause_ = pause
      Parametrized_ = parametrized
      KindConfidence_ = max 0.0 (min 1.0 kindConfidence)
      WaitScale_ = 1.0 }

  /// `Bound.fit` だけ が呼ぶ。外 から は `WaitScale` を動かせない
  let internal withWaitScale (scale: float) (s: PatternSpec) = { s with WaitScale_ = scale }

  /// `Bound.fit` が最後 の手段 で使う。`wait` を伸ばして も 1 回 の塊 は減らない ので、
  /// 段 を落とす しか 無い とき だけ。`WaitScale` は 1 に戻して 測り直す
  let internal withCascade (cascade: float) (s: PatternSpec) =
    { s with Cascade_ = onScale 4 cascade; WaitScale_ = 1.0 }
