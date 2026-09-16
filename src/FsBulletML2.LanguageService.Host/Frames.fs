/// 雛形（v2.6）。「その場所に置ける要素」ではなく「形」を出す。
///
/// `accel` は入れない。 「N 方向 に撃つ」の主役は `sequence`。
/// 字にするのは `Shape.WriteFrame`（雛形を増やしても表記ごとの手は増えない）。
/// F# の CE には出さない（要素名で書かないので、骨がそのまま字にならない）。
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
