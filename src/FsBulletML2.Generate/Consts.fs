/// 上限。`Exprs` と `Bound` の両方 が引く。
module FsBulletML2.Generate.Consts

/// AST の `fire` 要素 の数。撃った弾 の数 ではない
let [<Literal>] MAX_FIRES = 400

/// 同時 に画面 に居る 弾 の上限
let [<Literal>] MAX_ALIVE = 900

/// 弾速 の上限。`$rank = 1.0` で見る
let [<Literal>] MAX_SPEED = 6.0

/// `repeat` の `times` の上限。`$rank = 1.0` で見る
let [<Literal>] MAX_REPEAT = 200

/// 弾 が生きて いる コマ数。生成器 の `wait "180"` に合わせる
let [<Literal>] BULLET_LIFE = 180.0
