module FsBulletML2.Core.Tests.TuneNumbers

open NUnit.Framework
open FsUnit
open FsBulletML2

/// 手書き の弾幕 の数 だけ を振る ところ。
[<TestFixture>]
type TuneNumbers() =

    static let wrap (body: string) =
        "<?xml version=\"1.0\" ?><bulletml type=\"vertical\">" + body + "</bulletml>"

    static let xml (b: Bulletml) = BulletmlWriter.toIndentedXml 4 b

    static let tune (knob: Tune.Knob) (src: string) =
        xml (Tune.apply knob (Bulletml.ReadXmlString(wrap src)))

    /// 振る 先 が 6 種類 とも 1 つ ずつ 出る 弾幕。
    /// どれか が欠けて いる と、その欄 を触る つまみ が「何 も動かさない」で通る
    static let plain =
        """<action label="top"><repeat><times>8</times><action>
         <fire><direction type="sequence">13</direction>
           <speed>2.0 + $rank * 0.5</speed>
           <bullet><action>
             <accel><horizontal>0.5</horizontal><term>10</term></accel>
           </action></bullet></fire>
         <wait>18 - 9 * $rank</wait>
       </action></repeat></action>"""

    static let knobs =
        [
            Tune.Faster
            Tune.Slower
            Tune.Denser
            Tune.Sparser
            Tune.Spinnier
            Tune.Steadier
            Tune.MoreArms
            Tune.FewerArms
            Tune.MoreAccel
            Tune.LessAccel
            Tune.Snappier
            Tune.Lazier
        ]

    static let pairs =
        [
            Tune.Faster, Tune.Slower
            Tune.Denser, Tune.Sparser
            Tune.Spinnier, Tune.Steadier
            Tune.MoreArms, Tune.FewerArms
            Tune.MoreAccel, Tune.LessAccel
            Tune.Snappier, Tune.Lazier
        ]

    /// 軸 の名 は 目盛り（html）と AI の返り の 両方 が引く。
    /// 1 つ でも 落ちる と、その軸 は 押しても 頼んでも 動かない
    [<Test>]
    member _.``軸 の名 が 10 通り 別 に読める``() =
        let names =
            [
                "speed"
                "density"
                "spin"
                "arms"
                "accel"
                "term"
                "split"
                "layer"
                "thin"
                "hush"
            ]

        let got = names |> List.map Tune.axis
        got |> List.filter Option.isNone |> should be Empty
        got |> List.distinct |> List.length |> should equal names.Length
        // 20 つまみ が 対 で 出そろう。余り も 重なり も 無い ——
        // `knobs` は 数 を振る 12 本 だけ（形 を触る 側 は 要素 が動く ので 別 の点 で見る）
        let structural =
            [
                Tune.AddSplit
                Tune.DropSplit
                Tune.AddLayer
                Tune.DropLayer
                Tune.Thinner
                Tune.Thicker
                Tune.Hush
                Tune.Unhush
            ]

        let used = got |> List.choose id |> List.collect (fun (u, d) -> [ u; d ])
        used |> List.sort |> should equal (List.sort (knobs @ structural))
        Tune.axis "loudness" |> should equal None

    /// 段数 を そのまま 当てる。形 を足す 側 が先 ——
    /// あと から 数 を振る と、足した 段 や 層 にも 同じ 倍率 が掛かる
    [<Test>]
    member _.``段数 を まとめて 当てる``() =
        let b = Bulletml.ReadXmlString(wrap plain)
        // 1 軸 1 段 は、その つまみ を 1 回 押した のと同じ
        xml (Tune.applySteps [ "speed", 1 ] b)
        |> should equal (xml (Tune.apply Tune.Faster b))

        xml (Tune.applySteps [ "speed", -1 ] b)
        |> should equal (xml (Tune.apply Tune.Slower b))
        // 0 は触らない。知らない 軸 も 飛ばす
        xml (Tune.applySteps [ "speed", 0; "loudness", 3 ] b) |> should equal (xml b)
        // 形 を先 に足す。逆 だと 足した 段 に 倍率 が掛からない
        let both = xml (Tune.applySteps [ "speed", 1; "split", 1 ] b)
        both |> should equal (xml (Tune.apply Tune.Faster (Tune.apply Tune.AddSplit b)))

        both
        |> should not' (equal (xml (Tune.apply Tune.AddSplit (Tune.apply Tune.Faster b))))

    /// 頼んだ 相手 が どんな 数 を返して も、当てる 数 は こちら が決める
    [<Test>]
    member _.``段数 は 上限 で止まる``() =
        let b = Bulletml.ReadXmlString(wrap plain)

        let rec press i (x: Bulletml) =
            if i = 0 then
                x
            else
                press (i - 1) (Tune.apply Tune.Faster x)

        xml (Tune.applySteps [ "speed", 99 ] b)
        |> should equal (xml (press Tune.MAX_STEPS b))

    [<Test>]
    member _.``速く する と 速度 に倍率 が掛かる``() =
        tune Tune.Faster plain |> should haveSubstring "(2.0 + $rank * 0.5) * 1.25"

    /// 密 にする のは 待ち を短く する こと。逆数 を掛けない と 逆 に効く
    [<Test>]
    member _.``密 にする と 待ち が短く なる``() =
        tune Tune.Denser plain |> should haveSubstring "(18 - 9 * $rank) * 0.8"

    [<Test>]
    member _.``回す と 刻み に倍率 が掛かる``() =
        tune Tune.Spinnier plain |> should haveSubstring "(13) * 1.25"

    /// 腕 を増やす と 本数 は増え、刻み は 逆 に細かく なる ——
    /// 同じ 向き に振る と リング が開いて 扇 になる
    [<Test>]
    member _.``腕 を増やす と 刻み は細かく なる``() =
        let got = tune Tune.MoreArms plain
        got |> should haveSubstring "(8) * 1.25"
        got |> should haveSubstring "(13) * 0.8"

    [<Test>]
    member _.``加速 に倍率 が掛かる``() =
        tune Tune.MoreAccel plain |> should haveSubstring "(0.5) * 1.25"

    /// 「変化 を速く」は 掛かる コマ数 を短く する こと
    [<Test>]
    member _.``変化 を速く する と コマ数 が減る``() =
        tune Tune.Snappier plain |> should haveSubstring "(10) * 0.8"

    /// `absolute` と `aim` は「どこ を向く か」。掛ける と 狙い が別 の方 を指す
    [<Test>]
    member _.``絶対角 と狙い は振らない``() =
        let src =
            """<action label="top">
           <fire><direction type="absolute">180</direction><bullet/></fire>
           <fire><direction type="aim">10</direction><bullet/></fire>
           <fire><direction>0</direction><bullet/></fire>
         </action>"""

        let got = tune Tune.Spinnier src
        got |> should haveSubstring ">180<"
        got |> should haveSubstring ">10<"
        got |> should not' (haveSubstring "1.25")

    /// つまみ ごと に 動く 欄 の組み合わせ が違う。全部 が同じ 欄 を触る
    /// 形 だと、12 個 の札 が 1 つ の つまみ になる ——
    /// しかも 押した 人 には 区別 が付かない
    [<Test>]
    member _.``つまみ ごと に動く 欄 が違う``() =
        let b = Bulletml.ReadXmlString(wrap plain)
        let slots = [ "speed"; "times"; "wait"; "direction"; "horizontal"; "term" ]

        let slot (s: string) (tag: string) =
            match System.Text.RegularExpressions.Regex.Match(s, "<" + tag + "[^>]*>([^<]*)</" + tag + ">") with
            | m when m.Success -> m.Groups.[1].Value
            | _ -> ""

        let before = xml b

        let moved k =
            let after = xml (Tune.apply k b)
            slots |> List.filter (fun t -> slot after t <> slot before t) |> Set.ofList

        let sets = knobs |> List.map moved
        // どの つまみ も 1 つ 以上 動かす
        sets |> List.filter Set.isEmpty |> should be Empty
        // 対 は 同じ 欄 を逆向き に振る ので、組み合わせ は 対 の数 だけ 出る。
        // 減る と、別 の札 が同じ 仕事 を して いる
        sets |> List.distinct |> List.length |> should equal pairs.Length

    /// 押し戻す と 元 の字 に戻る。積み重ねる 形 だと
    /// `((元) * 1.25) * 0.8` が残って、10 回 押した 先 で読めなくなる
    [<Test>]
    member _.``押し戻す と 元 の字 に戻る``() =
        let b = Bulletml.ReadXmlString(wrap plain)

        for up, down in pairs do
            xml (Tune.apply down (Tune.apply up b)) |> should equal (xml b)

    /// 2 段 上げる と 倍率 が 1 つ に畳まる
    [<Test>]
    member _.``2 段 上げて も 倍率 は 1 つ``() =
        let b = Bulletml.ReadXmlString(wrap plain)
        let got = xml (Tune.apply Tune.Faster (Tune.apply Tune.Faster b))
        got |> should haveSubstring "(2.0 + $rank * 0.5) * 1.5625"
        got |> should not' (haveSubstring ") * 1.25) *")

    /// 形 は動かさない。要素 の数 が変われば 別 の弾幕 になって いる
    [<Test>]
    member _.``形 は動かない``() =
        let count (needle: string) (s: string) =
            (s.Length - s.Replace(needle, "").Length) / needle.Length

        let b = Bulletml.ReadXmlString(wrap plain)
        let before = xml b

        for k in knobs do
            let after = xml (Tune.apply k b)

            for tag in [ "<fire>"; "<repeat>"; "<bullet"; "<action"; "<wait>" ] do
                count tag after |> should equal (count tag before)

    /// 振る 先 が 1 つ も無い 弾幕 で 字 を変えない。
    /// どの つまみ でも 何 か が変わる形 だと、押した 実感 が嘘 になる
    [<Test>]
    member _.``当てる 先 が無ければ 字 は変わらない``() =
        let src = """<action label="top"><fire><bullet/></fire></action>"""
        let b = Bulletml.ReadXmlString(wrap src)
        xml (Tune.apply Tune.Faster b) |> should equal (xml b)
        xml (Tune.apply Tune.Spinnier b) |> should equal (xml b)

    // --- 形 を足す 側 ---

    [<Test>]
    member _.``層 を足す と 並行 の action が増える``() =
        let count (needle: string) (s: string) =
            (s.Length - s.Replace(needle, "").Length) / needle.Length

        let b = Bulletml.ReadXmlString(wrap plain)
        let got = xml (Tune.apply Tune.AddLayer b)
        count "<action label=" got |> should equal (count "<action label=" (xml b) + 1)
        // 同じ コマ に出す と 重なって 1 層 に見える。ずらし が要る
        got |> should haveSubstring "<wait>20</wait>"

    /// 足した もの に 印 が付いて いる。印 が無い と、外す とき に
    /// 元々 在った 層 まで 削る
    [<Test>]
    member _.``足した 層 と 段 に 印 が付く``() =
        let b = Bulletml.ReadXmlString(wrap plain)
        xml (Tune.apply Tune.AddLayer b) |> should haveSubstring "tuned-top1"
        xml (Tune.apply Tune.AddSplit b) |> should haveSubstring Tune.SPLIT_MARK

    [<Test>]
    member _.``段 を足す と 終点 の弾 が割れる``() =
        let count (needle: string) (s: string) =
            (s.Length - s.Replace(needle, "").Length) / needle.Length

        let b = Bulletml.ReadXmlString(wrap plain)
        let got = xml (Tune.apply Tune.AddSplit b)
        count "<fire>" got |> should be (greaterThan (count "<fire>" (xml b)))
        got |> should haveSubstring "<vanish"

    /// 足して 外す と 元 の字 に戻る。印 を見ない で消す と、
    /// 元々 在った 要素 まで 削る
    [<Test>]
    member _.``足して 外す と 元 の字 に戻る``() =
        let b = Bulletml.ReadXmlString(wrap plain)

        for add, drop in [ Tune.AddLayer, Tune.DropLayer; Tune.AddSplit, Tune.DropSplit ] do
            xml (Tune.apply drop (Tune.apply add b)) |> should equal (xml b)

    /// 足して いない ものは 外さない。押した とき に 元 の弾幕 が削れる のを止める
    [<Test>]
    member _.``足して いない ときは 外さない``() =
        let b = Bulletml.ReadXmlString(wrap plain)
        xml (Tune.apply Tune.DropLayer b) |> should equal (xml b)
        xml (Tune.apply Tune.DropSplit b) |> should equal (xml b)

    /// 撃つ のが `<bulletRef>` の弾幕 —— 同梱 の多く が そう —— は、
    /// 終点 が 撃つ 場所 でなく 定義 の側 に在る。
    [<Test>]
    member _.``名前 で引く 弾 にも 段 を足せる``() =
        let src =
            """<action label="top"><repeat><times>8</times><action>
           <fire><direction type="sequence">13</direction><bulletRef label="core"/></fire>
           <wait>4</wait>
         </action></repeat></action>
         <bullet label="core"><speed>2</speed></bullet>"""

        let b = Bulletml.ReadXmlString(wrap src)
        let got = xml (Tune.apply Tune.AddSplit b)
        got |> should not' (equal (xml b))
        got |> should haveSubstring Tune.SPLIT_MARK
        // 足して 外す と 元 に戻る のも、この形 で見る
        xml (Tune.apply Tune.DropSplit (Tune.apply Tune.AddSplit b))
        |> should equal (xml b)

    /// 段 は 掛け算 で増える。青天井 だと 3 段 目 で 面 が止まる
    [<Test>]
    member _.``段 と 層 は 上限 で止まる``() =
        let b = Bulletml.ReadXmlString(wrap plain)

        let rec press knob n (x: Bulletml) =
            if n = 0 then x else press knob (n - 1) (Tune.apply knob x)

        for knob in [ Tune.AddSplit; Tune.AddLayer ] do
            let full = press knob 3 b
            xml (Tune.apply knob full) |> should equal (xml full)
