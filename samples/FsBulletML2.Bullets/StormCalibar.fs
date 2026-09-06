// **このファイルは生成物。手で直すと次の焼き直しで消える。**
//
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）のほう。ここは
// その値を DU で直に組んだ形へ写したもので、突き合わせ門の相手として置いてある。
//
// 焼き直し:
//     dotnet build src/FsBulletML2.Bullets.Dsl -c Release
//     dotnet fsi src/FsBulletML2.Bullets.Dsl/gen.fsx

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// StormCalibar
[<RequireQualifiedAccess>]
module StormCalibar =

  /// ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん
  /// [STORM_CALIBAR]_last_boss_double_roll_bullets.xml
  let last_boss_double_roll_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "rollShots")},
            [
              Action.Repeat (Times (numExpr "200"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "11*$1")), Some (Speed (None, numExpr "1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Repeat (Times (numExpr "3+$rank*4"),
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
                    Action.Wait (numExpr "2")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "right")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5"), Term (numExpr "1"))
              Action.Wait (numExpr "50")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "left")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5"), Term (numExpr "1"))
              Action.Wait (numExpr "50")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                  ]
                )
              )
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "1")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "rollShots"}, ["-1"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "rollShots"}, ["1"])
            ]
          )
        ]
      )
