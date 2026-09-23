// このファイルは生成物。手で直すと次の焼き直しで消える。
// 人が書くのは src/FsBulletML2.Bullets.Dsl（CE）。ここは DU へ写した突き合わせ門の相手。

namespace FsBulletML2.Bullets.EnemyBullet
open FsBulletML2

/// その他
[<RequireQualifiedAccess>]
module Others =

  /// 全方位弾
  let AllWay =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "全方位弾"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "circle")},
            [
              Action.Repeat (Times (numExpr "$1"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/$1")), None,
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
                    Action.ActionRef ({actionRefLabel = ActionLabel "circle"}, ["20"])
                    Action.Wait (numExpr "20")
                  ]
                )
              )
            ]
          )
        ]
      )

  /// 前方5way弾
  let b5way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "前方5way弾"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Relative}, numExpr "-20+180")), None,
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Repeat (Times (numExpr "4"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "10")), None,
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

  /// 初期方向Aim弾１発
  let homingOne =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None; bulletmlType = None; bulletmlName = Some "初期方向Aim弾１発"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Fire ({fireLabel = None}, None, None,
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
        ]
      )
