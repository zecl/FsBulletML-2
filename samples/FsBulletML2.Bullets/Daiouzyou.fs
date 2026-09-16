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
/// Daiouzyou
[<RequireQualifiedAccess>]
module Daiouzyou =

  /// 怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_1.xml
  let hibachi_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10+$rank*70"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "$rand*30-74+$rank*2")), Some (Speed (None, numExpr "0.5+$rank*2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "n"}, [])
                    Action.Wait (numExpr "14-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "n")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$rand*2+7-$rank*2")), Some (Speed (None, numExpr "0.5+$rank*2")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// 怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_2.xml
  let hibachi_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "175")), Some (Speed (None, numExpr "1+$rank*4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "30"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.Wait (numExpr "1")
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "rights"}, [])
                    Action.Wait (numExpr "15-$rank*10")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4")), Some (Speed (None, numExpr "1+$rank*4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "tops")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "185")), Some (Speed (None, numExpr "1+$rank*4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "30"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
                    Action.Wait (numExpr "1")
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.ActionRef ({actionRefLabel = ActionLabel "lefts"}, [])
                    Action.Wait (numExpr "15-$rank*10")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-4")), Some (Speed (None, numExpr "1+$rank*4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "lefts")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-0.7")), Some (Speed (None, numExpr "1+$rank*4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.Wait (numExpr "1")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "rights")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0.7")), Some (Speed (None, numExpr "1+$rank*4")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.FireRef ({fireRefLabel = FireLabel "allway"}, [])
              Action.Wait (numExpr "1")
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "allway")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "45")), Some (Speed (None, numExpr "1+$rank*4")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
        ]
      )

  /// 怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_3.xml
  let hibachi_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "2"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
              Action.Wait (numExpr "10")
              Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute}, numExpr "0"), Term (numExpr "1"))
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "1.5")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blue"}, [])
              )
              Action.Repeat (Times (numExpr "60+$rank*60"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "red"}, [])
                    Action.Wait (numExpr "20-$rank*14")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-31.5")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blue"}, [])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "red")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (None, numExpr "1.5")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blue"}, [])
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "blue")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-110")), Some (Speed (None, numExpr "1.5")),
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

  /// 怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_4.xml
  let hibachi_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "45")), Some (Speed (None, numExpr "1+$rank*0.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "113+900/(16-$rank*10)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "86")), Some (Speed (None, numExpr "1+$rank*0.5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "tops")},
            [
              Action.Wait (numExpr "(16-$rank*10)*22.5")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "45")), Some (Speed (None, numExpr "1+$rank*0.5")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "91+900/(16-$rank*10)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "four"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "94")), Some (Speed (None, numExpr "1+$rank*0.5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "topt")},
            [
              Action.Wait (numExpr "(16-$rank*10)*45")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "gurugurup"}, [])
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "gurugurup")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.ActionRef ({actionRefLabel = ActionLabel "guru2"}, [])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "guru2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "1+$rank")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc"}, [])
              )
              Action.Repeat (Times (numExpr "450/(16-$rank*10)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru2"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc2"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru3"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc3"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru4"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc4"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru5"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc5"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru6"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc6"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru7"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "21")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc7"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru8"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "21.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc8"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru9"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc9"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru10"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.7")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc10"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "7"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru11"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "23")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc11"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru12"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "23.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc12"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru13"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc13"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru14"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc14"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru15"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc15"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru16"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc16"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru17"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc17"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru18"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc18"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "450/(16-$rank*10)"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.FireRef ({fireRefLabel = FireLabel "guru19"}, [])
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "24.5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc19"}, [])
                    )
                    Action.Wait (numExpr "16-$rank*10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru2")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc2"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru3")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc3"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru4")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc4"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru5")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc5"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru6")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc6"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru7")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc7"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru8")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc8"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru9")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc9"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru10")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.3+$rand*0.4")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc10"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru11")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc11"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru12")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc12"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru13")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc13"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru14")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc14"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru15")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc15"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru16")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc16"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru17")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc17"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru18")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc18"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "guru19")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), Some (Speed (None, numExpr "1+$rank")),
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "guruc19"}, [])
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "four")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), Some (Speed (None, numExpr "1+$rank*0.5")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              []
            )
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "90"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc2")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "170"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc3")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "260"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc4")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "300"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc5")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "450"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc6")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "600"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc7")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "700"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc8")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "800"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc9")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "-270"), Term (numExpr "900"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc10")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "0"), Term (numExpr "90"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc11")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "900"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc12")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "800"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc13")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "700"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc14")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "600"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc15")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "450"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc16")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "300"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc17")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "280"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc18")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "230"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "guruc19")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Relative}, numExpr "270"), Term (numExpr "90"))
                ]
              )
            ]
          )
        ]
      )

  /// 怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん
  /// [Daiouzyou]_hibachi_image.xml
  let hibachi_image =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん"; bulletmlDescription = None},
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "spiral")},
            [
              Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "30+$rank*45"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank*2")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["10"; "36"])
                              Action.Wait (numExpr "6-$rank*3")
                            ]
                          )
                        )
                        Action.Vanish
                      ]
                    )
                  ]
                )
              )
              Action.Wait (numExpr "225")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "spiral"}, ["7"])
              Action.ActionRef ({actionRefLabel = ActionLabel "spiral"}, ["-7"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fan4")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-$1")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "60+$rank*90"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank*2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["4"; "90"])
                    Action.Wait (numExpr "6-$rank*3")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan4"}, ["4"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan4"}, ["-4"])
            ]
          )
        ]
      )

  /// 怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん
  /// [Daiouzyou]_hibachi_maybe.xml
  let hibachi_maybe =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん"; bulletmlDescription = None},
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
              Action.Repeat (Times (numExpr "10+$rank*15"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "2"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1.5+$rank*$rank*1.5")),
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$1"])
                          )
                          Action.Repeat (Times (numExpr "10+$rank*10-1"),
                            ActionElm.Action ({actionLabel = None},
                              [
                                Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "36/($rank+1)")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                                  BulletElm.BulletRef ({bulletRefLabel = BulletLabel "curve"}, ["$1"])
                                )
                              ]
                            )
                          )
                          Action.Wait (numExpr "6-$rank*3")
                        ]
                      )
                    )
                    Action.Wait (numExpr "6-$rank*3")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank*2")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
                    )
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fan4")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-$1")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "30+$rank*45"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank*$rank*2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["4"; "90"])
                    Action.Wait (numExpr "6-$rank*3")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan4"}, ["4"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan4"}, ["-4"])
            ]
          )
        ]
      )

  /// 怒首領蜂大往生一面ボス by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss.xml
  let round_1_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "怒首領蜂大往生一面ボス by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
                    )
                    Action.Wait (numExpr "500")
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "9")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed2"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed2"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed2")}, None, Some (Speed (None, numExpr "18")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed3"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed3")}, None, Some (Speed (None, numExpr "0.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "1.2"), Term (numExpr "9999"))
                  Action.Repeat (Times (numExpr "100+200*$rank"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180-12")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "180")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "3-$rank*2*$rand")
                      ]
                    )
                  )
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.Bullet ({bulletLabel = None}, Some (Direction (None, numExpr "-8")), None,
                            []
                          )
                        )
                        Action.Repeat (Times (numExpr "4"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, None, None,
                                BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4")), None,
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
                        Action.Wait (numExpr "80")
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

  /// 怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss_hakkyou.xml
  let round_1_boss_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "128"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4")
                    Action.ActionRef ({actionRefLabel = ActionLabel "four"}, ["$rand*90+135"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "four")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rb"}, ["$1"])
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "60")), Some (Speed (None, numExpr "6")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rb"}, ["$1"])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rb")}, None, None,
            [
              ActionElm.ActionRef ({actionRefLabel = ActionLabel "red"}, ["$1+$rand*20-10"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "red")},
            [
              Action.Wait (numExpr "1")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "160")
                    Action.Fire ({fireLabel = None}, None, None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sht"}, ["1.2"])
                    )
                    Action.Wait (numExpr "80")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "sht")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*16-8")), Some (Speed (None, numExpr "($1+$rand*$1)*($rank/2+0.65)")),
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rd_seed"}, ["-5"; "-5"])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rd_seed"}, ["5"; "5"])
                    )
                    Action.Wait (numExpr "240")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rd_seed")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rd_seed2"}, [])
                  )
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bd_seed"}, ["0"; "$2"])
                  )
                  Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bd_seed"}, ["$1"; "$2"])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rd_seed2")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1.2")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Wait (numExpr "4")
                            ]
                          )
                        )
                        Action.Wait (numExpr "12")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bd_seed")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$2")), Some (Speed (None, numExpr "0.6")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "11"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.2")),
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

  /// 怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss.xml
  let round_3_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "155")), Some (Speed (None, numExpr "3.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "205")), Some (Speed (None, numExpr "3.3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), Some (Speed (None, numExpr "3.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "225")), Some (Speed (None, numExpr "3.2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["-1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "225")), Some (Speed (None, numExpr "2")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, ["-1"])
              )
              Action.Wait (numExpr "400")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "12")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180+90*$1")), None,
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
                  Action.Repeat (Times (numExpr "200"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "9")), Some (Speed (None, numExpr "1+$rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
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
        ]
      )

  /// 怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_2.xml
  let round_3_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん"; bulletmlDescription = None},
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim2"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "aim2"}, [])
              )
              Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "fanRoll"}, ["7"])
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, None, Some (Speed (None, numExpr "0")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.ActionRef ({actionRefLabel = ActionLabel "fanRoll"}, ["-7"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "3wayRoll"}, ["13"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "3wayRoll"}, ["-13"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "3wayRoll")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "14"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-1.3*$1")), Some (Speed (None, numExpr "1.4+$rank*0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "$1"])
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "1.3*$1")), Some (Speed (None, numExpr "1.4+$rank*0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "-$1"])
                    Action.Wait (numExpr "10")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fanRoll")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1*8")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "32"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-$1*2.1")), Some (Speed (None, numExpr "1.2+$rank*0.4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWayFan"}, ["4"; "$1"; "0.3"])
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "aim2")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "8")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "14+$rank*12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "320/(14+$rank*12)+$rand")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.4+$rank*0.8")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
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

  /// 怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_last.xml
  let round_3_boss_last =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん"; bulletmlDescription = None},
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "100")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "armSrc"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-100")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "armSrc"}, ["0"])
              )
              Action.FireRef ({fireRefLabel = FireLabel "center"}, [])
              Action.Wait (numExpr "500")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "center3")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-10.5*$1")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "6"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "1+$rank")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
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
                          Action.Wait (numExpr "5")
                        ]
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), None,
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
                    Action.Wait (numExpr "5")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "center")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "5")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "10")
                    Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                    Action.Repeat (Times (numExpr "2"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.ActionRef ({actionRefLabel = ActionLabel "center3"}, ["-4"])
                          Action.Wait (numExpr "30")
                          Action.ActionRef ({actionRefLabel = ActionLabel "center3"}, ["4"])
                          Action.Wait (numExpr "30")
                        ]
                      )
                    )
                    Action.Vanish
                  ]
                )
              ]
            )
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "armSrc")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "12")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "1")
                  Action.FireRef ({fireRefLabel = FireLabel "arm"}, ["8-16*$1"; "0"])
                  Action.Wait (numExpr "2")
                  Action.FireRef ({fireRefLabel = FireLabel "arm"}, ["8-16*$1"; "90"])
                  Action.Wait (numExpr "2")
                  Action.FireRef ({fireRefLabel = FireLabel "arm"}, ["8-16*$1"; "180"])
                  Action.Wait (numExpr "2")
                  Action.FireRef ({fireRefLabel = FireLabel "arm"}, ["8-16*$1"; "270"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "arm")}, None, Some (Speed (None, numExpr "0")),
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Repeat (Times (numExpr "80+$rank*80"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "480/(80+$rank*80)")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
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
          )
        ]
      )

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss.xml
  let round_4_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "110")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "armSrc"}, ["1"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-110")), Some (Speed (None, numExpr "3")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "armSrc"}, ["0"])
              )
              Action.Wait (numExpr "400")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "armSrc")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "12")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "1"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "1"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-60")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "1"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "-1"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "60")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "-1"])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-60")), Some (Speed (None, numExpr "1")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, ["$1"; "-1"])
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
                  Action.Wait (numExpr "12")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180*$1")), None,
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
                  Action.Repeat (Times (numExpr "400/(6-$rank*2)"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "6-$rank*2+$rand")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "11*$2")), Some (Speed (None, numExpr "1.5+$rank*0.5")),
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

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_1.xml
  let round_4_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "20")
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "116+$rand*6-$rank*15")), Some (Speed (None, numExpr "1.5")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Repeat (Times (numExpr "3.5+$rank*5"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3")), Some (Speed (None, numExpr "1.5")),
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
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "xway")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-7*$1-7")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "$1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "1.3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            []
                          )
                        ]
                      )
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.1")),
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
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "110")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-110")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
              )
              Action.Wait (numExpr "400")
            ]
          )
        ]
      )

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_2.xml
  let round_4_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "blue")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "16+$rank*16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "10-$rank*4+$rand")
                        Action.Repeat (Times (numExpr "3"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "95")), Some (Speed (None, numExpr "1.4")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.Repeat (Times (numExpr "3"),
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), Some (Speed (None, numExpr "1.4")),
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
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "xway")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-7*$1-7")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.Repeat (Times (numExpr "$1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "1.3")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            []
                          )
                        ]
                      )
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.08+$rank*0.08")),
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
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "red")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ActionRef ({actionRefLabel = ActionLabel "xway"}, ["$rand*3+$rank*2"])
                        Action.Wait (numExpr "40")
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
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blue"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
              )
              Action.Wait (numExpr "200")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blue"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "120")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "red"}, [])
              )
              Action.Wait (numExpr "200")
            ]
          )
        ]
      )

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_4.xml
  let round_4_boss_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん"; bulletmlDescription = None},
        [
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fan")},
            [
              Action.Wait (numExpr "30")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1.2+$rank")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["$2"; "$3"])
              Action.Repeat (Times (numExpr "6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "30")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$4")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["$2"; "$3"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["220"; "8"; "5"; "-42.5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["150"; "8"; "-5"; "42.5"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["200"; "7"; "2.5"; "-22.5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["170"; "7"; "-2.5"; "22.5"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top3")},
            [
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["160"; "8"; "5"; "-42.5"])
              Action.ActionRef ({actionRefLabel = ActionLabel "fan"}, ["210"; "8"; "-5"; "42.5"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top4")},
            [
              Action.Wait (numExpr "20")
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Repeat (Times (numExpr "36+$rank*20"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*360")), Some (Speed (None, numExpr "2")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              [
                                ActionElm.Action ({actionLabel = None},
                                  [
                                    Action.Wait (numExpr "10*$rand")
                                    Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                                    Action.Wait (numExpr "60")
                                    Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "1"))
                                    Action.ChangeSpeed (Speed (None, numExpr "2.4"), Term (numExpr "1"))
                                  ]
                                )
                              ]
                            )
                          )
                          Action.Wait (numExpr "3")
                        ]
                      )
                    )
                    Action.Wait (numExpr "60-$rank*60")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// 怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_5.xml
  let round_4_boss_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん"; bulletmlDescription = None},
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "blueFan")}, None, Some (Speed (None, numExpr "3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "6"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120+$1*2")), Some (Speed (None, numExpr "1.6")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "120"])
                        Action.Repeat (Times (numExpr "6+$rank*6"),
                          ActionElm.Action ({actionLabel = None},
                            [
                              Action.Wait (numExpr "56/(6+$rank*6)")
                              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "120+$1")), Some (Speed (None, numExpr "1.6")),
                                BulletElm.Bullet ({bulletLabel = None}, None, None,
                                  []
                                )
                              )
                              Action.ActionRef ({actionRefLabel = ActionLabel "XWay"}, ["3"; "120"])
                            ]
                          )
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
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "singleRedAim")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      []
                    )
                  ]
                )
              )
              Action.Repeat (Times (numExpr "15"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), Some (Speed (None, numExpr "2")),
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
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "doubleRedAim")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-5*$1")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      []
                    )
                  ]
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20*$1")), Some (Speed (None, numExpr "2")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  [
                    ActionElm.Action ({actionLabel = None},
                      []
                    )
                  ]
                )
              )
              Action.Repeat (Times (numExpr "15"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "4")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-20*$1")), Some (Speed (None, numExpr "2")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        [
                          ActionElm.Action ({actionLabel = None},
                            []
                          )
                        ]
                      )
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "20*$1")), Some (Speed (None, numExpr "2")),
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
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "redAim2")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Wait (numExpr "100")
                  Action.ActionRef ({actionRefLabel = ActionLabel "singleRedAim"}, [])
                  Action.Wait (numExpr "60")
                  Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim"}, ["-1"])
                  Action.Wait (numExpr "20")
                  Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim"}, ["-1"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "redAim1")}, None, Some (Speed (None, numExpr "1")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Wait (numExpr "40")
                  Action.ActionRef ({actionRefLabel = ActionLabel "singleRedAim"}, [])
                  Action.Wait (numExpr "60")
                  Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim"}, ["1"])
                  Action.Wait (numExpr "80")
                  Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim"}, ["1"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blueFan"}, ["4"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "blueFan"}, ["-4"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "redAim2"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-90")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "redAim1"}, [])
              )
              Action.Wait (numExpr "400")
            ]
          )
        ]
      )

  /// 怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_1.xml
  let round_5_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん"; bulletmlDescription = None},
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

  /// 怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_2.xml
  let round_5_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "Red")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                []
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "Stop")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seven")}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Repeat (Times (numExpr "5+$rank*4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-10")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "10")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-5")), Some (Speed (None, numExpr "1.3")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "5")), Some (Speed (None, numExpr "1.3")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-5")), Some (Speed (None, numExpr "1.7")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "5")), Some (Speed (None, numExpr "1.7")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "1.5")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Red"}, [])
                        )
                        Action.Wait (numExpr "360/(5+$rank*4)")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "fan")}, None, Some (Speed (None, numExpr "4")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ActionRef ({actionRefLabel = ActionLabel "Stop"}, [])
                  Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$1")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "Dummy"}, [])
                  )
                  Action.Repeat (Times (numExpr "35+$rank*35"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$2")), Some (Speed (None, numExpr "$3")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "10/(1+$rank)+$rand")
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
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seven"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["55"; "10"; "1.8+$rank*0.4"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["60"; "10"; "1+$rank*0.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["225"; "10"; "1.4+$rank*0.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["250"; "10"; "1.3+$rank*0.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["55"; "-10"; "1.8+$rank*0.4"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["60"; "-10"; "1+$rank*0.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["225"; "-10"; "1.4+$rank*0.2"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-170")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fan"}, ["250"; "-10"; "1.3+$rank*0.2"])
              )
              Action.Wait (numExpr "360")
            ]
          )
        ]
      )

  /// 怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_1.xml
  let round_6_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "64"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Wait (numExpr "2")
                    Action.ActionRef ({actionRefLabel = ActionLabel "four"}, ["$rand*90+135"])
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "four")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), Some (Speed (None, numExpr "6")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rb"}, ["$1"])
              )
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "60")), Some (Speed (None, numExpr "6")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "rb"}, ["$1"])
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "rb")}, None, None,
            [
              ActionElm.ActionRef ({actionRefLabel = ActionLabel "red"}, ["$1+$rand*20-10"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "red")},
            [
              Action.Wait (numExpr "1")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1.2+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Vanish
            ]
          )
        ]
      )

  /// 怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_2.xml
  let round_6_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "4"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180"), Term (numExpr "1"))
              Action.Wait (numExpr "10")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
              Action.Repeat (Times (numExpr "5"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bl_seed"}, [])
                    )
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bl_seed"}, [])
                    )
                    Action.Wait (numExpr "80")
                  ]
                )
              )
              Action.ChangeSpeed (Speed (None, numExpr "4"), Term (numExpr "1"))
              Action.ChangeDirection (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0"), Term (numExpr "1"))
              Action.Wait (numExpr "10")
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bl_seed")}, None, Some (Speed (None, numExpr "24")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.Bullet ({bulletLabel = None}, None, Some (Speed (None, numExpr "0")),
                      [
                        ActionElm.ActionRef ({actionRefLabel = ActionLabel "bl"}, [])
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bl")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30")), Some (Speed (Some {speedType = SpeedType.Absolute}, numExpr "1")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
              Action.Wait (numExpr "4")
              Action.Repeat (Times (numExpr "3+$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-30")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.4")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
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

  /// 怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_3.xml
  let round_6_boss_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "270")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bm_seed"}, ["-25"])
                    )
                    Action.Wait (numExpr "20")
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "90")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bm_seed"}, ["25"])
                    )
                    Action.Wait (numExpr "100")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bm_seed")}, None, Some (Speed (None, numExpr "24")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3")),
                      [
                        ActionElm.ActionRef ({actionRefLabel = ActionLabel "bm"}, [])
                      ]
                    )
                  )
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), Some (Speed (None, numExpr "2")),
                      [
                        ActionElm.ActionRef ({actionRefLabel = ActionLabel "bm"}, [])
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "bm")},
            [
              Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "50"))
              Action.Wait (numExpr "45")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round"}, ["1.5"; "0"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round"}, ["1.25"; "7"])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round"}, ["1"; "14"])
              )
              Action.Vanish
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "round")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$2")), Some (Speed (None, numExpr "$1")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "10+$rank*10"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/(10+$rank*10)")), Some (Speed (None, numExpr "$1")),
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

  /// 怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_4.xml
  let round_6_boss_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sht"}, ["0.8"])
              )
              Action.Wait (numExpr "20")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Wait (numExpr "100")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sht"}, ["1"])
              )
              Action.Wait (numExpr "20")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Wait (numExpr "100")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "sht"}, ["1.2"])
              )
              Action.Wait (numExpr "20")
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "round_seed"}, [])
              )
              Action.Wait (numExpr "25")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "sht")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "$rand*16-8")), Some (Speed (None, numExpr "($1+$rand*$1)*(1+$rank*$rank)")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "round_seed")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "two"}, [])
                  )
                  Action.Repeat (Times (numExpr "15"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "22.5")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "two"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "two")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-4")), Some (Speed (None, numExpr "1+$rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "4")), Some (Speed (None, numExpr "1+$rank")),
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

  /// 怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_5.xml
  let round_6_boss_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "4")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed"}, [])
                    )
                    Action.Wait (numExpr "500")
                  ]
                )
              )
              Action.Wait (numExpr "200")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "9")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "0")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed2"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "180")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed2"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed2")}, None, Some (Speed (None, numExpr "18")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "1")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "90")), None,
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "seed3"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "seed3")}, None, Some (Speed (None, numExpr "0.8")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "1.2"), Term (numExpr "9999"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "62+$rank*100"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "40-10")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "140")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "40")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "140")), None,
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Wait (numExpr "8-$rank*6")
                      ]
                    )
                  )
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "5"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "tw"}, [])
                        )
                        Action.Wait (numExpr "138")
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "tw")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, None, None,
                    BulletElm.Bullet ({bulletLabel = None}, Some (Direction (None, numExpr "-12")), None,
                      [
                        ActionElm.Action ({actionLabel = None},
                          []
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "3.5+$rank*5+$rand"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, None, None,
                          BulletElm.Bullet ({bulletLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4")), None,
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
                  Action.Vanish
                ]
              )
            ]
          )
        ]
      )
