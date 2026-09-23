// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Xevious
[<RequireQualifiedAccess>]
module XiiStag =

  /// トゥエルブスタッグ３ボス by 白い弾幕くん
  /// [XII_STAG]_3b.xml
  let b3b =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "トゥエルブスタッグ３ボス by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "bara"}, ["1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "bara"}, ["-1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, ["180-55"; "-5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, ["180"; "0"])
              Action.ActionRef ({actionRefLabel = ActionLabel "3way"}, ["180+55"; "5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "roll"}, ["180+45"; "1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "roll"}, ["180-45"; "-1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "straight"}, ["1"])
              Action.ActionRef ({actionRefLabel = ActionLabel "straight"}, ["-1"])
              Action.Wait (numExpr "50")
              Action.Repeat (Times (numExpr "3*$rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.ActionRef ({actionRefLabel = ActionLabel "fin1"}, ["1"])
                        ]
                      )
                    )
                    Action.Fire ({fireLabel = None}, None, Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.ActionRef ({actionRefLabel = ActionLabel "fin1"}, ["-1"])
                        ]
                      )
                    )
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.ActionRef ({actionRefLabel = ActionLabel "white1"}, [])
                        ]
                      )
                    )
                    Action.Wait (numExpr "5*(6+(12*$rank))")
                  ]
                )
              )
              Action.Wait (numExpr "110")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fin1")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fin2"}, ["$1"])
              Action.Vanish
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fin2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1*90")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.7+(0.8*$rank)")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Wait (numExpr "2/($rank+0.2)")
              Action.Repeat (Times (numExpr "1+$rank*32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-1*$1*((2.3/($rank+0.01))+0.35)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.7+(0.8*$rank)")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "2/($rank+0.2)")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "white1")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "white2"}, ["0"; "0.00001"; "0"])
              Action.FireRef ({fireRefLabel = FireLabel "white2"}, ["90"; "1"; "1.5"])
              Action.FireRef ({fireRefLabel = FireLabel "white2"}, ["-90"; "1"; "-1.5"])
              Action.Vanish
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "white2")}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4")
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.00001"), Term (numExpr "1"))
                    Action.Wait (numExpr "83-(70*$rank)")
                    Action.Repeat (Times (numExpr "6+(12*$rank)"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-1*$1+$3")), Some (Speed (None, numExpr "2.9")),
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
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bara")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "5c"}, ["44*$1"; "4.1"; "4"])
              Action.FireRef ({fireRefLabel = FireLabel "5c"}, ["55.5*$1"; "3.45"; "3"])
              Action.FireRef ({fireRefLabel = FireLabel "5c"}, ["55*$1"; "4.2"; "2"])
              Action.FireRef ({fireRefLabel = FireLabel "5c"}, ["70*$1"; "3"; "0"])
              Action.FireRef ({fireRefLabel = FireLabel "5c"}, ["68*$1"; "3.74"; "1"])
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "5c")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+$1")), Some (Speed (None, numExpr "$2/1.1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10")
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
                    Action.Wait (numExpr "5+($3*5)")
                    Action.Repeat (Times (numExpr "10-(5/($rank+0.001))"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Repeat (Times (numExpr "3"),
                            ActionElm.ActionRef ({actionRefLabel = ActionLabel "almond1"}, [])
                          )
                          Action.Wait (numExpr "85-(40*$rank)")
                        ]
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "almond1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "3.5-(7*$rand)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0+(0.3*$rand)")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "almond2"}, [])
              )
              Action.Wait (numExpr "3")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "almond2")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Relative}, numExpr "1.8+(0.8*$rank)"), Term (numExpr "10"))
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "3way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "3.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10")
                        Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
                        Action.Wait (numExpr "1")
                        Action.Repeat (Times (numExpr "7+(10*$rank)"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.FireRef ({fireRefLabel = FireLabel "9way"}, ["$1+16"])
                              Action.FireRef ({fireRefLabel = FireLabel "9way"}, ["$1"])
                              Action.FireRef ({fireRefLabel = FireLabel "9way"}, ["$1-16"])
                              Action.Wait (numExpr "25")
                            ]
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
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "9way")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "roll")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+(11*$2)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "10")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "5")
                        Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
                        Action.Wait (numExpr "1")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1+(30*$2)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "15")
                        Action.Repeat (Times (numExpr "11+(17*$rank)"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-35*$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30*$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.5")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
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
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "straight")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+(82*$1)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.7")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "13")
                        Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
                        Action.Wait (numExpr "1")
                        Action.Repeat (Times (numExpr "3+(5*$rank)"),
                          ActionElm.ActionRef ({actionRefLabel = ActionLabel "fall"}, [])
                        )
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fall")},
            [
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.9")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "5")
                  ]
                )
              )
              Action.Wait (numExpr "15")
            ]
          )
        ]
      )
