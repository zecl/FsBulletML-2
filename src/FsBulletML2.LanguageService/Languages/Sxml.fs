/// sxml の形。**中身は `Lookup` に在る。** XML と違うのはここだけ。
///
/// **`ISourceLanguage` に足した口は 0 個。** v0.3 から
/// 「次の言語はモジュールを 1 個 足すだけ」と言い続けてきたことの、
/// 2 本 目 の実例（`Plan_v0.9.md`）。
///
/// 語彙は XML と同じものを引く —— 正本は `Core/DTD.fs` で、
/// **表記が変わっても要素と属性は変わらない。** 変わるのは書き方だけ。
module FsBulletML2.LanguageService.Languages.Sxml

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

let contextAt = SxmlScan.contextAt

/// 属性を入れるときに食う手前の字。名前のぶんに、手前の `(` が在ればそれも。
///
/// **食わないと `((label "…")` になる。** `$rand` の `$` を食うのと同じ形で、
/// Monaco の「語」に任せると括弧が語に入らない版で二重になる
let attrReplace (src: string) (offset: int) =
  let n = Scan.nameLenBefore src offset
  let at = (min offset src.Length) - n
  if at > 0 && src.[at - 1] = '(' then n + 1 else n

let shape: Shape =
  { Kind = SourceKind.Sxml
    // **Monaco に sxml は無い。** いちばん近い組み込みが `scheme` で、
    // 括弧の対応・文字列・字下げがそのまま効く。
    //
    // **sxml は Scheme ではない**（`;` のコメントも `#|...|#` も、sxml の
    // パーサは読まない）。それでも Monarch の定義を書かないのは、書けば
    // Monaco 固有の物が 1 つ 増えるから。`EditorLanguageId` が表記と
    // 1 対 1 でないのは v0.6 で決めてある —— **その但し書きがここで効く**
    EditorLanguageId = "scheme"
    // `(` の直後は要素、`"` の直後は属性値。XML の `<` と同じ位置
    TriggerCharacters = [ "("; "\"" ]
    ContextAt = SxmlScan.contextAt
    TokenAt = SxmlScan.tokenAt
    // 括弧ごと入れて、引用符の中へカーソルを置く
    AttrSnippet = fun name -> "(" + name + " \"$0\")"
    AttrReplace = attrReplace
    ElementTitle = fun name -> "(" + name + ")"
    AttrValueTitle = fun attr value -> "(" + attr + " \"" + value + "\")" }

type SxmlLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
