module FsBulletML2.Generate.Tests.BudgetTests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate

/// 同時 に許す 弾数 の取り分。
///
/// --- 較正（当てた変異 と、赤くなった点）
///
///   `Bound.fitTo` が budget を捨てて MAX_ALIVE を使う      取り分 を半分 にすると 同時数 が減る
///   `Harmonic.fitTo` が同上                                 花 も 取り分 で減る
///   `Generate.generateTo` が fit を通さない                 取り分 を半分 にすると 同時数 が減る
[<TestFixture>]
type BudgetTests() =

  static let heavy =
    PatternSpec.create (fun a ->
      { a with
          Kind = Radial
          Speed = 1.0
          Density = 3.0
          Symmetry = 3.0
          Layers = 2.0
          Cascade = 1.0 })

  static let flower =
    HarmonicSpec.create (fun a -> { a with Folds = 8; Speed = 0.0; Amplitude = 2.0; Density = 3.0 })

  static let aliveOf (frames: int) (b: Bulletml) =
    Felt.run frames b |> List.map (fun s -> s.Positions.Length) |> List.max

  /// 既定 の口 は 1 バイト も 変わらない。呼び先 が 数百 在る
  [<Test>]
  member _.``既定 は いま までと 同じ``() =
    let xml (b: BulletmlInfo) = BulletmlWriter.toIndentedXml 2 b.Bulletml
    xml (Generate.generate heavy) |> should equal (xml (Generate.generateTo (float Consts.MAX_ALIVE) heavy))
    xml (Harmonic.generate flower) |> should equal (xml (Harmonic.generateTo (float Consts.MAX_ALIVE) flower))

  /// 取り分 を半分 にしたら、実際 に出る 数 も 半分 の側 に入る
  [<Test>]
  member _.``取り分 を半分 にすると 同時数 が減る``() =
    let full = aliveOf 600 (Generate.generate heavy).Bulletml
    let half = aliveOf 600 (Generate.generateTo (float Consts.MAX_ALIVE / 2.0) heavy).Bulletml
    full |> should be (greaterThan 0)
    half |> should be (lessThan full)
    float half |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE / 2.0))

  [<Test>]
  member _.``花 も 取り分 で減る``() =
    let full = aliveOf 600 (Harmonic.generate flower).Bulletml
    let half = aliveOf 600 (Harmonic.generateTo (float Consts.MAX_ALIVE / 2.0) flower).Bulletml
    half |> should be (lessThan full)
    float half |> should be (lessThanOrEqualTo (float Consts.MAX_ALIVE / 2.0))

  /// 0 や 負 を渡されて も 落とさない。呼ぶ側 の割り算 で 0 に なりうる
  [<Test>]
  member _.``0 以下 の取り分 は 1 に倒す``() =
    let a = Bound.fitTo 0.0 heavy
    let b = Bound.fitTo 1.0 heavy
    a.WaitScale |> should equal b.WaitScale
