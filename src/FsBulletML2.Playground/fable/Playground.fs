module FsBulletML2.Playground.Js

open Fable.Core
open Fable.Core.JsInterop
open Browser
open Browser.Types
open FsBulletML2.Fable
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

// **弾を選ぶのは同期で呼ぶ。** 添字はそのコマの `Pack` の並びのもので、
// 次のコマには別の弾を指しうる（`Playfield.Tick` が消しで末尾を移す）——
// 非同期にすると、押してから届くまでに 1 コマ 進みうる
[<Emit("$0.invokeMethod($1, $2)")>]
let private invoke1 (dn: obj) (name: string) (a: obj) : obj = jsNative

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

/// 印の数。**`String.Split` の overload に頼らない**（Fable の写し方が版に依る）
let private countMark (s: string) (mark: string) =
  let mutable n = 0
  let mutable i = s.IndexOf mark
  while i >= 0 do
    n <- n + 1
    i <- s.IndexOf(mark, i + mark.Length)
  n

[<Emit("$0.getContext('2d', { alpha: false })")>]
let private getCtx (c: HTMLCanvasElement) : CanvasRenderingContext2D = jsNative

/// 画面の 1 CSS 画素 が何 デバイス画素 か。**無ければ 1**
[<Emit("window.devicePixelRatio || 1")>]
let private dpr () : float = jsNative

/// `localStorage` を読む。**読めなければ null。**
///
/// **例外を握る。** プライベートウィンドウや「サイトデータを保存しない」設定では
/// `localStorage` に触った時点で投げる —— そこで起動が止まると、
/// **残す機能を足したせいで開かなくなる。**
[<Emit("(function(){ try { return localStorage.getItem($0) } catch (e) { return null } })()")>]
let private lsGet (key: string) : string = jsNative

/// `localStorage` に書く。**書けなければ false。**
///
/// 上限（5 MB くらい）を超えると `QuotaExceededError` を投げる。
/// 同梱の最大は 28.5 KB（上限の 0.557%）なので当たらないはずだが、
/// **人が書いた字に上限は無い。**
[<Emit("(function(){ try { localStorage.setItem($0, $1); return true } catch (e) { return false } })()")>]
let private lsSet (key: string) (value: string) : bool = jsNative

/// 残す口の id。**本文と `mode` は別**（下の但し書き）。
///
/// **`pattern` は入れない** —— 本文が正で、同梱を選んだ状態も
/// 「その字が入っている」で表せる。プルダウンだけ戻すと、
/// 字と食い違ったときにどちらが本当か読めなくなる。
///
/// `pattern2` / `compare`（2 面 と 比べる）も入れていない ——
/// あちらは 2 本 目 の弾幕を建てるので、戻す順が本文と絡む
let private keptControls =
  [| "theme"; "seed"; "rank"; "rate"; "player"; "trail"; "marks"; "seek-to"; "rec-len" |]

/// この中のものは `checked`、ほかは `value` で読み書きする。
///
/// **`keptControls` の隣 に置く。** 前は `if id = "trail"` と書いてあって、
/// 2 つ 目 のチェックを足したときに**書いたほうだけが直る**形だった
let private keptChecks = [| "trail"; "marks" |]

let private isCheck (id: string) = Array.contains id keptChecks

let private sourceKey = "fsbulletml2.source"
let private modeKey = "fsbulletml2.mode"
let private settingsKey = "fsbulletml2.settings"

/// いまの時刻（ミリ秒）。**rAF が渡してくる `t` と同じ物差し。**
///
/// **分解能は 0.1 ms**（測った。`crossOriginIsolated` が false なので
/// Spectre 対策で丸められる —— 連続で 2 回 呼んで差が 0 なのが 99.75%）。
/// 1 コマ の走行は .NET の平均 0.076 ms を 40 倍 して 3 ms くらいなので、
/// **30 刻み では読める。軽い弾幕では階段になる。**
[<Emit("performance.now()")>]
let private now () : float = jsNative

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

// dialog は素で Esc と被せを持っている。**自前で被せを作らない**
[<Emit("$0.showModal()")>]
let private showModal (dialog: obj) : unit = jsNative

[<Emit("$0.close()")>]
let private closeDialog (dialog: obj) : unit = jsNative

/// 背景を押したか。**`dialog` は Esc では閉じるが、背景では閉じない** ——
/// 「素で付いてくる」と但し書きに書いてあったが、測ったら閉じなかった。
///
/// 背景は `::backdrop` で、**当たり判定は dialog 自身に付く**ので、
/// `target` が dialog なら「中のどれにも当たっていない」が分かる。
///
/// **座標も見る。** `target` だけだと、中で字を選んで外で指を離したときに
/// 当たってしまう（選んだだけで窓が閉じる）。
///
/// **中の名前を `$0` / `$1` に来る名前と重ねない。** 最初は
/// `const d = $0, e = $1` と書いて、置換後が `const d = d, e = e` になった ——
/// 自分を自分で初期化する const で、押すたび ReferenceError。
/// **F# は通り、build も門も出ず、押して初めて落ちる**
[<Emit("""(() => {
  const dlg = $0, ev = $1;
  if (ev.target !== dlg) return false;
  const r = dlg.getBoundingClientRect();
  return ev.clientX < r.left || ev.clientX > r.right || ev.clientY < r.top || ev.clientY > r.bottom;
})()""")>]
let private clickedBackdrop (dialog: obj) (e: Event) : bool = jsNative

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

/// 焼ける形（v2.5）。**環境で違う** —— Chromium 系は WebM、Safari は mp4。
/// 上から順に見て、最初に通ったものを返す。**1 つ も通らなければ空文字。**
///
/// **表を html に置かない** —— 置くと、焼けない環境で黙って落ちる
/// （通らない mime を `MediaRecorder` に渡すと例外になる）。
///
/// vp9 を先に見るのは、同じ絵で **vp8 の 0.44 倍** だから
/// （480x640 を 5 秒 で 686.5 KB と 1,563.8 KB。実測）
[<Emit("""(() => {
  if (!window.MediaRecorder) return ''
  const list = ['video/webm;codecs=vp9', 'video/webm;codecs=vp8', 'video/webm', 'video/mp4']
  for (const m of list) { if (MediaRecorder.isTypeSupported(m)) return m }
  return ''
})()""")>]
let private recordMime () : string = jsNative

/// 面を録って、動く絵として落とす（v2.5）。
///
/// **`captureStream` は塗られたコマだけを拾う。** 止まっている面や
/// 後ろのタブでは中身の無い絵になる（実測 110 B）ので、呼ぶ側が走らせてから呼ぶ。
///
/// **ビットレートを渡さない。** 明示すると簡単な絵で逆に太る ——
/// 前方 5 方向 に撃つだけの本で 104.4 KB が 140.7 KB になった（600 kbps）。
/// 既定の律速は簡単な絵で下へ落ちるが、明示すると落ちなくなる。
///
/// **中の名前を、呼ぶ側の名前と重ねない**（`const d = d` の罠。
/// `$0` には呼ぶ側の式がそのまま入る）
[<Emit("""(() => {
  const cvEl = $0, mimeType = $1, waitMs = $2, dlName = $3, cb = $4
  const stream = cvEl.captureStream(60)
  const mr = new MediaRecorder(stream, { mimeType: mimeType })
  const parts = []
  mr.ondataavailable = ev => { if (ev.data && ev.data.size) parts.push(ev.data) }
  mr.onstop = () => {
    stream.getTracks().forEach(tr => tr.stop())
    const blob = new Blob(parts, { type: mimeType })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = dlName
    document.body.appendChild(a)
    a.click()
    a.remove()
    URL.revokeObjectURL(url)
    cb(blob.size)
  }
  mr.start()
  setTimeout(() => { if (mr.state !== 'inactive') mr.stop() }, waitMs)
})()""")>]
let private recordCanvas (target: obj) (mime: string) (ms: float) (name: string) (onDone: float -> unit) : unit = jsNative

// 種を振り直すときだけ使う。**弾幕の乱数はこれではない** ——
// あちらは host の `SeededRandom`（種から決まる並び）
[<Emit("Math.random()")>]
let private random () : float = jsNative

let private el (id: string) = document.getElementById id

/// 記事や本の中に貼られているか。**`?embed=1` か、自分が枠の中に居るか。**
///
/// 2 通り 見るのは、どちらか片方 では足りないから ——
/// 枠の外 から `?embed=1` で開かれることも在るし、`?embed=1` を付け忘れた
/// 枠の中 でも詰めたい。**枠の中 の判定は、同じ元 でなくても効く**
/// （`window.top` を読むだけで、中身を触らない）
[<Emit("(new URLSearchParams(location.search).get('embed') === '1') || (window.self !== window.top)")>]
let private isEmbedded () : bool = jsNative

/// **印は module の頭 で立てる。** ここは `js/Playground.js` の先頭 で走る ——
/// 手書きの `.js` を足す道は `guard-playground-boundaries` が塞いでいる
/// （追跡された `.js` は 0 件 でなければならない）ので、Fable が最も早い場所。
///
/// **`html` に付ける。** `body` だと、`html` にしか掛からない規則（`.page` の
/// 高さの元 になる `height: 100%`）から見えない
let private embedded =
  let e = isEmbedded ()
  if e then document.documentElement.classList.add "embed"
  e

/// 使い方の章。**html の `help-tab-◯◯` と `help-ch-◯◯` の後ろ半分**で、
/// 並びは目次の並び。
///
/// **要素名ではない。** ここに `fire` のような BulletML の名前を置くと、
/// `guard-playground-boundaries` の線（ブラウザ側に要素名を書かない）を越える。
///
/// 札と章の対はここでは数えない —— **数えるのは guard-help-chapters.ps1** で、
/// この並びと html の両方 を見る（片方 だけ足すと黙って通るので）
let private helpChapters = [ "start"; "write"; "trace"; "run"; "compare"; "io"; "keys" ]

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
  // 分かれるまで送っている最中か（v3.8）。**引きに来るのはこのあいだだけ** ——
  // 毎コマ の戻りに足すと、境界を越えて数を 1 つ 渡す 6 マイクロ秒 を一生 払う
  let mutable divergeWatch = false
  // 何コマ まで見ると言ったか。**言った数をそのまま出す** ——
  // ここで別の数を書くと、上限が 2 か所 に在ることになる
  let mutable divergeTo = 0
  // 録っている最中か（v2.5）。**押している最中にもう一度 押させない** ——
  // 2 本 目 の `MediaRecorder` が同じ面に付くと、どちらも半端な絵になる
  let mutable recording = false
  let mutable dotNet: obj = null
  let mutable frames = 0
  let mutable t0 = 0.
  let mutable lastN = -1
  let mutable lastFrame = -1
  // 追っている弾（v3.1 の段 3）。**真は host が持つ** —— ここは毎コマ 写すだけで、
  // 押した直後の値を握らない（消しで詰まると添字が動く）
  let mutable pickIdx = -1
  // いま描いている面の座標。**ヒープの窓は貯めない** —— WASM のヒープは
  // 育つと別の器になるので、押されたときに `heapF32` から引き直す
  let mutable packedOff = 0
  let mutable packedN = 0
  // 再開点の帯。**変わったときだけ書く**（弾数やコマ数と同じ扱い）
  let mutable lastPick = -2
  let mutable lastSerial = -2
  let mutable lastStops = -1
  let mutable lastDepth = -1
  let mutable lastPaths = -1
  let mutable lastLine = -2
  let mutable lastFromLine = -2
  let mutable resumeName = ""
  // 走っている場所の印（v3.1 の段 4）。**Apply の窓だけ。**
  //
  // 印は走行から出るので、**画面の字と走っている木が食い違った瞬間に嘘になる。**
  // 版の頭で測った —— どの 1 文字 を打っても木が読めるのは 40〜48% で、
  // `<` は 100% 読めなくなる。だから打った瞬間に下ろす。
  /// 3 桁 ごとに区切る。**桁が増えると読めなくなる** ——
  /// 撃った数は同梱でも 5 桁 に届く（15,917 発 出る本が在る）。
  ///
  /// `toLocaleString` を使わない —— あれは見ている人の設定で区切りが変わり、
  /// 場所によっては小数点と入れ替わる
  let groupDigits (n: int) =
    let s = string n
    let len = s.Length
    let sb = System.Text.StringBuilder()
    for i in 0 .. len - 1 do
      if i > 0 && (len - i) % 3 = 0 then sb.Append ',' |> ignore
      sb.Append s.[i] |> ignore
    sb.ToString()

  /// 式の横に出す数（v4.4）。**整数なら整数、そうでなければ小数 2 桁。**
  ///
  /// **桁区切りを入れない。** `groupDigits` は撃った発数のように
  /// 大きくなる数のためで、式の値は角度や速さ —— `1,024` と書くより
  /// `1024` のほうが、字の上の式と見比べやすい
  let showNumber (v: float) =
    if System.Double.IsNaN v then "?"
    elif v = System.Math.Floor v && abs v < 1e15 then string (int64 v)
    else
      let s = sprintf "%.2f" v
      // **後ろの 0 を落とす。** `2.50` より `2.5`
      if s.Contains "." then s.TrimEnd('0').TrimEnd('.') else s

  let mutable litOpen = false
  // いま光らせている添字。**-2 は「まだ何もしていない」** ——
  // -1（光らせるものが無い）と区別する
  let mutable litIndex = -2
  // 窓が開いているあいだの、名前の範囲の並び。**本文は変わらない**
  // （変わった瞬間に窓を閉じる）ので、走査は窓ごとに 1 回
  let mutable litSpans: NodeSpan list = []
  let mutable litScanned = false
  // 光らせた行（1 起点）。**帯に出す** —— 下地は見えている行にしか描かれない
  let mutable litLine = -1
  // 撃った場所（v3.2）。**弾を選んだ時点で決まって動かない**
  let mutable fromIndex = -2
  let mutable fromLine = -1
  // 選んだ弾（v3.6）。**`from` だけでは足りない** ——
  // 同じ `fire` から撃たれた別の弾は `from` が同じで、親の親が違う
  let mutable fromPick = -2
  // 系譜の行の列（1 起点）。**根に近い順**。帯に出す
  let mutable fromLines : int[] = [||]
  // 撃った数（v3.3 の段 1）。**出したかどうかだけ持つ** ——
  // 数そのものは出すたびに引き直す（貯めると、面を建て直したときに古い数が残る）
  let mutable tallyShown = false
  // 最後に出したコマ。**剰余で間引かない** —— `frame % 30 = 0` は
  // 「そのコマを必ず通る」前提で、コマ送りや飛ばしで通らないと
  // **次に当たるまで 30 コマ 待つ**（1 コマ ずつ進めていると永久に出ない）。
  // 実機で踏んだ：Frame 204 で止めた画面に、印が 1 つ も出なかった
  let mutable tallyAt = -1000
  // 何コマ に 1 回 出し直すか。**60 コマ/秒 なので 30 は 0.5 秒。**
  // 毎コマ 出し直さないのは、並べ替えが**腕の数ぶん**要るから ——
  // 弾の数と関係なく効くので、重い弾幕ほど損が目立つ
  let tallyEvery = 30
  // **撃った数**で切る上位いくつ。**同梱 176 本 で上位 3 つ が 93%**
  // （`fire` が 4 個 以上 の 130 本 の中央値）なので、5 で足りる。
  //
  // **生きている弾を持つ腕は、この数で切られない**（v4.0.1）——
  // 切ると字の上の合計が `Bullets:` に届かなくなる。
  // 和集合を作るのは `Main.TallyTop` の側
  let tallyTopN = 5
  /// 重さを出す行の上限（v3.9）。**割合で切るので、ここは天井**。
  /// 版の頭で数えた —— 5% 以上 の行は**中央 5 / 9 割 9 / 最大 12**
  let spanTopN = 20
  /// これ未満 の行には出さない。**全部 に出すと字が埋まる** ——
  /// 字に出せる行は中央 25 / 最大 87 で、1% で切ると中央 11 行 に印が付く
  let spanMinShare = 0.05
  let mutable spanShown = false
  /// 最後に書き直したコマ。**撃った数と別に持つ** ——
  /// あちらの数を見て「同じコマか」で判じると、
  /// **止めているあいだ毎コマ 引きに行く**（コマが進まないので差が 0 のまま）
  let mutable spanAt = -1000
  // 前に呼ばれたときのコマ（v4.0.1）。**進んだかどうかを見る** ——
  // 止めているあいだは `frame` が動かないので、30 コマ の間引きだけだと
  // **印が永久に古いまま**になる（`Bullets:` は毎コマ 書き直される）
  let mutable markFrame = -1
  // 字から弾（v4.7）。**カーソルが居る行の、読んだ木の添字**。
  // **-1 は「どの行でもない」** —— 面の側は 3 つ 目 の成分に添字を入れていて、
  // 撃たれていない弾（根の敵）は -1 なので、そこも自然に外れる
  let mutable fromIdx = -1
  // 字の右に印を出すか（v4.0.2）。**2 つ をまとめて切る** ——
  // 撃った数 も 割合 も同じ仕組み（30 コマ に 1 度 の並べ替え）の上に載っていて、
  // 別々 に切っても減り方は変わらない。切る理由は重さだけではなく、
  // **字を読みたいときに邪魔**でもある
  let mutable marksOn = true
  // 打った字を残すときの待ち（v3.5）。**0 は「予約が無い」**
  let mutable saveTimer = 0.0
  // 重さの帯（v3.3 の段 2）。**弾数とコマ時間の 2 本。**
  //
  // 1 本 では足りない —— 同じ本の中でも相関は中央値 0.569 で、
  // 176 本 中 55 本 は 0.5 も無い（版の頭で測った）。
  //
  // **環で持つ。** 毎コマ 足すので、伸びる入れ物だと確保が増え続ける
  let weightLen = 600
  let weightN = Array.zeroCreate<float> weightLen
  let weightMs = Array.zeroCreate<float> weightLen
  // そのコマが走行の何コマ 目 か。**いちばん重かったのがどこかを字に出す** ——
  // 立ち上がりなのか走行中なのかで、読み方がまるで違う
  let weightFrameAt = Array.zeroCreate<int> weightLen
  // 次に書く場所と、溜まった数（`weightLen` で頭打ち）
  let mutable weightAt = 0
  let mutable weightCount = 0
  // 面を建て直したかを見る。**コマ数が減ったら建て直された** ——
  // Reset / Apply / 選び直し で 0 に戻る
  let mutable weightLastFrame = -1
  /// 帯を**描いた**コマ（v4.9.1 の段 3）。溜めるのとは別 ——
  /// 溜めるのは毎コマ、描くのは間引く
  let mutable weightDrawnAt = -999
  /// 進まないまま呼ばれた回数。**止まっていたかを見分ける** ——
  /// 連続再生なら 0（毎回 進む）、コマ送りなら 1 以上。
  /// **止まっていたあとの 1 コマ は間引かない**（1 コマ ずつ追う人に効かない）
  let mutable weightIdleCalls = 0
  let mutable weightCanvas: HTMLCanvasElement = null
  // 木のノードになる要素名。**host から起動時に 1 回**。正本は Core の DTD.fs
  let mutable nodeNames: string list = []
  // 軌跡。**過去の位置を貯めない** —— 面を消さずに薄く塗り重ねるだけなので、
  // 確保は増えない（`draw` の但し書き）
  let mutable trail = false
  // 次の 1 コマ だけ面を全部 塗り潰す。軌跡が付いたまま別の弾幕へ行かない
  let mutable wipe = false
  let mutable canvas: HTMLCanvasElement = null
  let mutable canvasCtx: CanvasRenderingContext2D = null
  // 2 つ 目 の面（v2.1）。**大きさも敵も別**（向きは弾幕ごとに決まる）
  let mutable canvas2: HTMLCanvasElement = null
  let mutable canvasCtx2: CanvasRenderingContext2D = null
  let mutable field2W = 0.
  let mutable field2H = 0.
  let mutable enemyX2 = 240.
  let mutable enemyY2 = 80.
  // host からもらう語彙。正本は Core の DTD.fs。**表記が変わっても同じ**
  let mutable vocabulary: Vocab = { Elements = []; Expressions = []; Ce = []; CeLabels = []; CePlaces = []; Frames = []; TopPrefix = "" }
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
      // 弾を押すと、その弾を追う（v3.1 の段 3）。**自機の操作と同じ面の上**だが、
      // `mousemove` は自機、`click` は選び —— 押しても自機は動かない
      c.addEventListener (
        "click",
        fun (ev: Event) ->
          let e = ev :?> MouseEvent
          let r = c.getBoundingClientRect ()
          let cv = c :?> HTMLCanvasElement
          self.pickBullet ((e.clientX - r.left) * (float cv.width / r.width),
                     (e.clientY - r.top) * (float cv.height / r.height))
      )

  /// いちばん近い弾を追う。**離れていたら追うのをやめる** ——
  /// 面のどこを押しても何かが選ばれると、選び直しと選び解除が同じ操作になる。
  ///
  /// **探すのはここ。** 座標は WASM ヒープの `float32[]` をそのまま読んでいるので
  /// （`Playfield.Pack`）、こちらには全部 の座標がもう在る —— host へ座標を
  /// 送り返すと、同じ配列を 2 度 運ぶことになる。
  ///
  /// 半径は弾の見た目（4 px 角）より広く取る。**指で押せる大きさが要る**
  member _.pickBullet(x: float, y: float) =
    if isNull dotNet then ()
    else
      let mutable best = -1
      // **半径の 2 乗 で比べる。** 平方根は順を変えないので取らない
      let mutable bestD = 12. * 12.
      if packedN > 0 then
        // **1 点 につき 3 つ**（v4.7。x / y / 撃った腕の添字）
        let a = subarray (heapF32 ()) packedOff (packedOff + packedN * 3)
        let mutable i = 0
        while i < packedN do
          let dx = f32 a (i * 3) - x
          let dy = f32 a (i * 3 + 1) - y
          let d = dx * dx + dy * dy
          if d <= bestD then
            bestD <- d
            best <- i
          i <- i + 1
      if best >= 0 then invoke1 dotNet "PickBullet" (box best) |> ignore
      else invoke0 dotNet "UnpickBullet" |> ignore

  /// **2 面 のときは弾数を分けて出す**（`n / n2`）——
  /// 足してしまうと、並べて比べているのに「どちらが濃いか」が読めない。
  /// `n2` が負なら 2 面 目 は無い（0 は「弾が 1 つ も無い面」で別の意味）
  member _.hud(n: int, n2: int, t: float, frame: int) =
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
    // **合わせた数で「変わったか」を見る。** 片方 だけ見ると、
    // もう片方 だけが動いたコマで書き換わらない
    let key = if n2 >= 0 then n * 100000 + n2 else n
    if not (isNull b) && key <> lastN then
      lastN <- key
      b.textContent <- if n2 >= 0 then string n + " / " + string n2 else string n
    // **飛んでいる最中はここが速く動く。** 進んでいることの唯一の合図なので、
    // 弾数と同じく変わったときだけ書く（毎フレーム書くと DOM を無駄に触る）
    let f = el "frame-count"
    if not (isNull f) && frame <> lastFrame then
      lastFrame <- frame
      f.textContent <- string frame

  /// 追っている弾の再開点（v3.1 の段 3）。
  ///
  /// **名前は番号が変わったときだけ引く。** 再開点は `wait` の間ずっと同じ物なので、
  /// 毎コマ 引くと同じ字を 60 回/秒 境界越しに作ることになる。
  ///
  /// 数も一緒に出すのは、**「戻れた」と「正しい所へ戻れた」が別**だから ——
  /// `stop` は 0 か 2 以上（1 は出ない）で、これは走行の構造がそのまま出た数
  /// 走っている場所を字の上で光らせる（v3.1 の段 4）。**Apply の窓だけ。**
  ///
  /// 渡ってくるのは**読んだ木を書いてある順に歩いた添字**（`Main.StepFrame`）。
  /// 位置は木に無く、字はこちらにしか無いので、あいだを渡るのは順番だけ ——
  /// 木の k 番目 と札の k 番目 が揃うことは v2.9 で測ってある
  /// （3 表記 で 176 / 176。F# の CE は結べないので空が返る）。
  ///
  /// **走査は窓ごとに 1 回。** 窓が開いているあいだ本文は変わらない
  /// （変わった瞬間に閉じる）ので、添字が動いても数え直さない
  member _.lightRunning(order: int) : int =
    if not litOpen then
      if litIndex <> -2 then
        litIndex <- -2
        litLine <- -1
        Monaco.clearHighlight ()
      -1
    else
      if not litScanned then
        litScanned <- true
        litSpans <- current.NodeSpans (Monaco.getValue ()) nodeNames
      if order <> litIndex then
        litIndex <- order
        // **並びの外は光らせない。** 添字が外れているのに隣を光らせると、
        // 「そこで止まっている」という嘘になる
        if order < 0 || order >= List.length litSpans then
          litLine <- -1
          Monaco.clearHighlight ()
        else
          // **こちらは名前だけ。** 再開点は 100% が `wait` で、葉なので
          // 名前で足りる（同梱 383,655 コマ で数えた）
          let sp = List.item order litSpans
          litLine <- Monaco.highlight sp.NameStart sp.NameStop
      litLine

  /// 撃った場所を光らせる（v3.2 / v3.6）。戻りは**いちばん近い親**の
  /// 行（1 起点）。**無ければ -1。**
  ///
  /// **再開点と別の入れ物・別の色。** 再開点は毎コマ 変わり、こちらは
  /// 弾を選んだ時点で決まる —— 変わったときだけ字へ寄せる。
  ///
  /// **系譜ぜんぶ を出す**（v3.6）。撃った弾がまた撃つので、1 発 の弾には
  /// 撃った弾の列が在る —— 同梱の深さは**最大 5 段 / 中央 3 段**（測った）。
  ///
  /// **引くのは選び直したときだけ。** 系譜は弾を選んだ時点で決まって動かず、
  /// `pick` が変わらなければ同じ物が返る
  member _.lightOrigin(from: int, pick: int) : int =
    if not litOpen then
      if fromIndex <> -2 then
        fromIndex <- -2
        fromPick <- -2
        fromLine <- -1
        fromLines <- [||]
        Monaco.clearOrigin ()
      -1
    else
      if not litScanned then
        litScanned <- true
        litSpans <- current.NodeSpans (Monaco.getValue ()) nodeNames
      // **`from` だけでは足りない。** 同じ `fire` から撃たれた別の弾は
      // `from` が同じで、親の親が違う —— 選び直したかも見る
      if from <> fromIndex || pick <> fromPick then
        fromIndex <- from
        fromPick <- pick
        let span = List.length litSpans
        let raw =
          if pick < 0 then [||]
          else unbox<float[]> (invoke0 dotNet "PickedLineage")
        // **並びの外は出さない**（光らせる側と同じ守り）
        let spans = ResizeArray<int * int * int * int>()
        let lines = ResizeArray<int>()
        for v in raw do
          let idx = int v
          if idx >= 0 && idx < span then
            let sp = List.item idx litSpans
            spans.Add(sp.OpenStart, sp.OpenStop, sp.CloseStart, sp.CloseStop)
        if spans.Count = 0 then
          fromLine <- -1
          fromLines <- [||]
          Monaco.clearOrigin ()
        else
          fromLine <- Monaco.highlightOriginChain (spans.ToArray())
          // 帯に出す行の列。**字へ寄せたあとに引く**（行番号は Monaco が持つ）
          for (os, _, _, _) in spans do
            lines.Add(Monaco.lineOfOffset os)
          fromLines <- lines.ToArray()
      fromLine

  /// 重さの帯に 1 コマ 分 足して描く（v3.3 の段 2）。
  ///
  /// **弾数とコマ時間の 2 本。** 1 本 では足りないことは版の頭で測ってある ——
  /// 同じ本の中でも相関は中央値 0.569 で、176 本 中 55 本 は 0.5 も無い。
  ///
  /// **それぞれの最大で正規化する。** 単位が違う（発 と ミリ秒）ので
  /// 同じ目盛りには乗らない —— 見たいのは「どこで跳ねたか」であって
  /// 「どちらが大きいか」ではない
  member _.weight(n: int, ms: float, frame: int) =
    if isNull weightCanvas then weightCanvas <- el "weight" |> unbox
    if isNull weightCanvas then ()
    else
      // 面を建て直したら捨てる。**残すと前の弾幕の山が残ったまま**
      if frame < weightLastFrame then
        weightAt <- 0
        weightCount <- 0
        weightLastFrame <- frame
      // **進んだコマ数。** この帯は rAF ごとに呼ばれるので、
      // 「呼ばれた回数」と「進んだコマ数」は同じではない ——
      // Pause では 0、倍速では 2 や 4、飛んでいる最中は数十
      let advanced = frame - weightLastFrame
      // **立ち上がりの 1 コマ は溜めない。** 実機で見たら、最初のコマだけ
      // 26.0 ms 出て（以降 は 1 ms 未満）、そこで正規化された残りが
      // 全部 底に張り付いた —— あれは走行の重さではなく初回の費用
      // （JIT と、面が最初に伸ばす入れ物）。
      //
      // **2 コマ 目 以降 も重いなら、上の字に `（Frame 2）` と出る** ——
      // 捨てた数を増やす前に、そこを見る
      //
      // **進んでいないコマは溜めない。** 前は rAF ごとに足していたので、
      // **Pause で止めていても同じ数が入り続けて波形が流れた** ——
      // 「止めているのに動く」は、見る道具としては嘘をついている
      // **止まっていたあとの 1 コマ か。** 間引きを外す印（下で読む）
      let mutable afterPause = false
      if advanced > 0 && frame > 1 then
        weightN.[weightAt] <- float n
        // **1 コマ あたりに直す。** 倍速や「飛ぶ」では 1 回 の呼び出しで
        // 何コマ か進むので、割らないと字の `ms/Frame` が嘘になる
        weightMs.[weightAt] <- ms / float advanced
        weightFrameAt.[weightAt] <- frame
        weightAt <- (weightAt + 1) % weightLen
        if weightCount < weightLen then weightCount <- weightCount + 1
        afterPause <- weightIdleCalls > 0
        weightIdleCalls <- 0
      else weightIdleCalls <- weightIdleCalls + 1
      weightLastFrame <- frame

      // --- ここまでが溜める。以降 が描く -----------------------------------
      //
      // **描くほうだけ間引く**（v4.9.1 の段 3）。この帯は幅いっぱいを
      // 毎コマ 塗り直していて、**1 コマ の 25% を食っていた** ——
      // 配った物を A/B/B/A で測って 2.0 ms 対 1.5 ms（下限 0.5 ms）。
      //
      // **溜めるのは毎コマ のまま。** 間引くと山が落ちる。
      //
      // **剰余でなく差で数える**（v4.0.2 で踏んだ）—— `frame % 2 = 0` だと
      // コマ送りで踏まないコマが出て、帯が永久に更新されない。
      // 建て直し（`frame` が戻る）でも描き直す。
      //
      // **2 コマ に 1 度。** 帯は 600 点 を幅いっぱいに並べるので、
      // 1 コマ の進みは 2 から 4 px —— 30 回/秒 で足りる。
      //
      // **コマ送りは間引かない。** 1 コマ ずつ追っている人には
      // 「押したのに帯が変わらない」が出る（実機で数えたら 6 回 中 3 回）——
      // 止まっていたあとの 1 コマ は必ず描く
      if not afterPause && frame - weightDrawnAt < 2 && frame >= weightDrawnAt then () else

      weightDrawnAt <- frame

      // **箱に合わせて内部解像度を取り直す。**
      //
      // 素は html の `width="1200" height="48"` で固定していて、
      // CSS が幅いっぱいへ引き伸ばしていた —— **横だけ 1.35 倍**（1200 -> 1620
      // を実測）で、縦は 1 倍。線 2 本 のときは気にならなかったが、
      // **字を足した時点で嘘になった** —— 10px で焼いた字が横へ伸びて、
      // しかも拡大なのでぼける。
      //
      // **dpr も掛ける。** 掛けないと、2 倍 の画面で同じことがもう一度 起きる。
      //
      // `width` を書くと ctx の状態が消えるので、**変わったときだけ**書いて、
      // そのあとで倍率を入れ直す
      let ratio = dpr ()
      let w = float weightCanvas.clientWidth
      let h = float weightCanvas.clientHeight
      if w > 0. && h > 0. then
        let needW = int (w * ratio + 0.5)
        let needH = int (h * ratio + 0.5)
        if weightCanvas.width <> needW || weightCanvas.height <> needH then
          weightCanvas.width <- needW
          weightCanvas.height <- needH
      let ctx = getCtx weightCanvas
      // **以降 の座標は CSS の画素。** 描く側は倍率を知らないでよい
      ctx.setTransform (ratio, 0., 0., ratio, 0., 0.)
      ctx.fillStyle <- U3.Case1 "#101018"
      ctx.fillRect (0., 0., w, h)
      // **字の段と波形の段を分ける。**
      //
      // 前は同じ面に重ねて描いていて、**線 2 本 が字の中を通っていた** ——
      // 帯は画面の幅いっぱい（dpr 1.5 の 4K で 2,481px）なので、
      // 線は端から端まで在るのに字は左上の一角 にしかなく、**広い画面ほどひどい。**
      // 板を字の下に敷くだけでも重なりは消えるが、
      // **字が線の上に浮いている**ままで読みづらさが残った。
      let bandH = 20.
      ctx.fillStyle <- U3.Case1 "#181824"
      ctx.fillRect (0., 0., w, bandH)
      ctx.fillStyle <- U3.Case1 "#2c2c3c"
      ctx.fillRect (0., bandH - 1., w, 1.)
      if weightCount >= 2 then
        // **古い順に読む。** 環なので、溜まりきる前は 0 から、
        // 溜まったあとは書いた次から
        let start = if weightCount < weightLen then 0 else weightAt
        let at (k: int) = (start + k) % weightLen
        let mutable maxN = 1.0
        let mutable maxMs = 0.0001
        let mutable maxMsFrame = -1
        for k in 0 .. weightCount - 1 do
          let i = at k
          if weightN.[i] > maxN then maxN <- weightN.[i]
          if weightMs.[i] > maxMs then
            maxMs <- weightMs.[i]
            maxMsFrame <- weightFrameAt.[i]
        // **波形は字の段の下だけ。** 上端 は `bandH`、下端 は `h`
        let line (values: float[]) (top: float) (color: string) =
          ctx.strokeStyle <- U3.Case1 color
          ctx.lineWidth <- 1.0
          ctx.beginPath ()
          for k in 0 .. weightCount - 1 do
            let x = w * float k / float (max 1 (weightCount - 1))
            // **1px の余白を上下 に取る。** 天井に張り付くと線が切れて見える
            let y = h - 1.0 - (h - bandH - 2.0) * values.[at k] / top
            if k = 0 then ctx.moveTo (x, y) else ctx.lineTo (x, y)
          ctx.stroke ()
        // **弾数が先。** コマ時間を後に描くと、重なったとき時間が上に出る ——
        // 見たいのは時間のほう
        line weightN maxN "#5aa0e0"
        line weightMs maxMs "#e07a5a"
        // 目盛りの代わりに最大値を字で。**線だけだと桁が分からない。**
        //
        // **「いちばん重かったのがどこか」まで出す。** 数だけだと、
        // 立ち上がりの 1 発 なのか走行中に跳ねたのかが読めない。
        //
        // **数の色を線の色に合わせる。** どちらの数がどちらの線かは、
        // 字で断らなくても色で分かる —— 帯が広いと凡例を離して置けない
        // **等幅で書く。** 一度 `system-ui` にしたら、**字が左右へぶれた** ——
        // プロポーショナルは数字の字幅が桁ごとに違うので（`1` が細い）、
        // `7.4` が `8.0` になるだけで後ろが動く。**毎コマ 書き換える字で使わない。**
        ctx.font <- "bold 13px ui-monospace, SFMono-Regular, Consolas, monospace"
        ctx.textBaseline <- "middle"
        let mutable x = 8.
        let put (s: string) (color: string) =
          ctx.fillStyle <- U3.Case1 color
          ctx.fillText (s, x, bandH / 2.)
          x <- x + ctx.measureText(s).width
        // **桁を揃える枠は置かない。** 一度 最大桁（6 字）で右へ揃えてみたが、
        // ふだんは 3 字 なので**隙間が間延びした。**
        //
        // 置かなくてよい理由 —— **ここに出す 3 つ はどれも最大値で、
        // 走行の中では単調にしか増えない。** 等幅なら同じ桁数のあいだは
        // 1px も動かず、動くのは桁が増えた瞬間だけ（実測 200 コマ で 2 回）。
        // **毎コマ の揺れと、たまの繰り上がりは別**。
        //
        // **`Frame` と書く。** 下の帯が `Frame:` と出しているので、
        // ここだけ「コマ」だと同じものに 2 通り の名前が付く
        put ("弾 " + groupDigits (int maxN)) "#7cbcf0"
        put "  /  " "#5a5a6a"
        put (maxMs.ToString "F1" + " ms/Frame") "#f09a78"
        if maxMsFrame > 0 then put ("　（Frame " + groupDigits maxMsFrame + "）") "#9aa0aa"

  /// 撃った数と、**いま生きている数**を字の右へ出す（v3.3 の段 1 / v4.0.1）。
  /// **印ではなく数を足す。**
  ///
  /// 走っている場所（黄）と撃った場所（緑）がもう在るので、
  /// **3 色 目 を足さない** —— どれが何かを覚えられなくなる。
  /// 数そのものを字の後ろに置けば、色を使わずに済む。
  ///
  /// 出す形は **`×24（12）`** —— 24 発 撃って、いま 12 発 生きている。
  /// **括弧で 1 つ の印にまとめる**（別の印にすると、`%`（重さ）と合わせて
  /// 覚えるものが 3 つ になる）。
  ///
  /// **括弧の中の合計は `Bullets:` と一致する。** そのために
  /// **生きている弾を持つ腕は 1 本 も落とさない** —— 上位 n だけでは
  /// 足りないと版の頭で数えた（生きている腕は中央 5 / 9 割 14 / 最大 34 本 で、
  /// 176 本 中 85 本 が 5 本 を超える）。並べるのは `TallyTop` の側。
  ///
  /// **間引く。** 並べ替えは腕の数ぶんで、弾の数と関係なく効く
  member _.lightTally(frame: int) =
    if not litOpen then
      if tallyShown then
        tallyShown <- false
        Monaco.clearTally ()
    // **止まったら 1 度 だけ書き直す。** 走っているあいだは 30 コマ に 1 度 で
    // 足りるが、止めた瞬間 の印は最大 30 コマ 前 のもの ——
    // **そこで人は `Bullets:` と見比べる。**
    //
    // `frame = markFrame` は「前に呼ばれたときからコマが進んでいない」。
    // 書き直したら `tallyAt <- frame` になるので、**止まっているあいだ
    // 繰り返しはしない**（1 度 だけ）
    elif frame - tallyAt >= tallyEvery || (frame = markFrame && tallyAt <> frame) then
      tallyAt <- frame
      if not litScanned then
        litScanned <- true
        litSpans <- current.NodeSpans (Monaco.getValue ()) nodeNames
      let raw = unbox<float[]> (invoke1 dotNet "TallyTop" (box tallyTopN))
      // **先頭 2 つ は 総数 と 腕の数。** そこから後ろが
      // (添字, 撃った延べ, 生きている) の 3 つ 組
      let rows = (raw.Length - 2) / 3
      let span = List.length litSpans
      let marks = ResizeArray<int * int * string>()
      for k in 0 .. rows - 1 do
        let idx = int raw.[2 + k * 3]
        let cnt = int raw.[3 + k * 3]
        let now = int raw.[4 + k * 3]
        // **並びの外は出さない。** 添字が外れているのに隣へ数を付けると、
        // 「その行が撃った」という嘘になる（光らせる側と同じ守り）
        //
        // **撃った数が 0 でも、生きているなら出す。** 撃った数は面ごとに
        // 数え直すので（Reset / Apply）、**建て直した直後は 0 のまま
        // 弾だけが生きている**ことが在る —— そこで落とすと合計が合わない
        if idx >= 0 && idx < span && (cnt > 0 || now > 0) then
          let sp = List.item idx litSpans
          // **札の後ろに置く。** 名前の後ろに置くと `<fire ×252>` になって
          // **属性のように読める**（実機で見て気づいた）——
          // 開き札の終わりなら `<fire> ×252` で、字と添え物が分かれる
          marks.Add(sp.OpenStart, sp.OpenStop, "  ×" + groupDigits cnt + "（" + groupDigits now + "）")
      tallyShown <- marks.Count > 0
      Monaco.showTally (marks.ToArray())

  /// 走った重さを字の右へ出す（v3.9）。**撃った数の隣 の軸。**
  ///
  /// あちらは「その `fire` が何発 撃ったか」、こちらは
  /// **「その行に弾が何 弾コマ 居たか」** —— 撃たない行にも重さは在る
  /// （毎コマ 読み直される `changeDirection` など）。
  ///
  /// **出すのは割合。** 弾コマ の生の数は本によって桁が違うので、
  /// 「全体の何 %」でしか比べられない。
  ///
  /// **重さは 1 行 に集まりきらない**（版の頭で数えた。上位 5 行 で 77.8%、
  /// いちばん低い本は 23.5%）ので、上位 n 個 ではなく**割合で切る**
  member _.lightSpans(frame: int) =
    if not litOpen then
      if spanShown then
        spanShown <- false
        Monaco.clearSpans ()
    // **止まったら 1 度 だけ書き直す**（`lightTally` と同じ理由）
    elif frame - spanAt >= tallyEvery || (frame = markFrame && spanAt <> frame) then
      spanAt <- frame
      if not litScanned then
        litScanned <- true
        litSpans <- current.NodeSpans (Monaco.getValue ()) nodeNames
      let raw = unbox<float[]> (invoke1 dotNet "SpanTop" (box spanTopN))
      let total = raw.[0]
      let pairs = (raw.Length - 2) / 2
      let span = List.length litSpans
      let marks = ResizeArray<int * string>()
      if total > 0.0 then
        for k in 0 .. pairs - 1 do
          let idx = int raw.[2 + k * 2]
          let cnt = raw.[3 + k * 2]
          let share = cnt / total
          // **並びの外は出さない**（`lightTally` と同じ守り）
          if idx >= 0 && idx < span && share >= spanMinShare then
            let sp = List.item idx litSpans
            // **渡すのは開き札の頭 だけ。** 置き先は行末で、`Monaco` が引く
            marks.Add(sp.OpenStart, "  " + string (int (share * 100.0 + 0.5)) + "%")
      spanShown <- marks.Count > 0
      Monaco.showSpans (marks.ToArray())

  /// カーソルが居る行が撃った弾を、面の上で塗り分ける（v4.7）。
  ///
  /// **v3.2 の逆向き。** あちらは「押した弾 -> 字」、こちらは「字 -> 弾」——
  /// **材料は同じ `Live.From`**（撃った腕の添字）で、
  /// **系譜は 1 度 も辿らない**（版の頭で確かめた）。
  ///
  /// **窓が閉じていれば何もしない。** 字と走っている木が揃っていないので、
  /// 添字が別のものを指す
  member _.lightFrom() =
    if not litOpen then
      if fromIdx >= 0 then fromIdx <- -1
    else
      let off = Monaco.cursorOffset ()
      if off < 0 then fromIdx <- -1
      else
        if not litScanned then
          litScanned <- true
          litSpans <- current.NodeSpans (Monaco.getValue ()) nodeNames
        // **札の中に居るか**で決める。中身（子の要素）に居るときは
        // その子のほうが近い —— **いちばん内側 を採る**
        let mutable best = -1
        List.iteri
          (fun i (sp: NodeSpan) ->
            if off >= sp.OpenStart && off < sp.OpenStop then best <- i)
          litSpans
        fromIdx <- best

  /// 印の窓を開ける。**本文と走っている木が同じところで揃った瞬間だけ。**
  ///
  /// 呼ぶのは Apply が通ったとき・弾幕を選び直したとき・表記を書き直したとき・
  /// 起動時。**Open は呼ばない** —— 読んだだけで載せていないので、
  /// 字と走っている木は別物
  member _.openLight() =
    litOpen <- true
    litScanned <- false
    litIndex <- -2
    fromIndex <- -2
    // 窓を開けたら**次のコマで出す**（間引きの数を待たない）——
    // 建て直した直後は数が 0 なので、待つと空白の 0.5 秒 が見える
    tallyAt <- -1000
    spanAt <- -1000

  /// 印の窓を閉じる。**打った瞬間に。**
  member _.closeLight() =
    // 字から弾（v4.7）。**窓を閉じたら消す**
    fromIdx <- -1
    if litOpen then
      litOpen <- false
      litScanned <- false
      litSpans <- []
      tallyShown <- false
      Monaco.clearTally ()
      spanShown <- false
      Monaco.clearSpans ()
      litIndex <- -2
      fromIndex <- -2
      fromLine <- -1
      Monaco.clearHighlight ()
      Monaco.clearOrigin ()

  member _.focusHud(pick: int, stops: int, depth: int, serial: int, paths: int,
                    line: int, fromLine: int) =
    let fo = el "focus"
    if isNull fo then ()
    elif pick = lastPick && serial = lastSerial && stops = lastStops
         && depth = lastDepth && paths = lastPaths && line = lastLine
         && fromLine = lastFromLine then ()
    else
      if serial <> lastSerial then
        resumeName <-
          if isNull dotNet || serial < 0 then ""
          else string (invoke0 dotNet "ResumeName")
      lastPick <- pick
      lastSerial <- serial
      lastStops <- stops
      lastDepth <- depth
      lastPaths <- paths
      lastLine <- line
      lastFromLine <- fromLine
      // 撃った場所（v3.2）と、そこまでの系譜（v3.6）。
      //
      // **再開点が無いコマでも出す。** 出どころは弾を選んだ時点で決まっていて、
      // 走行がいま止まっているかとは関係が無い —— v3.2 では「再開点なし」で
      // 早く返っていたので、**同梱の 86.66% のコマで出どころが消えていた**
      // （v3.6 で系譜を出して初めて気づいた）。
      //
      // 2 段 以上 なら矢印で繋ぐ —— **根に近い順**なので、読むと
      // 「ここから撃たれて、そこからここへ」になる。
      // 同梱の深さは最大 5 段（測った）ので 1 行 に収まる
      let origin =
        if fromLines.Length >= 2 then
          " ／ 出どころ " + String.concat " → " (fromLines |> Array.map string) + " 行"
        elif fromLine >= 1 then " ／ 出どころ " + string fromLine + " 行"
        else ""
      fo.textContent <-
        if pick < 0 then ""
        // **追っているのに再開点が無いコマは在る** —— 台本を持たない弾と、
        // 全 top が終わったコマ（同梱の 86.66%）。空にせず、そう書く
        elif serial < 0 then "追跡: 再開点なし" + origin
        else
          // **道が 2 本 以上 のときは 1 本 目 だけ出している。** 数で見せる ——
          // 黙って落とすと、出ている場所が全部 だと読めてしまう
          // **行番号を先に出す。** 下地は見えている行にしか描かれないので、
          // 画面の外に在るときは、この数だけが在り処を言う
          (if line >= 1 then "追跡: " + string line + " 行 の " + resumeName
           else "追跡: " + resumeName)
          + "（stop " + string stops + " / 鎖 " + string depth
          + " / 道 " + string paths + "）"
          // **根の敵は撃たれていないので出ない**（上で組んである）
          + origin

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
          enemyY <- unbox<float> (jsItem v 5)
          // 2 つ 目 の敵。**無ければ -1** —— そのときは触らない
          let ex2 = unbox<float> (jsItem v 6)
          if ex2 >= 0. then
            enemyX2 <- ex2
            enemyY2 <- unbox<float> (jsItem v 7))
        (fun err -> setError (errText err))

  /// 毎コマ host から来る、自機と面の大きさ。
  ///
  /// **大きさが変わったら canvas を建て直す。** 向きは弾幕が持っているので、
  /// 変わるのは弾幕が変わったとき —— 建て直す道は 8 本 以上 あるので、
  /// 「変わったら」で拾うほうが呼び忘れが起きない
  member _.setView(px: float, py: float, w: float, h: float, w2: float, h2: float) =
    drawX <- px
    drawY <- py
    let mutable changed = false
    if w > 0. && (w <> fieldW || h <> fieldH) then
      fieldW <- w
      fieldH <- h
      changed <- true
      let c = el "stage"
      if not (isNull c) then
        let cv = c :?> HTMLCanvasElement
        cv.width <- int w
        cv.height <- int h
    // 2 つ 目。**幅が 0 なら「2 面 目 は無い」** —— canvas ごと隠す
    let c2 = el "stage2"
    if not (isNull c2) then
      if w2 <= 0. then
        if not (c2.hasAttribute "hidden") then c2.setAttribute ("hidden", "")
      else
        if c2.hasAttribute "hidden" then c2.removeAttribute "hidden"
        if w2 <> field2W || h2 <> field2H then
          field2W <- w2
          field2H <- h2
          changed <- true
          let cv2 = c2 :?> HTMLCanvasElement
          cv2.width <- int w2
          cv2.height <- int h2
    // **どちらかが変わったら 1 回 だけ引く。** 置き場所は 1 本 の口が
    // 両方 を返すので、面ごとに引くと往復が 2 倍 になる
    if changed then self.refreshPlaces ()

  member _.ensureCtx() : CanvasRenderingContext2D =
    if not (isNull canvasCtx) then canvasCtx
    else
      let c = el "stage"
      if isNull c then null
      else
        canvas <- c :?> HTMLCanvasElement
        canvasCtx <- getCtx canvas
        canvasCtx

  /// 2 つ 目 の面の下地（v2.1）。**1 つ 目 と別に持つ** ——
  /// `getContext` は同じ canvas なら同じ物を返すが、canvas が違えば別
  member _.ensureCtx2() : CanvasRenderingContext2D =
    if not (isNull canvasCtx2) then canvasCtx2
    else
      let c = el "stage2"
      if isNull c then null
      else
        canvas2 <- c :?> HTMLCanvasElement
        canvasCtx2 <- getCtx canvas2
        canvasCtx2

  /// 1 面 を塗る。**`draw` と `draw2` の中身はこれ 1 本。**
  ///
  /// 2 面 に増やしたときに写しを作らない —— 写すと、弾の描き方を変えたときに
  /// 片方 だけ変わって「同じ弾幕なのに絵が違う」が作れてしまう。
  ///
  /// **軌跡を落とすのはここではない。** 2 面 とも塗ってから落とす
  /// （ここで落とすと、1 面 目 で消えて 2 面 目 に効かない）
  member _.paint
    (c2d: CanvasRenderingContext2D)
    (cv: HTMLCanvasElement)
    (ex: float)
    (ey: float)
    (packed: obj)
    (n: int)
    (pick: int)
    : int =
    let n = n ||| 0
    // **軌跡は過去の位置を持たない。** 面を薄く塗るだけにすると、前のコマの
    // 弾がそのまま残って尾に見える。確保は 0 バイト。
    //
    // 消さずに残す形にはしない —— 弾の多い弾幕だと数秒で面が真っ白になる。
    // 薄く塗り重ねるほうは、放っておいても消えるので消す道が要らない
    // （切り替えと弾幕の差し替えだけ、1 コマ 塗り潰す）
    if trail && not wipe then c2d?fillStyle <- "rgba(16, 16, 24, 0.12)"
    else c2d?fillStyle <- "#101018"
    c2d.fillRect (0., 0., float cv.width, float cv.height)
    c2d?fillStyle <- "#66ccff"
    c2d.beginPath ()
    c2d.arc (drawX, drawY, 5., 0., System.Math.PI * 2.)
    c2d.fill ()
    c2d?fillStyle <- "#ffffff"
    if n <= 0 then
      c2d.beginPath ()
      c2d.arc (ex, ey, 6., 0., System.Math.PI * 2.)
      c2d.fill ()
      0
    else
      let mutable i = 0
      while i < n do
        c2d.fillRect (f32 packed (i * 3) - 2., f32 packed (i * 3 + 1) - 2., 4., 4.)
        i <- i + 1
      // 字から弾（v4.7）。**カーソルが居る行が撃った弾を塗り直す** ——
      // **白の上から描く**ので、先に全部 白で描いてから重ねる。
      //
      // **丸にしない。** 追っている弾（v3.1）が丸なので、同じ形にすると
      // 2 つ が混ざる —— こちらは四角のまま色だけ変える
      if fromIdx >= 0 then
        c2d?fillStyle <- "#66ff99"
        let mutable k = 0
        while k < n do
          if int (f32 packed (k * 3 + 2)) = fromIdx then
            c2d.fillRect (f32 packed (k * 3) - 2., f32 packed (k * 3 + 1) - 2., 4., 4.)
          k <- k + 1
        c2d?fillStyle <- "#ffffff"
      // 追っている弾（v3.1 の段 3）。**全部 の弾のあとに描く** ——
      // 先に描くと、あとから来た弾に上書きされて消える
      if pick >= 0 && pick < n then
        c2d?fillStyle <- "#ffcc33"
        c2d.beginPath ()
        c2d.arc (f32 packed (pick * 3), f32 packed (pick * 3 + 1), 6., 0., System.Math.PI * 2.)
        c2d.fill ()
      n

  member _.draw(packed: obj, n: int) : int =
    let c2d = self.ensureCtx ()
    if isNull c2d then 0 else self.paint c2d canvas enemyX enemyY packed n pickIdx

  /// 2 つ 目 の面。**`n` が負なら出さない**（0 は「弾が 1 つ も無い面」で別の意味）
  member _.draw2(packed: obj, n: int) : int =
    if n < 0 then 0
    else
      let c2d = self.ensureCtx2 ()
      // **2 つ 目 の面では選べない**（v3.1 の段 3 は 1 面 だけ）
      if isNull c2d then 0 else self.paint c2d canvas2 enemyX2 enemyY2 packed n (-1)

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
            // **ここで字と走っている木が揃った。** 印の窓を開ける（v3.1 の段 4）
            self.openLight ()
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
                    Monaco.Message = string m?message
                    // Apply が返すのは「読めない」。**そこは走らない側**
                    Monaco.Severity = Monaco.Severity.Error })
              |> Array.toList
              |> Monaco.markAll
              markedAt <- true)
        (fun err -> setError (errText err))

  /// 意味の層を引き直す（v2.3）。**打鍵ごとに呼ぶ。**
  ///
  /// 読めて・組めても走らないものが在る —— `top` で始まる定義が 1 つ も
  /// 無い本は、**読めて・組めて・黙って何も起きない。**
  ///
  /// **WASM へ行かない。** 字から数えるので、Apply を待たずに出せる
  /// （v1.8 で未決にした「打鍵ごとの波線」がここで実現する）。
  /// いちばん長い本 29,190 字 で 0.488 ms / 回（実測）。
  ///
  /// **文面はここで組む。** 器（`Semantics`）は持たない ——
  /// 持たせると、波線と別の出口で文面が割れて、どちらも単独では正しく見える
  /// 式の横に出す値（v4.4）。**見えている行のぶんだけ。**
  ///
  /// **評価は面がやる**（`EvalExprs`）—— 器は評価器を持たない。
  /// ここは並べ替えと畳みだけ。
  ///
  /// **同じ式は 1 度 しか渡さない。** いちばん長い本では 71 個 が 8 個 に畳まる
  /// （版の頭で数えた）—— 境界を越えるのは 1 回 だが、渡す配列は短いほうがよい。
  ///
  /// **起動前 は空。** 面がまだ無いので値が出せない —— そこで 0 を出すと
  /// 「畳んだ値が 0」に見える
  member _.hintsIn (src: string) (fromLine: int) (toLine: int) : (int * int * string) list =
    if isNull dotNet then []
    else
      let spots =
        current.Hints src
        |> List.filter (fun h -> h.Line >= fromLine && h.Line <= toLine)
      if List.isEmpty spots then []
      else
        let texts = spots |> List.map (fun h -> h.Text) |> List.distinct
        let values =
          unbox<float[]> (invoke1 dotNet "EvalExprs" (box (List.toArray texts)))
        // 式 -> 値。**畳んだぶんを戻す**
        let byText = System.Collections.Generic.Dictionary<string, float>()
        List.iteri (fun i (t: string) -> byText.[t] <- values.[i]) texts
        spots
        |> List.choose (fun h ->
             match byText.TryGetValue h.Text with
             // **読めない式は出さない**（そちらは波線の担当）
             | true, v when not (System.Double.IsNaN v) ->
                 Some (h.Line, h.Column, "= " + showNumber v)
             | _ -> None)

  member _.refreshFindings() =
    if not (Monaco.ready ()) then ()
    else
      let marks =
        current.Findings(Monaco.getValue ())
        |> List.map (fun f ->
            match f.Kind with
            | Semantics.NoEntryPoint ->
              { Monaco.Line = f.Line
                Monaco.Column = f.Column
                Monaco.EndColumn = f.EndColumn
                // **綴りを字に出す。** 「top で始まる」は Core が決めていて、
                // 語彙から来る（`vocabulary` の `TopPrefix`）
                Monaco.Message =
                  "走らせる入口が無い（"
                  + vocabulary.TopPrefix
                  + " で始まる名前の定義が 1 つ も無いので、読めても何も起きない）"
                Monaco.Severity = Monaco.Severity.Error }
            | Semantics.UnusedDefinition ->
              { Monaco.Line = f.Line
                Monaco.Column = f.Column
                Monaco.EndColumn = f.EndColumn
                Monaco.Message = f.Element + " " + f.Name + " はどこからも呼ばれていない"
                // **走りには影響しない。** 同梱 176 本 にも 6 件 在る ——
                // 警告にすると、正しい弾幕を開いた人が毎回 赤を見る
                Monaco.Severity = Monaco.Severity.Info }
            | Semantics.MissingRef ->
              { Monaco.Line = f.Line
                Monaco.Column = f.Column
                Monaco.EndColumn = f.EndColumn
                Monaco.Message = f.Element + " が指す " + f.Name + " が無い"
                // **走るが、書いたものが出ない。** Apply は通る（実測）——
                // 解けない参照は黙って無視されるだけなので、
                // 「読めない」（Error）とは別の強さ
                Monaco.Severity = Monaco.Severity.Warning }
            // 同じ名前の定義が 2 つ（v4.6）。**走るのは先に書いたほう** ——
            // 後のほうは読めて・組めて、書いたものが出ない
            | Semantics.DuplicateDefinition ->
              { Monaco.Line = f.Line
                Monaco.Column = f.Column
                Monaco.EndColumn = f.EndColumn
                Monaco.Message =
                  f.Element + " " + f.Name + " は同じ名前が上にも在る —— 走るのは上のほうで、こちらは使われない"
                Monaco.Severity = Monaco.Severity.Warning }
            // 読めない式（v4.1）。**強さが 2 段。**
            //
            // `$` を含まない式は読み込みの段で畳まれるので**走らない**（赤）。
            // 含む式はそこを通らず、走行中に NaN になる ——
            // **走るが値が出ない**（黄）。版の頭で測った：
            // `<wait>1+*$rank</wait>` は 60 コマ で 61 発 撃つ（間 が空かない）
            | Semantics.BadExpr stops ->
              { Monaco.Line = f.Line
                Monaco.Column = f.Column
                Monaco.EndColumn = f.EndColumn
                Monaco.Message =
                  if stops then f.Element + " の式が読めない —— この本は走らない"
                  else f.Element + " の式が読めない —— 走るが、この値は数にならない"
                Monaco.Severity =
                  if stops then Monaco.Severity.Error else Monaco.Severity.Warning })
      Monaco.markSemantic marks

  /// 表記を差し替える。**本文は触らない。**
  ///
  /// XML を書いたまま sxml にすると Apply が落ちる —— **それが正しい。**
  /// 黙って変換すると人が書いた字が消えるし、そもそも**書く口が repo に無い**
  member _.useLanguage(lang: ISourceLanguage) =
    current <- lang
    Monaco.setLanguage lang.EditorLanguageId
    let sel = el "mode"
    if not (isNull sel) then (sel :?> HTMLSelectElement).value <- lang.Kind.Id
    // 比べる先の並びは**いまの表記で変わる**（自分とは比べられない）。
    // 並べている最中なら、その中身も表記が変わった側に合わせて建て直す
    self.fillCompare ()
    if Monaco.diffShown () then self.setCompare ()

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

  /// 比べる先のプルダウン。**html に表を書かない**（表記・配色と同じ扱い）。
  ///
  /// 並びは 2 種類。
  ///
  ///     走っている弾幕と    同じ表記どうし。**差分の色が意味を持つ**
  ///     ほかの表記          いまの本文をそこへ変換したもの。色は全行 に付く
  ///
  /// 後ろは「差分を読む」道具ではなく、**同じ弾幕が別の書き方でどうなるかを
  /// 並べて読む**ための出し方。いまの表記は入れない（自分とは比べられない）
  member _.fillCompare() =
    let sel = el "compare"
    if isNull sel then ()
    else
      let keep = (sel :?> HTMLSelectElement).value
      sel.innerHTML <- ""
      let add (v: string) (t: string) =
        let o = document.createElement "option" :?> HTMLOptionElement
        o.value <- v
        o.textContent <- t
        sel.appendChild o |> ignore
      add "" "しない"
      add "applied" "走っている弾幕と"
      for lang in languages do
        if lang.Kind.Id <> current.Kind.Id then add lang.Kind.Id (lang.Kind.Id + " と")
      // 選んでいたものが並びから消えていたら「しない」へ倒れる。
      // **黙って別の先を選ばない** —— 表記を切り替えると自分自身が消える
      (sel :?> HTMLSelectElement).value <- keep
      if (sel :?> HTMLSelectElement).value <> keep then
        (sel :?> HTMLSelectElement).value <- ""

  /// 並べて出す / やめる。**欄と入れ替える** ——
  /// 横に並べると、弾幕は縦に長いのでどちらも読めない幅になる
  member _.setCompare() =
    let sel = el "compare"
    let src = el "source"
    let dst = el "diff"
    if isNull sel || isNull src || isNull dst then ()
    else
      let v = (sel :?> HTMLSelectElement).value
      let stop () =
        Monaco.hideDiff ()
        dst.setAttribute ("hidden", "")
        src.removeAttribute "hidden"
        // **欄は隠れているあいだ大きさを測れていない。** 戻したら測り直す
        Monaco.relayout ()
      let start (leftLang: string) (left: string) =
        src.setAttribute ("hidden", "")
        dst.removeAttribute "hidden"
        Monaco.showDiff "diff" leftLang left current.EditorLanguageId (Monaco.getValue ())
      if v = "" then stop ()
      elif isNull dotNet then setError "まだ起動していない"
      elif v = "applied" then
        // **「編集前」は host に聞く。** Fable 側に写しを持つと、
        // 弾幕が差し替わる道のどれかで更新し忘れて、古い本文と比べてしまう
        thenCatch
          (invokeAsync1 dotNet "AppliedSource" current.Kind.Id)
          (fun res ->
            let text = string res
            if text.StartsWith "ERROR:" then
              stop ()
              setError (text.Substring 6)
            else start current.EditorLanguageId text)
          (fun err -> setError (errText err))
      else
        match languages |> List.tryFind (fun l -> l.Kind.Id = v) with
        | None ->
          stop ()
          setError ("知らない表記: " + v)
        | Some target ->
          thenCatch
            (invokeAsync3 dotNet "Transcode" current.Kind.Id target.Kind.Id (Monaco.getValue ()))
            (fun res ->
              let r = jsonParse (string res)
              if unbox<bool> r?ok then start target.EditorLanguageId (string r?text)
              else
                stop ()
                setError (string r?message))
            (fun err -> setError (errText err))

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
              if unbox<bool> r?ok then
                Monaco.setValue (string r?text)
                // **走っている木は変わっていない。** 字を書き直しただけなので、
                // 並びも同じ —— 窓は開けたまま（`setValue` が閉じるので開け直す）
                self.openLight ()
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
      let len: int = names?length
      // **境目は host が返す**（v2.4.1）。同梱と公式配布のサンプルは
      // 別の集合なので、1 本 の並びに繋いだうえで見出しで割る。
      // **本数を此処に書かない** —— 書くと片方 だけ古びる
      let officialFrom = int (string (invoke0 dotNet "OfficialFrom"))
      /// 見出し 1 つ ぶんを足す。番号は並びの位置そのもの
      let group (target: HTMLElement) (label: string) (from: int) (until: int) =
        // **1 件 も無ければ見出しも出さない**（空の見出しは押せる口に見える）
        if until > from then
          let g = document.createElement "optgroup"
          g?label <- label
          let mutable i = from
          while i < until do
            let o = document.createElement "option" :?> HTMLOptionElement
            o.value <- string i
            o.textContent <- string (jsItem names i)
            g.appendChild o |> ignore
            i <- i + 1
          target.appendChild g |> ignore
      /// 同梱 -> 公式 の順に流す。**両方 の面で同じ 1 本 を使う**
      let fill (target: HTMLElement) =
        let bundled = if officialFrom < 0 || officialFrom > len then len else officialFrom
        group target "同梱" 0 bundled
        group target "BulletML 公式配布のサンプル" bundled len
      sel.innerHTML <- ""
      let blank = document.createElement "option" :?> HTMLOptionElement
      blank.value <- ""
      blank.textContent <- "（編集 / Open）"
      sel.appendChild blank |> ignore
      fill sel
      // 2 つ 目 の面の並びも同じ 1 本 から。**html にも別の口にも書かない**
      let sel2 = el "pattern2"
      if not (isNull sel2) then
        sel2.innerHTML <- ""
        let none = document.createElement "option" :?> HTMLOptionElement
        none.value <- ""
        none.textContent <- "なし"
        sel2.appendChild none |> ignore
        fill sel2

  /// 2 つ 目 の面に載せる弾幕を選ぶ（v2.1）。**「なし」で 1 面 に戻る。**
  ///
  /// 選ぶと**両方 の面が頭から**建て直る —— 並べた 2 つ が別のコマを
  /// 指していたら、それは「同じ時刻の 2 つ」ではないので比べられない
  member _.pickSecond() =
    let sel = el "pattern2"
    if isNull sel then ()
    elif isNull dotNet then setError "まだ起動していない"
    else
      // 面が建て直る。**前の尾を残さない**（`apply` と同じ理由）
      wipe <- true
      let v = (sel :?> HTMLSelectElement).value
      if v = "" then
        thenCatch
          (invokeAsync0 dotNet "ClearSecond")
          (fun _ -> ())
          (fun err -> setError (errText err))
      else
        thenCatch
          (invokeAsync1 dotNet "SetSecond" (int (float v)))
          (fun res ->
            let why = string res
            if why.StartsWith "ERROR:" then setError (why.Substring 6) else setError "")
          (fun err -> setError (errText err))

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
            setError ""
            // 選び直した弾幕がそのまま載っている。**窓を開ける**
            self.openLight ())
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

  /// 2 つ の面が分かれるコマまで送る（v3.8）。
  ///
  /// **見る範囲は「コマ」欄 の数をそのまま使う** —— 押した時点で頭から
  /// 建て直すので、「そのコマまで見る」と「そのコマへ飛ぶ」が同じ数になる。
  /// **ここで別の上限を書かない**（2 か所 に在ると片方 だけ古びる）。
  ///
  /// **2 面 が無ければ何も送らない。** 弾幕を選んでいなくても、
  /// 「ずらす」を選んでいれば host が面 1 と同じ弾幕で建てている
  member _.divergeRun() =
    let input = el "seek-to"
    let p2 = el "pattern2"
    let sh = el "shift"
    let has =
      (not (isNull p2) && (p2 :?> HTMLSelectElement).value <> "")
      || (not (isNull sh) && (sh :?> HTMLSelectElement).value <> "0")
    if isNull input then ()
    elif isNull dotNet then setError "まだ起動していない"
    elif not has then setError "2 面 が無い。弾幕を選ぶか、ずらす を選ぶ"
    else
      let mutable n = 0.0
      if not (System.Double.TryParse((input :?> HTMLInputElement).value, &n)) then
        setError "コマ数を入れて"
      else
        // 頭から建て直る。**軌跡は 1 コマ 塗り潰す**（`seek` と同じ）
        wipe <- true
        setError ""
        divergeTo <- int n
        divergeWatch <- true
        self.showNote ("分かれるまで送っている（" + groupDigits divergeTo + " コマ まで）")
        thenCatch
          (invokeAsync1 dotNet "DivergeRun" n)
          (fun _ -> ())
          (fun err ->
            divergeWatch <- false
            setError (errText err))

  /// 送っている最中に 1 コマ 1 回 引く（v3.8）。
  /// **-2 は走っている / -1 は分かれなかった / 0 以上 は分かれたコマ**
  member _.pollDiverge() =
    if isNull dotNet then divergeWatch <- false
    else
      let v = int (unbox<float> (invoke0 dotNet "DivergeResult"))
      if v <> -2 then
        divergeWatch <- false
        if v < 0 then
          self.showNote ("分かれなかった（" + groupDigits divergeTo + " コマ まで見た）")
        else
          self.showNote ("分かれた: " + groupDigits v + " コマ 目")

  /// 面 2 の走らせ方をずらす（v3.8）。**なし / 種 / 難度**
  member _.setShift() =
    let sel = el "shift"
    if isNull sel then ()
    else
      let mutable n = 0.0
      if System.Double.TryParse((sel :?> HTMLSelectElement).value, &n) then
        // 面が建て直る。**軌跡を 1 コマ 塗り潰してから**
        wipe <- true
        self.call ("SetShift", box (int n))

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

  /// 字の右の印の入り切り（v4.0.2）。**2 つ をまとめて。**
  ///
  /// **面にも伝える。** ブラウザ側で出すのをやめるだけだと、
  /// 面は弾コマ を数え続ける（毎コマ、弾の数ぶん）—— 数えたものの
  /// 行き先が無いので、そこも止める。撃った数は止めない（`SetMarks` の但し書き）
  member _.setMarks() =
    let chk = el "marks"
    if isNull chk then ()
    else
      marksOn <- (chk :?> HTMLInputElement).``checked``
      // **起動前 は面がまだ無い。** そのときは黙って進み、`onReady` が配り直す
      if not (isNull dotNet) then self.call ("SetMarks", box marksOn)
      if marksOn then
        // 入れたら**次のコマで出す**（間引きの数を待たない）
        tallyAt <- -1000
        spanAt <- -1000
      else
        // 切ったら、いま出ている印をその場で落とす
        tallyShown <- false
        Monaco.clearTally ()
        spanShown <- false
        Monaco.clearSpans ()

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
  /// Monaco の整形（v4.0）。**`Shift`+`Alt`+`F` と 右クリックの Format Document。**
  ///
  /// ボタン（`format`）との違いは**段の数**。あちらは字が変わるだけで
  /// 1 回 止まるが、こちらは**コメントが消えるときだけ**止まる ——
  /// コーパス 173 本 にコメントは 0 件（版の頭で数えた）なので、
  /// ほとんどの本文は 1 回 で整形される。
  ///
  /// **消える印の数で判じる。表記ごとのコメントの綴りを表に書かない** ——
  /// 書くと、表記が増えたときにここだけが古びる。整形の前後 で
  /// `<!--` と `//` を数えて、**減っていたら 1 文字 も動かさない。**
  ///
  /// **`//` は URL にも出る**（`xmlns` の値）が、整形では消えないので数は変わらない
  /// —— 見ているのは「在るか」ではなく「**減ったか**」。
  ///
  /// **`None` を返す道が 4 つ 在る。** 起動前 / 読めない / 字が変わらない /
  /// コメントが消える。**どれも編集を 0 個 返す** ——
  /// 例外を投げると Monaco が provider を黙って落とす。
  member _.formatEdits (src: string) (reply: string option -> unit) =
    if isNull dotNet then reply None
    else
      let id = current.Kind.Id
      thenCatch
        (invokeAsync3 dotNet "Transcode" id id src)
        (fun res ->
          let r = jsonParse (string res)
          // **読めない本文でも押される。** 波線が出ているだけで押せるので、
          // ここは静かに何もしない（下のバーには Apply が出す）
          if not (unbox<bool> r?ok) then reply None
          else
            let formatted = string r?text
            if formatted = src then reply None
            else
              let lost =
                (countMark src "<!--" - countMark formatted "<!--")
                + (countMark src "//" - countMark formatted "//")
              if lost > 0 then
                self.showNote
                  ("整形すると コメントが " + string lost + " 個 消える。入れ替えるなら 整形 のボタンから")
                reply None
              else reply (Some formatted))
        (fun _ -> reply None)

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
              // 読んで書き直しただけ。**走っている木も並びも変わらない**
              self.openLight ()
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

  /// 録る長さ（コマ）。**読めなければ 300**（5 秒）——
  /// 同梱 176 本 中 165 本 は、そこまでに使う撃ち方が全部 1 度 は出る
  member _.recordFrames() : int =
    let e = el "rec-len"
    if isNull e then 300
    else
      let mutable v = 0.0
      if System.Double.TryParse((e :?> HTMLSelectElement).value, &v) then max 60 (int v) else 300

  /// 面を動く絵にして落とす（v2.5）。**Share の対** ——
  /// あちらは本文を渡し、こちらは絵を渡す。
  ///
  /// **録るあいだは走らせる。** 止まっている面は塗られず、
  /// `captureStream` が中身の無い絵を返す。だから押した時点で `Play` を送る。
  ///
  /// **録るのは 1 面 目 だけ。** 2 面 目 は別の canvas なので、
  /// 両方 録ると 2 つ 落ちてくることになる。
  ///
  /// **長さは実時間。** 重い本は録画の中でもコマが落ちる（5 秒 で 300 コマ の
  /// ところが 126 コマ の本が在った）—— それが見えている速さなので直さない
  member _.record() =
    if recording then self.showNote "録っている最中"
    else
      let mime = recordMime ()
      let stage = el "stage"
      if mime = "" then setError "この browser では絵を焼けない"
      elif isNull stage then setError "面が見つからない"
      else
        let ms = float (self.recordFrames ()) / 60.0 * 1000.0
        // **頭は要素名にしない**（`save` と同じ線。ブラウザ側に綴りを持たない）
        let outName = "pattern" + (if mime.StartsWith "video/webm" then ".webm" else ".mp4")
        let btn = el "record"
        let setBtn (label: string) (off: bool) =
          if not (isNull btn) then
            btn.textContent <- label
            (btn :?> HTMLButtonElement).disabled <- off
        recording <- true
        setBtn "録画中" true
        self.showNote ("録っている（" + string (self.recordFrames ()) + " コマ）")
        self.call "Play"
        let finish =
          fun (size: float) ->
            recording <- false
            setBtn "録る" false
            self.showNote ("落とした: " + outName + " " + groupDigits (int (size / 1024.0)) + " KB")
        recordCanvas stage mime ms outName finish

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

  /// 章を 1 つ だけ出す。**出す側と隠す側を同じ回で書く** ——
  /// 「押した札を出す」だけにすると、前の章が残って 2 つ 並ぶ。
  ///
  /// 本文の送りは頭へ戻す。**戻さないと、短い章に切り替えたとき
  /// 中身の無いところが出る**（送りは入れ物のもので、章のものではない）
  member _.showChapter(name: string) =
    for c in helpChapters do
      let sect = el ("help-ch-" + c)
      if not (isNull sect) then
        if c = name then sect.removeAttribute "hidden"
        else sect.setAttribute ("hidden", "")

      let tab = el ("help-tab-" + c)
      if not (isNull tab) then
        if c = name then tab.setAttribute ("aria-current", "true")
        else tab.removeAttribute "aria-current"

    let body = el "help-body"
    if not (isNull body) then body.scrollTop <- 0.0

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

  /// 打った字と設定を残す（v3.5）。**口の一覧を 1 か所 に持って、
  /// 変わったら丸ごと書く** —— 口ごとにハンドラを足すと、
  /// 口が増えたときに足し忘れたものだけが静かに残らなくなる。
  ///
  /// **書けなくても黙って進む。** プライベートウィンドウでは
  /// `localStorage` に触った時点で投げる（`lsSet` が握る）。
  member _.saveSettings() =
    let o = createObj []
    for id in keptControls do
      let e = el id
      if not (isNull e) then
        let v =
          if isCheck id then box (unbox<HTMLInputElement>(box e)).``checked``
          else box (unbox<HTMLInputElement>(box e)).value
        o?(id) <- v
    lsSet settingsKey (JS.JSON.stringify o) |> ignore
    lsSet modeKey current.Kind.Id |> ignore

  /// 打った字を残す。**打鍵ごとには書かない** ——
  /// 最大 28.5 KB で 1 回 0.074 ms（測った）なので払えはするが、
  /// **打ち終わってから 1 回**で足りる
  member _.saveSourceSoon() =
    if saveTimer > 0.0 then window.clearTimeout saveTimer
    saveTimer <-
      window.setTimeout((fun _ ->
        saveTimer <- 0.0
        lsSet sourceKey (Monaco.getValue ()) |> ignore), 500)

  /// 残してあった字と設定を戻す（v3.5）。**共有リンクより先に呼ぶ** ——
  /// URL に本文が乗っているときは、そちらが明示された指定なので勝つ。
  ///
  /// **読めなければ捨てる。** 前の版が書いた形かもしれないし、
  /// 手で書き換えられているかもしれない —— そこで落ちると開かなくなる
  member _.restoreLocal() =
    // **URL に共有リンクが在るときは何もしない。** あちらが明示された指定で、
    // ここで建てても直後に上書きされる —— 面を 2 回 建てることになり、
    // リンクが読める（非同期）までのあいだ**別の弾幕が走って見える**
    let hash = window.location.hash
    if not (isNull hash) && hash <> "" && hash <> "#" then ()
    else
    try
      match lsGet settingsKey with
      | null -> ()
      | json ->
          let o = JS.JSON.parse json
          for id in keptControls do
            let e = el id
            let v = o?(id)
            if not (isNull e) && not (isNull v) then
              if isCheck id then (unbox<HTMLInputElement>(box e)).``checked`` <- unbox<bool> v
              else (unbox<HTMLInputElement>(box e)).value <- unbox<string> v
              // **既存の道を通す。** 値を入れるだけでは配色も速さも効かない
              e.dispatchEvent (Event.Create "change") |> ignore
      match lsGet sourceKey with
      | null -> ()
      | "" -> ()
      | text ->
          // **表記を本文と一緒に戻す。** `mode` の `change` は本文を
          // 書き換える（表記の変換）ので、そこは通さず `useLanguage` を直に
          Monaco.setValue text
          match lsGet modeKey with
          | null -> ()
          | kindId ->
              match languages |> List.tryFind (fun l -> l.Kind.Id = kindId) with
              | Some lang -> self.useLanguage lang
              | None -> ()
          self.showNote "前の続きから"
          self.apply ()
    with _ ->
      // 壊れていたら捨てる。**次の保存で上書きされる**
      ()

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
            // **起動時の字は host が焼いた同じ木。** 窓を開けておく
            self.openLight ()
            self.fillModes ()
            // **エディタが建ってから出す。** Monaco が読めなかったときに
            // 配色のプルダウンだけ在るのは、押せるのに何も起きない口になる
            self.fillThemes ()
            // **比べる先も同じところで。** 並びは表記に依るので、
            // `useLanguage` でも作り直す
            self.fillCompare ()
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
              // 同じ名前を薄く光らせる（v4.2）。**上と同じ 1 本 の上** ——
              // 光る先と飛ぶ先が食い違わない
              Monaco.registerHighlightProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Usages src offset)
              // 定義の行の上に参照の数（v4.3）。**押すと上の一覧が開く**
              Monaco.registerLensProvider
                lang.EditorLanguageId
                (fun src -> lang.Lenses src)
              // 式の横に値（v4.4）。**畳むのは面**（`EvalExprs`）
              Monaco.registerHintProvider
                lang.EditorLanguageId
                (fun src a b -> self.hintsIn src a b)
              // 参照が渡す引数の形（v4.5）
              Monaco.registerSignatureProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Signature src offset)
              Monaco.registerCodeActionProvider
                lang.EditorLanguageId
                (fun src offset -> lang.Fixes src offset)
              // アウトライン（Ctrl+Shift+O / Alt+O）と折りたたみ（v2.4）。
              // **どちらも同じ 1 本 の上**（`Outline.build`）
              Monaco.registerStructureProviders
                lang.EditorLanguageId
                (fun src -> lang.Outline src)
              // 整形（v4.0）。**本文まるごとだけ** ——
              // 範囲整形と打鍵整形は繋いでいない（選択範囲から木は作れない）
              Monaco.registerFormattingProvider
                lang.EditorLanguageId
                (fun src reply -> self.formatEdits src reply)
            // **割り当てはエディタに付く**（言語 id ではない）ので、
            // 上の for の外。中に置くと同じ規則が 4 本 積み上がる
            Monaco.addOutlineAltKey ()
            // **印は文字に追随しない。** 1 文字 打った時点で場所が嘘になるので、
            // そこで消す。付けていないときは何もしない（毎打鍵の空振りを避ける）
            Monaco.onContentChanged (fun () ->
              // **走っている場所の印もここで下ろす**（v3.1 の段 4）——
              // 印は走行から出るので、字が 1 文字 動いた時点で嘘になる
              self.closeLight ()
              if markedAt then
                markedAt <- false
                Monaco.clearMarks ()
              if noted then
                noted <- false
                setNote ""
              // **意味の層は逆に、打鍵ごとに引き直す**（v2.3）——
              // 字から出るので往復が要らない。上の 2 つ は「往復して出た
              // ものが古びる」ので消す側で、こちらは持ち主が別
              self.refreshFindings ()
              // 打った字を残す（v3.5）。**打ち終わってから 1 回**
              self.saveSourceSoon ())
            // 起動時に 1 回。**打鍵を待たない** —— リンクから開いた本文や
            // 同梱の弾幕にも、その場で波線が要る
            self.refreshFindings ()
            self.showInitialInPatterns ()
            // 字から弾（v4.7）。**カーソルが動いたときだけ数え直す** ——
            // 毎コマ 引くと、本文を走査するのがコマごとになる
            Monaco.onCursorMove (fun () -> self.lightFrom ())
            // 前に打った字と設定を戻す（v3.5）。**共有リンクより先** ——
            // URL に本文が乗っているときは、そちらが明示された指定なので勝つ
            self.restoreLocal ()
            // 設定が変わったら残す。**口ごとにハンドラを足さない** ——
            // 口が増えたときに足し忘れたものだけが静かに残らなくなる
            document.addEventListener ("change", fun _ -> self.saveSettings ())
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
      // 木のノードになる要素名（v3.1 の段 4）。**落とす側の表を持たない** ——
      // DTD に要素が増えたとき、こちらだけ古びて添字が黙ってずれる
      nodeNames <- unbox<string[]> (invoke0 dotNet "NodeElementNames") |> List.ofArray
      if nodeNames.IsEmpty then setError "ノードの要素名が空（Core の腕を読めていない）"

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
    // 印の入り切りを面へ配り直す（v4.0.2）。**戻すのが起動より先 のことが在る**
    // —— `restoreLocal` は `dotNet` がまだ無いときは黙って進む
    if not marksOn then self.call ("SetMarks", box false)
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
            // 走行にかかった時間を測る（v3.3 の段 2）。**rAF の間隔ではない** ——
            // あちらは描画も他の処理も含むので、「どこが重いか」には答えない
            let stepT0 = now ()
            let ret = invokeStep dotNet "StepFrame" t playerX playerY
            let stepMs = now () - stepT0
            let n = int (unbox<float> (jsItem ret 0))
            let packed =
              if n > 0 then
                let off = int (unbox<float> (jsItem ret 1) / 4.)
                // 押されたときに引き直せるように、窓ではなく位置を控える
                packedOff <- off
                packedN <- n
                subarray (heapF32 ()) off (off + n * 2)
              else
                packedN <- 0
                null
            // 追っている弾（v3.1 の段 3）。**描く前に写す** —— 印を付ける先は
            // このコマの並びなので、`draw` より後ろで写すと 1 コマ 遅れる
            pickIdx <- int (unbox<float> (jsItem ret 11))
            // **自機と面の大きさは host が決める。** 送った座標をそのまま
            // 描くと、止めているときと回っているときに絵と狙いが食い違う
            self.setView (
              unbox<float> (jsItem ret 3),
              unbox<float> (jsItem ret 4),
              unbox<float> (jsItem ret 5),
              unbox<float> (jsItem ret 6),
              unbox<float> (jsItem ret 9),
              unbox<float> (jsItem ret 10)
            )
            self.draw (packed, n) |> ignore
            // 2 つ 目 の面。**`n2` が負なら無い**（0 は「弾が 1 つ も無い面」）
            let n2 = int (unbox<float> (jsItem ret 7))
            let packed2 =
              if n2 > 0 then
                let off2 = int (unbox<float> (jsItem ret 8) / 4.)
                subarray (heapF32 ()) off2 (off2 + n2 * 2)
              else null
            self.draw2 (packed2, n2) |> ignore
            // **軌跡は 2 面 とも塗ってから落とす。** 先に落とすと、
            // 1 面 目 で消えて 2 面 目 に効かない
            wipe <- false
            // **人に見せるのは弾の数**（v4.0.1）。`n` は `Pack` が詰めた点の数で、
            // **根の敵が入っている** —— 描くのと添字はそちら、数はこちら
            let bn = int (unbox<float> (jsItem ret 18))
            let bn2 = int (unbox<float> (jsItem ret 19))
            self.hud (bn, bn2, t, int (unbox<float> (jsItem ret 2)))
            // 分かれるまで送っている最中だけ引きに来る（v3.8）
            if divergeWatch then self.pollDiverge ()
            // **光らせるのが先。** 帯にはその行番号を出すので、
            // あとに回すと 1 コマ 遅れた行が出る
            let litLine = self.lightRunning (int (unbox<float> (jsItem ret 16)))
            let fromLine =
              self.lightOrigin (int (unbox<float> (jsItem ret 17)), pickIdx)
            // **切ってあるなら、口ごと通らない**（v4.0.2）——
            // 中で判じると、切っていても毎コマ 2 本 の呼び出しが残る
            if marksOn then
              // 撃った数（v3.3 の段 1）。**弾を選んでいなくても出る** ——
              // 「どこが弾を増やしているか」は、追う前に知りたいこと
              self.lightTally (int (unbox<float> (jsItem ret 2)))
              // **撃った数のあと。** あちらが間引きの数を進めるので、
              // こちらは「進んだコマ」を見て同じコマで書き直す
              self.lightSpans (int (unbox<float> (jsItem ret 2)))
              // **2 つ の印を書いたあとで進める。** 手前 で進めると
              // 「止まった」が 1 度 も見えない
              markFrame <- int (unbox<float> (jsItem ret 2))
            // 重さの帯（v3.3 の段 2）。**弾数は 1 面 目 だけ** ——
            // 2 面 を足すと「どちらが重いか」が混ざる
            self.weight (bn, stepMs, int (unbox<float> (jsItem ret 2)))
            self.focusHud (
              pickIdx,
              int (unbox<float> (jsItem ret 12)),
              int (unbox<float> (jsItem ret 13)),
              int (unbox<float> (jsItem ret 14)),
              int (unbox<float> (jsItem ret 15)),
              litLine,
              fromLine
            )
          with ex ->
            console.error ex
            setError (string ex)
      window.requestAnimationFrame loop |> ignore

    // **エディタは rAF を予約したあと。** ローダは CDN 越しなので、
    // 先に呼ぶと最初の 1 コマ がその往復ぶん遅れる。
    // 読めなくても Canvas は 2way のまま動かす —— 理由だけ出す。
    //
    // **貼られているときは建てない。** 読み取り専用の埋め込みでは欄を出さないので、
    // Monaco の本体（CDN 越しの editor.main）を 1 バイト も取りに行かない。
    // 弾幕のプルダウンは `fillPatterns` が別に埋めるので、選び直しは効く ——
    // `pick` が呼ぶ `Monaco.setValue` は、エディタが無ければ何もしない
    // （Monaco.fs の口はどれも `isNull editor` で守られている）
    if not embedded then self.startEditor ()

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

/// 押した先が要るとき。**`on` は event を渡さない** ——
/// 背景のクリックは「押した先が dialog 自身」でしか見分けられない
let private onEvent (id: string) (event: string) (handler: Event -> unit) =
  let node = el id
  if isNull node then setError ("配線する先が無い: " + id)
  else node.addEventListener (event, handler)

on "pattern" "change" (fun () -> playground.pick ())
on "pattern2" "change" (fun () -> playground.pickSecond ())
on "mode" "change" (fun () -> playground.setMode ())
on "theme" "change" (fun () -> playground.setTheme ())
on "compare" "change" (fun () -> playground.setCompare ())
on "help" "click" (fun () -> playground.help ())
on "help-close" "click" (fun () -> playground.closeHelp ())

// **背景でも閉じる。** `dialog` が素で持っているのは Esc だけで、
// 背景のクリックは持っていない —— 但し書きには「素で付いてくる」と
// 書いてあったが、当たり判定を測ったら dialog 自身 に付いていた。
// 章に割って窓が小さくなったぶん、背景を押す人が増える。
//
// **押し始めと離しの両方 が外**のときだけ閉じる。`click` の座標は
// 離した位置なので、それだけを見ると**中で字を選んで外で指を離した**ときに
// 閉じる（実際に測って赤が出た）。逆に外で押して中で離す形も閉じない
let mutable private helpDownOnBackdrop = false

onEvent "help-dialog" "mousedown" (fun e ->
  let d = el "help-dialog"
  helpDownOnBackdrop <- not (isNull d) && clickedBackdrop d e)

onEvent "help-dialog" "click" (fun e ->
  let d = el "help-dialog"
  let outside = not (isNull d) && clickedBackdrop d e
  if helpDownOnBackdrop && outside then playground.closeHelp ()
  helpDownOnBackdrop <- false)

// 章の札。**`on` は配線する先が無ければ `setError` に出す**ので、
// html の側 を消したら気づく（黙って押せない札にはならない）
for c in helpChapters do
  on ("help-tab-" + c) "click" (fun () -> playground.showChapter c)
on "play" "click" (fun () -> playground.call("Play"))
on "pause" "click" (fun () -> playground.call("Pause"))
on "step-once" "click" (fun () -> playground.call("StepOnce"))
on "rate" "change" (fun () -> playground.setRate ())
on "reset" "click" (fun () ->
  // 面が建て直る。**軌跡を 1 コマ 塗り潰してから**（`apply` と同じ理由）
  playground.wipeTrail ()
  playground.call ("Reset"))
on "seek" "click" (fun () -> playground.seek ())
on "diverge" "click" (fun () -> playground.divergeRun ())
on "shift" "change" (fun () -> playground.setShift ())
on "rank" "input" (fun () -> playground.setRank ())
on "seed" "change" (fun () -> playground.setSeed ())
on "seed-roll" "click" (fun () -> playground.rollSeed ())
on "player" "change" (fun () -> playground.setPlayerMotion ())
on "trail" "change" (fun () -> playground.setTrail ())
on "marks" "change" (fun () -> playground.setMarks ())
on "apply" "click" (fun () -> playground.apply ())
on "open" "click" (fun () -> playground.``open``())
on "save" "click" (fun () -> playground.save ())
on "format" "click" (fun () -> playground.format ())
on "share" "click" (fun () -> playground.share ())
on "record" "click" (fun () -> playground.record ())

// **窓の大きさが変わったら、欄を測り直す**（v2.4.4）。
//
// `automaticLayout` は入れ物の変化を自分で見ることになっているが、
// 入れ物の幅を 728 -> 300 に変えて測ると `getLayoutInfo().width` が
// **726 のまま動かなかった**（親でも iframe の中でも同じ。`layout()` を
// 呼ぶと 326 に直る）。欄の中身が入れ物より広いままなので、**そのぶん
// 画面が横に溢れる** —— 狭い画面でいちばん目に付く崩れがこれ。
//
// **`automaticLayout` を外して替えるのではなく、足す。** 背面タブで
// 発火しないのはどちらも同じで、そこは変えない（Monaco.fs の但し書き）。
//
// **1 回 に畳む。** 掴んで窓を引き伸ばすと resize は数十回 飛んでくる。
//
// **畳むのに `requestAnimationFrame` を使わない。** 背面タブでは rAF が
// 止まるので、そこで測ると「resize は 2 回 届いたのに rAF は 0 回」になる
// （実際にそう出た）。窓の掴み替えは前面でしか起きないので実害は無いが、
// **測れない実装を増やさない** —— `setTimeout` なら背面でも走る
let mutable private relayoutQueued = false

window.addEventListener (
  "resize",
  fun _ ->
    if not relayoutQueued then
      relayoutQueued <- true
      window.setTimeout ((fun _ ->
        relayoutQueued <- false
        Monaco.relayout ()), 50)
      |> ignore)

// **いちばん最後。** 上の配線が済んでから WASM を起こす ——
// `onReady` はここから返ってくるので、先に起こすと受け口が無い。
// 失敗は `#loop-error` に出す。黙って白い画面にしない
thenCatch (blazorStart ()) ignore (fun err -> setError (errText err))
