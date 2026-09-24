namespace FsBulletML2.Generate.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 種 18 通り が「ダメ」に 落ちて いない こと。美しい は 測れない ので、弾く のは ダメ の側 だけ
[<TestFixture>]
type HarmonicSpace() =

    /// 速さ と 深さ。床 と 天井 は 速くて 深い 札 で 初めて 当たる 材料 が 出る
    static let FEEL = [ 2.0, 1.2; 3.0, 2.0; 1.0, 2.0; 0.0, 1.2 ]

    static let specOf (spd: float) (amp: float) (seed: int) =
        HarmonicSpec.create (fun a ->
            { a with
                Folds = 5
                Speed = spd
                Amplitude = amp
                Seed = seed
            })

    /// 撃った コマ の 向き と 速さ で 測る。Positions は 上 に飛んだ 弾 が 80 px で 間引かれる
    static let waveOf (h: HarmonicSpec) =
        Harmonic.generate h |> fun i -> Felt.run 90 i.Bulletml |> Felt.Draw.fullest

    /// 向き ごと に いちばん 速い 弾 だけ 残した 外周。層 を 重ねた まま 正弦 を 当てる と、
    /// 半周期 ずらした 層 が 第 1 の波 を 打ち消し合い 0 に なる
    static let envelope (s: Felt.Snapshot) : Felt.Snapshot =
        let hs, vs =
            List.zip s.Headings s.Speeds
            |> List.groupBy (fun (th, _) -> System.Math.Round(th, 4))
            |> List.map (fun (_, g) -> g |> List.maxBy snd)
            |> List.unzip

        { s with Headings = hs; Speeds = vs }

    /// 角 の順 に並べた 速さ の 山 の数。輪 なので 頭 と尻 を繋ぐ
    static let peaks (s: Felt.Snapshot) =
        let v =
            List.zip s.Headings s.Speeds |> List.sortBy fst |> List.map snd |> Array.ofList

        let n = v.Length

        [ 0 .. n - 1 ]
        |> List.filter (fun i -> v.[i] > v.[(i + n - 1) % n] && v.[i] >= v.[(i + 1) % n])
        |> List.length

    static let ratio (s: Felt.Snapshot) = List.max s.Speeds / List.min s.Speeds

    /// 外周 に 当てる 回数 は 第 1 の波・第 2 の波・層 の 数 倍 の うち 最大
    static let regularity (h: HarmonicSpec) (s: Felt.Snapshot) =
        let e = envelope s

        [ h.Folds; h.Folds2; h.Folds * h.Layers; h.Folds2 * h.Layers ]
        |> List.filter (fun k -> k > 0)
        |> List.map (fun k -> Felt.fitScore k id e)
        |> List.max

    /// Felt は $rank = 1 で走る（Felt.fs の runWith）ので、返る 速さ は 基準値 の 1.3 倍。
    /// 基準値 の まま 書く と 床 は 1 発 も 当たらず、天井 は 健全 な 弾 まで 拾う
    static let FLOOR = 0.3 * 1.3
    static let CEIL = float Consts.MAX_SPEED

    static let share (pred: float -> bool) (s: Felt.Snapshot) =
        float (s.Speeds |> List.filter pred |> List.length) / float s.Speeds.Length

    [<Test>]
    [<TestCase(1)>]
    [<TestCase(2)>]
    [<TestCase(3)>]
    [<TestCase(4)>]
    [<TestCase(5)>]
    [<TestCase(6)>]
    [<TestCase(7)>]
    [<TestCase(8)>]
    [<TestCase(9)>]
    [<TestCase(10)>]
    [<TestCase(11)>]
    [<TestCase(12)>]
    [<TestCase(13)>]
    [<TestCase(14)>]
    [<TestCase(15)>]
    [<TestCase(16)>]
    [<TestCase(17)>]
    [<TestCase(18)>]
    member _.``種 が ダメ に 落ちて いない``(seed: int) =
        for spd, amp in FEEL do
            let h = specOf spd amp seed
            let w = waveOf h
            // 潰れ を 先 に 見る。形 を 先 に 見る と そちら が 先 に 落ちて 床 と 天井 の 行 に 届かない
            if h.Folds2 > 0 then
                float h.Arms / float h.Folds2 |> should be (greaterThanOrEqualTo 4.0)

            share (fun v -> v <= FLOOR + 0.01) w |> should equal 0.0
            share (fun v -> v >= CEIL - 0.01) w |> should equal 0.0
            peaks (envelope w) |> should be (greaterThanOrEqualTo 2)
            ratio w |> should be (greaterThanOrEqualTo 1.3)
            ratio w |> should be (lessThanOrEqualTo 6.0)
            regularity h w |> should be (greaterThanOrEqualTo 0.55)

    [<Test>]
    member _.``札 x 枚数 x 種 の 字 と 上界 が 予算 に 収まる``() =
        // 畳めない 形 で 層 を 重ねる と 字 が 層 の 数 だけ 膨らむ
        for fig in [ Petal; Star; Heart ] do
            for k in [ 3; 5; 7; 8 ] do
                for seed in 1..18 do
                    let h =
                        HarmonicSpec.create (fun a ->
                            { a with
                                Figure = fig
                                Folds = k
                                Speed = 2.0
                                Amplitude = 1.2
                                Seed = seed
                            })

                    (BulletmlWriter.toIndentedXml 2 (Harmonic.generate h).Bulletml).Length
                    |> should be (lessThan 40000)

                    Harmonic.aliveBound (Harmonic.fit h)
                    |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE))

    [<Test>]
    member _.``重い 種 でも 同時 に MAX_ALIVE 以下``() =
        // 上界 は 見積もり。走らせて 数えた 同時数 も 見る —— 重ね 3 枚 と 入れ子 3 枚 が いちばん 重い
        for seed in [ 10; 13 ] do
            let alive =
                Harmonic.generate (specOf 3.0 2.0 seed)
                |> fun i -> Felt.run 400 i.Bulletml
                |> List.map (fun s -> s.Positions.Length)
                |> List.max

            alive |> should be (greaterThan 0)
            alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)
