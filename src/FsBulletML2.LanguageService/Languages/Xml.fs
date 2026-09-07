/// XML の形。**中身は `Lookup` に在る。**
///
/// v0.8 まで、ここに語彙の引き方も hover の組み立ても在った。sxml を足したら
/// **どちらも表記の話ではなかった** —— 残ったのはこの `shape` だけ。
///
/// 語彙は持たない —— host が `Core/DTD.fs` から焼いたものを受け取る。
///
/// **精度より、止まらないこと。** 打っている途中の XML は必ず壊れているので、
/// パーサは使わずに `XmlScan` が `<` から数える。
module FsBulletML2.LanguageService.Languages.Xml

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

/// 字を数えるのは `XmlScan` の 1 本。**ここが持つのは表記の形だけ。**
/// 名前をここへ引き直しているのは、呼ぶ側（試験と `Playground.fs`）が
/// 表記のモジュールだけを見ていれば済むようにするため
let contextAt = XmlScan.contextAt

let shape: Shape =
  { Kind = SourceKind.Xml
    EditorLanguageId = "xml"
    // `<` の直後は要素、`"` の直後は属性値。**空白は入れない** ——
    // 本文のどこで空白を打っても候補が出ることになる。
    // 属性名は 1 文字 打つか Ctrl+Space で出る
    TriggerCharacters = [ "<"; "\"" ]
    ContextAt = XmlScan.contextAt
    TokenAt = XmlScan.tokenAt
    Tags = XmlScan.tags
    // **`=""` まで入れて、引用符の中へカーソルを置く。**
    // 名前だけ入れると、必ず手で 3 文字 足すことになる
    AttrSnippet = fun name -> name + "=\"$0\""
    // 属性名の手前 に括弧のような字は無い。名前のぶんだけ
    AttrReplace = Scan.nameLenBefore
    ElementTitle = fun name -> "<" + name + ">"
    AttrValueTitle = fun attr value -> attr + "=\"" + value + "\"" }

type XmlLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
