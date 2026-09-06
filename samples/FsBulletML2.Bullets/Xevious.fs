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
/// Xevious
[<RequireQualifiedAccess>]
module Xevious =

  /// ゼビウス、らしい。 by 白い弾幕くん
  /// [XEVIOUS]_garu_zakato.xml
  let garu_zakato =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ゼビウス、らしい。 by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "10"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Absolute}, numExpr "180")), Some (Speed (None, numExpr "3")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "gzc"}, [])
                    )
                    Action.Wait (numExpr "20-$rank*10+$rand*10")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "gzc")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "10+$rand*10")
                  Action.Repeat (Times (numExpr "16"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "360/16")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "spr"}, [])
                        )
                      ]
                    )
                  )
                  Action.Repeat (Times (numExpr "4"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "90")), None,
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hrmSpr"}, [])
                        )
                      ]
                    )
                  )
                  Action.Vanish
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "spr")}, None, Some (Speed (None, numExpr "2")),
            []
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "hrmSpr")}, None, Some (Speed (None, numExpr "0")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "60"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "9999"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "40"))
                        Action.Wait (numExpr "1")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )
