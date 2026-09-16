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
/// Bulletsmorph
[<RequireQualifiedAccess>]
module Bulletsmorph =

  /// Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん
  /// [Bulletsmorph]_aba_2.xml
  let aba_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "center"}, ["90 * $rand"; "1"])
                    Action.Wait (numExpr "12")
                    Action.ActionRef ({actionRefLabel = ActionLabel "center"}, ["90 * $rand"; "-1"])
                    Action.Wait (numExpr "12")
                    Action.ActionRef ({actionRefLabel = ActionLabel "center"}, ["30 * $rand"; "1"])
                    Action.Wait (numExpr "12")
                    Action.ActionRef ({actionRefLabel = ActionLabel "center"}, ["30 * $rand"; "-1"])
                    Action.Wait (numExpr "12")
                  ]
                )
              )
              Action.Wait (numExpr "150")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "center")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "circle"}, ["$1"; "$2"])
              )
              Action.Repeat (Times (numExpr "(4 + 8 * $rank) - 1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / (4 + 8 * $rank)")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "circle"}, ["$1"; "$2"])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "circle")}, None, Some (Speed (None, numExpr "1.3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + $1 * $2"), Term (numExpr "1"))
                  Action.Wait (numExpr "125 - $1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "red")}, None, Some (Speed (None, numExpr "0.1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "4.0"), Term (numExpr "300"))
                ]
              )
            ]
          )
        ]
      )

  /// Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん
  /// [Bulletsmorph]_aba_3.xml
  let aba_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4 + 16 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120 + 120 * $rand")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bomb"}, [])
                    )
                    Action.Wait (numExpr "60 - 30 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "180")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bomb")}, None, Some (Speed (None, numExpr "0.5 + 1.9 * $rand")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "50")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bombbit"}, [])
                  )
                  Action.Repeat (Times (numExpr "(4 + 8 * $rank) - 1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / (4 + 8 * $rank)")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bombbit"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bombbit")}, None, Some (Speed (None, numExpr "0.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "120")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "120")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "240")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changecolor"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "changecolor")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0")),
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

  /// Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん
  /// [Bulletsmorph]_aba_4.xml
  let aba_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0.1")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cross"}, [])
              )
              Action.Wait (numExpr "5")
              Action.Repeat (Times (numExpr "40 + 60 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.04")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cross"}, [])
                    )
                    Action.Wait (numExpr "20 - 10 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "cross")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Relative}, numExpr "4.0"), Term (numExpr "300"))
                  Action.Wait (numExpr "45")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん
  /// [Bulletsmorph]_aba_5.xml
  let aba_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-1"])
              )
              Action.Repeat (Times (numExpr "300"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-(120 + 45 * $rank) + (240 + 90 * $rank) * $rand")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Repeat (Times (numExpr "5"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.2")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "0.2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "(45 - 25 * $rank) * $1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backstab"}, [])
                  )
                  Action.Wait (numExpr "3")
                  Action.Repeat (Times (numExpr "29"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-0.5 * $1")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backstab"}, [])
                        )
                        Action.Wait (numExpr "3")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "30"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0.5 * $1")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backstab"}, [])
                        )
                        Action.Wait (numExpr "3")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "30"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-0.5 * $1")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "backstab"}, [])
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "backstab")}, None, Some (Speed (None, numExpr "1.6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "70 + 20 * $rand")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
                ]
              )
            ]
          )
        ]
      )

  /// Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん
  /// [Bulletsmorph]_aba_6.xml
  let aba_6 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "allway"}, [])
              Action.ActionRef ({actionRefLabel = ActionLabel "bar"}, [])
              Action.Wait (numExpr "200")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "allway")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "15")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "allwaybit"}, [])
              )
              Action.Repeat (Times (numExpr "11"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "allwaybit"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "allwaybit")}, None, Some (Speed (None, numExpr "6.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stopandgo"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stopandgo"}, [])
                        )
                        Action.Wait (numExpr "6 - 4 * $rank")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "stopandgo")}, None, Some (Speed (None, numExpr "1.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "4.0"), Term (numExpr "300"))
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bar")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barhand"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barhand"}, ["-1"])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "barhand")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0.0001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "4.0 - 2.0 * $rank")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["1"])
                  )
                  Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "4.0 - 2.0 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["1"])
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (None, numExpr "4.0 - 2.0 * $rank")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["-1"])
                  )
                  Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "4.0 - 2.0 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["-1"])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180 + 10 * $1")), Some (Speed (None, numExpr "4.0 - 2.0 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["1"])
                        )
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "4.0 - 2.0 * $rank")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["1"])
                              )
                            ]
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (None, numExpr "4.0 - 2.0 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["-1"])
                        )
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "4.0 - 2.0 * $rank")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "barbit"}, ["-1"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "barbit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "5")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90 * $1")), Some (Speed (None, numExpr "1.3")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
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

  /// Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん
  /// [Bulletsmorph]_aba_7.xml
  let aba_7 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                    )
                    Action.Wait (numExpr "60")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                    )
                    Action.Wait (numExpr "60")
                  ]
                )
              )
              Action.Wait (numExpr "250 - 50 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-32")), Some (Speed (None, numExpr "1.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                  )
                  Action.Repeat (Times (numExpr "8"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "8")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.3")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "slowdown"}, [])
                  )
                  Action.Repeat (Times (numExpr "2 + 4 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.3")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "slowdown"}, [])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "20")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "(30 - 20 * $rank) * (-1.0 + 2.0 * $rand)"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Relative}, numExpr "2.0 + 2.0 * $rank"), Term (numExpr "300"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "slowdown")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeSpeed (Speed (None, numExpr "0.3"), Term (numExpr "60"))
                ]
              )
            ]
          )
        ]
      )

  /// Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん
  /// [Bulletsmorph]_convergent.xml
  let convergent =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "1.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwaybit"}, ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"])
              )
              Action.Repeat (Times (numExpr "35"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (None, numExpr "1.0")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwaybit"}, ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"])
                    )
                  ]
                )
              )
              Action.Repeat (Times (numExpr "36"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (None, numExpr "1.0")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwaybit"}, ["-90"; "1.5 * (0.5 + 0.5 * $rank)"; "-3"])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "150")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nwaybit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "4")
                  Action.Repeat (Times (numExpr "2 + 4 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$3")), Some (Speed (None, numExpr "$2")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "4")
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

  /// Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん
  /// [Bulletsmorph]_double_seduction.xml
  let double_seduction =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "30")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "parentbit"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "parentbit"}, ["-1"])
              )
              Action.Wait (numExpr "300")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "parentbit")}, None, Some (Speed (None, numExpr "2.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["75"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["70"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["65"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["60"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["55"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["50"; "0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["80"; "15 * $1"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["75"; "10 * $1"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["70"; "6 * $1"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["65"; "3 * $1"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["60"; "1 * $1"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "cross"}, ["55"; "0"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "cross")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimbit"}, ["$1"; "$2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimbit"}, ["$1"; "$2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimbit"}, ["$1"; "$2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aimbit"}, ["$1"; "$2"])
              )
              Action.Wait (numExpr "5")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aimbit")}, None, Some (Speed (None, numExpr "0.6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$2")), Some (Speed (None, numExpr "1.6 * (0.5 + 0.5 * $rank)")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "2 + 5 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
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

  /// Bulletsmorphで生成。落下するひも。by 白い弾幕くん
  /// [Bulletsmorph]_fallen_string.xml
  let fallen_string =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "Bulletsmorphで生成。落下するひも。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "impl:48"}, [])
                    Action.Wait (numExpr "50")
                  ]
                )
              )
              Action.Wait (numExpr "50")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:48")},
            [
              Action.Wait (numExpr "20")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ActionRef ({actionRefLabel = ActionLabel "impl:60"}, [])
                        Action.ActionRef ({actionRefLabel = ActionLabel "impl:38"}, [])
                      ]
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:60")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Wait (numExpr "3")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Accel (None, Some (Vertical (Some {verticalType = VerticalType.Absolute}, numExpr "2.7")), Term (numExpr "120"))
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:38")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt"}, ["90"])
              Action.Wait (numExpr "24-$rank*8")
              Action.FireRef ({fireRefLabel = FireLabel "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt"}, ["-90"])
              Action.Wait (numExpr "24-$rank*8")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt")}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1-25+$rand*50")), None,
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "impl:59"}, [])
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:59")},
            [
              Action.Repeat (Times (numExpr "9999"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.8")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel"}, [])
                    )
                    Action.Wait (numExpr "3")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん
  /// [Bulletsmorph]_kunekune_plus_homing.xml
  let kunekune_plus_homing =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "impl:259"}, [])
                    Action.Wait (numExpr "50")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:259")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "15+30*$rand")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.8-$rank+$rand")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ActionRef ({actionRefLabel = ActionLabel "impl:30"}, [])
                      ]
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:30")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ActionRef ({actionRefLabel = ActionLabel "impl:156"}, [])
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.Repeat (Times (numExpr "10+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "impl:12"}, [])
                  ]
                )
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:156")},
            [
              Action.Wait (numExpr "1")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr")}, None, Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.3"), Term (numExpr "30"))
                  Action.Wait (numExpr "100")
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "5"), Term (numExpr "100"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "45-$rank*30"))
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:12")},
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

  /// Bulletsmorphで生成。悟君が4人。by 白い弾幕くん
  /// [Bulletsmorph]_satoru4.xml
  let satoru4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "Bulletsmorphで生成。悟君が4人。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "impl:100"}, [])
              Action.Wait (numExpr "80")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:100")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*16-8")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "($1+$rand*$1)*($rank/2+0.65)")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.ActionRef ({actionRefLabel = ActionLabel "impl:205"}, [])
                            ]
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "impl:205")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way"}, ["$rank*3+$rand"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*360"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2"), Term (numExpr "1"))
              Action.Wait (numExpr "30")
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way"}, ["$1"])
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
              Action.Vanish
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way"}, ["$1"; "-30"])
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way"}, ["$1"; "-15"])
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way"}, ["$1"; "0"])
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way"}, ["$1"; "15"])
              Action.ActionRef ({actionRefLabel = ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way"}, ["$1"; "30"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$2+$1*$rand*2-$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1")),
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
        ]
      )
