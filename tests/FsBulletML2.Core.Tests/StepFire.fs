namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.DTD
open FsBulletML2.Domain

/// fire は撃つ側の状態と撃たれた弾の両方を決める。
/// bullet の中に書いた値が fire 側より勝つところが、いちばん間違えやすい。
[<TestFixture>]
type StepFire() =

  // 4 つとも別の値にしてある。同じ値にすると、fire 側と bullet 側で
  // 基準を取り違えていても門が緑のまま通る
  let env = { Rand = (fun () -> 0.5f); Rank = 0.5f; AimDir = 1.0f; EnemyAimDir = 2.0f; SpawnAimDir = 3.0f; SpawnEnemyAimDir = 4.0f }

  let state =
    { Pos = { X = 5.f; Y = 7.f }
      Speed = 1.f
      Dir = 0.f
      Accel = { X = 0.f; Y = 0.f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false
      Tops = [] }

  let noResolvers : Step.Resolvers =
    { Bullet = fun _ _ -> None
      Action = fun _ _ -> None }

  let bullet d s =
    RecBulletml.Bullet ({ bulletLabel = None }, d, s, [])

  [<Test>]
  member _.``fire は、撃たれた弾を 1 つ出す``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Absolute }, "0")),
                        Some (Speed (Some { speedType = SpeedType.Absolute }, "2")),
                        bullet None None)
    let (r, _, _), _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    r |> should equal Step.Ended
    match w with
    | [ Spawn b ] ->
        b.Speed |> should (equalWithin 0.0001) 2.0f
        b.IsBullet |> should equal true
        b.Pos.X |> should (equalWithin 0.0001) 5.0f
        b.Pos.Y |> should (equalWithin 0.0001) 7.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``撃った側は HasFired が立つ``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None }, None, None, bullet None None)
    let (_, _, _), st, _ = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    st.HasFired |> should equal true

  [<Test>]
  member _.``bullet の中の speed が fire 側より勝つ``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, "2")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, "9"))))
    let _, _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Speed |> should (equalWithin 0.0001) 9.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``bullet の中の direction が fire 側より勝つ``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Absolute }, "0")),
                        None,
                        bullet (Some (Direction (Some { directionType = DirectionType.Absolute }, "90"))) None)
    let _, _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) (float32 (System.Math.PI / 2.0))
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``direction を省くと aim になる``() =
    let script = RecBulletml.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) env.AimDir
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// final review 3: <bullet><direction type="aim"> は、fire 側の aim
  /// （撃った側の位置に依る env.AimDir）とは別の値になるはずなので、
  /// この時点（Step.fire の中）では解決できない。旧 createTask は
  /// GetNewBullet() が返す、まだ位置を持たない新しい弾オブジェクトの
  /// GetAimDir() を読んでいたが、Step.fire の時点では撃たれた弾の
  /// 実オブジェクトがまだ存在しない（Spawn は値で、実体は
  /// BulletRunner.applySpawn が newBullet として後で作る）。
  /// ここでは env.SpawnAimDir（産まれる弾の位置から見た向き）が使われ、
  /// 撃った側の env.AimDir は使われないことを見る。
  /// 実際に走らせたときの最終値は BulletAim.fs が Trace 経由で確かめる
  [<Test>]
  member _.``bullet 側の aim は、産まれる弾の位置から見た向きで解決する``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None }, None, None,
                        bullet (Some (Direction (Some { directionType = DirectionType.Aim }, "30"))) None)
    let _, _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] ->
        // 30 度 = π/6 に env.SpawnAimDir（3.0）が足された値。
        // 撃った側の env.AimDir（1.0）を混ぜていれば約 1.524 になるので、
        // 取り違えるとここで割れる
        b.Dir |> should (equalWithin 0.0001) (Step.calcDir (env.SpawnAimDir + float32 (System.Math.PI / 6.0)))
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// 対照: fire 側の aim（bullet 側は無指定）は撃った側の env.AimDir を使う。
  /// bullet 側と基準が違うことを、値が違うことで示す
  [<Test>]
  member _.``fire 側の aim は、撃った側の位置から見た向きで解決する``() =
    let script = RecBulletml.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env state (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] ->
        b.Dir |> should (equalWithin 0.0001) env.AimDir
        // 産まれる弾の側の値（3.0）ではないこと。両者が同じ値だと
        // 取り違えても緑になるので、違うことを明示で見る
        b.Dir |> should not' (equalWithin 0.0001 env.SpawnAimDir)
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``sequence は、直前の fire の値に積む``() =
    let script =
      RecBulletml.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Sequence }, "10")),
                        None,
                        bullet None None)
    let fc = { FireContext.zero with SrcDir = 1.0f }
    let (_, _, fc'), _, _ = Sim.run env state (Step.fire noResolvers script (PFire false) fc)
    // 10 度 = π/18 を足す
    fc'.SrcDir |> should (equalWithin 0.0001) (1.0f + float32 (System.Math.PI / 18.0))

  [<Test>]
  member _.``bulletRef は 1 段だけ解く``() =
    let target = bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, "7")))
    let resolvers : Step.Resolvers =
      { Bullet = (fun label _ -> if label = "b1" then Some target else None)
        Action = fun _ _ -> None }
    let script =
      RecBulletml.Fire ({ fireLabel = None }, None, None,
                        RecBulletml.BulletRef ({ bulletRefLabel = "b1" }, []))
    let _, _, w = Sim.run env state (Step.fire resolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Speed |> should (equalWithin 0.0001) 7.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// final review 5: 5 つめの draw site（設計文書 5.3 参照）。
  ///
  /// 旧 expandBulletRefOnce は convertRecBulletmlEx を通しており、解決した
  /// 瞬間に bullet 本体の中の wait をまとめて引いていた。この直後、
  /// createTask の bulletElm.Init(env) に当たる resetChild がもう一度
  /// 同じ wait を引き直す（1 回めの値は上書きされて捨てられるが、消費は
  /// 残る）ので、bulletRef で解決したときは合計 2 回。resetChild だけなら
  /// 1 回のはず——bulletRef の解決そのものがもう 1 回ぶんの乱数を消費する
  [<Test>]
  member _.``bulletRef を解いた瞬間にも、bullet 本体の中の wait を先に引く（resetChild と合わせて 2 回）``() =
    let target =
      RecBulletml.Bullet ({ bulletLabel = None }, None, None,
                          [ RecBulletml.Action ({ actionLabel = None }, [ RecBulletml.Wait "5" ]) ])
    let resolvers : Step.Resolvers =
      { Bullet = (fun label _ -> if label = "b1" then Some target else None)
        Action = fun _ _ -> None }
    let script =
      RecBulletml.Fire ({ fireLabel = None }, None, None,
                        RecBulletml.BulletRef ({ bulletRefLabel = "b1" }, []))
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let _, _, w = Sim.run counting state (Step.fire resolvers script (PFire false) FireContext.zero)
    draws |> should equal 2
    match w with
    | [ Spawn b ] ->
        match b.Tops with
        | [ (_, PAction (false, None, [ PWait (started, left) ]), _) ] ->
            started |> should equal true
            left |> should (equalWithin 0.0001) 5.0f
        | other -> Assert.Fail (sprintf "予期しない Tops: %A" other)
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``撃つ側が Player なら、direction を省くと EnemyAimDir になる``() =
    // aim = if self.Kind = Player then EnemyAimDir else AimDir。
    // 与えられたテストは Kind = Enemy 固定で AimDir 側しか通らないので、
    // 逆の枝（Player 側）もここで踏む。踏まないと AimDir と EnemyAimDir を
    // 取り違えても気づけない
    let playerState = { state with Kind = BulletType.Player }
    let script = RecBulletml.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env playerState (Step.fire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) env.EnemyAimDir
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``fire は、撃たれた弾の action の中身の getValue も先に引く``() =
    // 現行の createTask は bulletElm.Init(env) を先頭で呼び、Wait /
    // ChangeDirection / ChangeSpeed を Action / Repeat / Fire / Bullet を
    // 辿って先に引く（Processable.fs の Init 参照）。ここを Progress.initial で
    // 組むと、この分の乱数消費が丸ごと消えて、fire の直後から乱数列が
    // ずれてしまう。
    //
    // bullet の中身は Action [ Wait "3"; ChangeDirection(絶対 90, term "2");
    // ChangeSpeed(絶対 5, term "4") ] の 1 本。fire 側／bullet 側の
    // direction・speed はどちらも省いて getValue を呼ばせない。
    // 期待は Wait の term (1) + ChangeDirection の term (1) +
    // ChangeSpeed の term (1) = 3 回
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let bulletBody =
      RecBulletml.Bullet ({ bulletLabel = None }, None, None,
                          [ RecBulletml.Action ({ actionLabel = None },
                              [ RecBulletml.Wait "3"
                                RecBulletml.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, "90"), Term "2")
                                RecBulletml.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, "5"), Term "4") ]) ])
    let script = RecBulletml.Fire ({ fireLabel = None }, None, None, bulletBody)
    Sim.run counting state (Step.fire noResolvers script (PFire false) FireContext.zero) |> ignore
    draws |> should equal 3

  [<Test>]
  member _.``bullet の speed は、$rand を含まない定数式でも現行と同じく 2 回 getValue を読む``() =
    // getValue は式の中身に関わらず env.Rand () を無条件に呼ぶ（TryParse.eval に
    // 渡す前に呼ぶ）。現行は bullet 側の speed をこの式のまま 2 回読んでいる
    // （createTask 相当と、fireCommand 相当）。
    // 1 回めの結果は 2 回めの書き込みで必ず上書きされて使われないが、
    // getValue の呼び出しそのものは残るので、"9" のような $rand を含まない
    // 定数式でも消してはいけない
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script =
      RecBulletml.Fire ({ fireLabel = None }, None, None,
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, "9"))))
    Sim.run counting state (Step.fire noResolvers script (PFire false) FireContext.zero) |> ignore
    draws |> should equal 2

  [<Test>]
  member _.``撃つ側の SrcSpeed は、latch が立つまでは bullet 側の速さを引き継ぎ、fire 側の値は読まれない``() =
    // <fire><speed type="absolute">2</speed><bullet><speed type="absolute">5</speed></bullet></fire>
    // <fire><speed type="sequence">0</speed><bullet/></fire>
    //
    // 1 発め: fc.SpeedInit（撃つ側の top ごとの latch）がまだ false で、
    // bullet 側に speed が書いてあるので、bullet の速さ (5) をそのまま
    // SrcSpeed に採用して latch を立てる。fire 側の speed "2" は
    // getValue すら呼ばれない（bullet 側が勝つ、の 1 段深いところにある
    // 現行の fireCommand の癖）
    //
    // 2 発め: latch が立っているので、今度は fire 側の speed "0"（sequence）
    // を実際に読み、SrcSpeed(5) + 0 = 5。bullet 側に speed が無いので、
    // 撃たれた弾の速さは SrcSpeed(5) をそのまま引き継ぐ
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script1 =
      RecBulletml.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, "2")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, "5"))))
    let (_, _, fc1), st1, w1 =
      Sim.run counting state (Step.fire noResolvers script1 (PFire false) FireContext.zero)
    match w1 with
    | [ Spawn b1 ] -> b1.Speed |> should (equalWithin 0.0001) 5.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w1)
    fc1.SrcSpeed |> should (equalWithin 0.0001) 5.0f
    fc1.SpeedInit |> should equal true
    // bullet 側 speed "5" の 2 回読みのみ。fire 側 "2" は読まれないので 2 のまま
    draws |> should equal 2

    let script2 =
      RecBulletml.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Sequence }, "0")),
                        bullet None None)
    let (_, _, fc2), st2, w2 =
      Sim.run counting st1 (Step.fire noResolvers script2 (PFire false) fc1)
    match w2 with
    | [ Spawn b2 ] -> b2.Speed |> should (equalWithin 0.0001) 5.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w2)
    fc2.SrcSpeed |> should (equalWithin 0.0001) 5.0f
    // 2 発めで fire 側の "0" を 1 回だけ読む（累計 3）
    draws |> should equal 3

    // 3 発め: <fire><speed type="absolute">3</speed><bullet><speed type="absolute">20</speed></bullet></fire>
    // latch は既に立っているので、bullet 側に speed があっても
    // （bSpd.IsSome）採用条件（not latch && bSpd.IsSome）は成立しない。
    // 撃たれた弾自身の速さは bullet 側の 20（bullet が勝つのはここでは
    // latch と無関係）。撃つ側の SrcSpeed は今度は fire 側の "3" を実際に
    // 読んで 3 になる——latch を見ずに「bullet 側があれば常に採用」して
    // しまう写し間違いだと、ここで SrcSpeed が 20 になり、しかも fire 側の
    // "3" の getValue が呼ばれず draws が 1 つ少なくなる
    let script3 =
      RecBulletml.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, "3")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, "20"))))
    let (_, _, fc3), _, w3 =
      Sim.run counting st2 (Step.fire noResolvers script3 (PFire false) fc2)
    match w3 with
    | [ Spawn b3 ] -> b3.Speed |> should (equalWithin 0.0001) 20.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w3)
    fc3.SrcSpeed |> should (equalWithin 0.0001) 3.0f
    // 3 発めで bullet 側 "20" の 2 回読み + fire 側 "3" の 1 回読み = 3（累計 6）
    draws |> should equal 6
