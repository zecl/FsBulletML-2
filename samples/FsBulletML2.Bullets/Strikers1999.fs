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
/// Strikers1999
[<RequireQualifiedAccess>]
module Strikers1999 =

  /// ストライカーズ1999の花火かも。by 白い弾幕くん
  /// [Strikers1999]_hanabi.xml
  let hanabi =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ストライカーズ1999の花火かも。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "3"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "fastHanabi"}, [])
                    )
                    Action.Wait (numExpr "110-$rank*60")
                  ]
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "fastFour")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), Some (Speed (None, numExpr "2+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-10")), Some (Speed (None, numExpr "2+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "1.5+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-20")), Some (Speed (None, numExpr "1.5+$rank")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "slowFour")},
            [
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "5")), Some (Speed (None, numExpr "1+$rank*0.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-10")), Some (Speed (None, numExpr "1+$rank*0.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "15")), Some (Speed (None, numExpr "0.7+$rank*0.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
              Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-20")), Some (Speed (None, numExpr "0.7+$rank*0.8")),
                BulletElm.Bullet ({bulletLabel = None}, None, None,
                  []
                )
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "fastHanabi")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "15")
                  Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "0")), Some (Speed (None, numExpr "2.5+$rank")),
                    BulletElm.Bullet ({bulletLabel = None}, None, None,
                      []
                    )
                  )
                  Action.ActionRef ({actionRefLabel = ActionLabel "fastFour"}, [])
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "32.5")), Some (Speed (None, numExpr "2.5+$rank")),
                          BulletElm.Bullet ({bulletLabel = None}, None, None,
                            []
                          )
                        )
                        Action.ActionRef ({actionRefLabel = ActionLabel "fastFour"}, [])
                      ]
                    )
                  )
                  Action.FireRef ({fireRefLabel = FireLabel "slowHanabi"}, [])
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Fire ({fireLabel = Some (FireLabel "slowHanabi")}, None, None,
            BulletElm.Bullet ({bulletLabel = None}, None, None,
              [
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "0")), Some (Speed (None, numExpr "1.3+$rank*0.8")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "slowFour"}, [])
                    Action.Repeat (Times (numExpr "16"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "32.5")), Some (Speed (None, numExpr "1.3+$rank*0.8")),
                            BulletElm.Bullet ({bulletLabel = None}, None, None,
                              []
                            )
                          )
                          Action.ActionRef ({actionRefLabel = ActionLabel "slowFour"}, [])
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
