module FsBulletML2.Generate.Tests.LineageTests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// --- 較正（`Lineage.fs` に 当てた 変異 と、赤 くなった 試験）
///
///   種 0 の 並び を 全部 Trail に            Trail は 1 回 まで / 種 0 は 軸 から 組む
///   Trail の 回数 の 画面 の 上限 を 外す     Trail は 画面 を 横切る 時間 で 止める
///   Burst の 一列 から 難度 を 外す（2 か所） Burst の 一列 は 難度 で 伸びる / 難度 が 間隔 と 発数 に 入る
///   止める 速さ を 0 に                       止める 速さ は 0 に しない（直角 は 緑。0 でも 向き は 残る）
///   Relaunch の 加速 を 消す                  Relaunch は 止まって から 動く / 36 通り（止まった 弾 が 溜まる）
///   上界 から 重なる 周 を 外す               36 通り
///   Bar の 周 の 待ち を 1 に                 Bar の 発射台 は 周 に 1 度
[<TestFixture>]
type LineageTests() =

  static let make f = LineageSpec.create f

  static let grid =
    [ for root in [ Root.Radial; Root.Bar ] do
        for seed in 0 .. 9 do
          for g in 1 .. 3 do
            for v in [ 0.0; 1.0; 2.0 ] ->
              make (fun a ->
                { a with Root = root; Seed = seed; Generations = g
                         Streak = v; Stillness = v; Spread = v; Speed = v }) ]

  static let xml (s: LineageSpec) = BulletmlWriter.toIndentedXml 2 (Lineage.generate s).Bulletml

  static let all36 =
    [ for seed in 1 .. 9 do
        for still in [ 0.0; 1.0 ] do
          for root in [ Root.Radial; Root.Bar ] ->
            make (fun a -> { a with Root = root; Seed = seed; Stillness = still; Spread = 1.0; Streak = 1.0; Speed = 1.0 }) ]

  static let runs frames (s: LineageSpec) = Felt.run frames (Lineage.generate s).Bulletml
  static let births (snaps: Felt.Snapshot list) = snaps |> List.sumBy (fun s -> s.Headings.Length)

  /// 生まれて 1 コマ 目 の 弾 の 位置
  static let newborn (s: Felt.Snapshot) =
    List.zip s.Positions s.Born |> List.filter (fun (_, b) -> b = s.Frame - 1) |> List.map fst

  /// 同じ コマ に 生まれた 群 の、続く 2 コマ の 平均 移動 量。並び は 生き残り の 順 で 保たれる
  static let groupSpeed (snaps: Felt.Snapshot list) (born: int) (frame: int) =
    let at f =
      let s = snaps.[f - 1]
      List.zip s.Positions s.Born |> List.filter (fun (_, b) -> b = born) |> List.map fst
    let a, b = at frame, at (frame + 1)
    if a.Length = 0 || a.Length <> b.Length then nan
    else List.zip a b |> List.averageBy (fun ((x0, y0), (x1, y1)) -> sqrt ((x1 - x0) ** 2.0 + (y1 - y0) ** 2.0))

  [<Test>]
  member _.``目盛り の 外 を 切り詰める``() =
    let s = make (fun a -> { a with Generations = 9; Streak = 5.0; Ways = 99; Speed = 9.0 })
    s.Chain.Length |> should equal 3
    s.Ways |> should equal 24
    s.TrailWait.Base |> should equal 4
    (s.RootSpeed * 1.3) |> should be (lessThanOrEqualTo (float Consts.MAX_SPEED + 1e-9))
    (make (fun a -> { a with Generations = 0 })).Chain.Length |> should equal 1

  [<Test>]
  member _.``本数 0 は 根 ごと の 既定``() =
    (make (fun a -> { a with Root = Root.Radial })).Ways |> should equal 12
    (make (fun a -> { a with Root = Root.Bar })).Ways |> should equal 2

  [<Test>]
  member _.``Trail は 1 回 まで``() =
    for s in grid do
      s.Chain |> List.filter ((=) Spawner.Trail) |> List.length |> should be (lessThanOrEqualTo 1)

  [<Test>]
  member _.``種 は 9 通り の 表 を 引く``() =
    let chainOf seed = (make (fun a -> { a with Seed = seed })).Chain
    let t, b = Spawner.Trail, Spawner.Burst
    [ for s in 1 .. 9 -> chainOf s ]
    |> should equal
         [ [ t ]; [ b ]; [ t; b ]; [ b; t ]; [ b; b ]; [ t; b; b ]; [ b; t; b ]; [ b; b; t ]; [ b; b; b ] ]
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
    (make (fun a -> { a with Stillness = 1.0 })).Leaf |> should equal Terminal.Relaunch

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
    let s = make (fun a -> { a with Seed = 6; Stillness = 1.0 })   // T → B → B
    let x = xml s
    for l in [ "g1"; "g2"; "g3"; "leaf" ] do
      x |> should haveSubstring (sprintf "label=\"%s\"" l)
    x |> should not' (haveSubstring "label=\"g4\"")

  [<Test>]
  member _.``定義 に 速さ を 書かない``() =
    for s in all36 do
      match (Lineage.generate s).Bulletml with
      | Bulletml(_, elms) ->
        for e in elms do
          match e with
          | BulletmlElm.Bullet(attrs, _, spd, _) when attrs.bulletLabel <> Some(BulletLabel "bar") ->
            spd.IsNone |> should equal true
          | _ -> ()

  [<Test>]
  member _.``Trail と Relaunch は 台本 を 終わらせない``() =
    let x = xml (make (fun a -> { a with Seed = 1; Stillness = 1.0 }))   // T → Relaunch
    // Trail の 後 と Relaunch の 後 に 1 つ ずつ
    (x.Split("<wait>9999</wait>").Length - 1) |> should equal 2

  [<Test>]
  member _.``止める 速さ は 0 に しない``() =
    let x = xml (make (fun a -> { a with Seed = 2; Stillness = 1.0 }))   // B → Relaunch
    x |> should haveSubstring "0.0001"
    x |> should not' (haveSubstring "<speed>0</speed>")

  [<Test>]
  member _.``難度 が 間隔 と 発数 に 入る``() =
    let s = make (fun a -> { a with Seed = 3; Streak = 1.0; Spread = 1.0 })   // T → B
    let x = xml s
    x |> should haveSubstring (sprintf "%d - %d * $rank" s.TrailWait.Base s.TrailWait.Rank)
    x |> should haveSubstring (sprintf "%d + %d * $rank" s.Line.Base s.Line.Rank)

  /// 字 の 大きさ の 最大。参照 で 繋ぐ ので 世代数 に 比例 する
  [<Test>]
  member _.``字 は 数 KB に 収まる``() =
    let biggest = all36 |> List.map (fun s -> (xml s).Length) |> List.max
    biggest |> should be (lessThan 8000)

  /// 止めた 弾 から `relative 90` で 撃った 列 が、親 の 向き と 直角 か。
  /// 速さ 0.0001 で 向き が 残る か の 答え
  [<Test>]
  member _.``止めた 弾 の 横撃ち は 直角``() =
    let s = make (fun a -> { a with Ways = 1; Seed = 2 })   // B → Plain
    let snaps = runs 240 s
    let parent = snaps.[0].Headings |> List.exactlyOne
    let burst = snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)
    for h in burst.Headings do
      abs (sin (h - parent)) |> should be (greaterThan 0.99)

  [<Test>]
  member _.``Trail は 通り道 に 撒く``() =
    let s = make (fun a -> { a with Ways = 1; Seed = 1 })   // T → Plain
    let snaps = runs 120 s
    let spots =
      snaps |> List.map newborn |> List.filter (fun p -> p.Length > 0)
      |> List.map (fun p -> List.averageBy fst p, List.averageBy snd p)
    spots.Length |> should be (greaterThanOrEqualTo 5)
    let (x0, y0), (x1, y1) = List.head spots, List.last spots
    sqrt ((x1 - x0) ** 2.0 + (y1 - y0) ** 2.0) |> should be (greaterThan 50.0)

  [<Test>]
  member _.``Relaunch は 止まって から 動く``() =
    let s = make (fun a -> { a with Ways = 1; Seed = 2; Stillness = 1.0 })   // B → Relaunch
    let snaps = runs 300 s
    let born = (snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)).Frame
    let hold = s.RelaunchHold.Base - s.RelaunchHold.Rank     // $rank = 1
    let fly = LineageSpec.flyOf s 1
    groupSpeed snaps born (born + 5) |> should be (greaterThan 0.5)
    groupSpeed snaps born (born + fly + hold / 2) |> should be (lessThan 0.05)
    groupSpeed snaps born (born + fly + hold + 50) |> should be (greaterThan 0.5)

  [<Test>]
  member _.``難度 1 は 難度 0 より 多く 生む``() =
    for seed in [ 1; 2 ] do
      for root in [ Root.Radial; Root.Bar ] do
        let s = make (fun a -> { a with Root = root; Seed = seed; Spread = 1.0 })
        let b = (Lineage.generate s).Bulletml
        births (Felt.runAt 1.0f 400 b) |> should be (greaterThan (births (Felt.runAt 0.0f 400 b)))

  [<Test>]
  member _.``Burst の 一列 は 難度 で 伸びる``() =
    let s = make (fun a -> { a with Ways = 1; Seed = 2; Spread = 0.0 })   // B → Plain、1 列
    let column rank =
      let snaps = Felt.runAt rank 240 (Lineage.generate s).Bulletml
      (snaps |> List.skip 1 |> List.find (fun x -> x.Headings.Length > 1)).Headings.Length
    column 0.0f |> should equal (1 + s.Line.Base)
    column 1.0f |> should equal (1 + s.Line.Base + s.Line.Rank)

  /// 赤 の とき は 並び と 見積もり を 報告 に 書く。`BUDGET` を 黙って 上げない —— `shrink` の 順 が 足りない 証拠
  [<Test>]
  member _.``合計 は 削った 先 に 収まる``() =
    for s in all36 do
      LineageSpec.aliveBound s |> should be (lessThanOrEqualTo LineageSpec.BUDGET)

  [<Test>]
  member _.``Trail は 画面 を 横切る 時間 で 止める``() =
    // 根 の 速さ 1.0、間隔 8：280 / 1.0 / 8 = 35。本数 12 だと 上界 が 6,000 を 越えて `fit` が 半分 に する ので 4 本
    let s = make (fun a -> { a with Seed = 1; Ways = 4; Speed = 0.0; Streak = 0.0 })
    s.TrailTimes |> should equal 35

  /// 36 通り を 難度 1 で 走らせた 最大数。最大 に なる のは 遅くて 601 コマ 目 なので 610 コマ 見る。
  ///
  /// --- 較正 の 記録（700 コマ で 測った 実測）
  ///
  ///   最大数 の 最大            4,958 発（T → B → B・Bar・Relaunch）
  ///   実測 / 見積もり の 最大   0.756（T・Bar・Plain）
  ///   字 の 大きさ の 最大      4,126 字（B → B → B・Bar・Relaunch）
  ///
  /// 根 は 繰り返す。重なる 周 を 見積もり に 入れる 前 は 実測 / 見積もり が 4.1 倍、最大数 は 18,512 発 だった
  [<Test>]
  member _.``36 通り は 面 の 天井 に 収まる``() =
    for s in all36 do
      let peak = runs 610 s |> List.map (fun x -> x.Positions.Length) |> List.max
      peak |> should be (lessThanOrEqualTo 10000)
      float peak |> should be (lessThanOrEqualTo (LineageSpec.aliveBound s))

/// 走らせ 直し の 罠 だけ を 見る。壊す と `LineageTests` の 36 通り が 止まらなく なる ので、別 の 型 に 置いて 型 名 で 絞る
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
