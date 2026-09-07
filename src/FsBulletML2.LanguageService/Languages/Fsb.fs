/// インデント記法（fsb）の形。**中身は `Lookup` に在る。**
///
/// **`ISourceLanguage` に足した口は 0 個。** v0.3 から
/// 「次の言語はモジュールを 1 個 足すだけ」と言い続けてきたことの、
/// 3 本 目 の実例（`Plan_v1.1.md`）。
///
/// 語彙は XML / sxml と同じものを引く —— 正本は `Core/DTD.fs` で、
/// **表記が変わっても要素と属性は変わらない。** 変わるのは書き方だけ。
module FsBulletML2.LanguageService.Languages.Fsb

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

let contextAt = FsbScan.contextAt

let shape: Shape =
  { Kind = SourceKind.Fsb
    // **Monaco に fsb は無い。** 組み込み 91 本 に本文を通して測った
    // （v1.1 の頭）——
    //
    //     ini      属性が key / delimiter / string に割れる。**誤りが 0**
    //     python   要素名は付くが `type` を keyword と誤色する
    //     fsharp   同上
    //     yaml     行を丸ごと 1 つ の string にする（読めない）
    //
    // `ini` を選んだ。`#` `;` `[` を引用符の中で壊さないことも当てた。
    // **fsb は ini ではない**が、Monarch を書けば Monaco 固有の物が 1 つ 増える
    // —— `EditorLanguageId` が表記と 1 対 1 でないのは v0.6 で決めてある
    EditorLanguageId = "ini"
    // **要素を始める字が無い**（XML の `<`、sxml の `(` に当たるものが無く、
    // 行頭の字下げのあとに名前が直に来る）。だから要素の候補は
    // 1 文字 打つか Ctrl+Space で出る。ここに置けるのは属性値の `"` だけ
    TriggerCharacters = [ "\"" ]
    ContextAt = FsbScan.contextAt
    TokenAt = FsbScan.tokenAt
    // XML と同じ。`=""` まで入れて引用符の中へカーソルを置く
    AttrSnippet = fun name -> name + "=\"$0\""
    // 属性名の手前 に括弧のような字は無い。名前のぶんだけ
    AttrReplace = Scan.nameLenBefore
    // 札にも括弧にもならない。**その表記で打つ字そのもの**
    ElementTitle = fun name -> name
    AttrValueTitle = fun attr value -> attr + "=\"" + value + "\"" }

type FsbLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
