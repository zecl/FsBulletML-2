namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Noiz2sa
[<RequireQualifiedAccess>]
module Noiz2sa =

  /// Noiz2saより、88way。 by 白い弾幕くん
  /// [Noiz2sa]_88way.xml
  let b88way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Noiz2saより、88way。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "0.7")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [ActionRef ({actionRefLabel = ActionLabel "main";},[])])); Wait (numExpr "200")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "main");},
           [Repeat
              (Times (numExpr "6+$rank*10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "360/(6+$rank*10)")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "16way";},[]));
                   Wait (numExpr "100/(6+$rank*10)")])); Vanish]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "16way");},None,Some (Speed (None,numExpr "$rand+1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20+$rand*40");
                Repeat
                  (Times (numExpr "16"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
                          None,
                          Bullet
                            ({bulletLabel = None;},None,Some (Speed (None,numExpr "1.7")),[]))]));
                Vanish])])])

  /// Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん
  /// [Noiz2sa]_bit.xml
  let bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Noiz2saより、ビットから自機狙い弾。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "4+$rank*10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},None,None,
                      BulletRef ({bulletRefLabel = BulletLabel "bit";},[]))])); Wait (numExpr "180")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bit");},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "4"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360"),
                          Term (numExpr "20")); ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "20"));
                       Wait (numExpr "20"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "20"));
                       Wait (numExpr "20");
                       Fire
                         ({fireLabel = None;},None,None,
                          BulletRef ({bulletRefLabel = BulletLabel "seed";},[])); Wait (numExpr "0")]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "$rand*10-5")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "nrm";},[]));
                Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "6");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          None,BulletRef ({bulletRefLabel = BulletLabel "nrm";},[]))])); Vanish])]);
        BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nrm");},None,Some (Speed (None,numExpr "2")),[])])

  /// Noiz2saより、回る棒。by 白い弾幕くん
  /// [Noiz2sa]_rollbar.xml
  let rollbar = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Noiz2saより、回る棒。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "$rand*50")),
               Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[Vanish])]));
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},[])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "180+$rank*50")),
               Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[Vanish])]));
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},[])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "main");},
           [Repeat
              (Times (numExpr "15+$rank*10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "firebar";},["90"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "160")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "firebar";},["-90"]));
                   Wait (numExpr "200/(15+$rank*10)")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "firebar");},None,Some (Speed (None,numExpr "10")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "1");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
                          None,
                          Bullet
                            ({bulletLabel = None;},None,Some (Speed (None,numExpr "1.5")),[]))]));
                Vanish])])])
