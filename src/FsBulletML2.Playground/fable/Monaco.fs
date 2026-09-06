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
  (complete: string -> int -> FsBulletML2.Playground.SourceLanguage.Completion list)
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
