/// インデント記法（fsb）の形。中身は `Lookup` に在る。
/// 語彙は XML / sxml と同じ。表記が変わっても要素と属性は変わらない。
module FsBulletML2.LanguageService.Languages.Fsb

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

let contextAt = FsbScan.contextAt

/// 無い定義を根の直下 に作る。挿す先は本文の末尾。
/// 入れ子は行頭の空白だけ。閉じていない本文を心配しない。
let private definitionAt (source: string) (defName: string) (attr: string) (value: string) =
  let tags = FsbScan.tags source
  match tags with
  | [] -> None
  | root :: rest ->
    let indent =
      // 字下げは根の直下 に既に在る行に合わせる。無ければ書き手と同じ 2
      match rest |> List.tryFind (fun t -> Scan.columnOf source t.Start > Scan.columnOf source root.Start) with
      | Some t -> Scan.columnOf source t.Start
      | None -> 2
    let pad = System.String(' ', indent)
    let head = if source.Length > 0 && source.[source.Length - 1] <> '\n' then "\n" else ""
    Some(source.Length, head + pad + defName + " " + attr + "=\"" + value + "\"\n")

/// 雛形をその表記の字にする（v2.6）。閉じ札は無い。
/// 値は `:"…"`。字下げは 4。`Offside.write` と揃える。
let rec private writeFrame (indent: int) (f: Frame) =
  let pad = System.String(' ', indent)
  let attrs =
    f.Attrs |> List.map (fun (k, v) -> " " + k + "=\"" + v + "\"") |> String.concat ""
  let head = pad + f.Element + attrs + (if f.Text = "" then "" else ":\"" + f.Text + "\"")
  if List.isEmpty f.Children then head
  else head + "\n" + (f.Children |> List.map (writeFrame (indent + 4)) |> String.concat "\n")

let shape: Shape =
  { Kind = SourceKind.Fsb
    // Monaco に fsb は無い。ini を選んだ（Monarch は書かない）。
    EditorLanguageId = "ini"
    // 要素を始める字が無い。要素の候補は 1 文字 打つか Ctrl+Space。
    TriggerCharacters = [ "\"" ]
    ContextAt = FsbScan.contextAt
    TokenAt = FsbScan.tokenAt
    Tags = FsbScan.tags
    Texts = FsbScan.texts
    // XML と同じ。`=""` まで入れて引用符の中へカーソルを置く
    AttrSnippet = fun name -> name + "=\"$0\""
    WriteFrame = writeFrame 0
    // 属性名の手前 に括弧のような字は無い。名前のぶんだけ
    AttrReplace = Scan.nameLenBefore
    // 札にも括弧にもならない。その表記で打つ字そのもの
    ElementTitle = fun name -> name
    AttrValueTitle = fun attr value -> attr + "=\"" + value + "\""
    DefinitionAt = definitionAt }

type FsbLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
