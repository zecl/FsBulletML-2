module FsBulletML2.Generate.Tests.LineageTests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

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
