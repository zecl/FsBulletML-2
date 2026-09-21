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
