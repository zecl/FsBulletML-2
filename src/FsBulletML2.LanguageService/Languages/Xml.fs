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

/// 無い定義を根の直下 に作る。**挿す先は根の閉じ札の行の頭。**
///
/// 閉じ札が無ければ作らない —— 打っている途中の本文はふつうに閉じていない。
///
/// 中身は空にする。**入れる字を増やさない** —— 何を書くかは人が決めることで、
/// ここが決めると「消してから書く」ことになる
let private definitionAt (source: string) (defName: string) (attr: string) (value: string) =
  let tags = XmlScan.tags source
  match tags |> List.tryFind (fun t -> not t.Closing) with
  | None -> None
  | Some root ->
    match tags |> List.filter (fun t -> t.Closing && t.TagName = root.TagName) |> List.tryLast with
    | None -> None
    | Some close ->
      // 字下げは根の直下 に既に在る札に合わせる。無ければ書き手と同じ 4
      let indent =
        tags
        |> List.tryFind (fun t -> not t.Closing && t.Start > root.Stop && t.Start < close.Start)
        |> function
           | Some t -> Scan.columnOf source t.Start
           | None -> 4
      let pad = System.String(' ', indent)
      let body =
        pad + "<" + defName + " " + attr + "=\"" + value + "\">\n"
        + pad + "</" + defName + ">\n"
      // 閉じ札の手前 に字が在れば（1 行 で書いてある本文）改行から始める
      if Scan.blankBefore source close.Start
      then Some(Scan.lineStart source close.Start, body)
      else Some(close.Start, "\n" + body)

/// 雛形をその表記の字にする（v2.6）。**子が在れば入れ子、無ければ 1 行。**
///
/// 字下げは 4 —— `BulletmlWriter.toIndentedXml 4` と揃える
/// （`FrameWrite.Tests` が両方 を突き合わせる）
let rec private writeFrame (indent: int) (f: Frame) =
  let pad = System.String(' ', indent)
  let attrs =
    f.Attrs |> List.map (fun (k, v) -> " " + k + "=\"" + v + "\"") |> String.concat ""
  if List.isEmpty f.Children && f.Text = "" then
    pad + "<" + f.Element + attrs + " />"
  elif List.isEmpty f.Children then
    pad + "<" + f.Element + attrs + ">" + f.Text + "</" + f.Element + ">"
  else
    pad + "<" + f.Element + attrs + ">\n"
    + (f.Children |> List.map (writeFrame (indent + 4)) |> String.concat "\n")
    + "\n" + pad + "</" + f.Element + ">"

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
    Texts = XmlScan.texts
    // **`=""` まで入れて、引用符の中へカーソルを置く。**
    // 名前だけ入れると、必ず手で 3 文字 足すことになる
    AttrSnippet = fun name -> name + "=\"$0\""
    WriteFrame = writeFrame 0
    // 属性名の手前 に括弧のような字は無い。名前のぶんだけ
    AttrReplace = Scan.nameLenBefore
    ElementTitle = fun name -> "<" + name + ">"
    AttrValueTitle = fun attr value -> attr + "=\"" + value + "\""
    DefinitionAt = definitionAt }

type XmlLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
