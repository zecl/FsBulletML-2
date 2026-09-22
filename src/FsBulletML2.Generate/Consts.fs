/// 上限。`Exprs` と `Bound` の両方 が引く。
module FsBulletML2.Generate.Consts

/// AST の `fire` 要素 の数。撃った弾 の数 ではない
let [<Literal>] MAX_FIRES = 400

/// 同時 に画面 に居る 弾 の上限。`Generate` が組んだ 木 だけ の 約束 で、`Edit` を通した あと は 約束 しない
let [<Literal>] MAX_ALIVE = 900

/// 弾速 の上限。`$rank = 1.0` で見る
let [<Literal>] MAX_SPEED = 6.0

/// `repeat` の `times` の上限。`$rank = 1.0` で見る
let [<Literal>] MAX_REPEAT = 200

/// 弾 が生きて いる コマ数 の上限。生成器 の `wait "180"` に合わせる
let [<Literal>] BULLET_LIFE = 180.0

/// 弾 が画面 を抜ける まで に進む 距離（面 480x640 の 中ほど から 端 まで 240〜320 の 間）。
/// 速い 弾 は `BULLET_LIFE` より 先 に 抜ける ので、見ない と 上界 が 3 倍 過大 に なる
let [<Literal>] FIELD_SPAN = 280.0
