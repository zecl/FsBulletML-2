/// 共有リンクの、ブラウザにしか無い側。
///
/// 圧縮は `CompressionStream`。組み立てと base64url は器の `ShareLink` に在り、
/// あちらは .NET でも走るので `Parser.Tests` と `guard-fable-parity.ps1` が当てている。
/// ここに残っているのは、片方 の runtime にしか無い物だけ。
/// ## ライブラリを足さない
/// `CompressionStream` / `DecompressionStream` / `TextEncoder` はブラウザに在る。
module FsBulletML2.Fable.Share

open Fable.Core
open FsBulletML2.LanguageService

/// 読めたかどうか。`Result` を返さない ——
/// 焼くと tag と fields になり、JS から呼ぶ側（門）がその形を知ることになる。
/// ここは runtime をまたぐ境界なので、素の欄で持つ
type ShareRead =
  { Ok: bool
    /// 読めたときの表記の Id。読めなければ空
    KindId: string
    /// 読めたときの本文。読めなければ空
    Text: string
    /// 難度を 100 倍 した整数（版 2）。読めなければ -1
    Rank: int
    /// 乱数の種（版 2）。読めなければ -1
    Seed: int
    /// 読めなかった理由。読めれば空
    Message: string }

[<Emit("Promise.resolve($0)")>]
let private resolved (v: 'a) : JS.Promise<'a> = jsNative

[<Emit("$1.then($0)")>]
let private pmap (f: 'a -> 'b) (p: JS.Promise<'a>) : JS.Promise<'b> = jsNative

[<Emit("$1.catch($0)")>]
let private pcatch (f: obj -> 'a) (p: JS.Promise<'a>) : JS.Promise<'a> = jsNative

// 読む側を先に始める。 書くほうは読まれるまで詰まるので、
// 先に `Response` を作らずに `await w.write(...)` すると大きい本文で止まる。
//
// 書くほうの失敗はその場で捨てる。 壊れた中身を渡すと `close` も
// `out` も落ちるが、`close` の側を受け取らないと 誰も待っていない拒否
// になって、window の `unhandledrejection` に出る（node なら process ごと落ちた）。
// 理由は `out` の側から同じものが出るので、片方 だけ通せばよい
[<Emit("""(async (text) => {
  const s = new CompressionStream('deflate-raw')
  const out = new Response(s.readable).arrayBuffer()
  const w = s.writable.getWriter()
  w.write(new TextEncoder().encode(text)).catch(() => {})
  w.close().catch(() => {})
  return new Uint8Array(await out)
})($0)""")>]
let private deflate (text: string) : JS.Promise<byte[]> = jsNative

[<Emit("""(async (bytes) => {
  const s = new DecompressionStream('deflate-raw')
  const out = new Response(s.readable).arrayBuffer()
  const w = s.writable.getWriter()
  w.write(bytes).catch(() => {})
  w.close().catch(() => {})
  return new TextDecoder().decode(new Uint8Array(await out))
})($0)""")>]
let private inflate (bytes: byte[]) : JS.Promise<string> = jsNative

let private ng (message: string) =
  { Ok = false; KindId = ""; Text = ""; Rank = -1; Seed = -1; Message = message }

/// 本文と走らせ方を fragment にする。頭 の `#` は付けない（付ける側が決める）。
///
/// 走らせ方も乗せる（版 2） —— 同じ本文でも、難度と種が違えば別の絵。
/// リンクが指しているのは「その走り」
let encode (kind: SourceKind) (rank: int) (seed: int) (text: string) : JS.Promise<string> =
  deflate text |> pmap (fun bytes -> ShareLink.build kind rank seed bytes)

/// fragment を本文に戻す。
///
/// 読めないときは理由を返す。 黙って空にすると、開いた人には
/// 「リンクを踏んだのに何も起きない」に見える —— 起きなかったのか
/// 起きて空だったのかが、そこから区別できない
let decode (fragment: string) : JS.Promise<ShareRead> =
  match ShareLink.tryParse fragment with
  | Result.Error why -> resolved (ng why)
  | Result.Ok link ->
    inflate link.Bytes
    |> pmap (fun text ->
         { Ok = true
           KindId = link.Kind.Id
           Text = text
           Rank = link.Rank
           Seed = link.Seed
           Message = "" })
    // 中身が base64url を通っても、解けるとは限らない
    |> pcatch (fun _ -> ng "共有リンクの中身を開けない")
