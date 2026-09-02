namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Xsoldier
[<RequireQualifiedAccess>]
module Xsoldier =

  /// XSoldierの8面ボスの主砲 by 白い弾幕くん
  /// [xsoldier]_8_boss_main.xml
  let b8_boss_main =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "XSoldierの8面ボスの主砲 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),
               BulletRef ({bulletRefLabel = "dummy";},["90"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),
               BulletRef ({bulletRefLabel = "dummy";},["270"]));
            Wait (numExpr "100 - $rank * 90");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),
               BulletRef ({bulletRefLabel = "allway";},["0"; "1.5"])); Wait (numExpr "5");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),
               BulletRef ({bulletRefLabel = "allway";},["2.5"; "1.8"])); Wait (numExpr "20")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "dummy";},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "0")),BulletRef ({bulletRefLabel = "bit";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "0.5")),
                   BulletRef ({bulletRefLabel = "bit";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "1.0")),
                   BulletRef ({bulletRefLabel = "bit";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "1.5")),
                   BulletRef ({bulletRefLabel = "bit";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "allway";},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "71"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "5")),
                          Some (Speed (None,numExpr "$2")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bit";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Wait (numExpr "105 - $rank * 90");
                Repeat
                  (Times (numExpr "20"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                          Some (Speed (None,numExpr "3")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                          Some (Speed (None,numExpr "3.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                          Some (Speed (None,numExpr "4")),
                          Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "1")]));
                Vanish])])])



