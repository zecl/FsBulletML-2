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
/// Original
[<RequireQualifiedAccess>]
module Original =

  /// 大原さんのオリジナル、断罪 by 白い弾幕くん
  /// [Original]_accusation.xml
  let accusation =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、断罪 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "centerbit"}, [])
              )
              Action.Wait (numExpr "800")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "centerbit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "0.9")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pillarbit"}, [])
                  )
                  Action.Repeat (Times (numExpr "17"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummybit"}, [])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pillarbit"}, [])
                        )
                        Action.Repeat (Times (numExpr "17"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummybit"}, [])
                              )
                            ]
                          )
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "120")
                  Action.Repeat (Times (numExpr "140"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "0.2")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "weak"}, ["240"])
                        )
                        Action.Wait (numExpr "2")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "70"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "2.0")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "weak"}, ["24"])
                        )
                        Action.Repeat (Times (numExpr "4"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.2")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "weak"}, ["24"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "pillarbit")}, None, Some (Speed (None, numExpr "0.6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "120")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "120")
                  Action.Repeat (Times (numExpr "300 / (35 - 33 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "10"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "2.0")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "weak"}, ["15"])
                              )
                            ]
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-45 + 90 * $rand")), Some (Speed (None, numExpr "(2.5 + 1.0 * $rand) * (0.25 + 0.75 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "35 - 33 * $rank")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummybit")}, None, Some (Speed (None, numExpr "0.6")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "120")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "120")
                  Action.Repeat (Times (numExpr "300 / (35 - 33 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-45 + 90 * $rand")), Some (Speed (None, numExpr "(2.5 + 1.0 * $rand) * (0.25 + 0.75 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "35 - 33 * $rank")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "weak")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$1")
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、風の精 by 白い弾幕くん
  /// [Original]_air_elemental.xml
  let air_elemental =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、風の精 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "parentbit"}, [])
              )
              Action.Wait (numExpr "650")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "slash")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "0.22")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.22")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.22")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (None, numExpr "0.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["120"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["120"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["120"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "150")), Some (Speed (None, numExpr "0.17")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.17")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.17")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-150"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (None, numExpr "0.25")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-120"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.25")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-120"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "0.25")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiralbit"}, ["-120"])
              )
              Action.Repeat (Times (numExpr "18"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "parentbit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170 + 20 * $rand")), Some (Speed (None, numExpr "1.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ActionRef ({actionRefLabel = ActionLabel "arrow"}, [])
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "5")
                        Action.ChangeSpeed (Speed (None, numExpr "1.8"), Term (numExpr "1"))
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "170 + 20 * $rand"), Term (numExpr "1"))
                        Action.Wait (numExpr "40")
                        Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                        Action.Wait (numExpr "5")
                        Action.ActionRef ({actionRefLabel = ActionLabel "arrow"}, [])
                      ]
                    )
                  )
                  Action.Wait (numExpr "80")
                  Action.ChangeSpeed (Speed (None, numExpr "1.8"), Term (numExpr "1"))
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "170 + 20 * $rand"), Term (numExpr "1"))
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ActionRef ({actionRefLabel = ActionLabel "slash"}, [])
                  Action.Wait (numExpr "150")
                  Action.ChangeSpeed (Speed (None, numExpr "1.8"), Term (numExpr "1"))
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30 + 60 * $rand"), Term (numExpr "1"))
                  Action.Wait (numExpr "15")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ActionRef ({actionRefLabel = ActionLabel "slash"}, [])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "arrow")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.3")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "6")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "spiralbit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1"), Term (numExpr "90"))
                  Action.Repeat (Times (numExpr "2 + 6 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiral"}, ["-$1"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "10 - 7 * $rank")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spiral"}, ["$1"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "10 - 7 * $rank")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "spiral")}, None, Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1"), Term (numExpr "90"))
                ]
              )
            ]
          )
        ]
      )

  /// オリジナル。後ろに弾を撃つ人々。 by 白い弾幕くん
  /// [Original]_backfire.xml
  let backfire =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "オリジナル。後ろに弾を撃つ人々。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "5+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "backFire"}, [])
                  ]
                )
              )
              Action.Wait (numExpr "300")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "backFire")}, Some (Direction (None, numExpr "50-$rand*100")), Some (Speed (None, numExpr "1.2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "10"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ChangeDirection (Direction (None, numExpr "150-$rand*300"), Term (numExpr "30"))
                          Action.Repeat (Times (numExpr "5"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.Wait (numExpr "6")
                                Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180")), Some (Speed (None, numExpr "1.2")),
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
                    Action.Repeat (Times (numExpr "999"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "6")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180")), Some (Speed (None, numExpr "1.2")),
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
          )
        ]
      )

  /// 大原さんのオリジナル、ふきだしボム by 白い弾幕くん
  /// [Original]_balloon_bomb.xml
  let balloon_bomb =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、ふきだしボム by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3 + 17 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120 + 120 * $rand")), Some (Speed (None, numExpr "1.0 + 0.3 * $rand")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "balloon"}, ["0.6 + 1.2 * $rank"])
                    )
                    Action.Wait (numExpr "43 - 30 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "200 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "balloon")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "$1 * 0.88")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "balloonbit"}, ["$1 * 0.88"])
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "$1 * 0.88")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "balloonbit"}, ["$1 * 0.88"])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "24"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "$1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curvebit"}, ["10"; "40"; "$1"])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (None, numExpr "$1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curvebit"}, ["10"; "40"; "$1"])
                  )
                  Action.Repeat (Times (numExpr "23"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "$1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curvebit"}, ["10"; "40"; "$1"])
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-3")), Some (Speed (None, numExpr "$1 * 0.88")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "balloonbit"}, ["$1 * 0.88"])
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "$1 * 0.88")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "balloonbit"}, ["$1 * 0.88"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "balloonbit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "4")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-60")), Some (Speed (None, numExpr "$1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curvebit"}, ["5"; "-50"; "$1"])
                  )
                  Action.Repeat (Times (numExpr "9"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "13")), Some (Speed (None, numExpr "$1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curvebit"}, ["5"; "-50"; "$1"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "curvebit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$2")), Some (Speed (None, numExpr "$3")),
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

  /// 大原さんのオリジナル、原点回帰その一 by 白い弾幕くん
  /// [Original]_btb_1.xml
  let btb_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その一 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "0"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (3/4)"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/2)"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (3/4)"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank))"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/4)"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/2)"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nway"}, ["(8 + 28 * $rank * $rank)"; "0.4"; "(360 / (8 + 28 * $rank * $rank)) * (1/4)"])
                    )
                    Action.Wait (numExpr "20")
                  ]
                )
              )
              Action.Wait (numExpr "80")
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayaim"}, ["(8 + 28 * $rank * $rank)"; "0.8 + 0.6 * $rank"; "(360 / (8 + 28 * $rank * $rank)) / 2"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayaim"}, ["(8 + 28 * $rank * $rank)"; "1.0 + 0.6 * $rank"; "(360 / (8 + 28 * $rank * $rank)) / 4"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayaim"}, ["(8 + 28 * $rank * $rank)"; "1.2 + 0.6 * $rank"; "0"])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nway")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$3")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "$1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / $1")), Some (Speed (None, numExpr "$2")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nwayaim")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$3")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "$1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / $1")), Some (Speed (None, numExpr "$2")),
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

  /// 大原さんのオリジナル、原点回帰その二 by 白い弾幕くん
  /// [Original]_btb_2.xml
  let btb_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その二 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "280 / ((50 - 43 * $rank) * 4)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaim"}, ["0"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaim"}, ["18"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaim"}, ["-18"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Wait (numExpr "(50 - 43 * $rank)")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["0"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["60"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["120"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["180"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["240"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["300"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Wait (numExpr "(50 - 43 * $rank)")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaimrev"}, ["0"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaimrev"}, ["18"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vaimrev"}, ["-18"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Wait (numExpr "(50 - 43 * $rank)")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["30"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["90"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["150"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["210"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["270"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "vabsolute"}, ["330"; "0.7 + 0.9 * $rank"])
                    )
                    Action.Wait (numExpr "(50 - 43 * $rank)")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "vaim")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-6")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "9")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-12")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-18")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "vaimrev")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "$2 * 1.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-6")), Some (Speed (None, numExpr "$2 * 1.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "9")), Some (Speed (None, numExpr "$2 * 1.21")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-12")), Some (Speed (None, numExpr "$2 * 1.21")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "$2 * 1.331")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-18")), Some (Speed (None, numExpr "$2 * 1.331")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "vabsolute")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-6")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "9")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-12")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-18")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "7 - 4 * $rank")
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "red")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1000")
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、原点回帰その三 by 白い弾幕くん
  /// [Original]_btb_3.xml
  let btb_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その三 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "half"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "half"}, ["-1"])
              )
              Action.Wait (numExpr "90 + 100 / (5 - 4 * $rank)")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "half")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "4 * (5 - 4 * $rank) * $1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.8"])
                  )
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "30 / (5 - 4 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "6 * (5 - 4 * $rank) * $1")), None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.8"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.9"])
                  )
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "30 / (5 - 4 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "6 * (5 - 4 * $rank) * $1")), None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["0.9"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "11 * $1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["1.2"])
                  )
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "30 / (5 - 4 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "2"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "6 * (5 - 4 * $rank) * $1")), None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["1.2"])
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "laser")}, None, Some (Speed (None, numExpr "0.01")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "3")
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2.5 - 2.0 * $rank")), Some (Speed (None, numExpr "$1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
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
        ]
      )

  /// 大原さんのオリジナル、原点回帰その四 by 白い弾幕くん
  /// [Original]_btb_4.xml
  let btb_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その四 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["8 + 28 * $rank"; "0.6 + 0.9 * $rank"])
              )
              Action.Wait (numExpr "440 - 90 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1"; "$2"; "0"])
                        )
                        Action.Wait (numExpr "3")
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, None, None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12)"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "3")
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1"; "$2"; "(360 / 36) * (1/4)"])
                        )
                        Action.Wait (numExpr "3")
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, None, None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (3/4)"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "3")
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1"; "$2"; "(360 / 36) * (1/2)"])
                        )
                        Action.Wait (numExpr "3")
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, None, None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (1/2)"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "3")
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1"; "$2"; "(360 / 36) * (3/4)"])
                        )
                        Action.Wait (numExpr "3")
                        Action.Repeat (Times (numExpr "2 + 3 * $rank"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, None, None,
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "nwayabsolute"}, ["$1 / 3"; "$2 * (0.9 + 0.4 * $rand)"; "(360 / 12) * (1/4)"])
                              )
                            ]
                          )
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nwayabsolute")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$3")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "$1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "(360 / $1) + (3 + 12 * (1 - $rank) * (1 - $rank)) * (-1 + 2 * $rand)")), Some (Speed (None, numExpr "$2")),
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

  /// 大原さんのオリジナル、原点回帰その五 by 白い弾幕くん
  /// [Original]_btb_5.xml
  let btb_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その五 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["72 * $rand"])
              )
              Action.Wait (numExpr "880")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winder"}, ["2.0"])
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "72")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winder"}, ["2.0"])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "420")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1 + 96")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["1.0 + 1.0 * $rank"])
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "72")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["1.0 + 1.0 * $rank"])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "350")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1 - 24")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["1.0 + 1.0 * $rank"])
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "72")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["1.0 + 1.0 * $rank"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shotgun")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-34")), Some (Speed (None, numExpr "$1 * 0.93")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "68 / (12 - 10 * $rank * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "12 - 10 * $rank * $rank")), Some (Speed (None, numExpr "$1 * 0.93")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-1")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "66 / (12 - 10 * $rank * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-(12 - 10 * $rank * $rank)")), Some (Speed (None, numExpr "$1")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "winder")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"])
                        )
                        Action.Wait (numExpr "14")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "15"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"])
                        )
                        Action.Wait (numExpr "14")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"])
                        )
                        Action.Wait (numExpr "14")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "15"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-8")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"])
                        )
                        Action.Wait (numExpr "14")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"])
                        )
                        Action.Wait (numExpr "14")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "laser")}, None, Some (Speed (None, numExpr "0.01")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "1")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2")), Some (Speed (None, numExpr "$1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2")), Some (Speed (None, numExpr "$1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-2")), Some (Speed (None, numExpr "$1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2")), Some (Speed (None, numExpr "$1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
        ]
      )

  /// 大原さんのオリジナル、原点回帰その六 by 白い弾幕くん
  /// [Original]_btb_6.xml
  let btb_6 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、原点回帰その六 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "360"; "-7"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "330"; "-6"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "300"; "-5"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "270"; "-4"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "240"; "-3"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "210"; "-2"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "180"; "-1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "180"; "1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "150"; "2"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "120"; "3"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "90"; "4"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "60"; "5"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "30"; "6"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["1.8"; "2"; "0"; "7"])
              )
              Action.Wait (numExpr "700")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "halfwinder")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"; "$4"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changecolor"}, ["$1"; "$2"; "$3"; "-$4"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "changecolor")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$2 * 2")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"; "$4"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$3")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"; "$2"])
                  )
                  Action.Wait (numExpr "$2 * (15 - 9 * $rank)")
                  Action.Repeat (Times (numExpr "300 / (15 - 9 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$4")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "laser"}, ["$1"; "$2"])
                        )
                        Action.Wait (numExpr "$2 * (15 - 9 * $rank)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "laser")}, None, Some (Speed (None, numExpr "0.01")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "1 + 3 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * (0.5 + 0.5 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "$2")
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

  /// 大原さんのオリジナル、検閲済 by 白い弾幕くん
  /// [Original]_censored.xml
  let censored =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、検閲済 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "center"}, ["0"])
              )
              Action.Wait (numExpr "800 - 50 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "center")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-10 + $1")), Some (Speed (None, numExpr "2.0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "80 + $1")), Some (Speed (None, numExpr "2.0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170 + $1")), Some (Speed (None, numExpr "2.0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "260 + $1")), Some (Speed (None, numExpr "2.0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "arm")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "25")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["330"; "-8"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["270"; "-5"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["210"; "-2"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["150"; "2"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["90"; "5"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "halfwinder"}, ["30"; "8"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "halfwinder")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"])
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "changecolor"}, ["$1"; "-$2"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "changecolor")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "(62 - 50 * $rank)/3")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0.7 + 1.1 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "62 - 50 * $rank")
                  Action.Repeat (Times (numExpr "600 / (62 - 50 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (None, numExpr "0.7 + 1.1 * $rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "62 - 50 * $rank")
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

  /// 大原さんのオリジナル、キメラ by 白い弾幕くん
  /// [Original]_chimera.xml
  let chimera =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、キメラ by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "centerbit"}, [])
              )
              Action.Wait (numExpr "450")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "centerbit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "160 + 40 * $rand"), Term (numExpr "90"))
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sidebit"}, ["-40"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-60")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sidebit"}, ["40"])
                  )
                  Action.Wait (numExpr "90")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "170 + 20 * $rand"), Term (numExpr "1"))
                        Action.ChangeSpeed (Speed (None, numExpr "0.85"), Term (numExpr "1"))
                        Action.Wait (numExpr "40")
                        Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                        Action.Wait (numExpr "5")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sidebit"}, ["-40"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sidebit"}, ["40"])
                        )
                        Action.Wait (numExpr "45")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "sidebit")}, None, Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "30 + 220 * $rank * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1 - 45 + 90 * $rand")), Some (Speed (None, numExpr "3.5 + 1.0 * $rand")),
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

  /// オリジナル、円を描きながらの自機狙い3way by 白い弾幕くん
  /// [Original]_circle.xml
  let circle =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル、円を描きながらの自機狙い3way by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "5sp")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "maru")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "4")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4"), Term (numExpr "1000"))
                    Action.Repeat (Times (numExpr "90"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ActionRef ({actionRefLabel = ActionLabel "5sp"}, ["0"])
                          Action.ActionRef ({actionRefLabel = ActionLabel "5sp"}, ["70-$rank*40"])
                          Action.ActionRef ({actionRefLabel = ActionLabel "5sp"}, ["-70+$rank*40"])
                          Action.Wait (numExpr "3")
                        ]
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
              Action.FireRef ({fireRefLabel = FireLabel "maru"}, [])
              Action.Wait (numExpr "320")
            ]
          )
        ]
      )

  /// オリジナル、どかんと一発 by 白い弾幕くん
  /// [Original]_dokkaan.xml
  let dokkaan =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル、どかんと一発 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "200+200*$rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "120*$rand-60")), Some (Speed (None, numExpr "0.5+$rand*2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "150")
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、楕円ボム by 白い弾幕くん
  /// [Original]_ellipse_bomb.xml
  let ellipse_bomb =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、楕円ボム by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "215 + 20 * $rand")), Some (Speed (None, numExpr "2.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"])
              )
              Action.Wait (numExpr "40")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "145 - 20 * $rand")), Some (Speed (None, numExpr "2.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"])
              )
              Action.Wait (numExpr "40")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170 + 20 * $rand")), Some (Speed (None, numExpr "2.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["0.8"; "1.4 * (0.5 + 0.5 * $rank)"; "30 - 20 * $rank"])
              )
              Action.Wait (numExpr "600 - 250 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "15")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "ellipse"}, ["$1"; "$2"; "$3"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "ellipse")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "5")), Some (Speed (None, numExpr "$1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                  )
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-($1 * 0.04)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-($1 * 0.01)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "($1 * 0.01)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "($1 * 0.04)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                  )
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-($1 * 0.04)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-($1 * 0.01)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                  )
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "($1 * 0.01)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "($1 * 0.04)")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, ["$2"; "$3"])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "red")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "35")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Wait (numExpr "$2")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "140")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "$2")
                  Action.Repeat (Times (numExpr "12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "7")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "$1 * 0.03")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "$2")
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

  /// 大原さんのオリジナル、EntangledSpace by 白い弾幕くん
  /// [Original]_entangled_space.xml
  let entangled_space =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、EntangledSpace by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["2.5"; "1.2 * (0.5 + 0.5 * $rank)"; "11 * (3.5 - 2.5 * $rank)"; "0"; "10"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["1.5"; "1.5 * (0.5 + 0.5 * $rank)"; "7 * (3.5 - 2.5 * $rank)"; "0"; "-8"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["0.5"; "1.8 * (0.5 + 0.5 * $rank)"; "5 * (3.5 - 2.5 * $rank)"; "0"; "6"])
              )
              Action.Wait (numExpr "1050 - 150 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "2.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "15")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "$1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$2"; "$3"; "$4"; "$5"])
                  )
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (None, numExpr "$1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$2"; "$3"; "$4"; "$5"])
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "20")
                  Action.Repeat (Times (numExpr "2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (None, numExpr "$1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$2"; "$3"; "$4"; "$5"])
                        )
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (None, numExpr "$1")),
                                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$2"; "$3"; "$4"; "$5"])
                              )
                            ]
                          )
                        )
                        Action.Wait (numExpr "20")
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "200 / $3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$5 * 2.5")), Some (Speed (None, numExpr "$2 * 0.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "$2 * 0.05")),
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
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "$2 * 0.6")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Repeat (Times (numExpr "3"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "$2 * 0.05")),
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
                        Action.Wait (numExpr "$3 * 3")
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
                  Action.Wait (numExpr "15")
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-(17 * $4)"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.Wait (numExpr "200 - 100 * $rank")
                  Action.ChangeSpeed (Speed (None, numExpr "0.13"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$3")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "$2")
                  Action.Repeat (Times (numExpr "96 / $2"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$4")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-($1 * 0.1)")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Wait (numExpr "$2")
                            ]
                          )
                        )
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$4")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "($1 * 0.1)")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Wait (numExpr "$2")
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
        ]
      )

  /// 大原さんのオリジナル、邪眼 by 白い弾幕くん
  /// [Original]_evil_eye.xml
  let evil_eye =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、邪眼 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "0.02")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "0.06")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "0.10")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "0.02")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "0.06")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "0.10")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cannonbit"}, [])
              )
              Action.Wait (numExpr "120")
              Action.Repeat (Times (numExpr "5 + 10 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "5way"}, ["30"])
                    Action.Wait (numExpr "27 - 20 * $rank")
                    Action.ActionRef ({actionRefLabel = ActionLabel "5way"}, ["20"])
                    Action.Wait (numExpr "27 - 20 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "5way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1 * (-2)")), Some (Speed (None, numExpr "1.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1 * 2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1 * (-1)")), Some (Speed (None, numExpr "1.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1 * 1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["0"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1 * 1")), Some (Speed (None, numExpr "1.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1 * (-1)"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1 * 2")), Some (Speed (None, numExpr "1.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1 * (-2)"])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1 - 15 + 30 * $rand")), Some (Speed (None, numExpr "1.3 + 1.0 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "2 + 3 * $rank"),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "cannonbit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.Repeat (Times (numExpr "80"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "0.0001 + 12.0 * $rand")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "weak"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "weak")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、偽婦人乱舞 by 白い弾幕くん
  /// [Original]_fujin_ranbu_fake.xml
  let fujin_ranbu_fake =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、偽婦人乱舞 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["0.6 + 0.6 * $rank"; "48 - 41 * $rank"; "8 * $rank"])
              )
              Action.Wait (numExpr "400 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "20"))
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                  Action.Repeat (Times (numExpr "1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "30"))
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
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
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["$1"])
                  )
                  Action.Wait (numExpr "$2")
                  Action.Repeat (Times (numExpr "200"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$3")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["$1"])
                        )
                        Action.Wait (numExpr "$2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shotgun")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.21")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.331")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.4641")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.610510")),
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

  /// 大原さんのオリジナル、真婦人乱舞 by 白い弾幕くん
  /// [Original]_fujin_ranbu_true.xml
  let fujin_ranbu_true =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、真婦人乱舞 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["0.6 + 0.6 * $rank"; "48 - 41 * $rank"; "8 * $rank"])
              )
              Action.Wait (numExpr "400 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "20"))
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                  Action.Repeat (Times (numExpr "1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "30"))
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "-$3"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["$1"; "$2"; "$3"])
                        )
                        Action.Wait (numExpr "10")
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
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["$1"])
                  )
                  Action.Wait (numExpr "$2")
                  Action.Repeat (Times (numExpr "200"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$3")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shotgun"}, ["$1"])
                        )
                        Action.Wait (numExpr "$2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shotgun")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.21")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.331")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.4641")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "$1 * 1.610510")),
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

  /// オリジナル、ぐるぐる by 白い弾幕くん
  /// [Original]_guruguru.xml
  let guruguru =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル、ぐるぐる by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "issyuu")},
            [
              Action.Repeat (Times (numExpr "360/(20-$rank*10)+1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "18-$rank*6")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guruguru")}, None, Some (Speed (None, numExpr "0.3")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0.7")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "issyuu"}, [])
                    Action.Repeat (Times (numExpr "30"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "12")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-356")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "issyuu"}, [])
                        ]
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
              Action.FireRef ({fireRefLabel = FireLabel "guruguru"}, [])
              Action.Wait (numExpr "500")
            ]
          )
        ]
      )

  /// オリジナル。ぐるちょ。 by 白い弾幕くん
  /// [Original]_gurutyo.xml
  let gurutyo =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。ぐるちょ。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "gurutyo")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "$1*(3+$rank*4)")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1*6"), Term (numExpr "1000"))
                    Action.Repeat (Times (numExpr "500"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
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
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "gurutyo"}, ["1"])
              Action.FireRef ({fireRefLabel = FireLabel "gurutyo"}, ["-1"])
              Action.Wait (numExpr "550")
            ]
          )
        ]
      )

  /// オリジナル。逆噴射。by 白い弾幕くん
  /// [Original]_gyakuhunsya.xml
  let gyakuhunsya =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。逆噴射。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "gyakuhunsya")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "20"))
              Action.Wait (numExpr "60")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
              Action.Wait (numExpr "20")
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "30"))
              Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "80"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "2+$rank*2"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*120-60")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.005")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "10")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "gyakuhunsya"}, [])
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、ハジケリスト by 白い弾幕くん
  /// [Original]_hajike.xml
  let hajike =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、ハジケリスト by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 60")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["5"; "42"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 60")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["-5"; "-42"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 62")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["5"; "40"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 62")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["-5"; "-40"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 - 58")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["5"; "40"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180 + 58")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "wave"}, ["-5"; "-40"])
                    )
                    Action.Wait (numExpr "30 - 15 * $rank")
                  ]
                )
              )
              Action.Wait (numExpr "300 - 50 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "wave")}, None, Some (Speed (None, numExpr "1.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$2")), Some (Speed (None, numExpr "0.3")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cross"}, [])
                  )
                  Action.Wait (numExpr "3")
                  Action.Repeat (Times (numExpr "10 + 20 * $rank * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "cross"}, [])
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "cross")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "100")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "270")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "0.6 * $rank")),
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

  /// オリジナル。はさみ。by 白い弾幕くん
  /// [Original]_hasami.xml
  let hasami =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。はさみ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "curve")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "1.2+$rank*0.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1*-1.5"), Term (numExpr "100"))
                      ]
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-$1")), Some (Speed (None, numExpr "1.3+$rank*0.4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1*1.5"), Term (numExpr "100"))
                      ]
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "200"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "2")
                    Action.ActionRef ({actionRefLabel = ActionLabel "curve"}, ["$rand*90"])
                  ]
                )
              )
              Action.Wait (numExpr "50")
            ]
          )
        ]
      )

  /// オリジナル。ひらひら。by 白い弾幕くん
  /// [Original]_hirahira.xml
  let hirahira =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。ひらひら。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "5way")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20")), None,
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "hira")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-40+$1*60")), None,
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "5way"}, [])
              Action.Repeat (Times (numExpr "60"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "8-$rank*2")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-80-$1*2+$rand-0.5")), None,
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "5way"}, [])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "hira"}, ["-1"])
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "hira"}, ["1"])
              Action.Wait (numExpr "60")
            ]
          )
        ]
      )

  /// オリジナル。放水っぽい感じ。by 白い弾幕くん
  /// [Original]_housya.xml
  let housya =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。放水っぽい感じ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-80")), None,
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "70"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "3*($rank+0.5)"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "($rand*20-9)/($rank+0.5)")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.01/($rank+0.5)")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
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

  /// 大原さんのオリジナル、カゴメ by 白い弾幕くん
  /// [Original]_kagome.xml
  let kagome =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、カゴメ by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10 + 10 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "75")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "matrixbit"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "255")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "matrixbit"}, [])
                    )
                    Action.Wait (numExpr "30 * (2.0 - 1.0 * $rank)")
                  ]
                )
              )
              Action.Wait (numExpr "150")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "matrixbit")}, None, Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30 * (2.0 - 1.0 * $rank)")
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (None, numExpr "1.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "finalbit"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (None, numExpr "1.1")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "finalbit"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-10 + 20 * $rand")), Some (Speed (None, numExpr "1.1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "60 * (2.0 - 1.0 * $rank)")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "finalbit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30 * (2.0 - 1.0 * $rank)")
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "30")), Some (Speed (None, numExpr "1.1 * (2 / 1.7320508)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-30")), Some (Speed (None, numExpr "1.1 * (2 / 1.7320508)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "60 * (2.0 - 1.0 * $rank)")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、毛玉 by 白い弾幕くん
  /// [Original]_kedama.xml
  let kedama =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、毛玉 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["1"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-1"])
                    )
                    Action.Wait (numExpr "90")
                  ]
                )
              )
              Action.Wait (numExpr "250")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "3.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0.6"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-105 * $1"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "60 * $1")), Some (Speed (None, numExpr "0.6 + 0.7 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "12 - 10 * $rank")
                  Action.Repeat (Times (numExpr "999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "113 * $1")), Some (Speed (None, numExpr "0.6 + 0.7 * $rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "12 - 10 * $rank")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、弾幕の騎士その一 by 白い弾幕くん
  /// [Original]_knight_1.xml
  let knight_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、弾幕の騎士その一 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
              )
              Action.Wait (numExpr "450")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "1.1"), Term (numExpr "120"))
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "6"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "10"))
                              Action.Wait (numExpr "10")
                            ]
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-70 + 20 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kick"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "kick")}, None, Some (Speed (None, numExpr "4.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0.001"), Term (numExpr "30"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-20")), Some (Speed (None, numExpr "0.7")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "4 + 25 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10 - 8 * $rank")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "0.7")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "4 + 25 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10 - 8 * $rank")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "20")), Some (Speed (None, numExpr "0.7")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "4 + 25 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10 - 8 * $rank")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
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

  /// 大原さんのオリジナル、弾幕の騎士その二 by 白い弾幕くん
  /// [Original]_knight_2.xml
  let knight_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、弾幕の騎士その二 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["90"; "1.0"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "150")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["70"; "1.2"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["50"; "1.4"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "210")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["30"; "1.6"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["10"; "1.8"])
              )
              Action.Wait (numExpr "300 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "2.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "$1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "$2 * (0.5 + 0.5 * $rank)")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "19 + 100 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / (19 + 100 * $rank)")), Some (Speed (None, numExpr "$2 * (0.5 + 0.5 * $rank)")),
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

  /// 大原さんのオリジナル、弾幕の騎士その三 by 白い弾幕くん
  /// [Original]_knight_3.xml
  let knight_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、弾幕の騎士その三 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
              )
              Action.Wait (numExpr "300")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "3.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "2")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "groundbit"}, [])
                  )
                  Action.Wait (numExpr "30")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "skybit"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "skybit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "210")), Some (Speed (None, numExpr "3.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "110 + 20 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "groundbit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "230 + 20 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5 + 15 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-10 + 20 * $rand")), Some (Speed (None, numExpr "1.0 + 1.4 * $rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "5"),
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
                        Action.Wait (numExpr "12 - 10 * $rank")
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

  /// 大原さんのオリジナル、弾幕の騎士その四 by 白い弾幕くん
  /// [Original]_knight_4.xml
  let knight_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、弾幕の騎士その四 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "parentbit"}, ["1.6"])
              )
              Action.Wait (numExpr "300 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "parentbit")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20 + 10 * $rand")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-120"; "$1"])
                        )
                        Action.Wait (numExpr "10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-60")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["120"; "$1"])
                        )
                        Action.Wait (numExpr "10")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, Some (Speed (None, numExpr "2.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "4 + 4 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), Some (Speed (None, numExpr "$2 * (0.5 + 0.5 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
        ]
      )

  /// オリジナル。固体。 by 白い弾幕くん
  /// [Original]_kotai.xml
  let kotai =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。固体。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "src")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.Repeat (Times (numExpr "25"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$rand*360")), Some (Speed (None, numExpr "$rand*10")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            [
                              ActionElm.Action ({actionLabel = None},
                                [
                                  Action.Wait (numExpr "1")
                                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1.8")),
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
                  )
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "350/(10-$rank*6)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10-$rank*6")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "-10+$rand*20")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "src"}, [])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "30")
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、鯨幕砲 by 白い弾幕くん
  /// [Original]_kujira.xml
  let kujira =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、鯨幕砲 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["2"; "1.0"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-2"; "1.0"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["2.5"; "1.05"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-2.5"; "1.05"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["3"; "1.1"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-3"; "1.1"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["3.5"; "1.15"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, ["-3.5"; "1.15"])
              )
              Action.Wait (numExpr "160")
              Action.Repeat (Times (numExpr "5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "30")), Some (Speed (None, numExpr "1.55")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["179"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "40")), Some (Speed (None, numExpr "1.4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["170"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "50")), Some (Speed (None, numExpr "1.25")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["160"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["150"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "70")), Some (Speed (None, numExpr "0.95")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["140"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "80")), Some (Speed (None, numExpr "0.8")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["130"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-30")), Some (Speed (None, numExpr "1.55")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-179"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-40")), Some (Speed (None, numExpr "1.4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-170"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-50")), Some (Speed (None, numExpr "1.25")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-160"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-60")), Some (Speed (None, numExpr "1.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-150"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-70")), Some (Speed (None, numExpr "0.95")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-140"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-80")), Some (Speed (None, numExpr "0.8")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kujira"}, ["-130"])
                    )
                    Action.Wait (numExpr "2")
                  ]
                )
              )
              Action.Wait (numExpr "250 - 100 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "kujira")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "5")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1"), Term (numExpr "60"))
                  Action.Wait (numExpr "60 + 5")
                  Action.ChangeSpeed (Speed (None, numExpr "3.5 * (0.75 + 0.25 * $rank)"), Term (numExpr "30"))
                  Action.Repeat (Times (numExpr "10 / (2.0 - 1.0 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "8 * (2.0 - 1.0 * $rank)"))
                        Action.Wait (numExpr "8 * (2.0 - 1.0 * $rank)")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0.0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "$2 * (0.5 + 0.5 * $rank)")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "2 * (3.5 - 2.5 * $rank)")
                  Action.Repeat (Times (numExpr "100 / (3.5 - 2.5 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1 * (3.5 - 2.5 * $rank)")), Some (Speed (None, numExpr "$2 * (0.5 + 0.5 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "2 * (3.5 - 2.5 * $rank)")
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

  /// オリジナル。くねくね。by 白い弾幕くん
  /// [Original]_kunekune.xml
  let kunekune =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。くねくね。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fire")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Relative}, numExpr "-0.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "src")}, Some (Direction (None, numExpr "(30+$rank*20)*$1")), Some (Speed (None, numExpr "2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "10"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Relative}, numExpr "$1*4")), None, Term (numExpr "20"))
                          Action.Repeat (Times (numExpr "10"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, ["$1"])
                                Action.Wait (numExpr "2")
                              ]
                            )
                          )
                          Action.Accel (Some (Horizontal (Some {horizontalType = HorizontalType.Relative}, numExpr "-$1*4")), None, Term (numExpr "20"))
                          Action.Repeat (Times (numExpr "10"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.ActionRef ({actionRefLabel = ActionLabel "fire"}, ["$1"])
                                Action.Wait (numExpr "2")
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
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "src"}, ["1"])
                    Action.FireRef ({fireRefLabel = FireLabel "src"}, ["-1"])
                    Action.Wait (numExpr "80")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
        ]
      )

  /// オリジナル。扇状弾二つ。by 白い弾幕くん
  /// [Original]_oogi_hutatsu.xml
  let oogi_hutatsu =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。扇状弾二つ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "oogiSeq")},
            [
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "8")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
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
              Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-80")), Some (Speed (None, numExpr "$2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "oogiSeq"}, [])
              Action.Repeat (Times (numExpr "10+$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "2")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.04")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "oogiSeq"}, [])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "oogi"}, ["161"; "0.8+$rank*0.4"])
              Action.Wait (numExpr "30")
              Action.ActionRef ({actionRefLabel = ActionLabel "oogi"}, ["159"; "1+$rank*0.6"])
              Action.Wait (numExpr "150")
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、光学探査兵器 by 白い弾幕くん
  /// [Original]_optic_seeker.xml
  let optic_seeker =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、光学探査兵器 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "2 + 8 * $rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30 + 60 * $rand")), Some (Speed (None, numExpr "3.0 + 2.0 * $rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seal"}, [])
                    )
                    Action.Wait (numExpr "25 - 20 * $rank")
                    Action.Repeat (Times (numExpr "3"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-60 + 120 * $rand")), Some (Speed (None, numExpr "3.0 + 2.0 * $rank")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "reflect"}, [])
                          )
                          Action.Wait (numExpr "25 - 20 * $rank")
                        ]
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "reflect")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "(30 + 20 * $rand) * (1.0 - 0.5 * $rank)")
                  Action.Repeat (Times (numExpr "3"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "60 + 240 * $rand"), Term (numExpr "1"))
                        Action.Wait (numExpr "(10 + 10 * $rand) * (1.0 - 0.5 * $rank)")
                      ]
                    )
                  )
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seal")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10 + 10 * $rand")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "3.0 + 2.0 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "45")), Some (Speed (None, numExpr "3.0 + 2.0 * $rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "3.0 + 2.0 * $rank")),
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

  /// オリジナル。ぱん。by 白い弾幕くん
  /// [Original]_pan.xml
  let pan =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。ぱん。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "pan")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "95*$1")), Some (Speed (None, numExpr "1.8+$rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-90")), Some (Speed (None, numExpr "0.001")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pan"}, ["1"])
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.2+$rank*0.4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pan"}, ["1"])
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "90")), Some (Speed (None, numExpr "0.001")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pan"}, ["-1"])
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.2+$rank*0.4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "pan"}, ["-1"])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "40")
            ]
          )
        ]
      )

  /// オリジナル。弱誘導弾から左右に弾幕。by 白い弾幕くん
  /// [Original]_progear_cheap_fake.xml
  let progear_cheap_fake =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。弱誘導弾から左右に弾幕。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "weekHoming")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "0.1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "3"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ChangeSpeed (Speed (None, numExpr "1.5"), Term (numExpr "30"))
                          Action.Wait (numExpr "30")
                          Action.Repeat (Times (numExpr "2+$rank*4"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (None, numExpr "1.3")),
                                  BulletElm.Bullet ({bulletLabel = None}, None, None,
                                    []
                                  )
                                )
                                Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (None, numExpr "1.3")),
                                  BulletElm.Bullet ({bulletLabel = None}, None, None,
                                    []
                                  )
                                )
                                Action.Wait (numExpr "2")
                              ]
                            )
                          )
                          Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "60-120*$rand"), Term (numExpr "20"))
                          Action.ChangeSpeed (Speed (None, numExpr "0.1"), Term (numExpr "20"))
                          Action.Wait (numExpr "20")
                        ]
                      )
                    )
                    Action.ChangeSpeed (Speed (None, numExpr "1.5"), Term (numExpr "30"))
                    Action.Wait (numExpr "30")
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "weekHoming"}, [])
                    Action.Wait (numExpr "60-$rank*30")
                  ]
                )
              )
              Action.Wait (numExpr "180")
            ]
          )
        ]
      )

  /// オリジナル。炸裂弾。by 白い弾幕くん
  /// [Original]_sakuretudan.xml
  let sakuretudan =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。炸裂弾。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.ChangeDirection (Direction (None, numExpr "0"), Term (numExpr "60"))
                              Action.Wait (numExpr "60")
                              Action.Repeat (Times (numExpr "15+$rank*20"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Repeat (Times (numExpr "2"),
                                      ActionElm.Action ({actionLabel = None},
                                        [
                                          Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "360*$rand")), Some (Speed (None, numExpr "5")),
                                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                                              [
                                                ActionElm.Action ({actionLabel = None},
                                                  [
                                                    Action.Wait (numExpr "$rand*5")
                                                    Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "$rand*4+0.5")),
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
                    )
                    Action.Wait (numExpr "60")
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、シューティングスター by 白い弾幕くん
  /// [Original]_shooting_star.xml
  let shooting_star =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、シューティングスター by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "75 + 10 * $rand")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, ["215 + 10 * $rand"])
              )
              Action.Wait (numExpr "50")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-(75 + 10 * $rand)")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, ["-(215 + 10 * $rand)"])
              )
              Action.Wait (numExpr "30")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "75 + 10 * $rand")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, ["215 + 10 * $rand"])
              )
              Action.Wait (numExpr "10")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-(75 + 10 * $rand)")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, ["-(215 + 10 * $rand)"])
              )
              Action.Wait (numExpr "650 - 150 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "star")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "45")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "15")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (None, numExpr "2.0"), Term (numExpr "180"))
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "180"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "150 + 60 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "tail"}, ["1.5 * (0.25 + 0.75 * $rank)"])
                        )
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "head"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "head")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "20 * (0.25 + 0.75 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "(0.3 + 0.3 * $rand) * (0.25 + 0.75 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "30 * (0.25 + 0.75 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "(0.5 + 0.5 * $rand) * (0.25 + 0.75 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "50 * (0.25 + 0.75 * $rank)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "(0.8 + 0.8 * $rand) * (0.25 + 0.75 * $rank)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "1.6 * (0.25 + 0.75 * $rank)")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "12 + 24 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360 / (12 + 24 * $rank)")), Some (Speed (None, numExpr "1.6 * (0.25 + 0.75 * $rank)")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "tail")}, None, Some (Speed (None, numExpr "0.001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "1 + 3 * $rank * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-3 + 6 * $rand")), Some (Speed (None, numExpr "$1 * (1.0 + (0.1 + 0.2 * $rank) * $rand)")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
        ]
      )

  /// 大原さんのオリジナル、あの空に星を by 白い弾幕くん
  /// [Original]_star_in_the_sky.xml
  let star_in_the_sky =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、あの空に星を by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
              )
              Action.Repeat (Times (numExpr "11"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "450 - 200 * $rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, Some (Speed (None, numExpr "0.0001")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "0.6")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, [])
                  )
                  Action.Wait (numExpr "2")
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-7")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "star"}, [])
                        )
                        Action.Wait (numExpr "2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "star")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "55")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0001"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-170")), Some (Speed (None, numExpr "0.6 + 0.7 * $rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "3 * 4 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "11")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.05")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
        ]
      )

  /// オリジナル。池に落ちた石六個。 by 白い弾幕くん
  /// [Original]_stone6.xml
  let stone6 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "オリジナル。池に落ちた石六個。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "150")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "210")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "225")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "225")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Repeat (Times (numExpr "45"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "8")), Some (Speed (None, numExpr "1.3+$rank")),
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

  /// オリジナル。道を探せ。by 白い弾幕くん
  /// [Original]_stop_and_run.xml
  let stop_and_run =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "オリジナル。道を探せ。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "stopAndRun")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "60"))
                  Action.Wait (numExpr "180-$rank*120")
                  Action.Accel (None, Some (Vertical (None, numExpr "3")), Term (numExpr "120"))
                  Action.Wait (numExpr "120")
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "200"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "3*$rand+0.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "stopAndRun"}, [])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "260-$rank*120")
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、時空転換 by 白い弾幕くん
  /// [Original]_time_twist.xml
  let time_twist =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、時空転換 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "ancient"}, [])
              Action.Wait (numExpr "60 - 40 * $rank")
              Action.ActionRef ({actionRefLabel = ActionLabel "future"}, [])
              Action.Wait (numExpr "60 - 40 * $rank")
              Action.ActionRef ({actionRefLabel = ActionLabel "modern"}, [])
              Action.Wait (numExpr "60 - 40 * $rank")
              Action.ActionRef ({actionRefLabel = ActionLabel "medieval"}, [])
              Action.Wait (numExpr "60 - 40 * $rank")
              Action.ActionRef ({actionRefLabel = ActionLabel "primal"}, [])
              Action.Wait (numExpr "450")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "ancient")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "ancientBit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "ancientBit"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0.0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "ancientBit"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "ancientBit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "40"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "2.0")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "modern")},
            [
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-45 + 90 * $rand")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "modernBit"}, [])
                    )
                    Action.Wait (numExpr "6")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "modernBit")}, None, Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "160 + 40 * $rand"), Term (numExpr "90"))
                  Action.Wait (numExpr "30")
                  Action.Repeat (Times (numExpr "5 + 5 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-45 + 90 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "modernScore"}, [])
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "modernScore")}, None, Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "160 + 40 * $rand"), Term (numExpr "90"))
                  Action.ChangeSpeed (Speed (None, numExpr "1.0 + 2.0 * $rank"), Term (numExpr "300"))
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "future")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "futureBit"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "futureBit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "futureTriangle"}, [])
                  )
                  Action.Repeat (Times (numExpr "10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "futureTriangle"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "futureTriangle")}, None, Some (Speed (None, numExpr "1.1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "120"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "150"), Term (numExpr "60"))
                  Action.Repeat (Times (numExpr "20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "45")), Some (Speed (None, numExpr "0.8")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-45")), Some (Speed (None, numExpr "1.0")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-60")), Some (Speed (None, numExpr "1.2")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "medieval")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "medievalBit"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "medievalBit")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.ChangeSpeed (Speed (None, numExpr "0.0"), Term (numExpr "1"))
                  Action.Wait (numExpr "5")
                  Action.Repeat (Times (numExpr "50"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "medievalStar"}, ["90"])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "medievalStar"}, ["-90"])
                        )
                        Action.Wait (numExpr "2")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "medievalStar")}, None, Some (Speed (None, numExpr "1.1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1.1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "primal")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "primalBit"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "primalBit")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "0.5")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "1.1"), Term (numExpr "120"))
                  Action.Repeat (Times (numExpr "24"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "5"))
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                  Action.ChangeSpeed (Speed (None, numExpr "0.0"), Term (numExpr "120"))
                  Action.Repeat (Times (numExpr "24"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "5"))
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                  Action.Wait (numExpr "60")
                  Action.Repeat (Times (numExpr "80 + 220 * $rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ActionRef ({actionRefLabel = ActionLabel "primalRock"}, ["180 * $rand"; "1"])
                        Action.ActionRef ({actionRefLabel = ActionLabel "primalRock"}, ["180 * $rand"; "-1"])
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "primalRock")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1 * $2")), Some (Speed (None, numExpr "\r\n          (0.8 + 1.1*$1*(180-$1)/(90*90)) * (0.5+0.5*$rand) * (0.5+0.5*$rank)\r\n        ")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
        ]
      )

  /// 大原さんのオリジナル、津波。by 白い弾幕くん
  /// [Original]_tsunami.xml
  let tsunami =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "大原さんのオリジナル、津波。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "80+$rank*120"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "360 * $rand")), Some (Speed (None, numExpr "0.6 - 0.5 * $rank + 0.15 * $rand")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60 + 240 * $rand")), Some (Speed (None, numExpr "0.6 - 0.5 * $rank + 0.15 * $rand")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120 + 120 * $rand")), Some (Speed (None, numExpr "0.6 - 0.5 * $rank + 0.15 * $rand")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "2")
                  ]
                )
              )
              Action.Wait (numExpr "10 + 210 * $rank * $rank")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "70 + 40 * $rand")), Some (Speed (None, numExpr "0.6 - 0.3 * $rank")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer1"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (None, numExpr "0.6 - 0.3 * $rank")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer1"}, [])
              )
              Action.Wait (numExpr "600")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "layer1")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.Repeat (Times (numExpr "100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), Some (Speed (None, numExpr "0.8 - 0.4 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer2"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-90")), Some (Speed (None, numExpr "0.8 - 0.4 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer2"}, [])
                        )
                        Action.Wait (numExpr "60")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "layer2")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.Repeat (Times (numExpr "100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "45")), Some (Speed (None, numExpr "1.0 - 0.5 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer3"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-45")), Some (Speed (None, numExpr "1.0 - 0.5 * $rank")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "layer3"}, [])
                        )
                        Action.Wait (numExpr "60")
                      ]
                    )
                  )
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "layer3")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.Repeat (Times (numExpr "100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "30")), Some (Speed (None, numExpr "0.2 + 0.3 * $rank * $rand")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-30")), Some (Speed (None, numExpr "0.2 + 0.3 * $rank * $rand")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "60")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )

  /// オリジナル。二つ十字。by 白い弾幕くん
  /// [Original]_two_cross.xml
  let two_cross =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。二つ十字。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "add3")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), None,
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "slow")}, Some (Direction (None, numExpr "(50-$rank*20)*$1")), Some (Speed (None, numExpr "2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "5")
                    Action.Repeat (Times (numExpr "100"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4*$1")), None,
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                          Action.Wait (numExpr "4")
                        ]
                      )
                    )
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "slow"}, ["1"])
                    Action.FireRef ({fireRefLabel = FireLabel "slow"}, ["-1"])
                    Action.Wait (numExpr "80")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
        ]
      )

  /// オリジナル。うねりを作る。by 白い弾幕くん
  /// [Original]_uneri.xml
  let uneri =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。うねりを作る。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "src")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "1.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "450/(4-$rank*2)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4.5-$rank*3+$rand")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$rand*10-5")), Some (Speed (None, numExpr "1.5")),
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
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "srcFire")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "$1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "5")
                    Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                    Action.ActionRef ({actionRefLabel = ActionLabel "src"}, ["$2"])
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "srcFire"}, ["0"; "0"])
              Action.FireRef ({fireRefLabel = FireLabel "srcFire"}, ["4"; "10"])
              Action.FireRef ({fireRefLabel = FireLabel "srcFire"}, ["8"; "20"])
              Action.FireRef ({fireRefLabel = FireLabel "srcFire"}, ["-4"; "-10"])
              Action.FireRef ({fireRefLabel = FireLabel "srcFire"}, ["-8"; "-20"])
              Action.Wait (numExpr "500")
            ]
          )
        ]
      )

  /// オリジナル。ワナ。by 白い弾幕くん
  /// [Original]_wana.xml
  let wana =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。ワナ。by 白い弾幕くん"; bulletmlDescription = None},
        [
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-10")), Some (Speed (None, numExpr "0")),
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
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["20"; "5"; "1.10"; "-0.04"; "1.10"; "-0.04"])
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["15-$rank*6"; "5"; "1.06"; "0.04"; "1.06"; "0.04"])
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["5+$rank*20"; "5"; "1.10"; "-0.04"; "1.10"; "-0.04"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "215")), Some (Speed (None, numExpr "0")),
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
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["20"; "-5"; "1.10"; "-0.04"; "1.10"; "-0.04"])
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["15-$rank*6"; "-5"; "1.06"; "0.04"; "1.06"; "0.04"])
              Action.ActionRef ({actionRefLabel = ActionLabel "main"}, ["5+$rank*20"; "-5"; "1.10"; "-0.04"; "1.06"; "0.04"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "main")},
            [
              Action.Repeat (Times (numExpr "$1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "36+$2")), Some (Speed (None, numExpr "$3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["5"; "1"; "$4"])
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "36")), Some (Speed (None, numExpr "$3")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["5"; "1"; "$4"])
                        ]
                      )
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "36")), Some (Speed (None, numExpr "$5")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["5"; "1"; "$6"])
                        ]
                      )
                    )
                    Action.Wait (numExpr "15")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// オリジナル。横加速。by 白い弾幕くん
  /// [Original]_yokokasoku.xml
  let yokokasoku =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "オリジナル。横加速。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "accelShot")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Accel (Some (Horizontal (None, numExpr "3*$1")), None, Term (numExpr "80"))
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*90-45")), Some (Speed (None, numExpr "1")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Repeat (Times (numExpr "9999"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Wait (numExpr "$rand*20+30-$rank*20")
                                    Action.FireRef ({fireRefLabel = FireLabel "accelShot"}, ["1"])
                                    Action.FireRef ({fireRefLabel = FireLabel "accelShot"}, ["-1"])
                                  ]
                                )
                              )
                            ]
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
        ]
      )

  /// オリジナル。雑魚で突撃。by 白い弾幕くん
  /// [Original]_zako_atack.xml
  let zako_atack =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "オリジナル。雑魚で突撃。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "40+$rank*20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*180-90")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Repeat (Times (numExpr "3"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "(0.5-$rand)*$rank*10")), Some (Speed (None, numExpr "1.5")),
                                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                                        []
                                      )
                                    )
                                    Action.Wait (numExpr "20+$rand*$rank*40")
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
            ]
          )
        ]
      )
