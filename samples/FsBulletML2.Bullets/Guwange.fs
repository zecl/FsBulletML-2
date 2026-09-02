namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Guwange
[<RequireQualifiedAccess>]
module Guwange =

  /// ぐわんげ、二面ボス by 白い弾幕くん
  /// [Guwange]_round_2_boss_circle_fire.xml
  let round_2_boss_circle_fire = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ぐわんげ、二面ボス by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Fire
          ({fireLabel = Some "circle";},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
           Some (Speed (None,numExpr "6")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "3");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                      Some (Speed (None,numExpr "1.5+$rank")),
                      Bullet ({bulletLabel = None;},None,None,[])); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some "fireCircle";},
           [Repeat
              (Times (numExpr "18"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = "circle";},["20"; "$1"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Action.ActionRef ({actionRefLabel = "fireCircle";},["180-45+90*$rand"]);
            Wait (numExpr "10")])])

  /// ぐわんげ、三面ボス by 白い弾幕くん
  /// [Guwange]_round_3_boss_fast_3way.xml
  let round_3_boss_fast_3way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ぐわんげ、三面ボス by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "10+$rank*50"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},Some (Direction (None,numExpr "$rand*360")),
                      Some (Speed (None,numExpr "5")),
                      BulletRef ({bulletRefLabel = "seed";},["5+$rand*10"]));
                   Wait (numExpr "20-$rank*10")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "seed";},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "$1")); Wait (numExpr "$1");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "-20")),None,
                   BulletRef ({bulletRefLabel = "3way";},[]));
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20")),
                          None,BulletRef ({bulletRefLabel = "3way";},[]))]));
                Wait (numExpr "6");
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.1")),
                          BulletRef ({bulletRefLabel = "3way";},[]));
                       Repeat
                         (Times (numExpr "2"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "-20")),
                                 Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                 BulletRef ({bulletRefLabel = "3way";},[]))]));
                       Wait (numExpr "6");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.1")),
                          BulletRef ({bulletRefLabel = "3way";},[]));
                       Repeat
                         (Times (numExpr "2"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "20")),
                                 Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                 BulletRef ({bulletRefLabel = "3way";},[]))]));
                       Wait (numExpr "6")])); Vanish])]);
        BulletmlElm.Bullet ({bulletLabel = Some "3way";},None,Some (Speed (None,numExpr "3")),[])])

  /// ぐわんげ、四面ボス by 白い弾幕くん
  /// [Guwange]_round_4_boss_eye_ball.xml
  let round_4_boss_eye_ball =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ぐわんげ、四面ボス by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "10+$rank*10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},Some (Direction (None,numExpr "$rand*360")),None,
                      BulletRef ({bulletRefLabel = "eye";},[])); Wait (numExpr "30")]));
            Wait (numExpr "120")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "eye";},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "10"),Term (numExpr "400"));
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$rand*5-2"),
                   Term (numExpr "9999"));
                Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                          None,BulletRef ({bulletRefLabel = "shadow";},[]));
                       Wait (numExpr "4")]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "shadow";},None,Some (Speed (None,numExpr "0.1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90")),
                   Some (Speed (None,numExpr "0.6")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90")),
                   Some (Speed (None,numExpr "0.6")),
                   Bullet ({bulletLabel = None;},None,None,[])); Vanish])])])
