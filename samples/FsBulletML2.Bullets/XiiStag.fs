namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Xevious
[<RequireQualifiedAccess>]
module XiiStag =
  
  /// トゥエルブスタッグ３ボス by 白い弾幕くん
  /// [XII_STAG]_3b.xml
  let b3b = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "トゥエルブスタッグ３ボス by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "bara";},["1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "bara";},["-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "3way";},["180-55"; "-5"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "3way";},["180"; "0"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "3way";},["180+55"; "5"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "roll";},["180+45"; "1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "roll";},["180-45"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "straight";},["1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "straight";},["-1"]); Wait (numExpr "50");
            Repeat
              (Times (numExpr "3*$rank"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},None,
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [ActionRef ({actionRefLabel = ActionLabel "fin1";},["1"])]));
                   Fire
                     ({fireLabel = None;},None,
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [ActionRef ({actionRefLabel = ActionLabel "fin1";},["-1"])]));
                   Fire
                     ({fireLabel = None;},None,None,
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [ActionRef ({actionRefLabel = ActionLabel "white1";},[])]));
                   Wait (numExpr "5*(6+(12*$rank))")])); Wait (numExpr "110")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fin1");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fin2";},["$1"]); Vanish]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fin2");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "$1*90")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.7+(0.8*$rank)")),
               Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "2/($rank+0.2)");
            Repeat
              (Times (numExpr "1+$rank*32"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},
                            numExpr "-1*$1*((2.3/($rank+0.01))+0.35)")),
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.7+(0.8*$rank)")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "2/($rank+0.2)")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "white1");},
           [FireRef ({fireRefLabel = FireLabel "white2";},["0"; "0.00001"; "0"]);
            FireRef ({fireRefLabel = FireLabel "white2";},["90"; "1"; "1.5"]);
            FireRef ({fireRefLabel = FireLabel "white2";},["-90"; "1"; "-1.5"]); Vanish]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "white2");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$2")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "4");
                   ChangeSpeed
                     (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.00001"),Term (numExpr "1"));
                   Wait (numExpr "83-(70*$rank)");
                   Repeat
                     (Times (numExpr "6+(12*$rank)"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Relative;},numExpr "-1*$1+$3")),
                             Some (Speed (None,numExpr "2.9")),
                             Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "5")]));
                   Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "bara");},
           [FireRef ({fireRefLabel = FireLabel "5c";},["44*$1"; "4.1"; "4"]);
            FireRef ({fireRefLabel = FireLabel "5c";},["55.5*$1"; "3.45"; "3"]);
            FireRef ({fireRefLabel = FireLabel "5c";},["55*$1"; "4.2"; "2"]);
            FireRef ({fireRefLabel = FireLabel "5c";},["70*$1"; "3"; "0"]);
            FireRef ({fireRefLabel = FireLabel "5c";},["68*$1"; "3.74"; "1"])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "5c");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+$1")),
           Some (Speed (None,numExpr "$2/1.1")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10");
                   ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
                   Wait (numExpr "5+($3*5)");
                   Repeat
                     (Times (numExpr "10-(5/($rank+0.001))"),
                      Action
                        ({actionLabel = None;},
                         [Repeat
                            (Times (numExpr "3"),ActionRef ({actionRefLabel = ActionLabel "almond1";},[]));
                          Wait (numExpr "85-(40*$rank)")])); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "almond1");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "3.5-(7*$rand)")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0+(0.3*$rand)")),
               BulletRef ({bulletRefLabel = BulletLabel "almond2";},[])); Wait (numExpr "3")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "almond2");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed
                  (Speed (Some {speedType = SpeedType.Relative;},numExpr "1.8+(0.8*$rank)"),Term (numExpr "10"))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "3way");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+$2")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "3.5")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "10");
                       ChangeSpeed
                         (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
                       Wait (numExpr "1");
                       Repeat
                         (Times (numExpr "7+(10*$rank)"),
                          Action
                            ({actionLabel = None;},
                             [FireRef ({fireRefLabel = FireLabel "9way";},["$1+16"]);
                              FireRef ({fireRefLabel = FireLabel "9way";},["$1"]);
                              FireRef ({fireRefLabel = FireLabel "9way";},["$1-16"]);
                              Wait (numExpr "25")])); Vanish])]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "9way");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5")),
           Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "roll");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+(11*$2)")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "10")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "5");
                       ChangeSpeed
                         (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
                       Wait (numExpr "1");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                          Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "$1+(30*$2)")),
                          Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "15");
                       Repeat
                         (Times (numExpr "11+(17*$rank)"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "-35*$2")),
                                 Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "30*$2")),
                                 Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Wait (numExpr "15")])); Vanish])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "straight");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+(82*$1)")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.7")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "13");
                       ChangeSpeed
                         (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
                       Wait (numExpr "1");
                       Repeat
                         (Times (numExpr "3+(5*$rank)"),
                          ActionRef ({actionRefLabel = ActionLabel "fall";},[])); Vanish])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fall");},
           [Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.9")),
                      Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "5")]));
            Wait (numExpr "15")])])
