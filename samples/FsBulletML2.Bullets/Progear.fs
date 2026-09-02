namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Progear
[<RequireQualifiedAccess>]
module Progear =

  /// CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん
  /// [Progear]_round_1_boss_grow_bullets.xml
  let round_1_boss_grow_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、一面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "oogi");},
           [Fire
              ({fireLabel = None;},
               Some
                 (Direction
                    (Some {directionType = DirectionType.Absolute;},numExpr "270-(4+$rank*6)*15/2")),None,
               BulletRef ({bulletRefLabel = BulletLabel "seed";},[]));
            Repeat
              (Times (numExpr "4+$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "seed";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "oogi";},[]); Wait (numExpr "40")]));
            Wait (numExpr "40");
            Repeat
              (Times (numExpr "8"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "oogi";},[]); Wait (numExpr "20")]));
            Wait (numExpr "30")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,Some (Speed (None,numExpr "1.5")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "60")); Wait (numExpr "60");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0.75")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "4+$rank*4"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},None,
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.3")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん
  /// [Progear]_round_2_boss_struggling.xml
  let round_2_boss_struggling =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、二面ボス、発狂モード。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "1000"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "changeStraight";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "159")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "changeStraight";},[]));
                   Wait (numExpr "1+(1-$rank)*3*$rand")])); Wait (numExpr "180")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "changeStraight");},None,Some (Speed (None,numExpr "0.8")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20+$rand*100");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270"),Term (numExpr "60"));
                ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "40")); Wait (numExpr "40");
                ChangeSpeed (Speed (None,numExpr "0.5+$rand*0.7"),Term (numExpr "20"))])])])
    
  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_back_burst.xml
  let round_3_boss_back_burst =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "220+$rand*100")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "backBurst";},[]));
                   Wait (numExpr "4-$rank*2")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "backBurst");},None,Some (Speed (None,numExpr "1.2")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "80")); Wait (numExpr "60+$rand*20");
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "60+$rand*60")),
                          None,BulletRef ({bulletRefLabel = BulletLabel "downAccel";},[]))]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "downAccel");},None,Some (Speed (None,numExpr "1.8")),
           [Action
              ({actionLabel = None;},
               [Accel
                  (Some (Horizontal (Some {horizontalType = Relative;},numExpr "-7")),None,
                   Term (numExpr "250"))])])])

  /// CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん
  /// [Progear]_round_3_boss_wave_bullets.xml
  let round_3_boss_wave_bullets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、三面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "10"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "310")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "wave";},["-3"])); Wait (numExpr "30");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "230")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "wave";},["3"])); Wait (numExpr "30")]));
            Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "wave");},None,Some (Speed (None,numExpr "1.5")),
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},Some (Direction (None,numExpr "0")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "nrm";},[]));
                Repeat
                  (Times (numExpr "12+$rank*12"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                          None,BulletRef ({bulletRefLabel = BulletLabel "nrm";},[])); Wait (numExpr "3")]));
                Vanish])]);
        BulletmlElm.Bullet ({bulletLabel = Some (BulletLabel "nrm");},None,Some (Speed (None,numExpr "1")),[])])

  /// CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん
  /// [Progear]_round_4_boss_fast_rocket.xml
  let round_4_boss_fast_rocket =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、四面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fireRoot");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
               Some (Speed (None,numExpr "0.2")),BulletRef ({bulletRefLabel = BulletLabel "rootBl";},[]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "rootBl";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fireRoot";},["$rand*16"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "fireRoot";},["180+$rand*16"]); Wait (numExpr "120")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rootBl");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "40");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "274+$rand*4")),
                   None,BulletRef ({bulletRefLabel = BulletLabel "rocket";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rocket");},None,Some (Speed (None,numExpr "5+$rand")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                          Some (Speed (None,numExpr "1")),
                          BulletRef ({bulletRefLabel = BulletLabel "downAccel";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "60")),
                          Some (Speed (None,numExpr "1.8")),
                          BulletRef ({bulletRefLabel = BulletLabel "downAccel";},[]));
                       Wait (numExpr "5-$rank*4")]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "downAccel");},None,None,
           [Action
              ({actionLabel = None;},
               [Accel (None,Some (Vertical (None,numExpr "2.7")),Term (numExpr "120"))])])])

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_boss_last_round_wave.xml
  let round_5_boss_last_round_wave =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "2+$rank*1.5"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},None,None,
                             BulletRef ({bulletRefLabel = BulletLabel "rfRkt";},[])); Wait (numExpr "45")]));
                   Wait (numExpr "100")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rfRkt");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "2");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                          None,Bullet ({bulletLabel = None;},None,None,[]))]))])])])

  /// CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん
  /// [Progear]_round_5_middle_boss_rockets.xml
  let round_5_middle_boss_rockets =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、五面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "rocket";},[])); Wait (numExpr "10")]));
            Wait (numExpr "120")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rocket");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [FireRef ({fireRefLabel = FireLabel "udBlt";},["90"]); Wait (numExpr "20-$rank*8");
                       FireRef ({fireRefLabel = FireLabel "udBlt";},["-90"]);
                       Wait (numExpr "$rand*10+15-$rank*8")]))])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "udBlt");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1-25+$rand*50")),None,
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん
  /// [Progear]_round_6_boss_parabola_shot.xml
  let round_6_boss_parabola_shot =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletHorizontal;
        bulletmlName = Some "CAVEのプロギアの嵐、二周目一面ボス(嘘) by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "25"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Absolute;},numExpr "190+$rand*30")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "seed";},["1"]));
                   Wait (numExpr "15-$rank*5");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Absolute;},numExpr "350-$rand*30")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "seed";},["-1"]));
                   Wait (numExpr "15-$rank*5")])); Wait (numExpr "60")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "60")); Wait (numExpr "60");
                Fire
                  ({fireLabel = None;},None,None,
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction
                        (Some {directionType = DirectionType.Absolute;},numExpr "270+30*$1+$rand*50*$1")),
                   None,BulletRef ({bulletRefLabel = BulletLabel "downAccel";},["$1"]));
                Repeat
                  (Times (numExpr "3"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.4")),
                          BulletRef ({bulletRefLabel = BulletLabel "downAccel";},["$1"]))]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "downAccel");},None,Some (Speed (None,numExpr "2.5")),
           [Action
              ({actionLabel = None;},
               [Accel (None,Some (Vertical (None,numExpr "4*$1")),Term (numExpr "120"))])])])

  /// CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん
  /// [Progear]_round_9_boss.xml
  let round_9_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "CAVEのプロギアの嵐、二周目四面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "accel");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed
                  (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.03"),Term (numExpr "9999"));
                Wait (numExpr "9999")])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "80")),None,
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[Wait (numExpr "20"); Vanish])]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "40")),
                      Some (Speed (None,numExpr "5")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action
                            ({actionLabel = None;},
                             [Repeat
                                (Times (numExpr "9999"),
                                 Action
                                   ({actionLabel = None;},
                                    [Fire
                                       ({fireLabel = None;},
                                        Some
                                          (Direction
                                             (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                                        Some (Speed (None,numExpr "0.5")),
                                        BulletRef ({bulletRefLabel = BulletLabel "accel";},[]));
                                     Fire
                                       ({fireLabel = None;},
                                        Some
                                          (Direction
                                             (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                                        Some (Speed (None,numExpr "0.5")),
                                        BulletRef ({bulletRefLabel = BulletLabel "accel";},[]));
                                     Wait (numExpr "4-$rank*2+$rand")]))])]))])); Wait (numExpr "120")])])

  /// CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん
  /// [Progear]_round_10_boss_before_final.xml
  let round_10_boss_before_final =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "CAVEのプロギアの嵐、ラスボスの雰囲気。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "rollOut");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90")),
           Some (Speed (None,numExpr "0.0001")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "350"); ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "100"));
                   ChangeDirection
                     (Direction (Some {directionType = DirectionType.Relative;},numExpr "50-$rank*40"),
                      Term (numExpr "100")); Wait (numExpr "1000")])]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "setter");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "999"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "5"); FireRef ({fireRefLabel = FireLabel "rollOut";},[])]))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*10")),None,
               BulletRef ({bulletRefLabel = BulletLabel "setter";},[]));
            Repeat
              (Times (numExpr "45/(2-$rank)"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Sequence;},numExpr "16-$rank*8")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "setter";},[])); Wait (numExpr "1")]));
            Wait (numExpr "40");
            Repeat
              (Times (numExpr "125+$rank*125"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "1.5-$rank/2+$rand");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "45-$rand*90")),
                      Some (Speed (None,numExpr "1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Wait (numExpr "80"); ChangeSpeed (Speed (None,numExpr "0.7"),Term (numExpr "1"));
            ChangeDirection (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "1"));
            Wait (numExpr "1");
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1.44444"),Term (numExpr "250"));
            Wait (numExpr "250"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "20");
            ChangeSpeed (Speed (None,numExpr "0.7"),Term (numExpr "1"));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30"),Term (numExpr "12"));
            Wait (numExpr "12"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "200-$rank*60")])])
