namespace FsBulletML2.Parser.Tests

open System
open System.IO
open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2.LanguageService

/// `<!ELEMENT ...>` / `<!ATTLIST ...>` を、語彙から組む。
/// 表に書き写すと Core が動いたとき表だけが古びる。0 件 の一致は緑になる。
[<TestFixture>]
type DtdLines() =

    /// 行を、当てられる鍵にする。`<!ELEMENT x ...>` なら x、
    /// `<!ATTLIST x y ...>` なら x/@y
    static let key (line: string) =
        let m = Regex.Match(line, @"^<!(ELEMENT|ATTLIST)\s+(\S+)(?:\s+(\S+))?")

        if not m.Success then line
        elif m.Groups.[1].Value = "ELEMENT" then m.Groups.[2].Value
        else m.Groups.[2].Value + "/@" + m.Groups.[3].Value

    /// 写した人の癖を落とす
    static let norm (s: string) =
        Regex.Replace(s, @"\s+", " ").Replace(" )", ")").Replace("( ", "(")

    static let generated =
        [
            for e in Vocabulary.elements do
                yield e.Dtd

                for a in e.Attrs do
                    yield a.Dtd
        ]
        |> List.map norm
        |> List.distinct

    /// XML doc file に載った手書きの写し。同じ鍵で 2 通り の字が在れば
    /// そのまま持つ（写しどうしの食い違いを別の点で見るため）
    static let hand =
        lazy
            let path = Path.Combine(AppContext.BaseDirectory, "FsBulletML2.Core.xml")

            if not (File.Exists path) then
                []
            else
                let xml = File.ReadAllText path

                let decode (s: string) =
                    s
                        .Replace("&lt;", "<")
                        .Replace("&gt;", ">")
                        .Replace("&quot;", "\"")
                        .Replace("&apos;", "'")
                        .Replace("&amp;", "&")

                Regex.Matches(xml, @"&lt;!(ELEMENT|ATTLIST)[^\r\n]*?&gt;")
                |> Seq.map (fun m -> norm (decode m.Value))
                |> Seq.toList

    /// 生成と手書きが食い違うと分かっているもの。
    /// 直ったら（一致したら）下の点が赤くなる
    static let knownDifferent =
        [
            "action", "F# の DU にはヘッジが無い。手書きは (fire | fireRef) / (action | actionRef) と括るが、Action の腕は平らに並んでいる"
            "vanish", "手書きは (#PCDATA)。読む側が中身を許すのを写したもので、腕は持ち物を持たない"
            "bulletml/@type", "手書きは \"none\"、実装が属性なしのとき返すのは vertical。RELAX 定義に既定値は無いので、どちらが原文かを辿れない"
        ]
        |> Map.ofList

    /// 手書きに相当が無い。 仕様に無い、この実装だけの属性
    static let generatedOnly = set [ "bulletml/@name"; "bulletml/@description" ]

    static let handByKey =
        lazy
            (hand.Value
             |> List.groupBy key
             |> List.map (fun (k, v) -> k, List.distinct v)
             |> dict)

    static let generatedByKey = generated |> List.map (fun l -> key l, l) |> dict

    [<Test>]
    member _.``生成も手書きも空でない``() =
        // どちらかが空だと、下の突き合わせは「両方 空で緑」になる
        generated.Length |> should greaterThan 0
        hand.Value.Length |> should greaterThan 0

    [<Test>]
    member _.``手書きの写しどうしが食い違っていない``() =
        // doc コメントの写しは 2 か所 に在る。同じ鍵で 2 通り の字が出たら、
        // どちらかが古い
        let split =
            [
                for kv in handByKey.Value do
                    if kv.Value.Length > 1 then
                        yield sprintf "%s: %s" kv.Key (String.concat " / " kv.Value)
            ]

        split |> should be Empty

    [<Test>]
    member _.``生成と手書きが、同じ相手を並べている``() =
        let g = generatedByKey.Keys |> Set.ofSeq
        let h = handByKey.Value.Keys |> Set.ofSeq
        // 手書きに在って生成が作らない（腕が消えた / 生成が取りこぼした）
        Set.difference h g |> Set.toList |> should be Empty
        // 生成にだけ在る。仕様に無い拡張だけが許される
        Set.difference (Set.difference g h) generatedOnly
        |> Set.toList
        |> should be Empty

    [<Test>]
    member _.``両方に在る行は、食い違いの表に載っていなければ 一致する``() =
        let bad =
            [
                for kv in handByKey.Value do
                    if not (knownDifferent.ContainsKey kv.Key) then
                        match generatedByKey.TryGetValue kv.Key with
                        | true, g when g <> List.head kv.Value ->
                            yield sprintf "%s\n    生成 : %s\n    手書き: %s" kv.Key g (List.head kv.Value)
                        | _ -> ()
            ]

        bad |> should be Empty

    [<Test>]
    member _.``一致した行が 1 つ 以上 在る``() =
        // 上の点は「両方に在る鍵が 0 個」でも緑になる
        let same =
            handByKey.Value
            |> Seq.filter (fun kv ->
                match generatedByKey.TryGetValue kv.Key with
                | true, g -> g = List.head kv.Value
                | _ -> false)
            |> Seq.length

        same |> should greaterThan 0

    [<Test>]
    member _.``食い違いの表に載っているものは、いま本当に食い違っている``() =
        // 載せたまま直ると、表だけが古びて誰も気づかない
        let stale =
            [
                for KeyValue(k, why) in knownDifferent do
                    match handByKey.Value.TryGetValue k, generatedByKey.TryGetValue k with
                    | (true, h), (true, g) when g = List.head h -> yield sprintf "%s: 一致した。表から外すこと（載せていた理由: %s）" k why
                    | (false, _), _ -> yield sprintf "%s: 手書きに無い。表から外すこと" k
                    | _, (false, _) -> yield sprintf "%s: 生成に無い。表から外すこと" k
                    | _ -> ()
            ]

        stale |> should be Empty

    [<Test>]
    member _.``生成にだけ在る表も、いま本当に手書きに無い``() =
        let stale =
            [
                for k in generatedOnly do
                    if handByKey.Value.ContainsKey k then
                        yield sprintf "%s: 手書きに現れた。表から外すこと" k
                    elif not (generatedByKey.ContainsKey k) then
                        yield sprintf "%s: 生成に無い。表から外すこと" k
            ]

        stale |> should be Empty
