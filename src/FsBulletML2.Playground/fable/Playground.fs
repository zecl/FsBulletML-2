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

[<Emit("$0.invokeMethodAsync($1, $2, $3, $4)")>]
let private invokeAsync3 (dn: obj) (name: string) (a: obj) (b: obj) (c: obj) : obj = jsNative

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

// **無いことが在る。** http で開いた窓や、権限を落とした窓では
// `navigator.clipboard` そのものが生えない —— そこで落とすと、
// リンクは URL 欄 に入っているのに「作れなかった」に見える
[<Emit("navigator.clipboard ? navigator.clipboard.writeText($0) : null")>]
let private copyText (s: string) : obj = jsNative

/// 字をファイルとして落とす。**`a[download]` を 1 回 だけ組んで押す。**
///
/// Blob の URL は使い終わったら返す —— 返さないとページを閉じるまで残る。
/// `charset` を書くのは、開く側が Shift_JIS と読まないため
[<Emit("""(() => {
  const blob = new Blob([$1], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = $0
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
})()""")>]
let private saveText (name: string) (text: string) : unit = jsNative

// 種を振り直すときだけ使う。**弾幕の乱数はこれではない** ——
// あちらは host の `SeededRandom`（種から決まる並び）
[<Emit("Math.random()")>]
let private random () : float = jsNative

let private el (id: string) = document.getElementById id

let private setError (msg: string) =
  let e = el "loop-error"
  if not (isNull e) then e.textContent <- msg

/// 済んだことの知らせ。**`setError` と別のところへ出す** ——
/// 同じ場所へ赤で出すと、コピーできたことが失敗に見える
let private setNote (msg: string) =
  let e = el "share-note"
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
  // マウスが指している場所。**これがそのまま自機とは限らない** ——
  // 止めているときと回っているときは host が決める（`setView`）
  let mutable playerX = 240.
  let mutable playerY = 600.
  // 描く自機と、面の形。**どれも host から毎コマ来る** ——
  // ここに数を書き写すと、向きを足したときに 2 通り の真ができる
  let mutable drawX = 240.
  let mutable drawY = 600.
  let mutable fieldW = 0.
  let mutable fieldH = 0.
  let mutable homeX = 240.
  let mutable homeY = 600.
  let mutable enemyX = 240.
  let mutable enemyY = 80.
  let mutable running = false
  let mutable dotNet: obj = null
  let mutable frames = 0
  let mutable t0 = 0.
  let mutable lastN = -1
  let mutable lastFrame = -1
  // 軌跡。**過去の位置を貯めない** —— 面を消さずに薄く塗り重ねるだけなので、
  // 確保は増えない（`draw` の但し書き）
  let mutable trail = false
  // 次の 1 コマ だけ面を全部 塗り潰す。軌跡が付いたまま別の弾幕へ行かない
  let mutable wipe = false
  let mutable canvas: HTMLCanvasElement = null
  let mutable canvasCtx: CanvasRenderingContext2D = null
  // host からもらう語彙。正本は Core の DTD.fs。**表記が変わっても同じ**
  let mutable vocabulary: Vocab = { Elements = []; Expressions = []; Ce = []; CeLabels = []; CePlaces = [] }
  // **起動時の表記。** 欄に最初に出るのは host が焼く XML（`InitialSource`）。
  //
  // **同梱カタログを選ぶときの表記ではない**（v1.6）——
  // あちらはいま選ばれている表記で host に書いてもらう
  let initialLanguage: ISourceLanguage = Languages.Xml.XmlLanguage(fun () -> vocabulary)
  // 登録されている表記。**並びは `SourceKind.all` と同じ** ——
  // プルダウンも `Open` の accept もここから作るので、順が意味を持つ。
  // **v1.9 で 4 表記 が揃った。** F# の CE も候補・hover・rename・
  // 定義へ移動・直し方・波線を出す（`Languages/Fsharp.fs`）
  let languages: ISourceLanguage list =
    [ initialLanguage
      Languages.Sxml.SxmlLanguage(fun () -> vocabulary)
      Languages.Fsb.FsbLanguage(fun () -> vocabulary)
      Languages.Fsharp.FsharpLanguage(fun () -> vocabulary) ]
  // いま欄に載っている表記
  let mutable current = initialLanguage
  // 波線を付けたか。**印は文字に追随しない**ので、次の打鍵で消す
  let mutable markedAt = false
  // 知らせを出したか。**波線と同じ扱い** —— 知らせは「いまの本文がどこから
  // 来たか」の話なので、本文が変わった時点で嘘になる
  let mutable noted = false
  // 整形を待っているときの、その字。**空なら待っていない。**
  // 字そのものを持つので、打ち直せば待ちは自然に外れる
  let mutable pendingFormat = ""

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
          playerX <- homeX
          playerY <- homeY
      )

  member _.hud(n: int, t: float, frame: int) =
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
    // **飛んでいる最中はここが速く動く。** 進んでいることの唯一の合図なので、
    // 弾数と同じく変わったときだけ書く（毎フレーム書くと DOM を無駄に触る）
    let f = el "frame-count"
    if not (isNull f) && frame <> lastFrame then
      lastFrame <- frame
      f.textContent <- string frame

  /// 面の置き場所を host から引き直す。**大きさが変わったときだけ。**
  ///
  /// 敵と自機の定位置は向きで変わるので、ここで写す ——
  /// JS 側に数を書くと、`Stage` と 2 通り の真ができる
  member _.refreshPlaces() =
    if isNull dotNet then ()
    else
      thenCatch
        (invokeAsync0 dotNet "FieldSize")
        (fun (v: obj) ->
          homeX <- unbox<float> (jsItem v 2)
          homeY <- unbox<float> (jsItem v 3)
          enemyX <- unbox<float> (jsItem v 4)
          enemyY <- unbox<float> (jsItem v 5))
        (fun err -> setError (errText err))

  /// 毎コマ host から来る、自機と面の大きさ。
  ///
  /// **大きさが変わったら canvas を建て直す。** 向きは弾幕が持っているので、
  /// 変わるのは弾幕が変わったとき —— 建て直す道は 8 本 以上 あるので、
  /// 「変わったら」で拾うほうが呼び忘れが起きない
  member _.setView(px: float, py: float, w: float, h: float) =
    drawX <- px
    drawY <- py
    if w > 0. && (w <> fieldW || h <> fieldH) then
      fieldW <- w
      fieldH <- h
      let c = el "stage"
      if not (isNull c) then
        let cv = c :?> HTMLCanvasElement
        cv.width <- int w
        cv.height <- int h
      self.refreshPlaces ()

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
      // **軌跡は過去の位置を持たない。** 面を薄く塗るだけにすると、前のコマの
      // 弾がそのまま残って尾に見える。確保は 0 バイト。
      //
      // 消さずに残す形にはしない —— 弾の多い弾幕だと数秒で面が真っ白になる。
      // 薄く塗り重ねるほうは、放っておいても消えるので消す道が要らない
      // （切り替えと弾幕の差し替えだけ、1 コマ 塗り潰す）
      if trail && not wipe then c2d?fillStyle <- "rgba(16, 16, 24, 0.12)"
      else c2d?fillStyle <- "#101018"
      wipe <- false
      c2d.fillRect (0., 0., float canvas.width, float canvas.height)
      c2d?fillStyle <- "#66ccff"
      c2d.beginPath ()
      c2d.arc (drawX, drawY, 5., 0., System.Math.PI * 2.)
      c2d.fill ()
      c2d?fillStyle <- "#ffffff"
      if n <= 0 then
        c2d.beginPath ()
        c2d.arc (enemyX, enemyY, 6., 0., System.Math.PI * 2.)
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
    // 面が建て直る。**前の尾を残さない** —— 残ると別の弾幕の線に見える
    wipe <- true
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

  /// 配色のプルダウンを、Monaco が素で持っている並びから作る。
  /// **html に表を書かない**（表記のプルダウンと同じ扱い）。
  ///
  /// 名前を知っているのは `fable/Monaco.fs` だけ —— 器はどのエディタに
  /// 載るかを知らないので、あちらへ置くと線を越える
  member _.fillThemes() =
    let sel = el "theme"
    if isNull sel then ()
    else
      sel.innerHTML <- ""
      for t in Monaco.themes do
        let o = document.createElement "option" :?> HTMLOptionElement
        o.value <- t.Id
        o.textContent <- t.Label
        sel.appendChild o |> ignore
      (sel :?> HTMLSelectElement).value <- Monaco.defaultTheme

  /// 配色を替える。**載っている弾幕にも本文にも触らない** ——
  /// 見る側の都合であって、書いたものの一部ではない（速さと同じ扱い）。
  ///
  /// 知らない字は理由を出す。**Monaco は落ちずに明るいほうへ倒れる**ので、
  /// ここで言わないと「選んだのに違う配色」になる
  member _.setTheme() =
    let sel = el "theme"
    if isNull sel then ()
    else
      let id = (sel :?> HTMLSelectElement).value
      if Monaco.setTheme id then setError ""
      else setError ("知らない配色: " + id)

  member _.setMode() =
    let sel = el "mode"
    if isNull sel then ()
    else
      let id = (sel :?> HTMLSelectElement).value
      match languages |> List.tryFind (fun l -> l.Kind.Id = id) with
      | None -> setError ("知らない表記: " + id)
      // 同じ表記を選び直しただけなら、本文を通さない
      | Some lang when lang.Kind.Id = current.Kind.Id -> ()
      | Some lang ->
        // **いまの本文をその表記へ書き直す（v1.4）。**
        //
        // v1.3 まで本文は触らなかった。「黙って変換すると人が書いた字が消える」
        // と書いてあったが、**本当の理由は XML 以外 を書く口が repo に無かった**
        // こと —— 口ができたので変換する。
        //
        // **プルダウンの弾幕を読み直すのではなく、いまの本文を変換する** ——
        // 人が足した字が残るし、Open したファイルや編集後でも効く。
        // 読めなければ触らずに理由を出す（下の `ok:false`）
        let previous = current
        let fromId = current.Kind.Id
        let text = Monaco.getValue ()
        self.useLanguage lang
        // **印を下ろす。** 前の表記で引いた波線は、切り替えた時点で嘘になる
        markedAt <- false
        Monaco.clearMarks ()
        setError ""
        if isNull dotNet then ()
        else
          thenCatch
            (invokeAsync3 dotNet "Transcode" fromId lang.Kind.Id text)
            (fun res ->
              let r = jsonParse (string res)
              if unbox<bool> r?ok then Monaco.setValue (string r?text)
              else
                // **表記も戻す。** 本文を触らないだけだと、
                // 「sxml と名乗る fsb の本文」が残って色も Apply の理由も嘘になる
                // —— 人には**別の間違い**に見える
                self.useLanguage previous
                setError (string r?message))
            (fun err ->
              self.useLanguage previous
              setError (errText err))

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
      // 面が建て直る（`apply` と同じ理由）
      wipe <- true
      // **いま選ばれている表記で書いてもらう。** 表記はこちらが決めるのではなく
      // 人が決めているもので、弾幕を選び直しただけで動かしてはいけない
      thenCatch
        (invokeAsync2 dotNet "SelectPattern" i current.Kind.Id)
        (fun text ->
          let s = string text
          if s.StartsWith "ERROR:" then setError (s.Substring 6)
          else
            Monaco.setValue s
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

  /// そのコマへ飛ぶ。**丸めるのは host の `Seek`** ——
  /// ここで丸めると、上限が 2 か所 に書かれて片方 だけ古びる。
  ///
  /// 飛んでいるあいだ画面は止まらない（着くまで何フレームか かかる）。
  /// 進んでいることはコマ数の表示に出る
  member _.seek() =
    let input = el "seek-to"
    if isNull input then ()
    elif isNull dotNet then setError "まだ起動していない"
    else
      let v = (input :?> HTMLInputElement).value
      // **`float v` にしない。** Fable の `Double.Parse` は読めない字で例外を投げる ——
      // `IsNaN` で受けるつもりの門はそこまで来ない（焼いた JS を読んで気づいた）。
      // `type="number"` の欄は、数でない字が入っていると `value` が空になる
      let mutable n = 0.0
      // 空欄や字が入っているとき。**0 に倒さない** —— 頭へ飛んでしまう
      if not (System.Double.TryParse(v, &n)) then setError "コマ数を入れて"
      else
        // 戻る先だと面を建て直すので、軌跡は 1 コマ 塗り潰す
        wipe <- true
        setError ""
        thenCatch
          (invokeAsync1 dotNet "SeekTo" n)
          (fun _ -> ())
          (fun err -> setError (errText err))

  /// 次の 1 コマ だけ面を塗り潰す。面を建て直す口を呼ぶ手前で使う
  member _.wipeTrail() = wipe <- true

  /// 軌跡の入り切り。**host は知らない** —— 描き方の話で、
  /// 走っているものは変わらない（速さや配色と同じ扱い）
  member _.setTrail() =
    let box = el "trail"
    if isNull box then ()
    else
      trail <- (box :?> HTMLInputElement).``checked``
      // 切ったときに残っている尾を 1 コマ で消す
      if not trail then wipe <- true

  /// 本文を読んで、同じ表記で書き直す。
  ///
  /// **口を足していない。** `Transcode` の行き先を、いまの表記と同じにするだけ ——
  /// 整形は「別の表記へ書き直す」の行き先が同じ場合で、
  /// 読む口も書く口も 4 表記 ぶん もう在る。
  ///
  /// **押す前に知らせる。** 整形すると **コメントは残らない**（木がコメントを
  /// 持たないので、読んで書き直した時点で落ちる）。だから
  ///
  ///     字が変わらない   その場で「もう整形されている」
  ///     字が変わる       1 回 目 は知らせるだけ。もう一度 押すと入れ替える
  ///
  /// **1 回 目 と 2 回 目 のあいだに打てば、待っているものは消える** ——
  /// 待っているのは「その字から作った結果」で、字が変われば別のものになる
  member _.format() =
    if isNull dotNet then setError "まだ起動していない"
    else
      let text = Monaco.getValue ()
      let id = current.Kind.Id
      thenCatch
        (invokeAsync3 dotNet "Transcode" id id text)
        (fun res ->
          let r = jsonParse (string res)
          if not (unbox<bool> r?ok) then
            pendingFormat <- ""
            setError (string r?message)
          else
            let formatted = string r?text
            setError ""
            if formatted = text then
              pendingFormat <- ""
              self.showNote "もう整形されている"
            elif pendingFormat = text then
              pendingFormat <- ""
              Monaco.setValue formatted
              self.showNote "整形した"
            else
              pendingFormat <- text
              self.showNote "整形すると字が変わる。コメントは残らない。もう一度 押すと入れ替える")
        (fun err ->
          pendingFormat <- ""
          setError (errText err))

  /// いまの本文をファイルに落とす。**Open の対。**
  ///
  /// **host へ行かない。** 落とすのは欄に在る字そのもので、
  /// 読めるかどうかも関係ない（書きかけでも落とせる）。
  ///
  /// 名前の拡張子は表記から引く —— `Open` が名前で表記を決めるので、
  /// **落としたものをそのまま開くと同じ表記に戻る**
  member _.save() =
    let text = Monaco.getValue ()
    // **頭は要素名にしない。** 根の名前を付けたくなるが、ブラウザ側に
    // BulletML の綴りが在ってはいけない（門が当たる）——
    // 語彙は host が焼いて渡すもので、ここに表を持たない
    let name = "pattern" + current.Kind.FileExtension
    saveText name text
    self.showNote ("落とした: " + name)

  /// いま欄に出ている難度（0 から 100 の整数）。**字は 1 か所 でしか読まない**
  member _.rankPercent() : int =
    let el = el "rank"
    if isNull el then 50
    else
      let mutable v = 0.0
      if System.Double.TryParse((el :?> HTMLInputElement).value, &v)
      then max 0 (min 100 (int v))
      else 50

  /// いま欄に出ている種。**読めなければ 1**（種は 0 から動かない）
  member _.seedValue() : int =
    let el = el "seed"
    if isNull el then 1
    else
      let mutable v = 0.0
      if System.Double.TryParse((el :?> HTMLInputElement).value, &v) then max 1 (int v) else 1

  /// 難度の欄を動かした。**host が面を建て直す** ——
  /// `Runner.load` は木を組む段で rank を引くので、走っている面には効かない
  member _.setRank() =
    let label = el "rank-value"
    let n = self.rankPercent ()
    if not (isNull label) then label.textContent <- string n
    if isNull dotNet then ()
    else
      thenCatch
        (invokeAsync1 dotNet "SetRank" (float n / 100.0))
        (fun _ -> ())
        (fun err -> setError (errText err))

  /// 種の欄を動かした。**同じ種なら同じ走り**
  member _.setSeed() =
    if isNull dotNet then ()
    else
      thenCatch
        (invokeAsync1 dotNet "SetSeed" (self.seedValue ()))
        (fun _ -> ())
        (fun err -> setError (errText err))

  /// 自機の動かし方。**面を建て直さない** —— 難度や種と違って、
  /// これは走っている面の途中からでも効く（狙いは毎コマ 引かれる）。
  ///
  /// 狙いを使う弾幕は 176 本 中 103 本（実測）——
  /// つまり半分 以上 で、自機をどこに置くかが絵そのものを変える
  member _.setPlayerMotion() =
    let sel = el "player"
    if isNull sel || isNull dotNet then ()
    else
      let mutable v = 0.0
      let n =
        if System.Double.TryParse((sel :?> HTMLSelectElement).value, &v) then int v else 0
      thenCatch
        (invokeAsync1 dotNet "SetPlayerMotion" n)
        (fun _ -> ())
        (fun err -> setError (errText err))

  /// 種を振り直す。**「別の走りが見たい」を 1 手 に**
  member _.rollSeed() =
    let box = el "seed"
    if isNull box then ()
    else
      // 1 から 999999。**0 を入れない**（種は 0 から動かない）
      let n = 1 + int (floor (random () * 999999.0))
      (box :?> HTMLInputElement).value <- string n
      self.setSeed ()

  /// リンクから来た走らせ方を欄と host に入れる。**負は「載っていない」の印**
  member _.applyAxes(rank: int, seed: int) =
    if rank >= 0 then
      let box = el "rank"
      if not (isNull box) then (box :?> HTMLInputElement).value <- string rank
      self.setRank ()
    if seed >= 0 then
      let box = el "seed"
      if not (isNull box) then (box :?> HTMLInputElement).value <- string (max 1 seed)
      self.setSeed ()

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

  /// 知らせを出す。**波線と同じで、次の打鍵で消える** ——
  /// 「いまの本文がどこから来たか」の話なので、打った時点で嘘になる。
  /// 消すのは `onContentChanged` の側（出した札をここで立てておく）
  member _.showNote(msg: string) =
    noted <- msg <> ""
    setNote msg

  /// いまの本文を URL にする。**サーバを持たない。**
  ///
  /// 置くのは fragment（`#` の後ろ）—— **サーバへ送られない**ので、
  /// 書いた弾幕がホスティングのログに残らない。クエリに置くと残る。
  ///
  /// **アドレス欄をそのまま共有する字にする。** こちらで別の欄を用意すると、
  /// 貼るときにそこを見に行くことになる。コピーはついで
  member _.share() =
    self.showNote ""
    thenCatch
      // **走らせ方も乗せる（版 2）。** 同じ本文でも難度と種が違えば別の絵 ——
      // リンクが指しているのは「その走り」
      (box (Share.encode current.Kind (self.rankPercent ()) (self.seedValue ()) (Monaco.getValue ())))
      (fun fragment ->
        window.location.hash <- string fragment
        setError ""
        let p = copyText window.location.href
        // **コピーできなくても、リンクはもう URL 欄 に在る。**
        // そこを「失敗」と言うと、人は作り直しに行く
        if isNull p then self.showNote "URL 欄 に入れた（この窓ではコピーできない）"
        else
          thenCatch
            p
            (fun _ -> self.showNote "リンクをコピーした")
            (fun _ -> self.showNote "URL 欄 に入れた（コピーはできなかった）"))
      (fun err -> setError (errText err))

  /// URL に共有リンクが在れば、それを載せる。**無ければ何もしない。**
  ///
  /// **カタログは選ばない** —— 本文が上書きされるので、プルダウンが
  /// 指しているものと欄の中身が食い違う（`apply` が空に戻す）。
  ///
  /// 読めなければ理由を出して、**本文も表記も触らない** ——
  /// 起動時の弾幕がそのまま残るほうが、空の欄より読み解ける
  member _.loadShared() =
    let hash = window.location.hash
    if isNull hash || hash = "" || hash = "#" then ()
    else
      thenCatch
        (box (Share.decode hash))
        (fun res ->
          let r = unbox<Share.ShareRead> res
          if not r.Ok then setError r.Message
          else
            match languages |> List.tryFind (fun l -> l.Kind.Id = r.KindId) with
            | None -> setError ("知らない表記: " + r.KindId)
            | Some lang ->
              Monaco.setValue r.Text
              self.useLanguage lang
              // **走らせ方を先に入れる。** `apply` が面を建てるので、
              // あとから入れると建て直しが 1 回 増える
              self.applyAxes (r.Rank, r.Seed)
              self.showNote "リンクから読んだ"
              self.apply ())
        (fun err -> setError (errText err))

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
            // **エディタが建ってから出す。** Monaco が読めなかったときに
            // 配色のプルダウンだけ在るのは、押せるのに何も起きない口になる
            self.fillThemes ()
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
              Monaco.registerRenameProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Usages src offset)
              // 定義へ移動（F12）と参照（Shift+F12）。**rename と同じ 1 本 の上**
              Monaco.registerNavigationProviders
                lang.EditorLanguageId
                (fun src offset -> lang.Usages src offset)
              Monaco.registerCodeActionProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Fixes src offset)
            // **印は文字に追随しない。** 1 文字 打った時点で場所が嘘になるので、
            // そこで消す。付けていないときは何もしない（毎打鍵の空振りを避ける）
            Monaco.onContentChanged (fun () ->
              if markedAt then
                markedAt <- false
                Monaco.clearMarks ()
              if noted then
                noted <- false
                setNote "")
            self.showInitialInPatterns ()
            // **同梱を載せたあとで上書きする。** 先に空で建てると、
            // リンクが読めなかったときに空の欄だけが残る ——
            // 起動時の弾幕が見えているほうが、何が起きたか読み解ける
            self.loadShared ()
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
            // **自機と面の大きさは host が決める。** 送った座標をそのまま
            // 描くと、止めているときと回っているときに絵と狙いが食い違う
            self.setView (
              unbox<float> (jsItem ret 3),
              unbox<float> (jsItem ret 4),
              unbox<float> (jsItem ret 5),
              unbox<float> (jsItem ret 6)
            )
            self.draw (packed, n) |> ignore
            self.hud (n, t, int (unbox<float> (jsItem ret 2)))
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
on "theme" "change" (fun () -> playground.setTheme ())
on "help" "click" (fun () -> playground.help ())
on "help-close" "click" (fun () -> playground.closeHelp ())
on "play" "click" (fun () -> playground.call("Play"))
on "pause" "click" (fun () -> playground.call("Pause"))
on "step-once" "click" (fun () -> playground.call("StepOnce"))
on "rate" "change" (fun () -> playground.setRate ())
on "reset" "click" (fun () ->
  // 面が建て直る。**軌跡を 1 コマ 塗り潰してから**（`apply` と同じ理由）
  playground.wipeTrail ()
  playground.call ("Reset"))
on "seek" "click" (fun () -> playground.seek ())
on "rank" "input" (fun () -> playground.setRank ())
on "seed" "change" (fun () -> playground.setSeed ())
on "seed-roll" "click" (fun () -> playground.rollSeed ())
on "player" "change" (fun () -> playground.setPlayerMotion ())
on "trail" "change" (fun () -> playground.setTrail ())
on "apply" "click" (fun () -> playground.apply ())
on "open" "click" (fun () -> playground.``open``())
on "save" "click" (fun () -> playground.save ())
on "format" "click" (fun () -> playground.format ())
on "share" "click" (fun () -> playground.share ())

// **いちばん最後。** 上の配線が済んでから WASM を起こす ——
// `onReady` はここから返ってくるので、先に起こすと受け口が無い。
// 失敗は `#loop-error` に出す。黙って白い画面にしない
thenCatch (blazorStart ()) ignore (fun err -> setError (errText err))
