namespace FsBulletML2.Sample.Browser

open System.Threading.Tasks
open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open FsBulletML2

/// JSInvokable の実体。アセンブリ名経由だと WASM 起動直後に見つからない。
type PlaygroundHost() =

  let env = BrowserEnv()
  let mutable current = Playfield.Demo
  let mutable field = Playfield.Create env current
  let mutable playing = false
  let ret = Array.zeroCreate<float> 2

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

  /// 右側の XML を読んで弾幕を差し替える。成功なら空文字。
  [<JSInvokable>]
  member _.ApplySource(xml: string) : string =
    try
      match tryReadXmlString xml with
      | None -> "XML を読めなかった"
      | Some bulletml ->
          current <- bulletml
          field <- Playfield.Create env bulletml
          ""
    with ex -> ex.Message

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
