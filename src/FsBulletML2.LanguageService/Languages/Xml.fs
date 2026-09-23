/// XML の形。中身は `Lookup` に在る。語彙は host が焼いたものを受け取る。
/// パーサは使うな。打っている途中の XML は必ず壊れている。
module FsBulletML2.LanguageService.Languages.Xml

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

/// 字を数えるのは `XmlScan` の 1 本。ここが持つのは表記の形だけ。
let contextAt = XmlScan.contextAt

/// 無い定義を根の直下 に作る。挿す先は根の閉じ札の行の頭。
/// 閉じ札が無ければ作らない。中身は空。入れる字を増やすな。
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

/// 雛形をその表記の字にする（v2.6）。子が在れば入れ子、無ければ 1 行。
/// 字下げは 4。`BulletmlWriter.toIndentedXml 4` と揃える。
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
    // `<` の直後は要素、`"` の直後は属性値。空白は入れるな。どこでも候補が出る。
    TriggerCharacters = [ "<"; "\"" ]
    ContextAt = XmlScan.contextAt
    TokenAt = XmlScan.tokenAt
    Tags = XmlScan.tags
    Texts = XmlScan.texts
    // `=""` まで入れる。名前だけだと手で足すことになる。
    AttrSnippet = fun name -> name + "=\"$0\""
    WriteFrame = writeFrame 0
    // 属性名の手前 に括弧のような字は無い。名前のぶんだけ
    AttrReplace = Scan.nameLenBefore
    ElementTitle = fun name -> "<" + name + ">"
    AttrValueTitle = fun attr value -> attr + "=\"" + value + "\""
    DefinitionAt = definitionAt }

type XmlLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
