module FsBulletML2.Generate.Tests.HarmonicTests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

[<TestFixture>]
type HarmonicTests() =

  static let flower k =
    HarmonicSpec.create (fun a -> { a with Folds = k; Speed = 1.0; Amplitude = 2.0; Arms = 24; Vanishing = true })

  static let figure fig k amp spd =
    HarmonicSpec.create (fun a ->
      { a with Figure = fig; Folds = k; Speed = spd; Amplitude = amp; Arms = 24; Vanishing = true })

  /// 輪郭 の 尖り を見る 側。24 発 では 谷 と 山 の あいだ が 3 発 しか 無く、
  /// 「山 の 8 割 より 速い 弾 の 割合」が 刻み の粗さ に 埋もれる
  static let wide fig k amp spd =
    HarmonicSpec.create (fun a ->
      { a with Figure = fig; Folds = k; Speed = spd; Amplitude = amp; Arms = 96; Vanishing = true })

  static let runOf frames (h: HarmonicSpec) = Felt.run frames (Harmonic.generate h).Bulletml

  /// いちばん 多く 撃った コマ
  static let fullest (snaps: Felt.Snapshot list) = snaps |> List.maxBy (fun s -> s.Speeds.Length)

  /// 外 と内 の比。速さ の比 が そのまま 同じ コマ の半径 の比 になる
  static let ratio (s: Felt.Snapshot) = List.max s.Speeds / List.min s.Speeds

  /// 角 の順 に並べた 速さ の 山 の数。輪 なので 頭 と尻 を繋ぐ
  static let peaks (s: Felt.Snapshot) =
    let v = List.zip s.Headings s.Speeds |> List.sortBy fst |> List.map snd |> Array.ofList
    let n = v.Length
    [ 0 .. n - 1 ]
    |> List.filter (fun i -> v.[i] > v.[(i + n - 1) % n] && v.[i] >= v.[(i + 1) % n])
    |> List.length

  /// 種 を撒いて 咲かせた 弾幕。撒く のは `Scatter`（中身 を読まない ので 花 に限らない）で、
  /// `Blooms` は 1 輪 の腕 を割る だけ —— 対 で使わない と 弾数 か 密度 の どちら か が壊れる
  static let blooms n k =
    let h = HarmonicSpec.create (fun a -> { a with Folds = k; Speed = 1.0; Amplitude = 1.5; Blooms = n })
    h, Scatter.apply n (Harmonic.generate h).Bulletml

  /// いちばん 多く 生まれた コマ。種 が咲いた コマ が そこ に来る
  static let bloomFrame (snaps: Felt.Snapshot list) =
    snaps |> List.maxBy (fun s -> s.Born |> List.filter (fun b -> b = s.Frame - 1) |> List.length)

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
        { a with Speed = -3.0; Amplitude = -1.0; Spin = -1.0; Density = -2.0; Arms = 3; Phase = -1.0 })
    let hi =
      HarmonicSpec.create (fun a ->
        { a with Speed = 99.0; Amplitude = 9.0; Spin = 9.0; Density = 99.0; Arms = 999; Phase = 7.0 })
    (lo.Speed, lo.Amplitude, lo.Spin, lo.Density, lo.Arms) |> should equal (0.0, 0.0, 0.0, 0.0, 8)
    (hi.Speed, hi.Amplitude, hi.Spin, hi.Density, hi.Arms) |> should equal (3.0, 2.0, 2.0, 3.0, 160)
    lo.Phase |> should (equalWithin 1e-9) (2.0 * System.Math.PI - 1.0)
    hi.Phase |> should (equalWithin 1e-9) (7.0 - 2.0 * System.Math.PI)
    // 名指し が無ければ 花弁 x 24。8 枚 は 192 で 上限 に当たる
    (HarmonicSpec.create id).Arms |> should equal 120
    (HarmonicSpec.create (fun a -> { a with Folds = 3 })).Arms |> should equal 72
    (HarmonicSpec.create (fun a -> { a with Folds = 8 })).Arms |> should equal 160
    // 咲かせる 数 は 1..8。腕 は その数 で割られ、名指し して も 頭打ち になる
    (HarmonicSpec.create (fun a -> { a with Blooms = 0 })).Blooms |> should equal 1
    (HarmonicSpec.create (fun a -> { a with Blooms = 99 })).Blooms |> should equal 8
    (HarmonicSpec.create (fun a -> { a with Folds = 5; Blooms = 3 })).Arms |> should equal 40
    (HarmonicSpec.create (fun a -> { a with Folds = 5; Blooms = 8 })).Arms |> should equal 16
    (HarmonicSpec.create (fun a -> { a with Arms = 999; Blooms = 8 })).Arms |> should equal 20

  [<Test>]
  member _.``k=5 は 5 回 対称 で、k=3 より foldScore 5 が高い``() =
    let s5 = fullest (runOf 90 (flower 5))
    let s3 = fullest (runOf 90 (flower 3))
    Felt.foldScore 5 s5 |> should be (greaterThan 0.5)
    Felt.foldScore 5 s5 |> should be (greaterThan (Felt.foldScore 5 s3))
    Felt.foldScore 3 s3 |> should be (greaterThan (Felt.foldScore 3 s5))

  [<Test>]
  member _.``振幅 0 は花 に見えない``() =
    let flat = HarmonicSpec.create (fun a -> { a with Folds = 5; Amplitude = 0.0; Arms = 24 })
    Felt.foldScore 5 (fullest (runOf 90 flat)) |> should be (lessThan 0.3)

  /// --- 較正（輪郭 の 4 札 に当てた 変異 と、赤 くなった 数）
  ///
  ///   星 の正規化 sqrt(1-a^2) を 1 に        2 本
  ///   星 の分岐 を 花 に倒す                  3 本
  ///   星 の抉り alpha を 0 に                 3 本
  ///   薔薇 の 半角 を落とす（k t/2 -> k t）   2 本
  ///   知らない 字 の既定 を Star に           1 本
  ///   速さ の床 を 0 に                       1 本
  ///   速さ の天井 を MAX_SPEED そのもの に    1 本
  ///   recip の 逆数 を そのまま に            3 本
  ///   recip の ほぼ 0 落とし を外す           1 本
  ///   ハート を 素 の カージオイド に戻す      2 本
  ///   ハート を HEART_MEAN で 割らない        1 本
  ///   ハート の くびれ beta を 0 に           5 本
  ///   ハート の くびれ を 真下 に             1 本
  ///   ハート を 床 から 立ち上げない          1 本
  ///
  /// 1 周 目 が 緑 だった のは 4 つ。床 は 見る 門 が無く、ほぼ 0 落とし は 材料 が
  /// ちょうど 0 で 逆数 の 無限 を `IsFinite` が 先 に落として いた。
  /// ハート の 2 つ は どちら も 形 を変えず に 通る 変異 —— 平均 で 割らない のは
  /// 速さ だけ が 2 倍 に なり、床 から 立ち上げない のは 谷 が 床 で 切られて 平ら になる
  [<Test>]
  member _.``知らない 字 は Petal``() =
    Figure.ofString "star" |> should equal Star
    Figure.ofString "rose" |> should equal Rose
    Figure.ofString "cardioid" |> should equal Heart
    Figure.ofString "petal" |> should equal Petal
    Figure.ofString "" |> should equal Petal
    Figure.ofString "STAR" |> should equal Petal
    // 書かない 呼び手 は 今 の花 の まま
    (HarmonicSpec.create id).Figure |> should equal Petal

  [<Test>]
  member _.``振幅 0 は 4 札 とも 真円``() =
    for fig in [ Petal; Star; Rose; Heart ] do
      let s = fullest (runOf 90 (figure fig 5 0.0 1.0))
      ratio s |> should (equalWithin 1e-9) 1.0
      Felt.foldScore 5 s |> should equal 0.0

  /// α = 0.85 で 外/内 は (1+α)/(1-α) = 12.3。床 で内 が止まる ので 実測 は 11.69。
  /// 同じ 軸 の 花 は 5.00 —— 星 は 花 の 2 倍 以上 抉れる
  [<Test>]
  member _.``星 は 外 と内 が 10 倍 を超える``() =
    let star = ratio (fullest (runOf 90 (figure Star 5 1.51 0.0)))
    let petal = ratio (fullest (runOf 90 (figure Petal 5 1.51 0.0)))
    star |> should be (greaterThan 10.0)
    star |> should be (greaterThan (petal * 2.0))

  /// 速さ の逆数 が 正弦 に乗る か で 星 を 剥がす。`recip` 単独 では 割れない ——
  /// 振幅 の浅い 花 も 0.85 まで 出る ので、`fold` との 差 の符号 で見る
  ///
  /// --- 較正（4 札 x 振幅 3 段 x 速さ 3 段 = 36 通り の実測）
  ///
  ///   星 の差 の 最小      +0.093（amp 1.00 / spd 3.0）
  ///   星 以外 の 最大      +0.035（ハート。k を 輪郭 に使わない ので fold も recip も ~0）
  ///   花 の差              -0.027 .. -0.147
  ///   薔薇 の差            -0.103 .. -0.323
  ///
  /// 床 0.05 は その 2 つ の あいだ
  [<Test>]
  member _.``星 だけ 逆数 が 正弦 に乗る``() =
    let diff fig amp =
      let s = fullest (runOf 90 (figure fig 5 amp 0.0))
      Felt.recipScore 5 s - Felt.foldScore 5 s
    diff Star 1.51 |> should be (greaterThan 0.05)
    diff Star 1.0 |> should be (greaterThan 0.05)
    diff Petal 1.51 |> should be (lessThan 0.0)
    diff Rose 1.51 |> should be (lessThan 0.0)
    diff Heart 1.51 |> should be (lessThan 0.05)

  /// `|cos(kθ/2)|` の 山 は 1 周 に k 個。k が偶数 でも 倍 に ならない
  [<Test>]
  member _.``薔薇 の葉 は k 枚 で、偶数 でも 倍 に ならない``() =
    for k in [ 3; 5; 8 ] do
      peaks (fullest (runOf 90 (figure Rose k 2.0 1.0))) |> should equal k

  /// 薔薇 は 谷 に折り目 が立つ ぶん だけ 花 より 正弦 から 離れる。
  /// 輪郭 は 花 に近い（実測 0.949 対 1.000）ので、剥がす 材料 は 逆数 のほう
  [<Test>]
  member _.``薔薇 は 花 より 正弦 から 離れる``() =
    let s fig = fullest (runOf 90 (figure fig 5 1.51 0.0))
    Felt.foldScore 5 (s Rose) |> should be (lessThan (Felt.foldScore 5 (s Petal)))
    Felt.recipScore 5 (s Rose) |> should be (lessThan (Felt.recipScore 5 (s Petal)))

  /// ハート の 下 は 尖る。素 の カージオイド（r = 1 - cos θ）に 戻す と ここ が 赤 になる ——
  /// あれ は 尖点 が 在る だけ で 裾 が 広く、96 発 で描く と 卵 に見えた。
  ///
  /// --- 較正（山 の 8 割 より 速い 弾 の 割合。振幅 3 段 x 速さ 3 段 の 実測）
  ///
  ///   ハート   0.031 .. 0.115
  ///   星       0.094 .. 0.219
  ///   花       0.323 .. 0.448
  ///   薔薇     0.448 .. 0.594
  ///
  /// 床 0.15 は ハート と 花 の あいだ。星 と は 分けない —— 分ける のは 逆数 の門 のほう
  [<Test>]
  member _.``ハート の 下 は 尖る``() =
    let tip (s: Felt.Snapshot) =
      let hi = List.max s.Speeds
      float (s.Speeds |> List.filter (fun v -> v >= 0.8 * hi) |> List.length) / float s.Speeds.Length
    let shot fig = fullest (runOf 90 (wide fig 5 1.51 1.0))
    tip (shot Heart) |> should be (lessThan 0.15)
    tip (shot Petal) |> should be (greaterThan 0.15)
    tip (shot Rose) |> should be (greaterThan 0.15)

  /// 平均 は 4 札 とも r0 の 近く。ハート の 素 の式 は 山 が 4 / 谷 が 0 なので、
  /// `HEART_MEAN` で 割らない と ハート だけ 2 倍 速い ——
  /// 形 は 変わらない ので 尖り の門 も 比 の門 も 通って しまう
  ///
  /// --- 較正（平均 の速さ。振幅 1.51）
  ///
  ///   速さ 0    花 1.300   ハート 1.302
  ///   速さ 1    花 1.950   ハート 1.953
  ///   速さ 3    花 3.250   ハート 3.244
  [<Test>]
  member _.``ハート の 速さ は 花 と 揃う``() =
    for spd in [ 0.0; 1.0; 3.0 ] do
      let mean fig = List.average (fullest (runOf 90 (wide fig 5 1.51 spd))).Speeds
      mean Heart / mean Petal |> should (equalWithin 0.03) 1.0

  /// くびれ は 1 点。床 から 立ち上げず に 掛ける と、谷 の まわり が 床 で 切られて
  /// 平ら になる —— 96 発 の うち 谷 に並ぶ 数 で出る（花 は k 個、星 は 最大 17 個）
  [<Test>]
  member _.``ハート の くびれ は 1 点``() =
    let s = fullest (runOf 90 (wide Heart 5 1.51 1.0))
    let lo = List.min s.Speeds
    s.Speeds |> List.filter (fun v -> v <= lo + 1e-6) |> List.length |> should equal 1

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
    for fig in [ Petal; Star; Rose; Heart ] do
      for spd in [ 0.0; 1.0; 3.0 ] do
        let s = fullest (runOf 90 (figure fig 5 2.0 spd))
        List.min s.Speeds |> should be (greaterThanOrEqualTo 0.38)
        List.max s.Speeds |> should be (lessThanOrEqualTo (float Consts.MAX_SPEED))

  /// `Speed` が高い 札 は 星 の山 が `MAX_SPEED` で 平ら に潰れ、★ が 角 の丸い 多角形 になる。
  /// 潰れた こと は 弾数 にも 形 の門 にも 出ない ので、速さ の最大 と 外/内 で止める
  [<Test>]
  member _.``速さ の高い 星 は 山 が 天井 で潰れる``() =
    let hot = fullest (runOf 90 (figure Star 5 1.51 3.0))
    let cool = fullest (runOf 90 (figure Star 5 1.51 0.0))
    List.max hot.Speeds |> should (equalWithin 1e-6) (float Consts.MAX_SPEED)
    List.max cool.Speeds |> should be (lessThan (float Consts.MAX_SPEED))
    ratio hot |> should be (lessThan (ratio cool))

  /// 1 波 の弾 が全部 1 コマ に出る。本数 は Arms
  [<Test>]
  member _.``1 波 で Arms 発 を全周 に撒く``() =
    let s = fullest (runOf 90 (flower 5))
    s.Headings.Length |> should equal 24
    Felt.ringScore s |> should be (greaterThan 0.9)

  [<Test>]
  member _.``Spin が在れば 波 ごと に回り、無ければ 回らない``() =
    let spin = HarmonicSpec.create (fun a -> { a with Folds = 8; Amplitude = 2.0; Spin = 1.0 })
    let still = HarmonicSpec.create (fun a -> { a with Folds = 8; Amplitude = 2.0; Spin = 0.0 })
    Felt.rotationScore (runOf 180 spin) |> should be (greaterThan 0.6)
    Felt.rotationScore (runOf 180 still) |> should be (lessThan 0.5)

  /// 花 1 輪 が 広がり ながら 回る。撃つ 向き だけ を輪 ごと に回す と、1 輪 は向き を変えず に広がって
  /// 隣 と ずれて 見える だけ だった。
  ///
  /// 測り方: 最初 の輪 の弾 の位置 を敵 (240, 80) から見た 極座標 にして、花弁 の向き を k 次 の位相
  /// arg Σ r e^{i k θ} / k で取る。生まれて 5 コマ と 20 コマ の差。20 コマ までは 面 の外 へ出ない ので 1 発 も欠けない
  [<Test>]
  member _.``回す 花 は 1 輪 ごと 広がり ながら 回る``() =
    let phase (k: int) (s: Felt.Snapshot) =
      let pts = List.zip s.Positions s.Born |> List.filter (fun (_, b) -> b = 1) |> List.map fst
      let re, im =
        pts
        |> List.fold (fun (re, im) (x, y) ->
            let dx, dy = x - 240.0, y - 80.0
            let r = sqrt (dx * dx + dy * dy)
            let t = atan2 dy dx
            re + r * cos (float k * t), im + r * sin (float k * t)) (0.0, 0.0)
      atan2 im re / float k
    let turned (spin: float) =
      let k = 5
      let snaps = runOf 21 (HarmonicSpec.create (fun a -> { a with Folds = k; Amplitude = 2.0; Spin = spin }))
      let at age = snaps |> List.find (fun s -> s.Frame = 1 + age)
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
        let h = HarmonicSpec.create (fun a -> { a with Folds = k; Speed = sp; Amplitude = 2.0 })
        let speeds = runOf 30 h |> List.collect (fun s -> s.Speeds)
        List.min speeds |> should be (greaterThan 0.0)
        List.max speeds |> should be (lessThanOrEqualTo Consts.MAX_SPEED)

  /// 走らせて 数える。面 の外 は Felt が間引く ので Positions.Length が 同時 に居る 数
  [<Test>]
  member _.``いちばん 重い 花 でも 同時 に MAX_ALIVE 以下``() =
    let heavy =
      HarmonicSpec.create (fun a -> { a with Folds = 8; Speed = 0.0; Amplitude = 2.0; Arms = 160; Density = 3.0; Vanishing = false })
    Harmonic.aliveBound (Harmonic.fit heavy) |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE))
    let alive = runOf 600 heavy |> List.map (fun s -> s.Positions.Length) |> List.max
    alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)
    alive |> should be (greaterThan 0)

  /// 回す 花 も曲がり を止めた あと 面 の外 へ出る ので 数 が積もらない
  [<Test>]
  member _.``回る 重い 花 でも 同時 に MAX_ALIVE 以下``() =
    let heavy =
      HarmonicSpec.create (fun a -> { a with Folds = 8; Speed = 0.0; Amplitude = 2.0; Arms = 160; Density = 3.0; Spin = 2.0 })
    let alive = runOf 900 heavy |> List.map (fun s -> s.Positions.Length) |> List.max
    alive |> should be (lessThanOrEqualTo Consts.MAX_ALIVE)

  /// 180 コマ で消して いた とき、いちばん 速い 弾 でも 敵 から 489 px で消え、520 px 先 の自機 に 1 発 も届かなかった
  [<Test>]
  member _.``花 の弾 は 自機 の高さ まで 届く``() =
    for spin in [ 0.0; 1.0 ] do
      let h = HarmonicSpec.create (fun a -> { a with Folds = 5; Speed = 1.0; Amplitude = 1.5; Spin = spin })
      runOf 400 h
      |> List.exists (fun s -> s.Positions |> List.exists (fun (_, y) -> y >= 600.0))
      |> should equal true

  /// 軽い 花 は 絞らない
  [<Test>]
  member _.``軽い 花 は wait を伸ばさない``() =
    (Harmonic.fit (flower 5)).WaitScale |> should equal 1.0

  /// --- 較正（当てた変異 と、赤くなった点）
  ///
  ///   `create` で 腕 を 咲かせる 数 で割らない     同時 に MAX_ALIVE 以下
  ///   `aliveBound` の `* Blooms` を落とす          同時 に MAX_ALIVE 以下
  ///   `bloomCenters` で いつも 束ねる              n か所 で咲く
  ///   `bloomCenters` で 1 つ も 束ねない           n か所 で咲く
  ///   `bloomCenters` で 全部 の弾 を見る           何 も生まれて いない コマ は 空
  ///
  /// 赤く ならなかった 変異：`aliveBound` の 下限（`Arms * Blooms`）から `Blooms` を落とす。
  /// 腕 の上限 を 咲かせる 数 で割った ので、1 波 は 160 発 を越えられず この 下限 は 効かない
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
      // 束ねた のは その コマ に生まれた 弾 だけ。古い 弾 を混ぜる と ここ で増える
      groups |> List.sumBy snd |> should equal (n * h.Arms)
      // 1 か所 なら 敵 の位置、2 か所 以上 なら 茎 の先 を中心 と する n 角形 の頂点
      for (x, y), _ in groups do
        if n = 1 then
          sqrt ((x - 240.0) ** 2.0 + (y - 80.0) ** 2.0) |> should be (lessThan 20.0)
        else
          sqrt ((x - 240.0) ** 2.0 + (y - (80.0 + Scatter.DROP)) ** 2.0)
          |> should (equalWithin 8.0) Scatter.REACH

  /// 割って 咲かせて も 花弁 は 読める。速さ は 角 の関数 なので、同じ 輪 を 何 か所 で撒いて も
  /// `foldScore` は 同じ 当てはまり を返す。
  ///
  /// 見る のは 撃たれた コマ（`Speeds` が乗る のは そこ）—— 咲いた 場所 を数える `bloomFrame` は
  /// その 1 つ 後 で、発射角 が 1 つ も無い
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
          { a with Folds = 8; Speed = 0.0; Amplitude = 2.0; Density = 3.0; Spin = 1.0; Blooms = n })
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
