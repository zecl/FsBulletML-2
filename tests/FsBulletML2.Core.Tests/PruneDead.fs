module FsBulletML2.Core.Tests.PruneDead

open System.IO
open NUnit.Framework
open FsUnit
open FsBulletML2

/// 書きかけ の木 から 走らない 枝 を落とす ところ。
[<TestFixture>]
type PruneDead() =

    static let wrap (body: string) =
        "<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + body + "</bulletml>"

    static let read (src: string) = Bulletml.ReadXmlString(wrap src)

    static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

    static let pruned (src: string) = xml (Prune.apply (read src))

    /// 根直下 の要素 の数
    static let tops (b: Bulletml) =
        match b with
        | Bulletml(_, elms) -> List.length elms

    /// `<actionRef>` を 消した あと の形。`spin` を 誰 も 引いて いない
    static let orphan =
        """<action label="top"><fire><bullet/></fire></action>
       <action label="spin"><wait>10</wait></action>"""

    [<Test>]
    member _.``引かれて いない 根直下 の action が落ちる``() =
        let b = read orphan
        tops b |> should equal 2
        Prune.count b |> should equal 1
        tops (Prune.apply b) |> should equal 1
        pruned orphan |> should not' (haveSubstring "spin")
        pruned orphan |> should haveSubstring "top"

    /// 走らせる 側 は 名前 が `top` で始まる action を 並行 に走らせる。
    /// 引く 人 が 居なくて も 入口
    [<Test>]
    member _.``top で始まる action は 引かれて いなくて も 残る``() =
        let src =
            """<action label="top"><fire><bullet/></fire></action>
         <action label="top2"><wait>10</wait></action>"""

        Prune.count (read src) |> should equal 0
        pruned src |> should haveSubstring "top2"

    [<Test>]
    member _.``引かれて いる 定義 は 残る``() =
        let src =
            """<action label="top"><actionRef label="spin"/></action>
         <action label="spin"><wait>10</wait></action>"""

        Prune.count (read src) |> should equal 0
        pruned src |> should haveSubstring "spin"

    /// 種類 が違う 名前 は ぶつからない。`action:core` を引いて いて も
    /// `bullet:core` は 生きない
    [<Test>]
    member _.``名前 が同じ でも 種類 が違え ば 引いた ことに ならない``() =
        let src =
            """<action label="top"><actionRef label="core"/></action>
         <action label="core"><wait>10</wait></action>
         <bullet label="core"><action><wait>5</wait></action></bullet>"""

        Prune.count (read src) |> should equal 1
        let got = pruned src
        got |> should haveSubstring "<action label=\"core\">"
        got |> should not' (haveSubstring "<bullet label=\"core\">")

    /// 入口 から 2 段 離れた 定義。1 周 で止める と ここ が落ちる
    [<Test>]
    member _.``引いた 先 が また 引く 定義 も 残る``() =
        let src =
            """<action label="top"><actionRef label="a"/></action>
         <action label="a"><actionRef label="b"/></action>
         <action label="b"><actionRef label="c"/></action>
         <action label="c"><wait>10</wait></action>"""

        Prune.count (read src) |> should equal 0
        tops (Prune.apply (read src)) |> should equal 4

    /// `BulletmlOps.getAction` は 木 の 隅々 まで 歩く ので、
    /// 深い ところ の `label` も `actionRef` から 引ける
    [<Test>]
    member _.``入れ子 の 定義 も 引き先 になる``() =
        let src =
            """<action label="top"><actionRef label="deep"/></action>
         <action label="holder"><action label="deep"><wait>10</wait></action></action>"""

        Prune.count (read src) |> should equal 0
        pruned src |> should haveSubstring "holder"

    /// 入れ子 の `<action label>` は 親 から 順 に走る。
    /// 名前 が 浮いて いる ことと、その枝 が走らない ことは 別
    [<Test>]
    member _.``入れ子 の 浮いた 名前 は 落とさない``() =
        let src =
            """<action label="top"><action label="unused"><wait>10</wait></action></action>"""

        Prune.count (read src) |> should equal 0
        pruned src |> should haveSubstring "unused"

    /// 根直下 の `bullet` と `fire` は 自分 では 走らない
    /// （`Api.fs` の `scripts` が `action` だけ を選ぶ）
    [<Test>]
    member _.``引かれて いない 根直下 の bullet と fire が落ちる``() =
        let src =
            """<action label="top"><fire><bullet/></fire></action>
         <bullet label="core"><action><wait>5</wait></action></bullet>
         <fire label="shot"><bullet/></fire>"""

        Prune.count (read src) |> should equal 2
        tops (Prune.apply (read src)) |> should equal 1

    [<Test>]
    member _.``bulletRef と fireRef で 引いた 定義 は 残る``() =
        let src =
            """<action label="top"><fire><bulletRef label="core"/></fire><fireRef label="shot"/></action>
         <bullet label="core"><action><wait>5</wait></action></bullet>
         <fire label="shot"><bullet/></fire>"""

        Prune.count (read src) |> should equal 0
        tops (Prune.apply (read src)) |> should equal 3

    /// 名前 を持たない 根直下 の bullet / fire は 引きよう が無く、走り も しない
    [<Test>]
    member _.``無名 の 根直下 の bullet と fire が落ちる``() =
        let src =
            """<action label="top"><fire><bullet/></fire></action>
         <bullet><action><wait>5</wait></action></bullet>"""

        Prune.count (read src) |> should equal 1

    /// 落ちた 結果 その定義 だけ が引いて いた もの も 落ちる。
    /// 1 度 で 落としきる —— 押す たび に 減る 形 だと、
    /// 「何回 押せば 終わる か」が 人 に分からない
    [<Test>]
    member _.``死んだ 枝 だけ が引いて いた 定義 も 一緒 に落ちる``() =
        let src =
            """<action label="top"><fire><bullet/></fire></action>
         <action label="dead"><actionRef label="helper"/></action>
         <action label="helper"><wait>10</wait></action>"""

        Prune.count (read src) |> should equal 2
        tops (Prune.apply (read src)) |> should equal 1
        // もう一度 当てて も 減らない
        let once = Prune.apply (read src)
        Prune.count once |> should equal 0

    [<Test>]
    member _.``落とす もの が 無ければ 字 が 1 つ も 動かない``() =
        let src = """<action label="top"><fire><bullet/></fire></action>"""
        xml (Prune.apply (read src)) |> should equal (xml (read src))

    /// いちばん 強い 門。落とす のは 1 度 も 走らない 枝 だけ なので、
    /// 走らせた 軌跡 は 1 ビット も 動かない ——
    /// 「見た目 が変わらない」を 字 でなく 走行 で言う
    [<Test>]
    member _.``落とす 前後 で 軌跡 が 一致 する``() =
        let readable =
            CorpusData.uniqueSamples ()
            |> List.choose (fun f ->
                try
                    let src = File.ReadAllText f
                    Some(CorpusData.relative f, src, Bulletml.ReadXmlString src)
                with _ ->
                    None)

        // 当てる 先 が 本当 に在る か を 先 に見る。0 本 なら 緑 にしない
        let victims = readable |> List.filter (fun (_, _, b) -> Prune.count b > 0)

        if List.isEmpty victims then
            Assert.Fail(sprintf "samples %d 本 に 落ちる 枝 が 1 本 も ありません。この点 は 何 も 測って いません" (List.length readable))

        let mutable checked' = 0

        for (name, src, b) in victims do
            // 走行 で 落ちる 弾幕 が 在る（`CorpusData` の但し書き）。
            // 両側 が 同じ 形 で落ちた なら 軌跡 は 比べられない ので 飛ばす
            let run (s: string) =
                try
                    Some(TraceRun.std s 60)
                with _ ->
                    None

            match run src, run (BulletmlWriter.toIndentedXml 4 (Prune.apply b)) with
            | Some before, Some after ->
                after |> should equal before
                checked' <- checked' + 1
            | None, None -> ()
            | _ -> Assert.Fail(sprintf "%s は 片方 だけ が走った" name)

        if checked' = 0 then
            Assert.Fail(sprintf "落ちる 枝 を持つ %d 本 が どれ も 走らず、軌跡 を 1 度 も 比べて いません" (List.length victims))

        TestContext.Out.WriteLine(
            sprintf "samples %d 本 中 %d 本 に 落ちる 枝。%d 本 の軌跡 が 一致" (List.length readable) (List.length victims) checked'
        )
