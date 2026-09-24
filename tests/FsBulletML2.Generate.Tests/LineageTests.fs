module FsBulletML2.Generate.Tests.LineageTests

open System
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

[<TestFixture>]
type LineageTests() =

    static let make f = LineageSpec.create f

    static let grid =
        [
            for root in [ Root.Radial; Root.Bar; Root.Fan ] do
                for seed in 0..9 do
                    for g in 1..3 do
                        for v in [ 0.0; 1.0; 2.0 ] ->
                            make (fun a ->
                                { a with
                                    Root = root
                                    Seed = seed
                                    Generations = g
                                    Streak = v
                                    Stillness = v
                                    Spread = v
                                    Speed = v
                                    Fall = v
                                })
        ]

    static let xml (s: LineageSpec) =
        BulletmlWriter.toIndentedXml 2 (Lineage.generate s).Bulletml

    static let all81 =
        [
            for seed in 1..9 do
                for leaf in [ 0; 1; 2 ] do
                    for root in [ Root.Radial; Root.Bar; Root.Fan ] ->
                        make (fun a ->
                            { a with
                                Root = root
                                Seed = seed
                                Spread = 1.0
                                Streak = 1.0
                                Speed = 1.0
                                Stillness = (if leaf = 1 then 1.0 else 0.0)
                                Homing = (if leaf = 1 then 1.0 else 0.0)
                                Fall = (if leaf = 2 then 2.0 else 0.0)
                                Strands = 2.0
                                Drift = 2.0
                                Alternate = true
                            })
        ]

    static let runs frames (s: LineageSpec) =
        Felt.run frames (Lineage.generate s).Bulletml

    static let births (snaps: Felt.Snapshot list) =
        snaps |> List.sumBy (fun s -> s.Headings.Length)

    /// 生まれて 1 コマ 目 の 弾 の 位置
    static let newborn (s: Felt.Snapshot) =
        List.zip s.Positions s.Born
        |> List.filter (fun (_, b) -> b = s.Frame - 1)
        |> List.map fst

    /// 同じ コマ に 生まれた 群 の、続く 2 コマ の 平均 移動 量。並び は 生き残り の 順 で 保たれる
    static let groupSpeed (snaps: Felt.Snapshot list) (born: int) (frame: int) =
        let at f =
            let s = snaps.[f - 1]

            List.zip s.Positions s.Born
            |> List.filter (fun (_, b) -> b = born)
            |> List.map fst

        let a, b = at frame, at (frame + 1)

        if a.Length = 0 || a.Length <> b.Length then
            nan
        else
            List.zip a b
            |> List.averageBy (fun ((x0, y0), (x1, y1)) -> sqrt ((x1 - x0) ** 2.0 + (y1 - y0) ** 2.0))

    /// Trail の 子 を 生む コマ だけ を 並べる。根 の 1 コマ目 は 除く
    static let trailFrames (snaps: Felt.Snapshot list) (perPair: int) =
        snaps |> List.skip 1 |> List.filter (fun x -> x.Headings.Length = perPair)

    static let wrapDeg (r: float) =
        let d = r * 180.0 / Math.PI % 360.0

        if d > 180.0 then d - 360.0
        elif d < -180.0 then d + 360.0
        else d

    [<Test>]
    member _.``目盛り の 外 を 切り詰める``() =
        let s =
            make (fun a ->
                { a with
                    Generations = 9
                    Streak = 5.0
                    Ways = 99
                    Speed = 9.0
                })

        s.Chain.Length |> should equal 3
        s.Ways |> should equal 24
        s.TrailWait.Base |> should equal 4

        (s.RootSpeed * 1.3)
        |> should be (lessThanOrEqualTo (float Consts.MAX_SPEED + 1e-9))

        (make (fun a -> { a with Generations = 0 })).Chain.Length |> should equal 1

    [<Test>]
    member _.``本数 0 は 根 ごと の 既定``() =
        (make (fun a -> { a with Root = Root.Radial })).Ways |> should equal 12
        (make (fun a -> { a with Root = Root.Bar })).Ways |> should equal 2

    [<Test>]
    member _.``扇 の 本数 0 は 2``() =
        (make (fun a -> { a with Root = Root.Fan })).Ways |> should equal 2

    [<Test>]
    member _.``終わり は 落ち が 先``() =
        let leaf fall still =
            (make (fun a ->
                { a with
                    Fall = fall
                    Stillness = still
                }))
                .Leaf

        leaf 1.0 1.0 |> should equal Terminal.Fall
        leaf 0.9 1.0 |> should equal Terminal.Relaunch
        leaf 0.9 0.9 |> should equal Terminal.Plain

    [<Test>]
    member _.``振り と 撚り と 落ち の 目盛り``() =
        let s v =
            make (fun a ->
                { a with
                    Drift = v
                    Strands = v
                    Fall = v
                })

        [ for v in [ 0.0; 1.0; 2.0 ] -> (s v).Sweep ]
        |> should equal [ 0.0; 48.0; 96.0 ]

        [ for v in [ 0.0; 1.0; 2.0 ] -> (s v).Strands ] |> should equal [ 1; 2; 3 ]
        [ for v in [ 1.0; 2.0 ] -> (s v).Gravity ] |> should equal [ 2.1; 4.2 ]
        LineageSpec.strandMul 3 |> should equal [ 0.5; 1.0; 1.5 ]

        (LineageSpec.strandMul 2 |> List.map (fun m -> Math.Round(m, 4)))
        |> should equal [ 0.6667; 1.3333 ]

    /// v 1.3、G 4.2：tr = 120 x 1.3 / 5.5 = 28.36、上がる 距離 18.4、H = (64 - 18.4) / 1.3 = 35.07
    [<Test>]
    member _.``落ち の 待ち は 上端 を 越えない 長さ``() =
        LineageSpec.fallHoldOf 1.3 4.2 |> should equal 35
        LineageSpec.fallHoldOf 0.1 4.2 |> should equal 45
        LineageSpec.fallHoldOf 9.0 0.1 |> should equal 0

        for s in grid do
            s.FallHold |> should be (inRange 0 45)

    [<Test>]
    member _.``Trail は 1 回 まで``() =
        for s in grid do
            s.Chain
            |> List.filter ((=) Spawner.Trail)
            |> List.length
            |> should be (lessThanOrEqualTo 1)

    [<Test>]
    member _.``種 は 9 通り の 表 を 引く``() =
        let chainOf seed =
            (make (fun a -> { a with Seed = seed })).Chain

        let t, b = Spawner.Trail, Spawner.Burst

        [ for s in 1..9 -> chainOf s ]
        |> should
            equal
            [
                [ t ]
                [ b ]
                [ t; b ]
                [ b; t ]
                [ b; b ]
                [ t; b; b ]
                [ b; t; b ]
                [ b; b; t ]
                [ b; b; b ]
            ]

        chainOf 10 |> should equal (chainOf 1)
        chainOf 18 |> should equal (chainOf 9)

    [<Test>]
    member _.``種 0 は 軸 から 組む``() =
        (make (fun a -> { a with Generations = 3; Streak = 1.0 })).Chain
        |> should equal [ Spawner.Trail; Spawner.Burst; Spawner.Burst ]

        (make (fun a -> { a with Generations = 2; Streak = 0.9 })).Chain
        |> should equal [ Spawner.Burst; Spawner.Burst ]

    [<Test>]
    member _.``止まる 感じ 1 以上 で 終わり は Relaunch``() =
        (make (fun a -> { a with Stillness = 0.9 })).Leaf |> should equal Terminal.Plain

        (make (fun a -> { a with Stillness = 1.0 })).Leaf
        |> should equal Terminal.Relaunch

    [<Test>]
    member _.``待ち は 難度 1 でも 1 コマ 以上``() =
        for s in grid do
            for w in [ s.WaveWait; s.TrailWait; s.RelaunchHold ] do
                (w.Base - w.Rank) |> should be (greaterThanOrEqualTo 1)

    [<Test>]
    member _.``広がり 1.5 以上 で 2 列``() =
        (make (fun a -> { a with Spread = 1.4 })).Columns |> should equal 1
        (make (fun a -> { a with Spread = 1.5 })).Columns |> should equal 2

    [<Test>]
    member _.``世代 は ラベル で 繋がる``() =
        let s = make (fun a -> { a with Seed = 6; Stillness = 1.0 }) // T → B → B
        let x = xml s

        for l in [ "g1"; "g2"; "g3"; "leaf" ] do
            x |> should haveSubstring (sprintf "label=\"%s\"" l)

        x |> should not' (haveSubstring "label=\"g4\"")

    [<Test>]
    member _.``定義 に 速さ を 書かない``() =
        for s in all81 do
            match (Lineage.generate s).Bulletml with
            | Bulletml(_, elms) ->
                for e in elms do
                    match e with
                    | BulletmlElm.Bullet(attrs, _, spd, _) when attrs.bulletLabel <> Some(BulletLabel "bar") ->
                        spd.IsNone |> should equal true
                    | _ -> ()

    [<Test>]
    member _.``Trail と Relaunch は 台本 を 終わらせない``() =
        let x = xml (make (fun a -> { a with Seed = 1; Stillness = 1.0 })) // T → Relaunch
        // Trail の 後 と Relaunch の 後 に 1 つ ずつ
        (x.Split("<wait>9999</wait>").Length - 1) |> should equal 2

    [<Test>]
    member _.``止める 速さ は 0 に しない``() =
        let x = xml (make (fun a -> { a with Seed = 2; Stillness = 1.0 })) // B → Relaunch
        x |> should haveSubstring "0.0001"
        x |> should not' (haveSubstring "<speed>0</speed>")

    [<Test>]
    member _.``難度 が 間隔 と 発数 に 入る``() =
        let s =
            make (fun a ->
                { a with
                    Seed = 3
                    Streak = 1.0
                    Spread = 1.0
                }) // T → B

        let x = xml s

        x
        |> should haveSubstring (sprintf "%d - %d * $rank" s.TrailWait.Base s.TrailWait.Rank)

        x |> should haveSubstring (sprintf "%d + %d * $rank" s.Line.Base s.Line.Rank)

    /// 字 の 大きさ の 最大。参照 で 繋ぐ ので 世代数 に 比例 する
    [<Test>]
    member _.``字 は 数 KB に 収まる``() =
        let biggest = all81 |> List.map (fun s -> (xml s).Length) |> List.max
        biggest |> should be (lessThan 8000)

    /// 止めた 弾 から `relative 90` で 撃った 列 が、親 の 向き と 直角 か。
    /// 速さ 0.0001 で 向き が 残る か の 答え
    [<Test>]
    member _.``止めた 弾 の 横撃ち は 直角``() =
        let s = make (fun a -> { a with Ways = 1; Seed = 2 }) // B → Plain
        let snaps = runs 240 s
        let parent = snaps.[0].Headings |> List.exactlyOne
        let burst = snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)

        for h in burst.Headings do
            abs (sin (h - parent)) |> should be (greaterThan 0.99)

    [<Test>]
    member _.``Trail は 通り道 に 撒く``() =
        let s = make (fun a -> { a with Ways = 1; Seed = 1 }) // T → Plain
        let snaps = runs 120 s

        let spots =
            snaps
            |> List.map newborn
            |> List.filter (fun p -> p.Length > 0)
            |> List.map (fun p -> List.averageBy fst p, List.averageBy snd p)

        spots.Length |> should be (greaterThanOrEqualTo 5)
        let (x0, y0), (x1, y1) = List.head spots, List.last spots
        sqrt ((x1 - x0) ** 2.0 + (y1 - y0) ** 2.0) |> should be (greaterThan 50.0)

    [<Test>]
    member _.``Relaunch は 止まって から 動く``() =
        let s =
            make (fun a ->
                { a with
                    Ways = 1
                    Seed = 2
                    Stillness = 1.0
                }) // B → Relaunch

        let snaps = runs 300 s

        let born =
            (snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)).Frame

        let hold = s.RelaunchHold.Base - s.RelaunchHold.Rank // $rank = 1
        let fly = LineageSpec.flyOf s 1
        groupSpeed snaps born (born + 5) |> should be (greaterThan 0.5)
        groupSpeed snaps born (born + fly + hold / 2) |> should be (lessThan 0.05)
        groupSpeed snaps born (born + fly + hold + 50) |> should be (greaterThan 0.5)

    /// 1 組目 は 親 と 直角、1 組 ごと に 掃く 角 / (T - 1) 回る。2 波目 の 組 が 混ざる ので、2 波目 より 前 だけ を 見る。
    /// 親 は 扇 の 120° へ 飛ばす。真下 へ 飛ぶ と 起点 0 の sequence 90 も 直角 に なり、直角 の 確かめ が 何 も 見ない
    [<Test>]
    member _.``振り は 1 本 の 間 に 掃く 角 だけ 回る``() =
        let s =
            make (fun a ->
                { a with
                    Root = Root.Fan
                    Ways = 2
                    Seed = 1
                    Drift = 2.0
                }) // T → Plain

        let snaps = runs 400 s
        let parent = snaps.[0].Headings.[0]
        let wave2 = 1 + s.WaveWait.Base - s.WaveWait.Rank

        let pairs =
            trailFrames snaps 4
            |> List.filter (fun x -> x.Frame < wave2)
            |> List.truncate s.TrailTimes

        pairs.Length |> should be (greaterThanOrEqualTo 5)
        let first, last = pairs.Head.Headings.[0], (List.last pairs).Headings.[0]
        abs (sin (first - parent)) |> should be (greaterThan 0.99)
        let d = 96.0 / float (s.TrailTimes - 1)

        wrapDeg (last - first)
        |> should (equalWithin 0.5) (d * float (pairs.Length - 1))

    [<Test>]
    member _.``撚り は 同じ 向き に 速さ 違い を 重ねる``() =
        let s =
            make (fun a ->
                { a with
                    Ways = 1
                    Seed = 1
                    Strands = 2.0
                }) // T → Plain、3 本

        let pair = trailFrames (runs 120 s) 6 |> List.head
        // 難度 1 の 速さ は v x 1.3、v は 1.0 x 倍率
        (pair.Speeds
         |> List.map (fun v -> Math.Round(v, 2))
         |> List.distinct
         |> List.sort)
        |> should equal [ 0.65; 1.3; 1.95 ]

        (pair.Headings |> List.map (fun h -> Math.Round(h, 3)) |> List.distinct).Length
        |> should equal 2

    /// 放射 1 本 は 自機（真下）へ、子 は 真横 へ 撒く。落ち の 葉 は 待って から 下 へ 動く
    [<Test>]
    member _.``落ち の 葉 は 待って から 下 へ 引かれる``() =
        let drop fall =
            let s =
                make (fun a ->
                    { a with
                        Ways = 1
                        Seed = 1
                        Fall = fall
                    })

            let snaps = runs 400 s
            // コマ F に 撃った 弾 は Born = F で、次 の コマ から Positions に 出る
            let born = (trailFrames snaps 2 |> List.head).Frame

            let meanY f =
                let x = snaps.[f - 1]

                List.zip x.Positions x.Born
                |> List.filter (fun (_, b) -> b = born)
                |> List.averageBy (fst >> snd)

            meanY (born + 1 + s.FallHold + 100) - meanY (born + 1 + s.FallHold)

        drop 2.0 |> should be (greaterThan 30.0)
        abs (drop 0.0) |> should be (lessThan 5.0)

    [<Test>]
    member _.``撚り と 落ち は 上界 に 入る``() =
        let bound f =
            LineageSpec.aliveBound (make (fun a -> f { a with Seed = 1; Ways = 1 }))

        bound (fun a -> { a with Strands = 2.0 }) |> should be (greaterThan (bound id))
        bound (fun a -> { a with Fall = 1.0 }) |> should be (greaterThan (bound id))

    /// 扇 2 本 は 自機（真下 180°）から ±60° ＝ 120° と 240°
    [<Test>]
    member _.``扇 は 自機 の 向き から 幅 120°``() =
        let heads ways =
            let s =
                make (fun a ->
                    { a with
                        Root = Root.Fan
                        Ways = ways
                        Seed = 2
                    })

            (runs 2 s).[0].Headings
            |> List.map (fun h -> Math.Round(h * 180.0 / Math.PI))
            |> List.sort

        heads 2 |> should equal [ 120.0; 240.0 ]
        heads 3 |> should equal [ 120.0; 180.0; 240.0 ]
        heads 1 |> should equal [ 180.0 ]

    /// 放射 2 本 は 真横。入れ替え が あれば 左右 の 糸 は 敵 の 縦 の 線 を 挟んで 鏡写し
    [<Test>]
    member _.``入れ替え は 左右 を 鏡写し に する``() =
        let mirrored alt =
            let s =
                make (fun a ->
                    { a with
                        Ways = 2
                        Seed = 1
                        Drift = 2.0
                        Alternate = alt
                    })

            let snap = (runs 150 s).[149]
            let ps = snap.Positions

            let hit (x, y) =
                ps
                |> List.exists (fun (x2, y2) -> abs (x2 - (2.0 * float Felt.EnemyX - x)) < 1.0 && abs (y2 - y) < 1.0)

            float (ps |> List.filter hit |> List.length) / float ps.Length

        mirrored true |> should be (greaterThan 0.95)
        mirrored false |> should be (lessThan 0.5)
        // 振り が 回って いない と、入れ替え が 無くて も 鏡写し に なる。$1 が 届かず 0 の とき が それ
        let s =
            make (fun a ->
                { a with
                    Ways = 2
                    Seed = 1
                    Drift = 2.0
                    Alternate = true
                })

        let heads =
            trailFrames (runs 60 s) 4
            |> List.map (fun x -> Math.Round(x.Headings.[0], 3))
            |> List.distinct

        heads.Length |> should be (greaterThan 3)

    [<Test>]
    member _.``入れ替え の 波 は 2 波 で 1 組``() =
        let x =
            xml (
                make (fun a ->
                    { a with
                        Ways = 2
                        Seed = 1
                        Alternate = true
                    })
            )

        x |> should haveSubstring "(1 + 1 * $rank + 1) / 2"
        x |> should haveSubstring "<param>1</param>"
        x |> should haveSubstring "<param>-1</param>"
        // 難度 0 でも 2 波（1 周 に 1 波 増える）
        let s =
            make (fun a ->
                { a with
                    Ways = 2
                    Seed = 2
                    Alternate = true
                })

        let roots =
            Felt.runAt 0.0f (1 + 2 * s.WaveWait.Base) (Lineage.generate s).Bulletml
            |> List.filter (fun x -> x.Headings.Length = 2)

        roots.Length |> should be (greaterThanOrEqualTo 2)

    [<Test>]
    member _.``入れ替え が 無ければ 引数 を 渡さない``() =
        for s in grid do
            if not s.Alternate then
                (xml s) |> should not' (haveSubstring "<param>")

    [<Test>]
    member _.``難度 1 は 難度 0 より 多く 生む``() =
        for seed in [ 1; 2 ] do
            for root in [ Root.Radial; Root.Bar ] do
                let s =
                    make (fun a ->
                        { a with
                            Root = root
                            Seed = seed
                            Spread = 1.0
                        })

                let b = (Lineage.generate s).Bulletml

                births (Felt.runAt 1.0f 400 b)
                |> should be (greaterThan (births (Felt.runAt 0.0f 400 b)))

    [<Test>]
    member _.``Burst の 一列 は 難度 で 伸びる``() =
        let s =
            make (fun a ->
                { a with
                    Ways = 1
                    Seed = 2
                    Spread = 0.0
                }) // B → Plain、1 列

        let column rank =
            let snaps = Felt.runAt rank 240 (Lineage.generate s).Bulletml
            (snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)).Headings.Length

        column 0.0f |> should equal (1 + s.Line.Base)
        column 1.0f |> should equal (1 + s.Line.Base + s.Line.Rank)

    /// 赤 の とき は 並び と 見積もり を 報告 に 書く。`BUDGET` を 黙って 上げない —— `shrink` の 順 が 足りない 証拠
    [<Test>]
    member _.``合計 は 削った 先 に 収まる``() =
        for s in all81 do
            LineageSpec.aliveBound s |> should be (lessThanOrEqualTo LineageSpec.BUDGET)

    [<Test>]
    member _.``Trail は 画面 を 横切る 時間 で 止める``() =
        // 根 の 速さ 1.0、間隔 8：280 / 1.0 / 8 = 35。本数 12 だと 上界 が 削る 先 を 越えて `fit` が 半分 に する ので 4 本
        let s =
            make (fun a ->
                { a with
                    Seed = 1
                    Ways = 4
                    Speed = 0.0
                    Streak = 0.0
                })

        s.TrailTimes |> should equal 35

    /// 81 通り（並び 9 × 終わり 3 × 根 3、撚り 3 本・振り 96°・入れ替え あり）を 難度 1 で 700 コマ 走らせた 最大数
    [<Test>]
    member _.``81 通り は 面 の 天井 に 収まる``() =
        for s in all81 do
            let peak = runs 700 s |> List.map (fun x -> x.Positions.Length) |> List.max
            peak |> should be (lessThanOrEqualTo 10000)
            float peak |> should be (lessThanOrEqualTo (LineageSpec.aliveBound s))

    [<Test>]
    member _.``狙う 弾 は 軸 が 1 以上 の とき だけ``() =
        for seed in [ 1; 9 ] do // 最後 の 遺伝子 が Trail / Burst
            let x h =
                xml (make (fun a -> { a with Seed = seed; Homing = h }))

            x 0.0 |> should not' (haveSubstring "label=\"seeker\"")
            // 定義 が 在る だけ では 撃って いない ことが ある。撃つ 側 の 参照 を 見る
            x 1.0 |> should haveSubstring "<bulletRef label=\"seeker\""
            x 1.0 |> should haveSubstring "<changeDirection>"

    /// 自機 の まわり 48 px を 通った 弾 の 延べ 数。固まった 一列 から 狙う 弾 が ばらけて 来る か
    [<Test>]
    member _.``狙う 弾 は 自機 に 届く``() =
        let near h =
            let s = make (fun a -> { a with Seed = 9; Homing = h })

            runs 610 s
            |> List.sumBy (fun x ->
                x.Positions
                |> List.filter (fun (px, py) ->
                    sqrt ((px - float Felt.PlayerX) ** 2.0 + (py - float Felt.PlayerY) ** 2.0) < 48.0)
                |> List.length)

        let off, on = near 0.0, near 1.0
        on |> should be (greaterThan (off * 2 + 20))

/// 走らせ 直し の 罠 だけ を 見る。壊す と `LineageTests` の 81 通り が 止まらなく なる ので、別 の 型 に 置いて 型 名 で 絞る
[<TestFixture>]
type LineageCycleTests() =

    /// 速さ 0.0001 で 撃つ のは 発射台 だけ。`top` が 待たない と 毎コマ 出る
    [<Test>]
    member _.``Bar の 発射台 は 周 に 1 度``() =
        let s = LineageSpec.create (fun a -> { a with Root = Root.Bar; Seed = 2 })

        let pads =
            Felt.run 30 (Lineage.generate s).Bulletml
            |> List.map (fun x -> x.Speeds |> List.filter (fun v -> v < 0.001) |> List.length)

        pads |> should equal (s.Ways :: List.replicate 29 0)

    /// Bar の 入れ替え は 2 周 を 1 つ の top に 書く。2 周目 も 発射台 は 腕 の 数 だけ
    [<Test>]
    member _.``Bar の 入れ替え でも 発射台 は 周 に 1 度``() =
        let s =
            LineageSpec.create (fun a ->
                { a with
                    Root = Root.Bar
                    Seed = 2
                    Alternate = true
                })

        let c =
            s.BarSteps.Base * LineageSpec.BAR_WAIT
            + s.WaveWait.Base
            + (s.BarSteps.Rank * LineageSpec.BAR_WAIT - s.WaveWait.Rank)

        let pads =
            Felt.run (c + 5) (Lineage.generate s).Bulletml
            |> List.map (fun x -> x.Speeds |> List.filter (fun v -> v < 0.001) |> List.length)

        pads |> List.sum |> should equal (2 * s.Ways)
