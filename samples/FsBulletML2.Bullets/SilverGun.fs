// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// SilverGun
[<RequireQualifiedAccess>]
module SilverGun =

  /// レイディアントシルバーガン4Dボス、PENTA。by 白い弾幕くん
  /// [SilverGun]_4D_boss_PENTA.xml
  let b4D_boss_PENTA =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "レイディアントシルバーガン4Dボス、PENTA。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "100")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-100")), Some (Speed (None, numExpr "4")),
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "arm"}, [])
              )
              Action.Repeat (Times (numExpr "400"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "7")), Some (Speed (None, numExpr "1.5")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "arm")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "12")
                  Action.ChangeSpeed (Speed (None, numExpr "0"), Term (numExpr "1"))
                  Action.Repeat (Times (numExpr "7"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Wait (numExpr "60")
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-15")), Some (Speed (None, numExpr "1.8")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "homing"}, [])
                        )
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (None, numExpr "1.8")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "homing"}, [])
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
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "homing")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "60")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "15-$rank*10"))
                  Action.Wait (numExpr "15-$rank*10")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "15-$rank*10"))
                ]
              )
            ]
          )
        ]
      )
