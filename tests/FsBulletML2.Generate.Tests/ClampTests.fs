module FsBulletML2.Generate.Tests.ClampTests

open NUnit.Framework
open FsUnit
open FsBulletML2.Generate

/// 段階数 はフィールド ごと に違う ので 4 段 と 3 段 を別 に見る ——
/// 9 本 が同じ `onScale` を通る ので、関数 を壊す と 全部 赤 になって どれ か 分からない
[<TestFixture>]
type ClampTests() =

    /// 9 本 の段 に同じ 値 を入れる。clamp は 段階数 ごと に違う ので、
    /// 端 を 1 つ の数 で入れて 別々 に見る
    static let all (v: float) =
        PatternSpec.create (fun a ->
            { a with
                Kind = Spiral
                Speed = v
                Density = v
                Symmetry = v
                Cascade = v
                Layers = v
                Jitter = v
                Rhythm = v
                Depth = v
                BulletKinds = v
                Breathe = true
                Vanishing = true
                Pause = true
                KindConfidence = 0.8
            })

    [<Test>]
    member _.``下 に外れた値 は 0 に戻る``() =
        let s = all -3.0
        s.Speed |> should equal 0.0
        s.Density |> should equal 0.0
        s.Layers |> should equal 0.0
        s.Cascade |> should equal 0.0

    [<Test>]
    member _.``4 段 のフィールド は 3 が上限``() =
        let s = all 99.0
        s.Speed |> should equal 3.0
        s.Density |> should equal 3.0
        s.Symmetry |> should equal 3.0
        s.Cascade |> should equal 3.0

    [<Test>]
    member _.``3 段 のフィールド は 2 が上限``() =
        let s = all 99.0
        s.Layers |> should equal 2.0
        s.Jitter |> should equal 2.0
        s.Rhythm |> should equal 2.0
        s.Depth |> should equal 2.0
        s.BulletKinds |> should equal 2.0

    /// 例外 を投げる と、呼ぶ側 が新しい 選択肢 を返した 日 に画面 が黙って 壊れる
    [<Test>]
    member _.``知らない kind は Radial に倒れる``() =
        PatternSpec.kindOfString "laser" |> should equal Radial
        PatternSpec.kindOfString "spiral" |> should equal Spiral
        PatternSpec.kindOfString "aimed" |> should equal Aimed
        PatternSpec.kindOfString "spread" |> should equal Spread

    /// `KindConfidence` は 軸 ではない —— 画面 に出す 値 で 生成器 は読まない
    [<Test>]
    member _.``確率 は 0 から 1``() =
        let over = PatternSpec.create (fun a -> { a with KindConfidence = 9.9 })
        over.KindConfidence |> should equal 1.0

    [<Test>]
    member _.``WaitScale の既定 は 1``() = (all 1.0).WaitScale |> should equal 1.0
