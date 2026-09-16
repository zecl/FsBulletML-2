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
/// Garegga
[<RequireQualifiedAccess>]
module Garegga =

  /// バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん
  /// [Garegga]_black_heart_mk2_winder.xml
  let black_heart_mk2_winder =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "135")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winder"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "225")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winder"}, [])
              )
              Action.Wait (numExpr "220")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "winder")}, None, Some (Speed (None, numExpr "2.3")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "230")), None,
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
                  Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["0"; "40"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["0.7+$rank"; "20"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["-0.7-$rank"; "40"])
                  Action.ActionRef ({actionRefLabel = ActionLabel "move"}, ["0.7+$rank"; "20"])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "move")},
            [
              Action.Repeat (Times (numExpr "$2"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1-100")), Some (Speed (None, numExpr "5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Repeat (Times (numExpr "4"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "25")), Some (Speed (None, numExpr "5")),
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
        ]
      )
