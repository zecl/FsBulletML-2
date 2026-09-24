namespace FsBulletML2.Parser.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// 共有リンクの、.NET でも走る側。
/// 圧縮はブラウザにしか無い。ここは base64url と、版・表記の組み立て。
[<TestFixture>]
type ShareLinkTests() =

    /// 決まった並びのバイト列。乱数にしない ——
    /// 落ちたときに同じものをもう一度 作れないと、原因を追えない
    let bytesOf (n: int) =
        Array.init n (fun i -> byte ((i * 37 + 11) % 256))

    [<Test>]
    member _.``base64url は URL で意味を持つ字を出さない``() =
        // `+` `/` `=` はどれも URL の中で別の意味を持つ。
        // `=` は fragment では通るが、貼るときに切られることが在る
        for n in 0..300 do
            let s = ShareLink.toBase64Url (bytesOf n)
            s |> Seq.exists (fun c -> c = '+' || c = '/' || c = '=') |> should equal false

    [<Test>]
    member _.``区切りの字が base64url に出ない``() =
        // 出ると `tryParse` の分け目が中身の途中で動く
        for n in 0..300 do
            ShareLink.toBase64Url (bytesOf n) |> Seq.contains '.' |> should equal false

    [<Test>]
    member _.``バイト列は往復する``() =
        // 長さは 4 で割った余りが 3 通り とも通る（0 は下の点）
        for n in 1..300 do
            let bytes = bytesOf n

            match ShareLink.tryFromBase64Url (ShareLink.toBase64Url bytes) with
            | Some back -> back |> should equal bytes
            | None -> failwithf "%d バイト が戻らなかった" n

    [<Test>]
    member _.``0 バイト はリンクにならない``() =
        // `toBase64Url` が空を返し、`tryParse` は中身が壊れていると読む。
        // ここが黙って通ると「空のリンク」が作れてしまう。
        ShareLink.toBase64Url [||] |> should equal ""
        ShareLink.tryFromBase64Url "" |> should equal None

        match ShareLink.tryParse (ShareLink.build SourceKind.Xml 50 7 [||]) with
        | Result.Error why -> why |> should equal "共有リンクの中身が壊れている"
        | Result.Ok _ -> failwith "空の中身が読めてしまった"

    [<Test>]
    member _.``base64url に無い字は通さない``() =
        // 素通しにすると、貼るときに切れたリンクが「中身の化けた弾幕」になる
        for s in [ "AQ*D"; "AQ+D"; "AQ/D"; "AQID="; "AQ ID"; "AQ\nD" ] do
            ShareLink.tryFromBase64Url s |> should equal None

    [<Test>]
    member _.``余りが 1 になる長さは base64 に無い``() =
        ShareLink.tryFromBase64Url "AQIDA" |> should equal None

    [<Test>]
    member _.``4 表記 とも往復する``() =
        for kind in SourceKind.all do
            let bytes = bytesOf 64

            match ShareLink.tryParse (ShareLink.build kind 50 7 bytes) with
            | Result.Ok link ->
                link.Kind |> should equal kind
                link.Bytes |> should equal bytes
                link.Rank |> should equal 50
                link.Seed |> should equal 7
            | Result.Error why -> failwithf "%s が戻らなかった: %s" kind.Id why

    [<Test>]
    member _.``走らせ方も往復する``() =
        // 同じ本文でも、難度と種が違えば別の絵（版 2 で乗せた）——
        // 端も通す（0 と 100、種の上限）
        for (rank, seed) in [ 0, 1; 100, 999999; 37, 12345 ] do
            match ShareLink.tryParse (ShareLink.build SourceKind.Xml rank seed (bytesOf 8)) with
            | Result.Ok link ->
                link.Rank |> should equal rank
                link.Seed |> should equal seed
            | Result.Error why -> failwithf "%d/%d が戻らなかった: %s" rank seed why

    [<Test>]
    member _.``難度は範囲で丸める``() =
        // `JSInvokable` の先から来る値なので、UI に無い数も来うる
        let rankOf (n: int) =
            match ShareLink.tryParse (ShareLink.build SourceKind.Xml n 1 (bytesOf 4)) with
            | Result.Ok link -> link.Rank
            | Result.Error why -> failwith why

        rankOf -5 |> should equal 0
        rankOf 999 |> should equal ShareLink.RankScale

    [<Test>]
    member _.``走らせ方が違えばリンクも違う``() =
        // 上の往復は、乗せていなくても「同じものが戻った」で緑になる
        let link r s =
            ShareLink.build SourceKind.Xml r s (bytesOf 8)

        link 50 1 |> should not' (equal (link 60 1))
        link 50 1 |> should not' (equal (link 50 2))

    [<Test>]
    member _.``版が頭に出る``() =
        // 形を変えたときに、古いリンクを黙って誤読しないための字
        ShareLink.build SourceKind.Xml 50 7 (bytesOf 8)
        |> _.StartsWith(ShareLink.version + ".")
        |> should equal true

    [<Test>]
    member _.``違う版は読まない``() =
        match ShareLink.tryParse "1.xml.50.7.AQID" with
        | Result.Error why -> why |> should contain "版"
        | Result.Ok _ -> failwith "違う版が読めてしまった"

    [<Test>]
    member _.``頭 の シャープ は在っても無くてもよい``() =
        // `location.hash` は付けて返す
        let link = ShareLink.build SourceKind.Sxml 50 7 (bytesOf 16)
        ShareLink.tryParse link |> should equal (ShareLink.tryParse ("#" + link))

    [<Test>]
    member _.``読めないときは理由を返す``() =
        // 黙って空にしない。 開いた人には「踏んだのに何も起きない」に見える
        let why (fragment: string) =
            match ShareLink.tryParse fragment with
            | Result.Error w -> w
            | Result.Ok _ -> failwithf "読めてしまった: %s" fragment

        why "" |> should equal "共有リンクが空"
        why "#" |> should equal "共有リンクが空"
        why null |> should equal "共有リンクが空"
        why "2.xml" |> should equal "共有リンクの形が違う"
        why "2.xml.50.7.AQID.AQID" |> should equal "共有リンクの形が違う"
        why "2.nope.50.7.AQID" |> should equal "知らない表記: nope"
        why "2.XML.50.7.AQID" |> should equal "知らない表記: XML"
        why "2.xml.x.7.AQID" |> should equal "共有リンクの走らせ方が読めない"
        why "2.xml.50.x.AQID" |> should equal "共有リンクの走らせ方が読めない"
        why "2.xml.101.7.AQID" |> should equal "共有リンクの難度が範囲の外"

    [<Test>]
    member _.``表記は器の 1 本 から引く``() =
        // ここに `"xml"` の表を持つと、表記を足したときそちらだけ古びる
        for kind in SourceKind.all do
            match ShareLink.tryParse (ShareLink.build kind 50 7 (bytesOf 4)) with
            | Result.Ok link -> link.Kind.Id |> should equal kind.Id
            | Result.Error why -> failwithf "%s: %s" kind.Id why
