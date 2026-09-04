// **このファイルは生成物。手で直すと次の焼き直しで消える。**
//
// 人が書くのは samples/FsBulletML2.Bullets.Dsl（CE）のほう。ここは
// その値を DU で直に組んだ形へ写したもので、突き合わせ門の相手として置いてある。
//
// 焼き直し:
//     dotnet build samples/FsBulletML2.Bullets.Dsl -c Release
//     dotnet fsi samples/FsBulletML2.Bullets.Dsl/gen.fsx

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// GWange
[<RequireQualifiedAccess>]
module GWange =

  /// G-わんげスレの957氏、回転ガラ by 白い弾幕くん
  /// [G-Wange]_roll_gara.xml
  let _roll_gara =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "G-わんげスレの957氏、回転ガラ by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "600/(3-$rank*2)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "line"}, [])
                    Action.Wait (numExpr "3-$rank*2+$rand")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "line")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-7")), Some (Speed (None, numExpr "0.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "5+$rank*5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-1+$rand*2"), Term (numExpr "30"))
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "(-1+$rand*2)*($rank*2+1)"), Term (numExpr "30"))
                    Action.Wait (numExpr "30")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// G-わんげスレの966氏考案、往復ビット by 白い弾幕くん
  /// [G-Wange]_round_trip_bit.xml
  let round_trip_bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "G-わんげスレの966氏考案、往復ビット by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "src"}, ["5"; "91"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "src"}, ["4"; "-91"])
              )
              Action.Wait (numExpr "600")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Xway")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-(5+$rank*5)*($1-1)-4+$rand*8")), Some (Speed (None, numExpr "1.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5+$rank*5")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fire")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "Xway"}, ["3"])
              Action.Wait (numExpr "15")
              Action.ActionRef ({actionRefLabel = ActionLabel "Xway"}, ["5"])
              Action.Wait (numExpr "15")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "src")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "$1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeSpeed (Speed (None, numExpr "0.01"), Term (numExpr "30"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, [])
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-$2"), Term (numExpr "1"))
                        Action.ChangeSpeed (Speed (None, numExpr "$1"), Term (numExpr "30"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, [])
                        Action.ChangeSpeed (Speed (None, numExpr "0.01"), Term (numExpr "30"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, [])
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2"), Term (numExpr "1"))
                        Action.ChangeSpeed (Speed (None, numExpr "$1"), Term (numExpr "30"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, [])
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )
