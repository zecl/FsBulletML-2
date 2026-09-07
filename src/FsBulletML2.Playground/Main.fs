namespace FsBulletML2.Playground

open System.Text.Json
open System.Threading.Tasks
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open FsBulletML2
open FsBulletML2.Bullets.Dsl
// `SourceKind`。**ブラウザ側（Fable）が同じ 1 本 を引く** ——
// 前は こちらが `match kind with | "xml"` で受け、あちらが
// `SourceKind.Xml.Id` で作っていて、片方 だけ変えても落ちなかった
open FsBulletML2.LanguageService

/// 起動時に載せる弾幕。**同梱カタログの CE が正本。**
///
/// 欄に出す XML は `BulletmlWriter.toIndentedXml` で焼く —— html に直書きすると、
/// 弾幕を差し替えたとき字だけが古びる（走るのは CE、見えるのは古い XML）。
///
/// **一覧の番号で指さない。** 並びが動くと黙って別の弾幕になる。
/// 番号のほうを値から引く（`InitialIndex`）。
///
/// 起動時に XML を読まないのは前と同じ理由 —— WASM で `XmlReader` に
/// 入る経路をひとつ減らす。CE なら木がもう建っている。
module private Initial =
  let pattern = EnemyBullet.Sdmkun.EspRade.round_123_boss_izuna_hakkyou

/// JSInvokable の実体。アセンブリ名経由だと WASM 起動直後に見つからない。
type PlaygroundHost() =

  let env = BrowserEnv()
  let mutable current = Initial.pattern.Bulletml
  let mutable field = Playfield.Create env current
  // **開いた時点で走っている。** Play を押すまで止まっていると、
  // 弾幕を見に来た人が最初に見るのが静止画になる。止めたい人は Pause を
  // 押せばよく、そちらは 1 手 で戻せる。
  //
  // rAF は `onReady` で回り始めるので、ここが true でも起こす順は変わらない
  let mutable playing = true
  // 進め方は `Pacing` が持つ。**ここで判断しない** ——
  // 判断をここへ書くと、Bolero を参照するこのファイルの中にしか無くなって
  // .NET で当てられなくなる
  let mutable pacing = Pacing.normal
  // **1 個 だけ作って持ち回る。** 毎コマ `field.Tick` を関数値にすると、
  // 1 フレーム につき 1 個 の閉包がヒープに乗る。
  // `field` は Apply で差し替わるが、その都度 読み直すのでこれで足りる
  let tick = fun () -> field.Tick()
  let ret = Array.zeroCreate<float> 2
  let catalog = lazy (All.bullets |> List.toArray)

  /// `[n; ptr]`。Apply で配列が差し替わるので ptr は毎コマ返す。
  [<JSInvokable>]
  member _.StepFrame(_now: float, playerX: float, playerY: float) : float[] =
    env.SetPlayer (float32 playerX) (float32 playerY)
    if playing then pacing <- Pacing.step tick pacing
    ret.[0] <- float (field.Pack())
    ret.[1] <- field.PackedPtr
    ret

  [<JSInvokable>]
  member _.Play() = playing <- true

  [<JSInvokable>]
  member _.Pause() = playing <- false

  /// 1 コマ だけ進める。**押した時点で止まる** ——
  /// 走っているまま 1 コマ 足しても、次のコマで流れてしまって見えない
  [<JSInvokable>]
  member _.StepOnce() =
    playing <- false
    field.Tick()

  /// 正なら倍速（1 フレームに n 回）、負ならスロー（-n フレームに 1 回）。
  ///
  /// **Apply / Reset / プルダウンでは戻さない。** 速さは見る側の都合で、
  /// 載っている弾幕の一部ではない（拡大率と同じ扱い）
  [<JSInvokable>]
  member _.SetRate(n: int) = pacing <- Pacing.withRate n

  [<JSInvokable>]
  member _.Reset() =
    field <- Playfield.Create env current

  /// 起動時に欄へ出す XML。**html に直書きしない。**
  ///
  /// **定数を畳まない側で焼く（v1.4）。** `Parser` の `ToIndentedXmlString` は
  /// `foldConstants` を通すので `8` が `8.0000000000` になる ——
  /// 表記を切り替えると、その字が sxml にも fsb にも CE にも伝播する。
  /// **人が書いた式のまま見せる**ほうが正しい。
  [<JSInvokable>]
  member _.InitialSource() : string = BulletmlWriter.toIndentedXml 4 current

  /// 補完の語彙。**起動時に 1 回 だけ。** 正本は Core の DTD.fs で、
  /// ここは reflection で読んだものを JSON にして渡すだけ。
  /// 毎キー呼ばない —— 引くのは Fable 側でやる
  [<JSInvokable>]
  member _.Vocabulary() : string = Vocabulary.toJson ()

  /// 起動時の弾幕が一覧の何番目か。**無ければ -1。**
  ///
  /// 中身の `Bulletml` で引く —— `BulletmlInfo` は struct なので、
  /// 一覧に入るとき写される。参照が同じなのは中の木のほう。
  ///
  /// -1 は「走っている弾幕がプルダウンに出ていない」なので、
  /// 呼ぶ側は黙って空にせず理由を出す
  [<JSInvokable>]
  member _.InitialIndex() : int =
    catalog.Value
    |> Array.tryFindIndex (fun i -> System.Object.ReferenceEquals(i.Bulletml, Initial.pattern.Bulletml))
    |> Option.defaultValue -1

  /// 同梱 CE の名前。初回だけ木を組む。
  [<JSInvokable>]
  member _.ListPatterns() : string[] =
    catalog.Value
    |> Array.mapi (fun i info ->
         if System.String.IsNullOrEmpty info.Name then sprintf "#%d" i else info.Name)

  /// 一覧の番号で差し替える。成功なら XML。失敗は `ERROR:` で始まる。
  [<JSInvokable>]
  member _.SelectPattern(index: int) : string =
    try
      let items = catalog.Value
      if index < 0 || index >= items.Length then "ERROR:範囲外"
      else
        let info = items.[index]
        // 建ててから差し替える（`ApplySource` と同じ理由。落ちたあとの
        // `Reset` が `current` から建て直すので、進めてはいけない）
        let next = Playfield.Create env info.Bulletml
        current <- info.Bulletml
        field <- next
        // 畳まない側。理由は `InitialSource` と同じ
        BulletmlWriter.toIndentedXml 4 info.Bulletml
    with ex -> "ERROR:" + ex.Message

  /// 右側の本文を読んで弾幕を差し替える。
  ///
  /// **どの表記かを受け取る。** v0.9 で読めるのは xml と sxml。
  /// テキストだけ受け取る形にすると、呼ぶ側にも XML が焼き込まれて、
  /// 次の表記で両方 直すことになる。
  ///
  /// **表記ごとの腕はここに無い。** `SourceReader` が束ねている ——
  /// fsb を足すのはあちらに 1 行、`Diagnosis` に読む筋 1 本。
  ///
  /// **戻りは JSON。空文字を成功の印にしない** ——
  /// 「読めない理由が空文字」と見分けられない。
  ///
  ///     {"ok":true}
  ///     {"ok":false,"message":"…","marks":[{"line":14,"column":11,"endColumn":15,"message":"…"}]}
  ///
  /// `message` は帯に出す代表で、`marks` が波線。**`marks` が空なら
  /// 位置が無い層**（呼ぶ側は波線を引かない）。`endColumn` が 0 なら行末まで。
  /// 分け方は `Diagnosis`（`Parser.Tests` が同じ道を通って当てている）。
  [<JSInvokable>]
  member _.ApplySource(kind: string, text: string) : string =
    let jstr (s: string) = JsonSerializer.Serialize s
    /// 帯に出す代表。**何本 引いたかは帯にしか出ない** ——
    /// 波線は 1 本 ずつ別のところに在るので、まとめて数えられない
    let banner (fs: Failure list) =
      match fs with
      | [] -> ""
      | [ f ] -> f.Message
      | f :: rest -> sprintf "%s（ほか %d 件）" f.Message rest.Length
    let failed (fs: Failure list) =
      let marks =
        fs
        |> List.filter (fun f -> f.Line > 0)
        |> List.map (fun f ->
            sprintf
              "{\"line\":%d,\"column\":%d,\"endColumn\":%d,\"message\":%s}"
              f.Line f.Column f.EndColumn (jstr f.Message))
        |> String.concat ","
      sprintf "{\"ok\":false,\"message\":%s,\"marks\":[%s]}" (jstr (banner fs)) marks
    // **建ててから差し替える。** 木は読めるが組めない層が在るので、
    // 先に `current` を書くと、落ちたあとの Reset がその弾幕で作り直して
    // また落ちる（`Reset` は `current` から建てる）
    let put bulletml =
      let next = Playfield.Create env bulletml
      current <- bulletml
      field <- next
    // **字を直に書かない。** 送ってくるのは Fable 側の `SourceKind.Id` で、
    // どちらも `LanguageService` の 1 本 を引く。
    //
    // **表記ごとの腕をここに書かない（v0.9）。** 読む口は `SourceReader` に
    // 束ねてある —— 書くと、表記を足すたびにここも直すことになる
    match SourceKind.tryParse kind |> Option.bind SourceReader.tryFind with
    // **組み合わせは `References.explain` 1 本。** 試験も同じそれを通る
    | Some reader ->
      match References.explain reader.Tags (reader.Apply put text) text with
      | [] -> "{\"ok\":true}"
      | fs -> failed fs
    // 知らない字と、まだ読めない表記を分けない。**どちらも人には同じ** ——
    // 分けると「口は在るが読めない」を人に見せることになる
    | None -> failed [ Diagnosis.plain ("未対応: " + kind) ]

  /// **本文を別の表記に書き直す。** 弾幕は差し替えない ——
  /// 読んで書くだけで、走っているものは触らない。
  ///
  /// v1.4 まで「表記を変えても本文はそのまま」だった。**決めたのではなく、
  /// XML 以外 を書く口が repo に無かった。**
  ///
  /// **いまの本文を変換する**（プルダウンの弾幕を読み直すのではない）——
  /// 人が足した字が消えないし、Open したファイルや編集後でも効く。
  /// 読めない本文のときは触らない（呼ぶ側が `ok:false` を見て何もしない）。
  ///
  ///     {"ok":true,"text":"…"}
  ///     {"ok":false,"message":"…"}
  [<JSInvokable>]
  member _.Transcode(fromKind: string, toKind: string, text: string) : string =
    let jstr (s: string) = JsonSerializer.Serialize s
    let ng (message: string) = sprintf "{\"ok\":false,\"message\":%s}" (jstr message)
    match SourceKind.tryParse fromKind |> Option.bind SourceReader.tryFind,
          SourceKind.tryParse toKind |> Option.bind SourceWriter.tryFind with
    | Some reader, Some writer ->
      // **載せない。** `ApplySource` と違って、ここは字を作るだけ
      let mutable got = None
      match reader.Apply (fun b -> got <- Some b) text with
      | Some failure -> ng failure.Message
      | None ->
        match got with
        | None -> ng "読めたが弾幕が取れなかった"
        | Some bulletml ->
          match writer.Write bulletml with
          | Result.Ok written -> sprintf "{\"ok\":true,\"text\":%s}" (jstr written)
          | Result.Error why -> ng why
    | None, _ -> ng ("未対応: " + fromKind)
    | _, None -> ng ("未対応: " + toKind)

/// 空の根。描画のあとで host を JS に渡す。
type MyApp() =
  inherit Component()

  let host = PlaygroundHost()
  let mutable started = false
  let mutable dotNetRef : DotNetObjectReference<PlaygroundHost> = null

  [<Inject>]
  member val JS : IJSRuntime = Unchecked.defaultof<_> with get, set

  override _.Render() = text ""

  override this.OnAfterRenderAsync first =
    if first && not started then
      started <- true
      dotNetRef <- DotNetObjectReference.Create(host)
      this.JS.InvokeVoidAsync("playground.onReady", dotNetRef).AsTask()
    else
      Task.CompletedTask
