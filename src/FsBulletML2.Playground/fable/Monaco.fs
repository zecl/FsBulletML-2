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

[<Emit("globalThis.monaco.languages.registerCompletionItemProvider($0, { provideCompletionItems: $1 })")>]
let private registerCompletion (language: string) (fn: obj -> obj -> obj) : unit = jsNative

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
let registerCompletionProvider (language: string) (complete: string -> int -> string list) =
  let provide (model: obj) (position: obj) : obj =
    let w = wordUntil model position
    let range =
      createObj [
        "startLineNumber" ==> position?lineNumber
        "endLineNumber" ==> position?lineNumber
        "startColumn" ==> w?startColumn
        "endColumn" ==> w?endColumn ]
    let items =
      complete (getVal model) (offsetAt model position)
      |> List.map (fun label ->
           createObj [
             "label" ==> label
             // 1 = Keyword。数を名前で書けないのは Monaco 側の enum なので
             "kind" ==> 1
             "insertText" ==> label
             "range" ==> range ])
      |> List.toArray
    createObj [ "suggestions" ==> items ]

  registerCompletion language provide
