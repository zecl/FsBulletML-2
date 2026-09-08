namespace FsBulletML2.Playground

open System

/// 自機の動かし方。
///
/// **enum にしてある。** 外（JS）から数で来るので、増えたときに
/// 「知らない数」を既定へ倒せる形が要る。
type PlayerMotion =
  /// マウスを追う。**既定**
  | Follow = 0
  /// 定位置で止める
  | Fixed = 1
  /// 定位置のまわりを回る
  | Orbit = 2

/// 自機の置き場所。
///
/// **狙いを使う弾幕は 176 本 中 103 本**（自機を 2 か所 に置いて
/// 120 コマ 後 の弾の位置を比べた実測）。つまり自機をどう動かすかは、
/// 半分 以上 の弾幕で絵そのものを変える。
module Player =

  /// 回る周期。**コマ数で持つ** —— 時計で回すと、速さを変えたときと
  /// 飛んだときに絵が変わってしまう
  [<Literal>]
  let Period = 240

  /// 面のどれだけを使って回るか。1.0 にすると端に貼りつく
  [<Literal>]
  let Reach = 0.8f

  /// 回る自機の位置。**コマ数だけで決まる。**
  ///
  /// 同じ種・同じ難度なら同じ絵になる、を自機でも守る ——
  /// 時計から出すと、リンクを開いた人が別の絵を見る。
  ///
  /// 半径は**定位置から近いほうの端まで**の 8 割。だから縦の面では
  /// 下のほうで横に大きく、横の面では左端で縦に大きく動く。
  /// どちらも面の中に収まる（`Reach < 1`）
  let orbit (field: Field) (frame: int) : struct (float32 * float32) =
    let cx = field.PlayerX
    let cy = field.PlayerY
    let rx = min cx (field.Width - cx) * Reach
    let ry = min cy (field.Height - cy) * Reach
    let t = float (((frame % Period) + Period) % Period) / float Period * 2.0 * Math.PI
    struct (cx + rx * float32 (sin t), cy + ry * float32 (cos t))

  /// 数から動かし方へ。**知らない数は追う** ——
  /// 落とすと、開いた人には「自機が消えた」に見える
  let ofInt (n: int) =
    match n with
    | 1 -> PlayerMotion.Fixed
    | 2 -> PlayerMotion.Orbit
    | _ -> PlayerMotion.Follow
