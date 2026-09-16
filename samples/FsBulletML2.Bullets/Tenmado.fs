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
/// Tenmado
[<RequireQualifiedAccess>]
module Tenmado =

  /// tenmadoより、三面ボス「Disconnection」by 白い弾幕くん
  /// [tenmado]_3_boss_2.xml
  let b3_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "tenmadoより、三面ボス「Disconnection」by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), Some (Speed (None, numExpr "0.6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bitlaser"}, ["60"; "10"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-240")), Some (Speed (None, numExpr "0.6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bitlaser"}, ["-60"; "-10"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), Some (Speed (None, numExpr "0.6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bitaim"}, ["60"; "10"; "35"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-240")), Some (Speed (None, numExpr "0.6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bitaim"}, ["-60"; "-10"; "5"])
              )
              Action.Wait (numExpr "60")
              Action.Repeat (Times (numExpr "600 / (6.0 - 4.0 * $rank)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30 + 60 * $rand")), Some (Speed (None, numExpr "1.3+$rank*0.7")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "6.0 - 4.0 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "90")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bitlaser")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "120")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "0.6"), Term (numExpr "1"))
                  Action.Wait (numExpr "90")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.3"])
                  )
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "20")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (None, numExpr "0.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.3"])
                        )
                      ]
                    )
                  )
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "0.8"), Term (numExpr "1"))
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1 + 3.5 * $2")), Some (Speed (None, numExpr "0.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["1.5"])
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "20")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-$2")), Some (Speed (None, numExpr "0.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["1.5"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bitaim")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "120")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "0.6"), Term (numExpr "1"))
                  Action.Wait (numExpr "40 - $3")
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "70")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-10 + 20 * $rand")), Some (Speed (None, numExpr "0.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "30 + $3")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "0.8"), Term (numExpr "1"))
                  Action.Wait (numExpr "40 - $3")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-10 + 20 * $rand")), Some (Speed (None, numExpr "0.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "50 + $3")
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "laser")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 + 0.01")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 + 0.02")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 + 0.03")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 + 0.04")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 + 0.05")),
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

  /// tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん
  /// [tenmado]_5_boss_1.xml
  let b5_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "random"}, [])
              )
              Action.Repeat (Times (numExpr "8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "0.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "surprise"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), Some (Speed (None, numExpr "0.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "surprise"}, [])
                    )
                    Action.Wait (numExpr "100")
                  ]
                )
              )
              Action.Wait (numExpr "20")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "surprise")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "100")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "30"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "3.5")), Some (Speed (None, numExpr "15+$rand*15")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  [
                                    ActionElm.Action ({actionLabel = None},
                                      []
                                    )
                                  ]
                                )
                              )
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3.5")), Some (Speed (None, numExpr "15+$rand*15")),
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
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "random")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "200")
                  Action.Repeat (Times (numExpr "6000/(130 - 100 * $rank) "),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-22 + 44 * $rand")), Some (Speed (None, numExpr "1.6 + 1.0 * $rand")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "0.1 * (130 - 100 * $rank)")
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

  /// tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん
  /// [tenmado]_5_boss_3.xml
  let b5_boss_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stardust"}, [])
              )
              Action.Wait (numExpr "120")
              Action.Repeat (Times (numExpr "840/(120 - 100 * $rank)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["2"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["2.05"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["2.1"])
                    )
                    Action.Wait (numExpr "0.5 * (120 - 100 * $rank)")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "stardust")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5+$rank*10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135 + 90 * $rand")), Some (Speed (None, numExpr "0.3 + 1.7 * $rand")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stardust2"}, ["60"; "1.2"; "0.8"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135 + 90 * $rand")), Some (Speed (None, numExpr "0.3 + 1.7 * $rand")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stardust2"}, ["68"; "0.8"; "1.2"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135 + 90 * $rand")), Some (Speed (None, numExpr "0.3 + 1.7 * $rand")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stardust2"}, ["76"; "1.2"; "0.8"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135 + 90 * $rand")), Some (Speed (None, numExpr "0.3 + 1.7 * $rand")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stardust2"}, ["84"; "0.8"; "1.2"])
                        )
                        Action.Wait (numExpr "960/(10+$rank*20)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "stardust2")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "30")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "150")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "210")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "300")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "330")), Some (Speed (None, numExpr "$3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "laser")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "$1"), Term (numExpr "1"))
                ]
              )
            ]
          )
        ]
      )
