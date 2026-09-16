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
/// Dodonpachi
[<RequireQualifiedAccess>]
module Dodonpachi =

  /// 怒首領蜂、火蜂。by 白い弾幕くん
  /// [Dodonpachi]_hibachi.xml
  let hibachi =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂、火蜂。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "allWay")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-50+$rand*20")), Some (Speed (None, numExpr "1+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "15+16*$rank*$rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24-$rank*12")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "right")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1"), Term (numExpr "1"))
              Action.Repeat (Times (numExpr "25"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "allWay"}, [])
                    Action.Wait (numExpr "3")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "left")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1"), Term (numExpr "1"))
              Action.Repeat (Times (numExpr "25"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "allWay"}, [])
                    Action.Wait (numExpr "3")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                  ]
                )
              )
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "1")
            ]
          )
        ]
      )

  /// 怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_1.xml
  let kitiku_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "fast")}, None, Some (Speed (None, numExpr "10")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "6")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "20")
                  Action.Repeat (Times (numExpr "10+$rank*18"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-11-$rand*2")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                        Action.Repeat (Times (numExpr "4"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1+$rank*0.2")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                            ]
                          )
                        )
                        Action.Wait (numExpr "336/(10+$rank*18)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "add3")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "slowColorChange")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+45*$1")), Some (Speed (None, numExpr "7")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "6")
                    Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                    Action.Repeat (Times (numExpr "50+$rank*50"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "(8-$rank*4)*$1")), Some (Speed (None, numExpr "1.2")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                          Action.Wait (numExpr "8-$rank*4+$rand")
                        ]
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "slow")}, None, None,
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "slowColorChange"}, ["$1"])
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-85")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fast"}, [])
              )
              Action.Wait (numExpr "1")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "85")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fast"}, [])
              )
              Action.Wait (numExpr "1")
              Action.FireRef ({fireRefLabel = FireLabel "slow"}, ["1"])
              Action.Wait (numExpr "1")
              Action.FireRef ({fireRefLabel = FireLabel "slow"}, ["-1"])
              Action.Wait (numExpr "430")
            ]
          )
        ]
      )

  /// 怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_2.xml
  let kitiku_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "feather")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "6")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
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
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10-(3+$rank*6)*3")), Some (Speed (None, numExpr "1")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "3+$rank*6"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.15")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                            ]
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "feather"}, [])
              )
              Action.Wait (numExpr "1")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "feather"}, [])
              )
              Action.Wait (numExpr "430")
            ]
          )
        ]
      )

  /// 怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_3.xml
  let kitiku_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "200+$rank*200"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-50+$rand*100")), Some (Speed (None, numExpr "1.6")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "2-$rank+$rand")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "kobati")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "$1")
                  Action.Repeat (Times (numExpr "20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "(16-$rank*8)*3")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Repeat (Times (numExpr "8+$rank*8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["(16-$rank*8)*3"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["(16+$rank*8)*3"])
                    )
                    Action.Wait (numExpr "16-$rank*8")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["(16-$rank*8)*2"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["(16-$rank*8)*2"])
                    )
                    Action.Wait (numExpr "16-$rank*8")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["16-$rank*8"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-80")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, ["16-$rank*8"])
                    )
                    Action.Wait (numExpr "16-$rank*8")
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
        ]
      )

  /// 怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん
  /// [Dodonpachi]_kitiku_5.xml
  let kitiku_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "30+$rank*30"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "220+$rand*2")), Some (Speed (None, numExpr "1.2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "1.2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120")), Some (Speed (None, numExpr "1.2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "1.2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "20-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Repeat (Times (numExpr "30+$rank*30"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30+$rand*60")), Some (Speed (None, numExpr "1.3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            []
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "20-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "kobati")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "5")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.2"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "30-$rank*10")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "45")), Some (Speed (None, numExpr "0.4+$rank*0.2")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (None, numExpr "0.4+$rank*0.2")),
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
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.Repeat (Times (numExpr "5+$rank*5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "10")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "10")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), Some (Speed (None, numExpr "5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "kobati"}, [])
                    )
                    Action.Wait (numExpr "120-$rank*60")
                  ]
                )
              )
              Action.Wait (numExpr "120")
            ]
          )
        ]
      )
