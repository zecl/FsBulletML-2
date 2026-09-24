// `namespace` の手前 に `///` は置けない（FS3520 が出る）。
// 型の doc は型に付けてある
namespace FsBulletML2.LanguageService

open System
open System.Text

/// 共有リンクの中身。サーバを持たない。本文そのものが URL に乗る。
/// `Bytes` はここでは解かない。解くのはブラウザ側。
type ShareLink =
    {
        /// どの表記で書かれた本文か。本文だけだと開いた側が決められない
        Kind: SourceKind
        /// 難度（`$rank`）を 100 倍 した整数。0 から 100。
        /// 小数を字にすると runtime で桁が変わるので、整数で載せる
        Rank: int
        /// 乱数の種。同じ種なら同じ走り（`SeededRandom`）
        Seed: int
        /// 圧縮された本文。中身はここでは開かない
        Bytes: byte[]
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ShareLink =

    /// 形の版。頭に置く。
    /// 無いと、形を変えたとき古いリンクを黙って誤読する。
    let version = "2"

    /// 難度は 100 倍 の整数で載る。上限
    [<Literal>]
    let RankScale = 100

    /// 区切り。base64url に出ない字を選ぶ（出る字だと分け目が動く）
    let private separator = '.'

    let private isUrlChar (c: char) =
        (c >= 'A' && c <= 'Z')
        || (c >= 'a' && c <= 'z')
        || (c >= '0' && c <= '9')
        || c = '-'
        || c = '_'

    /// base64url にする。`+` `/` を替えて `=` を落とす ——
    /// 素の base64 は URL で意味を持つ字を含む
    let toBase64Url (bytes: byte[]) : string =
        if isNull bytes then
            ""
        else
            Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')

    /// base64url を戻す。知らない字が 1 つ でも在れば `None` ——
    /// 素通しにすると、貼るときに切れたリンクが「中身が化けた弾幕」になる
    let tryFromBase64Url (s: string) : byte[] option =
        if isNull s || s.Length = 0 then
            None
        elif s |> Seq.exists (isUrlChar >> not) then
            None
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

                try
                    Some(Convert.FromBase64String padded)
                with _ ->
                    None

    /// 数を 10 進 の字にする。`string` に任せない。
    /// `StringBuilder.Insert` は使うな。Fable に口が無く、焼いた JS だけ落ちる。
    let private digits (n: int) =
        let n = max 0 n

        if n = 0 then
            "0"
        else
            let mutable s = ""
            let mutable v = n

            while v > 0 do
                s <- string (char (int '0' + v % 10)) + s
                v <- v / 10

            s

    /// 10 進 の字を数へ。数字以外 が 1 つ でも在れば `None`
    let private tryDigits (s: string) =
        if isNull s || s.Length = 0 || s.Length > 9 then
            None
        elif s |> Seq.exists (fun c -> c < '0' || c > '9') then
            None
        else
            Some(s |> Seq.fold (fun acc c -> acc * 10 + int c - int '0') 0)

    /// リンクの中身を字にする。組み立てはここ 1 か所で、読む側と対
    let build (kind: SourceKind) (rank: int) (seed: int) (bytes: byte[]) : string =
        let rank =
            if rank < 0 then 0
            elif rank > RankScale then RankScale
            else rank

        let sb = StringBuilder()

        sb
            .Append(version)
            .Append(separator)
            .Append(kind.Id)
            .Append(separator)
            .Append(digits rank)
            .Append(separator)
            .Append(digits seed)
            .Append(separator)
            .Append(toBase64Url bytes)
            .ToString()

    /// 字からリンクを戻す。読めないときは理由を返す。空に黙らない。
    /// 頭の `#` は在っても無くてもよい。
    let tryParse (fragment: string) : Result<ShareLink, string> =
        let s =
            if isNull fragment then ""
            elif fragment.StartsWith "#" then fragment.Substring 1
            else fragment

        if s.Length = 0 then
            Result.Error "共有リンクが空"
        else
            let parts = s.Split separator
            // 版だけは形が違っても読む。古いリンクに「形が違う」と出すな。
            let v = if parts.Length > 0 then parts.[0] else ""

            if v <> version then
                Result.Error(sprintf "このリンクは読めない（版 %s / いまは %s）" v version)
            elif parts.Length <> 5 then
                Result.Error "共有リンクの形が違う"
            else
                let id = parts.[1]

                match SourceKind.tryParse id with
                | None -> Result.Error(sprintf "知らない表記: %s" id)
                | Some kind ->
                    match tryDigits parts.[2], tryDigits parts.[3] with
                    | None, _
                    | _, None -> Result.Error "共有リンクの走らせ方が読めない"
                    | Some rank, Some seed when rank > RankScale ->
                        ignore seed
                        Result.Error "共有リンクの難度が範囲の外"
                    | Some rank, Some seed ->
                        match tryFromBase64Url parts.[4] with
                        | None -> Result.Error "共有リンクの中身が壊れている"
                        | Some bytes ->
                            Result.Ok
                                {
                                    Kind = kind
                                    Rank = rank
                                    Seed = seed
                                    Bytes = bytes
                                }

    /// 2 runtime の突き合わせ口。組み立てはここ 1 か所。
    /// バイト列を表に書かない。`byte[]` の JSON は 2 通り に書ける。
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
            add (digits link.Rank)
            add "/"
            add (digits link.Seed)
            add "/"
            add (string link.Bytes.Length)
            add "/"
            // 中身そのもの。長さだけだと、並びが入れ替わっても気づかない
            add (link.Bytes |> Array.map int |> Array.map string |> String.concat "-")
        // 長さから決まる並び。0 バイト も通す（`toBase64Url` が空を返す側）
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

            add (
                if back.Length = n && Array.forall2 (=) back bytes then
                    "same"
                else
                    "differs"
            )

        add " link="
        // 走らせ方も長さから決める。 表に数を並べると、そちらが 2 つ 目 の表になる
        add (build SourceKind.Xml (n * 7 % (RankScale + 1)) (n * 12345 + 1) bytes)
        sb.ToString()
