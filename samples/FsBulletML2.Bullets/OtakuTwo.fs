namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// OtakuTwo
[<RequireQualifiedAccess>]
module OtakuTwo =

  /// おたくツーさん作、円形発射弾・花火型 by 白い弾幕くん
  /// [OtakuTwo]_circle_fireworks.xml
  let circle_fireworks =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "おたくツーさん作、円形発射弾・花火型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "20+$rank*40"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "0.4+$rank*0.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "59"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "6")),
                             Some (Speed (None,numExpr "0.4+$rank*0.6")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "30-$rank*20");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "9")),
                      Some (Speed (None,numExpr "1.2+$rank*1.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "59"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "6")),
                             Some (Speed (None,numExpr "1.2+$rank*1.8")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "30-$rank*20")]));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))])])

  /// おたくツーさん作、円形発射弾・花火型弐式 by 白い弾幕くん
  /// [OtakuTwo]_circle_fireworks2.xml
  let circle_fireworks2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "おたくツーさん作、円形発射弾・花火型弐式 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "20+$rank*40"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "0")),
                      Some (Speed (None,numExpr "1.2+$rank*1.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "59"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "6")),
                             Some (Speed (None,numExpr "1.2+$rank*1.8")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "30-$rank*20");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "9")),
                      Some (Speed (None,numExpr "0.4+$rank*0.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "59"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "6")),
                             Some (Speed (None,numExpr "0.4+$rank*0.6")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "30-$rank*20")]));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))])])

  /// おたくツーさん作、円形発射弾・速度変化型 by 白い弾幕くん
  /// [OtakuTwo]_circle_trap.xml
  let circle_trap =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "おたくツーさん作、円形発射弾・速度変化型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "10"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "$rand"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "main");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["100+$1*160"; "$2"]);
            Wait (numExpr "40-$rank*20"); 
            Action.ActionRef ({actionRefLabel = ActionLabel "round";},[]);
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["280+$1*160"; "$2"]); Wait (numExpr "25")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "$2*1.5+$rank*1.5"),Term (numExpr "1"));
            Wait (numExpr "40-$rank*20"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "round");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360")),
               Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[Vanish])])); Wait (numExpr "1");
            Repeat
              (Times (numExpr "6"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "30"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1")),
                             Some (Speed (None,numExpr "0.8+$rank*0.4")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Repeat
                     (Times (numExpr "30"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1")),
                             Some (Speed (None,numExpr "0.6+$rank*0.3")),
                             BulletRef ({bulletRefLabel = BulletLabel "speed";},[]))]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "speed");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "100-$rank*50");
                ChangeSpeed (Speed (None,numExpr "1.2+$rank*0.6"),Term (numExpr "1"));
                Wait (numExpr "(100-$rank*50)/2");
                ChangeSpeed (Speed (None,numExpr "0.8+$rank*0.4"),Term (numExpr "1"))])])])

  /// 最臭鬼畜兵器「非蜂」１：ニオイ波動 by 白い弾幕くん
  /// [OtakuTwo]_dis_bee_1.xml
  let dis_bee_1 = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "最臭鬼畜兵器「非蜂」１：ニオイ波動 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "position";},["$rand"; "$rand"; "10"; "6-$rank*4"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "position";},["$rand"; "$rand"; "15"; "6-$rank*4"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "position";},["$rand"; "$rand"; "20"; "6-$rank*4"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "position";},["$rand"; "$rand"; "25"; "6-$rank*4"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "position";},["$rand"; "$rand"; "30"; "6-$rank*4"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "position");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["100+$1*160"; "$2"]);
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360"),Term (numExpr "1"));
            Wait (numExpr "1"); Action.ActionRef ({actionRefLabel = ActionLabel "wave";},["$3"; "$4"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["(100+$1*160)+180"; "$2"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "$2*6+$rank*6"),Term (numExpr "1")); Wait (numExpr "10-$rank*5");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "wave");},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 0.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 3.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 5.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 6.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 6.5"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 6.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 5.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 3.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},[" 0.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-3.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-5.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-6.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-6.5"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-6.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-5.0"; "$1"]);
                   Wait (numExpr "$2");
                   Action.ActionRef ({actionRefLabel = ActionLabel "allrange";},["-3.0"; "$1"]);
                   Wait (numExpr "$2")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "allrange");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
               Some (Speed (None,numExpr "0.8+$rank*1.6")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "$2-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "360/$2")),
                      Some (Speed (None,numExpr "0.8+$rank*1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))])])

  /// 最臭鬼畜兵器「非蜂」２：壁花火 by 白い弾幕くん
  /// [OtakuTwo]_dis_bee_2.xml
  let dis_bee_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "最臭鬼畜兵器「非蜂」２：壁花火 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["180"]);
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360"),Term (numExpr "1"));
            Wait (numExpr "5");
            Repeat
              (Times (numExpr "20+$rank*20"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "wall";},["15"]);
                   Wait (numExpr "25-$rank*$rank*12");
                   Action.ActionRef ({actionRefLabel = ActionLabel "wall";},[" 0"]);
                   Wait (numExpr "25-$rank*$rank*12")]));
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "5")); Wait (numExpr "6");
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "50")); Wait (numExpr "55");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "50")); Wait (numExpr "55")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "wall");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
               Some (Speed (None,numExpr "1+$rank*1.2")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Action.ActionRef ({actionRefLabel = ActionLabel "wallbody";},[]);
            Repeat
              (Times (numExpr "11"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (None,numExpr "1+$rank*1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "wallbody";},[])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "wallbody");},
           [Repeat
              (Times (numExpr "15"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1")),
                      Some (Speed (None,numExpr "1+$rank*1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))])])

  /// 最臭鬼畜兵器「非蜂」３：ぐるぐる風車 by 白い弾幕くん
  /// [OtakuTwo]_dis_bee_3.xml
  let dis_bee_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "最臭鬼畜兵器「非蜂」３：ぐるぐる風車 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),BulletRef ({bulletRefLabel = BulletLabel "top";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),None,
               BulletRef ({bulletRefLabel = BulletLabel "byakko";},[]));
            Action.ActionRef ({actionRefLabel = ActionLabel "byakko";},[]);
            Repeat
              (Times (numExpr "120+$rank*$rank*$rank*120"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "8-$rank*$rank*$rank*4");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "128+$rand*0.5")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "byakko";},[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "byakko";},[])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "byakko");},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "120")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "byakko";},[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "byakko");},None,Some (Speed (None,numExpr "6")),
           [Action
              ({actionLabel = None;},
               [FireRef ({fireRefLabel = FireLabel "byakkoway";},[" 48.4-$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},[" 32.4-$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},[" 16.4-$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},["  0.4-$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},["-16.4+$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},["-32.4+$rand*0.8"]);
                FireRef ({fireRefLabel = FireLabel "byakkoway";},["-48.4+$rand*0.8"]); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "byakkoway");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (None,numExpr "0.8+$rank*$rank*1")),
           Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "top");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "backfire";},[]));
                Action.ActionRef ({actionRefLabel = ActionLabel "backfire";},[]);
                Repeat
                  (Times (numExpr "120+$rank*$rank*$rank*120"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "8-$rank*$rank*$rank*4");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},
                                numExpr "112+$rand*0.5-$rank*$rank*$rank*$rank*$rank*9.5")),
                          None,BulletRef ({bulletRefLabel = BulletLabel "backfire";},[]));
                       Action.ActionRef ({actionRefLabel = ActionLabel "backfire";},[])]))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "backfire");},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "120")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "backfire";},[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "backfire");},None,Some (Speed (None,numExpr "10")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "4"); FireRef ({fireRefLabel = FireLabel "backfire";},["100.5"]);
                FireRef ({fireRefLabel = FireLabel "backfire";},["110.5"]);
                FireRef ({fireRefLabel = FireLabel "backfire";},["120.5"]);
                FireRef ({fireRefLabel = FireLabel "backfire";},["130.5"]);
                FireRef ({fireRefLabel = FireLabel "backfire";},["140.5"]); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "backfire");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1-$rand")),
           Some (Speed (None,numExpr "1+$rank*$rank")),
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// おたくツーさん作、回転砲台・鶚型 by 白い弾幕くん
  /// [OtakuTwo]_roll_misago.xml
  let roll_misago = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "おたくツーさん作、回転砲台・鶚型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "10");
            Repeat
              (Times (numExpr "6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                      Some (Speed (None,numExpr "0.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "white";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                      Some (Speed (None,numExpr "0.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "black";},[]))]));
            Repeat
              (Times (numExpr "120"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                      Some (Speed (None,numExpr "0.48")),
                      BulletRef ({bulletRefLabel = BulletLabel "normal";},[]))])); Wait (numExpr "1700");
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "25");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "white");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "80");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90"),Term (numExpr "4"));
                Wait (numExpr "4");
                Repeat
                  (Times (numExpr "8"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = DirectionType.Relative;},numExpr "-135"),
                          Term (numExpr "195"));
                       Repeat
                         (Times (numExpr "30+$rank*15"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Relative;},numExpr "90")),
                                 Some (Speed (None,numExpr "0.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Wait (numExpr "6-$rank*2")]))]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "black");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90")),
                   Some (Speed (None,numExpr "0.8")),
                   BulletRef ({bulletRefLabel = BulletLabel "direction";},[])); Wait (numExpr "80");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90"),Term (numExpr "4"));
                Wait (numExpr "4");
                Repeat
                  (Times (numExpr "8"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = DirectionType.Relative;},numExpr "-135"),
                          Term (numExpr "195"));
                       Repeat
                         (Times (numExpr "30+$rank*30"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},
                                       numExpr "3.92307692307692307692307692307833*(2-$rank)")),
                                 Some (Speed (None,numExpr "0.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Wait (numExpr "6-$rank*3")]))]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "normal");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "80");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-90"),Term (numExpr "4"));
                Wait (numExpr "4");
                Repeat
                  (Times (numExpr "8"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction (Some {directionType = DirectionType.Relative;},numExpr "-135"),
                          Term (numExpr "195")); Wait (numExpr "180")]))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "direction");},None,None,
           [Action ({actionLabel = None;},[Vanish])])])

  /// 回転発射弾・四段風車形 by 白い弾幕くん
  /// [OtakuTwo]_self-0012.xml
  let self_0012 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "回転発射弾・四段風車形 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "0")),
               BulletRef ({bulletRefLabel = BulletLabel "4way";},["$rand"; "$rand"]));
            Action.ActionRef ({actionRefLabel = ActionLabel "3way";},["$rand"; "$rand"]); Wait (numExpr "100")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "3way");},
           [Repeat
              (Times (numExpr "200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "123.7+$1")),
                      Some (Speed (None,numExpr "(1+$2*0.5)*(1+$rank*$rank*$rank*$rank)")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "2"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "120")),
                             Some
                               (Speed
                                  (None,numExpr "(1+$2*0.5)*(1+$rank*$rank*$rank*$rank)")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "8-$rank*4")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "4way");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "200"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "92.1+$1")),
                          Some
                            (Speed (None,numExpr "(0.8+$2*0.8)*(1+$rank*$rank*$rank*$rank)")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Repeat
                         (Times (numExpr "3"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "90")),
                                 Some
                                   (Speed
                                      (None,
                                       numExpr "(0.8+$2*0.8)*(1+$rank*$rank*$rank*$rank)")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "8-$rank*4")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top5");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "5way";},["$rand"; "$rand"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "5way");},
           [Repeat
              (Times (numExpr "200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "78.7+$1")),
                      Some (Speed (None,numExpr "(0.6+$2*1.6)*(1+$rank*$rank*$rank*$rank)")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action
                            ({actionLabel = None;},
                             [ChangeSpeed
                                (Speed (Some {speedType = SpeedType.Relative;},numExpr "0"),
                                 Term (numExpr "9999"))])]));
                   Repeat
                     (Times (numExpr "4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "72")),
                             Some
                               (Speed
                                  (None,numExpr "(0.6+$2*1.6)*(1+$rank*$rank*$rank*$rank)")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action
                                   ({actionLabel = None;},
                                    [ChangeSpeed
                                       (Speed (Some {speedType = SpeedType.Relative;},numExpr "0"),
                                        Term (numExpr "9999"))])]))])); Wait (numExpr "8-$rank*4")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top6");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "6way";},["$rand"; "$rand"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "6way");},
           [Repeat
              (Times (numExpr "200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "63.3+$1")),
                      Some (Speed (None,numExpr "(0.6+$2*0.9)*(1+$rank*$rank*$rank*$rank)")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action
                            ({actionLabel = None;},
                             [Wait (numExpr "9999");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                                 Some (Speed (Some {speedType = SpeedType.Relative;},numExpr "0")),
                                 Bullet
                                   ({bulletLabel = None;},None,None,
                                    [Action ({actionLabel = None;},[Vanish])]))])]));
                   Repeat
                     (Times (numExpr "5"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "60")),
                             Some
                               (Speed
                                  (None,numExpr "(0.6+$2*0.9)*(1+$rank*$rank*$rank*$rank)")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action
                                   ({actionLabel = None;},
                                    [Wait (numExpr "9999");
                                     Fire
                                       ({fireLabel = None;},
                                        Some
                                          (Direction
                                             (Some {directionType = DirectionType.Relative;},numExpr "0")),
                                        Some
                                          (Speed (Some {speedType = SpeedType.Relative;},numExpr "0")),
                                        Bullet
                                          ({bulletLabel = None;},None,None,
                                           [Action ({actionLabel = None;},[Vanish])]))])]))]));
                   Wait (numExpr "8-$rank*4")]))])])

  /// 自機拘束弾・不規則回転型 by 白い弾幕くん
  /// [OtakuTwo]_self-0062.xml
  let self_0062 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "自機拘束弾・不規則回転型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["180"]);
            ChangeDirection (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "1"));
            Wait (numExpr "1");
            Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "1");
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7.1"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7.1"])]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [ChangeDirection
                     (Direction (Some {directionType = DirectionType.Relative;},numExpr "(-60+$rand*120)"),
                      Term (numExpr "100+$rank*50"));
                   Repeat
                     (Times (numExpr "100-$rand*50"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "1");
                          FireRef
                            ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7"]);
                          FireRef
                            ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7"]);
                          FireRef
                            ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7.1"]);
                          FireRef
                            ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7.1"])]))]));
            Wait (numExpr "50"); Action.ActionRef ({actionRefLabel = ActionLabel "move";},["0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "1")); Wait (numExpr "18");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "5")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "winder");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (None,numExpr "$2")),Bullet ({bulletLabel = None;},None,None,[]))])
  
  /// 自機拘束弾・不規則回転型改 by 白い弾幕くん
  /// [OtakuTwo]_self-0063.xml
  let self_0063 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "自機拘束弾・不規則回転型改 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["180"]);
            ChangeDirection (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "1"));
            Wait (numExpr "1");
            Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "1");
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7.1"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7.1"])]));
            Repeat
              (Times (numExpr "5"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "wall";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
                      Some (Speed (None,numExpr "0")),
                      BulletRef ({bulletRefLabel = BulletLabel "round";},[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "wall";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "wall";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "wall";},[])])); Wait (numExpr "50");
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "1")); Wait (numExpr "18");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "5")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "wall");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Relative;},numExpr "-60+$rand*120"),
               Term (numExpr "100+$rank*50"));
            Repeat
              (Times (numExpr "100-$rand*50"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "1");
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 22.5-$rank*12.5"; "7.1"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-22.5+$rank*12.5"; "7.1"])]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "winder");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (None,numExpr "$2")),Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "round");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "100*$rand");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360")),
                   Some (Speed (None,numExpr "1.0+$rank*1.0")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "        4")),
                   Some (Speed (None,numExpr "0.8+$rank*0.8")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "44"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                          Some (Speed (None,numExpr "1.0+$rank*1.0")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                          Some (Speed (None,numExpr "0.8+$rank*0.8")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// 回転砲台・鶚型副産物 by 白い弾幕くん
  /// [OtakuTwo]_self-0071.xml
  let self_0071 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "回転砲台・鶚型副産物 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["180"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "way";},["6"; "$rand"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "way";},["8"; "$rand"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "way";},["10"; "$rand"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "way";},["12"; "$rand"]); Wait (numExpr "50");
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "2")); Wait (numExpr "3");
            ChangeSpeed (Speed (None,numExpr "2"),Term (numExpr "25")); Wait (numExpr "27");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "25")); Wait (numExpr "27")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "way");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$rand*360")),
               Some (Speed (None,numExpr "1")),BulletRef ({bulletRefLabel = BulletLabel "turn";},["$2"]));
            Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "360/$1")),
                      Some (Speed (None,numExpr "1")),
                      BulletRef ({bulletRefLabel = BulletLabel "turn";},["$2"]))])); Wait (numExpr "500")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "turn");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "100")); Wait (numExpr "90");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "180")),
                   Some (Speed (None,numExpr "0.1")),
                   BulletRef ({bulletRefLabel = BulletLabel "bit";},["$1"])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bit");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0.5"),Term (numExpr "50"));
                ChangeDirection
                  (Direction
                     (Some {directionType = DirectionType.Relative;},numExpr "0.1+$rank*$rank*$rank*90"),
                   Term (numExpr "1000-$rank*$rank*$rank*500"));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1*360")),
                   Some (Speed (None,numExpr "0")),
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4.6")),
                          Some (Speed (None,numExpr "0.8+$rank*$rank*0.4")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "10-$rank*5")]))])])])

  /// 加速弾・巨大弾落下型 by 白い弾幕くん
  /// [OtakuTwo]_self-0081.xml
  let self_0081 = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "加速弾・巨大弾落下型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "seed";},[" 1"; "$rand"; "$rand"]);
                   Wait (numExpr "15");
                   FireRef ({fireRefLabel = FireLabel "seed";},["-1"; "$rand"; "$rand"]);
                   Wait (numExpr "15")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90*$1")),
           Some (Speed (None,numExpr "$rand*3")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "20");
                   Repeat
                     (Times (numExpr "40"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "9")),
                             Some (Speed (None,numExpr "0.5+$rank")),
                             BulletRef
                               ({bulletRefLabel = BulletLabel "roundbase";},["$2"; "$3"]))]));
                   Vanish])]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "roundbase");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                   Some (Speed (None,numExpr "$1")),
                   BulletRef ({bulletRefLabel = BulletLabel "round";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),
                   Some (Speed (None,numExpr "$2")),
                   BulletRef ({bulletRefLabel = BulletLabel "round";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "round");},None,None,
           [Action
              ({actionLabel = None;},
               [Accel (None,Some (Vertical (None,numExpr "10")),Term (numExpr "250"))])])])

  /// 「緋蜂のような物体」超速青弾part1 by 白い弾幕くん
  /// [OtakuTwo]_self-1020.xml
  let self_1020 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "「緋蜂のような物体」超速青弾part1 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30+1.1*5")),
                      Some (Speed (None,numExpr "2+$rank*$rank*2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "11"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                             Some (Speed (None,numExpr "2+$rank*$rank*2")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Repeat
                     (Times (numExpr "4+$rank*10"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "1");
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "31.1")),
                             Some (Speed (None,numExpr "2+$rank*$rank*2")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Repeat
                            (Times (numExpr "11"),
                             Action
                               ({actionLabel = None;},
                                [Fire
                                   ({fireLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                                    Some (Speed (None,numExpr "2+$rank*$rank*2")),
                                    Bullet ({bulletLabel = None;},None,None,[]))]))]));
                   Wait (numExpr "10")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Repeat
              (Times (numExpr "50"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-30-0.7*5")),
                      Some (Speed (None,numExpr "2+$rank*$rank*2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "11"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-30")),
                             Some (Speed (None,numExpr "2+$rank*$rank*2")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Repeat
                     (Times (numExpr "4+$rank*10"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "1");
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-30.7")),
                             Some (Speed (None,numExpr "2+$rank*$rank*2")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Repeat
                            (Times (numExpr "11"),
                             Action
                               ({actionLabel = None;},
                                [Fire
                                   ({fireLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "-30")),
                                    Some (Speed (None,numExpr "2+$rank*$rank*2")),
                                    Bullet ({bulletLabel = None;},None,None,[]))]))]));
                   Wait (numExpr "10")]))])])

  /// 「緋蜂のような物体」超速青弾part2 by 白い弾幕くん
  /// [OtakuTwo]_self-1021.xml
  let self_1021 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "「緋蜂のような物体」超速青弾part2 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "cyclone";},["0.1+$rand"; " 1"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "cyclone";},["0.1+$rand"; "-1"]); Wait (numExpr "10")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "cyclone");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "($rand*360)*$2")),
               None,BulletRef ({bulletRefLabel = BulletLabel "speed";},[]));
            Repeat
              (Times (numExpr "11"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "(30)*$2")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "speed";},[]))])); Wait (numExpr "1");
            Repeat
              (Times (numExpr "9"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Sequence;},numExpr "(30+$1)*$2")),
                      None,BulletRef ({bulletRefLabel = BulletLabel "speed";},[]));
                   Repeat
                     (Times (numExpr "11"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "(30)*$2")),None,
                             BulletRef ({bulletRefLabel = BulletLabel "speed";},[]))]));
                   Wait (numExpr "1")])); Wait (numExpr "10");
            Repeat
              (Times (numExpr "39"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "(30+$1*10)*$2")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "speed";},[]));
                   Repeat
                     (Times (numExpr "11"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "(30)*$2")),None,
                             BulletRef ({bulletRefLabel = BulletLabel "speed";},[]))]));
                   Wait (numExpr "1");
                   Repeat
                     (Times (numExpr "9"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "(30+$1)*$2")),
                             None,BulletRef ({bulletRefLabel = BulletLabel "speed";},[]));
                          Repeat
                            (Times (numExpr "11"),
                             Action
                               ({actionLabel = None;},
                                [Fire
                                   ({fireLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "(30)*$2")),
                                    None,BulletRef ({bulletRefLabel = BulletLabel "speed";},[]))]));
                          Wait (numExpr "1")])); Wait (numExpr "10")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "speed");},None,Some (Speed (None,numExpr "1+$rank*2")),[])])

  /// rRootageより妄想　Part01 by 白い弾幕くん
  /// [OtakuTwo]_self-2010.xml
  let self_2010 = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "rRootageより妄想　Part01 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; " 1"; " 1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; "-1"; "-1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; "-1"; " 1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; " 1"; "-1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; "-1"; "-1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; " 1"; " 1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; " 1"; "-1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; "-1"; " 1"; "-1"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "main");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "45")),None,
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$3"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$4"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$5"]));
            Wait (numExpr "200-$rank*100")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "cross");},None,Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "15"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "360*$1*$2")),
                   Some (Speed (None,numExpr "1+$rank*$rank*1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180*$2")),
                   Some (Speed (None,numExpr "1+$rank*$rank*1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "49"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "4-$rank*2");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "187.7*$2")),
                          Some (Speed (None,numExpr "1+$rank*$rank*1")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180*$2")),
                          Some (Speed (None,numExpr "1+$rank*$rank*1")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// rRootageより妄想　Part01-ANOTHER by 白い弾幕くん
  /// [OtakuTwo]_self-2011.xml
  let self_2011 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "rRootageより妄想　Part01-ANOTHER by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; " 1"; " 1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; "-1"; "-1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; "-1"; " 1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; " 1"; "-1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; "-1"; "-1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; " 1"; " 1"; "-1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; "-1"; " 1"; "-1"; " 1"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "main";},["$rand"; " 1"; "-1"; " 1"; "-1"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "main");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "45")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$2"]));
            Repeat
              (Times (numExpr "1+$rank*2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                      BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$2"]))]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$3"]));
            Repeat
              (Times (numExpr "1+$rank*2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                      BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$3"]))]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$4"]));
            Repeat
              (Times (numExpr "1+$rank*2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                      BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$4"]))]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
               Some (Speed (None,numExpr "1")),
               BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$5"]));
            Repeat
              (Times (numExpr "1+$rank*2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                      BulletRef ({bulletRefLabel = BulletLabel "cross";},["$1"; "$5"]))]));
            Wait (numExpr "200-$rank*100")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "cross");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "15"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "360*$1*$2")),
                   Some (Speed (None,numExpr "1+$rank*$rank*1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180*$2")),
                   Some (Speed (None,numExpr "1+$rank*$rank*1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "49"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "4-$rank*2");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "187.7*$2")),
                          Some (Speed (None,numExpr "1+$rank*$rank*1")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180*$2")),
                          Some (Speed (None,numExpr "1+$rank*$rank*1")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// rRootageより妄想　Part02 by 白い弾幕くん
  /// [OtakuTwo]_self-2020.xml
  let self_2020 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "rRootageより妄想　Part02 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1")); Wait (numExpr "1");
            ChangeSpeed (Speed (None,numExpr "5"),Term (numExpr "1")); Wait (numExpr "15");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "45"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "seed";},[" 1"]);
                   FireRef ({fireRefLabel = FireLabel "seed";},["-1"]); Wait (numExpr "30")]));
            Wait (numExpr "450")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90*$1")),
           Some (Speed (None,numExpr "(1.5-$rank*0.5)")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "$rand*(30-$rank*15)");
                   Repeat
                     (Times (numExpr "9999"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                             Some (Speed (None,numExpr "1")),
                             BulletRef ({bulletRefLabel = BulletLabel "bomb";},[]));
                          Wait (numExpr "30-$rank*15")]))])]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bomb");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "225");
                Fire
                  ({fireLabel = None;},
                   Some
                     (Direction
                        (Some {directionType = Aim;},
                         numExpr "-20+($rand-0.5)*$rank*$rank*$rank*$rank*$rank*$rank*$rank*$rank*20")),
                   Some (Speed (None,numExpr "1.5+$rank*$rank")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "2"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20")),
                          Some (Speed (None,numExpr "1.5+$rank*$rank")),
                          Bullet ({bulletLabel = None;},None,None,[]))]))])])])

  /// おたくツーさん作、自機拘束弾・低速移動型 by 白い弾幕くん
  /// [OtakuTwo]_slow_move.xml
  let slow_move = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "おたくツーさん作、自機拘束弾・低速移動型 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [FireRef ({fireRefLabel = FireLabel "lr";},[" 90"; "1.5"]);
            FireRef ({fireRefLabel = FireLabel "lr";},["-90"; "1.5"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "move";},["  0"; "0.9"]); Wait (numExpr "150");
            Repeat
              (Times (numExpr "100+100*$rank"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "bara";},[" 90"]);
                   FireRef ({fireRefLabel = FireLabel "bara";},["-90"]); Wait (numExpr "10-$rank*5")]));
            Wait (numExpr "30"); Action.ActionRef ({actionRefLabel = ActionLabel "move";},["180"; "0.9"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1"),Term (numExpr "10"));
            Wait (numExpr "12"); ChangeSpeed (Speed (None,numExpr "$2"),Term (numExpr "50")); Wait (numExpr "55");
            ChangeSpeed (Speed (None,numExpr " 0"),Term (numExpr "50")); Wait (numExpr "55")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "bara");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (None,numExpr "1+$rank*5")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "30-$rand*60")),
                      Some (Speed (None,numExpr "1")),
                      Bullet ({bulletLabel = None;},None,None,[])); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "lr");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
           Some (Speed (None,numExpr "0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["$1"; "$2"]);
                   FireRef ({fireRefLabel = FireLabel "tb";},["  0"; "0.9"]);
                   FireRef ({fireRefLabel = FireLabel "tb";},["180"; "3.0"]); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "tb");},None,Some (Speed (None,numExpr "0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "move";},["$1"; "$2"]); Wait (numExpr "20");
                   ChangeDirection
                     (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "5"));
                   Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "shot";},[]); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "shot");},
           [Repeat
              (Times (numExpr "500"),
               Action
                 ({actionLabel = None;},
                  [ChangeDirection
                     (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "10+$rank*10"));
                   FireRef ({fireRefLabel = FireLabel "winder";},[" 20-$rank*10"]);
                   FireRef ({fireRefLabel = FireLabel "winder";},["-20+$rank*10"]); Wait (numExpr "2")]));
            Vanish]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "winder");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (None,numExpr "8")),Bullet ({bulletLabel = None;},None,None,[]))])