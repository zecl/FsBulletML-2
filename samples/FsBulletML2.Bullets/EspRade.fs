namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// EspRade
[<RequireQualifiedAccess>]
module EspRade =

  /// エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん
  /// [ESP_RADE]_round_5_alice_clone.xml
  let round_5_alice_clone =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、最終面後半「アリスクローン」by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Fire
          ({fireLabel = Some "alice";},Some (Direction (None,numExpr "$rand*360")),
           Some (Speed (None,numExpr "8")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10*$rand");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "$rand*30-15")),
                      None,Bullet ({bulletLabel = None;},None,None,[])); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "600"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = "alice";},[]); Wait (numExpr "$rank+1+$rand")]));
            Wait (numExpr "100")])])

  /// エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_ares_2.xml
  let round_5_boss_ares_2 = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "エスプレイド、無敵の軍神アレス第二形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "Stop";},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some "XWay";},
           [Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aim3";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = "aim3Impl";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aim3Impl";},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "7"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-33+$rand*6")),
                          Some (Speed (None,numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = "XWay";},
                          ["3"; "30"]);
                       Repeat
                         (Times (numExpr "2+$rank*3"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "3");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "-60")),
                                 Some (Speed (None,numExpr "1.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Action.ActionRef
                                ({actionRefLabel = "XWay";},
                                 ["3"; "30"])]));
                       Wait (numExpr "54-$rank*9")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aim";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = "aimImpl";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aimImpl";},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "7"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-3+$rand*6")),
                          Some (Speed (None,numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Repeat
                         (Times (numExpr "2+$rank*3"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "3");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                                 Some (Speed (None,numExpr "1.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "54-$rank*9")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "fan";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = "Stop";},[]);
                Repeat
                  (Times (numExpr "3+$rank*4"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "$1-$2*3")),
                          Some (Speed (None,numExpr "$3")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = "XWay";},
                          ["7"; "10"]);
                       Wait (numExpr "420/(3+$rank*4)")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = "aim3";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = "aim3";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "125")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-125")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "150")),
               Some (Speed (None,numExpr "7")),
               BulletRef ({bulletRefLabel = "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-150")),
               Some (Speed (None,numExpr "7")),
               BulletRef ({bulletRefLabel = "aim";},[])); Wait (numExpr "10");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "6")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["-135"; "10"; "1.3"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               Some (Speed (None,numExpr "6")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["135"; "10"; "1.3"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               Some (Speed (None,numExpr "4")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["-164"; "8"; "1.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               Some (Speed (None,numExpr "4")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["156"; "8"; "1.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "130")),
               Some (Speed (None,numExpr "2")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["180"; "8"; "1.1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-130")),
               Some (Speed (None,numExpr "2")),
               BulletRef
                 ({bulletRefLabel = "fan";},
                  ["180"; "5"; "1.1"]));
            Wait (numExpr "430")])])

  /// エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_1_a.xml
  let round_5_boss_gara_1_a =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ婦人第一形態の片方 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "sequenceThree";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "12")),
               Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Action.ActionRef ({actionRefLabel = "sequenceTwo";},[])]);
        BulletmlElm.Action
          ({actionLabel = Some "sequenceTwo";},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "oogi";},
           [Fire
              ({fireLabel = None;},Some (Direction (None,numExpr "-90")),
               Some (Speed (None,numExpr "1.5")),Bullet ({bulletLabel = None;},None,None,[]));
            Action.ActionRef ({actionRefLabel = "sequenceTwo";},[]);
            Repeat (Times (numExpr "11"),ActionRef ({actionRefLabel = "sequenceThree";},[]));
            Wait (numExpr "10")]);
        BulletmlElm.Action
          ({actionLabel = Some "oogiOuHuku";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-213")),
               Some (Speed (None,numExpr "1.5")),Bullet ({bulletLabel = None;},None,None,[]));
            Action.ActionRef ({actionRefLabel = "sequenceTwo";},[]);
            Repeat (Times (numExpr "11"),ActionRef ({actionRefLabel = "sequenceThree";},[]));
            Wait (numExpr "10")]);
        BulletmlElm.Action
          ({actionLabel = Some "gara1a";},
           [Repeat
              (Times (numExpr "5"),
               Action
                 ({actionLabel = None;},
                  [ChangeDirection (Direction (None,numExpr "360*$rand"),Term (numExpr "1"));
                   ChangeSpeed (Speed (None,numExpr "0.5*$rand+0.5"),Term (numExpr "1"));
                   Action.ActionRef ({actionRefLabel = "oogi";},[]);
                   Repeat
                     (Times (numExpr "$rand*(3+$rank*2)+1+$rank*2"),
                      Action
                        ({actionLabel = None;},
                         [Action.ActionRef ({actionRefLabel = "oogiOuHuku";},[])]));
                   ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "50")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},[Action.ActionRef ({actionRefLabel = "gara1a";},[])])])

  /// エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_1_b.xml
  let round_5_boss_gara_1_b =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ第一形態のもう一方 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "8way2";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180-75")),
               Some (Speed (None,numExpr "4")),Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "9")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.25")),
                      Bullet ({bulletLabel = None;},None,None,[]))]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+75")),
               Some (Speed (None,numExpr "4")),Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-9")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.25")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "downShot";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-60+$rand*120")),
               Some (Speed (None,numExpr "4*$rand")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "20");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),

                          Some (Speed (None,numExpr "1.2")),
                          Bullet ({bulletLabel = None;},None,None,[])); Vanish])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "gara";},
           [ChangeDirection
              (Direction (Some {directionType = Aim;},numExpr "10+$rand*340"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.3"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "3+$rank*4"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "8"),
                      Action
                        ({actionLabel = None;},
                         [Action.ActionRef ({actionRefLabel = "downShot";},[]);
                          Wait (numExpr "3*(3-$rank*2)*$rand")]));
                   Action.ActionRef ({actionRefLabel = "8way2";},[])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat (Times (numExpr "5"),ActionRef ({actionRefLabel = "gara";},[]));
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "30")])])

  /// エスプレイド、ガラ婦人第二形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_2.xml
  let round_5_boss_gara_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ婦人第二形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "featherShot";},None,Some (Speed (None,numExpr "6")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "20")); Wait (numExpr "20");
                Fire
                  ({fireLabel = None;},None,None,
                   BulletRef ({bulletRefLabel = "featherAim";},[]));
                Repeat
                  (Times (numExpr "150+$rank*100"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},Some (Direction (None,numExpr "90*$rand-45")),

                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "3-$rank*2")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "featherAim";},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "7"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},None,Some (Speed (None,numExpr "3")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Repeat
                         (Times (numExpr "20"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "2");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                                 Some (Speed (None,numExpr "3")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "30")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "featherShot";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
               BulletRef ({bulletRefLabel = "featherShot";},[])); Wait (numExpr "550")])])

  /// エスプレイド、ガラ第三形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_3.xml
  let round_5_boss_gara_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ第三形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "stop";},
           [Wait (numExpr "15"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "featherAllWay";},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some
                     (Direction
                        (Some {directionType = DirectionType.Relative;},numExpr "$2*(180-(10-$1)*60)")),
                   Some (Speed (None,numExpr "0")),
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                Action.ActionRef ({actionRefLabel = "stop";},[]);
                Repeat
                  (Times (numExpr "40"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "-2*(7-$1)*$2")),
                          Some (Speed (None,numExpr "0.9+0.2*(6-$1)")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Repeat
                         (Times (numExpr "$1-1"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "-2*$2")),
                                 Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "15")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "featherAim";},None,None,
           [Action
              ({actionLabel = None;},
               [Action.ActionRef ({actionRefLabel = "stop";},[]);
                Repeat
                  (Times (numExpr "10+$rank*20"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = Aim;},numExpr "-3")),
                          Some (Speed (None,numExpr "1.2")),
                          Bullet
                            ({bulletLabel = None;},None,None,
                             [Action ({actionLabel = None;},[])]));
                       Repeat
                         (Times (numExpr "2"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                                 Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                 Bullet
                                   ({bulletLabel = None;},None,None,
                                    [Action ({actionLabel = None;},[])]))]));
                       Wait (numExpr "40-$rank*20")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = "featherAim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = "featherAim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "70")),
               Some (Speed (None,numExpr "2")),
               BulletRef ({bulletRefLabel = "featherAim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-70")),
               Some (Speed (None,numExpr "2")),
               BulletRef ({bulletRefLabel = "featherAim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "100")),
               Some (Speed (None,numExpr "1.8")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["3"; "1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-100")),
               Some (Speed (None,numExpr "1.8")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["3"; "-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["4"; "1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["4"; "-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "85")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["5"; "1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-85")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["5"; "-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "72")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["6"; "1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-72")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = "featherAllWay";},["6"; "-1"]));
            Wait (numExpr "700")])])

  /// エスプレイド、ガラ婦人第四形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_4.xml
  let round_5_boss_gara_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ婦人第四形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "featherShot";},None,Some (Speed (None,numExpr "7")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "20")); Wait (numExpr "20");
                Repeat
                  (Times (numExpr "50"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},Some (Direction (None,numExpr "20*$rand-10")),

                          Some (Speed (None,numExpr "2*$rand+0.7")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,

                      BulletRef ({bulletRefLabel = "featherShot";},[]));
                   Wait (numExpr "30+$rank*30");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
                      BulletRef ({bulletRefLabel = "featherShot";},[]));
                   Wait (numExpr "30+$rank*30")])); Wait (numExpr "120");
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,

                      BulletRef ({bulletRefLabel = "featherShot";},[]));
                   Wait (numExpr "40-$rank*20");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
                      BulletRef ({bulletRefLabel = "featherShot";},[]));
                   Wait (numExpr "40-$rank*20")])); Wait (numExpr "60")])])

  /// エスプレイド、ガラ婦人最終形態 by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_gara_5.xml
  let round_5_boss_gara_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、ガラ婦人最終形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "accel";},
           [ChangeDirection (Direction (None,numExpr "360*$rand"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "0.5+$rand*0.5"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some "stop";},[ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some "stopAndWait";},
           [Action.ActionRef ({actionRefLabel = "stop";},[]); Wait (numExpr "70-$rank*50")]);
        BulletmlElm.Action
          ({actionLabel = Some "ippon";},
           [Repeat
              (Times (numExpr "26"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.12")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "murasaki";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
               Some (Speed (None,numExpr "0.8")),Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "3+$rand*17"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "ippon";},[]); Wait (numExpr "6");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "0.8")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "ao";},
           [Repeat
              (Times (numExpr "3+$rand*17"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},None,Some (Speed (None,numExpr "0.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = "ippon";},[]); Wait (numExpr "6")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "gara5";},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "accel";},[]);
                   Action.ActionRef ({actionRefLabel = "murasaki";},["5"; "180-$rand*90"]);

                   Action.ActionRef ({actionRefLabel = "stopAndWait";},[]);
                   Action.ActionRef ({actionRefLabel = "accel";},[]);
                   Action.ActionRef ({actionRefLabel = "ao";},[]);
                   Action.ActionRef ({actionRefLabel = "stopAndWait";},[]);
                   Action.ActionRef ({actionRefLabel = "accel";},[]);
                   Action.ActionRef ({actionRefLabel = "murasaki";},["-5"; "180+$rand*90"]);
                   Action.ActionRef ({actionRefLabel = "stopAndWait";},[]);
                   Action.ActionRef ({actionRefLabel = "accel";},[]);
                   Action.ActionRef ({actionRefLabel = "ao";},[]);
                   Action.ActionRef ({actionRefLabel = "stopAndWait";},[])]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},[Action.ActionRef ({actionRefLabel = "gara5";},[])])
    ])

  /// エスプレイド、五行覚師、発狂。by 白い弾幕くん
  /// [ESP_RADE]_round_5_boss_kakusi_hakkyou.xml
  let round_5_boss_kakusi_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、五行覚師、発狂。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "6shots";},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "3");
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = Aim;},numExpr "-15+30*$rand")),
                          Some (Speed (None,numExpr "0.8+$rank+$rand")),
                          Bullet ({bulletLabel = None;},None,None,[]))]));
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = Aim;},numExpr "-45+30*$rand")),
                          Some (Speed (None,numExpr "0.8+$rank+$rand")),
                          Bullet ({bulletLabel = None;},None,None,[]))]));
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = Aim;},numExpr "15+30*$rand")),

                          Some (Speed (None,numExpr "0.8+$rank+$rand")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "kakusi";},None,Some (Speed (None,numExpr "6")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "10")); Wait (numExpr "10");
                Repeat
                  (Times (numExpr "4+$rank*6"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = Aim;},numExpr "90")),None,
                          BulletRef ({bulletRefLabel = "6shots";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = Aim;},numExpr "-90")),None,

                          BulletRef ({bulletRefLabel = "6shots";},[]));
                       Wait (numExpr "200/(4+$rank*6)")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "kakusi";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
               BulletRef ({bulletRefLabel = "kakusi";},[])); Wait (numExpr "200")])])

  /// エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_izuna_hakkyou.xml
  let round_123_boss_izuna_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "エスプレイド、1-3面のボスとなる、IZUNA発狂 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "Red";},None,None,[Action ({actionLabel = None;},[])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "Dummy";},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "Stop";},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some "XWay";},
           [Action.ActionRef
              ({actionRefLabel = "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some "XWayFan";},
           [Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "$3")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "roll";},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90*$1")),
           Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = "Stop";},[]);
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "1.5+$rank")),
                   BulletRef ({bulletRefLabel = "Dummy";},[]));
                Repeat
                  (Times (numExpr "10"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "8");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "5.3*$1")),
                          Some
                            (Speed
                               (Some {speedType = SpeedType.Sequence;},numExpr "-0.3-$rank*0.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = "XWay";},
                          ["8"; "45"]);
                       Repeat
                         (Times (numExpr "5"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "8");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "3*$1")),
                                 Some
                                   (Speed
                                      (Some {speedType = SpeedType.Sequence;},numExpr "0.06+$rank*0.1")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Action.ActionRef
                                ({actionRefLabel = "XWay";},
                                 ["8"; "45"])]))]));
                Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = "roll";},["1"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = "roll";},["-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "10");
                       Action.ActionRef ({actionRefLabel = "Stop";},[]);
                       Action.ActionRef ({actionRefLabel = "aim";},[]); Vanish])]));
            Wait (numExpr "500")]);
        BulletmlElm.Action
          ({actionLabel = Some "aim";},
           [Repeat
              (Times (numExpr "10"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "50");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "-1")),
                      Some (Speed (None,numExpr "1.7")),
                      BulletRef ({bulletRefLabel = "Red";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "2")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      BulletRef ({bulletRefLabel = "Red";},[]));
                   Repeat
                     (Times (numExpr "2+$rank*6"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "-2")),
                             Some
                               (Speed
                                  (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                             BulletRef ({bulletRefLabel = "Red";},[]));
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "2")),
                             Some
                               (Speed
                                  (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                             BulletRef ({bulletRefLabel = "Red";},[]))]))]))])])

  /// エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_pelaboy_hakkyou.xml
  let round_123_boss_pelaboy_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "エスプレイド、1-3面のボスとなる、ペラボーイ発狂 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "Red";},None,None,[Action ({actionLabel = None;},[])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "Dummy";},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "Stop";},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some "XWay";},
           [Action.ActionRef
              ({actionRefLabel = "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some "XWayFan";},
           [Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "$3")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "subBatteryFan";},None,
           Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = "Stop";},[]);
                Wait (numExpr "250");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = Aim;},numExpr "-45")),
                   Some (Speed (None,numExpr "1.6")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Action.ActionRef
                  ({actionRefLabel = "XWay";},
                   ["10+$rank*10"; "90/(10+$rank*10)"]);
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "5");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},
                                numExpr "-90")),
                          Some (Speed (None,numExpr "1.6")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = "XWay";},
                          ["11+$rank*10";"90/(10+$rank*10)"])])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "aimFan";},None,Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "5");
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction
                        (Some {directionType = Aim;},numExpr "-4-$rank*8")),
                   Some (Speed (None,numExpr "1.6")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Action.ActionRef
                  ({actionRefLabel = "XWay";},
                   ["5+$rank*8"; "2"]); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "subBatteryAim";},None,
           Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = "Stop";},[]);
                Repeat
                  (Times (numExpr "4"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "100+$rand*50");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                          None,BulletRef ({bulletRefLabel = "aimFan";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
                          BulletRef ({bulletRefLabel = "aimFan";},[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "soldier";},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90*$1")),
           Some (Speed (None,numExpr "2")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = "Stop";},[]);
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                   Some (Speed (None,numExpr "1.3")),
                   BulletRef ({bulletRefLabel = "Dummy";},[]));
                Repeat
                  (Times (numExpr "120+$rank*200"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "440/(120+$rank*200)+$rand");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "17*$1")),
                          Some
                            (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                          Bullet ({bulletLabel = None;},None,None,[]))]))])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef
                 ({bulletRefLabel = "soldier";},["1"; "90"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef
                 ({bulletRefLabel = "soldier";},
                  ["-1"; "-80"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               None,BulletRef ({bulletRefLabel = "subBatteryAim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               None,BulletRef ({bulletRefLabel = "subBatteryFan";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               None,BulletRef ({bulletRefLabel = "subBatteryFan";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Wait (numExpr "10");
                       Fire
                         ({fireLabel = None;},None,
                          Some (Speed (None,numExpr "0")),
                          Bullet
                            ({bulletLabel = None;},None,None,
                             [Action
                                ({actionLabel = None;},
                                 [Action.ActionRef ({actionRefLabel = "mainBattery";},[]);
                                  Vanish])])); Vanish])])); Wait (numExpr "500")]);
        BulletmlElm.Action
          ({actionLabel = Some "mainBattery";},
           [Repeat
              (Times (numExpr "15"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "8");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]))]));
            Wait (numExpr "195");
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "20");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "88+$rand*4")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = "XWay";},
                      ["12+$rank*16";"180/(12+$rank*16)"]); Wait (numExpr "20");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "93+$rand*4")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = "XWay";},
                      ["11+$rank*16";"170/(11+$rank*16)"])])); Wait (numExpr "40")])])

  /// エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん
  /// [ESP_RADE]_round_123_boss_satoru_5way.xml
  let round_123_boss_satoru_5way =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "エスプレイド、1-3面のボスとなる、近江悟君 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "1way";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "$2+$1*$rand*2-$1")),
               Some (Speed (None,numExpr "1")),Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "$2+$1*$rand*2-$1")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "5way";},
           [Action.ActionRef ({actionRefLabel = "1way";},["$1"; "-30"]);
            Action.ActionRef ({actionRefLabel = "1way";},["$1"; "-15"]);
            Action.ActionRef ({actionRefLabel = "1way";},["$1"; "0"]);
            Action.ActionRef ({actionRefLabel = "1way";},["$1"; "15"]);
            Action.ActionRef ({actionRefLabel = "1way";},["$1"; "30"])]);
        BulletmlElm.Action
          ({actionLabel = Some "idousite5way";},
           [ChangeDirection (Direction (None,numExpr "$rand*360"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "1")); Wait (numExpr "30");
            Action.ActionRef ({actionRefLabel = "5way";},["$1"]);
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "90-$rank*60")]);
        BulletmlElm.Action
          ({actionLabel = Some "satoru";},
           [Action.ActionRef ({actionRefLabel = "idousite5way";},["1"]);
            Action.ActionRef ({actionRefLabel = "idousite5way";},["2"]);
            Action.ActionRef ({actionRefLabel = "idousite5way";},["3"]);
            Action.ActionRef ({actionRefLabel = "idousite5way";},["4"]);
            Action.ActionRef ({actionRefLabel = "idousite5way";},["5"])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Action.ActionRef ({actionRefLabel = "satoru";},[]); Wait (numExpr "30")])])
