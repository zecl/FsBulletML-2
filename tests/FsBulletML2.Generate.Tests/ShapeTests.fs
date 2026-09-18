module FsBulletML2.Generate.Tests.ShapeTests

open System.Text.RegularExpressions
open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Generate
open FsBulletML2.Generate.Consts
open FsBulletML2.Generate.Exprs

[<TestFixture>]
type ShapeTests() =

  /// `SourceWriter` は `LanguageService.Host` に在って 試験 が引く には 重い
  static let toXml (info: BulletmlInfo) =
    BulletmlWriter.toIndentedXml 4 info.Bulletml

  static let countOf (needle: string) (s: string) =
    (s.Length - s.Replace(needle, "").Length) / needle.Length

  static let spec kind speed density symmetry layers jitter rhythm depth kinds cascade
                 breathe vanishing aiming pause parametrized =
    PatternSpec.create kind speed density symmetry layers jitter rhythm depth kinds cascade
                      breathe vanishing aiming pause parametrized 0.8

  static let plain () = spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false

  /// 段 の形 を見る 3 本 は 腕 を細く する —— 腕 が多い と 1 回 の塊 が上限 を越えて
  /// `Bound.fit` が段 を落とし、**形 でなく 上限 の話 を測って しまう**
  static let thin cascade =
    spec Spiral 1.0 0.0 0.0 0.0 0.0 0.0 0.0 1.0 cascade false true false false false

  /// `Cascade = N` で `bullet` の定義 が N + 1 個
  [<Test>]
  member _.``段 が出る``() =
    for n in 0 .. 3 do
      countOf "<bullet label=" (toXml (Generate.generate (thin (float n)))) |> should equal (n + 1)

  [<Test>]
  member _.``いちばん 深い 段 は撒かない``() =
    toXml (Generate.generate (thin 2.0)) |> should not' (haveSubstring "\"core3\"")

  /// 定義 の数 だけ 数える と 中身 が空 の弾 が N + 1 個 出て いて も 通る
  /// （`Cascade` の枝 を落とす 変異 が空振り した）。鎖 なら 定義 と 参照 で 2 回 ずつ 出る
  [<Test>]
  member _.``段 が鎖 で繋がる``() =
    let x = toXml (Generate.generate (thin 2.0))
    countOf "core1" x |> should equal 2
    countOf "core2" x |> should equal 2

  /// `Layers = N` で top の action が N + 1 個
  [<Test>]
  member _.``層 が出る``() =
    for n in 0 .. 2 do
      let x =
        toXml (Generate.generate (spec Spiral 1.0 0.0 1.0 (float n) 0.0 0.0 0.0 1.0 0.0
                                       false true false false false))
      countOf "<action label=\"top" x |> should equal (n + 1)

  /// `Depth = N` で `repeat` が N + 2 本（波 1 ＋ 中間 N ＋ 腕 1）。
  /// 「flat より 多い」だけ だと `nestRepeat` の `n <= 1` 変異 が通り抜けた
  [<Test>]
  member _.``段 が入れ子``() =
    for n in 0 .. 2 do
      let x =
        toXml (Generate.generate (spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 (float n) 1.0 0.0
                                       false true false false false))
      countOf "<repeat>" x |> should equal (n + 2)

  [<Test>]
  member _.``間 が出る``() =
    let withPause =
      toXml (Generate.generate (spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0
                                     false true false true false))
    let waits =
      Regex.Matches(withPause, @"<wait>([^<]+)</wait>")
      |> Seq.map (fun m -> evalAt 0.0 m.Groups.[1].Value)
      |> List.ofSeq
    waits |> List.exists (fun v -> v >= 60.0) |> should equal true

  [<Test>]
  member _.``引数違い で呼ぶ``() =
    let x =
      toXml (Generate.generate (spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0
                                     false true false false true))
    countOf "<actionRef label=\"arm\"" x |> should be (greaterThanOrEqualTo 2)
    // 渡した のに読まない 引数 を残さない
    x |> should haveSubstring "$1"
    x |> should haveSubstring "$2"

  /// `sequence` の速度 は波 を跨いで 累積 する —— 0.06 で弾 が 12 個 に減った
  [<Test>]
  member _.``speedSeq は 0``() =
    let x =
      toXml (Generate.generate (spec Spiral 2.0 2.0 2.0 1.0 1.0 1.0 1.0 1.0 1.0
                                     true true false true false))
    for m in Regex.Matches(x, @"<speed type=""sequence"">([^<]+)</speed>") do
      evalAt 1.0 m.Groups.[1].Value |> should (equalWithin 0.001) 0.0

  /// 数 を直書き する と 難度 のつまみ が 1 mm も効かない
  [<Test>]
  member _.``rank が式 に入る``() =
    toXml (Generate.generate (plain ())) |> should haveSubstring "$rank"

  [<Test>]
  member _.``fire の数 が上限 以下``() =
    let x =
      toXml (Generate.generate (spec Spiral 3.0 3.0 3.0 2.0 2.0 2.0 2.0 2.0 3.0
                                     true true true true true))
    countOf "<fire>" x |> should be (lessThanOrEqualTo Consts.MAX_FIRES)

  [<Test>]
  member _.``速さ が上限 以下``() =
    let x =
      toXml (Generate.generate (spec Spiral 3.0 3.0 3.0 2.0 2.0 2.0 2.0 2.0 3.0
                                     true true true true true))
    for m in Regex.Matches(x, @"<speed>([^<]+)</speed>") do
      evalAt 1.0 m.Groups.[1].Value |> should be (lessThanOrEqualTo Consts.MAX_SPEED)

  [<Test>]
  member _.``repeat の回数 が上限 以下``() =
    let x =
      toXml (Generate.generate (spec Spiral 3.0 3.0 3.0 2.0 2.0 2.0 2.0 2.0 3.0
                                     true true true true true))
    for m in Regex.Matches(x, @"<times>([^<]+)</times>") do
      evalAt 1.0 m.Groups.[1].Value |> should be (lessThanOrEqualTo (float Consts.MAX_REPEAT))

  /// 軸 を 9 -> 15 に増やす 途中 で 3 回 続けて「型 には在る が 生成器 が読まない」を作った
  /// （Layers / Depth / Kind / Aiming / Rhythm）。目 で確かめる のをやめて 数える
  [<Test>]
  member _.``全軸 が読まれる``() =
    let b = toXml (Generate.generate (plain ()))

    let others =
      [ "Kind", spec Aimed 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false
        "Speed", spec Spiral 3.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false
        "Density", spec Spiral 1.0 3.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false
        "Symmetry", spec Spiral 1.0 0.0 3.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false false
        "Layers", spec Spiral 1.0 0.0 1.0 2.0 0.0 0.0 0.0 1.0 0.0 false true false false false
        "Jitter", spec Spiral 1.0 0.0 1.0 0.0 2.0 0.0 0.0 1.0 0.0 false true false false false
        "Rhythm", spec Spiral 1.0 0.0 1.0 0.0 0.0 2.0 0.0 1.0 0.0 false true false false false
        "Depth", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 2.0 1.0 0.0 false true false false false
        "BulletKinds", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 2.0 1.0 false true false false false
        "Cascade", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 3.0 false true false false false
        "Breathe", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 true true false false false
        "Vanishing", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false false false false false
        "Aiming", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true true false false
        "Pause", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false true false
        "Parametrized", spec Spiral 1.0 0.0 1.0 0.0 0.0 0.0 0.0 1.0 0.0 false true false false true ]

    let unread =
      others
      |> List.filter (fun (_, s) -> toXml (Generate.generate s) = b)
      |> List.map fst

    unread |> should be Empty
