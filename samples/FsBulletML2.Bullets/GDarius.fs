// **このファイルは生成物。手で直すと次の焼き直しで消える。**
//
// 人が書くのは samples/FsBulletML2.Bullets.Dsl（CE）のほう。ここは
// その値を DU で直に組んだ形へ写したもので、突き合わせ門の相手として置いてある。
//
// 焼き直し:
//     dotnet build samples/FsBulletML2.Bullets.Dsl -c Release
//     dotnet fsi samples/FsBulletML2.Bullets.Dsl/gen.fsx

namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// GDarius
[<RequireQualifiedAccess>]
module GDarius =

  /// Gダライアス中のホーミングレーザー by 白い弾幕くん
  /// [G_DARIUS]_homing_laser.xml
  let homing_laser =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "Gダライアス中のホーミングレーザー by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top")},
            [
              Action.Repeat (Times (numExpr "20"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (None, numExpr "-60+$rand*120")), None,
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hmgLsr"}, [])
                    )
                    Action.Repeat (Times (numExpr "8"),
                      ActionElm.Action ({actionLabel = None},
                        [
                          Action.Wait (numExpr "1")
                          Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "0")), None,
                            BulletElm.BulletRef ({bulletRefLabel = BulletLabel "hmgLsr"}, [])
                          )
                        ]
                      )
                    )
                    Action.Wait (numExpr "10")
                  ]
                )
              )
              Action.Wait (numExpr "60")
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "hmgLsr")}, None, Some (Speed (None, numExpr "2")),
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeSpeed (Speed (None, numExpr "0.3"), Term (numExpr "30"))
                  Action.Wait (numExpr "100")
                  Action.ChangeSpeed (Speed (None, numExpr "5"), Term (numExpr "100"))
                ]
              )
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Repeat (Times (numExpr "12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim}, numExpr "0"), Term (numExpr "45-$rank*30"))
                        Action.Wait (numExpr "5")
                      ]
                    )
                  )
                ]
              )
            ]
          )
        ]
      )
