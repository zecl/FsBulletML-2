namespace FsBulletML2.LanguageService

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

  /// F# の CE の名前が、**BulletML の何を作るか。**
  ///
  /// --- 散文をここに書かない
  ///
  /// 上の 3 つ の表と違って、ここに在るのは対応だけ。**hover に出す字は
  /// 上の表からそのまま引く** —— CE で書いていても読んでいるのは BulletML で、
  /// `fire` の意味は表記が変わっても変わらない。
  /// 書くと同じ説明が 2 か所 に在って、片方 だけ古びる。
  ///
  /// --- 形
  ///
  ///     (CE の名前, 要素, 固定する属性, 固定する値)
  ///
  /// 属性を固定しないものは `"" ""`。**同じ名前が何行 在ってもよい** ——
  ///
  ///     `changeDirectionAbs` は `<changeDirection>` を作り、
  ///     その中の `<direction>` の型を `absolute` に固定する（2 行）
  ///
  ///     `vertical` は**名前が 2 つ の意味を持つ** —— 根の
  ///     `<bulletml>` の型と、`accel` の中の `<vertical>`。
  ///     どちらか に決められないので両方 出す
  ///
  /// **型を固定する名前にも、要素の行を書く。** 値の散文だけだと
  /// 「どの要素の話か」「ほかにどんな属性が在るか」が hover から落ちる ——
  /// `aim` が「自機のいる向き」とだけ出て、`<direction>` の DTD が出ない形。
  /// 揃えないと、名前によって出る量が違う理由が読めない
  ///
  /// --- 過不足は門で見る
  ///
  /// `FsBulletML2.Dsl` を reflection で舐めた名前を**過不足なく覆うこと**と、
  /// ここに書いた要素・属性・値が**語彙に在ること**を試験が当てている
  /// （`SpecCoverage`）。名前が増えても消えても赤くなる。
  let ce : (string * string * string * string) list =
    [ // --- 根 -------------------------------------------------------------
      "bulletmlOf", "bulletml", "", ""
      "untyped", "bulletml", "", ""
      "untypedAnon", "bulletml", "", ""
      "untypedXmlns", "bulletml", "", ""
      "untypedXmlnsAnon", "bulletml", "", ""
      "vertical", "bulletml", "", ""
      "vertical", "bulletml", "type", "vertical"
      "verticalAnon", "bulletml", "", ""
      "verticalAnon", "bulletml", "type", "vertical"
      "verticalXmlns", "bulletml", "", ""
      "verticalXmlns", "bulletml", "type", "vertical"
      "verticalXmlnsAnon", "bulletml", "", ""
      "verticalXmlnsAnon", "bulletml", "type", "vertical"
      "horizontal", "bulletml", "", ""
      "horizontal", "bulletml", "type", "horizontal"
      "horizontalAnon", "bulletml", "", ""
      "horizontalAnon", "bulletml", "type", "horizontal"
      "horizontalXmlns", "bulletml", "", ""
      "horizontalXmlns", "bulletml", "type", "horizontal"
      "horizontalXmlnsAnon", "bulletml", "", ""
      "horizontalXmlnsAnon", "bulletml", "type", "horizontal"
      "none", "bulletml", "", ""
      "none", "bulletml", "type", "none"
      "noneAnon", "bulletml", "", ""
      "noneAnon", "bulletml", "type", "none"
      "noneXmlns", "bulletml", "", ""
      "noneXmlns", "bulletml", "type", "none"
      "noneXmlnsAnon", "bulletml", "", ""
      "noneXmlnsAnon", "bulletml", "type", "none"

      // --- action -----------------------------------------------------------
      "action", "action", "", ""
      "top", "action", "", ""
      "defAction", "action", "", ""
      "defActionAnon", "action", "", ""
      "body", "action", "", ""
      "bodyAs", "action", "", ""
      "nest", "action", "", ""
      "nestAs", "action", "", ""
      "doActs", "action", "", ""
      "actionRef", "actionRef", "", ""
      "bodyRef", "actionRef", "", ""
      "refActs", "actionRef", "", ""

      // --- 動作 -------------------------------------------------------------
      "wait", "wait", "", ""
      "vanish", "vanish", "", ""
      "repeat", "repeat", "", ""
      "repeatAs", "repeat", "", ""
      "repeatRef", "repeat", "", ""
      "repeatRef", "actionRef", "", ""

      "changeDirection", "changeDirection", "", ""
      "changeDirectionAim", "changeDirection", "", ""
      "changeDirectionAim", "direction", "type", "aim"
      "changeDirectionAbs", "changeDirection", "", ""
      "changeDirectionAbs", "direction", "type", "absolute"
      "changeDirectionRel", "changeDirection", "", ""
      "changeDirectionRel", "direction", "type", "relative"
      "changeDirectionSeq", "changeDirection", "", ""
      "changeDirectionSeq", "direction", "type", "sequence"

      "changeSpeed", "changeSpeed", "", ""
      "changeSpeedAbs", "changeSpeed", "", ""
      "changeSpeedAbs", "speed", "type", "absolute"
      "changeSpeedRel", "changeSpeed", "", ""
      "changeSpeedRel", "speed", "type", "relative"
      "changeSpeedSeq", "changeSpeed", "", ""
      "changeSpeedSeq", "speed", "type", "sequence"

      // --- 撃つ -------------------------------------------------------------
      "fire", "fire", "", ""
      "fireAs", "fire", "", ""
      "topFire", "fire", "", ""
      "topFireAs", "fire", "", ""
      "fireRef", "fireRef", "", ""

      "bullet", "bullet", "", ""
      "bulletAnon", "bullet", "", ""
      "defBullet", "bullet", "", ""
      "defBulletAnon", "bullet", "", ""
      "plain", "bullet", "", ""
      "ofBullet", "bullet", "", ""
      "bulletRef", "bulletRef", "", ""
      "refBullet", "bulletRef", "", ""

      // --- 向きと速さ（fire / bullet の中） ---------------------------------
      "dir", "direction", "", ""
      "aim", "direction", "", ""
      "aim", "direction", "type", "aim"
      "absolute", "direction", "", ""
      "absolute", "direction", "type", "absolute"
      "relative", "direction", "", ""
      "relative", "direction", "type", "relative"
      "sequence", "direction", "", ""
      "sequence", "direction", "type", "sequence"

      "speed", "speed", "", ""
      "speedAbs", "speed", "", ""
      "speedAbs", "speed", "type", "absolute"
      "speedRel", "speed", "", ""
      "speedRel", "speed", "type", "relative"
      "speedSeq", "speed", "", ""
      "speedSeq", "speed", "type", "sequence"

      // --- accel の中 -------------------------------------------------------
      //
      // **`horizontal` と `vertical` は上にも在る。** あちらは根の型で、
      // こちらは accel の中の要素。同じ綴りで別のもの
      "accel", "accel", "", ""
      "horizontal", "horizontal", "", ""
      "horizontalAbs", "horizontal", "", ""
      "horizontalAbs", "horizontal", "type", "absolute"
      "horizontalRel", "horizontal", "", ""
      "horizontalRel", "horizontal", "type", "relative"
      "horizontalSeq", "horizontal", "", ""
      "horizontalSeq", "horizontal", "type", "sequence"
      "vertical", "vertical", "", ""
      "verticalAbs", "vertical", "", ""
      "verticalAbs", "vertical", "type", "absolute"
      "verticalRel", "vertical", "", ""
      "verticalRel", "vertical", "type", "relative"
      "verticalSeq", "vertical", "", ""
      "verticalSeq", "vertical", "type", "sequence" ]

  /// **CE の名前が載せる label。** `ce` は「その名前が何を作るか」で、
  /// こちらは「その名前が名前を決めるか、使うか」。
  ///
  ///     (CE の名前, label の付く要素, 名前が何番目 の文字列か,
  ///      引数を取らないときの名前, 根の直下 に書けるか)
  ///
  /// 何番目 は**その名前のあとに続く文字列リテラルの番号**（0 起点）——
  ///
  ///     defAction "center" {          0 番目 が名前
  ///     repeatRef "8" "center" [ ]    **1 番目**（手前 に回数が在る）
  ///     top {                         引数を取らない。名前は固定の top
  ///
  /// **`ce` と別の表にする。** あちらは「作る要素」で、`repeatAs` は
  /// `<repeat>` を作るが、**名前が付くのはその中の `<action>`** ——
  /// 同じ欄に混ぜると、どちらの意味かが行から読めなくなる。
  ///
  /// 名前を持たない CE 名（`action` / `body` / `nest` / `fire` …）は
  /// ここに書かない。**書かないことが「名前を持たない」の印。**
  ///
  /// 最後の欄は「定義を作る」が使う —— **同じ要素を作る名前が複数 在り、
  /// どこに置けるかで使い分ける。** `defAction` は根の直下、`nestAs` は
  /// `action` の中、`bodyAs` は `bullet` の中。要素だけでは選べない。
  ///
  /// --- 過不足は門で見る
  ///
  /// ここに書いた名前が `ce` にも在ること、要素が語彙に在ることを
  /// `SpecCoverage` が当てる。**数そのもの**は、同梱 176 本 を CE と XML の
  /// 両方 で書いて「定義と参照の数が一致する」ことで見る。
  let ceLabels : (string * string * int * string * bool) list =
    [ // --- 名前を決める側 ---------------------------------------------------
      "top", "action", -1, "top", true
      "defAction", "action", 0, "", true
      "nestAs", "action", 0, "", false
      "bodyAs", "action", 0, "", false
      // `<repeat>` を作るが、名前が付くのは中の `<action>`
      "repeatAs", "action", 1, "", false
      "fireAs", "fire", 0, "", false
      "topFireAs", "fire", 0, "", true
      "bullet", "bullet", 0, "", false
      "defBullet", "bullet", 0, "", true

      // --- 名前を使う側 -----------------------------------------------------
      "actionRef", "actionRef", 0, "", false
      "bodyRef", "actionRef", 0, "", false
      "refActs", "actionRef", 0, "", false
      "repeatRef", "actionRef", 1, "", false
      "fireRef", "fireRef", 0, "", false
      "bulletRef", "bulletRef", 0, "", false
      "refBullet", "bulletRef", 0, "", false ]
