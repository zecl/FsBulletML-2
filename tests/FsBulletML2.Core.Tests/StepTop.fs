namespace FsBulletML2.Core.Tests

open NUnit.Framework
open FsUnit
open FsBulletML2
open FsBulletML2.Domain

/// top* の走査と、1 コマの結果の組み立て。
[<TestFixture>]
type StepTop() =

    let env =
        {
            Rand = (fun () -> 0.5f)
            Rank = 0.5f
            Aim = { ToPlayer = 0.f; ToEnemy = 0.f }
            Spawn = { ToPlayer = 0.f; ToEnemy = 0.f }
        }

    let noResolvers: Step.Resolvers =
        {
            Bullet = (fun _ _ -> None)
            Action = fun _ _ -> None
        }

    let stateWith tops =
        {
            Pos = { X = 0.f; Y = 0.f }
            Speed = 2.f
            Dir = 0.f
            Accel = { X = 1.f; Y = 0.f }
            Kind = BulletType.Enemy
            IsBullet = false
            HasFired = false
            Tops = tops
        }

    let top children =
        let s =
            ActionElm.Action(
                {
                    actionLabel = Some(ActionLabel "top")
                },
                children
            )

        s, Progress.initialActionElm s, FireContext.zero

    [<Test>]
    member _.``差分は、加速度と 速さ かける 向き の和``() =
        let t = top [ Action.Wait(numExpr "10") ]
        let r = Step.step noResolvers env (stateWith [ t ])
        // dir = 0 なので sin 0 = 0、-cos 0 = -1。速さ 2 なので (0, -2)。加速度 (1, 0) を足す
        r.Delta.X |> should (equalWithin 0.0001) 1.0f
        r.Delta.Y |> should (equalWithin 0.0001) -2.0f

    [<Test>]
    member _.``ある top が止まっても、後ろの top は同じフレームで回る``() =
        let t1 = top [ Action.Wait(numExpr "10") ]
        let t2 = top [ Action.Vanish ]
        let r = Step.step noResolvers env (stateWith [ t1; t2 ])
        r.Effects |> should equal [ Vanished ]

    [<Test>]
    member _.``全部の top が終わったら Finished``() =
        let t = top [ Action.Vanish ]
        let r = Step.step noResolvers env (stateWith [ t ])
        r.Finished |> should equal true

    [<Test>]
    member _.``止まっている top があるうちは Finished ではない``() =
        let t = top [ Action.Wait(numExpr "10") ]
        let r = Step.step noResolvers env (stateWith [ t ])
        r.Finished |> should equal false

    [<Test>]
    member _.``撃たれた弾で自分も撃っていれば、終わったときに回収される``() =
        let t = top [ Action.Vanish ]

        let st =
            { stateWith [ t ] with
                IsBullet = true
                HasFired = true
            }

        let r = Step.step noResolvers env st
        r.Retired |> should equal true

    [<Test>]
    member _.``根の弾は、終わっても回収されない``() =
        let t = top [ Action.Vanish ]

        let st =
            { stateWith [ t ] with
                IsBullet = false
                HasFired = true
            }

        let r = Step.step noResolvers env st
        r.Retired |> should equal false

    /// 前の top の効果が後ろより先に出ることを、Spawn と Vanished で確かめる。
    /// 同じ効果 2 つでは逆順でも結果が一致し、この門は働かない。
    [<Test>]
    member _.``複数 top の効果は、top の並び順のまま出る``() =
        let bullet = BulletElm.Bullet({ bulletLabel = None }, None, None, [])
        let fireTop = top [ Action.Fire({ fireLabel = None }, None, None, bullet) ]
        let vanishTop = top [ Action.Vanish ]
        let r = Step.step noResolvers env (stateWith [ fireTop; vanishTop ])

        match r.Effects with
        | [ Spawn _; Vanished ] -> ()
        | other -> Assert.Fail(sprintf "top の並び順で出るはずが %A" other)

    /// ここまでの 6 本はどれも Step.step を 1 回しか呼ばない。
    [<Test>]
    member _.``Progress は次のコマへ持ち越される: wait は 2 コマ目で終わる``() =
        let t = top [ Action.Wait(numExpr "1") ]
        let r1 = Step.step noResolvers env (stateWith [ t ])
        r1.Finished |> should equal false
        let r2 = Step.step noResolvers env r1.State
        r2.Finished |> should equal true

    /// FireContext（SrcSpeed と SpeedInit）も同じ Tops のスロットへ持ち越る。
    [<Test>]
    member _.``FireContext は次のコマへ持ち越される: 2 発めの sequence は 1 発めの速さに積む``() =
        let bullet d s =
            BulletElm.Bullet({ bulletLabel = None }, d, s, [])

        let fire1 =
            Action.Fire(
                { fireLabel = None },
                None,
                None,
                bullet None (Some(Speed(Some { speedType = SpeedType.Absolute }, numExpr "5")))
            )

        let fire2 =
            Action.Fire(
                { fireLabel = None },
                None,
                Some(Speed(Some { speedType = SpeedType.Sequence }, numExpr "3")),
                bullet None None
            )

        let t = top [ fire1; Action.Wait(numExpr "1"); fire2 ]
        let r1 = Step.step noResolvers env (stateWith [ t ])

        match r1.Effects with
        | [ Spawn b1 ] -> b1.Speed |> should (equalWithin 0.0001) 5.0f
        | other -> Assert.Fail(sprintf "1 発めの Spawn のはずが %A" other)

        let r2 = Step.step noResolvers env r1.State

        match r2.Effects with
        | [ Spawn b2 ] -> b2.Speed |> should (equalWithin 0.0001) 8.0f
        | other -> Assert.Fail(sprintf "2 発めの Spawn のはずが %A" other)

    /// times=9999 の repeat を、Step.step 経由で 1 コマ回す。
    [<Test>]
    member _.``top 直下の repeat 9999 も、1 コマで走り切って StackOverflow しない``() =
        let bullet = BulletElm.Bullet({ bulletLabel = None }, None, None, [])

        let fire =
            Action.Fire(
                { fireLabel = None },
                Some(
                    Direction(
                        Some
                            {
                                directionType = DirectionType.Absolute
                            },
                        numExpr "0"
                    )
                ),
                Some(Speed(Some { speedType = SpeedType.Absolute }, numExpr "1")),
                bullet
            )

        let body = ActionElm.Action({ actionLabel = None }, [ fire ])
        let t = top [ Action.Repeat(Times(numExpr "9999"), body) ]
        let r = Step.step noResolvers env (stateWith [ t ])
        r.Effects |> List.length |> should equal 9999
        r.Finished |> should equal true

    /// 生きている top が無いコマは aim を組まない。生きているコマを見る門は緑のままだった。
    [<Test>]
    member _.``終わった top しか無いコマは、aim を読まない``() =
        let poisoned =
            { env with
                Aim = { ToPlayer = 1.25f; ToEnemy = -2.5f }
                Spawn = { ToPlayer = 3.0f; ToEnemy = -0.75f }
            }
        // vanish は 1 コマで終わる。2 コマめが「生きている top が無い」コマ
        let t = top [ Action.Vanish ]
        let first = Step.step noResolvers env (stateWith [ t ])
        first.Finished |> should equal true
        // 前提そのもの: この状態では List.exists (not << isDone) が false
        first.State.Tops
        |> List.exists (fun (_, p, _) -> not (Step.isDone p))
        |> should equal false

        let withZero = Step.step noResolvers env first.State
        let withPoison = Step.step noResolvers poisoned first.State
        withPoison.Delta |> should equal withZero.Delta
        withPoison.State |> should equal withZero.State
        withPoison.Effects |> should equal withZero.Effects
        withPoison.Finished |> should equal withZero.Finished
        withPoison.Retired |> should equal withZero.Retired
