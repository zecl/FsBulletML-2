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
/// Progear
[<RequireQualifiedAccess>]
module Progear =

  /// CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん
  /// [Progear]_round_1_boss_grow_bullets.xml
  let round_1_boss_grow_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "oogi")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270-(4+$rank*6)*15/2")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
              )
              Action.Repeat (Times (numExpr "4+$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "oogi"}, [])
                    Action.Wait (numExpr "40")
                  ]
                )
              )
              Action.Wait (numExpr "40")
              Action.Repeat (Times (numExpr "8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "oogi"}, [])
                    Action.Wait (numExpr "20")
                  ]
                )
              )
              Action.Wait (numExpr "30")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, Some (Speed (None, numExpr "1.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "60"))
                  Action.Wait (numExpr "60")
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0.75")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "4+$rank*4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.3")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
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

  /// CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん
  /// [Progear]_round_2_boss_struggling.xml
  let round_2_boss_struggling =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "1000"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changeStraight"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "159")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changeStraight"}, [])
                    )
                    Action.Wait (numExpr "1+(1-$rank)*3*$rand")
                  ]
                )
              )
              Action.Wait (numExpr "180")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "changeStraight")}, None, Some (Speed (None, numExpr "0.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20+$rand*100")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270"), Term (numExpr "60"))
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "40"))
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "0.5+$rand*0.7"), Term (numExpr "20"))
                ]
              )
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_back_burst.xml
  let round_3_boss_back_burst =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "200"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "220+$rand*100")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backBurst"}, [])
                    )
                    Action.Wait (numExpr "4-$rank*2")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "backBurst")}, None, Some (Speed (None, numExpr "1.2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "80"))
                  Action.Wait (numExpr "60+$rand*20")
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60+$rand*60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "downAccel")}, None, Some (Speed (None, numExpr "1.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Relative}, numExpr "-7")), None, Term (numExpr "250"))
                ]
              )
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_wave_bullets.xml
  let round_3_boss_wave_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "310")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["-3"])
                    )
                    Action.Wait (numExpr "30")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "230")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["3"])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "wave")}, None, Some (Speed (None, numExpr "1.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nrm"}, [])
                  )
                  Action.Repeat (Times (numExpr "12+$rank*12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nrm"}, [])
                        )
                        Action.Wait (numExpr "3")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nrm")}, None, Some (Speed (None, numExpr "1")),
            []
          )
        ]
      )

  /// CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん
  /// [Progear]_round_4_boss_fast_rocket.xml
  let round_4_boss_fast_rocket =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fireRoot")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rootBl"}, [])
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rootBl"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fireRoot"}, ["$rand*16"])
              Action.ActionRef ({actionRefLabel = ActionLabel "fireRoot"}, ["180+$rand*16"])
              Action.Wait (numExpr "120")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rootBl")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "40")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "274+$rand*4")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rocket"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rocket")}, None, Some (Speed (None, numExpr "5+$rand")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (None, numExpr "1.8")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, [])
                        )
                        Action.Wait (numExpr "5-$rank*4")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "downAccel")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Accel (None, Some (Vertical (None, numExpr "2.7")), Term (numExpr "120"))
                ]
              )
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_boss_last_round_wave.xml
  let round_5_boss_last_round_wave =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "2+$rank*1.5"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, None, None,
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rfRkt"}, [])
                          )
                          Action.Wait (numExpr "45")
                        ]
                      )
                    )
                    Action.Wait (numExpr "100")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rfRkt")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "2")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_middle_boss_rockets.xml
  let round_5_middle_boss_rockets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "50"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rocket"}, [])
                    )
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rocket")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.FireRef ({fireRefLabel = FireLabel "udBlt"}, ["90"])
                        Action.Wait (numExpr "20-$rank*8")
                        Action.FireRef ({fireRefLabel = FireLabel "udBlt"}, ["-90"])
                        Action.Wait (numExpr "$rand*10+15-$rank*8")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "udBlt")}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1-25+$rand*50")), None,
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん
  /// [Progear]_round_6_boss_parabola_shot.xml
  let round_6_boss_parabola_shot =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "25"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "190+$rand*30")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["1"])
                    )
                    Action.Wait (numExpr "15-$rank*5")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "350-$rand*30")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["-1"])
                    )
                    Action.Wait (numExpr "15-$rank*5")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "60"))
                  Action.Wait (numExpr "60")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270+30*$1+$rand*50*$1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, ["$1"])
                  )
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.4")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, ["$1"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "downAccel")}, None, Some (Speed (None, numExpr "2.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Accel (None, Some (Vertical (None, numExpr "4*$1")), Term (numExpr "120"))
                ]
              )
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん
  /// [Progear]_round_9_boss.xml
  let round_9_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "accel")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.03"), Term (numExpr "9999"))
                  Action.Wait (numExpr "9999")
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "80")), None,
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "20")
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "40")), Some (Speed (None, numExpr "5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Repeat (Times (numExpr "9999"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0.5")),
                                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "accel"}, [])
                                    )
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "0.5")),
                                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "accel"}, [])
                                    )
                                    Action.Wait (numExpr "4-$rank*2+$rand")
                                  ]
                                )
                              )
                            ]
                          )
                        ]
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
        ]
      )

  /// CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん
  /// [Progear]_round_10_boss_before_final.xml
  let round_10_boss_before_final =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "rollOut")}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (None, numExpr "0.0001")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "350")
                    Action.ChangeSpeed (Speed (None, numExpr "1"), Term (numExpr "100"))
                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "50-$rank*40"), Term (numExpr "100"))
                    Action.Wait (numExpr "1000")
                  ]
                )
              ]
            )
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "setter")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "5")
                        Action.FireRef ({fireRefLabel = FireLabel "rollOut"}, [])
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$rand*10")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "setter"}, [])
              )
              Action.Repeat (Times (numExpr "45/(2-$rank)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "16-$rank*8")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "setter"}, [])
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
              Action.Wait (numExpr "40")
              Action.Repeat (Times (numExpr "125+$rank*125"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "1.5-$rank/2+$rand")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "45-$rand*90")), Some (Speed (None, numExpr "1.2")),
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
              Action.Wait (numExpr "80")
              Action.ChangeSpeed (Speed (None, numExpr "0.7"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "1")
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "1.44444"), Term (numExpr "250"))
              Action.Wait (numExpr "250")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "20")
              Action.ChangeSpeed (Speed (None, numExpr "0.7"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30"), Term (numExpr "12"))
              Action.Wait (numExpr "12")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "200-$rank*60")
            ]
          )
        ]
      )
