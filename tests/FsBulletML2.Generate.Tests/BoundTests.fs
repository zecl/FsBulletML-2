module FsBulletML2.Generate.Tests.BoundTests

open NUnit.Framework
open FsUnit
open FsBulletML2.Generate
open FsBulletML2.Generate.Bound
open FsBulletML2.Generate.Consts
open FsBulletML2.Generate.Exprs

[<TestFixture>]
type BoundTests() =

  /// 軸 の数。9 つ の段 ＋ 5 つ の真偽
  static let AXES = 14

  /// 各軸 の 最小 と最大 の 2 値 だけ を回す（2^14 = 16,384 通り）。
  /// 掛け算 は端 で最大 になる ので、中 を回して も 上界 は破れない
  static let allExtremes () =
    let at (bits: int) (i: int) (hi: float) = if (bits >>> i) &&& 1 = 0 then 0.0 else hi
    let flag (bits: int) (i: int) = (bits >>> i) &&& 1 = 1
    seq {
      for bits in 0 .. (1 <<< AXES) - 1 ->
        PatternSpec.create (fun a ->
          { a with
              Kind = Spiral
              Speed = at bits 0 3.0
              Density = at bits 1 3.0
              Symmetry = at bits 2 3.0
              Layers = at bits 3 2.0
              Jitter = at bits 4 2.0
              Rhythm = at bits 5 2.0
              Depth = at bits 6 2.0
              BulletKinds = at bits 7 2.0
              Cascade = at bits 8 3.0
              Breathe = flag bits 9
              Vanishing = flag bits 10
              Aiming = flag bits 11
              Pause = flag bits 12
              Parametrized = flag bits 13
              KindConfidence = 0.8 })
    }

  /// この束 の土台。各点 は ここ から の差分 で書く
  static let spec (f: Axes -> Axes) =
    PatternSpec.create (fun a ->
      f { a with
            Kind = Spiral
            Speed = 1.0
            Density = 1.0
            Symmetry = 1.0
            BulletKinds = 1.0
            Vanishing = true
            KindConfidence = 0.8 })

  static let baseSpec () = spec id

  /// 全軸 を 端 まで 上げた もの。`fit` が どこ まで 絞る か を見る 点 が使う
  static let allMax () =
    spec (fun a ->
      { a with
          Speed = 3.0
          Density = 3.0
          Symmetry = 3.0
          Layers = 2.0
          Jitter = 2.0
          Rhythm = 2.0
          Depth = 2.0
          BulletKinds = 2.0
          Cascade = 3.0
          Breathe = true
          Aiming = true
          Pause = true
          Parametrized = true })

  static let withAxis kind =
    match kind with
    | "cascade" -> spec (fun a -> { a with Cascade = 2.0 })
    | "layers" -> spec (fun a -> { a with Layers = 2.0 })
    | "depth" -> spec (fun a -> { a with Depth = 2.0 })
    | "symmetry" -> spec (fun a -> { a with Symmetry = 3.0 })
    | _ -> failwithf "知らない 軸: %s" kind

  /// 端 を 1 つ も 落として いない —— 畳んだ 添字 がずれる と 数 が減る
  [<Test>]
  member _.``端 の組み合わせ を全部 回す``() =
    allExtremes () |> Seq.length |> should equal 16384
    allExtremes () |> Seq.filter (fun s -> s.Cascade = 3.0) |> Seq.length |> should equal 8192
    allExtremes () |> Seq.filter (fun s -> s.Pause) |> Seq.length |> should equal 8192

  /// 軸 ごと の clamp は「1 本 の式 が暴れない」ことしか 保証 しない。実機 で確かめた:
  ///
  ///     Cascade 3 段   同時 21,517 発（MAX_ALIVE の 24 倍）
  ///     Cascade 無し   同時  1,070 発（それでも 破れる）
  [<Test>]
  member _.``fit を通せば どの 組み合わせ でも 上界 の中``() =
    let bad =
      allExtremes ()
      |> Seq.map fit
      |> Seq.filter (fun s -> aliveBound s > float MAX_ALIVE)
      |> Seq.truncate 3
      |> List.ofSeq

    bad |> List.length |> should equal 0

  /// 実機 で 1,958 -> 662 に絞った 後 も、腕 18 本 のリング も 3 層 の重なり も残った。
  /// `Cascade` だけ は最後 の手段 で落ちる（下 の 2 本 が見る）
  [<Test>]
  member _.``fit は形 を壊さない``() =
    for s in allExtremes () |> Seq.truncate 2000 do
      let f = fit s
      f.Symmetry |> should equal s.Symmetry
      f.Layers |> should equal s.Layers
      f.Depth |> should equal s.Depth
      f.BulletKinds |> should equal s.BulletKinds
      f.Kind |> should equal s.Kind

  /// 段 を落とす のは `wait` で絞りきれない とき だけ。
  /// 収まって いる 仕様 の段 を削る と、頼んだ 個性 が黙って 消える
  [<Test>]
  member _.``収まる 仕様 の段 は落とさない``() =
    let ok =
      allExtremes ()
      |> Seq.filter (fun s -> aliveBound s <= float MAX_ALIVE)
      |> Seq.truncate 3000
      |> List.ofSeq

    ok |> List.isEmpty |> should equal false
    for s in ok do
      (fit s).Cascade |> should equal s.Cascade

  /// `wait` を伸ばして も 1 回 の塊 は減らない ので、段 を落とす 経路 が要る ——
  /// 全軸 最大 は wait 278 倍 でも 同時 3,351 発 だった
  [<Test>]
  member _.``塊 が越える 仕様 では 段 が落ちる``() =
    let heavy = allMax ()
    (fit heavy).Cascade |> should be (lessThan heavy.Cascade)

  [<Test>]
  member _.``余裕 が在れば WaitScale は 1 のまま``() =
    let s =
      spec (fun a ->
        { a with Density = 0.0; Symmetry = 0.0; BulletKinds = 0.0; Pause = true })
    (fit s).WaitScale |> should equal 1.0

  [<Test>]
  member _.``重い 仕様 では WaitScale が伸びる``() =
    (fit (allMax ())).WaitScale |> should be (greaterThan 1.0)

  // ------------------------------------------------------------------
  // `aliveBound (fit s) <= MAX_ALIVE` だけ を見る と、両側 が一緒 にずれて
  // 緑 のまま になる —— 掛け算 の項 を落とす 変異 が 4 本 とも 空振り した。
  // だから 軸 を 1 つ 上げた ら 上界 が増える を直 に見る
  // ------------------------------------------------------------------

  [<Test>]
  member _.``Cascade を上げる と 上界 が増える``() =
    aliveBound (withAxis "cascade") |> should be (greaterThan (aliveBound (baseSpec ())))

  [<Test>]
  member _.``Layers を上げる と 上界 が増える``() =
    aliveBound (withAxis "layers") |> should be (greaterThan (aliveBound (baseSpec ())))

  /// `Depth` は上界 に効かない —— 中間 `repeat` は発射 の回数 を増やす が
  /// 同じ だけ 時間 も 伸びる。掛けた とき 上界 5,100 万 発 が出た
  [<Test>]
  member _.``Depth は上界 に効かない``() =
    aliveBound (withAxis "depth") |> should (equalWithin 0.001) (aliveBound (baseSpec ()))

  [<Test>]
  member _.``Symmetry を上げる と 上界 が増える``() =
    aliveBound (withAxis "symmetry") |> should be (greaterThan (aliveBound (baseSpec ())))

  /// 速い 弾 ほど 短命。180 固定 で見積もる と 上界 が過大 になり、
  /// `fit` が `wait` を伸ばして「速い が スカスカ」になる（実機 で踏んだ）
  [<Test>]
  member _.``速い 弾幕 ほど 上界 が小さい``() =
    let atSpeed v = spec (fun a -> { a with Speed = v })
    aliveBound (atSpeed 3.0) |> should be (lessThan (aliveBound (atSpeed 0.0)))

  /// `Breathe` は `changeSpeed "0.25"` で 60 コマ ほど 溜める ——
  /// その間 ほとんど 進まない ので、見ない と 上界 が破れた（全軸 最大 で 0.85 倍）
  [<Test>]
  member _.``溜める 弾幕 は 上界 が大きい``() =
    let withBreathe b = spec (fun a -> { a with Speed = 3.0; Breathe = b })
    aliveBound (withBreathe true) |> should be (greaterThan (aliveBound (withBreathe false)))

  /// 段 の子 は 親 より 遅い（`subSpeedExpr` の係数 が 0.9 - lv * 0.15）ので 長命。
  /// 親 の速度 だけ 見る と 短命 に見積もって 上界 が破れる。
  ///
  /// 段 の子 は 親 より 遅い（`subSpeedExpr` の係数 は 0.9 - lv * 0.15）ので 長命。
  ///
  /// `Cascade` は `burst` でも `wait` でも 上界 を動かす ので、比 では 寿命 の効き が
  /// 見えない —— `subSpeedExpr` が 親 より 遅い こと を直 に固定 する。
  /// ここ が破れる と `lifeOf` の `List.min` が 親 だけ 見て も 同じ 値 になり、
  /// 上界 が短命 に寄って 破れる（実機 の全軸 最大 で 0.85 倍）
  [<Test>]
  member _.``段 の子 は 親 より 遅い``() =
    let s = spec (fun a -> { a with Speed = 3.0; Cascade = 2.0 })
    let parent = evalAt 1.0 (speedExpr s)
    let kids = [ for lv in 0 .. step s.Cascade - 1 -> evalAt 1.0 (subSpeedExpr s lv) ]

    kids |> List.isEmpty |> should equal false
    for k in kids do
      k |> should be (lessThan parent)
    // 深い 段 ほど 遅い
    kids |> List.last |> should be (lessThan (List.head kids))

  /// 子 の速度 が `lifeOf` に届いて いる か。
  ///
  /// 上 の 1 本 は `subSpeedExpr` の中身 しか 見ない ので、`lifeOf` が 子 を
  /// 数え損ねて も 緑 のまま だった（`for lv in 0 .. -1` の変異 が空振り）。
  ///
  /// 段 を深く する と 最遅 が下がる —— `scatterExpr` の増分 を打ち消して、
  /// 寿命 が伸びた ぶん だけ 上界 が余計 に増える ことを 見る
  [<Test>]
  member _.``子 の速度 が 上界 に届く``() =
    let atCascade c = spec (fun a -> { a with Speed = 3.0; Cascade = c })

    // 段 2 は 段 1 より 最遅 が遅い（3.3 対 3.5）
    let slowestOf (s: PatternSpec) =
      [ yield evalAt 1.0 (speedExpr s)
        for lv in 0 .. step s.Cascade - 1 -> evalAt 1.0 (subSpeedExpr s lv) ]
      |> List.min

    slowestOf (atCascade 2.0) |> should be (lessThan (slowestOf (atCascade 1.0)))

    // その差 が 上界 に出る。burst と wait の効き を割って、寿命 の比 だけ を残す
    let normalized (s: PatternSpec) =
      let burst =
        let mutable acc = 1.0
        let mutable total = 1.0
        for lv in 0 .. step s.Cascade - 1 do
          acc <- acc * evalAt 1.0 (scatterExpr s lv)
          total <- total + acc
        total
      aliveBound s / burst * (evalAt 1.0 (waitExpr s 0))

    // 寿命 = FIELD_SPAN / 最遅。遅い ほう が大きい
    let ratio = normalized (atCascade 2.0) / normalized (atCascade 1.0)
    let lifeRatio = slowestOf (atCascade 1.0) / slowestOf (atCascade 2.0)
    ratio |> should (equalWithin 0.05) lifeRatio

  /// 値 を手 で固定 する —— 不等式 で書く と 緩すぎて、項 を 1 つ だけ
  /// `$rank = 0.5` にした 変異 が通り抜けた。
  ///
  ///     armsExpr 8 腕 ＋ 速度 の起点 1 発 = 9   waitExpr 5 間隔
  ///     Depth 0 / Cascade 0 / Layers 0 / Pause 無し
  ///
  /// 寿命 は 180 固定 でなく 速度 で決まる。`speedExpr` が `$rank = 1` で 2.5 なので
  /// `FIELD_SPAN 280 ÷ 2.5 = 112` —— 9 ÷ 5 × 112 × 1.3 = 262.08
  [<Test>]
  member _.``上界 は いちばん 重い側 の値 になる``() =
    evalAt 1.0 (armsExpr (baseSpec ())) |> should (equalWithin 0.001) 8.0
    evalAt 1.0 (waitExpr (baseSpec ()) 0) |> should (equalWithin 0.001) 5.0
    aliveBound (baseSpec ()) |> should (equalWithin 0.01) 262.08

  /// `perTurn` は 型 ごと に撃つ 数 が違う（頭 を 1 本目 に数える 型 と、頭 の後 に n 本 撃つ 型）。
  /// 式 だけ 見る と 生成器 の数え方 が変わって も 緑 のまま なので、走らせて 1 コマ の発射数 と突き合わせる
  [<Test>]
  member _.``perTurn は 1 回 の腕 で実際 に撃つ 数``() =
    let cases =
      [ for kind in [ Spiral; Radial; Aimed; Spread; Curtain ] do
          for ways in [ 0; 1; 3; 8 ] do
            for p in [ false; true ] ->
              spec (fun a -> { a with Kind = kind; Ways = ways; Parametrized = p }) ]
    cases |> List.length |> should equal 40
    for s in cases do
      let fired =
        Felt.run 30 (Generate.generate s).Bulletml |> List.map (fun f -> f.Headings.Length) |> List.max
      (s.Kind, s.Ways, s.Parametrized, float fired) |> should equal (s.Kind, s.Ways, s.Parametrized, perTurn s)

  /// Core で 10 通り 走らせて 校正 した（2026-09-19、`$rank = 1.0`、種 3 通り の最大、
  /// 1,800 コマ）。ブラウザ では 背面タブ で コマ が間引かれて 数 が取れない ——
  /// 弾数 は論理値 なので 同じ Core を .NET で回す。
  ///
  /// 面 も 実機 に合わせる（`Stage.portrait` の 480x640、敵 240,80、自機 240,600）——
  /// 画面外 で消える 条件（`client/Playfield.fs:432`）を入れない と、
  /// `Vanishing` が偽 の弾 が永久 に溜まって 別物 を測る。
  ///
  ///     仕様                      予測   実測    比
  ///     a-spiral-2層-間            724    418   1.73
  ///     b-radial-parametrized      710    514   1.38
  ///     c-aimed-cascade3           793    682   1.16
  ///     d-全軸最大                  672    322   2.09
  ///     e-最小                     134     68   1.97
  ///     f-間なし-密                 728    506   1.44
  ///     g-間あり-密                  18     18   1.01
  ///     h-depth2                   262    167   1.57
  ///     i-layers2                  728    528   1.38
  ///     j-spread-breathe           493    265   1.86
  ///
  /// `SAFETY` を 1.15 まで 下げた ら g が 0.89 で破れた ので 1.3 に置く。
  /// 上げすぎる と `fit` が `wait` を伸ばして 密度 が死ぬ ——
  /// 「速くて 避けにくい」と頼んだ のに スカスカ になる（実機 で踏んだ）
  [<Test>]
  member _.``SAFETY は いちばん 外す ところ を覆う``() =
    SAFETY |> should be (greaterThanOrEqualTo (1.0 / 1.01))
    // 大きすぎる と 収まって いる 仕様 まで 絞る
    SAFETY |> should be (lessThanOrEqualTo 2.0)
