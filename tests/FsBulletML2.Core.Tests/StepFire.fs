namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// fire は撃つ側の状態と撃たれた弾の両方を決める。
/// bullet の中に書いた値が fire 側より勝つところが、いちばん間違えやすい。
[<TestFixture>]
type StepFire() =

  // 4 つとも別の値にしてある。同じ値にすると、fire 側と bullet 側で
  // 基準を取り違えていても門が緑のまま通る
  let env = { Rand = (fun () -> 0.5f); Rank = 0.5f; Aim = { ToPlayer = 1.0f; ToEnemy = 2.0f }; Spawn = { ToPlayer = 3.0f; ToEnemy = 4.0f } }

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
    BulletElm.Bullet ({ bulletLabel = None }, d, s, [])

  [<Test>]
  member _.``fire は、撃たれた弾を 1 つ出す``() =
    let script =
      Action.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Absolute }, numExpr "0")),
                        Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "2")),
                        bullet None None)
    let (r, _, _), _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
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
      Action.Fire ({ fireLabel = None }, None, None, bullet None None)
    let (_, _, _), st, _ = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    st.HasFired |> should equal true

  [<Test>]
  member _.``bullet の中の speed が fire 側より勝つ``() =
    let script =
      Action.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "2")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "9"))))
    let _, _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Speed |> should (equalWithin 0.0001) 9.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``bullet の中の direction が fire 側より勝つ``() =
    let script =
      Action.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Absolute }, numExpr "0")),
                        None,
                        bullet (Some (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"))) None)
    let _, _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) (float32 (System.Math.PI / 2.0))
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``direction を省くと aim になる``() =
    let script = Action.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) env.Aim.ToPlayer
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// bullet 側の aim は、stepFire の中では解決できない。
  [<Test>]
  member _.``bullet 側の aim は、産まれる弾の位置から見た向きで解決する``() =
    let script =
      Action.Fire ({ fireLabel = None }, None, None,
                        bullet (Some (Direction (Some { directionType = DirectionType.Aim }, numExpr "30"))) None)
    let _, _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] ->
        // 30 度 = π/6 に env.Spawn.ToPlayer（3.0）が足された値。
        b.Dir |> should (equalWithin 0.0001) (Step.calcDir (env.Spawn.ToPlayer + float32 (System.Math.PI / 6.0)))
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// 対照: fire 側の aim（bullet 側は無指定）は撃った側の env.Aim.ToPlayer を使う。
  /// bullet 側と基準が違うことを、値が違うことで示す
  [<Test>]
  member _.``fire 側の aim は、撃った側の位置から見た向きで解決する``() =
    let script = Action.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env state (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] ->
        b.Dir |> should (equalWithin 0.0001) env.Aim.ToPlayer
        // 産まれる弾の側の値（3.0）ではないこと。両者が同じ値だと
        // 取り違えても緑になるので、違うことを明示で見る
        b.Dir |> should not' (equalWithin 0.0001 env.Spawn.ToPlayer)
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``sequence は、直前の fire の値に積む``() =
    let script =
      Action.Fire ({ fireLabel = None },
                        Some (Direction (Some { directionType = DirectionType.Sequence }, numExpr "10")),
                        None,
                        bullet None None)
    let fc = { FireContext.zero with SrcDir = 1.0f }
    let (_, _, fc'), _, _ = Sim.run env state (stepFire noResolvers script (PFire false) fc)
    // 10 度 = π/18 を足す
    fc'.SrcDir |> should (equalWithin 0.0001) (1.0f + float32 (System.Math.PI / 18.0))

  [<Test>]
  member _.``bulletRef は 1 段だけ解く``() =
    let target = bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "7")))
    let resolvers : Step.Resolvers =
      { Bullet = (fun label _ -> if label = BulletLabel "b1" then Some target else None)
        Action = fun _ _ -> None }
    let script =
      Action.Fire ({ fireLabel = None }, None, None,
                        BulletElm.BulletRef ({ bulletRefLabel = BulletLabel "b1" }, []))
    let _, _, w = Sim.run env state (stepFire resolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Speed |> should (equalWithin 0.0001) 7.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  /// final review 5: 5 つめの draw site（設計文書 5.3 参照）。
  [<Test>]
  member _.``bulletRef を解いた瞬間にも、bullet 本体の中の wait を先に引く（resetChild と合わせて 2 回）``() =
    let target =
      BulletElm.Bullet ({ bulletLabel = None }, None, None,
                          [ ActionElm.Action ({ actionLabel = None }, [ Action.Wait (numExpr "5") ]) ])
    let resolvers : Step.Resolvers =
      { Bullet = (fun label _ -> if label = BulletLabel "b1" then Some target else None)
        Action = fun _ _ -> None }
    let script =
      Action.Fire ({ fireLabel = None }, None, None,
                        BulletElm.BulletRef ({ bulletRefLabel = BulletLabel "b1" }, []))
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let _, _, w = Sim.run counting state (stepFire resolvers script (PFire false) FireContext.zero)
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
  member _.``撃つ側が Player なら、direction を省くと Aim.ToEnemy になる``() =
    // aim = if self.Kind = Player then Aim.ToEnemy else Aim.ToPlayer。
    let playerState = { state with Kind = BulletType.Player }
    let script = Action.Fire ({ fireLabel = None }, None, None, bullet None None)
    let _, _, w = Sim.run env playerState (stepFire noResolvers script (PFire false) FireContext.zero)
    match w with
    | [ Spawn b ] -> b.Dir |> should (equalWithin 0.0001) env.Aim.ToEnemy
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w)

  [<Test>]
  member _.``fire は、撃たれた弾の action の中身の getValue も先に引く``() =
    // Progress.initial で組むと乱数消費が消え、fire の直後から乱数列がずれる。
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let bulletBody =
      BulletElm.Bullet ({ bulletLabel = None }, None, None,
                          [ ActionElm.Action ({ actionLabel = None },
                              [ Action.Wait (numExpr "3")
                                Action.ChangeDirection (Direction (Some { directionType = DirectionType.Absolute }, numExpr "90"), Term (numExpr "2"))
                                Action.ChangeSpeed (Speed (Some { speedType = SpeedType.Absolute }, numExpr "5"), Term (numExpr "4")) ]) ])
    let script = Action.Fire ({ fireLabel = None }, None, None, bulletBody)
    Sim.run counting state (stepFire noResolvers script (PFire false) FireContext.zero) |> ignore
    draws |> should equal 3

  [<Test>]
  member _.``bullet の speed は、$rand を含まない定数式でも旧と同じく 2 回 getValue を読む``() =
    // getValue は式の中身に関わらず env.Rand () を呼ぶ。定数式でも呼び出しは消さない。
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script =
      Action.Fire ({ fireLabel = None }, None, None,
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "9"))))
    Sim.run counting state (stepFire noResolvers script (PFire false) FireContext.zero) |> ignore
    draws |> should equal 2

  [<Test>]
  member _.``撃つ側の SrcSpeed は、latch が立つまでは bullet 側の速さを引き継ぎ、fire 側の値は読まれない``() =
    // 1 発めは latch が立つまで、bullet 側の速さ (5) を SrcSpeed にする。
    // その間、fire 側の speed は getValue すら呼ばれない。
    let mutable draws = 0
    let counting = { env with Rand = fun () -> draws <- draws + 1; 0.5f }
    let script1 =
      Action.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "2")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "5"))))
    let (_, _, fc1), st1, w1 =
      Sim.run counting state (stepFire noResolvers script1 (PFire false) FireContext.zero)
    match w1 with
    | [ Spawn b1 ] -> b1.Speed |> should (equalWithin 0.0001) 5.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w1)
    fc1.SrcSpeed |> should (equalWithin 0.0001) 5.0f
    fc1.SpeedInit |> should equal true
    // bullet 側 speed "5" の 2 回読みのみ。fire 側 "2" は読まれないので 2 のまま
    draws |> should equal 2

    let script2 =
      Action.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Sequence }, numExpr "0")),
                        bullet None None)
    let (_, _, fc2), st2, w2 =
      Sim.run counting st1 (stepFire noResolvers script2 (PFire false) fc1)
    match w2 with
    | [ Spawn b2 ] -> b2.Speed |> should (equalWithin 0.0001) 5.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w2)
    fc2.SrcSpeed |> should (equalWithin 0.0001) 5.0f
    // 2 発めで fire 側の "0" を 1 回だけ読む（累計 3）
    draws |> should equal 3

    // 3 発め: <fire><speed type="absolute">3</speed><bullet><speed type="absolute">20</speed></bullet></fire>
    // latch は既に立っているので、bullet 側に speed があっても
    // （bSpd.IsSome）採用条件（not latch && bSpd.IsSome）は成立しない。
    let script3 =
      Action.Fire ({ fireLabel = None },
                        None,
                        Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "3")),
                        bullet None (Some (Speed (Some { speedType = SpeedType.Absolute }, numExpr "20"))))
    let (_, _, fc3), _, w3 =
      Sim.run counting st2 (stepFire noResolvers script3 (PFire false) fc2)
    match w3 with
    | [ Spawn b3 ] -> b3.Speed |> should (equalWithin 0.0001) 20.0f
    | _ -> Assert.Fail (sprintf "Spawn 1 つのはずが %A" w3)
    fc3.SrcSpeed |> should (equalWithin 0.0001) 3.0f
    // 3 発めで bullet 側 "20" の 2 回読み + fire 側 "3" の 1 回読み = 3（累計 6）
    draws |> should equal 6
