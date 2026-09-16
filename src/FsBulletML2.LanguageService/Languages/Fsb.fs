/// インデント記法（fsb）の形。中身は `Lookup` に在る。
///
/// `ISourceLanguage` に足した口は 0 個。
/// 語彙は XML / sxml と同じものを引く —— 表記が変わっても要素と属性は変わらない。
module FsBulletML2.LanguageService.Languages.Fsb

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

let contextAt = FsbScan.contextAt

/// 無い定義を根の直下 に作る。挿す先は本文の末尾。
///
/// 閉じ札も閉じ括弧も無く、入れ子は行頭の空白だけで決まる ——
/// 根の直下 の字下げで 1 行 足せば、それがどこに在っても根の子になる。
/// だから XML / sxml のような「閉じていない本文」の心配も無い。
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

/// 雛形をその表記の字にする（v2.6）。閉じ札が無い ——
/// 入れ子は字下げだけで表す。値は `:"…"` で名前に続ける
/// （`Offside.write` がそう書く。`FrameWrite.Tests` が見る）。
///
/// 字下げは 4 —— こちらも `Offside.write` と揃える
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
