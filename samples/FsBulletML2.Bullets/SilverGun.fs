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
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "レイディアントシルバーガン4Dボス、PENTA。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "100")),
               Some (Speed (None,numExpr "4")),BulletRef ({bulletRefLabel = BulletLabel "arm";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-100")),
               Some (Speed (None,numExpr "4")),BulletRef ({bulletRefLabel = BulletLabel "arm";},[]));
            Repeat
              (Times (numExpr "400"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "7")),
                      Some (Speed (None,numExpr "1.5")),
                      Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "1")]));
            Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "arm");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "12"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Repeat
                  (Times (numExpr "7"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "60");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = Aim;},numExpr "-15")),
                          Some (Speed (None,numExpr "1.8")),
                          BulletRef ({bulletRefLabel = BulletLabel "homing";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                          Some (Speed (None,numExpr "1.8")),
                          BulletRef ({bulletRefLabel = BulletLabel "homing";},[])); Wait (numExpr "2")]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "homing");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "60");
                ChangeDirection
                  (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "15-$rank*10"));
                Wait (numExpr "15-$rank*10");
                ChangeDirection
                  (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "15-$rank*10"))])])])