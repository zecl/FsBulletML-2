namespace FsBulletML2.MonoGame.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.MonoGame

/// このフロントを実際に回して、軌跡を凍らせる門。
///
/// `EnvGate` はこのフロントが組む `Env` の値だけを見ていて、
/// 組んだあと何をするかは 1 行 も見ていなかった。 見ていないもの:
[<TestFixture>]
[<NonParallelizable>]
type FrontRun() =

  /// 撃つ・狙う・向きを変える・待つ・繰り返す・終わって走らせ直す・消える、が
  /// 12 コマ のあいだに全部 通る。通らない命令は凍らせても意味が無い
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

  /// 自機 (30, 100)、rand 0.5、rank 0.25。`EnvGate` と同じ値
  let fixedManager () =
    { new IBulletMLManager with
        member _.GetRandom() = 0.5f
        member _.GetRank() = 0.25f
        member _.GetPlayerPosX() = 30.0f
        member _.GetPlayerPosY() = 100.0f }

  /// 1 コマ ぶんの姿。弾の数と並びも見る —— 撃った弾がどの一覧へ
  /// 入るかを取り違えても、位置だけ見ていると気づけない
  let snapshot (i: int) =
    let one (tag: string) (xs: System.Collections.Generic.List<IBullet>) =
      xs
      |> Seq.mapi (fun j (b: IBullet) ->
          sprintf "%s%d x=%.4f y=%.4f d=%.4f s=%.4f used=%b" tag j b.X b.Y b.Dir b.Speed b.Used)
      |> String.concat " | "
    sprintf "f%02d  E[%s]  EB[%s]  PB[%s]"
      i (one "e" Manager.enemies) (one "b" Manager.enemyBullets) (one "p" Manager.playerBullets)

  /// 根の敵を 1 体 置いて 12 コマ 回す。ゲーム本体と同じ順
  /// （`Manager.update` のあと `Manager.free`）
  let run () =
    let script = Runner.load loadRand (loadRank ()) (readXmlString Xml)
    let root = BaseBullet () :> IBullet
    // 立場を先に立ててから SetScript。 この順が逆だと根が
    // 「撃たれた弾」として起き、Retired が変わる（Front の SetScript の但し書き）
    root.BulletType <- BulletType.Enemy
    root.Init ()
    root.IsBullet <- false
    root.Radius <- 18.0f
    root.X <- 240.0f
    root.Y <- 100.0f
    Manager.addEnemy root
    root.SetScript (Some script)

    let sb = System.Text.StringBuilder()
    for i in 0 .. 11 do
      Manager.update ()
      Manager.free ()
      sb.AppendLine(snapshot i) |> ignore
    sb.ToString().Replace("\r\n", "\n")

  [<SetUp>]
  member _.SetUp() =
    BulletMLManager.Init(fixedManager ())
    Manager.removeAll ()

  /// 控えは目で読んでから入れた。
  /// f00 で 1 発 目、f03 で 2 発 目、f06 で top が終わって走らせ直し、
  /// 子は 4 コマ かけて向きが 180 度 へ寄っていく
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

    // 撃った弾が「敵の弾」の一覧に入っている（自機の弾ではなく）
    trace |> should haveSubstring "EB[b0 "
    trace |> should not' (haveSubstring "PB[b")

    // 子が動いている。撃った直後の位置のまま止まっていない
    let ys =
      lines
      |> Array.choose (fun l ->
          let m = System.Text.RegularExpressions.Regex.Match(l, @"b0 x=[-\d.]+ y=([-\d.]+)")
          if m.Success then Some m.Groups.[1].Value else None)
    ys |> Array.distinct |> Array.length |> should be (greaterThan 3)

    // 走らせ直しが通った。 repeat は 2 周 なので、撃った回数が
    // 3 回 以上 なら top が終わって引き直されている。
    // 一覧の添字では数えられない —— 消えた弾を抜くので番号が詰まる
    let births =
      System.Text.RegularExpressions.Regex.Matches(trace, System.Text.RegularExpressions.Regex.Escape(FrontRunGolden.Birth)).Count
    births |> should be (greaterThan 2)

    // 消えて、一覧から抜けた。 前のコマより数が減るコマが在る
    let counts =
      lines |> Array.map (fun l ->
        System.Text.RegularExpressions.Regex.Matches(l, @"\bb\d+ x=").Count)
    let shrank = Seq.init (counts.Length - 1) id |> Seq.exists (fun i -> counts.[i + 1] < counts.[i])
    shrank |> should equal true
