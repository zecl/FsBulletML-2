// このファイルは生成物。手で直すと次の焼き直しで消える。
//
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）のほう。ここは
// その値を DU で直に組んだ形へ写したもので、突き合わせ門の相手として置いてある。
//
// 焼き直し:
//     dotnet build src/FsBulletML2.Bullets.Dsl -c Release
//     dotnet fsi src/FsBulletML2.Bullets.Dsl/gen.fsx

namespace FsBulletML2.Bullets.EnemyBullet
open FsBulletML2

/// BulletML 公式配布（bulletml0_21）のサンプル。
///
/// `All.bullets`（同梱 176 本）には混ぜない —— あちらは
/// 白い弾幕くん由来の集合で、そこに測った数（$rank を使う 173 本 /
/// 狙いを使う 103 本 / 横画面 9 本 …）が全部 紐づいている。
/// 出自の違うものを混ぜると、その数が何の集合の話か分からなくなる。
///
/// `All.official` から引く。 v2.4.1 で「公式と突き合わせる」ために足した。
///
/// 同じ遊びの名前が `EnemyBullet.Sdmkun` にも在るが、中身は別物
/// （あちらは改変版。例: GDarius は repeat が 20 と 12、公式は 8 と 9999）。
///
/// 手で書き写していない。 XML を読んで `SourceWriter` の F# CE で起こした。
/// `template.xml` は入れていない（action も bullet も無い雛形で、
/// 入れると意味の検査に当たる唯一 の本になる）。
[<RequireQualifiedAccess>]
module Official =

  /// [1943]_rolling_fire.xml
  let g1943_rolling_fire =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[1943]_rolling_fire"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "40+$rand*20")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90"), Term (numExpr "4"))
                  Action.ChangeSpeed (Speed (None, numExpr "3"), Term (numExpr "4"))
                  Action.Wait (numExpr "4")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15"), Term (numExpr "9999"))
                  Action.Wait (numExpr "80+$rand*40")
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// [G_DARIUS]_homing_laser.xml
  let g_darius_homing_laser =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[G_DARIUS]_homing_laser"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-60+$rand*120")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hmgLsr"}, [])
                    )
                    Action.Repeat (Times (numExpr "8"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "1")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hmgLsr"}, [])
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "hmgLsr")}, None, Some (Speed (None, numExpr "2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0.3"), Term (numExpr "30"))
                  Action.Wait (numExpr "100")
                  Action.ChangeSpeed (Speed (None, numExpr "5"), Term (numExpr "100"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "60-$rank*20"))
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// [Guwange]_round_2_boss_circle_fire.xml
  let guwange_round_2_boss_circle_fire =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Guwange]_round_2_boss_circle_fire"; bulletmlDescription = None},
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
            ]
          )
        ]
      )

  /// [Guwange]_round_3_boss_fast_3way.xml
  let guwange_round_3_boss_fast_3way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Guwange]_round_3_boss_fast_3way"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "6+$rank*8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["5+$rand*10"])
                    )
                    Action.Wait (numExpr "20")
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
                  Action.Wait (numExpr "1")
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
                        Action.Wait (numExpr "1")
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
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "3way")}, None, Some (Speed (None, numExpr "1.8")),
            []
          )
        ]
      )

  /// [Guwange]_round_4_boss_eye_ball.xml
  let guwange_round_4_boss_eye_ball =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Guwange]_round_4_boss_eye_ball"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4+$rank*4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "eye"}, [])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shadow")}, None, Some (Speed (None, numExpr "0")),
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

  /// [Progear]_round_1_boss_grow_bullets.xml
  let progear_round_1_boss_grow_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_1_boss_grow_bullets"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, Some (Speed (None, numExpr "1.2")),
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
                        Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.15")),
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

  /// [Progear]_round_2_boss_struggling.xml
  let progear_round_2_boss_struggling =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_2_boss_struggling"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "100"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changeStraight"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "160")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changeStraight"}, [])
                    )
                    Action.Wait (numExpr "2")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "changeStraight")}, None, Some (Speed (None, numExpr "0.6")),
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

  /// [Progear]_round_3_boss_back_burst.xml
  let progear_round_3_boss_back_burst =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_3_boss_back_burst"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "100"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "220+$rand*100")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backBurst"}, [])
                    )
                    Action.Wait (numExpr "6-$rank*2")
                  ]
                )
              )
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
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "50+$rand*80")), None,
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
                  Action.Accel (Some (Horizontal (None, numExpr "-7")), None, Term (numExpr "250"))
                ]
              )
            ]
          )
        ]
      )

  /// [Progear]_round_3_boss_wave_bullets.xml
  let progear_round_3_boss_wave_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_3_boss_wave_bullets"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "320")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["-3"])
                    )
                    Action.Wait (numExpr "30")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "220")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["3"])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "wave")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nrm"}, [])
                  )
                  Action.Repeat (Times (numExpr "8+$rank*10"),
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

  /// [Progear]_round_4_boss_fast_rocket.xml
  let progear_round_4_boss_fast_rocket =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_4_boss_fast_rocket"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fireRoot")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rootBl"}, [])
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.4")),
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
                        Action.Wait (numExpr "3")
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

  /// [Progear]_round_5_boss_last_round_wave.xml
  let progear_round_5_boss_last_round_wave =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_5_boss_last_round_wave"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
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
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rfRkt")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "13")), None,
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

  /// [Progear]_round_5_middle_boss_rockets.xml
  let progear_round_5_middle_boss_rockets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_5_middle_boss_rockets"; bulletmlDescription = None},
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
                        Action.Wait (numExpr "20-$rank*8")
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

  /// [Progear]_round_6_boss_parabola_shot.xml
  let progear_round_6_boss_parabola_shot =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "[Progear]_round_6_boss_parabola_shot"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "50"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "190+$rand*30")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
                    )
                    Action.Wait (numExpr "15-$rank*5")
                  ]
                )
              )
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
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "330+$rand*25")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "downAccel"}, [])
                  )
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.4")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "downAccel")}, None, Some (Speed (None, numExpr "2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Accel (None, Some (Vertical (None, numExpr "3")), Term (numExpr "120"))
                ]
              )
            ]
          )
        ]
      )

  /// [Psyvariar]_X-A_boss_opening.xml
  let psyvariar_x_a_boss_opening =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Psyvariar]_X-A_boss_opening"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "100"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-45+$rand*90")), Some (Speed (None, numExpr "0.4+$rand*0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "2")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// [Psyvariar]_X-A_boss_winder.xml
  let psyvariar_x_a_boss_winder =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Psyvariar]_X-A_boss_winder"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "winderBullet")}, None, Some (Speed (None, numExpr "3")),
            []
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "fireWinder")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), None,
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "roundWinder")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "fireWinder"}, ["$1"])
              Action.Repeat (Times (numExpr "11"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "fireWinder"}, ["30"])
                  ]
                )
              )
              Action.Wait (numExpr "5")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "winderSequence")},
            [
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["30"])
              )
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["$1"])
              )
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["30"])
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "2")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "winderSequence"}, ["31"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-2")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "winderSequence"}, ["29"])
            ]
          )
        ]
      )

  /// [Psyvariar]_X-B_colony_shape_satellite.xml
  let psyvariar_x_b_colony_shape_satellite =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[Psyvariar]_X-B_colony_shape_satellite"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "152")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "norm"}, [])
                    )
                    Action.Repeat (Times (numExpr "8"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "7")), None,
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "norm"}, [])
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "8")
                  ]
                )
              )
              Action.Wait (numExpr "10")
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180-45+$rand*90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "norm"}, [])
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (None, numExpr "1.5")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "norm"}, [])
                          )
                          Action.Wait (numExpr "4")
                        ]
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "10")
              Action.Repeat (Times (numExpr "12"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "0")), Some (Speed (None, numExpr "2")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "norm"}, [])
                    )
                    Action.Wait (numExpr "6")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "norm")}, None, Some (Speed (None, numExpr "1")),
            []
          )
        ]
      )

  /// [XEVIOUS]_garu_zakato.xml
  let xevious_garu_zakato =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "[XEVIOUS]_garu_zakato"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "gzc"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "gzc")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10+$rand*10")
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/16")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spr"}, [])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hrmSpr"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "spr")}, None, Some (Speed (None, numExpr "2")),
            []
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "hrmSpr")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "60"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "40"))
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )
