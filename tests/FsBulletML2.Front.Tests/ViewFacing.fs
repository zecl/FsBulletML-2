namespace FsBulletML2.Front.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Front
open FsBulletML2.Playground

/// **面の形は弾幕が決める。**
///
/// `bulletml/@type` はエンジンが使わない属性で、受け取って描き方を
/// 決めるのはフロントの仕事（仕様の但し書き）。v1.9 までは Playground も
/// Front も 1 度 も見ていなかった。
///
/// --- 版の頭で測ったこと
///
/// 横画面と名乗る 9 本 を境界の無い面で 200 コマ 走らせたら、
/// **自機に依存しない 4 本 がどれも左へ偏った**（弾の 6 割 から 8 割）。
/// つまり座標系（Y が下向き・0 度 が上）はそのままに、270 度 の側へ
/// 撃つように書かれている。**面を回すのではなく、横長にして
/// 敵を右・自機を左に置く**のが正しい読み方。
///
/// 自機を追う 5 本 は、横の面に置くともっとはっきり出て
/// 左へ 99 / 416 / 278 / 102 と揃った（縦の面では下と左に割れる）。
///
/// --- 較正（当てた変異と、赤くなった点）
///
///   `ofDirection` が常に portrait を返す      横画面は横の面
///   横の面の幅と高さを入れ替える              横の面は横長
///   `Playfield` が `Stage.portrait` 固定       横画面の敵は右
///   `orbit` が定位置を返すだけ                回ると自機が動く
///   `orbit` の半径から `Reach` を外す          回っても面の中
///   `ofInt` の知らない数を Fixed へ倒す        知らない数は追う
[<TestFixture>]
type ViewFacing() =

  let env =
    { new IFrontEnv with
        member _.Rand = fun () -> 0.5f
        member _.Rank = 0.5f
        member _.PlayerX = 0.0f
        member _.PlayerY = 0.0f
        member _.TryTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false
        member _.TrySpawnTargetFrom(_, _, tx, ty) =
          tx <- 0.0f
          ty <- 0.0f
          false }

  /// 型だけを差し替えた同じ弾幕。**中身を変えない** ——
  /// 中身が違うと、面の形の違いなのか弾幕の違いなのかが割れない
  let xmlOf (typeAttr: string) =
    sprintf
      """<?xml version="1.0" ?>
<bulletml%s xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <fire><direction type="absolute">270</direction><speed>2</speed><bullet/></fire>
    <wait>300</wait>
  </action>
</bulletml>"""
      typeAttr

  let fieldOf (typeAttr: string) =
    (Playfield.Create env (Bulletml.readXmlString (xmlOf typeAttr))).Field

  // --- 面の形 ---------------------------------------------------------------

  [<Test>]
  member _.``横画面は横の面``() =
    fieldOf """ type="horizontal" """ |> should equal Stage.landscape

  [<Test>]
  member _.``縦画面は縦の面``() =
    fieldOf """ type="vertical" """ |> should equal Stage.portrait

  [<Test>]
  member _.``型を書かなければ縦の面``() =
    // 属性を省いたときに走るのは vertical（`Api.fs` の実測）
    fieldOf "" |> should equal Stage.portrait

  [<Test>]
  member _.``none も縦の面``() =
    fieldOf """ type="none" """ |> should equal Stage.portrait

  [<Test>]
  member _.``横の面は横長``() =
    // 上の 4 点 は、**2 つ の面が同じ形でも緑**になる
    Stage.landscape.Width |> should greaterThan Stage.landscape.Height
    Stage.portrait.Width |> should lessThan Stage.portrait.Height

  [<Test>]
  member _.``横の面では敵が右で自機が左``() =
    // 測ったとおりの置き方かを字で押さえる。**ここが向きの意味**
    Stage.landscape.EnemyX |> should greaterThan (Stage.landscape.Width / 2.0f)
    Stage.landscape.PlayerX |> should lessThan (Stage.landscape.Width / 2.0f)

  [<Test>]
  member _.``縦の面では敵が上で自機が下``() =
    Stage.portrait.EnemyY |> should lessThan (Stage.portrait.Height / 2.0f)
    Stage.portrait.PlayerY |> should greaterThan (Stage.portrait.Height / 2.0f)

  [<Test>]
  member _.``置き場所はどちらも面の中``() =
    for f in [ Stage.portrait; Stage.landscape ] do
      f.EnemyX |> should be (inRange 0.0f f.Width)
      f.EnemyY |> should be (inRange 0.0f f.Height)
      f.PlayerX |> should be (inRange 0.0f f.Width)
      f.PlayerY |> should be (inRange 0.0f f.Height)

  // --- 面が実際に効いているか -----------------------------------------------

  [<Test>]
  member _.``横の面では横へ 480 を越えても生きている``() =
    // **面の形が消しの境界に効いているか。** 上の点は `Field` を見るだけなので、
    // `Playfield` が `Stage.portrait` を固定で使っていても緑になる。
    //
    // 敵 (560, 240) から 270 度（左）へ速さ 2 —— 200 コマ で 400 進んで
    // x は 160。縦の面（幅 480）に置いたら x = 240 - 400 = -160 で消えている
    let f = Playfield.Create env (Bulletml.readXmlString (xmlOf """ type="horizontal" """))
    for _ in 1 .. 200 do f.Tick()
    f.Count |> should equal 2   // 根 と 弾 1 個

  [<Test>]
  member _.``縦の面では同じ弾が消える``() =
    // 上の点の対。**縦の面なら消える**ことを見ないと、
    // 「そもそも消しが働いていない」でも緑になる
    let f = Playfield.Create env (Bulletml.readXmlString (xmlOf """ type="vertical" """))
    for _ in 1 .. 200 do f.Tick()
    f.Count |> should equal 1   // 根 だけ

  // --- 自機 -----------------------------------------------------------------

  [<Test>]
  member _.``回ると自機が動く``() =
    let struct (x0, y0) = Player.orbit Stage.portrait 0
    let struct (x1, y1) = Player.orbit Stage.portrait (Player.Period / 4)
    (x0, y0) |> should not' (equal (x1, y1))

  [<Test>]
  member _.``回りは周期で戻る``() =
    // **コマ数だけで決まる。** 時計から出すと、同じ種でも別の絵になる
    Player.orbit Stage.portrait 37
    |> should equal (Player.orbit Stage.portrait (37 + Player.Period))

  [<Test>]
  member _.``回っても面の中``() =
    for f in [ Stage.portrait; Stage.landscape ] do
      for i in 0 .. Player.Period - 1 do
        let struct (x, y) = Player.orbit f i
        x |> should be (inRange 0.0f f.Width)
        y |> should be (inRange 0.0f f.Height)

  [<Test>]
  member _.``回る軸は面の向きで入れ替わる``() =
    // 縦の面は下のほうで横に大きく、横の面は左端で縦に大きく動く。
    // **入れ替わらないと、横の面で自機が面の外へ出るか、ほとんど動かない**
    let width (f: Field) =
      let xs = [ for i in 0 .. Player.Period - 1 -> let struct (x, _) = Player.orbit f i in x ]
      List.max xs - List.min xs
    let height (f: Field) =
      let ys = [ for i in 0 .. Player.Period - 1 -> let struct (_, y) = Player.orbit f i in y ]
      List.max ys - List.min ys
    width Stage.portrait |> should greaterThan (height Stage.portrait)
    height Stage.landscape |> should greaterThan (width Stage.landscape)

  // --- マウスから来る自機 ---------------------------------------------------

  [<Test>]
  member _.``面の外の自機は定位置へ倒れる``() =
    // **向きが変わると、前の面での座標が外になる。**
    // 縦の面の自機 (240, 600) は、横の面（高さ 480）には無い場所 ——
    // 倒さないと、横の弾幕に切り替えた直後の自機が画面の外に居る
    let e = BrowserEnv()
    e.SetField Stage.landscape
    e.SetPlayer Stage.portrait.PlayerX Stage.portrait.PlayerY
    e.PlayerX |> should equal Stage.landscape.PlayerX
    e.PlayerY |> should equal Stage.landscape.PlayerY

  [<Test>]
  member _.``面の中の自機はそのまま``() =
    // 上の点は、**何を渡しても定位置に倒していれば緑**になる
    let e = BrowserEnv()
    e.SetField Stage.landscape
    e.SetPlayer 300.0f 100.0f
    e.PlayerX |> should equal 300.0f
    e.PlayerY |> should equal 100.0f

  [<Test>]
  member _.``止めているあいだはマウスを読み捨てる``() =
    let e = BrowserEnv()
    e.SetField Stage.portrait
    e.SetMotion PlayerMotion.Fixed
    e.SetPlayer 100.0f 100.0f
    e.PlayerX |> should equal Stage.portrait.PlayerX
    e.PlayerY |> should equal Stage.portrait.PlayerY

  [<Test>]
  member _.``回るとコマで自機が動く``() =
    // **進める道は 3 本 ある**（走る・1 コマ 送り・飛ぶ）ので、
    // 動かすのは `tick` 1 か所。ここではその中身を当てる
    let e = BrowserEnv()
    e.SetField Stage.portrait
    e.SetMotion PlayerMotion.Orbit
    e.AdvancePlayer 0
    let x0 = e.PlayerX
    e.AdvancePlayer (Player.Period / 4)
    e.PlayerX |> should not' (equal x0)

  [<Test>]
  member _.``追っているあいだはコマで動かない``() =
    // 上の点の対。**いつ呼んでも動く**なら、追う側が壊れている
    let e = BrowserEnv()
    e.SetField Stage.portrait
    e.SetPlayer 100.0f 200.0f
    e.AdvancePlayer (Player.Period / 4)
    e.PlayerX |> should equal 100.0f

  [<Test>]
  member _.``知らない数は追う``() =
    // **落とさない。** 落とすと、開いた人には自機が消えたように見える
    Player.ofInt 0 |> should equal PlayerMotion.Follow
    Player.ofInt 1 |> should equal PlayerMotion.Fixed
    Player.ofInt 2 |> should equal PlayerMotion.Orbit
    Player.ofInt 99 |> should equal PlayerMotion.Follow
    Player.ofInt -1 |> should equal PlayerMotion.Follow
