/// 共有リンクの、ブラウザにしか無い側。圧縮だけここ。
/// 組み立ては器の `ShareLink`。ライブラリは足さない。
module FsBulletML2.Fable.Share

open Fable.Core
open FsBulletML2.LanguageService

/// 読めたかどうか。`Result` は返すな。焼くと tag と fields になる。
/// runtime をまたぐ境界なので、素の欄で持つ。
type ShareRead =
    {
        Ok: bool
        /// 読めたときの表記の Id。読めなければ空
        KindId: string
        /// 読めたときの本文。読めなければ空
        Text: string
        /// 難度を 100 倍 した整数（版 2）。読めなければ -1
        Rank: int
        /// 乱数の種（版 2）。読めなければ -1
        Seed: int
        /// 読めなかった理由。読めれば空
        Message: string
    }

[<Emit("Promise.resolve($0)")>]
let private resolved (v: 'a) : JS.Promise<'a> = jsNative

[<Emit("$1.then($0)")>]
let private pmap (f: 'a -> 'b) (p: JS.Promise<'a>) : JS.Promise<'b> = jsNative

[<Emit("$1.catch($0)")>]
let private pcatch (f: obj -> 'a) (p: JS.Promise<'a>) : JS.Promise<'a> = jsNative

// 読む側を先に始める。`await w.write` を先にすると大きい本文で止まる。
// 書くほうの失敗は捨てる。`close` を受け取らないと unhandledrejection になる。
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
    {
        Ok = false
        KindId = ""
        Text = ""
        Rank = -1
        Seed = -1
        Message = message
    }

/// 本文と走らせ方を fragment にする。頭の `#` は付けない。
let encode (kind: SourceKind) (rank: int) (seed: int) (text: string) : JS.Promise<string> =
    deflate text |> pmap (fun bytes -> ShareLink.build kind rank seed bytes)

/// fragment を本文に戻す。読めないときは理由を返す。
/// 黙って空にすると、起きなかったのか空だったのか分からない。
let decode (fragment: string) : JS.Promise<ShareRead> =
    match ShareLink.tryParse fragment with
    | Result.Error why -> resolved (ng why)
    | Result.Ok link ->
        inflate link.Bytes
        |> pmap (fun text ->
            {
                Ok = true
                KindId = link.Kind.Id
                Text = text
                Rank = link.Rank
                Seed = link.Seed
                Message = ""
            })
        // 中身が base64url を通っても、解けるとは限らない
        |> pcatch (fun _ -> ng "共有リンクの中身を開けない")
