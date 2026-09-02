namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Tenmado
[<RequireQualifiedAccess>]
module Tenmado =

  /// tenmadoより、三面ボス「Disconnection」by 白い弾幕くん
  /// [tenmado]_3_boss_2.xml
  let b3_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "tenmadoより、三面ボス「Disconnection」by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "240")),
               Some (Speed (None,numExpr "0.6")),
               BulletRef ({bulletRefLabel = BulletLabel "bitlaser";},["60"; "10"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-240")),
               Some (Speed (None,numExpr "0.6")),
               BulletRef ({bulletRefLabel = BulletLabel "bitlaser";},["-60"; "-10"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "240")),
               Some (Speed (None,numExpr "0.6")),
               BulletRef ({bulletRefLabel = BulletLabel "bitaim";},["60"; "10"; "35"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-240")),
               Some (Speed (None,numExpr "0.6")),
               BulletRef ({bulletRefLabel = BulletLabel "bitaim";},["-60"; "-10"; "5"]));
            Wait (numExpr "60");
            Repeat
              (Times (numExpr "600 / (6.0 - 4.0 * $rank)"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "-30 + 60 * $rand")),
                      Some (Speed (None,numExpr "1.3+$rank*0.7")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "6.0 - 4.0 * $rank")])); Wait (numExpr "90")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bitlaser");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "120"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "30");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
                ChangeSpeed (Speed (None,numExpr "0.6"),Term (numExpr "1")); Wait (numExpr "90");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                   Some (Speed (None,numExpr "0.1")),
                   BulletRef ({bulletRefLabel = BulletLabel "laser";},["0.3"]));
                Repeat
                  (Times (numExpr "6"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "20");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                          Some (Speed (None,numExpr "0.1")),
                          BulletRef ({bulletRefLabel = BulletLabel "laser";},["0.3"]))]));
                ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "30");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1"));
                ChangeSpeed (Speed (None,numExpr "0.8"),Term (numExpr "1")); Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1 + 3.5 * $2")),
                   Some (Speed (None,numExpr "0.1")),
                   BulletRef ({bulletRefLabel = BulletLabel "laser";},["1.5"]));
                Repeat
                  (Times (numExpr "4"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "20");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-$2")),
                          Some (Speed (None,numExpr "0.1")),
                          BulletRef ({bulletRefLabel = BulletLabel "laser";},["1.5"]))]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bitaim");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "120"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "30");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
                ChangeSpeed (Speed (None,numExpr "0.6"),Term (numExpr "1")); Wait (numExpr "40 - $3");
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "70");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-10 + 20 * $rand")),
                          Some (Speed (None,numExpr "0.6")),
                          Bullet ({bulletLabel = None;},None,None,[]))]));
                Wait (numExpr "30 + $3"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "30");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1"));
                ChangeSpeed (Speed (None,numExpr "0.8"),Term (numExpr "1")); Wait (numExpr "40 - $3");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "-10 + 20 * $rand")),
                   Some (Speed (None,numExpr "0.6")),
                   Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "50 + $3");
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "laser");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1 + 0.01")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1 + 0.02")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1 + 0.03")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1 + 0.04")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "$1 + 0.05")),
                   Bullet ({bulletLabel = None;},None,None,[])); Vanish])])])

  /// tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん
  /// [tenmado]_5_boss_1.xml
  let b5_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "tenmadoより、最終ボス「L」第一形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),BulletRef ({bulletRefLabel = BulletLabel "random";},[]));
            Repeat
              (Times (numExpr "8"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                      Some (Speed (None,numExpr "0.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "surprise";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),
                      Some (Speed (None,numExpr "0.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "surprise";},[])); Wait (numExpr "100")]));
            Wait (numExpr "20")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "surprise");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "100"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Repeat
                         (Times (numExpr "30"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = Aim;},numExpr "3.5")),
                                 Some (Speed (None,numExpr "15+$rand*15")),
                                 Bullet
                                   ({bulletLabel = None;},None,None,
                                    [Action ({actionLabel = None;},[])]));
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = Aim;},numExpr "-3.5")),
                                 Some (Speed (None,numExpr "15+$rand*15")),
                                 Bullet
                                   ({bulletLabel = None;},None,None,
                                    [Action ({actionLabel = None;},[])]))]));
                       Wait (numExpr "1")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "random");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "200");
                Repeat
                  (Times (numExpr "6000/(130 - 100 * $rank) "),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-22 + 44 * $rand")),
                          Some (Speed (None,numExpr "1.6 + 1.0 * $rand")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "0.1 * (130 - 100 * $rank)")])); Vanish])])])

  /// tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん
  /// [tenmado]_5_boss_3.xml
  let b5_boss_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "tenmadoより、最終ボス「L」第三形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),BulletRef ({bulletRefLabel = BulletLabel "stardust";},[]));
            Wait (numExpr "120");
            Repeat
              (Times (numExpr "840/(120 - 100 * $rank)"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "1")),
                      BulletRef ({bulletRefLabel = BulletLabel "laser";},["2"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "1")),
                      BulletRef ({bulletRefLabel = BulletLabel "laser";},["2.05"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "1")),
                      BulletRef ({bulletRefLabel = BulletLabel "laser";},["2.1"]));
                   Wait (numExpr "0.5 * (120 - 100 * $rank)")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "stardust");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "5+$rank*10"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "135 + 90 * $rand")),
                          Some (Speed (None,numExpr "0.3 + 1.7 * $rand")),
                          BulletRef
                            ({bulletRefLabel = BulletLabel "stardust2";},["60"; "1.2"; "0.8"]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "135 + 90 * $rand")),
                          Some (Speed (None,numExpr "0.3 + 1.7 * $rand")),
                          BulletRef
                            ({bulletRefLabel = BulletLabel "stardust2";},["68"; "0.8"; "1.2"]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "135 + 90 * $rand")),
                          Some (Speed (None,numExpr "0.3 + 1.7 * $rand")),
                          BulletRef
                            ({bulletRefLabel = BulletLabel "stardust2";},["76"; "1.2"; "0.8"]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "135 + 90 * $rand")),
                          Some (Speed (None,numExpr "0.3 + 1.7 * $rand")),
                          BulletRef
                            ({bulletRefLabel = BulletLabel "stardust2";},["84"; "0.8"; "1.2"]));
                       Wait (numExpr "960/(10+$rank*20)")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "stardust2");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "$1");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "30")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "60")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "120")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "150")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "210")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "240")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "300")),
                   Some (Speed (None,numExpr "$2")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "330")),
                   Some (Speed (None,numExpr "$3")),
                   Bullet ({bulletLabel = None;},None,None,[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "laser");},None,None,
           [Action
              ({actionLabel = None;},[ChangeSpeed (Speed (None,numExpr "$1"),Term (numExpr "1"))])])])
