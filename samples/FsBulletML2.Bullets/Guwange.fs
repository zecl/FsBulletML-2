// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Guwange
[<RequireQualifiedAccess>]
module Guwange =

  /// ぐわんげ、二面ボス by 白い弾幕くん
  /// [Guwange]_round_2_boss_circle_fire.xml
  let round_2_boss_circle_fire =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ぐわんげ、二面ボス by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "circle")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "6")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "3")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "1.5+$rank")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fireCircle")},
            [
              Action.Repeat (Times (numExpr "18"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "circle"}, ["20"; "$1"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fireCircle"}, ["180-45+90*$rand"])
              Action.Wait (numExpr "10")
            ]
          )
        ]
      )

  /// ぐわんげ、三面ボス by 白い弾幕くん
  /// [Guwange]_round_3_boss_fast_3way.xml
  let round_3_boss_fast_3way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ぐわんげ、三面ボス by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10+$rank*50"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["5+$rand*10"])
                    )
                    Action.Wait (numExpr "20-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "$1"))
                  Action.Wait (numExpr "$1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-20")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "6")
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                        )
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-20")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "6")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                        )
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "3way"}, [])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "6")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "3way")}, None, Some (Speed (None, numExpr "3")),
            []
          )
        ]
      )

  /// ぐわんげ、四面ボス by 白い弾幕くん
  /// [Guwange]_round_4_boss_eye_ball.xml
  let round_4_boss_eye_ball =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ぐわんげ、四面ボス by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "eye"}, [])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "eye")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "10"), Term (numExpr "400"))
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$rand*5-2"), Term (numExpr "9999"))
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shadow"}, [])
                        )
                        Action.Wait (numExpr "4")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shadow")}, None, Some (Speed (None, numExpr "0.1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (None, numExpr "0.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (None, numExpr "0.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )
