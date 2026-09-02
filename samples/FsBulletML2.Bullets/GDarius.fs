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
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "Gダライアス中のホーミングレーザー by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},Some (Direction (None,numExpr "-60+$rand*120")),
                      None,BulletRef ({bulletRefLabel = "hmgLsr";},[]));
                   Repeat
                     (Times (numExpr "8"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "1");
                          Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                             None,BulletRef ({bulletRefLabel = "hmgLsr";},[]))]));
                   Wait (numExpr "10")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "hmgLsr";},None,Some (Speed (None,numExpr "2")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0.3"),Term (numExpr "30")); Wait (numExpr "100");
                ChangeSpeed (Speed (None,numExpr "5"),Term (numExpr "100"))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "12"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = Aim;},numExpr "0"),
                          Term (numExpr "45-$rank*30")); Wait (numExpr "5")]))])])])
