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

  /// この束 の素。各点 は ここ から の差分 で書く ——
  /// 軸 を足して も 呼び出し が 1 つ も 壊れない
  static let spec (f: Axes -> Axes) =
    PatternSpec.create (fun a ->
      f { a with
            Kind = Spiral
            Speed = 1.0
            Symmetry = 1.0
            BulletKinds = 1.0
            Vanishing = true
            KindConfidence = 0.8 })

  /// 名指し の 3 つ（本数 / 向き / 弾 の振る舞い）を 差し替えた 素
  static let named kind ways facing motion =
    spec (fun a -> { a with Kind = kind; Ways = ways; Facing = facing; Motion = motion })

  static let plain () = spec id

  /// 段 の形 を見る 3 本 は 腕 を細く する。腕 が多い と 1 回 の塊 が上限 を越えて
  /// `Bound.fit` が段 を落とし、形 でなく 上限 の話 を測って しまう
  static let thin cascade =
    spec (fun a -> { a with Symmetry = 0.0; Cascade = cascade })

  /// `Cascade = N` で `bullet` の定義 が N + 1 個
  [<Test>]
  member _.``段 が出る``() =
    for n in 0 .. 3 do
      countOf "<bullet label=" (toXml (Generate.generate (thin (float n)))) |> should equal (n + 1)

  [<Test>]
  member _.``いちばん 深い 段 は撒かない``() =
    toXml (Generate.generate (thin 2.0)) |> should not' (haveSubstring "\"core3\"")

  /// 定義 の数 だけ 数える と 中身 が空 の弾 が N + 1 個 出て いて も 通る。鎖 なら 定義 と 参照 で 2 回 ずつ 出る
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
        toXml (Generate.generate (spec (fun a -> { a with Layers = float n })))
      countOf "<action label=\"top" x |> should equal (n + 1)

  /// `Depth = N` で `repeat` が N + 2 本（波 1 ＋ 中間 N ＋ 腕 1）
  [<Test>]
  member _.``段 が入れ子``() =
    for n in 0 .. 2 do
      let x =
        toXml (Generate.generate (spec (fun a -> { a with Depth = float n })))
      countOf "<repeat>" x |> should equal (n + 2)

  [<Test>]
  member _.``間 が出る``() =
    let withPause =
      toXml (Generate.generate (spec (fun a -> { a with Pause = true })))
    let waits =
      Regex.Matches(withPause, @"<wait>([^<]+)</wait>")
      |> Seq.map (fun m -> evalAt 0.0 m.Groups.[1].Value)
      |> List.ofSeq
    waits |> List.exists (fun v -> v >= 60.0) |> should equal true

  [<Test>]
  member _.``引数違い で呼ぶ``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Parametrized = true })))
    countOf "<actionRef label=\"arm\"" x |> should be (greaterThanOrEqualTo 2)
    // 渡した のに読まない 引数 を残さない
    x |> should haveSubstring "$1"
    x |> should haveSubstring "$2"

  /// `sequence` の速度 は波 を跨いで 累積 する
  [<Test>]
  member _.``speedSeq は 0``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Speed = 2.0; Density = 2.0; Symmetry = 2.0; Layers = 1.0; Jitter = 1.0; Rhythm = 1.0; Depth = 1.0; Cascade = 1.0; Breathe = true; Pause = true })))
    for m in Regex.Matches(x, @"<speed type=""sequence"">([^<]+)</speed>") do
      evalAt 1.0 m.Groups.[1].Value |> should (equalWithin 0.001) 0.0

  /// 数 を直書き する と 難度 のつまみ が 1 mm も効かない
  [<Test>]
  member _.``rank が式 に入る``() =
    toXml (Generate.generate (plain ())) |> should haveSubstring "$rank"

  [<Test>]
  member _.``fire の数 が上限 以下``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Speed = 3.0; Density = 3.0; Symmetry = 3.0; Layers = 2.0; Jitter = 2.0; Rhythm = 2.0; Depth = 2.0; BulletKinds = 2.0; Cascade = 3.0; Breathe = true; Aiming = true; Pause = true; Parametrized = true })))
    countOf "<fire>" x |> should be (lessThanOrEqualTo Consts.MAX_FIRES)

  [<Test>]
  member _.``速さ が上限 以下``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Speed = 3.0; Density = 3.0; Symmetry = 3.0; Layers = 2.0; Jitter = 2.0; Rhythm = 2.0; Depth = 2.0; BulletKinds = 2.0; Cascade = 3.0; Breathe = true; Aiming = true; Pause = true; Parametrized = true })))
    for m in Regex.Matches(x, @"<speed>([^<]+)</speed>") do
      evalAt 1.0 m.Groups.[1].Value |> should be (lessThanOrEqualTo Consts.MAX_SPEED)

  [<Test>]
  member _.``repeat の回数 が上限 以下``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Speed = 3.0; Density = 3.0; Symmetry = 3.0; Layers = 2.0; Jitter = 2.0; Rhythm = 2.0; Depth = 2.0; BulletKinds = 2.0; Cascade = 3.0; Breathe = true; Aiming = true; Pause = true; Parametrized = true })))
    for m in Regex.Matches(x, @"<times>([^<]+)</times>") do
      evalAt 1.0 m.Groups.[1].Value |> should be (lessThanOrEqualTo (float Consts.MAX_REPEAT))

  /// 幕 は 下向き（180 度）を中心 に した 帯 を 等間隔 に掃く
  /// 頭 が毎波 帯 の左端 へ `absolute` で戻る ので、腕 の `sequence` が 累積 しても 帯 は回り出さない
  [<Test>]
  member _.``幕 は帯 を等間隔 に掃く``() =
    let d = spec (fun a -> { a with Kind = Curtain; Density = 1.0 })
    let x = toXml (Generate.generate d)
    // 頭 は帯 の左端。ここ へ毎波 戻る のが「回らない」の中身
    x |> should haveSubstring (sprintf "<direction type=\"absolute\">180 - %d</direction>" (curtainSpan d))
    // 刻み は 帯 の幅 を腕 で割った もの。`360 /` が出たら 全周 を掃いて いる
    x |> should haveSubstring (sprintf "<direction type=\"sequence\">%d / (" (curtainSpan d * 2))
    countOf "360 /" x |> should equal 0

  /// `Parametrized` は `arm` を 0 度 と 180 度 の対称 で組む ので、
  /// 幕 に当てる と 帯 が上下 に割れる。頼まれた 形 のほう を優先 する
  [<Test>]
  member _.``幕 は 引数違い の対称 を通さない``() =
    let x =
      toXml (Generate.generate (spec (fun a -> { a with Kind = Curtain; Density = 1.0; Parametrized = true })))
    x |> should not' (haveSubstring "label=\"arm\"")
    // 他 の型 では そのまま 通る
    let other =
      toXml (Generate.generate (spec (fun a -> { a with Density = 1.0; Parametrized = true })))
    other |> should haveSubstring "label=\"arm\""

  /// 「型 には在る が 生成器 が読まない」軸 を 数える
  [<Test>]
  member _.``全軸 が読まれる``() =
    let b = toXml (Generate.generate (plain ()))

    let others =
      [ "Kind", spec (fun a -> { a with Kind = Aimed })
        "Speed", spec (fun a -> { a with Speed = 3.0 })
        "Density", spec (fun a -> { a with Density = 3.0 })
        "Symmetry", spec (fun a -> { a with Symmetry = 3.0 })
        "Layers", spec (fun a -> { a with Layers = 2.0 })
        "Jitter", spec (fun a -> { a with Jitter = 2.0 })
        "Rhythm", spec (fun a -> { a with Rhythm = 2.0 })
        "Depth", spec (fun a -> { a with Depth = 2.0 })
        "BulletKinds", spec (fun a -> { a with BulletKinds = 2.0; Cascade = 1.0 })
        "Cascade", spec (fun a -> { a with Cascade = 3.0 })
        "Breathe", spec (fun a -> { a with Breathe = true })
        "Vanishing", spec (fun a -> { a with Vanishing = false })
        "Aiming", spec (fun a -> { a with Aiming = true })
        "Pause", spec (fun a -> { a with Pause = true })
        "Parametrized", spec (fun a -> { a with Parametrized = true }) ]

    let unread =
      others
      |> List.filter (fun (_, s) -> toXml (Generate.generate s) = b)
      |> List.map fst

    unread |> should be Empty

  /// 「3way」は どの難度 でも 3 本。`$rank` で増えたら 3way ではない
  [<Test>]
  member _.``本数 を名指し する と 定数 になる``() =
    armsExpr (named Radial 3 Around Plain) |> should equal "3"
    // 刻み も 追随 する。2 か所 で別 に計算 する と 輪 が閉じない
    armStep (named Radial 3 Around Plain) |> should haveSubstring "360 / (3)"
    // 名指し が無ければ `Symmetry` から 引く（`$rank` が効く）
    armsExpr (named Radial 0 Around Plain) |> should haveSubstring "$rank"

  /// 範囲 の外 は「名指し しない」に倒す。固定 の本数 に すると
  /// `$rank` が 1 本 も効かなく なる
  [<Test>]
  member _.``届かない 本数 は 名指し しない``() =
    for n in [ -1; 0; PatternSpec.MAX_WAYS + 1; 99 ] do
      (named Radial n Around Plain).Ways |> should equal 0
    (named Radial PatternSpec.MAX_WAYS Around Plain).Ways |> should equal PatternSpec.MAX_WAYS

  /// 面 は 縦 で、180 度 が 自機 の方向。名指し が無ければ 型 が決める 向き を
  /// 上書き しない
  [<Test>]
  member _.``向き を名指し する と 頭 が絶対角 になる``() =
    let head (f: Facing) =
      let x = toXml (Generate.generate (named Radial 3 f Plain))
      Regex.Match(x, "<direction type=\"absolute\">(-?\d+)</direction>").Groups.[1].Value

    head Forward |> should equal "180"
    head Backward |> should equal "0"
    head Sideways |> should equal "90"

  /// 名指し が無い とき は 型 の向き の まま。`Radial` は `absolute 0` で 撒く
  [<Test>]
  member _.``向き を名指し しなければ 型 のまま``() =
    let spiral = toXml (Generate.generate (named Spiral 3 Around Plain))
    // 渦 は `sequence` で回る。`absolute` に変える と 回らなく なる
    spiral |> should not' (haveSubstring "type=\"absolute\"")
    // 名指し すると 渦 は そのまま（向き は `Radial` と `Spread` に効く）
    toXml (Generate.generate (named Spiral 3 Forward Plain))
    |> should not' (haveSubstring "type=\"absolute\"")

  /// 1 発 が どう 飛ぶ か。`BulletKinds`（何種類 出すか）とは 別 の軸
  [<Test>]
  member _.``弾 の振る舞い が字 に出る``() =
    let plainX = toXml (Generate.generate (named Radial 3 Around Plain))
    let laser = toXml (Generate.generate (named Radial 3 Around Laser))
    let missile = toXml (Generate.generate (named Radial 3 Around Missile))

    // 素 は どちら も 持たない
    plainX |> should not' (haveSubstring "<changeSpeed>")
    plainX |> should not' (haveSubstring "<accel>")
    // レーザー は 撃った 直後 に 伸びる
    laser |> should haveSubstring "<changeSpeed>"
    laser |> should not' (haveSubstring "<accel>")
    // ミサイル は 曲がり ながら 加速。狙い は `Aiming` と別 に持つ
    missile |> should haveSubstring "<accel>"
    missile |> should haveSubstring "type=\"aim\""

  /// 段 の先 まで 引き継ぐ と、割れた 破片 まで レーザー に なって 形 が消える
  [<Test>]
  member _.``振る舞い は 頭 の 1 段 だけ``() =
    let deep =
      spec (fun a ->
        { a with Kind = Radial; Symmetry = 0.0; Cascade = 2.0; Ways = 3; Motion = Laser })
    countOf "<changeSpeed>" (toXml (Generate.generate deep)) |> should equal 1
