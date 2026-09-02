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
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "カオスシード、大猿ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "roll");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "3"),Term (numExpr "10000"));
                ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "60")); Wait (numExpr "60");
                ChangeSpeed (Speed (None,numExpr "1.8"),Term (numExpr "40")); Wait (numExpr "40");
                ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "30")); Wait (numExpr "30");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "2"),Term (numExpr "10000"));
                ChangeSpeed
                  (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.01"),Term (numExpr "100000"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "explosionBullet");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "30");
                Repeat
                  (Times (numExpr "12"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                          Some (Speed (None,numExpr "1.2")),
                          BulletRef ({bulletRefLabel = BulletLabel "roll";},[]))])); Vanish])]);

        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "3+$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-90+180*$rand")),
                      Some (Speed (None,numExpr "$rand*3+1")),
                      BulletRef ({bulletRefLabel = BulletLabel "explosionBullet";},[]));
                   Wait (numExpr "90-$rank*60")]))])])
