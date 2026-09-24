namespace FsBulletML2.Front

/// 画面の Y がどちらを向いているか。
/// F# の判別共用体は C# から既定が null なので、enum。
type Space =
    | YDown = 0
    | YUp = 1

/// これから産まれる弾を、どこに作るか。狙いの式の基準点。
/// 同梱の 2 つ で違う。片方だけ直すと軌跡が割れる。
type SpawnOrigin =
    /// 原点 (0, 0) に作る
    | AtOrigin = 0
    /// 撃った側と同じ場所に作る
    | AtShooter = 1

/// 狙いの向きを出す式。この式は 1 本 しかない。
module Aiming =

    /// `(fx, fy)` から `(tx, ty)` を狙う向き。
    /// 同梱の 2 つ は、この式の `space` 違いだった。
    [<CompiledName "Toward">]
    let toward (space: Space) (fx: float32) (fy: float32) (tx: float32) (ty: float32) : float32 =
        let dy = ty - fy
        let dy = if space = Space.YDown then -dy else dy
        float32 (System.Math.Atan2(float (tx - fx), float dy))

    /// `origin` に従って、産まれる弾の位置を返す
    [<CompiledName "SpawnPoint">]
    let spawnPoint (origin: SpawnOrigin) (x: float32) (y: float32) : struct (float32 * float32) =
        if origin = SpawnOrigin.AtOrigin then
            struct (0.0f, 0.0f)
        else
            struct (x, y)
