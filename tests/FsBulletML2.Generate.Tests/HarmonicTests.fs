module FsBulletML2.Generate.Tests.HarmonicTests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

[<TestFixture>]
type HarmonicTests() =

    static let flower k =
        HarmonicSpec.create (fun a ->
            { a with
                Folds = k
                Speed = 1.0
                Amplitude = 2.0
                Arms = 24
                Vanishing = true
            })

    static let figure fig k amp spd =
        HarmonicSpec.create (fun a ->
            { a with
                Figure = fig
                Folds = k
                Speed = spd
                Amplitude = amp
                Arms = 24
                Vanishing = true
            })

    /// 輪郭 の 尖り を見る 側。24 発 では 谷 と 山 の あいだ が 3 発 しか 無く、刻み の粗さ に 埋もれる
    static let wide fig k amp spd =
        HarmonicSpec.create (fun a ->
            { a with
                Figure = fig
                Folds = k
                Speed = spd
                Amplitude = amp
                Arms = 96
                Vanishing = true
            })

    static let runOf frames (h: HarmonicSpec) =
        Felt.run frames (Harmonic.generate h).Bulletml

    /// いちばん 多く 撃った コマ
    static let fullest (snaps: Felt.Snapshot list) =
        snaps |> List.maxBy (fun s -> s.Speeds.Length)

    /// 外 と内 の比。速さ の比 が そのまま 同じ コマ の半径 の比 になる
    static let ratio (s: Felt.Snapshot) = List.max s.Speeds / List.min s.Speeds

    /// 輪郭 を 角 の順 に並べ、続く 3 点 の 真ん中 が 両隣 を結ぶ 線 から 離れる 距離。
    /// 山 の半径 で割った 値 の 中央値 —— 辺 が 直線 なら 0 に近い
    static let bend (s: Felt.Snapshot) =
        let pts =
            List.zip s.Headings s.Speeds
            |> List.sortBy fst
            |> List.map (fun (h, r) -> r * sin h, -(r * cos h))
            |> Array.ofList

        let n = pts.Length
        let hi = List.max s.Speeds

        let d i =
            let (ax, ay) = pts.[(i + n - 1) % n]
            let (bx, by) = pts.[i]
            let (cx, cy) = pts.[(i + 1) % n]
            let ux, uy = cx - ax, cy - ay
            let len = sqrt (ux * ux + uy * uy)

            if len < 1e-9 then
                0.0
            else
                abs ((bx - ax) * uy - (by - ay) * ux) / len / hi

        let all = [ 0 .. n - 1 ] |> List.map d |> List.sort
        all.[n / 2]

    /// 角 の順 に並べた 速さ の 山 の数。輪 なので 頭 と尻 を繋ぐ
    static let peaks (s: Felt.Snapshot) =
        let v =
            List.zip s.Headings s.Speeds |> List.sortBy fst |> List.map snd |> Array.ofList

        let n = v.Length

        [ 0 .. n - 1 ]
        |> List.filter (fun i -> v.[i] > v.[(i + n - 1) % n] && v.[i] >= v.[(i + 1) % n])
        |> List.length

    /// 種 を撒いて 咲かせた 弾幕。`Blooms` と `Scatter` は 対 で使わない と 弾数 か 密度 の どちら か が壊れる
    static let blooms n k =
        let h =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = k
                    Speed = 1.0
                    Amplitude = 1.5
                    Blooms = n
                })

        h, Scatter.apply n (Harmonic.generate h).Bulletml

    /// いちばん 多く 生まれた コマ。種 が咲いた コマ が そこ に来る
    static let bloomFrame (snaps: Felt.Snapshot list) =
        snaps
        |> List.maxBy (fun s -> s.Born |> List.filter (fun b -> b = s.Frame - 1) |> List.length)

    /// 5 枚 の 花 に 軸 を 足す 側
    static let petal5 (f: HarmonicAxes -> HarmonicAxes) =
        HarmonicSpec.create (fun a ->
            f
                { a with
                    Folds = 5
                    Speed = 2.0
                    Amplitude = 1.2
                })

    [<Test>]
    member _.``Folds の外 は 5 に倒す``() =
        (HarmonicSpec.create (fun a -> { a with Folds = 2 })).Folds |> should equal 5
        (HarmonicSpec.create (fun a -> { a with Folds = 6 })).Folds |> should equal 5
        (HarmonicSpec.create (fun a -> { a with Folds = 5 })).Folds |> should equal 5
        (HarmonicSpec.create (fun a -> { a with Folds = 3 })).Folds |> should equal 3
        (HarmonicSpec.create (fun a -> { a with Folds = 7 })).Folds |> should equal 7
        (HarmonicSpec.create (fun a -> { a with Folds = 8 })).Folds |> should equal 8

    [<Test>]
    member _.``目盛り の外 を閉じる``() =
        let lo =
            HarmonicSpec.create (fun a ->
                { a with
                    Speed = -3.0
                    Amplitude = -1.0
                    Spin = -1.0
                    Density = -2.0
                    Arms = 3
                    Phase = -1.0
                })

        let hi =
            HarmonicSpec.create (fun a ->
                { a with
                    Speed = 99.0
                    Amplitude = 9.0
                    Spin = 9.0
                    Density = 99.0
                    Arms = 999
                    Phase = 7.0
                })

        (lo.Speed, lo.Amplitude, lo.Spin, lo.Density, lo.Arms)
        |> should equal (0.0, 0.0, 0.0, 0.0, 8)

        (hi.Speed, hi.Amplitude, hi.Spin, hi.Density, hi.Arms)
        |> should equal (3.0, 2.0, 2.0, 3.0, 160)

        lo.Phase |> should (equalWithin 1e-9) (2.0 * System.Math.PI - 1.0)
        hi.Phase |> should (equalWithin 1e-9) (7.0 - 2.0 * System.Math.PI)
        // 名指し が無ければ 花弁 x 24。8 枚 は 192 で 上限 に当たる
        (HarmonicSpec.create id).Arms |> should equal 120
        (HarmonicSpec.create (fun a -> { a with Folds = 3 })).Arms |> should equal 72
        (HarmonicSpec.create (fun a -> { a with Folds = 8 })).Arms |> should equal 160
        // 咲かせる 数 は 1..8。腕 は その数 で割られ、名指し して も 頭打ち になる
        (HarmonicSpec.create (fun a -> { a with Blooms = 0 })).Blooms |> should equal 1
        (HarmonicSpec.create (fun a -> { a with Blooms = 99 })).Blooms |> should equal 8

        (HarmonicSpec.create (fun a -> { a with Folds = 5; Blooms = 3 })).Arms
        |> should equal 40

        (HarmonicSpec.create (fun a -> { a with Folds = 5; Blooms = 8 })).Arms
        |> should equal 16

        (HarmonicSpec.create (fun a -> { a with Arms = 999; Blooms = 8 })).Arms
        |> should equal 20

    [<Test>]
    member _.``k=5 は 5 回 対称 で、k=3 より foldScore 5 が高い``() =
        let s5 = fullest (runOf 90 (flower 5))
        let s3 = fullest (runOf 90 (flower 3))
        Felt.foldScore 5 s5 |> should be (greaterThan 0.5)
        Felt.foldScore 5 s5 |> should be (greaterThan (Felt.foldScore 5 s3))
        Felt.foldScore 3 s3 |> should be (greaterThan (Felt.foldScore 3 s5))

    [<Test>]
    member _.``振幅 0 は花 に見えない``() =
        let flat =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 5
                    Amplitude = 0.0
                    Arms = 24
                })

        Felt.foldScore 5 (fullest (runOf 90 flat)) |> should be (lessThan 0.3)

    [<Test>]
    member _.``知らない 字 は Petal``() =
        Figure.ofString "star" |> should equal Star
        // 薔薇 は 落とした。字 が来て も 花 に倒れる
        Figure.ofString "rose" |> should equal Petal
        Figure.ofString "cardioid" |> should equal Heart
        Figure.ofString "petal" |> should equal Petal
        Figure.ofString "" |> should equal Petal
        Figure.ofString "STAR" |> should equal Petal
        // 書かない 呼び手 は 今 の花 の まま
        (HarmonicSpec.create id).Figure |> should equal Petal

    [<Test>]
    member _.``振幅 0 は 3 札 とも 真円``() =
        for fig in [ Petal; Star; Heart ] do
            let s = fullest (runOf 90 (figure fig 5 0.0 1.0))
            ratio s |> should (equalWithin 1e-9) 1.0
            Felt.foldScore 5 s |> should equal 0.0

    /// 外/内 は 内/外 の 逆数 —— 1 / 0.382 = 2.618
    [<Test>]
    member _.``星 の 外 と内 は 正 五芒星 の 比``() =
        for spd in [ 0.0; 1.0; 3.0 ] do
            ratio (fullest (runOf 90 (wide Star 5 1.51 spd)))
            |> should (equalWithin 0.15) 2.618

    /// 続く 3 点 の 真ん中 が 両隣 を結ぶ 線 から 離れる 距離 の 中央値 を、山 の半径 で割る。
    /// 振幅 0 の 真円 も 床 の 下 に 出る ので、当てる のは 振幅 を入れた 木 だけ
    [<Test>]
    member _.``星 の 辺 は 直線``() =
        bend (fullest (runOf 90 (wide Star 5 1.51 1.0))) |> should be (lessThan 0.005)

        bend (fullest (runOf 90 (wide Petal 5 1.51 1.0)))
        |> should be (greaterThan 0.005)

    /// 速さ の逆数 が 正弦 に乗る か で 星 を 剥がす。`recip` 単独 では 割れない ので、`fold` との 差 の符号 で見る
    [<Test>]
    member _.``星 だけ 逆数 が 正弦 に乗る``() =
        let diff fig amp =
            let s = fullest (runOf 90 (figure fig 5 amp 0.0))
            Felt.recipScore 5 s - Felt.foldScore 5 s

        diff Star 1.51 |> should be (greaterThan 0.05)
        diff Star 1.0 |> should be (greaterThan 0.05)
        diff Petal 1.51 |> should be (lessThan 0.0)
        diff Heart 1.51 |> should be (lessThan 0.05)

    /// k 回 対称 の 札 は 1/k 周 だけ 書いて `repeat` で 回す。字 の `<fire>` の数 と
    /// 走らせた 弾 の数 の 両方 から 見る —— 片方 だけ だと 数え方 の 写し に なる
    [<Test>]
    member _.``k 回 対称 の 札 は 1/k 周 だけ 書く``() =
        let firesIn (h: HarmonicSpec) =
            let xml = BulletmlWriter.toIndentedXml 2 (Harmonic.generate h).Bulletml
            xml.Split([| "<fire" |], System.StringSplitOptions.None).Length - 1

        let plain fig k =
            HarmonicSpec.create (fun a ->
                { a with
                    Figure = fig
                    Folds = k
                    Speed = 1.0
                    Amplitude = 1.51
                })
        // 畳まる 3 つ の k。字 は 1/k 周 ＋ 頭
        for k in [ 3; 5; 8 ] do
            let h = plain Star k
            firesIn h |> should equal (h.Arms / k + 1)
            Harmonic.shotsPerWave h |> should equal (h.Arms + 1)
        // 割り切れない k と、1 回 対称 の ハート は そのまま
        let odd = plain Star 7
        firesIn odd |> should equal odd.Arms
        Harmonic.shotsPerWave odd |> should equal odd.Arms
        let heart = plain Heart 5
        firesIn heart |> should equal heart.Arms
        Harmonic.shotsPerWave heart |> should equal heart.Arms

    /// 走らせて 数える。`shotsPerWave` は 数え方 の 写し なので 走行 と 突き合わせる。
    /// `Arms` は 名指し しない —— 24 も 96 も 5 で 割り切れず 畳まれない。既定 の 120 なら 畳まる
    [<Test>]
    member _.``畳んだ 波 は 走らせて も 1 発 しか 増えない``() =
        let auto fig =
            HarmonicSpec.create (fun a ->
                { a with
                    Figure = fig
                    Folds = 5
                    Speed = 1.0
                    Amplitude = 1.51
                })

        let shotsOf (h: HarmonicSpec) =
            Felt.run 90 (Harmonic.generate h).Bulletml
            |> List.map (fun s -> s.Speeds.Length)
            |> List.max

        let folded, notFolded = auto Star, auto Heart
        folded.Arms |> should equal 120
        shotsOf folded |> should equal (folded.Arms + 1)
        shotsOf notFolded |> should equal notFolded.Arms




    /// ハート の 下 は 尖る。山 の 8 割 より 速い 弾 の 割合 で見る
    [<Test>]
    member _.``ハート の 下 は 尖る``() =
        let tip (s: Felt.Snapshot) =
            let hi = List.max s.Speeds

            float (s.Speeds |> List.filter (fun v -> v >= 0.8 * hi) |> List.length)
            / float s.Speeds.Length

        let shot fig =
            fullest (runOf 90 (wide fig 5 1.51 1.0))

        tip (shot Heart) |> should be (lessThan 0.15)
        tip (shot Petal) |> should be (greaterThan 0.15)

    /// 平均 は r0 の まま。割り忘れ は 形 を変えず に 速さ だけ を動かす ので、尖り の門 も 比 の門 も 通って しまう
    [<Test>]
    member _.``速さ の 平均 は 花 と 揃う``() =
        for spd in [ 0.0; 1.0; 3.0 ] do
            let mean fig =
                List.average (fullest (runOf 90 (wide fig 5 1.51 spd))).Speeds

            for fig in [ Star; Heart ] do
                mean fig / mean Petal |> should (equalWithin 0.03) 1.0

    /// くびれ は 1 点。床 から 立ち上げず に 掛ける と、谷 の まわり が 床 で 切られて 平ら になる
    [<Test>]
    member _.``ハート の くびれ は 1 点``() =
        let s = fullest (runOf 90 (wide Heart 5 1.51 1.0))
        let lo = List.min s.Speeds

        s.Speeds
        |> List.filter (fun v -> v <= lo + 1e-6)
        |> List.length
        |> should equal 1

    /// くびれ は 真上。撒く 向き が 回って いない こと も ここ で見る
    [<Test>]
    member _.``ハート の くびれ は 真上``() =
        let s = fullest (runOf 90 (wide Heart 5 1.51 1.0))
        let head, _ = List.zip s.Headings s.Speeds |> List.minBy snd
        // 0 と 2π の 継ぎ目 を またぐ ので、どちら の端 でも いい
        min head (2.0 * System.Math.PI - head) |> should be (lessThan 0.2)

    /// 床 を割る と 敵 の近く に居座る。ハート は 振幅 2.0 で 谷 が 床 に着く ので、
    /// 床 を外した こと が ここ に出る
    [<Test>]
    member _.``速さ は 床 と 天井 の あいだ``() =
        for fig in [ Petal; Star; Heart ] do
            for spd in [ 0.0; 1.0; 3.0 ] do
                let s = fullest (runOf 90 (figure fig 5 2.0 spd))
                List.min s.Speeds |> should be (greaterThanOrEqualTo 0.38)
                List.max s.Speeds |> should be (lessThanOrEqualTo (float Consts.MAX_SPEED))

    /// `Speed` を 振って も 外/内 が 動かない。天井 そのもの を見る のは `速さ は 床 と 天井 の あいだ` の側
    [<Test>]
    member _.``星 の 外/内 は 速さ で 動かない``() =
        let of_ spd =
            ratio (fullest (runOf 90 (wide Star 5 1.51 spd)))

        let hot, cool = of_ 3.0, of_ 0.0
        abs (hot - cool) |> should be (lessThan 0.1)

        List.max (fullest (runOf 90 (wide Star 5 1.51 3.0))).Speeds
        |> should be (lessThan (float Consts.MAX_SPEED))

    /// 1 波 の弾 が全部 1 コマ に出る。本数 は Arms
    [<Test>]
    member _.``1 波 で Arms 発 を全周 に撒く``() =
        let s = fullest (runOf 90 (flower 5))
        s.Headings.Length |> should equal 24
        Felt.ringScore s |> should be (greaterThan 0.9)

    [<Test>]
    member _.``Spin が在れば 波 ごと に回り、無ければ 回らない``() =
        let spin =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 8
                    Amplitude = 2.0
                    Spin = 1.0
                })

        let still =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 8
                    Amplitude = 2.0
                    Spin = 0.0
                })

        Felt.rotationScore (runOf 180 spin) |> should be (greaterThan 0.6)
        Felt.rotationScore (runOf 180 still) |> should be (lessThan 0.5)

    /// 花弁 の向き は 最初 の輪 を敵 (240, 80) から見た k 次 の位相 arg Σ r e^{i k θ} / k。
    /// 生まれて 5 コマ と 20 コマ の差（20 コマ までは 面 の外 へ出ない ので 1 発 も欠けない）
    [<Test>]
    member _.``回す 花 は 1 輪 ごと 広がり ながら 回る``() =
        let phase (k: int) (s: Felt.Snapshot) =
            let pts =
                List.zip s.Positions s.Born |> List.filter (fun (_, b) -> b = 1) |> List.map fst

            let re, im =
                pts
                |> List.fold
                    (fun (re, im) (x, y) ->
                        let dx, dy = x - 240.0, y - 80.0
                        let r = sqrt (dx * dx + dy * dy)
                        let t = atan2 dy dx
                        re + r * cos (float k * t), im + r * sin (float k * t))
                    (0.0, 0.0)

            atan2 im re / float k

        let turned (spin: float) =
            let k = 5

            let snaps =
                runOf
                    21
                    (HarmonicSpec.create (fun a ->
                        { a with
                            Folds = k
                            Amplitude = 2.0
                            Spin = spin
                        }))

            let at age =
                snaps |> List.find (fun s -> s.Frame = 1 + age)

            let d = (phase k (at 20) - phase k (at 5)) * 180.0 / System.Math.PI
            // 位相 は 360 / k で巡る
            let p = 360.0 / float k
            abs (((d % p) + p + p / 2.0) % p - p / 2.0)

        turned 0.0 |> should be (lessThan 0.1)
        // Spin 1 は ω = 0.75 度 / コマ、花 は ω/2 で回る ので 15 コマ で 5.6 度。Spin 2 は 1 度 / コマ で 7.5 度
        turned 1.0 |> should (equalWithin 1.0) 5.625
        turned 2.0 |> should (equalWithin 1.0) 7.5

    /// 振幅 を目一杯 に上げて も 逆走 しない。$rank = 1.0 でも MAX_SPEED を越えない
    [<Test>]
    member _.``速さ は 正 で MAX_SPEED 以下``() =
        for sp in [ 0.0; 3.0 ] do
            for k in [ 3; 5; 7; 8 ] do
                let h =
                    HarmonicSpec.create (fun a ->
                        { a with
                            Folds = k
                            Speed = sp
                            Amplitude = 2.0
                        })

                let speeds = runOf 30 h |> List.collect (fun s -> s.Speeds)
                List.min speeds |> should be (greaterThan 0.0)
                List.max speeds |> should be (lessThanOrEqualTo Consts.MAX_SPEED)

    /// 走らせて 数える。面 の外 は Felt が間引く ので Positions.Length が 同時 に居る 数
    [<Test>]
    member _.``いちばん 重い 花 でも 同時 に MAX_ALIVE 以下``() =
        let heavy =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 8
                    Speed = 0.0
                    Amplitude = 2.0
                    Arms = 160
                    Density = 3.0
                    Vanishing = false
                })

        Harmonic.aliveBound (Harmonic.fit heavy)
        |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE))

        let alive = runOf 600 heavy |> List.map (fun s -> s.Positions.Length) |> List.max
        alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)
        alive |> should be (greaterThan 0)

    /// 回す 花 も曲がり を止めた あと 面 の外 へ出る ので 数 が積もらない
    [<Test>]
    member _.``回る 重い 花 でも 同時 に MAX_ALIVE 以下``() =
        let heavy =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 8
                    Speed = 0.0
                    Amplitude = 2.0
                    Arms = 160
                    Density = 3.0
                    Spin = 2.0
                })

        let alive = runOf 900 heavy |> List.map (fun s -> s.Positions.Length) |> List.max
        alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)

    /// 180 コマ で消す と、自機 に 1 発 も届かない
    [<Test>]
    member _.``花 の弾 は 自機 の高さ まで 届く``() =
        for spin in [ 0.0; 1.0 ] do
            let h =
                HarmonicSpec.create (fun a ->
                    { a with
                        Folds = 5
                        Speed = 1.0
                        Amplitude = 1.5
                        Spin = spin
                    })

            runOf 400 h
            |> List.exists (fun s -> s.Positions |> List.exists (fun (_, y) -> y >= 600.0))
            |> should equal true

    /// 軽い 花 は 絞らない
    [<Test>]
    member _.``軽い 花 は wait を伸ばさない``() =
        (Harmonic.fit (flower 5)).WaitScale |> should equal 1.0

    [<Test>]
    member _.``n か所 で咲き、1 は 敵 の位置 のまま``() =
        for n in [ 1; 3; 5; 8 ] do
            let h, xml = blooms n 5
            let snaps = Felt.run 200 xml
            let s = bloomFrame snaps
            // 何 も生まれて いない コマ は 空。古い 弾 を束ねて いたら ここ に群 が出る
            snaps
            |> List.find (fun q -> q.Frame > s.Frame && q.Born |> List.forall (fun b -> b <> q.Frame - 1))
            |> Felt.bloomCenters 20.0
            |> should be Empty

            let groups = Felt.bloomCenters 20.0 s |> List.filter (fun (_, k) -> k >= h.Arms / 2)
            groups.Length |> should equal n
            // 束ねた のは その コマ に生まれた 弾 だけ。`Blooms` で `Arms` が 変わり、n ごと に 畳めたり 畳めなかったり する
            groups |> List.sumBy snd |> should equal (n * Harmonic.shotsPerWave h)
            // 1 か所 なら 敵 の位置、2 か所 以上 なら 茎 の先 を中心 と する n 角形 の頂点
            for (x, y), _ in groups do
                if n = 1 then
                    sqrt ((x - 240.0) ** 2.0 + (y - 80.0) ** 2.0) |> should be (lessThan 20.0)
                else
                    sqrt ((x - 240.0) ** 2.0 + (y - (80.0 + Scatter.DROP)) ** 2.0)
                    |> should (equalWithin 8.0) Scatter.REACH

    /// 割って 咲かせて も 花弁 は 読める。見る のは 撃たれた コマ —— `bloomFrame` は その 1 つ 後 で、発射角 が 1 つ も無い
    [<Test>]
    member _.``割った 花 でも 5 枚 に読める``() =
        let _, xml = blooms 3 5
        Felt.foldScore 5 (fullest (Felt.run 200 xml)) |> should be (greaterThan 0.5)

    /// 咲かせる 数 だけ 弾 は増える。`Blooms` が 腕 を割って いない と ここ で越える
    [<Test>]
    member _.``n か所 で咲かせて も 同時 に MAX_ALIVE 以下``() =
        for n in [ 3; 8 ] do
            let h =
                HarmonicSpec.create (fun a ->
                    { a with
                        Folds = 8
                        Speed = 0.0
                        Amplitude = 2.0
                        Density = 3.0
                        Spin = 1.0
                        Blooms = n
                    })

            let bound = Harmonic.aliveBound (Harmonic.fit h)
            bound |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE))

            let alive =
                Scatter.apply n (Harmonic.generate h).Bulletml
                |> Felt.run 600
                |> List.map (fun s -> s.Positions.Length)
                |> List.max

            alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)
            alive |> should be (greaterThan 0)
            // 上界 が 咲かせる 数 を見て いない と ここ で下 に潜る。`fit` は この 数 だけ を見て 絞る
            float alive |> should be (lessThanOrEqualTo bound)

    [<Test>]
    member _.``難度 の式 を出す``() =
        let xml = BulletmlWriter.toIndentedXml 2 (Harmonic.generate (flower 5)).Bulletml
        xml |> should haveSubstring "$rank"
        xml |> should haveSubstring "harmonic"

    // --- 第 2 の波 と 層 と 種
    //
    // Felt は $rank = 1 で 走る ので、返る 速さ は 基準値 の 1.3 倍 —— 床 は 0.39、天井 は MAX_SPEED

    [<Test>]
    member _.``軸 を書かない 花 は 第 2 の波 を持たない``() =
        let h = petal5 id
        h.Folds2 |> should equal 0
        h.Layers |> should equal 1
        h.Seed |> should equal 0

    [<Test>]
    member _.``種 は 18 通り を 巡る``() =
        let of_ seed =
            let h = petal5 (fun a -> { a with Seed = seed })
            h.Folds2, h.Amplitude2, h.Layers, h.LayerPhase, h.LayerScale

        [ 1..18 ] |> List.map of_ |> List.distinct |> List.length |> should equal 18
        of_ 19 |> should equal (of_ 1)

    [<Test>]
    member _.``種 が引く 比 は 倍音 2 つ と 黄金比 の相手``() =
        [ 1..18 ]
        |> List.map (fun seed -> (petal5 (fun a -> { a with Seed = seed })).Folds2)
        |> List.distinct
        |> List.sort
        |> should equal [ 8; 10; 15 ]

    [<Test>]
    member _.``名指し が 種 に勝つ``() =
        (petal5 (fun a -> { a with Seed = 1; Folds2 = 7 })).Folds2 |> should equal 7

    [<Test>]
    member _.``層 は 同じ 角 に 半径 の違う 弾 を置く``() =
        let s =
            fullest (runOf 90 (petal5 (fun a -> { a with Layers = 2; LayerPhase = 0.5 })))
        // 同じ 向き の 弾 を 束 に して、束 の中 に 速さ（＝ 同じ コマ の 半径）の違う もの が在る か
        let split =
            List.zip s.Headings s.Speeds
            |> List.groupBy (fun (th, _) -> System.Math.Round(th, 1))
            |> List.map (fun (_, g) -> g |> List.map snd)
            |> List.filter (fun vs -> List.length vs >= 2 && List.max vs / List.min vs > 1.2)

        List.length split |> should be (greaterThan 4)

    [<Test>]
    member _.``ハート の 層 は 半周 ずれる``() =
        // 位相 を Folds で割る と 半周期 の つもり が 1/10 しか ずれない。その ずれ（0.5 / 5 = 0.1）と 比べる
        let spanOf (phase: float) =
            let h =
                HarmonicSpec.create (fun a ->
                    { a with
                        Figure = Heart
                        Folds = 5
                        Speed = 2.0
                        Amplitude = 1.2
                        Layers = 2
                        LayerPhase = phase
                    })

            let s = fullest (runOf 90 h)

            List.zip s.Headings s.Speeds
            |> List.groupBy (fun (th, _) -> System.Math.Round(th, 1))
            |> List.map (fun (_, g) -> g |> List.map snd)
            |> List.filter (fun vs -> List.length vs >= 2)
            |> List.map (fun vs -> List.max vs / List.min vs)
            |> List.max

        let half, tenth = spanOf 0.5, spanOf 0.1
        TestContext.Out.WriteLine(sprintf "半周 %.2f / 1/10 %.2f" half tenth)
        half |> should be (greaterThan (tenth * 1.5))

    [<Test>]
    member _.``層 は 解像度 を 割らず に 層 の 数 だけ 撃つ``() =
        // 割る と 1 層 60 点・40 点 に なって、半周期 ずらした 層 が 絡み 点 の 散らばり に 見えた
        let mk layers =
            petal5 (fun a ->
                { a with
                    Folds2 = 10
                    Amplitude2 = 0.4
                    Layers = layers
                })

        (mk 3).Arms |> should equal (mk 1).Arms
        // 倍音 は 畳める まま —— (Arms + 1) x 層
        for layers in [ 1; 2; 3 ] do
            Harmonic.shotsPerWave (mk layers) |> should equal (((mk 1).Arms + 1) * layers)

    [<Test>]
    member _.``層 を 重ねる と 7 枚 も 畳める``() =
        // 160 は 7 で 割れず 畳めない。層 を 重ねる とき だけ 154 に 丸める —— 単層 は 160 の まま
        let mk layers =
            HarmonicSpec.create (fun a ->
                { a with
                    Folds = 7
                    Speed = 2.0
                    Amplitude = 1.2
                    Layers = layers
                })

        (mk 1).Arms |> should equal 160
        (mk 2).Arms |> should equal 154
        Harmonic.shotsPerWave (mk 2) |> should equal ((154 + 1) * 2)

    [<Test>]
    member _.``種 は ハート に 層 を 足さない``() =
        // ハート は 1 回 対称 で 畳めない。種 4 は 重ね 2 枚 の 組
        let heart (f: HarmonicAxes -> HarmonicAxes) =
            HarmonicSpec.create (fun a ->
                f
                    { a with
                        Figure = Heart
                        Folds = 5
                        Speed = 2.0
                        Amplitude = 1.2
                    })

        (petal5 (fun a -> { a with Seed = 4 })).Layers |> should equal 2
        (heart (fun a -> { a with Seed = 4 })).Layers |> should equal 1
        (heart (fun a -> { a with Seed = 4; Layers = 2 })).Layers |> should equal 2

    [<Test>]
    member _.``入れ子 の 層 は 交わらない``() =
        // 全部 の 向き で 外 の 層 ほど 速い こと と、刻み 0 の 重ね は 入れ替わる こと を 対 で 見る。
        // 層 は 撃った 順 で 分かる（`ring` は 向き ごと に 層 0, 1, … と 続けて 撃つ）。
        // 速さ を 小さい 順 に 並べて 隣 と 比べる 形 は 使えない。入れ替わり が 見えない
        let orderedOf scale =
            let s =
                fullest (
                    runOf
                        90
                        (petal5 (fun a ->
                            { a with
                                Amplitude = 2.0
                                Layers = 2
                                LayerPhase = 0.5
                                LayerScale = scale
                            }))
                )

            let chunks = List.zip s.Headings s.Speeds |> List.chunkBySize 2

            chunks
            |> List.forall (fun c -> c |> List.map fst |> List.distinct |> List.length = 1)
            |> should equal true

            chunks
            |> List.forall (fun c ->
                c
                |> List.map snd
                |> List.pairwise
                |> List.forall (fun (inner, outer) -> outer > inner))

        orderedOf 0.8 |> should equal true
        orderedOf 0.0 |> should equal false

    [<Test>]
    member _.``入れ子 でも 天井 と 床 に 当たらない``() =
        let floor, ceil = 0.3 * 1.3, float Consts.MAX_SPEED

        for spd in [ 1.0; 3.0 ] do
            let h =
                HarmonicSpec.create (fun a ->
                    { a with
                        Folds = 5
                        Speed = spd
                        Amplitude = 2.0
                        Layers = 3
                        LayerPhase = 0.5
                        LayerScale = 0.6
                    })

            let s = fullest (runOf 90 h)
            List.min s.Speeds |> should be (greaterThan (floor + 0.01))
            List.max s.Speeds |> should be (lessThan (ceil - 0.01))

    [<Test>]
    member _.``深さ は 足さず に 分け合う``() =
        // 足す 式 だと 天井 と 床 の 両方 で クリップ する
        let floor, ceil = 0.3 * 1.3, float Consts.MAX_SPEED

        for a2 in [ 0.25; 0.5; 1.0 ] do
            let h =
                HarmonicSpec.create (fun a ->
                    { a with
                        Folds = 5
                        Speed = 3.0
                        Amplitude = 2.0
                        Folds2 = 10
                        Amplitude2 = a2
                    })

            let s = fullest (runOf 90 h)
            List.min s.Speeds |> should be (greaterThan (floor + 0.01))
            List.max s.Speeds |> should be (lessThan (ceil - 0.01))

    [<Test>]
    member _.``取り分 0 は 第 2 の波 を書いて も いま と同じ``() =
        // 8 は 5 と 互いに素。倒さない と gcd 1 で 畳み が外れ、見た目 同じ まま 字 が 4 倍
        let mk k2 =
            petal5 (fun a -> { a with Folds2 = k2; Amplitude2 = 0.0 })

        (mk 8).Folds2 |> should equal 0

        BulletmlWriter.toIndentedXml 2 (Harmonic.generate (mk 8)).Bulletml
        |> should equal (BulletmlWriter.toIndentedXml 2 (Harmonic.generate (mk 0)).Bulletml)

    [<Test>]
    member _.``種 は 標本 の足りる 組 だけ を引く``() =
        // Folds2 が 大きい と 1 周期 の 点 が 足りなく なる。
        // 折り返した 正弦 は fitScore に 高く 出る ので、ここ で 別 に測る
        for folds in [ 3; 5; 7; 8 ] do
            for seed in 1..18 do
                let h =
                    HarmonicSpec.create (fun a ->
                        { a with
                            Folds = folds
                            Speed = 2.0
                            Amplitude = 1.2
                            Seed = seed
                        })

                if h.Folds2 > 0 then
                    float h.Arms / float h.Folds2 |> should be (greaterThanOrEqualTo 4.0)

    [<Test>]
    member _.``互いに素 の 第 2 の波 は 畳めない``() =
        // 取り分 を 書く —— 0 だと create が Folds2 を 0 に倒して、どちら も 畳まる
        let mk k2 =
            petal5 (fun a -> { a with Folds2 = k2; Amplitude2 = 0.4 })

        Harmonic.shotsPerWave (mk 10) |> should equal ((mk 10).Arms + 1)
        Harmonic.shotsPerWave (mk 8) |> should equal (mk 8).Arms
