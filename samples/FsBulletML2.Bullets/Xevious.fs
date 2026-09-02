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
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ゼビウス、らしい。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                      Some (Speed (None,numExpr "3")),
                      BulletRef ({bulletRefLabel = "gzc";},[]));
                   Wait (numExpr "20-$rank*10+$rand*10")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "gzc";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10+$rand*10");
                Repeat
                  (Times (numExpr "16"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "360/16")),
                          None,BulletRef ({bulletRefLabel = "spr";},[]))]));
                Repeat
                  (Times (numExpr "4"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
                          None,BulletRef ({bulletRefLabel = "hrmSpr";},[]))]));
                Vanish])]);
        BulletmlElm.Bullet ({bulletLabel = Some "spr";},None,Some (Speed (None,numExpr "2")),[]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "hrmSpr";},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},[ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "60"))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "40"));
                       Wait (numExpr "1")]))])])])
