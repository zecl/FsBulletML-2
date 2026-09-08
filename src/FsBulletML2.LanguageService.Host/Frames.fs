/// 雛形（v2.6）。**「その場所に置ける要素」ではなく「形」を出す。**
///
/// 補完は要素名を出すが、「N 方向 に撃つ」「自機狙い」のような**形**は出ない。
/// 書き始めの人がいちばん止まるところ。
///
/// ## 何を入れるかは数で決めた
///
/// 同梱 176 本 + 公式 17 本 = 193 本 を数えた（v2.6 の頭）——
///
///     repeat の中の fire      574 個。うち **336 個（58.5%）が sequence**
///     changeSpeed の中身      222 個 とも `speed term`（**100%**）
///     changeDirection の中身   143 個 とも `direction term`（**100%**）
///     fire の中身             `direction speed bullet` 518 個 38.6%
///                            `direction speed bulletRef` 377 個 28.1%
///
/// **本の数**（1 本 に何回 出ても 1）——
///
///     wait 191 / repeat 191 / sequence 157 / bulletRef 143 / vanish 133 /
///     changeSpeed 115 / aim 114 / changeDirection 72 / accel 13
///
/// **`accel` は入れない**（193 本 中 13 本、6.7%）。
///
/// **予測が 1 つ 外れた。** 「`direction` の type は `aim` がいちばん多い
/// （自機狙いが弾幕の基本）」と凍結したが、回数では 4 番目（12.2%）で、
/// `absolute` 36.5% / `sequence` 35.7% / `relative` 12.5% の後。
/// **「N 方向 に撃つ」の主役は `sequence`** で、そこを雛形の 1 番 に置いた。
///
/// ## 骨は表記を知らない
///
/// 字にするのは `Shape.WriteFrame`。**3 表記 × 1 関数**なので、
/// 雛形をここに増やしても表記ごとの手は増えない。
///
/// **F# の CE には出さない。** あちらは要素名で書かないので
/// （`aim` と打つと `<direction type="aim">` になる）、骨がそのまま字にならない
/// —— `Fsharp.fs` を `Shape` に載せないのと同じ理由。
module FsBulletML2.LanguageService.Frames

open FsBulletML2.LanguageService.SourceLanguage

let private el name attrs text children =
  { Element = name; Attrs = attrs; Text = text; Children = children }

/// 子を持たない要素
let private leaf name text = el name [] text []

let all: FrameSnippet list =
  [ // repeat の中の fire 574 個 のうち 336 個（58.5%）が sequence
    { Label = "N 方向 に撃つ"
      In = [ "action" ]
      Detail = "repeat + sequence（193 本 中 157 本 が sequence を使う）"
      Frame =
        el "repeat" [] "" [
          leaf "times" "${1:8}"
          el "action" [] "" [
            el "fire" [] "" [
              el "direction" [ "type", "sequence" ] "${2:45}" []
              leaf "speed" "${3:2}"
              leaf "bullet" "" ] ] ] }

    // aim は 193 本 中 114 本（59.1%）
    { Label = "自機を狙って撃つ"
      In = [ "action" ]
      Detail = "fire + aim（193 本 中 114 本）"
      Frame =
        el "fire" [] "" [
          el "direction" [ "type", "aim" ] "${1:0}" []
          leaf "speed" "${2:2}"
          leaf "bullet" "" ] }

    // fire の中身 1342 個 のうち 377 + 248 = 625 個（46.6%）が bulletRef
    { Label = "決めてある弾を撃つ"
      In = [ "action" ]
      Detail = "fire + bulletRef（193 本 中 143 本）"
      Frame =
        el "fire" [] "" [
          el "direction" [ "type", "aim" ] "${1:0}" []
          leaf "speed" "${2:2}"
          el "bulletRef" [ "label", "${3:b1}" ] "" [] ] }

    // 193 本 中 191 本（99.0%）
    { Label = "待つ"
      In = [ "action" ]
      Detail = "wait（193 本 中 191 本）"
      Frame = leaf "wait" "${1:30}" }

    // 222 個 とも speed + term（100%）
    { Label = "速さを変える"
      In = [ "action" ]
      Detail = "changeSpeed（222 個 とも同じ形）"
      Frame =
        el "changeSpeed" [] "" [
          leaf "speed" "${1:1}"
          leaf "term" "${2:30}" ] }

    // 143 個 とも direction + term（100%）
    { Label = "向きを変える"
      In = [ "action" ]
      Detail = "changeDirection（143 個 とも同じ形）"
      Frame =
        el "changeDirection" [] "" [
          el "direction" [ "type", "absolute" ] "${1:180}" []
          leaf "term" "${2:30}" ] }

    // vanish は 193 本 中 133 本（68.9%）
    { Label = "撃って消える弾"
      In = [ "bulletml" ]
      Detail = "bullet + vanish（193 本 中 133 本 が vanish を使う）"
      Frame =
        el "bullet" [ "label", "${1:b1}" ] "" [
          el "action" [] "" [
            leaf "wait" "${2:30}"
            leaf "vanish" "" ] ] }

    // bulletml の直下 の形。193 本 中 27 + 16 + 15 = 58 本 が action + bullet
    { Label = "撃ち始める（top）"
      In = [ "bulletml" ]
      Detail = "action label=\"top\"（根から走る名前）"
      Frame =
        el "action" [ "label", "top" ] "" [
          el "fire" [] "" [
            el "direction" [ "type", "aim" ] "${1:0}" []
            leaf "speed" "${2:2}"
            leaf "bullet" "" ] ] } ]
