namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Strikers1999
[<RequireQualifiedAccess>]
module Strikers1999 =

  /// ストライカーズ1999の花火かも。by 白い弾幕くん
  /// [Strikers1999]_hanabi.xml
  let hanabi =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ストライカーズ1999の花火かも。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                      Some (Speed (None,numExpr "3")),
                      BulletRef ({bulletRefLabel = BulletLabel "fastHanabi";},[]));
                   Wait (numExpr "110-$rank*60")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fastFour");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "5")),
               Some (Speed (None,numExpr "2+$rank")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-10")),
               Some (Speed (None,numExpr "2+$rank")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
               Some (Speed (None,numExpr "1.5+$rank")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-20")),
               Some (Speed (None,numExpr "1.5+$rank")),
               Bullet ({bulletLabel = None;},None,None,[]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "slowFour");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "5")),
               Some (Speed (None,numExpr "1+$rank*0.8")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-10")),
               Some (Speed (None,numExpr "1+$rank*0.8")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
               Some (Speed (None,numExpr "0.7+$rank*0.8")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-20")),
               Some (Speed (None,numExpr "0.7+$rank*0.8")),
               Bullet ({bulletLabel = None;},None,None,[]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "fastHanabi");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "15");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "0")),
                   Some (Speed (None,numExpr "2.5+$rank")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Action.ActionRef ({actionRefLabel = ActionLabel "fastFour";},[]);
                Repeat
                  (Times (numExpr "16"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "32.5")),
                          Some (Speed (None,numExpr "2.5+$rank")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef ({actionRefLabel = ActionLabel "fastFour";},[])]));
                FireRef ({fireRefLabel = FireLabel "slowHanabi";},[]); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "slowHanabi");},None,None,
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                      Some (Speed (None,numExpr "1.3+$rank*0.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "slowFour";},[]);
                   Repeat
                     (Times (numExpr "16"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "32.5")),
                             Some (Speed (None,numExpr "1.3+$rank*0.8")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Action.ActionRef ({actionRefLabel = ActionLabel "slowFour";},[])])); Vanish])]))])
