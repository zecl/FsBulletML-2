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
/// MDA
[<RequireQualifiedAccess>]
module MAD =

  /// 紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん
  /// [MDA]_2f.xml
  let b2f =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["0"; "57"; "0.8"; "0.8"; "0"; "-0.8"; "0"])
              Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["270"; "206"; "1.73"; "0"; "-1.2"; "0"; "1.2"])
              Action.FireRef ({fireRefLabel = FireLabel "seed2"}, [])
              Action.Wait (numExpr "3*(260+(60-($rank*60)))")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$3")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "90")
                    Action.FireRef ({fireRefLabel = FireLabel "leaf"}, ["1"; "$1"; "$4"; "$5"; "$6"; "$7"])
                    Action.FireRef ({fireRefLabel = FireLabel "leaf"}, ["-1"; "$1"; "$4"; "$5"; "$6"; "$7"])
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "leaf")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "50")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "5.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$1"])
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["35"; "$1"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["120"; "$1/2"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["45"; "0"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["90"; "$3/2"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["60-($rank*60)"; "0"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["120"; "$4*3/8"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["60-($rank*60)"; "0"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["90"; "$5/2"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["60-($rank*60)"; "0"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["120"; "$6*3/8"; "$1"])
                    Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["45"; "0"; "$1"])
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "move")},
            [
              Action.Repeat (Times (numExpr "$1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (None, numExpr "5.1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$3"])
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "curve")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1*85"), Term (numExpr "9-($rank*5)"))
                ]
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed2")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "131.5")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.05")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "90")
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
                    Action.Repeat (Times (numExpr "10"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.4+(0.4*$rank*$rank)")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "3*$rand*$rank")), Some (Speed (None, numExpr "1.4+(0.4*$rand*$rank*$rank)")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-3*$rand*$rank")), Some (Speed (None, numExpr "1.4+(0.4*$rand*$rank*$rank)")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.Wait (numExpr "3*(260+(60-($rank*60)))/10")
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

  /// 紫月飴さんのオリジナル、花。by 白い弾幕くん
  /// [MDA]_10flower_2.xml
  let b10flower_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、花。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["36.1"; "144"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, ["-36.4"; "3.4+144"])
              )
              Action.Wait (numExpr "164+316*$rank")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "4"))
                  Action.Wait (numExpr "4")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "1.1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                  )
                  Action.Repeat (Times (numExpr "21+79*$rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "10"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1.1")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// 紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん
  /// [MDA]_14b_2-3w.xml
  let b14b_2_3w =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "seed_a"}, [])
              Action.Repeat (Times (numExpr "35+$rank*21"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "seed_b"}, [])
              )
              Action.Wait (numExpr "110")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "seed_b")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "7")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
              )
              Action.Repeat (Times (numExpr "$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.14")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
              )
              Action.Repeat (Times (numExpr "$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "-0.14")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
                    )
                  ]
                )
              )
              Action.Wait (numExpr "11")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shoot")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "18")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "1.4")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "7-1"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/7")), Some (Speed (None, numExpr "1.4")),
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
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed_a")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "3+$rank*7"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.FireRef ({fireRefLabel = FireLabel "fire1"}, ["1"; "1"])
                          Action.FireRef ({fireRefLabel = FireLabel "fire1"}, ["-1"; "1"])
                          Action.FireRef ({fireRefLabel = FireLabel "fire1"}, ["0.5"; "-2"])
                          Action.FireRef ({fireRefLabel = FireLabel "fire1"}, ["-0.5"; "-2"])
                          Action.Wait (numExpr "63")
                        ]
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "fire1")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1*90")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.5")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10")
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.5"), Term (numExpr "1"))
                    Action.Wait (numExpr "1")
                    Action.Repeat (Times (numExpr "4+$rank*5"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.FireRef ({fireRefLabel = FireLabel "fire2"}, ["$2*$1"; "-1"])
                          Action.FireRef ({fireRefLabel = FireLabel "fire2"}, ["$2*$1"; "1"])
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
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "fire2")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "($1+$2)*7")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.2+$rank*1")),
            BulletElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  []
                )
              ]
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん
  /// [MDA]_75l-42.xml
  let b75l_42 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), Some (Speed (None, numExpr "9.2-$rank*4")),
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), Some (Speed (None, numExpr "9.2-$rank*4")),
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                  ]
                )
              )
              Action.Wait (numExpr "40")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "240")), Some (Speed (None, numExpr "9.2-$rank*4")),
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "right"}, [])
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), Some (Speed (None, numExpr "9.2-$rank*4")),
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "left"}, [])
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "right")},
            [
              Action.Repeat (Times (numExpr "32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["0+1"; "1.4"])
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["60+1"; "0.7"])
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["300+1"; "2.1"])
                    Action.Wait (numExpr "1")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "left")},
            [
              Action.Repeat (Times (numExpr "32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["360-1"; "1.4"])
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["300-1"; "0.7"])
                    Action.FireRef ({fireRefLabel = FireLabel "shoot"}, ["60-1"; "2.1"])
                    Action.Wait (numExpr "1")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "shoot")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "45")
                    Action.Accel (None, Some (Vertical (None, numExpr "4.2")), Term (numExpr "120"))
                  ]
                )
              ]
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん
  /// [MDA]_acc_n_dec.xml
  let acc_n_dec =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "5+(20*$rank)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["90"])
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["270"])
                    Action.Wait (numExpr "55-($rank*30)")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "2.0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "20"))
                    Action.Wait (numExpr "20")
                    Action.ActionRef ({actionRefLabel = ActionLabel "way"}, [])
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*60-30-70")), Some (Speed (None, numExpr "4.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "br"}, [])
              )
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "8.5")), Some (Speed (None, numExpr "1.05")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "ac"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "8.5")), Some (Speed (None, numExpr "4.2")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "br"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "br")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "1.05"), Term (numExpr "25"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "ac")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "8.4"), Term (numExpr "150"))
                  Action.Wait (numExpr "9999")
                  Action.Fire ({fireLabel = None}, None, None,
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

  /// 紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん
  /// [MDA]_circular.xml
  let circular =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "$rank*10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["90"; "2"; "355"])
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["270"; "358"; "5"])
                    Action.FireRef ({fireRefLabel = FireLabel "aimbl"}, [])
                    Action.Wait (numExpr "20")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "9"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "aimbl"}, [])
                    Action.Wait (numExpr "20")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "2.8")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["$2"; "$3"])
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1"), Term (numExpr "9999"))
                  Action.ActionRef ({actionRefLabel = ActionLabel "shoot"}, ["$2"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "shoot")},
            [
              Action.Repeat (Times (numExpr "22"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1.4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "4+$rand*8")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "aimbl")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "2.8")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、四方から。 by 白い弾幕くん
  /// [MDA]_circular_model.xml
  let circular_model =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、四方から。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["90"; "2"; "355"])
              Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["270"; "358"; "5"])
              Action.Wait (numExpr "380-$rank*200")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "2.8")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["$2"; "$3"])
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1"), Term (numExpr "9999"))
                  Action.ActionRef ({actionRefLabel = ActionLabel "shoot"}, ["$2"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "shoot")},
            [
              Action.Repeat (Times (numExpr "22*8"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "0.4+$rank")),
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

  /// 紫月飴さんのオリジナル、春っぽい by 白い弾幕くん
  /// [MDA]_circular_sun.xml
  let circular_sun =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、春っぽい by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.75"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90"), Term (numExpr "1"))
              Action.Wait (numExpr "1")
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0.7"), Term (numExpr "514"))
              Action.Wait (numExpr "2")
              Action.Repeat (Times (numExpr "32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "shoot"}, [])
                    Action.Wait (numExpr "16")
                  ]
                )
              )
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "120")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "shoot")},
            [
              Action.Repeat (Times (numExpr "1+(63*$rank)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/(1+(63*$rank))")), Some (Speed (None, numExpr "1.28+(0.08*$rand)")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, [])
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
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "1.25-(1.6*$rand)"), Term (numExpr "360"))
                  Action.Wait (numExpr "360")
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )

  /// 紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん
  /// [MDA]_double_w.xml
  let double_w =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, ["0.31"])
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, ["0.00"])
              Action.Wait (numExpr "30")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "seed")},
            [
              Action.Repeat (Times (numExpr "($rank*$rank*50+21)*(2-$1)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "shoot1"}, ["$1"])
                    Action.Repeat (Times (numExpr "10*(1+$1)"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.FireRef ({fireRefLabel = FireLabel "shoot2"}, [])
                        ]
                      )
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "shoot1")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "41")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.0-$1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "shoot2")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-19")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、袋詰め by 白い弾幕くん
  /// [MDA]_fukuro.xml
  let fukuro =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、袋詰め by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
              Action.ChangeSpeed (Speed (None, numExpr "1"), Term (numExpr "1"))
              Action.Wait (numExpr "30")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Repeat (Times (numExpr "$rank*17+1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["3"; "$rank*18+1"])
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["2"; "$rank*18+1"])
                    Action.FireRef ({fireRefLabel = FireLabel "seed"}, ["1"; "$rank*18+1"])
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Wait (numExpr "(3*($rank*18*10))")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "seed")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/($2*3)")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1.75*$1")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10")
                    Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "10"))
                    Action.Wait (numExpr "(($1-1)*($2*10))+30")
                    Action.ActionRef ({actionRefLabel = ActionLabel "n_way"}, [])
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "n_way")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["2.00"; "60"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["2.00"; "-60"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.64"; "52.5"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.64"; "-52.5"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.41"; "45"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.41"; "-45"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.16"; "30"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.16"; "-30"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.04"; "15"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.04"; "-15"])
              Action.FireRef ({fireRefLabel = FireLabel "curve"}, ["1.00"; "0"])
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "curve")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$1*2.0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "5")
                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "5"))
                  ]
                )
              ]
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん
  /// [MDA]_gnnnyari.xml
  let gnnnyari =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, [])
              Action.Wait (numExpr "120")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "seed")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "7.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
              )
              Action.Wait (numExpr "2")
              Action.Repeat (Times (numExpr "30+$rank*80"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "27")), Some (Speed (None, numExpr "7.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "shoot"}, [])
                    )
                    Action.Wait (numExpr "2")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "shoot")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), Some (Speed (None, numExpr "1.0+0.4*$rank")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
                  )
                  Action.Repeat (Times (numExpr "11"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (None, numExpr "1.0+0.4*$rank")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                []
              )
            ]
          )
        ]
      )

  /// 紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん
  /// [MDA]_mojya.xml
  let mojya =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "15+25*$rank"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "first"}, ["15"])
                    Action.FireRef ({fireRefLabel = FireLabel "first"}, ["153"])
                    Action.Wait (numExpr "3")
                  ]
                )
              )
              Action.Wait (numExpr "240")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "first")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "0.54")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "second"}, [])
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "second")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["21+$rand*84"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["-21-$rand*84"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["7+$rand*28"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["-7-$rand*28"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["$rand*14"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["$rand*(-14)"])
                  Action.FireRef ({fireRefLabel = FireLabel "third"}, ["0"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "third")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1")), Some (Speed (None, numExpr "0.4+$rand*1.4")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// 紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん
  /// [MDA]_mossari.xml
  let mossari =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, ["-2"; "0"])
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, ["25"; "10"])
              Action.ActionRef ({actionRefLabel = ActionLabel "seed"}, ["41"; "-10"])
              Action.ActionRef ({actionRefLabel = ActionLabel "center"}, [])
              Action.Wait (numExpr "180")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "seed")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$1/4-2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "shoot"}, ["-1*$2"])
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180-$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "$1/4-2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "shoot"}, ["$2"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "shoot")},
            [
              Action.Wait (numExpr "9")
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "4"))
              Action.Wait (numExpr "4")
              Action.Repeat (Times (numExpr "10+($rank*30)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "shoot2"}, ["0+($rand*30)"; "$1"])
                    Action.FireRef ({fireRefLabel = FireLabel "shoot2"}, ["0-($rand*30)"; "$1"])
                    Action.Wait (numExpr "24-($rand*12)")
                  ]
                )
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "shoot2")}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$1+$2")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.6")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "center")},
            [
              Action.Wait (numExpr "10")
              Action.Repeat (Times (numExpr "12+($rank*20)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.ActionRef ({actionRefLabel = ActionLabel "center2"}, [])
                    Action.Repeat (Times (numExpr "7-1"),
                      ActionElm.ActionRef ({actionRefLabel = ActionLabel "center3"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "center2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-60")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.6")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "12"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0.6")),
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "center3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "wind"}, ["46"])
              Action.ActionRef ({actionRefLabel = ActionLabel "wind"}, ["16"])
              Action.ActionRef ({actionRefLabel = ActionLabel "wind"}, ["47.5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "wind"}, ["15"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "wind")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.7")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180-$1")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2.7")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Wait (numExpr "1")
            ]
          )
        ]
      )

  /// 紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん
  /// [MDA]_wind_cl.xml
  let wind_cl =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "side"}, ["120"])
              Action.FireRef ({fireRefLabel = FireLabel "side"}, ["240"])
              Action.Wait (numExpr "31")
              Action.Repeat (Times (numExpr "5+$rank*20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "center"}, [])
                    )
                    Action.Wait (numExpr "30")
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "side")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "18.6")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "1")
                    Action.ChangeSpeed (Speed (None, numExpr "0.02"), Term (numExpr "2"))
                    Action.Wait (numExpr "30")
                    Action.ChangeDirection (Direction (None, numExpr "0"), Term (numExpr "1"))
                    Action.Repeat (Times (numExpr "77+$rank*306"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "2")
                          Action.ChangeDirection (Direction (None, numExpr "0"), Term (numExpr "30"))
                          Action.FireRef ({fireRefLabel = FireLabel "3way"}, ["0"])
                          Action.FireRef ({fireRefLabel = FireLabel "3way"}, ["20"])
                          Action.FireRef ({fireRefLabel = FireLabel "3way"}, ["-20"])
                        ]
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "3way")}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), Some (Speed (None, numExpr "4.9")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "2way")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "$1")), Some (Speed (None, numExpr "2.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-$1")), Some (Speed (None, numExpr "2.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, [])
              )
              Action.Wait (numExpr "5")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                []
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "center")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0.01"), Term (numExpr "1"))
                  Action.ActionRef ({actionRefLabel = ActionLabel "2way"}, ["0"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "2way"}, ["8-$rank*4"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "2way"}, ["16-$rank*8"])
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )
