module FsBulletML2.Playground.Js

open Fable.Core
open Fable.Core.JsInterop
open Browser
open Browser.Types
open FsBulletML2.LanguageService
open FsBulletML2.LanguageService.SourceLanguage

[<Emit("$0[$1]")>]
let private jsItem (arr: obj) (i: int) : obj = jsNative

[<Emit("$0[$1]")>]
let private f32 (arr: obj) (i: int) : float = jsNative

[<Emit("$0.subarray($1, $2)")>]
let private subarray (arr: obj) (start: int) (end': int) : obj = jsNative

[<Emit("$0.invokeMethod($1)")>]
let private invoke0 (dn: obj) (name: string) : obj = jsNative

[<Emit("$0.invokeMethod($1, $2, $3, $4)")>]
let private invokeStep (dn: obj) (name: string) (t: float) (x: float) (y: float) : obj = jsNative

[<Emit("$0.invokeMethodAsync($1)")>]
let private invokeAsync0 (dn: obj) (name: string) : obj = jsNative

[<Emit("$0.invokeMethodAsync($1, $2)")>]
let private invokeAsync1 (dn: obj) (name: string) (arg: obj) : obj = jsNative

[<Emit("$0.invokeMethodAsync($1, $2, $3)")>]
let private invokeAsync2 (dn: obj) (name: string) (a: obj) (b: obj) : obj = jsNative

[<Emit("$0.then($1).catch($2)")>]
let private thenCatch (p: obj) (ok: obj -> unit) (err: obj -> unit) : unit = jsNative

[<Emit("$0.getContext('2d', { alpha: false })")>]
let private getCtx (c: HTMLCanvasElement) : CanvasRenderingContext2D = jsNative

[<Emit("new FileReader()")>]
let private newFileReader () : FileReader = jsNative

[<Emit("globalThis.getDotnetRuntime && globalThis.getDotnetRuntime(0)")>]
let private runtime () : obj = jsNative

// html は `autostart="false"` で読み込むだけ。**起こすのはこちら** ——
// html にロジックを置くと、そこだけ検査も型も掛からない
[<Emit("globalThis.Blazor.start()")>]
let private blazorStart () : obj = jsNative

// Error は string にすると [object Object] になる
[<Emit("($0 && $0.message) ? $0.message : String($0)")>]
let private errText (e: obj) : string = jsNative

// Apply の戻りだけ JSON。**空文字を成功の印にしない**ため
[<Emit("JSON.parse($0)")>]
let private jsonParse (s: string) : obj = jsNative

// dialog は素で Esc と背景を持っている。**自前で被せを作らない**
[<Emit("$0.showModal()")>]
let private showModal (dialog: obj) : unit = jsNative

[<Emit("$0.close()")>]
let private closeDialog (dialog: obj) : unit = jsNative

let private el (id: string) = document.getElementById id

let private setError (msg: string) =
  let e = el "loop-error"
  if not (isNull e) then e.textContent <- msg

let private heapF32 () : obj =
  let rt = runtime ()
  if isNull rt then failwith "WASM heap が見えない"
  let fn: obj = rt?localHeapViewF32
  if jsTypeof fn <> "function" then failwith "WASM heap が見えない"
  rt?localHeapViewF32 ()

/// Bolero の onclick / onReady と同じ名前で出す。
[<AttachMembers>]
type Playground() as self =
  let mutable playerX = 240.
  let mutable playerY = 600.
  let mutable running = false
  let mutable dotNet: obj = null
  let mutable frames = 0
  let mutable t0 = 0.
  let mutable lastN = -1
  let mutable canvas: HTMLCanvasElement = null
  let mutable canvasCtx: CanvasRenderingContext2D = null
  // host からもらう語彙。正本は Core の DTD.fs。**表記が変わっても同じ**
  let mutable vocabulary: Vocab = { Elements = []; Expressions = [] }
  // **同梱カタログは XML ダンプ。** `SelectPattern` が `ToIndentedXmlString` で
  // 焼くので、選んだら表記も XML に戻す。ほかの表記では焼けない ——
  // repo に**書く口が無い**（読むだけ。v0.9 の頭で測った）
  let catalogLanguage: ISourceLanguage = Languages.Xml.XmlLanguage(fun () -> vocabulary)
  // 登録されている表記。**並びは `SourceKind.all` と同じ** ——
  // プルダウンも `Open` の accept もここから作るので、順が意味を持つ。
  // **F# の CE だけ候補と hover を出さない**（語彙が DTD から引けないため。
  // 理由は `Languages/Fsharp.fs`）が、書いて Apply する道は通っている
  let languages: ISourceLanguage list =
    [ catalogLanguage
      Languages.Sxml.SxmlLanguage(fun () -> vocabulary)
      Languages.Fsb.FsbLanguage(fun () -> vocabulary)
      Languages.Fsharp.FsharpLanguage() ]
  // いま欄に載っている表記
  let mutable current = catalogLanguage
  // 波線を付けたか。**印は文字に追随しない**ので、次の打鍵で消す
  let mutable markedAt = false

  member _.attach() =
    let c = el "stage"
    if isNull c then ()
    elif c.getAttribute "data-attached" = "1" then ()
    else
      c.setAttribute ("data-attached", "1")
      c.addEventListener (
        "mousemove",
        fun (ev: Event) ->
          let e = ev :?> MouseEvent
          let r = c.getBoundingClientRect ()
          let cv = c :?> HTMLCanvasElement
          let sx = float cv.width / r.width
          let sy = float cv.height / r.height
          playerX <- (e.clientX - r.left) * sx
          playerY <- (e.clientY - r.top) * sy
      )
      c.addEventListener (
        "mouseleave",
        fun _ ->
          playerX <- 240.
          playerY <- 600.
      )

  member _.hud(n: int, t: float) =
    frames <- frames + 1
    if t0 = 0. then t0 <- t
    let elapsed = t - t0
    if elapsed >= 500. then
      let fps = round (float frames * 1000. / elapsed)
      frames <- 0
      t0 <- t
      let fpsEl = el "fps"
      if not (isNull fpsEl) then fpsEl.textContent <- string (int fps)
    let b = el "bullet-count"
    if not (isNull b) && n <> lastN then
      lastN <- n
      b.textContent <- string n

  member _.ensureCtx() : CanvasRenderingContext2D =
    if not (isNull canvasCtx) then canvasCtx
    else
      let c = el "stage"
      if isNull c then null
      else
        canvas <- c :?> HTMLCanvasElement
        canvasCtx <- getCtx canvas
        canvasCtx

  member _.draw(packed: obj, n: int) : int =
    let c2d = self.ensureCtx ()
    if isNull c2d then 0
    else
      let n = n ||| 0
      c2d?fillStyle <- "#101018"
      c2d.fillRect (0., 0., float canvas.width, float canvas.height)
      c2d?fillStyle <- "#66ccff"
      c2d.beginPath ()
      c2d.arc (playerX, playerY, 5., 0., System.Math.PI * 2.)
      c2d.fill ()
      c2d?fillStyle <- "#ffffff"
      if n <= 0 then
        c2d.beginPath ()
        c2d.arc (240., 80., 6., 0., System.Math.PI * 2.)
        c2d.fill ()
        0
      else
        let mutable i = 0
        while i < n do
          c2d.fillRect (f32 packed (i * 2) - 2., f32 packed (i * 2 + 1) - 2., 4., 4.)
          i <- i + 1
        n

  member _.call(name: string, ?arg: obj) =
    if isNull dotNet then setError "まだ起動していない"
    else
      let p =
        match arg with
        | None -> invokeAsync0 dotNet name
        | Some a -> invokeAsync1 dotNet name a
      thenCatch
        p
        (fun err -> if jsTypeof err = "string" then setError (string err))
        (fun err -> setError (errText err))

  /// 表記を明示して読ませる。**`apply` から XML を名指ししない** ——
  /// 名指しすると、次の言語を足すとき呼ぶ側も直すことになる
  member _.call2(name: string, a: obj, b: obj) =
    if isNull dotNet then setError "まだ起動していない"
    else
      thenCatch
        (invokeAsync2 dotNet name a b)
        // 戻りは「読めなかった理由」。空なら成功
        (fun err -> if jsTypeof err = "string" then setError (string err))
        (fun err -> setError (errText err))

  /// Apply の戻りだけ JSON。**`call2` を使わない** ——
  /// あちらは `Play` なども通るので、JSON を前提にすると全部 壊れる
  member _.apply() =
    let sel = el "pattern"
    if not (isNull sel) then (sel :?> HTMLSelectElement).value <- ""
    if isNull dotNet then setError "まだ起動していない"
    else
      thenCatch
        (invokeAsync2 dotNet "ApplySource" current.Kind.Id (Monaco.getValue ()))
        (fun res ->
          let r = jsonParse (string res)
          // **消したら札も下ろす。** 立てっぱなしだと、次の打鍵が
          // 付いていない印を消しに行く
          let clear () =
            markedAt <- false
            Monaco.clearMarks ()
          if unbox<bool> r?ok then
            setError ""
            clear ()
          else
            setError (string r?message)
            // **`marks` が空なら位置が無い層。** 推定で引かない
            let marks: obj[] = unbox r?marks
            if marks.Length = 0 then clear ()
            else
              marks
              |> Array.map (fun m ->
                  { Monaco.Line = int (unbox<float> m?line)
                    Monaco.Column = int (unbox<float> m?column)
                    Monaco.EndColumn = int (unbox<float> m?endColumn)
                    Monaco.Message = string m?message })
              |> Array.toList
              |> Monaco.markAll
              markedAt <- true)
        (fun err -> setError (errText err))

  /// 表記を差し替える。**本文は触らない。**
  ///
  /// XML を書いたまま sxml にすると Apply が落ちる —— **それが正しい。**
  /// 黙って変換すると人が書いた字が消えるし、そもそも**書く口が repo に無い**
  member _.useLanguage(lang: ISourceLanguage) =
    current <- lang
    Monaco.setLanguage lang.EditorLanguageId
    let sel = el "mode"
    if not (isNull sel) then (sel :?> HTMLSelectElement).value <- lang.Kind.Id

  /// 表記のプルダウンを、登録されている並びから作る。**html に表を書かない**
  member _.fillModes() =
    let sel = el "mode"
    if isNull sel then ()
    else
      sel.innerHTML <- ""
      for lang in languages do
        let o = document.createElement "option" :?> HTMLOptionElement
        o.value <- lang.Kind.Id
        o.textContent <- lang.Kind.Id
        sel.appendChild o |> ignore
      (sel :?> HTMLSelectElement).value <- current.Kind.Id
      // 開ける拡張子も同じ並びから。**html と 2 か所 に書かない**
      let input = el "open-file"
      if not (isNull input) then
        input.setAttribute (
          "accept",
          languages |> List.map (fun l -> l.Kind.FileExtension) |> String.concat ",")

  member _.setMode() =
    let sel = el "mode"
    if isNull sel then ()
    else
      let id = (sel :?> HTMLSelectElement).value
      match languages |> List.tryFind (fun l -> l.Kind.Id = id) with
      | None -> setError ("知らない表記: " + id)
      | Some lang ->
        self.useLanguage lang
        // **印を下ろす。** 前の表記で引いた波線は、切り替えた時点で嘘になる
        markedAt <- false
        Monaco.clearMarks ()
        setError ""

  member _.fillPatterns() =
    let sel = el "pattern"
    if isNull sel || isNull dotNet then ()
    else
      let names = invoke0 dotNet "ListPatterns"
      sel.innerHTML <- ""
      let blank = document.createElement "option" :?> HTMLOptionElement
      blank.value <- ""
      blank.textContent <- "（編集 / Open）"
      sel.appendChild blank |> ignore
      let len: int = names?length
      let mutable i = 0
      while i < len do
        let o = document.createElement "option" :?> HTMLOptionElement
        o.value <- string i
        o.textContent <- string (jsItem names i)
        sel.appendChild o |> ignore
        i <- i + 1

  member _.pick() =
    let sel = el "pattern"
    if isNull sel then ()
    elif (sel :?> HTMLSelectElement).value = "" then ()
    elif isNull dotNet then setError "まだ起動していない"
    else
      let i = float (sel :?> HTMLSelectElement).value
      thenCatch
        (invokeAsync1 dotNet "SelectPattern" i)
        (fun xml ->
          let s = string xml
          if s.StartsWith "ERROR:" then setError (s.Substring 6)
          else
            Monaco.setValue s
            // **表記も戻す。** 返ってくるのは同梱カタログの XML ダンプなので、
            // sxml のまま載せると Apply が落ちる
            self.useLanguage catalogLanguage
            setError "")
        (fun err -> setError (errText err))

  /// 速さのプルダウン。**値の意味を html に書かない** ——
  /// html に在るのは `-4` のような数だけで、それが何回 進めるかは host の `Pacing`。
  ///
  /// 速さは host にだけ持つ。Fable 側にも持つと、Apply や Reset のあとで
  /// 2 か所 が食い違う
  member _.setRate() =
    let sel = el "rate"
    if isNull sel then ()
    elif isNull dotNet then setError "まだ起動していない"
    else
      let n = float (sel :?> HTMLSelectElement).value
      thenCatch
        (invokeAsync1 dotNet "SetRate" n)
        (fun _ -> ())
        (fun err -> setError (errText err))

  /// 補完の使い方。**中身は html に在る字だけ**で、ここは開け閉めだけ。
  /// ループは止めない —— 開いている間も弾幕は動く
  member _.help() =
    let d = el "help-dialog"
    if not (isNull d) then showModal d

  member _.closeHelp() =
    let d = el "help-dialog"
    if not (isNull d) then closeDialog d

  /// 入れ物の大きさを変えた側から呼ぶ。**モーダルに入れて開いた直後** ——
  /// 0x0 で建った版が、そこで実寸を測り直す
  member _.relayout() = Monaco.relayout ()

  member _.``open``() =
    let input = el "open-file"
    if isNull input then ()
    else
      (input :?> HTMLInputElement).value <- ""
      input?click ()

  member _.loadFile(file: obj) =
    if isNull file then ()
    else
      // **名前で表記を決める。** 決めないと、`.sxml` を開いた人がまず見るのは
      // 「読めなかった」になる —— 拡張子はその場に在る材料
      let name = (string (file?name : obj)).ToLower()
      let byName =
        languages
        |> List.tryFind (fun l -> name.EndsWith l.Kind.FileExtension)
        |> Option.defaultValue current
      let reader = newFileReader ()
      reader.onload <-
        fun _ ->
          Monaco.setValue (string reader.result)
          self.useLanguage byName
          let sel = el "pattern"
          if not (isNull sel) then (sel :?> HTMLSelectElement).value <- ""
          self.apply ()
      reader.onerror <- fun _ -> setError "ファイルを読めなかった"
      reader.readAsText (file :?> Blob) |> ignore

  /// Monaco を読んで `#source` に建てる。**ここが落ちてもループは回す。**
  ///
  /// 初期の本文は host が焼く（`InitialSource`）。**html に XML を置かない** ——
  /// 置くと、走る弾幕を替えたとき欄の字だけが古びる。
  member _.startEditor() =
    try
      match Monaco.vsBaseFromPage () with
      | None -> setError "Monaco のローダの script src が見つからない"
      | Some vs ->
        Monaco.load vs (fun () ->
          try
            let seed = if isNull dotNet then "" else string (invoke0 dotNet "InitialSource")
            Monaco.create "source" current.EditorLanguageId seed
            self.loadVocabulary ()
            self.fillModes ()
            // **表記ごとに 1 回 ずつ、起動時に。** Monaco の provider は
            // language id に付くので、切り替えるたびに登録すると同じ id へ
            // 何本も積み上がり、候補が表記の数だけ重なる
            for lang in languages do
              Monaco.registerCompletionProvider
                lang.EditorLanguageId
                lang.TriggerCharacters
                (fun src offset -> lang.Complete src offset)
              Monaco.registerHoverProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Hover src offset)
            // **印は文字に追随しない。** 1 文字 打った時点で場所が嘘になるので、
            // そこで消す。付けていないときは何もしない（毎打鍵の空振りを避ける）
            Monaco.onContentChanged (fun () ->
              if markedAt then
                markedAt <- false
                Monaco.clearMarks ())
            self.showInitialInPatterns ()
          with ex -> setError ("エディタ: " + string ex))
    with ex -> setError ("エディタ: " + string ex)

  /// 語彙を host から **1 回 だけ** もらう。正本は `Core/DTD.fs`。
  ///
  /// **毎キー WASM に行かない。** 引くのはこちら側で、行き来はここ 1 回。
  /// 空で返ってきたら黙って進まない —— reflection が効いていない印
  /// （`PublishTrimmed` を true にした、など）で、そのまま進むと
  /// 「候補が出ないエディタ」が正常に見える
  member _.loadVocabulary() =
    if isNull dotNet then ()
    else
      vocabulary <- VocabularyJson.parseVocabulary (string (invoke0 dotNet "Vocabulary"))
      if vocabulary.Elements.IsEmpty then setError "語彙が空（Core の型を読めていない）"

  /// 走っている弾幕をプルダウンにも出す。**空のままにしない** ——
  /// 空は「XML 編集 / Open」の意味なので、同梱を走らせているのに嘘になる
  member _.showInitialInPatterns() =
    let sel = el "pattern"
    if isNull sel || isNull dotNet then ()
    else
      let i = int (unbox<float> (invoke0 dotNet "InitialIndex"))
      if i >= 0 then (sel :?> HTMLSelectElement).value <- string i
      else setError "起動時の弾幕が一覧に無い"

  member _.onReady(dn: obj) =
    dotNet <- dn
    self.attach ()
    try self.fillPatterns ()
    with ex -> setError (string ex)
    let errEl = el "loop-error"
    if not (isNull errEl) && errEl.textContent = "起動待ち" then errEl.textContent <- ""
    if running then ()
    else
      running <- true
      let rec loop (t: float) =
        if not running then ()
        else
          window.requestAnimationFrame loop |> ignore
          try
            let ret = invokeStep dotNet "StepFrame" t playerX playerY
            let n = int (unbox<float> (jsItem ret 0))
            let packed =
              if n > 0 then
                let off = int (unbox<float> (jsItem ret 1) / 4.)
                subarray (heapF32 ()) off (off + n * 2)
              else null
            self.draw (packed, n) |> ignore
            self.hud (n, t)
          with ex ->
            console.error ex
            setError (string ex)
      window.requestAnimationFrame loop |> ignore

    // **エディタは rAF を予約したあと。** ローダは CDN 越しなので、
    // 先に呼ぶと最初の 1 コマ がその往復ぶん遅れる。
    // 読めなくても Canvas は 2way のまま動かす —— 理由だけ出す
    self.startEditor ()

let playground = Playground()
window?playground <- playground
playground.attach ()
playground.draw (null, 0) |> ignore

let openInput = el "open-file"
if not (isNull openInput) then
  openInput.addEventListener (
    "change",
    fun _ ->
      let files: obj = openInput?files
      let file = if isNull files then null else jsItem files 0
      playground.loadFile file
  )

/// html の `onclick=` を使わない。**CSP に `'unsafe-inline'` を出していないので
/// インラインのハンドラは弾かれる** —— ボタンが押せないだけで、
/// エラーも出ず、走行でも試験でも見えない（実際に踏んだ）。
///
/// **`html にロジックを書かない`線とも合う。** `onclick="playground.call('Play')"`
/// は、そこだけ型も検査も掛からない字だった
let private on (id: string) (event: string) (handler: unit -> unit) =
  let node = el id
  if isNull node then setError ("配線する先が無い: " + id)
  else node.addEventListener (event, fun _ -> handler ())

on "pattern" "change" (fun () -> playground.pick ())
on "mode" "change" (fun () -> playground.setMode ())
on "help" "click" (fun () -> playground.help ())
on "help-close" "click" (fun () -> playground.closeHelp ())
on "play" "click" (fun () -> playground.call("Play"))
on "pause" "click" (fun () -> playground.call("Pause"))
on "step-once" "click" (fun () -> playground.call("StepOnce"))
on "rate" "change" (fun () -> playground.setRate ())
on "reset" "click" (fun () -> playground.call("Reset"))
on "apply" "click" (fun () -> playground.apply ())
on "open" "click" (fun () -> playground.``open``())

// **いちばん最後。** 上の配線が済んでから WASM を起こす ——
// `onReady` はここから返ってくるので、先に起こすと受け口が無い。
// 失敗は `#loop-error` に出す。黙って白い画面にしない
thenCatch (blazorStart ()) ignore (fun err -> setError (errText err))
