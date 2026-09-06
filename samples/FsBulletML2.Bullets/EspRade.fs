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
/// EspRade
[<RequireQualifiedAccess>]
module EspRade =

  /// エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん
  /// [ESP_RADE]_round_5_alice_clone.xml
  let round_5_alice_clone =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "alice")}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "8")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10*$rand")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*30-15")), None,
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "600"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "alice"}, [])
                    Action.Wait (numExpr "$rank+1+$rand")
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
        ]
      )

  /// エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_ares_2.xml
  let round_5_boss_ares_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "XWay")},
            [
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aim3")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim3Impl"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aim3Impl")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-33+$rand*6")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "30"])
                        Action.Repeat (Times (numExpr "2+$rank*3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "3")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-60")), Some (Speed (None, numExpr "1.5")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "30"])
                            ]
                          )
                        )
                        Action.Wait (numExpr "54-$rank*9")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aim")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimImpl"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aimImpl")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3+$rand*6")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "2+$rank*3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "3")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (None, numExpr "1.5")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "54-$rank*9")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "fan")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "3+$rank*4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1-$2*3")), Some (Speed (None, numExpr "$3")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["7"; "10"])
                        Action.Wait (numExpr "420/(3+$rank*4)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "110")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim3"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-110")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim3"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "125")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-125")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "150")), Some (Speed (None, numExpr "7")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-150")), Some (Speed (None, numExpr "7")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim"}, [])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["-135"; "10"; "1.3"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["135"; "10"; "1.3"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "110")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["-164"; "8"; "1.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-110")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["156"; "8"; "1.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "130")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["180"; "8"; "1.1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-130")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["180"; "5"; "1.1"])
              )
              Action.Wait (numExpr "430")
            ]
          )
        ]
      )

  /// エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_1_a.xml
  let round_5_boss_gara_1_a =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "sequenceThree")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "12")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "sequenceTwo"}, [])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "sequenceTwo")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "oogi")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-90")), Some (Speed (None, numExpr "1.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "sequenceTwo"}, [])
              Action.Repeat (Times (numExpr "11"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "sequenceThree"}, [])
              )
              Action.Wait (numExpr "10")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "oogiOuHuku")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-213")), Some (Speed (None, numExpr "1.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "sequenceTwo"}, [])
              Action.Repeat (Times (numExpr "11"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "sequenceThree"}, [])
              )
              Action.Wait (numExpr "10")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "gara1a")},
            [
              Action.Repeat (Times (numExpr "5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ChangeDirection (Direction (None, numExpr "360*$rand"), Term (numExpr "1"))
                    Action.ChangeSpeed (Speed (None, numExpr "0.5*$rand+0.5"), Term (numExpr "1"))
                    Action.ActionRef ({actionRefLabel = ActionLabel "oogi"}, [])
                    Action.Repeat (Times (numExpr "$rand*(3+$rank*2)+1+$rank*2"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ActionRef ({actionRefLabel = ActionLabel "oogiOuHuku"}, [])
                        ]
                      )
                    )
                    Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                    Action.Wait (numExpr "50")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "gara1a"}, [])
            ]
          )
        ]
      )

  /// エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_1_b.xml
  let round_5_boss_gara_1_b =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "8way2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180-75")), Some (Speed (None, numExpr "4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "9")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.25")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+75")), Some (Speed (None, numExpr "4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-9")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.25")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "downShot")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-60+$rand*120")), Some (Speed (None, numExpr "4*$rand")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "20")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1.2")),
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
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "gara")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "10+$rand*340"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.3"), Term (numExpr "1"))
              Action.Repeat (Times (numExpr "3+$rank*4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "8"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ActionRef ({actionRefLabel = ActionLabel "downShot"}, [])
                          Action.Wait (numExpr "3*(3-$rank*2)*$rand")
                        ]
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "8way2"}, [])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "5"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "gara"}, [])
              )
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "30")
            ]
          )
        ]
      )

  /// エスプレイド、ガラ婦人第二形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_2.xml
  let round_5_boss_gara_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ婦人第二形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "featherShot")}, None, Some (Speed (None, numExpr "6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                  Action.Wait (numExpr "20")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAim"}, [])
                  )
                  Action.Repeat (Times (numExpr "150+$rank*100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "90*$rand-45")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "3-$rank*2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "featherAim")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "3")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "20"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "2")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (None, numExpr "3")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "30")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
              )
              Action.Wait (numExpr "550")
            ]
          )
        ]
      )

  /// エスプレイド、ガラ第三形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_3.xml
  let round_5_boss_gara_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ第三形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "stop")},
            [
              Action.Wait (numExpr "15")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "1")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "featherAllWay")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$2*(180-(10-$1)*60)")), Some (Speed (None, numExpr "0")),
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
                  Action.ActionRef ({actionRefLabel = ActionLabel "stop"}, [])
                  Action.Repeat (Times (numExpr "40"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2*(7-$1)*$2")), Some (Speed (None, numExpr "0.9+0.2*(6-$1)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "$1-1"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2*$2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "15")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "featherAim")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ActionRef ({actionRefLabel = ActionLabel "stop"}, [])
                  Action.Repeat (Times (numExpr "10+$rank*20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3")), Some (Speed (None, numExpr "1.2")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            [
                              ActionElm.Action ({actionLabel = None},
                                []
                              )
                            ]
                          )
                        )
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  [
                                    ActionElm.Action ({actionLabel = None},
                                      []
                                    )
                                  ]
                                )
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "40-$rank*20")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "1")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "1")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "70")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-70")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "100")), Some (Speed (None, numExpr "1.8")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["3"; "1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-100")), Some (Speed (None, numExpr "1.8")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["3"; "-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["4"; "1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["4"; "-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "85")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["5"; "1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-85")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["5"; "-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "72")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["6"; "1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-72")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherAllWay"}, ["6"; "-1"])
              )
              Action.Wait (numExpr "700")
            ]
          )
        ]
      )

  /// エスプレイド、ガラ婦人第四形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_4.xml
  let round_5_boss_gara_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ婦人第四形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "featherShot")}, None, Some (Speed (None, numExpr "7")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                  Action.Wait (numExpr "20")
                  Action.Repeat (Times (numExpr "50"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "20*$rand-10")), Some (Speed (None, numExpr "2*$rand+0.7")),
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
                    )
                    Action.Wait (numExpr "30+$rank*30")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
                    )
                    Action.Wait (numExpr "30+$rank*30")
                  ]
                )
              )
              Action.Wait (numExpr "120")
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
                    )
                    Action.Wait (numExpr "40-$rank*20")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "featherShot"}, [])
                    )
                    Action.Wait (numExpr "40-$rank*20")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
        ]
      )

  /// エスプレイド、ガラ婦人最終形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_5.xml
  let round_5_boss_gara_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、ガラ婦人最終形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "accel")},
            [
              Action.ChangeDirection (Direction (None, numExpr "360*$rand"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (None, numExpr "0.5+$rand*0.5"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "stopAndWait")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "stop"}, [])
              Action.Wait (numExpr "70-$rank*50")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "ippon")},
            [
              Action.Repeat (Times (numExpr "26"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.12")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "murasaki")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "0.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "3+$rand*17"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "ippon"}, [])
                    Action.Wait (numExpr "6")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "ao")},
            [
              Action.Repeat (Times (numExpr "3+$rand*17"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "ippon"}, [])
                    Action.Wait (numExpr "6")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "gara5")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "accel"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "murasaki"}, ["5"; "180-$rand*90"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "stopAndWait"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "accel"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "ao"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "stopAndWait"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "accel"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "murasaki"}, ["-5"; "180+$rand*90"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "stopAndWait"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "accel"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "ao"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "stopAndWait"}, [])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "gara5"}, [])
            ]
          )
        ]
      )

  /// エスプレイド、五行覚師、発狂。by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_kakusi_hakkyou.xml
  let round_5_boss_kakusi_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、五行覚師、発狂。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "6shots")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "3")
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-15+30*$rand")), Some (Speed (None, numExpr "0.8+$rank+$rand")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-45+30*$rand")), Some (Speed (None, numExpr "0.8+$rank+$rand")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "15+30*$rand")), Some (Speed (None, numExpr "0.8+$rank+$rand")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "kakusi")}, None, Some (Speed (None, numExpr "6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "10"))
                  Action.Wait (numExpr "10")
                  Action.Repeat (Times (numExpr "4+$rank*6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "6shots"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "6shots"}, [])
                        )
                        Action.Wait (numExpr "200/(4+$rank*6)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kakusi"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kakusi"}, [])
              )
              Action.Wait (numExpr "200")
            ]
          )
        ]
      )

  /// エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_izuna_hakkyou.xml
  let round_123_boss_izuna_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Red")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                []
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "XWay")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["$1"; "$2"; "0"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "XWayFan")},
            [
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "$3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90*$1")), Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "1.5+$rank")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
                  )
                  Action.Repeat (Times (numExpr "10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "8")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5.3*$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.3-$rank*0.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["8"; "45"])
                        Action.Repeat (Times (numExpr "5"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "8")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3*$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.06+$rank*0.1")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["8"; "45"])
                            ]
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10")
                        Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                        Action.ActionRef ({actionRefLabel = ActionLabel "aim"}, [])
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.Wait (numExpr "500")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "aim")},
            [
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "50")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-1")), Some (Speed (None, numExpr "1.7")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                    )
                    Action.Repeat (Times (numExpr "2+$rank*6"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                          )
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                          )
                        ]
                      )
                    )
                  ]
                )
              )
            ]
          )
        ]
      )

  /// エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_pelaboy_hakkyou.xml
  let round_123_boss_pelaboy_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Red")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                []
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "XWay")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["$1"; "$2"; "0"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "XWayFan")},
            [
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "$3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "subBatteryFan")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Wait (numExpr "250")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-45")), Some (Speed (None, numExpr "1.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["10+$rank*10"; "90/(10+$rank*10)"])
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "5")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-90")), Some (Speed (None, numExpr "1.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["11+$rank*10"; "90/(10+$rank*10)"])
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aimFan")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-4-$rank*8")), Some (Speed (None, numExpr "1.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["5+$rank*8"; "2"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "subBatteryAim")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "100+$rand*50")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimFan"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimFan"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "soldier")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90*$1")), Some (Speed (None, numExpr "2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
                  )
                  Action.Repeat (Times (numExpr "120+$rank*200"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "440/(120+$rank*200)+$rand")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "17*$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "soldier"}, ["1"; "90"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "soldier"}, ["-1"; "-80"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "subBatteryAim"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "subBatteryFan"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "subBatteryFan"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            [
                              ActionElm.Action ({actionLabel = None},
                                [
                                  Action.ActionRef ({actionRefLabel = ActionLabel "mainBattery"}, [])
                                  Action.Vanish
                                ]
                              )
                            ]
                          )
                        )
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.Wait (numExpr "500")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "mainBattery")},
            [
              Action.Repeat (Times (numExpr "15"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "8")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "195")
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "88+$rand*4")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["12+$rank*16"; "180/(12+$rank*16)"])
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "93+$rand*4")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["11+$rank*16"; "170/(11+$rank*16)"])
                  ]
                )
              )
              Action.Wait (numExpr "40")
            ]
          )
        ]
      )

  /// エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_satoru_5way.xml
  let round_123_boss_satoru_5way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "1way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$2+$1*$rand*2-$1")), Some (Speed (None, numExpr "1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$2+$1*$rand*2-$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "5way")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "1way"}, ["$1"; "-30"])
              Action.ActionRef ({actionRefLabel = ActionLabel "1way"}, ["$1"; "-15"])
              Action.ActionRef ({actionRefLabel = ActionLabel "1way"}, ["$1"; "0"])
              Action.ActionRef ({actionRefLabel = ActionLabel "1way"}, ["$1"; "15"])
              Action.ActionRef ({actionRefLabel = ActionLabel "1way"}, ["$1"; "30"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "idousite5way")},
            [
              Action.ChangeDirection (Direction (None, numExpr "$rand*360"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "1"))
              Action.Wait (numExpr "30")
              Action.ActionRef ({actionRefLabel = ActionLabel "5way"}, ["$1"])
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "90-$rank*60")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "satoru")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "idousite5way"}, ["1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "idousite5way"}, ["2"])
              Action.ActionRef ({actionRefLabel = ActionLabel "idousite5way"}, ["3"])
              Action.ActionRef ({actionRefLabel = ActionLabel "idousite5way"}, ["4"])
              Action.ActionRef ({actionRefLabel = ActionLabel "idousite5way"}, ["5"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "satoru"}, [])
              Action.Wait (numExpr "30")
            ]
          )
        ]
      )
