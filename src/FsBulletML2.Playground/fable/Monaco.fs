/// Monaco のバインディング。**手書きの `.js` を足さない** ——
/// ここの `[<Emit>]` が glue の代わり。
///
/// 本体は CDN の AMD ローダで読む。版は `wwwroot/index.html` の script src に
/// 焼いてあり、`vsBase` はそこから渡す。**このファイルに版を書かない。**
///
/// `create` は `onReady` のあと、**rAF の外**で 1 回 だけ呼ぶ。
/// ローダの読み込みは非同期なので、描画は待たせない。
module FsBulletML2.Playground.Monaco

open Fable.Core
open Fable.Core.JsInterop
open Browser

// AMD ローダは `<script src>` が globalThis に置く。module の中から見るので
// `globalThis.` を明示する（bundler を挟んでいないので裸の require は解決しない）
[<Emit("globalThis.require")>]
let private amd: obj = jsNative

[<Emit("globalThis.require.config({ paths: { vs: $0 } })")>]
let private amdConfig (vsBase: string) : unit = jsNative

[<Emit("globalThis.require([$0], $1)")>]
let private amdRequire (id: string) (cb: unit -> unit) : unit = jsNative

[<Emit("globalThis.monaco.editor.create($0, $1)")>]
let private createEditor (host: obj) (opts: obj) : obj = jsNative

[<Emit("globalThis.monaco.languages.registerCompletionItemProvider($0, { triggerCharacters: $2, provideCompletionItems: $1 })")>]
let private registerCompletion (language: string) (fn: obj -> obj -> obj) (triggers: string[]) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerHoverProvider($0, { provideHover: $1 })")>]
let private registerHover (language: string) (fn: obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerCodeActionProvider($0, { provideCodeActions: $1 })")>]
let private registerCodeAction (language: string) (fn: obj -> obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerRenameProvider($0, { resolveRenameLocation: $1, provideRenameEdits: $2 })")>]
let private registerRename
  (language: string)
  (resolve: obj -> obj -> obj)
  (provide: obj -> obj -> string -> obj)
  : unit = jsNative

// WorkspaceEdit は「どの model の、どの版に当てるか」を持つ
[<Emit("$0.uri")>]
let private modelUri (model: obj) : obj = jsNative

[<Emit("$0.getVersionId()")>]
let private modelVersion (model: obj) : int = jsNative

[<Emit("globalThis.monaco.editor.setModelLanguage($0.getModel(), $1)")>]
let private setModelLanguage (editor: obj) (language: string) : unit = jsNative

[<Emit("$0.getValue()")>]
let private getVal (editor: obj) : string = jsNative

[<Emit("$0.setValue($1)")>]
let private setVal (editor: obj) (text: string) : unit = jsNative

[<Emit("$0.getWordUntilPosition($1)")>]
let private wordUntil (model: obj) (position: obj) : obj = jsNative

// 行と桁でなく文字数で渡す。**行の数え方を自分で書かない**
[<Emit("$0.getOffsetAt($1)")>]
let private offsetAt (model: obj) (position: obj) : int = jsNative

// 引数なしの layout() は「入れ物の実寸を測り直す」。寸法を渡すと逆に固定される
[<Emit("$0.layout()")>]
let private layoutToContainer (editor: obj) : unit = jsNative

let mutable private editor: obj = null

/// 版は `index.html` の `<script src>` 1 か所 だけに在る。**そこから読む** ——
/// `vsBase` を別に書くと、片方だけ上げたときに黙ってずれる
let vsBaseFromPage () : string option =
  let tail = "/loader.js"
  let nodes = document.querySelectorAll "script[src]"
  let mutable found = None
  for i in 0 .. nodes.length - 1 do
    let src = (nodes.[i] :?> Types.HTMLScriptElement).src
    if src.EndsWith("/vs" + tail) then
      found <- Some(src.Substring(0, src.Length - tail.Length))
  found

/// **create が済むまで false。** 呼ぶ側はこれを見て textarea 相当の
/// 空文字ではなく「まだ」を区別する
let ready () = not (isNull editor)

/// ローダを走らせる。`vsBase` は `.../min/vs`。
/// 読めなければ例外 —— **黙って textarea のまま続けない。**
let load (vsBase: string) (onLoaded: unit -> unit) =
  if isNull amd then failwith "Monaco のローダが読めていない（CDN に届いていない）"
  amdConfig vsBase
  amdRequire "vs/editor/editor.main" onLoaded

let create (hostId: string) (language: string) (initial: string) =
  let host = document.getElementById hostId
  if isNull host then failwith ("要素が無い: " + hostId)
  editor <-
    createEditor
      host
      (createObj [
        "value" ==> initial
        "language" ==> language
        "theme" ==> "vs-dark"
        // 入れ物の大きさへの追随は Monaco 自身に任せる。
        // **自前の ResizeObserver に替えかけたが戻した** —— 背面タブでは
        // どちらも発火しない（layout / paint の流れが回らない）ので、
        // 「自前のほうが確かめやすい」は成り立たなかった。
        // 動く実績のある側を残し、測れないものを増やさない
        "automaticLayout" ==> true
        "minimap" ==> createObj [ "enabled" ==> false ]
        "scrollBeyondLastLine" ==> false
        "fontSize" ==> 12
        "tabSize" ==> 2 ])

/// 入れ物の実寸に合わせ直す。
///
/// **`automaticLayout` は入れ物の大きさが変わったときに動く。**
/// 幅も高さも 0 のところ（`display: none` のモーダルなど）で `create` すると、
/// 見えるようになるまで 0 のままのことがある。開いた側が 1 回 これを呼べば直る。
/// 大きさを引数で渡さないのは、渡すとそこで固定されて追随しなくなるから。
let relayout () = if not (isNull editor) then layoutToContainer editor

/// 印の持ち主。**1 つ に固定する** —— 名前が揺れると、前に付けた印を
/// 自分で消せなくなる。
/// 要素名と綴りを分ける。Fable 側に要素名を書かない線を、門が見ている
let private markerOwner = "fsbulletml2"

[<Emit("globalThis.monaco.editor.setModelMarkers($0.getModel(), $1, $2)")>]
let private setMarkers (editor: obj) (owner: string) (markers: obj[]) : unit = jsNative

[<Emit("$0.getModel().getLineMaxColumn($1)")>]
let private lineMaxColumn (editor: obj) (line: int) : int = jsNative

[<Emit("$0.getModel().getLineCount()")>]
let private lineCount (editor: obj) : int = jsNative

[<Emit("$0.getModel().onDidChangeContent($1)")>]
let private onChange (editor: obj) (cb: unit -> unit) : unit = jsNative

/// 波線 1 本 ぶん。行・桁 は 1 起点。**`endColumn` が 0 なら行末まで。**
type Mark =
  { Line: int
    Column: int
    EndColumn: int
    Message: string }

/// 波線を引き直す。**渡した並びで丸ごと置き換える** ——
/// 足す口にすると、前に付けた印が残って場所が嘘になる。
///
/// 終わりが分からない印（`EndColumn = 0`）は行末まで伸ばす。1 文字 だけだと
/// 細すぎて見つけられないし、桁が指すのは「そこから先が読めない」なので。
/// 行末が桁と同じ（行の終わり）なら 1 文字 ぶん伸ばす。
///
/// 行が本文より下を指していたら最後の行に丸める。**範囲が本文の外へ出ると
/// Monaco は何も描かない** —— 出ないのを「印が付いていない」と読むことになる
let markAll (marks: Mark list) =
  if not (isNull editor) then
    let items =
      marks
      |> List.map (fun mk ->
          let line = max 1 (min mk.Line (lineCount editor))
          let maxCol = lineMaxColumn editor line
          let startCol = max 1 (min mk.Column maxCol)
          let wanted = if mk.EndColumn > 0 then min mk.EndColumn maxCol else maxCol
          let endCol = if wanted > startCol then wanted else startCol + 1
          createObj [
            "startLineNumber" ==> line
            "endLineNumber" ==> line
            "startColumn" ==> startCol
            "endColumn" ==> endCol
            "message" ==> mk.Message
            // 8 = Error。Monaco 側の enum なので数で書く
            "severity" ==> 8 ])
      |> List.toArray
    setMarkers editor markerOwner items

let clearMarks () = if not (isNull editor) then setMarkers editor markerOwner [||]

/// 本文が変わったら呼ぶ。**印は文字に追随しない** ——
/// 1 文字 打った時点で場所が嘘になるので、そこで消す側が要る
let onContentChanged (handler: unit -> unit) =
  if not (isNull editor) then onChange editor handler

let getValue () : string = if isNull editor then "" else getVal editor
let setValue (text: string) = if not (isNull editor) then setVal editor text
let setLanguage (language: string) = if not (isNull editor) then setModelLanguage editor language

/// 候補を出す口。**候補の中身はここで決めない** —— 呼ぶ側（言語モジュール）が
/// 本文とカーソルの位置から並びを作る。Monaco 側の形（range を持つこと）だけ
/// ここが知っている。
///
/// range を省くと版によっては候補が出ないので、必ず「いま打っている語」に当てる。
///
/// 本文は毎回 `getValue` で取る。**rAF の中ではないので写しても構わない** ——
/// 呼ばれるのは人がキーを打ったときだけ
let registerCompletionProvider
  (language: string)
  (triggerCharacters: string list)
  (complete: string -> int -> FsBulletML2.LanguageService.SourceLanguage.Completion list)
  =
  let provide (model: obj) (position: obj) : obj =
    let column: int = position?column
    let items =
      complete (getVal model) (offsetAt model position)
      |> List.map (fun c ->
           // **置き換える幅は候補の側が決める。** `getWordUntilPosition` に
           // 任せると、語に入らない字（`$`）を持つ候補で二重に入る
           let range =
             createObj [
               "startLineNumber" ==> position?lineNumber
               "endLineNumber" ==> position?lineNumber
               "startColumn" ==> max 1 (column - c.Replace)
               "endColumn" ==> column ]
           let o =
             createObj [
               "label" ==> c.Label
               // 1 = Keyword。数を名前で書けないのは Monaco 側の enum なので
               "kind" ==> 1
               "insertText" ==> c.Insert
               "range" ==> range ]
           // 4 = InsertAsSnippet。`$0` をカーソルの置き場として読ませる
           if c.Snippet then o?insertTextRules <- 4
           o)
      |> List.toArray
    createObj [ "suggestions" ==> items ]

  registerCompletion language provide (List.toArray triggerCharacters)

/// カーソルの下に在るものの仕様を浮かせる口。**中身はここで決めない** ——
/// 呼ぶ側（言語モジュール）が markdown の字を組む。
///
/// **範囲を返さない。** 返すと Monaco がその範囲を枠で囲うが、範囲の出どころは
/// 言語のほうなので、渡すなら `tokenAt` の当たった範囲を持ち回ることになる。
/// いまは中身だけで足りている。
///
/// 何の上でもなければ `null`。**空の `contents` を返さない** ——
/// 空でも枠が浮くので、出ていないことと見分けがつかなくなる
let registerHoverProvider (language: string) (hover: string -> int -> string option) =
  let provide (model: obj) (position: obj) : obj =
    match hover (getVal model) (offsetAt model position) with
    | None -> null
    | Some markdown -> createObj [ "contents" ==> [| createObj [ "value" ==> markdown ] |] ]

  registerHover language provide

/// 名前を書き換える口。**書き換える場所を決めるのは言語モジュール** ——
/// ここが知っているのは Monaco の形（範囲と WorkspaceEdit）だけ。
///
/// Monaco は 2 段 で呼ぶ（0.56 で現物に当てて確かめた）——
///
///     resolveRenameLocation   { range, text }。text が入力欄の初期値
///     provideRenameEdits      { edits: [ { resource, versionId, textEdit } ] }
///
/// **1 段 目 で `null` を返してはいけない。** 返すと Monaco は自分で拾った
/// 「語」を範囲にして**入力欄を開いてしまう**（ここも現物で踏んだ）——
/// こちらは 2 段 目 で 0 件 を返すので何も起きないが、
/// **人からは「名前を入れたのに何も変わらない」に見える。**
///
/// 名前の上に居ないときは `{ rejectReason }` を返す。**理由が字で出る。**
let registerRenameProvider
  (language: string)
  (usages: string -> int -> FsBulletML2.LanguageService.SourceLanguage.Usage list)
  =
  let range (u: FsBulletML2.LanguageService.SourceLanguage.Usage) =
    createObj [
      "startLineNumber" ==> u.Line
      "endLineNumber" ==> u.Line
      "startColumn" ==> u.Column
      "endColumn" ==> u.EndColumn ]

  let resolve (model: obj) (position: obj) : obj =
    let line: int = position?lineNumber
    let column: int = position?column
    // **カーソルを覆う 1 つ を返す。** 先頭を返すと、参照の上で始めた rename が
    // 定義の側の範囲を書き換えることになる
    usages (getVal model) (offsetAt model position)
    |> List.tryFind (fun u -> u.Line = line && column >= u.Column && column <= u.EndColumn)
    |> function
       | None -> createObj [ "rejectReason" ==> "ここは名前ではない（label の値の上で押す）" ]
       | Some u -> createObj [ "range" ==> range u; "text" ==> u.Text ]

  let provide (model: obj) (position: obj) (newName: string) : obj =
    let edits =
      usages (getVal model) (offsetAt model position)
      |> List.map (fun u ->
           createObj [
             "resource" ==> modelUri model
             "versionId" ==> modelVersion model
             "textEdit" ==> createObj [ "range" ==> range u; "text" ==> newName ] ])
      |> List.toArray
    createObj [ "edits" ==> edits ]

  registerRename language resolve provide

/// 「こう直す」を出す口。**直し方を決めるのは言語モジュール** ——
/// ここが知っているのは Monaco の形（アクションと WorkspaceEdit）だけ。
///
/// **`context.markers` を見ない。** 波線は 1 文字 打った時点で消えるので、
/// marker を材料にすると **Apply の直後の窓でしか出ない**
/// （v1.3 の頭で現物に当てた）。言語モジュールが本文から数え直す。
///
/// **`dispose` を返す。** Monaco は返り値の後片付けを呼ぶ ——
/// 無いと、押した瞬間に落ちる。
///
/// 人が押すのは Ctrl+.。`editor.action.quickFix` という action は
/// `getSupportedActions()` に出てこない（動かすのは
/// `editor.contrib.codeActionController`）。**試験もそちらから叩く。**
let registerCodeActionProvider
  (language: string)
  (fixes: string -> int -> FsBulletML2.LanguageService.SourceLanguage.Fix list)
  =
  let provide (model: obj) (range: obj) (_context: obj) : obj =
    // 範囲の頭をカーソルとして読む。Monach は空の範囲（カーソルそのもの）で
    // 呼んでくるので、頭と尻は同じことが多い
    let position = createObj [ "lineNumber" ==> range?startLineNumber; "column" ==> range?startColumn ]
    let found = fixes (getVal model) (offsetAt model position)
    let actions =
      found
      |> List.map (fun f ->
           createObj [
             "title" ==> f.Title
             // Monaco 側の分類。`quickfix` は波線の直し方という意味
             "kind" ==> "quickfix"
             // **1 つ のときだけ「これ」と言う。** 2 つ 以上 で立てると、
             // Ctrl+. の一発適用がどちらかを勝手に選ぶ
             "isPreferred" ==> (found.Length = 1)
             "edit" ==> createObj [
               "edits" ==> [| createObj [
                 "resource" ==> modelUri model
                 "versionId" ==> modelVersion model
                 "textEdit" ==> createObj [
                   "range" ==> createObj [
                     "startLineNumber" ==> f.Line
                     "endLineNumber" ==> f.Line
                     "startColumn" ==> f.Column
                     "endColumn" ==> f.EndColumn ]
                   "text" ==> f.Text ] ] |] ] ])
      |> List.toArray
    createObj [ "actions" ==> actions; "dispose" ==> (fun () -> ()) ]

  registerCodeAction language provide
