namespace FsBulletML2.Playground

open System.Text.Json
open System.Threading.Tasks
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open FsBulletML2
open FsBulletML2.Bullets.Dsl

/// 起動時に載せる弾幕。**同梱カタログの CE が正本。**
///
/// 欄に出す XML は `ToIndentedXmlString` で焼く —— html に直書きすると、
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
  let mutable playing = false
  let ret = Array.zeroCreate<float> 2
  let catalog = lazy (All.bullets |> List.toArray)

  /// `[n; ptr]`。Apply で配列が差し替わるので ptr は毎コマ返す。
  [<JSInvokable>]
  member _.StepFrame(_now: float, playerX: float, playerY: float) : float[] =
    env.SetPlayer (float32 playerX) (float32 playerY)
    if playing then field.Tick()
    ret.[0] <- float (field.Pack())
    ret.[1] <- field.PackedPtr
    ret

  [<JSInvokable>]
  member _.Play() = playing <- true

  [<JSInvokable>]
  member _.Pause() = playing <- false

  [<JSInvokable>]
  member _.Reset() =
    field <- Playfield.Create env current

  /// 起動時に欄へ出す XML。**html に直書きしない。**
  [<JSInvokable>]
  member _.InitialSource() : string = current.ToIndentedXmlString()

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
        info.Bulletml.ToIndentedXmlString()
    with ex -> "ERROR:" + ex.Message

  /// 右側の本文を読んで弾幕を差し替える。
  ///
  /// **どの表記かを受け取る。** v0.3 が読めるのは `"xml"` だけだが、
  /// 口だけ先に開けておく —— テキストだけ受け取る形にすると、
  /// 呼ぶ側にも XML が焼き込まれて、次の言語で両方 直すことになる。
  ///
  /// sxml / fsb は Parser に既に口が在る（`tryReadSxmlString` /
  /// `tryReadFsbString`）。載せるのはここに腕を 1 本 足すだけ。
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
    match kind with
    | "xml" ->
      // **建ててから差し替える。** 木は読めるが組めない層が在るので、
      // 先に `current` を書くと、落ちたあとの Reset がその弾幕で作り直して
      // また落ちる（`Reset` は `current` から建てる）
      let put bulletml =
        let next = Playfield.Create env bulletml
        current <- bulletml
        field <- next
      // **組み合わせは `References.explain` 1 本。** 試験も同じそれを通る
      match References.explain (Diagnosis.apply put text) text with
      | [] -> "{\"ok\":true}"
      | fs -> failed fs
    | other -> failed [ Diagnosis.plain ("未対応: " + other) ]

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
