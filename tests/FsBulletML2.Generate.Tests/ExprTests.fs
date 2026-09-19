module FsBulletML2.Generate.Tests.ExprTests

open NUnit.Framework
open FsUnit
open FsBulletML2.Generate
open FsBulletML2.Generate.Exprs

[<TestFixture>]
type ExprTests() =

  static let spec (f: Axes -> Axes) =
    PatternSpec.create (fun a ->
      f { a with
            Kind = Spiral
            Breathe = true; Vanishing = true; Pause = true
            KindConfidence = 0.8 })

  /// 同梱 193 本 中 184 本（95%）が `$rank` を式 に持つ
  [<Test>]
  member _.``どの式 にも rank が入る``() =
    let s = spec (fun a -> { a with Speed = 2.0; Density = 2.0; Symmetry = 2.0; Layers = 1.0; Jitter = 1.0; Rhythm = 1.0; Depth = 1.0; BulletKinds = 1.0; Cascade = 1.0 })
    for e in [ armsExpr s; speedExpr s; wavesExpr s; midExpr s; waitExpr s 0; pauseExpr s ] do
      e |> should haveSubstring "$rank"

  /// 負 の `wait` は 上界 の計算（÷ 間隔）ごと 壊す。3 軸 が掛かる ので端 を全部 回す
  [<Test>]
  member _.``wait は rank が 1 でも 1 以上``() =
    for d in [ 0.0; 1.0; 2.0; 3.0 ] do
      for r in [ 0.0; 1.0; 2.0 ] do
        for c in [ 0.0; 1.0; 2.0; 3.0 ] do
          let s = spec (fun a -> { a with Density = d; Rhythm = r; Cascade = c })
          for i in 0 .. 3 do
            evalAt 1.0 (waitExpr s i) |> should be (greaterThanOrEqualTo 1.0)

  /// 360/n を n 回 撃って 1 周 する。2 か所 で別 に計算 する と渦 が閉じない
  [<Test>]
  member _.``腕 の刻み は腕 の数 から 引く``() =
    let s = spec (fun a -> { a with Symmetry = 2.0 })
    armStep s |> should haveSubstring (armsExpr s)

  /// `BulletKinds` は 0 で見る —— 大きく する と `BURST_BUDGET` の頭打ち が
  /// 先 に効いて lv の差 が消え、`- lv * 2` を外して も 緑 のまま になる
  [<Test>]
  member _.``段 が深い ほど 撒く 数 が減る``() =
    let s = spec (fun a -> { a with Cascade = 3.0 })
    let a = evalAt 1.0 (scatterExpr s 0)
    let b = evalAt 1.0 (scatterExpr s 2)
    b |> should be (lessThan a)

  /// 3 段 なら 各 4 発（4^3 = 64）、1 段 なら 16 発
  [<Test>]
  member _.``段 が深い ほど 1 段 の予算 が小さい``() =
    let one = spec (fun a -> { a with BulletKinds = 2.0; Cascade = 1.0 })
    let three = spec (fun a -> { a with BulletKinds = 2.0; Cascade = 3.0 })
    evalAt 1.0 (scatterExpr three 0) |> should be (lessThan (evalAt 1.0 (scatterExpr one 0)))

  /// 同梱 は 1 本 が wait を中央 3 種類 使い、1 種類 だけ の本 は 6% しか 無い
  [<Test>]
  member _.``Rhythm が wait を散らす``() =
    let flat = spec (fun a -> { a with Density = 1.0 })
    let varied = spec (fun a -> { a with Density = 1.0; Rhythm = 2.0 })
    let kindsOf (s: PatternSpec) =
      [ 0 .. 5 ] |> List.map (waitExpr s) |> List.distinct |> List.length
    kindsOf flat |> should equal 1
    kindsOf varied |> should be (greaterThan 1)

  /// 外 で掛ける と、上界 を計算 した 式 と 実際 に出る 式 が割れる
  [<Test>]
  member _.``WaitScale が wait を伸ばす``() =
    let s = spec id
    let scaled = PatternSpec.withWaitScale 3.0 s
    evalAt 0.0 (waitExpr scaled 0) |> should be (greaterThan (evalAt 0.0 (waitExpr s 0)))
