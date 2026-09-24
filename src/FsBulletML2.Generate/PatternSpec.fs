namespace FsBulletML2.Generate

/// 弾幕 の型。知らない 字 は `Radial` に倒す（`PatternSpec.kindOfString`）
type PatternKind =
    | Spiral
    | Radial
    | Aimed
    | Spread
    /// 幕。上 から 横一列 に降りて くる
    | Curtain

/// 撃つ 向き。`absolute` の 180 度 が 自機 の方向 で、0 度 が その 真裏
/// `Around` が既定 で「向き を名指し しない」。型 が決める 向き を 上書き しない
type Facing =
    | Around
    | Forward
    | Backward
    | Sideways

/// 弾 の振る舞い（1 発 が どう 飛ぶ か）。`BulletKinds`（何種類 出すか）とは 別 の軸
type Motion =
    /// 撃った 速さ の まま 飛ぶ
    | Plain
    /// 撃った 直後 に 一気 に伸びる。細くて 速い 線 に見える
    | Laser
    /// 曲がり ながら 加速 する。狙い を `Aiming` と別 に持つ ——
    /// ミサイル と 言われた なら 狙う のが 本体
    | Missile

/// 軸 の生値。`PatternSpec.create` に渡す 途中 の形 で、clamp を通って いない
type Axes =
    {
        Kind: PatternKind
        Speed: float
        Density: float
        Symmetry: float
        Cascade: float
        Layers: float
        Jitter: float
        Rhythm: float
        Depth: float
        BulletKinds: float
        Breathe: bool
        Vanishing: bool
        Aiming: bool
        Pause: bool
        Parametrized: bool
        Ways: int
        Facing: Facing
        Motion: Motion
        KindConfidence: float
    }

/// 弾幕 の仕様。`private` なので `PatternSpec.create` を通らず に作れない（公開 すると clamp を抜ける）
/// 中 に `Axes` を そのまま 持つ ので、軸 を足す ときに 触る のは `Axes` だけ
type PatternSpec =
    private
        {
            Axes_: Axes
            /// `Bound.fit` が上界 を超えた ときに 入れる 係数（既定 1.0）
            /// 外 で掛ける と、上界 を計算 した 式 と 実際 に出る 式 が割れる
            WaitScale_: float
        }

    member this.Kind = this.Axes_.Kind
    member this.Speed = this.Axes_.Speed
    member this.Density = this.Axes_.Density
    member this.Symmetry = this.Axes_.Symmetry
    member this.Cascade = this.Axes_.Cascade
    member this.Layers = this.Axes_.Layers
    member this.Jitter = this.Axes_.Jitter
    member this.Rhythm = this.Axes_.Rhythm
    member this.Depth = this.Axes_.Depth
    member this.BulletKinds = this.Axes_.BulletKinds
    member this.Breathe = this.Axes_.Breathe
    member this.Vanishing = this.Axes_.Vanishing
    member this.Aiming = this.Axes_.Aiming
    member this.Pause = this.Axes_.Pause
    member this.Parametrized = this.Axes_.Parametrized
    /// 腕 の本数 を名指し する（「3way」）。0 は「名指し しない」で、`Symmetry` から 引く
    member this.Ways = this.Axes_.Ways
    member this.Facing = this.Axes_.Facing
    member this.Motion = this.Axes_.Motion
    /// `kind` の確からしさ。軸 ではない —— 画面 に出す 値 で、生成器 は読まない
    member this.KindConfidence = this.Axes_.KindConfidence
    member this.WaitScale = this.WaitScale_

/// 仕様 を作る 口。ここ だけ が値 を作れる。
[<RequireQualifiedAccess>]
module PatternSpec =

    /// 目盛り の外 に出た値 を戻す。呼ぶ側 が段階数 を守る 保証 は無い
    let private onScale (steps: int) (v: float) = max 0.0 (min (float steps - 1.0) v)

    /// 腕 の本数 の上。超えて 名指し されたら `Symmetry` に返す（固定 に すると `$rank` が効かなく なる）
    [<Literal>]
    let MAX_WAYS = 12

    /// 知らない 字 は `Around`（向き を名指し しない）
    let facingOfString (s: string) =
        match s with
        | "forward" -> Forward
        | "backward" -> Backward
        | "sideways" -> Sideways
        | _ -> Around

    /// 知らない 字 は `Plain`
    let motionOfString (s: string) =
        match s with
        | "laser" -> Laser
        | "missile" -> Missile
        | _ -> Plain

    /// 知らない 字 は `Radial` に倒す
    let kindOfString (s: string) =
        match s with
        | "spiral" -> Spiral
        | "aimed" -> Aimed
        | "spread" -> Spread
        | "curtain" -> Curtain
        | _ -> Radial

    /// 何 も名指し して いない 軸。ここ から の差分 で 仕様 を書く
    let zero =
        {
            Kind = Radial
            Speed = 0.0
            Density = 0.0
            Symmetry = 0.0
            Cascade = 0.0
            Layers = 0.0
            Jitter = 0.0
            Rhythm = 0.0
            Depth = 0.0
            BulletKinds = 0.0
            Breathe = false
            Vanishing = false
            Aiming = false
            Pause = false
            Parametrized = false
            Ways = 0
            Facing = Around
            Motion = Plain
            KindConfidence = 0.0
        }

    /// 唯一 の入口。JSON を受け取らない。受け取る のは 差分 を作る 関数 で、
    /// 位置 でなく 名前 で渡す ので 同じ 型 の軸 の取り違え が 型 で止まる
    let create (f: Axes -> Axes) : PatternSpec =
        let a = f zero

        {
            Axes_ =
                { a with
                    Speed = onScale 4 a.Speed
                    Density = onScale 4 a.Density
                    Symmetry = onScale 4 a.Symmetry
                    Cascade = onScale 4 a.Cascade
                    Layers = onScale 3 a.Layers
                    Jitter = onScale 3 a.Jitter
                    Rhythm = onScale 3 a.Rhythm
                    Depth = onScale 3 a.Depth
                    BulletKinds = onScale 3 a.BulletKinds
                    // 範囲 の外 は「名指し しない」に倒す。1 本 未満 も 上 を超えた のも 同じ ——
                    // どちら も `Symmetry` から 引く ほう が 形 になる
                    Ways = (if a.Ways >= 1 && a.Ways <= MAX_WAYS then a.Ways else 0)
                    KindConfidence = max 0.0 (min 1.0 a.KindConfidence)
                }
            WaitScale_ = 1.0
        }

    /// `Bound.fit` だけ が呼ぶ。外 から は `WaitScale` を動かせない
    let internal withWaitScale (scale: float) (s: PatternSpec) = { s with WaitScale_ = scale }

    /// `Bound.fit` が最後 の手段 で使う。`wait` を伸ばして も 1 回 の塊 は減らない ので、
    /// 段 を落とす しか 無い とき だけ。`WaitScale` は 1 に戻して 測り直す
    let internal withCascade (cascade: float) (s: PatternSpec) =
        { s with
            Axes_ =
                { s.Axes_ with
                    Cascade = onScale 4 cascade
                }
            WaitScale_ = 1.0
        }
