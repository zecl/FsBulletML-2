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
/// Xsoldier
[<RequireQualifiedAccess>]
module Xsoldier =

  /// XSoldierの8面ボスの主砲 by 白い弾幕くん
  /// [xsoldier]_8_boss_main.xml
  let b8_boss_main =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "XSoldierの8面ボスの主砲 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["90"])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "dummy"}, ["270"])
              )
              Action.Wait (numExpr "100 - $rank * 90")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "allway"}, ["0"; "1.5"])
              )
              Action.Wait (numExpr "5")
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "0")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "allway"}, ["2.5"; "1.8"])
              )
              Action.Wait (numExpr "20")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "dummy")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "0.5")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1.0")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                  )
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "1.5")),
                    BulletElm.BulletRef ({bulletRefLabel = BulletLabel "bit"}, [])
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "allway")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "$1")), Some (Speed (None, numExpr "$2")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.Repeat (Times (numExpr "71"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), Some (Speed (None, numExpr "$2")),
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "bit")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "20")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Wait (numExpr "105 - $rank * 90")
                  Action.Repeat (Times (numExpr "20"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3.5")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "4")),
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
