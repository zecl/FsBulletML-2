namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Bulletsmorph
[<RequireQualifiedAccess>]
module Bulletsmorph =

  /// Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん
  /// [Bulletsmorph]_aba_2.xml
  let aba_2 =  
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その二。by 白い弾幕くん";
        bulletmlDescription = None},
        [BulletmlElm.Action
          ({actionLabel = Some "top";},
            [Repeat
              (Times (numExpr "8"),
                Action
                  ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "center";},["90 * $rand"; "1"]);
                    Wait (numExpr "12");
                    Action.ActionRef ({actionRefLabel = "center";},["90 * $rand"; "-1"]);
                    Wait (numExpr "12");
                    Action.ActionRef ({actionRefLabel = "center";},["30 * $rand"; "1"]);
                    Wait (numExpr "12");
                    Action.ActionRef ({actionRefLabel = "center";},["30 * $rand"; "-1"]);
                    Wait (numExpr "12")])); Wait (numExpr "150")]);
        BulletmlElm.Action
          ({actionLabel = Some "center";},
            [Fire
              ({fireLabel = None;},
                Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "360 * $rand")),
                None,BulletRef ({bulletRefLabel = "circle";},["$1"; "$2"]));
            Repeat
              (Times (numExpr "(4 + 8 * $rank) - 1"),
                Action
                  ({actionLabel = None;},
                  [Fire
                      ({fireLabel = None;},
                      Some
                        (Direction
                            (Some {directionType = DirectionType.Sequence;},numExpr "360 / (4 + 8 * $rank)")),
                      None,BulletRef ({bulletRefLabel = "circle";},["$1"; "$2"]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "circle";},None,Some (Speed (None,numExpr "1.3")),
            [Action
              ({actionLabel = None;},
                [Wait (numExpr "20");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180 + $1 * $2"),
                    Term (numExpr "1")); Wait (numExpr "125 - $1");
                Fire
                  ({fireLabel = None;},
                    Some (Direction (Some {directionType = Aim;},numExpr "0")),None,
                    BulletRef ({bulletRefLabel = "red";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "red";},None,Some (Speed (None,numExpr "0.1")),
            [Action
              ({actionLabel = None;},[ChangeSpeed (Speed (None,numExpr "4.0"),Term (numExpr "300"))])])])

  /// Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん
  /// [Bulletsmorph]_aba_3.xml
  let aba_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その三。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "4 + 16 * $rank"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "120 + 120 * $rand")),
                      None,BulletRef ({bulletRefLabel = "bomb";},[]));
                   Wait (numExpr "60 - 30 * $rank")])); Wait (numExpr "180")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bomb";},None,Some (Speed (None,numExpr "0.5 + 1.9 * $rand")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "50");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "360 * $rand")),
                   None,BulletRef ({bulletRefLabel = "bombbit";},[]));
                Repeat
                  (Times (numExpr "(4 + 8 * $rank) - 1"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},
                                numExpr "360 / (4 + 8 * $rank)")),None,
                          BulletRef ({bulletRefLabel = "bombbit";},[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bombbit";},None,Some (Speed (None,numExpr "0.8")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "120");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "120")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "240")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "0")),
                   Some (Speed (None,numExpr "1.3")),
                   BulletRef ({bulletRefLabel = "changecolor";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "changecolor";},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (Some {speedType = SpeedType.Relative;},numExpr "0")),
                   Bullet ({bulletLabel = None;},None,None,[])); Vanish])])])

  /// Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん
  /// [Bulletsmorph]_aba_4.xml
  let aba_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その四。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},None,Some (Speed (None,numExpr "0.1")),
               BulletRef ({bulletRefLabel = "cross";},[])); Wait (numExpr "5");
            Repeat
              (Times (numExpr "40 + 60 * $rank"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},None,
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.04")),
                      BulletRef ({bulletRefLabel = "cross";},[]));
                   Wait (numExpr "20 - 10 * $rank")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "cross";},
           Some (Direction (Some {directionType = Aim;},numExpr "0")),None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (Some {speedType = SpeedType.Relative;},numExpr "4.0"),Term (numExpr "300"));

                Wait (numExpr "45");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "0")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]))])])])

  /// Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん
  /// [Bulletsmorph]_aba_5.xml
  let aba_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その五。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "bit";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "bit";},["-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
               BulletRef ({bulletRefLabel = "bit";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
               BulletRef ({bulletRefLabel = "bit";},["-1"]));
            Repeat
              (Times (numExpr "300"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "-(120 + 45 * $rank) + (240 + 90 * $rank) * $rand")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "5"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                             Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "2")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bit";},None,Some (Speed (None,numExpr "0.2")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "60"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "5");
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction
                        (Some {directionType = Aim;},numExpr "(45 - 25 * $rank) * $1")),None,
                   BulletRef ({bulletRefLabel = "backstab";},[])); Wait (numExpr "3");
                Repeat
                  (Times (numExpr "29"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "-0.5 * $1")),None,
                          BulletRef ({bulletRefLabel = "backstab";},[])); Wait (numExpr "3")]));
                Repeat
                  (Times (numExpr "30"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0.5 * $1")),
                          None,BulletRef ({bulletRefLabel = "backstab";},[]));
                       Wait (numExpr "3")]));
                Repeat
                  (Times (numExpr "30"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "-0.5 * $1")),None,
                          BulletRef ({bulletRefLabel = "backstab";},[])); Wait (numExpr "3")]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "backstab";},None,Some (Speed (None,numExpr "1.6")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "70 + 20 * $rand");
                ChangeDirection
                  (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "1"))])])])

  /// Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん
  /// [Bulletsmorph]_aba_6.xml
  let aba_6 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その六。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Action.ActionRef ({actionRefLabel = "allway";},[]);
            Action.ActionRef ({actionRefLabel = "bar";},[]); Wait (numExpr "200")]);
        BulletmlElm.Action
          ({actionLabel = Some "allway";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "15")),None,
               BulletRef ({bulletRefLabel = "allwaybit";},[]));
            Repeat
              (Times (numExpr "11"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),None,
                      BulletRef ({bulletRefLabel = "allwaybit";},[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "allwaybit";},None,Some (Speed (None,numExpr "6.0")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "999"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90")),
                          None,BulletRef ({bulletRefLabel = "stopandgo";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90")),
                          None,BulletRef ({bulletRefLabel = "stopandgo";},[]));
                       Wait (numExpr "6 - 4 * $rank")]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "stopandgo";},None,Some (Speed (None,numExpr "1.0")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); ChangeSpeed (Speed (None,numExpr "0.0001"),Term (numExpr "1")); Wait (numExpr "40");
                ChangeSpeed (Speed (None,numExpr "4.0"),Term (numExpr "300"))])]);
        BulletmlElm.Action
          ({actionLabel = Some "bar";},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = "barhand";},["1"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = "barhand";},["-1"]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "barhand";},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
           Some (Speed (None,numExpr "0.0001")),
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                   Some (Speed (None,numExpr "4.0 - 2.0 * $rank")),
                   BulletRef ({bulletRefLabel = "barbit";},["1"]));
                Repeat
                  (Times (numExpr "2 + 3 * $rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some
                            (Speed
                               (Some {speedType = SpeedType.Sequence;},numExpr "4.0 - 2.0 * $rank")),
                          BulletRef ({bulletRefLabel = "barbit";},["1"]))]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),
                   Some (Speed (None,numExpr "4.0 - 2.0 * $rank")),
                   BulletRef ({bulletRefLabel = "barbit";},["-1"]));
                Repeat
                  (Times (numExpr "2 + 3 * $rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some
                            (Speed
                               (Some {speedType = SpeedType.Sequence;},numExpr "4.0 - 2.0 * $rank")),
                          BulletRef ({bulletRefLabel = "barbit";},["-1"]))]));
                Wait (numExpr "5");
                Repeat
                  (Times (numExpr "20"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "180 + 10 * $1")),
                          Some (Speed (None,numExpr "4.0 - 2.0 * $rank")),
                          BulletRef ({bulletRefLabel = "barbit";},["1"]));
                       Repeat
                         (Times (numExpr "2 + 3 * $rank"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                                 Some
                                   (Speed
                                      (Some {speedType = SpeedType.Sequence;},
                                       numExpr "4.0 - 2.0 * $rank")),
                                 BulletRef ({bulletRefLabel = "barbit";},["1"]))]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),
                          Some (Speed (None,numExpr "4.0 - 2.0 * $rank")),
                          BulletRef ({bulletRefLabel = "barbit";},["-1"]));
                       Repeat
                         (Times (numExpr "2 + 3 * $rank"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                                 Some
                                   (Speed
                                      (Some {speedType = SpeedType.Sequence;},
                                       numExpr "4.0 - 2.0 * $rank")),
                                 BulletRef ({bulletRefLabel = "barbit";},["-1"]))]));
                       Wait (numExpr "5")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "barbit";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "5"); ChangeSpeed (Speed (None,numExpr "0.0001"),Term (numExpr "1")); Wait (numExpr "5");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90 * $1")),
                   Some (Speed (None,numExpr "1.3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん
  /// [Bulletsmorph]_aba_7.xml
  let aba_7 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。紋章遺伝学その七。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                      Some (Speed (None,numExpr "1.1")),
                      BulletRef ({bulletRefLabel = "dummy";},[])); Wait (numExpr "60");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
                      Some (Speed (None,numExpr "1.1")),
                      BulletRef ({bulletRefLabel = "dummy";},[])); Wait (numExpr "60")]));
            Wait (numExpr "250 - 50 * $rank")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "dummy";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "60");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "-32")),
                   Some (Speed (None,numExpr "1.1")),
                   BulletRef ({bulletRefLabel = "bit";},[]));
                Repeat
                  (Times (numExpr "8"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "8")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                          BulletRef ({bulletRefLabel = "bit";},[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bit";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (Some {speedType = SpeedType.Relative;},numExpr "0.3")),
                   BulletRef ({bulletRefLabel = "slowdown";},[]));
                Repeat
                  (Times (numExpr "2 + 4 * $rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.3")),
                          BulletRef ({bulletRefLabel = "slowdown";},[]))]));
                Wait (numExpr "20");
                ChangeDirection
                  (Direction
                     (Some {directionType = Aim;},
                      numExpr "(30 - 20 * $rank) * (-1.0 + 2.0 * $rand)"),Term (numExpr "1"));
                ChangeSpeed
                  (Speed (Some {speedType = SpeedType.Relative;},numExpr "2.0 + 2.0 * $rank"),
                   Term (numExpr "300"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "slowdown";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); ChangeSpeed (Speed (None,numExpr "0.3"),Term (numExpr "60"))])])])

  /// Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん
  /// [Bulletsmorph]_convergent.xml
  let convergent = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。収束全方位弾。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "360 * $rand")),
               Some (Speed (None,numExpr "1.0")),
               BulletRef
                 ({bulletRefLabel = "nwaybit";},
                  ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"]));
            Repeat
              (Times (numExpr "35"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "10")),
                      Some (Speed (None,numExpr "1.0")),
                      BulletRef
                        ({bulletRefLabel = "nwaybit";},
                         ["90"; "1.5 * (0.5 + 0.5 * $rank)"; "3"]))]));
            Repeat
              (Times (numExpr "36"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "10")),
                      Some (Speed (None,numExpr "1.0")),
                      BulletRef
                        ({bulletRefLabel = "nwaybit";},
                         ["-90"; "1.5 * (0.5 + 0.5 * $rank)"; "-3"]))])); Wait (numExpr "150")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "nwaybit";},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "4");
                Repeat
                  (Times (numExpr "2 + 4 * $rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$3")),
                          Some (Speed (None,numExpr "$2")),
                          Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "4")]));
                Vanish])])])
  
  /// Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん
  /// [Bulletsmorph]_double_seduction.xml
  let double_seduction =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "Bulletsmorphで生成。ダブルいろじかけ。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "30")),None,
               BulletRef ({bulletRefLabel = "parentbit";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-30")),None,
               BulletRef ({bulletRefLabel = "parentbit";},["-1"])); Wait (numExpr "300")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "parentbit";},None,Some (Speed (None,numExpr "2.0")),
           [Action
              ({actionLabel = None;},
               [Action.ActionRef ({actionRefLabel = "cross";},["75"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["70"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["65"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["60"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["55"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["50"; "0"]);
                Action.ActionRef ({actionRefLabel = "cross";},["80"; "15 * $1"]);
                Action.ActionRef ({actionRefLabel = "cross";},["75"; "10 * $1"]);
                Action.ActionRef ({actionRefLabel = "cross";},["70"; "6 * $1"]);
                Action.ActionRef ({actionRefLabel = "cross";},["65"; "3 * $1"]);
                Action.ActionRef ({actionRefLabel = "cross";},["60"; "1 * $1"]);
                Action.ActionRef ({actionRefLabel = "cross";},["55"; "0"]); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "cross";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),None,
               BulletRef ({bulletRefLabel = "aimbit";},["$1"; "$2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "aimbit";},["$1"; "$2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),None,
               BulletRef ({bulletRefLabel = "aimbit";},["$1"; "$2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
               BulletRef ({bulletRefLabel = "aimbit";},["$1"; "$2"])); Wait (numExpr "5")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aimbit";},None,Some (Speed (None,numExpr "0.6")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "$1");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "$2")),
                   Some (Speed (None,numExpr "1.6 * (0.5 + 0.5 * $rank)")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "2 + 5 * $rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// Bulletsmorphで生成。落下するひも。by 白い弾幕くん
  /// [Bulletsmorph]_fallen_string.xml
  let fallen_string = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "Bulletsmorphで生成。落下するひも。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "5"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "impl:48";},[]); Wait (numExpr "50")]));
            Wait (numExpr "50")]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:48";},
           [Wait (numExpr "20");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.6")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Action.ActionRef ({actionRefLabel = "impl:60";},[]);
                       Action.ActionRef ({actionRefLabel = "impl:38";},[])])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:60";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "0")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.8")),
               Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "3")]);
        BulletmlElm.Bullet
          ({bulletLabel =
             Some "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel";},
           None,None,
           [Action
              ({actionLabel = None;},
               [Accel
                  (None,Some (Vertical (Some {verticalType = VerticalType.Absolute;},numExpr "2.7")),
                   Term (numExpr "120"))])]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:38";},
           [FireRef
              ({fireRefLabel =
                 "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt";},
               ["90"]); Wait (numExpr "24-$rank*8");
            FireRef
              ({fireRefLabel =
                 "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt";},
               ["-90"]); Wait (numExpr "24-$rank*8")]);
        BulletmlElm.Fire
          ({fireLabel =
             Some "bulletmls/[Progear]_round_5_middle_boss_rockets.xml:_:udBlt";},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1-25+$rand*50")),None,
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "impl:59";},[])])]));
        BulletmlElm.Action
          ({actionLabel = Some "impl:59";},
           [Repeat
              (Times (numExpr "9999"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1")),
                      BulletRef
                        ({bulletRefLabel =
                           "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel";},
                         []));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "60")),
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.8")),
                      BulletRef
                        ({bulletRefLabel =
                           "bulletmls/[Progear]_round_4_boss_fast_rocket.xml:_:downAccel";},
                         [])); Wait (numExpr "3")]))])])

  /// Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん                         
  /// [Bulletsmorph]_kunekune_plus_homing.xml
  let kunekune_plus_homing =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "Bulletsmorphで生成。くねくねと誘導弾。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "impl:259";},[]); Wait (numExpr "50")]));
            Wait (numExpr "60")]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:259";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "15+30*$rand")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.8-$rank+$rand")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Action.ActionRef ({actionRefLabel = "impl:30";},[])])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:30";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$1")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Action.ActionRef ({actionRefLabel = "impl:156";},[]); Vanish])]));
            Repeat
              (Times (numExpr "10+$rank*10"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "impl:12";},[])])); Vanish]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:156";},
           [Wait (numExpr "1");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),None,
               BulletRef
                 ({bulletRefLabel =
                    "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr";},[]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "bulletmls/[G_DARIUS]_homing_laser.xml:_:hmgLsr";},
           None,Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.3"),Term (numExpr "30"));
                Wait (numExpr "100");
                ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "5"),Term (numExpr "100"))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "12"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = Aim;},numExpr "0"),
                          Term (numExpr "45-$rank*30")); Wait (numExpr "5")]))])]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:12";},
           [Repeat
              (Times (numExpr "9999"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "2");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),None,
                      Bullet ({bulletLabel = None;},None,None,[]))]))])])

  /// Bulletsmorphで生成。悟君が4人。by 白い弾幕くん
  /// [Bulletsmorph]_satoru4.xml
  let satoru4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "Bulletsmorphで生成。悟君が4人。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Action.ActionRef ({actionRefLabel = "impl:100";},[]); Wait (numExpr "80")]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:100";},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "$rand*16-8")),
                      Some
                        (Speed
                           (Some {speedType = SpeedType.Absolute;},
                            numExpr "($1+$rand*$1)*($rank/2+0.65)")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action
                            ({actionLabel = None;},
                             [Action.ActionRef ({actionRefLabel = "impl:205";},[])])]));
                   Wait (numExpr "1")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "impl:205";},
           [Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way";},
               ["$rank*3+$rand"])]);
        BulletmlElm.Action
          ({actionLabel =
             Some
               "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:idousite5way";},
           [ChangeDirection
              (Direction (Some {directionType = Aim;},numExpr "$rand*360"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2"),Term (numExpr "1"));
            Wait (numExpr "30");
            Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way";},
               ["$1"]);
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1")); Vanish]);
        BulletmlElm.Action
          ({actionLabel =
             Some "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:5way";},
           [Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
               ["$1"; "-30"]);
            Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
               ["$1"; "-15"]);
            Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
               ["$1"; "0"]);
            Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
               ["$1"; "15"]);
            Action.ActionRef
              ({actionRefLabel =
                 "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
               ["$1"; "30"])]);
        BulletmlElm.Action
          ({actionLabel =
             Some "bulletmls/[ESP_RADE]_round_123_boss_satoru_5way.xml:_:1way";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "$2+$1*$rand*2-$1")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "$2+$1*$rand*2-$1")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))])])
