// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Noiz2sa
[<RequireQualifiedAccess>]
module Noiz2sa =

  /// Noiz2saより、88way。 by 白い弾幕くん
  /// [Noiz2sa]_88way.xml
  let b88way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Noiz2saより、88way。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "0.7")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "main"}, [])
                  ]
                )
              )
              Action.Wait (numExpr "200")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "main")},
            [
              Action.Repeat (Times (numExpr "6+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/(6+$rank*10)")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "16way"}, [])
                    )
                    Action.Wait (numExpr "100/(6+$rank*10)")
                  ]
                )
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "16way")}, None, Some (Speed (None, numExpr "$rand+1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20+$rand*40")
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, Some (Speed (None, numExpr "1.7")),
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん
  /// [Noiz2sa]_bit.xml
  let bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "180")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$rand*360"), Term (numExpr "20"))
                        Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "20"))
                        Action.Wait (numExpr "20")
                        Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                        Action.Wait (numExpr "20")
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
                        )
                        Action.Wait (numExpr "0")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*10-5")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nrm"}, [])
                  )
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "6")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nrm"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nrm")}, None, Some (Speed (None, numExpr "2")),
            []
          )
        ]
      )

  /// Noiz2saより、回る棒。by 白い弾幕くん
  /// [Noiz2sa]_rollbar.xml
  let rollbar =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Noiz2saより、回る棒。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*50")), Some (Speed (None, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, [])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "180+$rank*50")), Some (Speed (None, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, [])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "main")},
            [
              Action.Repeat (Times (numExpr "15+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "firebar"}, ["90"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "160")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "firebar"}, ["-90"])
                    )
                    Action.Wait (numExpr "200/(15+$rank*10)")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "firebar")}, None, Some (Speed (None, numExpr "10")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, Some (Speed (None, numExpr "1.5")),
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )
