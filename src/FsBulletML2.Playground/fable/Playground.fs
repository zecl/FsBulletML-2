module FsBulletML2.Playground.Js

open Fable.Core
open Fable.Core.JsInterop
open Browser
open Browser.Types
open FsBulletML2.Playground.SourceLanguage

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
  // host からもらう語彙。正本は Core の DTD.fs
  let mutable vocabulary: VocabElement list = []
  // 登録されている表記。**v0.3 は XML 1 本。**
  // 次の言語はここに 1 個 足して、host の kind に腕を 1 本 足すだけ
  let languages: ISourceLanguage list = [ Languages.Xml.XmlLanguage(fun () -> vocabulary) ]
  // いま欄に載っている表記。UI に切替は出さない
  let mutable current = List.head languages

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
        (fun err -> setError (string err))

  /// 表記を明示して読ませる。**`apply` から XML を名指ししない** ——
  /// 名指しすると、次の言語を足すとき呼ぶ側も直すことになる
  member _.call2(name: string, a: obj, b: obj) =
    if isNull dotNet then setError "まだ起動していない"
    else
      thenCatch
        (invokeAsync2 dotNet name a b)
        // 戻りは「読めなかった理由」。空なら成功
        (fun err -> if jsTypeof err = "string" then setError (string err))
        (fun err -> setError (string err))

  member _.apply() =
    let sel = el "pattern"
    if not (isNull sel) then (sel :?> HTMLSelectElement).value <- ""
    self.call2 ("ApplySource", current.Kind.Id, Monaco.getValue ())

  member _.fillPatterns() =
    let sel = el "pattern"
    if isNull sel || isNull dotNet then ()
    else
      let names = invoke0 dotNet "ListPatterns"
      sel.innerHTML <- ""
      let blank = document.createElement "option" :?> HTMLOptionElement
      blank.value <- ""
      blank.textContent <- "（XML 編集 / Open）"
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
            Monaco.setLanguage current.MonacoLanguage
            setError "")
        (fun err -> setError (string err))

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
      let reader = newFileReader ()
      reader.onload <-
        fun _ ->
          Monaco.setValue (string reader.result)
          Monaco.setLanguage current.MonacoLanguage
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
            Monaco.create "source" current.MonacoLanguage seed
            self.loadVocabulary ()
            // **XML を名指ししない。** 次の言語が来ても、通る道はここ 1 本
            Monaco.registerCompletionProvider current.MonacoLanguage (fun src offset ->
              current.Complete src offset)
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
      vocabulary <- SourceLanguage.parseVocabulary (string (invoke0 dotNet "Vocabulary"))
      if vocabulary.IsEmpty then setError "語彙が空（Core の型を読めていない）"

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
