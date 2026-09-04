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
/// DragonBlaze
[<RequireQualifiedAccess>]
module DragonBlaze =

  /// ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん
  /// [DragonBlaze]_nebyurosu_2.xml
  let nebyurosu_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "add3")},
            [
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
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top1")},
            [
              Action.Repeat (Times (numExpr "150"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "4")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                    Action.Wait (numExpr "2")
                  ]
                )
              )
              Action.Wait (numExpr "60-$rank*30")
            ]
          )
          BulletmlElm.Action ({actionLabel = Some (ActionLabel "top2")},
            [
              Action.Repeat (Times (numExpr "150"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "-5")), Some (Speed (None, numExpr "1+$rank")),
                      BulletElm.Bullet ({bulletLabel = None}, None, None,
                        []
                      )
                    )
                    Action.ActionRef ({actionRefLabel = ActionLabel "add3"}, [])
                    Action.Wait (numExpr "2")
                  ]
                )
              )
              Action.Wait (numExpr "60-$rank*30")
            ]
          )
        ]
      )
