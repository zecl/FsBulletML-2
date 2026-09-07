// **`namespace` の手前 に `///` は置けない**（FS3520 が出る）。
// 型の doc は型に付けてある
namespace FsBulletML2.LanguageService

open System
open System.Text

/// 共有リンクの中身。**サーバを持たない** —— 本文そのものが URL に乗る。
///
/// 字にすると `<版>.<表記の Id>.<base64url>`。fragment（`#` の後ろ）に置くので
/// サーバへは送られない。
///
/// ## 中身を開かない
///
/// `Bytes` は圧縮された本文で、**ここでは解かない。** 解くのはブラウザ側の
/// `DecompressionStream` で、そちらは .NET に無い —— この器は
/// **2 つ の runtime で走る側**なので、片方 にしか無い物を持てない。
///
/// 逆に言うと、**base64url と組み立てはここに全部 在る。** そこは .NET でも
/// 走るので `Parser.Tests` から当てられるし、答えが 2 つ の runtime で
/// 一致することは `guard-fable-parity.ps1` が走行で見ている。
type ShareLink =
  { /// どの表記で書かれた本文か。**本文だけだと開いた側が決められない**
    Kind: SourceKind
    /// 圧縮された本文。**中身はここでは開かない**
    Bytes: byte[] }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ShareLink =

  /// 形の版。**頭に置く。**
  ///
  /// 置かないと、形を変えたときに古いリンクを黙って誤読する ——
  /// 圧縮の形式を替えれば base64url は通るのに中身だけが化ける、が起きる。
  /// 版が違えば「このリンクは読めない」と言って止まる
  let version = "1"

  /// 区切り。**base64url に出ない字**を選ぶ（出る字だと分け目が動く）
  let private separator = '.'

  let private isUrlChar (c: char) =
    (c >= 'A' && c <= 'Z')
    || (c >= 'a' && c <= 'z')
    || (c >= '0' && c <= '9')
    || c = '-'
    || c = '_'

  /// base64url にする。**`+` `/` を替えて `=` を落とす** ——
  /// 素の base64 は URL で意味を持つ字を含む
  let toBase64Url (bytes: byte[]) : string =
    if isNull bytes then ""
    else Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')

  /// base64url を戻す。**知らない字が 1 つ でも在れば `None`** ——
  /// 素通しにすると、貼るときに切れたリンクが「中身が化けた弾幕」になる
  let tryFromBase64Url (s: string) : byte[] option =
    if isNull s || s.Length = 0 then None
    elif s |> Seq.exists (isUrlChar >> not) then None
    else
      // 長さ 4 で割った余りが 1 になる base64 は無い
      let pad =
        match s.Length % 4 with
        | 0 -> Some ""
        | 2 -> Some "=="
        | 3 -> Some "="
        | _ -> None
      match pad with
      | None -> None
      | Some pad ->
        let padded = s.Replace('-', '+').Replace('_', '/') + pad
        try Some(Convert.FromBase64String padded) with _ -> None

  /// リンクの中身を字にする。**組み立てはここ 1 か所**で、読む側と対
  let build (kind: SourceKind) (bytes: byte[]) : string =
    let sb = StringBuilder()
    sb.Append(version).Append(separator).Append(kind.Id).Append(separator)
      .Append(toBase64Url bytes).ToString()

  /// 字からリンクを戻す。**読めないときは理由を返す** ——
  /// 黙って空にすると、人には「開いたのに何も起きない」に見える。
  ///
  /// 頭の `#` は在っても無くてもよい（`location.hash` は付けて返す）
  let tryParse (fragment: string) : Result<ShareLink, string> =
    let s =
      if isNull fragment then ""
      elif fragment.StartsWith "#" then fragment.Substring 1
      else fragment
    if s.Length = 0 then Result.Error "共有リンクが空"
    else
      let parts = s.Split separator
      if parts.Length <> 3 then
        Result.Error "共有リンクの形が違う"
      else
        let v = parts.[0]
        let id = parts.[1]
        let payload = parts.[2]
        if v <> version then
          Result.Error(sprintf "このリンクは読めない（版 %s / いまは %s）" v version)
        else
          match SourceKind.tryParse id with
          | None -> Result.Error(sprintf "知らない表記: %s" id)
          | Some kind ->
            match tryFromBase64Url payload with
            | None -> Result.Error "共有リンクの中身が壊れている"
            | Some bytes -> Result.Ok { Kind = kind; Bytes = bytes }

  /// 2 つ の runtime で同じ答えが返ることを見る口。
  /// **組み立てはここ 1 か所。** node 側 と .NET 側 で別々に組むと、
  /// 組み方のほうが食い違って「中身は同じなのに赤」になる。
  ///
  /// 渡された字を 2 通り に使う —— **読ませる**のと、
  /// **長さから決まったバイト列を作って往復させる**の。
  /// バイト列を表に書かないのは、`byte[]` を JSON に載せると
  /// 表の側で 2 通り の書き方ができてしまうため
  let describe (fragment: string) : string =
    let sb = StringBuilder()
    let add (s: string) = sb.Append s |> ignore
    match tryParse fragment with
    | Result.Error why ->
      add "err:"
      add why
    | Result.Ok link ->
      add "ok:"
      add link.Kind.Id
      add "/"
      add (string link.Bytes.Length)
      add "/"
      // 中身そのもの。**長さだけだと、並びが入れ替わっても気づかない**
      add (link.Bytes |> Array.map int |> Array.map string |> String.concat "-")
    // 長さから決まる並び。**0 バイト も通す**（`toBase64Url` が空を返す側）
    let n = (if isNull fragment then 0 else fragment.Length) % 13
    let bytes = Array.init n (fun i -> byte ((i * 37 + 11) % 256))
    let encoded = toBase64Url bytes
    add " b64="
    add (if encoded = "" then "-" else encoded)
    add " back="
    match tryFromBase64Url encoded with
    | None -> add "none"
    | Some back ->
      add (string back.Length)
      add ":"
      add (if back.Length = n && Array.forall2 (=) back bytes then "same" else "differs")
    add " link="
    add (build SourceKind.Xml bytes)
    sb.ToString()
