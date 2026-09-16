/// sxml の形。中身は `Lookup` に在る。 XML と違うのはここだけ。
///
/// `ISourceLanguage` に足した口は 0 個。
/// 語彙は XML と同じものを引く —— 表記が変わっても要素と属性は変わらない。
module FsBulletML2.LanguageService.Languages.Sxml

open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage
open FsBulletML2.LanguageService.Languages.Lookup

let contextAt = SxmlScan.contextAt

/// 属性を入れるときに食う手前の字。名前のぶんに、手前の `(` が在ればそれも。
///
/// 食わないと `((label "…")` になる。 `$rand` の `$` を食うのと同じ形で、
/// Monaco の「語」に任せると括弧が語に入らない版で二重になる
let attrReplace (src: string) (offset: int) =
  let n = Scan.nameLenBefore src offset
  let at = (min offset src.Length) - n
  if at > 0 && src.[at - 1] = '(' then n + 1 else n

/// 無い定義を根の直下 に作る。挿す先は根の閉じ括弧の行の頭。
///
/// XML と違って閉じ札が別に無い —— `Stop` がその括弧の `)` を指している
/// （閉じていなければ本文の末尾を指すので、そこは作らない）。
let private definitionAt (source: string) (defName: string) (attr: string) (value: string) =
  let tags = SxmlScan.tags source
  match tags |> List.tryHead with
  | None -> None
  // 閉じていない。`Stop` が本文の末尾を指しているので場所にならない
  | Some root when root.Stop >= source.Length -> None
  | Some root ->
    let indent =
      tags
      |> List.tryFind (fun t -> t.Start > root.Start && t.Start < root.Stop)
      |> function
         | Some t -> Scan.columnOf source t.Start
         | None -> 4
    let pad = System.String(' ', indent)
    let body = pad + "(" + defName + " (@ (" + attr + " \"" + value + "\")))\n"
    if Scan.blankBefore source root.Stop
    then Some(Scan.lineStart source root.Stop, body)
    else Some(root.Stop, "\n" + body)

/// 雛形をその表記の字にする（v2.6）。閉じ括弧は最後の子の行に寄せる ——
/// `Sxml.write` がそう書くので、そこへ合わせる（`FrameWrite.Tests` が見る）。
///
/// 字下げは 2 —— こちらも `Sxml.write` と揃える
let rec private writeFrame (indent: int) (f: Frame) =
  let pad = System.String(' ', indent)
  let attrs =
    if List.isEmpty f.Attrs then ""
    else
      " (@ "
      + (f.Attrs |> List.map (fun (k, v) -> "(" + k + " \"" + v + "\")") |> String.concat " ")
      + ")"
  if List.isEmpty f.Children then
    pad + "(" + f.Element + attrs + (if f.Text = "" then "" else " \"" + f.Text + "\"") + ")"
  else
    pad + "(" + f.Element + attrs + "\n"
    + (f.Children |> List.map (writeFrame (indent + 2)) |> String.concat "\n")
    + ")"

let shape: Shape =
  { Kind = SourceKind.Sxml
    // Monaco に sxml は無い。いちばん近い組み込みが scheme。Monarch は書かない。
    EditorLanguageId = "scheme"
    // `(` の直後は要素、`"` の直後は属性値。XML の `<` と同じ位置
    TriggerCharacters = [ "("; "\"" ]
    ContextAt = SxmlScan.contextAt
    TokenAt = SxmlScan.tokenAt
    Tags = SxmlScan.tags
    Texts = SxmlScan.texts
    // 括弧ごと入れて、引用符の中へカーソルを置く
    AttrSnippet = fun name -> "(" + name + " \"$0\")"
    WriteFrame = writeFrame 0
    AttrReplace = attrReplace
    ElementTitle = fun name -> "(" + name + ")"
    AttrValueTitle = fun attr value -> "(" + attr + " \"" + value + "\")"
    DefinitionAt = definitionAt }

type SxmlLanguage(vocabulary: unit -> Vocab) =
  inherit VocabularyLanguage(shape, vocabulary)
