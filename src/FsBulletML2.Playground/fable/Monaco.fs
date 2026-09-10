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

// 配色は editor ごとではなく **Monaco 全体**に掛かる。だから引数に editor が要らない
[<Emit("globalThis.monaco.editor.setTheme($0)")>]
let private applyTheme (id: string) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerCompletionItemProvider($0, { triggerCharacters: $2, provideCompletionItems: $1 })")>]
let private registerCompletion (language: string) (fn: obj -> obj -> obj) (triggers: string[]) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerHoverProvider($0, { provideHover: $1 })")>]
let private registerHover (language: string) (fn: obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerDocumentSymbolProvider($0, { provideDocumentSymbols: $1 })")>]
let private registerSymbols (language: string) (fn: obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerFoldingRangeProvider($0, { provideFoldingRanges: $1 })")>]
let private registerFolding (language: string) (fn: obj -> obj) : unit = jsNative

// 囲む要素を選ぶ（v2.4.5）。**位置ごとに 1 本 の列**を返す ——
// 位置は配列で来る（複数カーソル）ので、返すのも配列の配列
[<Emit("globalThis.monaco.languages.registerSelectionRangeProvider($0, { provideSelectionRanges: $1 })")>]
let private registerSelectionRanges (language: string) (fn: obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.editor.addKeybindingRule({ keybinding: globalThis.monaco.KeyMod.Alt | globalThis.monaco.KeyCode[$0], command: $1 })")>]
let private addAltBinding (keyName: string) (command: string) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerCodeActionProvider($0, { provideCodeActions: $1 })")>]
let private registerCodeAction (language: string) (fn: obj -> obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerDefinitionProvider($0, { provideDefinition: $1 })")>]
let private registerDefinition (language: string) (fn: obj -> obj -> obj) : unit = jsNative

[<Emit("globalThis.monaco.languages.registerReferenceProvider($0, { provideReferences: $1 })")>]
let private registerReferences (language: string) (fn: obj -> obj -> obj -> obj) : unit = jsNative

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

/// エディタの配色。**Monaco が素で持っているものだけ。**
///
/// 名前は Monaco 固有なので、ここ以外 に書かない（器はどのエディタに
/// 載るかを知らない）。表を持つ理由は下の `setTheme` に書いてある。
type Theme =
  { /// Monaco へ渡す字
    Id: string
    /// 人へ見せる名。**`Id` から作れない** —— `vs` が明るいほうで、
    /// `vs-dark` が暗いほう、という対応は字から読めない
    Label: string }

/// 素で在る 4 本。**測って決めた**（0.56 で当てた）——
/// `vs` / `vs-dark` / `hc-black` / `hc-light` 以外 は 1 つ も効かなかった。
///
/// 並びは 暗い / 明るい を対にして、高コントラスト（`HC`）を後ろへ。
///
/// **名は短く。** `select` の幅はいちばん長い選択肢で決まり、そこが太ると
/// プルダウンの列が折り返して欄の縦を食う。「明るい（高コントラスト）」と
/// 書くと 193px、`HC` に詰めると 97px、素の id なら 83px ——
/// **素の id は 14px しか縮まず、折り返す幅も同じ**だったので、読めるほうを取った。
let themes =
  [ { Id = "vs-dark"; Label = "暗い" }
    { Id = "vs"; Label = "明るい" }
    { Id = "hc-black"; Label = "暗い HC" }
    { Id = "hc-light"; Label = "明るい HC" } ]

/// 起動時の配色。**`create` と プルダウンの初期値が同じ 1 本 を引く** ——
/// 2 か所 に書くと、片方 だけ変えても落ちず、
/// 「プルダウンは暗いと言っているのに明るい」になる
let defaultTheme = "vs-dark"

/// 配色を替える。**知らない字なら false を返して何もしない。**
///
/// `monaco.editor.setTheme` は知らない名前で**落ちない** ——
/// 黙って `vs`（明るいほう）に倒れる。`dark` も `VS-DARK` も、
/// 綴りの間違いも全部 そこへ行く（測った）。
/// だから当てるのはこちらで、渡すのは表に在る字だけにする。
let setTheme (id: string) : bool =
  if themes |> List.exists (fun t -> t.Id = id) then
    applyTheme id
    true
  else false

let create (hostId: string) (language: string) (initial: string) =
  let host = document.getElementById hostId
  if isNull host then failwith ("要素が無い: " + hostId)
  editor <-
    createEditor
      host
      (createObj [
        "value" ==> initial
        "language" ==> language
        "theme" ==> defaultTheme
        // 入れ物の大きさへの追随は Monaco 自身に任せる。
        // **自前の ResizeObserver に替えかけたが戻した** —— 背面タブでは
        // どちらも発火しない（layout / paint の流れが回らない）ので、
        // 「自前のほうが確かめやすい」は成り立たなかった。
        // 動く実績のある側を残し、測れないものを増やさない
        "automaticLayout" ==> true
        // 右の縮小図。**素の値のまま出す** —— VS Code と同じ見え方にする
        // （字を描く / つまみは重ねたときだけ / 幅は欄に比例）。
        //
        // 幅を持っていくが、測ると欄の 1 割・上限 80px だった。
        // 弾幕は入れ子が深くて縦に長いので、縮小図のほうが効く
        "minimap" ==> createObj [ "enabled" ==> true ]
        "scrollBeyondLastLine" ==> false
        // Ctrl を押しながらホイールで字の大きさを変える（VS Code と同じ）。
        // **拡大率は載っている弾幕の一部ではない**ので、Apply でも Reset でも
        // プルダウンでも戻さない（速さ・配色と同じ扱い）
        "mouseWheelZoom" ==> true
        // 起点の大きさ。**ここが Ctrl+ホイール の基準**で、
        // 拡大率はここへ掛かる（`getOption fontSize` は掛けた後 の値を返す）
        "fontSize" ==> 12
        "tabSize" ==> 2 ])

/// 入れ物の実寸に合わせ直す。
///
/// **`automaticLayout` は入れ物の大きさが変わったときに動く。**
/// 幅も高さも 0 のところ（`display: none` のモーダルなど）で `create` すると、
/// 見えるようになるまで 0 のままのことがある。開いた側が 1 回 これを呼べば直る。
/// 大きさを引数で渡さないのは、渡すとそこで固定されて追随しなくなるから。
let relayout () = if not (isNull editor) then layoutToContainer editor

// --- 並べて見る（v2.1）------------------------------------------------------

[<Emit("globalThis.monaco.editor.createDiffEditor($0, $1)")>]
let private createDiff (host: obj) (opts: obj) : obj = jsNative

[<Emit("globalThis.monaco.editor.createModel($0, $1)")>]
let private createModel (text: string) (language: string) : obj = jsNative

[<Emit("$0.setModel({ original: $1, modified: $2 })")>]
let private setDiffModel (diff: obj) (original: obj) (modified: obj) : unit = jsNative

[<Emit("$0.dispose()")>]
let private disposeOf (x: obj) : unit = jsNative

let mutable private diff: obj = null
let mutable private diffOriginal: obj = null
let mutable private diffModified: obj = null

/// 並べて出しているか。**呼ぶ側はこれで「戻す」を決める**
let diffShown () = not (isNull diff)

/// 作った物を全部 捨てる。**model も捨てる** ——
/// エディタだけ捨てると、model が `monaco.editor.getModels()` に残り続け、
/// 開き直すたびに増える（言語 id で登録した provider は残った model にも効く）
let hideDiff () =
  if not (isNull diff) then
    disposeOf diff
    diff <- null
  if not (isNull diffOriginal) then
    disposeOf diffOriginal
    diffOriginal <- null
  if not (isNull diffModified) then
    disposeOf diffModified
    diffModified <- null

/// 2 つ の本文を並べて出す。**両側 読むだけ。**
///
/// 直すのは元のエディタでやる —— ここは「何が違うか」を見る道具で、
/// 2 か所 で編集できると「どちらが本文か」が人にも実装にも曖昧になる。
///
/// **model の言語 id は元のエディタと同じものを渡す。** provider は
/// `monaco.languages.register*Provider(language, ...)` に**言語 id 単位**で
/// 登録してあるので、それだけで両側に効く（測って確かめた）。
/// ただし readOnly の側で Monaco が起こすのは hover と定義へ移動だけで、
/// **補完・Quick Fix・rename は呼ばれもしない** —— ここでは読むだけなので、
/// それがちょうどよい。
///
/// **言語は左右 別に受け取る。** 同じ表記どうしを比べるとは限らない ——
/// 別の表記へ変換したものを左に置く道が在る（そちらは差分の色が
/// 意味を持たず、並べて読むための出し方）。
///
/// 開き直すたびに作り直す。**使い回さない** —— 言語も本文も毎回 変わりうるので、
/// 使い回すと「前の言語のまま」を作れてしまう。
let showDiff
  (hostId: string)
  (originalLanguage: string)
  (original: string)
  (modifiedLanguage: string)
  (modified: string)
  =
  let host = document.getElementById hostId
  if isNull host then failwith ("要素が無い: " + hostId)
  hideDiff ()
  diffOriginal <- createModel original originalLanguage
  diffModified <- createModel modified modifiedLanguage
  diff <-
    createDiff
      host
      (createObj [
        "automaticLayout" ==> true
        // 左右 に並べる。上下 に積むと、弾幕は縦に長いので同じ行が離れる
        "renderSideBySide" ==> true
        "readOnly" ==> true
        "originalEditable" ==> false
        // 元のエディタと同じ見え方に揃える（`create` の但し書きと同じ理由）
        "minimap" ==> createObj [ "enabled" ==> true ]
        "scrollBeyondLastLine" ==> false
        "mouseWheelZoom" ==> true
        "fontSize" ==> 12
        "tabSize" ==> 2 ])
  setDiffModel diff diffOriginal diffModified

/// 印の持ち主。**1 つ に固定する** —— 名前が揺れると、前に付けた印を
/// 自分で消せなくなる。
/// 要素名と綴りを分ける。Fable 側に要素名を書かない線を、門が見ている
let private markerOwner = "fsbulletml2"

[<Emit("globalThis.monaco.editor.setModelMarkers($0.getModel(), $1, $2)")>]
let private setMarkers (editor: obj) (owner: string) (markers: obj[]) : unit = jsNative

/// 走っている場所の印（v3.1 の段 4）。**波線と同じ仕組みにしない。**
///
/// `setModelMarkers` は owner ごとに丸ごと置き換わるので、そちらへ混ぜると
/// 打鍵ごとの波線と取り合う（v2.3 で 1 度 分けた）。飾りは自分の入れ物を
/// 持つので、**ほかの印を 1 つ も動かさずに付け外しできる。**
///
/// **入れ物は 1 つ だけ作って持ち回る** —— 毎回 作ると、前の入れ物が
/// 消えないまま残って印が積み上がる
[<Emit("$0.createDecorationsCollection([])")>]
let private newDecorations (editor: obj) : obj = jsNative

[<Emit("$0.set($1)")>]
let private setDecorations (collection: obj) (items: obj[]) : unit = jsNative

[<Emit("$0.getModel().getPositionAt($1)")>]
let private positionAt (editor: obj) (offset: int) : obj = jsNative

[<Emit("$0.revealLineInCenterIfOutsideViewport($1)")>]
let private revealLine (editor: obj) (line: int) : unit = jsNative

let mutable private lit: obj = null
/// 撃った場所の入れ物（v3.2）。**再開点と別に持つ** ——
/// 1 つ にまとめると、毎コマ 書き換わる側がもう片方 を消す
let mutable private origin: obj = null
/// 撃った数の入れ物（v3.3 の段 1）。**3 つ 目。**
///
/// 上の 2 つ と混ぜられない —— あちらは 1 か所 ずつで毎コマ 書き換わり、
/// こちらは何か所 も同時に出て、間引いて書き換わる
let mutable private tallyDeco: obj = null

[<Emit("$0.getModel().getLineMaxColumn($1)")>]
let private lineMaxColumn (editor: obj) (line: int) : int = jsNative

[<Emit("$0.getModel().getLineCount()")>]
let private lineCount (editor: obj) : int = jsNative

[<Emit("$0.getModel().onDidChangeContent($1)")>]
let private onChange (editor: obj) (cb: unit -> unit) : unit = jsNative

/// 走っている場所を光らせる。**範囲は本文の添字**（0 起点、`stop` は含まない）。
/// 戻りは光らせた行（1 起点）。**付けられなければ -1。**
///
/// **勝手にスクロールしない。** 見ている場所を動かされると、1 コマ 進むたびに
/// 画面が飛んで、字を読んでいられない。
///
/// **代わりに、画面の外でも在り処が分かる形にする** ——
/// 下地は見えている行にしか描かれない（Monaco は見えている行しか組まない）ので、
/// スクロールバーの帯（`overviewRuler`）とミニマップにも印を置き、
/// **行番号は呼ぶ側が帯に出す。** 押したのに何も出ないように見えるのが
/// いちばん困る —— 実際に踏んだ（102 行目 に付いて、画面は 69 行目 まで）
let highlight (startOffset: int) (stopOffset: int) : int =
  if isNull editor then -1
  else
    if isNull lit then lit <- newDecorations editor
    let a = positionAt editor startOffset
    let b = positionAt editor stopOffset
    setDecorations lit [|
      createObj [
        "range" ==> createObj [
          "startLineNumber" ==> a?lineNumber
          "startColumn" ==> a?column
          "endLineNumber" ==> b?lineNumber
          "endColumn" ==> b?column ]
        "options" ==> createObj [
          "className" ==> "running-node"
          // **数で書く**（あちらの enum なので）—— 7 = Full / 1 = Inline。
          // 色をここに書くのは、Monaco が css の class を読まないため。
          //
          // **配色では切り替わらない**（下地は css なので付いていける）——
          // 明るい地と暗い地の両方 で見える 1 本 にする
          "overviewRuler" ==> createObj [ "color" ==> "#d98f00"; "position" ==> 7 ]
          "minimap" ==> createObj [ "color" ==> "#d98f00"; "position" ==> 1 ] ] ] |]
    unbox<int> a?lineNumber

/// 印を下ろす。**入れ物は残す**（次に付けるときに作り直さない）
let clearHighlight () =
  if not (isNull lit) then setDecorations lit [||]

/// 撃った場所を光らせる（v3.2）。戻りは開き札の行（1 起点）。**無ければ -1。**
///
/// **範囲は 2 つ 受け取る** —— 開き札と閉じ札。`fire` は撃たれる弾の一生を
/// 抱えるので、要素まるごとだと中央 5 行 / 最大 71 行 が染まり、
/// 追っている弾の現在地（黄）を飲む。**だから両端だけを光らせて、中身は残す。**
///
/// **2 つ が同じなら 1 枚 にする。** 下地は半透明なので、重ねると色が濃くなって
/// 別の意味に見える（閉じ札を持たない表記では同じ範囲が渡ってくる）。
///
/// **こちらは字へ飛ぶ。** 再開点（段 4）は毎コマ 変わるので飛ばないが、
/// 撃った場所は**弾を選んだ時点で決まって動かない** —— 押した人が
/// 見に行きたい場所なので、画面の外なら 1 回 だけ寄せる
/// （`...IfOutsideViewport` なので、見えているときは動かさない）
let highlightOrigin (openStart: int) (openStop: int) (closeStart: int) (closeStop: int) : int =
  if isNull editor then -1
  else
    if isNull origin then origin <- newDecorations editor
    let deco (fromOffset: int) (toOffset: int) =
      let a = positionAt editor fromOffset
      let b = positionAt editor toOffset
      createObj [
        "range" ==> createObj [
          "startLineNumber" ==> a?lineNumber
          "startColumn" ==> a?column
          "endLineNumber" ==> b?lineNumber
          "endColumn" ==> b?column ]
        "options" ==> createObj [
          "className" ==> "fired-node"
          "overviewRuler" ==> createObj [ "color" ==> "#1aa06a"; "position" ==> 7 ]
          "minimap" ==> createObj [ "color" ==> "#1aa06a"; "position" ==> 1 ] ] ]
    let items =
      if closeStart = openStart && closeStop = openStop then [| deco openStart openStop |]
      else [| deco openStart openStop; deco closeStart closeStop |]
    setDecorations origin items
    let line = unbox<int> (positionAt editor openStart)?lineNumber
    revealLine editor line
    line

let clearOrigin () =
  if not (isNull origin) then setDecorations origin [||]

/// 撃った数を字の右へ出す（v3.3 の段 1）。`marks` は
/// (範囲の頭, 範囲の尻, 添える字) の並び。**添える字は呼ぶ側が作る** ——
/// 桁区切りは場所ごとの決まりなので、器が決める話ではない。
///
/// **下地をもう 1 色 増やさない。** 走っている場所（黄）と撃った場所（緑）が
/// もう在るので、3 色 目 は「どれが何か」を覚えられなくなる ——
/// **数そのものを字の後ろに置けば、色を使わずに済む。**
///
/// **飛ばない。** 何か所 も同時に出るので、寄せる先が決まらない
let showTally (marks: (int * int * string)[]) : unit =
  if isNull editor then ()
  else
    if isNull tallyDeco then tallyDeco <- newDecorations editor
    let items =
      marks
      |> Array.map (fun (fromOffset, toOffset, text) ->
          let a = positionAt editor fromOffset
          let b = positionAt editor toOffset
          createObj [
            "range" ==> createObj [
              "startLineNumber" ==> a?lineNumber
              "startColumn" ==> a?column
              "endLineNumber" ==> b?lineNumber
              "endColumn" ==> b?column ]
            "options" ==> createObj [
              // **`after` は範囲の後ろに字を足す**（本文は 1 文字 も変わらない）。
              // フィールド名は `content`（`contentText` ではない）
              "after" ==> createObj [
                "content" ==> text
                "inlineClassName" ==> "tally-mark" ] ] ])
    setDecorations tallyDeco items

let clearTally () =
  if not (isNull tallyDeco) then setDecorations tallyDeco [||]

/// 波線 1 本 ぶん。行・桁 は 1 起点。**`endColumn` が 0 なら行末まで。**
type Mark =
  { Line: int
    Column: int
    EndColumn: int
    Message: string
    /// Monaco の severity。**数で書く**（あちらの enum なので）——
    /// 8 = Error / 4 = Warning / 2 = Info / 1 = Hint。
    ///
    /// **強さを持つのはこの型から。** v2.3 で意味の層が入って、
    /// 「走らない」（Error）と「走りには影響しない」（Info）を
    /// 同じ画に並べることになった
    Severity: int }

/// 強さの名前。**数を呼ぶ側に書かせない** —— 書かせると、
/// 4 と 8 を取り違えても型が通る（どちらも int）
module Severity =
  [<Literal>]
  let Error = 8

  [<Literal>]
  let Warning = 4

  [<Literal>]
  let Info = 2

/// **印の持ち主を 2 つ に分ける。**
///
/// Monaco の `setModelMarkers` は owner ごとに丸ごと置き換えるので、
/// 1 つ に混ぜると**片方 を書くたびにもう片方 が消える** ——
/// Apply の波線（往復して出る）と、打鍵ごとの波線（字から出る）は
/// 出る間隔が桁で違うので、必ずどちらかが消える。
let private semanticOwner = markerOwner + "-semantic"

/// 波線を引き直す。**渡した並びで丸ごと置き換える** ——
/// 足す口にすると、前に付けた印が残って場所が嘘になる。
///
/// 終わりが分からない印（`EndColumn = 0`）は行末まで伸ばす。1 文字 だけだと
/// 細すぎて見つけられないし、桁が指すのは「そこから先が読めない」なので。
/// 行末が桁と同じ（行の終わり）なら 1 文字 ぶん伸ばす。
///
/// 行が本文より下を指していたら最後の行に丸める。**範囲が本文の外へ出ると
/// Monaco は何も描かない** —— 出ないのを「印が付いていない」と読むことになる
let private markWith (owner: string) (marks: Mark list) =
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
            "severity" ==> mk.Severity ])
      |> List.toArray
    setMarkers editor owner items

let markAll (marks: Mark list) = markWith markerOwner marks

let clearMarks () = if not (isNull editor) then setMarkers editor markerOwner [||]

/// 意味の層の波線（v2.3）。**Apply の波線とは持ち主が別。**
///
/// こちらは字から出るので、**打鍵ごとに引き直す**（WASM へ行かない）。
/// 引き直しは丸ごと置き換えなので、消す口が別に要らない ——
/// 空を渡せば消える
let markSemantic (marks: Mark list) = markWith semanticOwner marks

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
               // 1 = Keyword、27 = Snippet。数を名前で書けないのは Monaco 側の
               // enum なので。**雛形は分ける**（v2.6）—— 候補の並びで「形」と
               // 「置ける要素」が同じ顔をしていると、打ち始めた人に区別が付かない
               "kind" ==> (if c.IsFrame then 27 else 1)
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

/// 定義へ移動（F12）と、参照を出す（Shift+F12）。
///
/// **どちらも `Usages` の 1 本 の上に載る。** 名前が書いてある場所は
/// rename と同じ並びで、**違うのはそこから何を選ぶか**だけ ——
///
///     定義へ移動   `IsDefinition` の側だけ
///     参照         全部（定義も参照も）
///
/// **口を足していない。** `Usage` に 1 欄 足りた ——
/// 語彙から引いた対の定義側と参照側は要素名が 1 つ も重ならないので、
/// どちら側かは札の名前で言える（測ってから決めた）。
///
/// **定義が 0 件 のことが在る。** 参照だけ在って定義が無い本文はふつうに書ける
/// （そこは波線と Quick Fix の担当）—— そのときは空を返す。
/// **カーソルの位置へ飛ばさない** —— 飛ばすと「定義が在った」に見える。
///
/// Monaco が要る形は `{ uri, range }`（の並び）。`uri` は同じ本文なので
/// いま開いているモデルのもの。
let registerNavigationProviders
  (language: string)
  (usages: string -> int -> FsBulletML2.LanguageService.SourceLanguage.Usage list)
  =
  let location (model: obj) (u: FsBulletML2.LanguageService.SourceLanguage.Usage) =
    createObj [
      "uri" ==> modelUri model
      "range" ==>
        createObj [
          "startLineNumber" ==> u.Line
          "endLineNumber" ==> u.Line
          "startColumn" ==> u.Column
          "endColumn" ==> u.EndColumn ] ]

  let definition (model: obj) (position: obj) : obj =
    usages (getVal model) (offsetAt model position)
    |> List.filter (fun u -> u.IsDefinition)
    |> List.map (location model)
    |> List.toArray
    |> box

  let references (model: obj) (position: obj) (_context: obj) : obj =
    usages (getVal model) (offsetAt model position)
    |> List.map (location model)
    |> List.toArray
    |> box

  registerDefinition language definition
  registerReferences language references

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

/// アウトライン（Ctrl+Shift+O）と折りたたみ（v2.4）。
///
/// **どちらも同じ 1 本 の上に載る**（`Outline.build`）——
/// 出す先が違うだけで、材料は「名前・深さ・行の範囲」の同じ並び。
/// 別々に数えると、片方 だけが古い形を返しても**どちらも単独では正しく見える。**
///
/// ### 木にして渡す
///
/// Monaco の symbol provider は入れ子を `children` で受ける。器が返すのは
/// **深さの付いた平らな並び**なので、ここで積み直す ——
/// 器の側で木にしないのは、**2 runtime で突き合わせているのが並びのほう**
/// だから（`Outline.describe`）。木が要るのはここ 1 か所 だけ。
///
/// ### 折りたたみは 1 行 のものを出さない
///
/// Monaco は `start = end` の範囲を黙って捨てるが、**捨てられたことは
/// 出ない** —— こちらで落としておくほうが、数が合わないときに気づける。
let registerStructureProviders
  (language: string)
  (outline: string -> FsBulletML2.LanguageService.Outline.Node list)
  =
  let symbols (model: obj) : obj =
    let nodes = outline (getVal model) |> List.toArray
    // **深さの並びから木を積む。** 親は「1 つ 手前 で、自分より浅いもの」
    let childrenOf = Array.map (fun _ -> ResizeArray<obj>()) nodes
    let roots = ResizeArray<obj>()
    // 後ろから作ると子が先に揃う
    let made = Array.zeroCreate<obj> nodes.Length
    for i in nodes.Length - 1 .. -1 .. 0 do
      let n = nodes.[i]
      let range =
        createObj [
          "startLineNumber" ==> n.Line
          "startColumn" ==> 1
          "endLineNumber" ==> n.EndLine
          // **行末まで。** 桁を持たないので大きい数を渡す（Monaco が丸める）
          "endColumn" ==> 1000 ]
      made.[i] <-
        createObj [
          "name" ==> (if n.Detail = "" then n.Name else n.Name + " " + n.Detail)
          "detail" ==> n.Detail
          // Monaco の SymbolKind。5 = Class 相当（要素を 1 つ の塊として出す）
          "kind" ==> 5
          "tags" ==> [||]
          "range" ==> range
          "selectionRange" ==>
            createObj [
              "startLineNumber" ==> n.Line
              "startColumn" ==> n.Column
              "endLineNumber" ==> n.Line
              "endColumn" ==> n.EndColumn ]
          "children" ==> (childrenOf.[i] |> Seq.toArray) ]
      // 親を探す。**手前 に向かって、自分より浅い最初のもの**
      let mutable p = i - 1
      while p >= 0 && nodes.[p].Depth >= n.Depth do
        p <- p - 1
      if p >= 0 then childrenOf.[p].Insert(0, made.[i]) else roots.Insert(0, made.[i])
    roots |> Seq.toArray |> box

  let folding (model: obj) : obj =
    outline (getVal model)
    // **1 行 のものは出さない**（Monaco が黙って捨てる側を、こちらで落とす）
    |> List.filter (fun n -> n.EndLine > n.Line)
    |> List.map (fun n ->
         createObj [ "start" ==> n.Line; "end" ==> n.EndLine ])
    |> List.toArray
    |> box

  // 囲む要素を選ぶ（v2.4.5）。**同じ木の 3 つ 目 の出し先。**
  //
  // Monaco は「広げる」を押すたび、この列を 1 段 ずつ外へ進む。
  // 列は器が作る（`Outline.enclosing`）—— **2 runtime で突き合わせている**
  // ので、内から外への並びがブラウザ側だけ違うことにならない。
  //
  // **始まりは名前の桁から。** 行頭からにすると字下げまで選ぶ ——
  // 版の頭で数えたら、1 行 に収まる要素の字下げは xml で平均 18 字 だった。
  // 終わりは行末（`1000`）で、これは折りたたみと同じ扱い。
  let selectionRanges (model: obj) (positions: obj) : obj =
    let nodes = outline (getVal model)
    positions
    |> unbox<obj array>
    |> Array.map (fun p ->
         FsBulletML2.LanguageService.Outline.enclosing nodes (unbox<int> p?lineNumber)
         |> List.map (fun n ->
              createObj [
                "range" ==> createObj [
                  "startLineNumber" ==> n.Line
                  "startColumn" ==> n.Column
                  "endLineNumber" ==> n.EndLine
                  "endColumn" ==> 1000 ] ])
         |> List.toArray)
    |> box

  registerSymbols language symbols
  registerFolding language folding
  registerSelectionRanges language selectionRanges

/// アウトラインを **Alt+O でも**出せるようにする（v2.4）。
///
/// Monaco が素で持っている割り当ては <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>O</kbd>。
/// **その組み合わせには、ページの外に持ち主が居ることがある。**
///
///     OS の常駐ソフト   実際に出た。AMD Software の性能オーバーレイ。
///                      **ブラウザにすら届かない**
///     ブラウザ          Chrome の「ブックマーク マネージャ」も同じ組み合わせ
///
/// 取られると keydown がページに来ないので、**エディタ側は 1 行 も走らない**
/// （押した人からは「効いていない」と区別が付かない）。
///
/// **消さずに足す。** Ctrl+Shift+O が通る環境では、そちらも今までどおり効く
/// —— どちらが通るかは機械の側が決めるので、こちらからは選べない。
///
/// **`language` を取らない。** 割り当ては言語 id ではなくエディタに付くので、
/// 表記ごとに呼ぶと同じ規則が 4 本 積み上がる
let addOutlineAltKey () = addAltBinding "KeyO" "editor.action.quickOutline"
