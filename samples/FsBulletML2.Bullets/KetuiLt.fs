// このファイルは生成物。手で直すと次の焼き直しで消える。
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
/// KetuiLt
[<RequireQualifiedAccess>]
module KetuiLt =

  /// ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん
  /// [Ketui_LT]_1boss_bit.xml
  let b1boss_bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "3way")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "30")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3")), Some (Speed (None, numExpr "1.4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "2"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Absolute}, numExpr "0")), Some (Vertical (Some {verticalType = VerticalType.Absolute}, numExpr "1")), Term (numExpr "60"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, [])
                        Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Absolute}, numExpr "-2")), Some (Vertical (Some {verticalType = VerticalType.Absolute}, numExpr "0")), Term (numExpr "60"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, [])
                        Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Absolute}, numExpr "0")), Some (Vertical (Some {verticalType = VerticalType.Absolute}, numExpr "-1")), Term (numExpr "60"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, [])
                        Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Absolute}, numExpr "2")), Some (Vertical (Some {verticalType = VerticalType.Absolute}, numExpr "0")), Term (numExpr "60"))
                        Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, [])
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4+$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "2")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                    )
                    Action.Wait (numExpr "245/(4+$rank*6)")
                  ]
                )
              )
              Action.Wait (numExpr "550")
            ]
          )
        ]
      )

  /// ケツイロケテより、三ボスのくねくね by 白い弾幕くん
  /// [Ketui_LT]_3boss_kunekune.xml
  let b3boss_kunekune =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ケツイロケテより、三ボスのくねくね by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aimSrc")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Repeat (Times (numExpr "5+$rank*10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "340/(5+$rank*10)")
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "2")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "2")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "circleSrc")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0.5+$rank"), Term (numExpr "1"))
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5"), Term (numExpr "9999"))
                  Action.Repeat (Times (numExpr "200"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "2")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3+$rand*0.02")),
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
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "circleSrc"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "circleSrc"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimSrc"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-135")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimSrc"}, [])
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "12-$rank*8")
                    Action.Repeat (Times (numExpr "4+$rank*4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "2")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "4")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              [
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Wait (numExpr "6")
                                    Action.ChangeSpeed (Speed (None, numExpr "1"), Term (numExpr "5"))
                                    Action.Wait (numExpr "20")
                                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
                                    Action.ChangeSpeed (Speed (None, numExpr "2.2"), Term (numExpr "1"))
                                  ]
                                )
                              ]
                            )
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

  /// ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん
  /// [Ketui_LT]_3boss_roll_and_aim.xml
  let b3boss_roll_and_aim =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "curve")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-$1*(4+$rank*$rank*4)"), Term (numExpr "10"))
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "spiral")},
            [
              Action.Wait (numExpr "$rand * 30")
              Action.Repeat (Times (numExpr "10+$rank*15"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "2"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1*5")), Some (Speed (None, numExpr "1.5+$rank*$rank*1.5")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$1"])
                          )
                          Action.Repeat (Times (numExpr "4"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                  BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$1"])
                                )
                              ]
                            )
                          )
                          Action.Wait (numExpr "6 + $rand * 3")
                        ]
                      )
                    )
                    Action.Wait (numExpr "6")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank*2")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "twoWay")},
            [
              Action.Repeat (Times (numExpr "5+$rank*4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "3+$rank*4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "3")), Some (Speed (None, numExpr "1.8")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3")), Some (Speed (None, numExpr "1.8")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.Wait (numExpr "5")
                        ]
                      )
                    )
                    Action.Wait (numExpr "20")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "spiral"}, ["-2"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "spiral"}, ["2"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "twoWay"}, [])
            ]
          )
        ]
      )

  /// ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん
  /// [Ketui_LT]_2boss_winder_crash.xml
  let b2boss_winder_crash =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "pre")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-20")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "40")), Some (Speed (None, numExpr "4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-40")), Some (Speed (None, numExpr "4")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "missile")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "5-$rank*2+$rand")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "0.0000001")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            [
                              ActionElm.Action ({actionLabel = None},
                                [
                                  Action.Wait (numExpr "60")
                                  Action.ChangeSpeed (Speed (None, numExpr "3"), Term (numExpr "30"))
                                ]
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "missiles")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-($1-1)*1.5")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "missile"}, [])
              )
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "missile"}, [])
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "40-($1-1)*3")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "missile"}, [])
              )
              Action.Repeat (Times (numExpr "$1-1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "missile"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "pre"}, [])
              Action.ActionRef ({actionRefLabel = ActionLabel "missiles"}, ["3+$rank*4"])
              Action.Wait (numExpr "160")
              Action.ActionRef ({actionRefLabel = ActionLabel "pre"}, [])
              Action.ActionRef ({actionRefLabel = ActionLabel "missiles"}, ["4+$rank*6"])
              Action.Wait (numExpr "160")
            ]
          )
        ]
      )
