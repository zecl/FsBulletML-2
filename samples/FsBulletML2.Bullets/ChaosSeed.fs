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
/// ChaosSeed
[<RequireQualifiedAccess>]
module ChaosSeed =

  /// カオスシード、大猿ボス。by 白い弾幕くん
  /// [ChaosSeed]_big_monkey_boss.xml
  let big_monkey_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = None; bulletmlName = Some "カオスシード、大猿ボス。by 白い弾幕くん"; bulletmlDescription = None},
        [
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "roll")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "3"), Term (numExpr "10000"))
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "60"))
                  Action.Wait (numExpr "60")
                  Action.ChangeSpeed (Speed (None, numExpr "1.8"), Term (numExpr "40"))
                  Action.Wait (numExpr "40")
                  Action.ChangeSpeed (Speed (None, numExpr "2"), Term (numExpr "30"))
                  Action.Wait (numExpr "30")
                  Action.ChangeDirection (Direction (Some {directionType = DirectionType.Sequence}, numExpr "2"), Term (numExpr "10000"))
                  Action.ChangeSpeed (Speed (Some {speedType = SpeedType.Sequence}, numExpr "0.01"), Term (numExpr "100000"))
                ]
              )
            ]
          )
          BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "explosionBullet")}, None, None,
            [
              ActionElm.Action ({actionLabel = None},
                [
                  Action.Wait (numExpr "30")
                  Action.Repeat (Times (numExpr "12"),
                    ActionElm.Action ({actionLabel = None},
                      [
                        Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Sequence}, numExpr "30")), Some (Speed (None, numExpr "1.2")),
                          BulletElm.BulletRef ({bulletRefLabel = BulletLabel "roll"}, [])
                        )
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
              Action.Repeat (Times (numExpr "3+$rank*6"),
                ActionElm.Action ({actionLabel = None},
                  [
                    Action.Fire ({fireLabel = None}, Some (Direction (Some {directionType = DirectionType.Aim}, numExpr "-90+180*$rand")), Some (Speed (None, numExpr "$rand*3+1")),
                      BulletElm.BulletRef ({bulletRefLabel = BulletLabel "explosionBullet"}, [])
                    )
                    Action.Wait (numExpr "90-$rank*60")
                  ]
                )
              )
            ]
          )
        ]
      )
