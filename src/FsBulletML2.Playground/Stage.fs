namespace FsBulletML2.Playground

open FsBulletML2

/// 面の形。**向きで 2 通り。**
///
/// 大きさと、敵・自機の置き場所をひとまとめにしてある —— 別々に持つと、
/// 向きを足したときに「大きさだけ横で、自機は縦のまま」が作れてしまう。
[<Struct>]
type Field =
  { Width: float32
    Height: float32
    EnemyX: float32
    EnemyY: float32
    PlayerX: float32
    PlayerY: float32 }

/// Canvas の大きさと、その中の置き場所。
///
/// **`bulletml/@type` を読むのはここ 1 か所。** エンジンは type を使わず、
/// 受け取って描き方を決めるのはフロントの仕事（仕様の但し書き）。
/// この版まで Playground も Front も 1 度 も見ていなかった。
module Stage =

  /// 縦画面。MonoGame の `Settings.Display` と同じ 480x640
  let portrait =
    { Width = 480.0f
      Height = 640.0f
      EnemyX = 240.0f
      EnemyY = 80.0f
      PlayerX = 240.0f
      PlayerY = 600.0f }

  /// 横画面。**面を回すのではなく、横長にして敵を右・自機を左に置く。**
  ///
  /// 横画面と名乗る 9 本 を境界の無い面で 200 コマ 走らせて測ったら、
  /// **自機に依存しない 4 本 がどれも左へ偏っていた**（弾の 6 割 から
  /// 8 割）。座標系（Y が下向き・0 度 が上）はそのままに、270 度 の側へ
  /// 撃つように書かれている。だから自機を左端に置けば絵が成り立つ。
  ///
  /// 縦の面のままだと、同じ 4 本 が下と左に割れる。自機を追う 5 本 は
  /// もっとはっきり出て、左へ 99 / 416 / 278 / 102 と揃った
  let landscape =
    { Width = 640.0f
      Height = 480.0f
      EnemyX = 560.0f
      EnemyY = 240.0f
      PlayerX = 80.0f
      PlayerY = 240.0f }

  /// 弾幕の type から面を選ぶ。**`none` は縦** ——
  /// 属性を省いたときに走るのが縦なので、そこへ寄せる
  let ofDirection (d: ShootingDirection) =
    match d with
    | ShootingDirection.BulletHorizontal -> landscape
    | ShootingDirection.BulletVertical
    | ShootingDirection.BulletNone -> portrait
