/// 雛形（v2.6）。置ける要素ではなく形を出す。
/// `accel` は入れない。主役は `sequence`。CE には出さない。
module FsBulletML2.LanguageService.Frames

open FsBulletML2.LanguageService.SourceLanguage

let private el name attrs text children =
  { Element = name; Attrs = attrs; Text = text; Children = children }

/// 子を持たない要素
let private leaf name text = el name [] text []

let all: FrameSnippet list =
  [
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

    { Label = "自機を狙って撃つ"
      In = [ "action" ]
      Detail = "fire + aim（193 本 中 114 本）"
      Frame =
        el "fire" [] "" [
          el "direction" [ "type", "aim" ] "${1:0}" []
          leaf "speed" "${2:2}"
          leaf "bullet" "" ] }

    { Label = "決めてある弾を撃つ"
      In = [ "action" ]
      Detail = "fire + bulletRef（193 本 中 143 本）"
      Frame =
        el "fire" [] "" [
          el "direction" [ "type", "aim" ] "${1:0}" []
          leaf "speed" "${2:2}"
          el "bulletRef" [ "label", "${3:b1}" ] "" [] ] }

    { Label = "待つ"
      In = [ "action" ]
      Detail = "wait（193 本 中 191 本）"
      Frame = leaf "wait" "${1:30}" }

    { Label = "速さを変える"
      In = [ "action" ]
      Detail = "changeSpeed（222 個 とも同じ形）"
      Frame =
        el "changeSpeed" [] "" [
          leaf "speed" "${1:1}"
          leaf "term" "${2:30}" ] }

    { Label = "向きを変える"
      In = [ "action" ]
      Detail = "changeDirection（143 個 とも同じ形）"
      Frame =
        el "changeDirection" [] "" [
          el "direction" [ "type", "absolute" ] "${1:180}" []
          leaf "term" "${2:30}" ] }

    { Label = "撃って消える弾"
      In = [ "bulletml" ]
      Detail = "bullet + vanish（193 本 中 133 本 が vanish を使う）"
      Frame =
        el "bullet" [ "label", "${1:b1}" ] "" [
          el "action" [] "" [
            leaf "wait" "${2:30}"
            leaf "vanish" "" ] ] }

    { Label = "撃ち始める（top）"
      In = [ "bulletml" ]
      Detail = "action label=\"top\"（根から走る名前）"
      Frame =
        el "action" [ "label", "top" ] "" [
          el "fire" [] "" [
            el "direction" [ "type", "aim" ] "${1:0}" []
            leaf "speed" "${2:2}"
            leaf "bullet" "" ] ] } ]
