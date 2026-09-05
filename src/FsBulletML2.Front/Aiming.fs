namespace FsBulletML2.Front

/// 画面の Y がどちらを向いているか。
///
/// **enum にしてある。** F# の判別共用体は C# から見ると既定が null で
/// （0 ではない）、型は合うのでコンパイルは通り、走らせて初めて落ちる。
/// C# のフロントが `default(Space)` を作る道が在るので enum。
type Space =
  | YDown = 0
  | YUp = 1

/// これから産まれる弾を、どこに作るか。
///
/// 狙いの式の基準点になる。同梱の 2 つ で違う ——
/// MonoGame は位置を入れずに作るので原点、Unity2D は撃った側と同じ場所。
/// **片方だけ直すと軌跡が割れる。**
type SpawnOrigin =
  /// 原点 (0, 0) に作る
  | AtOrigin = 0
  /// 撃った側と同じ場所に作る
  | AtShooter = 1

/// 狙いの向きを出す式。**この式は 1 本 しかない。**
module Aiming =

  /// `(fx, fy)` から `(tx, ty)` を狙う向き。
  ///
  /// **同梱の 2 つ のフロントは、この式の `space` 違いだった。**
  /// MonoGame は画面座標で Y が下向き（`YDown`）、Unity2D は上向き（`YUp`）。
  /// 「座標系が違うから Core へ畳めない」と書いてあったが、格子 11 点 ×
  /// 4 変数 = 14,641 組 を並べたら float32 のビットまで一致した。
  /// 違いは 1 ビット だけ。
  ///
  /// **`Math.Atan2` を double で通してから float32 に落とす。**
  /// Unity の `Mathf.Atan2` も中身は同じで、値が 1 ビット も動かない。
  [<CompiledName "Toward">]
  let toward (space: Space) (fx: float32) (fy: float32) (tx: float32) (ty: float32) : float32 =
    let dy = ty - fy
    let dy = if space = Space.YDown then -dy else dy
    float32 (System.Math.Atan2(float (tx - fx), float dy))

  /// `origin` に従って、産まれる弾の位置を返す
  [<CompiledName "SpawnPoint">]
  let spawnPoint (origin: SpawnOrigin) (x: float32) (y: float32) : struct (float32 * float32) =
    if origin = SpawnOrigin.AtOrigin then struct (0.0f, 0.0f) else struct (x, y)
