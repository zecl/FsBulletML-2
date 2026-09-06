namespace FsBulletML2.Playground

/// hover に出す散文。**この repo で唯一、語彙の表を手で書くところ。**
///
/// DTD の行は語彙から組める（`Vocabulary`）。既定値は Core の札から引ける
/// （`[<BulletmlDefault>]`）。**散文だけは型から出てこない。**
///
/// --- なぜ Core でなくここか
///
/// エンジンは説明文を要らない。Unity / MonoGame へ配る dll に日本語の散文を
/// 載せる理由が無い。Core に足したのは既定値の札だけで、あれは
/// 「省いたときに走る値」という**型の情報**なので Core が正本でないと嘘になる。
///
/// --- なぜ属性に散らさず一覧にするか
///
/// `[<Description>]` を腕に付ける手も在るが、属性は型の隣に散る。
/// **全部 を一度に読んで直すことができない。** ここは一覧なのでレビューできる。
/// 型が消えて表だけ残る危険は門で塞ぐ（`SpecCoverage`。足りないだけでなく
/// **余りも赤にする**）。
///
/// --- 既定値をここに書かない
///
/// 「既定は aim」は `Vocabulary` の `Defaults` から出る。ここに書くと
/// 同じことが 2 か所 に在って、片方 だけ古びる。
///
/// --- 中身の正しさは機械で測れない
///
/// 門が見るのは「在ること」だけ。裏取りの出どころは
/// `docs/local/bulletml-spec-draft.md` に 1 行 ずつ添えてある。
module Spec =

  /// 要素 1 つ につき 1 〜 2 文。何をするか、と、省いたときどうなるか
  let elements : (string * string) list =
    [ "bulletml",
      "弾幕 1 つ の根。中に action / bullet / fire を好きな数 並べる。走るのは label が top で始まる action。"

      "action",
      "動作の並び。上から順に 1 つ ずつ進み、wait で足が止まる。label を付けると actionRef から呼べる。"

      "actionRef",
      "label が同じ action を、その場に展開して走らせる。param を並べると、呼ばれた側の $1 $2 … に入る。"

      "fire",
      "弾を 1 つ 撃つ。direction を省くと自機狙い、speed を省くと直前に撃った弾の速さを引き継ぐ。"

      "fireRef",
      "label が同じ fire を、その場で撃つ。param は $1 $2 … に入る。"

      "bullet",
      "撃たれる弾の中身。向き・速さと、その弾自身が走らせる action を書く。label を付けると bulletRef から呼べる。"

      "bulletRef",
      "label が同じ bullet を撃つ。param は $1 $2 … に入る。"

      "changeDirection",
      "term フレーム かけて、弾の向きを direction へ寄せる。毎フレーム 同じ量ずつ回る。"

      "changeSpeed",
      "term フレーム かけて、弾の速さを speed へ寄せる。毎フレーム 同じ量ずつ変わる。"

      "accel",
      "term フレーム かけて、弾に加速度をかける。horizontal は横、vertical は縦。省いた軸は 0 として扱う —— そのままではなく、いまの加速度を term フレーム かけて 0 へ戻す。"

      "wait",
      "書いた数だけフレームを止める。"

      "vanish",
      "その弾を消す。"

      "repeat",
      "中の action を times 回 繰り返す。"

      "direction",
      "向き。書くのは度（エンジンの中ではラジアン）。上が 0 で、時計回り。type で「何を基準にした角度か」が変わる。"

      "speed",
      "速さ。1 フレーム あたりに進む量。type で基準が変わる。"

      "horizontal",
      "accel の横向きの加速度。type で基準が変わる。"

      "vertical",
      "accel の縦向きの加速度。type で基準が変わる。"

      "term",
      "何フレーム かけて変えるか。"

      "times",
      "repeat が中身を繰り返す回数。"

      "param",
      "actionRef / fireRef / bulletRef に渡す実引数。並べた順に、呼ばれた側の $1 $2 … になる。" ]

  /// 属性。鍵は `要素/@属性`
  let attributes : (string * string) list =
    [ "bulletml/@type",
      "弾幕が縦画面向きか横画面向きか。エンジンは使わない —— フロントが受け取って描き方を決める。"

      "bulletml/@xmlns",
      "BulletML の名前空間。http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"

      "bulletml/@name",
      "BulletML の仕様にはない。この実装が読み書きする拡張。弾幕の名前。"

      "bulletml/@description",
      "BulletML の仕様にはない。この実装が読み書きする拡張。弾幕の説明。"

      "action/@label",
      "actionRef から引ける名前。top で始まると、そこから走り始める。"

      "fire/@label",
      "fireRef から引ける名前。"

      "bullet/@label",
      "bulletRef から引ける名前。"

      "actionRef/@label",
      "呼ぶ相手の action の名前。省けない。"

      "fireRef/@label",
      "呼ぶ相手の fire の名前。省けない。"

      "bulletRef/@label",
      "呼ぶ相手の bullet の名前。省けない。"

      "direction/@type",
      "向きの決め方。fire の中では撃つ向き、changeDirection の中では寄せ先の向き。"

      "speed/@type",
      "速さの決め方。fire の中では撃つ速さ、changeSpeed の中では寄せ先の速さ。"

      "horizontal/@type",
      "横向きの加速度の決め方。"

      "vertical/@type",
      "縦向きの加速度の決め方。" ]

  /// `horizontal` と `vertical` は、値の意味が同じ。**字を 2 度 書かない** ——
  /// 別々に書くと片方 だけ直る。要素の散文（横／縦）のほうは違うので分けてある
  let private accelAxis (axis: string) =
    [ axis + "/@type=absolute", "その加速度にする。"
      axis + "/@type=relative", "いまの加速度からの差。term で割る。"
      axis + "/@type=sequence", "毎フレーム 足す量。term で割らない。" ]

  /// 属性値。鍵は `要素/@属性=値`。
  ///
  /// **同じ綴りでも型が違えば計算が違う**（`absolute` は direction では
  /// 画面の絶対角、speed ではその速さ、accel ではその加速度）。別々に書く。
  let attrValues : (string * string) list =
    [ "direction/@type=aim", "自機のいる向きを 0 として測る。"
      "direction/@type=absolute", "画面の絶対角。上が 0 度。"
      "direction/@type=relative", "いまの弾の向きから測る。"
      "direction/@type=sequence",
      "直前に撃った弾の向きから測る（fire の中）。changeDirection の中では毎フレーム 足す量そのもの。"

      "speed/@type=absolute", "その速さにする。"
      "speed/@type=relative", "いまの速さからの差。"
      "speed/@type=sequence",
      "直前に撃った弾の速さからの差（fire の中）。changeSpeed の中では毎フレーム 足す量。"

      "bulletml/@type=none", "向きを決めない。"
      "bulletml/@type=vertical", "縦画面。"
      "bulletml/@type=horizontal", "横画面。" ]
    @ accelAxis "horizontal"
    @ accelAxis "vertical"
