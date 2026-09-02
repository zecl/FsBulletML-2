namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Garegga
[<RequireQualifiedAccess>]
module Garegga =

  /// バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん
  /// [Garegga]_black_heart_mk2_winder.xml
  let black_heart_mk2_winder =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "バトルガレッガのBlackHeartMk2のワインダー。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "135")),None,
               BulletRef ({bulletRefLabel = "winder";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "225")),None,
               BulletRef ({bulletRefLabel = "winder";},[])); Wait (numExpr "220")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "winder";},None,Some (Speed (None,numExpr "2.3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "230")),None,
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                Action.ActionRef ({actionRefLabel = "move";},["0"; "40"]);
                Action.ActionRef ({actionRefLabel = "move";},["0.7+$rank"; "20"]);
                Action.ActionRef ({actionRefLabel = "move";},["-0.7-$rank"; "40"]);
                Action.ActionRef ({actionRefLabel = "move";},["0.7+$rank"; "20"]); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "move";},
           [Repeat
              (Times (numExpr "$2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1-100")),
                      Some (Speed (None,numExpr "5")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "25")),
                             Some (Speed (None,numExpr "5")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "2")]))])])

