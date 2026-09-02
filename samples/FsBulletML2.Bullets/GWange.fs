namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// GWange
[<RequireQualifiedAccess>]
module GWange =

  /// G-わんげスレの957氏、回転ガラ by 白い弾幕くん
  /// [G-Wange]_roll_gara.xml
  let _roll_gara =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "G-わんげスレの957氏、回転ガラ by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Repeat
              (Times (numExpr "600/(3-$rank*2)"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "line";},[]);
                   Wait (numExpr "3-$rank*2+$rand")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "line");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-7")),
               Some (Speed (None,numExpr "0.6")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "5+$rank*5"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.3")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [ChangeDirection
                     (Direction
                        (Some {directionType = DirectionType.Sequence;},numExpr "-1+$rand*2"),
                      Term (numExpr "30"));
                   ChangeSpeed
                     (Speed
                        (Some {speedType = SpeedType.Absolute;},numExpr "(-1+$rand*2)*($rank*2+1)"),
                      Term (numExpr "30")); Wait (numExpr "30")]))])])

  /// G-わんげスレの966氏考案、往復ビット by 白い弾幕くん
  /// [G-Wange]_round_trip_bit.xml
  let round_trip_bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "G-わんげスレの966氏考案、往復ビット by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "src";},["5"; "91"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "src";},["4"; "-91"]));
            Wait (numExpr "600")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Xway");},
           [Fire
              ({fireLabel = None;},
               Some
                 (Direction
                    (Some {directionType = Aim;},numExpr "-(5+$rank*5)*($1-1)-4+$rand*8")),
               Some (Speed (None,numExpr "1.6")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "5+$rank*5")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fire");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "Xway";},["3"]); Wait (numExpr "15");
            Action.ActionRef ({actionRefLabel = ActionLabel "Xway";},["5"]); Wait (numExpr "15")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "src");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
           Some (Speed (None,numExpr "$1")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [ChangeSpeed
                         (Speed (None,numExpr "0.01"),
                          Term (numExpr "30"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "fire";},[]);
                       ChangeDirection
                         (Direction
                            (Some {directionType = DirectionType.Absolute;},numExpr "-$2"),
                          Term (numExpr "1"));
                       ChangeSpeed
                         (Speed (None,numExpr "$1"),Term (numExpr "30"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "fire";},[]);
                       ChangeSpeed
                         (Speed (None,numExpr "0.01"),
                          Term (numExpr "30"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "fire";},[]);
                       ChangeDirection
                         (Direction
                            (Some {directionType = DirectionType.Absolute;},numExpr "$2"),
                          Term (numExpr "1"));
                       ChangeSpeed
                         (Speed (None,numExpr "$1"),Term (numExpr "30"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "fire";},[])]))])])])
