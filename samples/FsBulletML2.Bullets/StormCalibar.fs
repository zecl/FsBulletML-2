namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// StormCalibar
[<RequireQualifiedAccess>]
module StormCalibar =

  /// ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん
  /// [STORM_CALIBAR]_last_boss_double_roll_bullets.xml
  let last_boss_double_roll_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "ストームキャリバーのラスボス、回転二つ。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "rollShots");},
           [Repeat
              (Times (numExpr "200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "11*$1")),
                      Some (Speed (None,numExpr "1")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "3+$rank*4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                             Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.3")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "2")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "right");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5"),Term (numExpr "1"));
            Wait (numExpr "50")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "left");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5"),Term (numExpr "1"));
            Wait (numExpr "50")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "right";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "left";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "left";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "right";},[])]));
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "rollShots";},["-1"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "rollShots";},["1"])])])
