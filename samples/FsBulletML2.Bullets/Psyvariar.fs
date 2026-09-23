// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Psyvariar
[<RequireQualifiedAccess>]
module Psyvariar =

  /// サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん
  /// [Psyvariar]_4-D_boss_MZIQ.xml
  let b4_D_boss_MZIQ =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "add11")},
            [
              Action.Repeat (Times (numExpr "11"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "30"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-11")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "add11"}, [])
                    Action.Repeat (Times (numExpr "3"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "4-$rank*2+$rand")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-5+30")), Some (Speed (None, numExpr "1+$rank")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "add11"}, [])
                        ]
                      )
                    )
                    Action.Wait (numExpr "4-$rank*2+$rand")
                  ]
                )
              )
              Action.Wait (numExpr "30-$rank*30")
            ]
          )
        ]
      )

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_opening.xml
  let X_A_boss_opening =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "サイヴァリア、多分最終面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "600"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-45+$rand*90")), Some (Speed (None, numExpr "(0.3+$rand*0.5)*($rank+1)")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.Wait (numExpr "1")
                  ]
                )
              )
              Action.Wait (numExpr "100")
            ]
          )
        ]
      )

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_winder.xml
  let X_A_boss_winder =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "サイヴァリア、多分最終面ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "winderBullet")}, None, Some (Speed (None, numExpr "3")),
            []
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "fireWinder")}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "$1")), None,
            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "roundWinder")},
            [
              Action.FireRef ({fireRefLabel = FireLabel "fireWinder"}, ["$1"])
              Action.Repeat (Times (numExpr "11"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.FireRef ({fireRefLabel = FireLabel "fireWinder"}, ["30"])
                  ]
                )
              )
              Action.Wait (numExpr "5")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "winderSequence")},
            [
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["30"])
              )
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["$1"])
              )
              Action.Repeat (Times (numExpr "12"),
                ActionElm.ActionRef ({actionRefLabel = ActionLabel "roundWinder"}, ["30"])
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "2")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "winderSequence"}, ["30.9+0.1*$rank"])
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "-2")), None,
                BulletElm.BulletRef ({bulletRefLabel = BulletLabel "winderBullet"}, [])
              )
              Action.ActionRef ({actionRefLabel = ActionLabel "winderSequence"}, ["29.1-0.1*$rank"])
            ]
          )
        ]
      )
