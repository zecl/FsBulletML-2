namespace FsBulletML2.Unity2D.Tests

open System.Collections.Generic
open NUnit.Framework
open FsUnit
open UnityEngine
open FsBulletML2
open FsBulletML2.Unity2D

/// 撃たれた弾の実体。`GetBulletPrefubInstance` を差し替えるためだけの派生。
/// Unity は起こさない。産まれた弾は静的な籠へ入れ、回す側が拾う。
type TestBullet(t: Transform) =
  inherit DefaultBullet(t)

  static member val Born : List<IDefaultBullet> = List<IDefaultBullet>() with get

  static member Make () =
    let b = TestBullet(Transform())
    b

  override _.GetBulletPrefubInstance () =
    let b = TestBullet.Make () :> IDefaultBullet
    TestBullet.Born.Add b
    b

/// このフロントを実際に回して、軌跡を凍らせる門。MonoGame 側と対。
/// こちらは 1/100 で縮め、Y は反転する。均すと全弾幕の軌跡が割れる。
[<TestFixture>]
[<NonParallelizable>]
type FrontRun() =

  let Xml = """<?xml version="1.0" ?>
<bulletml type="vertical" xmlns="http://www.asahi-net.or.jp/~cs8k-cyu/bulletml">
  <action label="top">
    <repeat><times>2</times>
      <action>
        <fire>
          <direction type="aim">10</direction>
          <speed>2</speed>
          <bullet>
            <!-- **弾じたいの aim。** これが無いと Env の Spawn 側が
                 一度も読まれず、SpawnOrigin を取り違えても軌跡が動かない -->
            <direction type="aim">0</direction>
            <action>
              <changeDirection>
                <direction type="absolute">180</direction><term>4</term>
              </changeDirection>
              <wait>6</wait>
              <vanish/>
            </action>
          </bullet>
        </fire>
        <wait>3</wait>
      </action>
    </repeat>
  </action>
</bulletml>"""

  /// 自機 (0.3, 1.0)、rand 0.5、rank 0.25。Unity の単位なので値が小さい
  let fixedManager () =
    { new IBulletMLManager with
        member _.GetRandom() = 0.5f
        member _.GetRank() = 0.25f
        member _.GetPlayerPosX() = 0.3f
        member _.GetPlayerPosY() = 1.0f }

  let at (x: float32) (y: float32) =
    let t = Transform()
    t.position <- Vector3(x, y, 0.0f)
    t

  let snapshot (i: int) (live: List<IDefaultBullet>) =
    let one =
      live
      |> Seq.mapi (fun j (b: IDefaultBullet) ->
          sprintf "b%d x=%.5f y=%.5f d=%.5f s=%.5f used=%b" j b.X b.Y b.Dir b.Speed b.Used)
      |> String.concat " | "
    sprintf "f%02d  [%s]" i one

  /// 根の敵を 1 体 置いて 12 コマ 回す。
  /// Manager にまとめて回す口が無いので、生きている弾を自分で回して落とす。
  let run () =
    let script = Runner.load loadRand (loadRank ()) (readXmlString Xml)
    TestBullet.Born.Clear()

    // 狙う相手。置かないと敵向きの aim が 0 になり、覚える枝が通らない
    let enemy = TestBullet(at 1.0f 2.0f) :> IDefaultBullet
    Manager.addEnemy enemy

    let root = TestBullet(at 2.4f -1.0f) :> IDefaultBullet
    // 立場を先に立ててから SetScript（Front の SetScript の但し書き）
    root.BulletType <- BulletType.Enemy
    root.Init ()
    root.IsBullet <- false
    root.X <- 2.4f
    root.Y <- -1.0f
    root.SetScript (Some script)

    let live = List<IDefaultBullet>()
    live.Add root
    let sb = System.Text.StringBuilder()
    for i in 0 .. 11 do
      let n = live.Count
      for j in 0 .. n - 1 do
        live.[j].Update ()
      // 産まれた弾を拾う（Unity では prefab の Awake が Manager へ入れる）
      for b in TestBullet.Born do live.Add b
      TestBullet.Born.Clear()
      // 使い終わったものを落とす
      let mutable k = live.Count - 1
      while k >= 0 do
        if not live.[k].Used then live.RemoveAt k
        k <- k - 1
      sb.AppendLine(snapshot i live) |> ignore
    sb.ToString().Replace("\r\n", "\n")

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(fixedManager ())
    // `Manager.removeAll` では一覧から抜けない（EnvGate の SetUp と同じ理由）。
    // 抜かないと試験ごとに敵が積み上がり、走らせる順で軌跡が変わる
    Manager.enemies.Clear()
    Manager.rootBullets.Clear()
    Manager.enemyBullets.Clear()
    Manager.playerBullets.Clear()
    TestBullet.Born.Clear()

  [<Test>]
  member _.``フロントを 12 コマ 回した軌跡が控えと同じ``() =
    let expected = FrontRunGolden.Expected
    let actual = run ()
    if expected <> actual then
      let e = expected.Split('\n')
      let a = actual.Split('\n')
      let i = Seq.init (min e.Length a.Length) id |> Seq.tryFind (fun i -> e.[i] <> a.[i])
      let where =
        match i with
        | Some i -> sprintf "%d 行目で割れました。\n  控え: %s\n  いま: %s" (i + 1) e.[i] a.[i]
        | None -> sprintf "行数が違います。控え %d 行 / いま %d 行" e.Length a.Length
      Assert.Fail(sprintf "軌跡が控えと違います。\n%s\n\n--- いま ---\n%s" where actual)

  /// 当てる先が在るか。 撃っていない・動いていない走行を凍らせても
  /// 何も守らない
  [<Test>]
  member _.``12 コマ のあいだに、撃って・動いて・消して・走らせ直している``() =
    let trace = run ()
    let lines = trace.Split('\n') |> Array.filter (fun l -> l <> "")
    lines.Length |> should equal 12
    // 撃った弾が居る（根 1 体 だけの行ではない）
    trace |> should haveSubstring "b1 "

    // 走らせ直しが通った。 repeat は 2 周 なので、撃った回数が 3 回 以上 なら
    // top が終わって引き直されている
    let births =
      System.Text.RegularExpressions.Regex.Matches(
        trace, System.Text.RegularExpressions.Regex.Escape(FrontRunGolden.Birth)).Count
    births |> should be (greaterThan 2)

    // 消えて落ちた。 撃った回数より、いちばん多いコマの弾の数が少ない。
    // 数が減るコマでは見られない —— 消えるのと次を撃つのが同じコマなので
    // 数が凹まない
    let counts =
      lines |> Array.map (fun l ->
        System.Text.RegularExpressions.Regex.Matches(l, @"\bb\d+ x=").Count)
    (Array.max counts) |> should be (lessThan (births + 1))

  /// MonoGame と係数が違う。 こちらは 1/100 で縮め、Y は反転する。
  /// 均すと全弾幕の軌跡が割れるので、ここで固定する
  [<Test>]
  member _.``座標は 1 / 100 で縮み、Y は反転する``() =
    let script = Runner.load loadRand (loadRank ()) (readXmlString Xml)
    Manager.addEnemy (TestBullet(at 1.0f 2.0f) :> IDefaultBullet)
    let b = TestBullet(at 0.0f 0.0f) :> IDefaultBullet
    b.BulletType <- BulletType.Enemy
    b.Init ()
    b.IsBullet <- false
    b.X <- 0.0f
    b.Y <- 0.0f
    b.SetScript (Some script)
    // 根は動かない弾幕なので、動いた弾で見る
    b.Update ()
    let child = TestBullet.Born.[0]
    let x0, y0 = child.X, child.Y
    child.Update ()
    let dx, dy = child.X - x0, child.Y - y0
    // 速さ 2 が 1/100 で縮む。1 コマ の移動は 0.02 のあたり
    (sqrt (dx * dx + dy * dy)) |> should be (lessThan 0.05f)
    (sqrt (dx * dx + dy * dy)) |> should be (greaterThan 0.005f)
