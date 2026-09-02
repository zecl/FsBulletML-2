namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Daiouzyou
[<RequireQualifiedAccess>]
module Daiouzyou =

  /// 怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_1.xml
  let hibachi_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」開幕攻撃 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "10+$rank*70"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = Aim;},numExpr "$rand*30-74+$rank*2")),
                      Some (Speed (None,numExpr "0.5+$rank*2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]);
                   FireRef ({fireRefLabel = FireLabel "n";},[]); Wait (numExpr "14-$rank*10")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "n");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$rand*2+7-$rank*2")),
           Some (Speed (None,numExpr "0.5+$rank*2")),
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// 怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_2.xml
  let hibachi_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」超速青弾 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "175")),
               Some (Speed (None,numExpr "1+$rank*4")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "30"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]); Wait (numExpr "1");
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "rights";},[]); Wait (numExpr "15-$rank*10");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                      Some (Speed (None,numExpr "1+$rank*4")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "tops");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "185")),
               Some (Speed (None,numExpr "1+$rank*4")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "30"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]);
                   FireRef ({fireRefLabel = FireLabel "allway";},[]); Wait (numExpr "1");
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "lefts";},[]); Wait (numExpr "15-$rank*10");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-4")),
                      Some (Speed (None,numExpr "1+$rank*4")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "lefts");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-0.7")),
               Some (Speed (None,numExpr "1+$rank*4")),
               Bullet ({bulletLabel = None;},None,None,[]));
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]); Wait (numExpr "1")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "rights");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0.7")),
               Some (Speed (None,numExpr "1+$rank*4")),
               Bullet ({bulletLabel = None;},None,None,[]));
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]);
            FireRef ({fireRefLabel = FireLabel "allway";},[]); Wait (numExpr "1")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "allway");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "45")),
           Some (Speed (None,numExpr "1+$rank*4")),
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// 怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_3.xml
  let hibachi_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」第三攻撃 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2"),Term (numExpr "1"));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            Wait (numExpr "10");
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "1.5")),BulletRef ({bulletRefLabel = BulletLabel "blue";},[]));

            Repeat
              (Times (numExpr "60+$rank*60"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]);
                   FireRef ({fireRefLabel = FireLabel "red";},[]); Wait (numExpr "20-$rank*14");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-31.5")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "blue";},[]))]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "red");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
           Some (Speed (None,numExpr "1.5")),BulletRef ({bulletRefLabel = BulletLabel "blue";},[]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "blue");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "30");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-110")),
                   Some (Speed (None,numExpr "1.5")),
                   Bullet ({bulletLabel = None;},None,None,[]))])])])

  /// 怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん
  /// [Daiouzyou]_hibachi_4.xml
  let hibachi_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」発狂攻撃 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "45")),
               Some (Speed (None,numExpr "1+$rank*0.5")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "113+900/(16-$rank*10)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "four";},[]);
                   FireRef ({fireRefLabel = FireLabel "four";},[]);
                   FireRef ({fireRefLabel = FireLabel "four";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "86")),
                      Some (Speed (None,numExpr "1+$rank*0.5")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "16-$rank*10")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "tops");},
           [Wait (numExpr "(16-$rank*10)*22.5");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "45")),
               Some (Speed (None,numExpr "1+$rank*0.5")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "91+900/(16-$rank*10)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "four";},[]);
                   FireRef ({fireRefLabel = FireLabel "four";},[]);
                   FireRef ({fireRefLabel = FireLabel "four";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "94")),
                      Some (Speed (None,numExpr "1+$rank*0.5")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "16-$rank*10")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "topt");},
           [Wait (numExpr "(16-$rank*10)*45");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "gurugurup";},[]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "gurugurup");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Action.ActionRef ({actionRefLabel = ActionLabel "guru2";},[]); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "guru2");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")),
               Some (Speed (None,numExpr "1+$rank")),
               BulletRef ({bulletRefLabel = BulletLabel "guruc";},[]));
            Repeat
              (Times (numExpr "450/(16-$rank*10)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru2";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc2";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru3";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc3";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru4";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc4";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru5";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc5";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru6";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc6";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru7";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "21")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc7";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "6"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru8";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "21.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc8";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru9";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc9";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru10";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.7")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc10";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru11";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "23")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc11";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "6"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru12";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "23.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc12";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru13";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc13";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru14";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc14";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru15";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc15";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru16";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc16";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru17";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc17";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru18";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc18";},[]));
                   Wait (numExpr "16-$rank*10")]));
            Repeat
              (Times (numExpr "450/(16-$rank*10)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   FireRef ({fireRefLabel = FireLabel "guru19";},[]);
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24.5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      BulletRef ({bulletRefLabel = BulletLabel "guruc19";},[]));
                   Wait (numExpr "16-$rank*10")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru2");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc2";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru3");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc3";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru4");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc4";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru5");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc5";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru6");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc6";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru7");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc7";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru8");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc8";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru9");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),BulletRef ({bulletRefLabel = BulletLabel "guruc9";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru10");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.3+$rand*0.4")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc10";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru11");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc11";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru12");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc12";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru13");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc13";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru14");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc14";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru15");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc15";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru16");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc16";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru17");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc17";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru18");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc18";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "guru19");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
           Some (Speed (None,numExpr "1+$rank")),
           BulletRef ({bulletRefLabel = BulletLabel "guruc19";},[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "four");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
           Some (Speed (None,numExpr "1+$rank*0.5")),
           Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "90"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc2");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "170"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc3");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "260"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc4");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "300"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc5");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "450"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc6");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "600"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc7");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "700"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc8");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "800"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc9");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "-270"),Term (numExpr "900"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc10");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "0"),Term (numExpr "90"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc11");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "900"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc12");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "800"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc13");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "700"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc14");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "600"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc15");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "450"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc16");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "300"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc17");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "280"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc18");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "230"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "guruc19");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "270"),Term (numExpr "90"))])])])

  /// 怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん
  /// [Daiouzyou]_hibachi_image.xml
  let hibachi_image = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」最終形態を妄想してみた by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWayFan");},
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
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "spiral");},
           [Fire
              ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action
                     ({actionLabel = None;},
                      [Repeat
                         (Times (numExpr "30+$rank*45"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                                 Some (Speed (None,numExpr "1+$rank*2")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Action.ActionRef
                                ({actionRefLabel = ActionLabel "XWay";},
                                 ["10"; "36"]);
                              Wait (numExpr "6-$rank*3")])); Vanish])]));
            Wait (numExpr "225")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "spiral";},["7"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "spiral";},["-7"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fan4");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-$1")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "60+$rank*90"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "1+$rank*2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["4"; "90"]);
                   Wait (numExpr "6-$rank*3")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fan4";},["4"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fan4";},["-4"])])])

  /// 怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん
  /// [Daiouzyou]_hibachi_maybe.xml
  let hibachi_maybe =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生「緋蜂」最終形態に多分似たもの by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWayFan");},
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
          ({bulletLabel = Some (BulletLabel "curve");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [ChangeDirection
                         (Direction
                            (Some {directionType = DirectionType.Relative;},numExpr "-$1*(4+$rank*$rank*4)"),
                          Term (numExpr "10")); Wait (numExpr "10")]))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "spiral");},
           [Repeat
              (Times (numExpr "10+$rank*15"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "2"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                             Some
                               (Speed (None,numExpr "1.5+$rank*$rank*1.5")),
                             BulletRef
                               ({bulletRefLabel = BulletLabel "curve";},
                                ["$1"]));
                          Repeat
                            (Times (numExpr "10+$rank*10-1"),
                             Action
                               ({actionLabel = None;},
                                [Fire
                                   ({fireLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "36/($rank+1)")),
                                    Some
                                      (Speed
                                         (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                    BulletRef
                                      ({bulletRefLabel = BulletLabel "curve";},
                                       ["$1"]))]));
                          Wait (numExpr "6-$rank*3")]));
                   Wait (numExpr "6-$rank*3");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "1+$rank*2")),
                      BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "spiral";},["-2"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fan4");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-$1")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "30+$rank*45"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "1+$rank*$rank*2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["4"; "90"]);
                   Wait (numExpr "6-$rank*3")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fan4";},["4"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "fan4";},["-4"])])])

  /// 怒首領蜂大往生一面ボス by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss.xml
  let round_1_boss = 
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂大往生一面ボス by 白い弾幕くん";
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
                      Some (Speed (None,numExpr "4")),
                      BulletRef ({bulletRefLabel = BulletLabel "seed";},[])); Wait (numExpr "500")]));
            Wait (numExpr "100")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "9");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed2";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "180")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed2";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed2");},None,Some (Speed (None,numExpr "18")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "1");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed3";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed3");},None,Some (Speed (None,numExpr "0.8")),
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1.2"),Term (numExpr "9999"));
                Repeat
                  (Times (numExpr "100+200*$rank"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180-12")),

                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),

                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "3-$rank*2*$rand")]))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "6"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},None,None,
                          Bullet
                            ({bulletLabel = None;},Some (Direction (None,numExpr "-8")),None
    ,
                             []));
                       Repeat
                         (Times (numExpr "4"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},None,None,
                                 Bullet
                                   ({bulletLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                                    None,[Action ({actionLabel = None;},[])]))]));
                       Wait (numExpr "80")])); Vanish])])])

  /// 怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん
  /// [Daiouzyou]_round_1_boss_hakkyou.xml
  let round_1_boss_hakkyou =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生一面ボス、発狂。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Repeat
              (Times (numExpr "128"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "4"); Action.ActionRef ({actionRefLabel = ActionLabel "four";},["$rand*90+135"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "four");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "6")),BulletRef ({bulletRefLabel = BulletLabel "rb";},["$1"]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "60")),
                      Some (Speed (None,numExpr "6")),
                      BulletRef ({bulletRefLabel = BulletLabel "rb";},["$1"]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rb");},None,None,
           [ActionRef ({actionRefLabel = ActionLabel "red";},["$1+$rand*20-10"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "red");},
           [Wait (numExpr "1");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
               Some (Speed (None,numExpr "1+$rank")),
               Bullet ({bulletLabel = None;},None,None,[])); Vanish]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "160");
                   Fire
                     ({fireLabel = None;},None,None,
                      BulletRef ({bulletRefLabel = BulletLabel "sht";},["1.2"])); Wait (numExpr "80")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "sht");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "16"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},Some (Direction (None,numExpr "$rand*16-8")),
                          Some (Speed (None,numExpr "($1+$rand*$1)*($rank/2+0.65)")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "rd_seed";},["-5"; "-5"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "rd_seed";},["5"; "5"]));
                   Wait (numExpr "240")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rd_seed");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "1");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = BulletLabel "rd_seed2";},[]));
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = BulletLabel "bd_seed";},["0"; "$2"]));
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = BulletLabel "bd_seed";},["$1"; "$2"])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rd_seed2");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Repeat
                         (Times (numExpr "3"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                                 Some (Speed (None,numExpr "1.2")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Wait (numExpr "4")])); Wait (numExpr "12")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bd_seed");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},Some (Direction (None,numExpr "$2")),
                   Some (Speed (None,numExpr "0.6")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "11"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                          Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.2")),
                          Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "4")]));
                Vanish])])])

  /// 怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss.xml
  let round_3_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」第二形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "155")),
               Some (Speed (None,numExpr "3.3")),
               BulletRef ({bulletRefLabel = BulletLabel "roll";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "205")),
               Some (Speed (None,numExpr "3.3")),
               BulletRef ({bulletRefLabel = BulletLabel "roll";},["-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "135")),
               Some (Speed (None,numExpr "3.2")),
               BulletRef ({bulletRefLabel = BulletLabel "roll";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "225")),
               Some (Speed (None,numExpr "3.2")),
               BulletRef ({bulletRefLabel = BulletLabel "roll";},["-1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "135")),
               Some (Speed (None,numExpr "2")),BulletRef ({bulletRefLabel = BulletLabel "roll";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "225")),
               Some (Speed (None,numExpr "2")),BulletRef ({bulletRefLabel = BulletLabel "roll";},["-1"]));
            Wait (numExpr "400")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "roll");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "12"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+90*$1")),
                   None,
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                Repeat
                  (Times (numExpr "200"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "9")),
                          Some (Speed (None,numExpr "1+$rank")),
                          Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "2")]));
                Vanish])])])

  /// 怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_2.xml
  let round_3_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」第三形態 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Red");},None,None,[Action ({actionLabel = None;},[])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWayFan");},
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
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "aim2";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "aim2";},[]));
            Fire
              ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [ActionRef ({actionRefLabel = ActionLabel "fanRoll";},["7"])]));
            Fire
              ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [ActionRef ({actionRefLabel = ActionLabel "fanRoll";},["-7"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "3wayRoll";},["13"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "3wayRoll";},["-13"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "3wayRoll");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "14"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "-1.3*$1")),
                      Some (Speed (None,numExpr "1.4+$rank*0.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["3"; "$1"]);
                   Wait (numExpr "10")]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "1.3*$1")),
                      Some (Speed (None,numExpr "1.4+$rank*0.8")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["3"; "-$1"]);
                   Wait (numExpr "10")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fanRoll");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1*8")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "32"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "-$1*2.1")),
                      Some (Speed (None,numExpr "1.2+$rank*0.4")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWayFan";},
                      ["4"; "$1"; "0.3"]);
                   Wait (numExpr "10")])); Vanish]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "aim2");},None,Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "8"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "14+$rank*12"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "320/(14+$rank*12)+$rand");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "0")),
                          Some (Speed (None,numExpr "1.4+$rank*0.8")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]))])); Vanish])])])

  /// 怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん
  /// [Daiouzyou]_round_3_boss_last.xml
  let round_3_boss_last =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生三面ボス「厳武」発狂 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "100")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = BulletLabel "armSrc";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-100")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = BulletLabel "armSrc";},["0"]));
            FireRef ({fireRefLabel = FireLabel "center";},[]); Wait (numExpr "500")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "center3");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-10.5*$1")),None,
               BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "6"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                             Some (Speed (None,numExpr "1+$rank")),
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
                                    Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                    Bullet ({bulletLabel = None;},None,None,[]))]));
                          Wait (numExpr "5")]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),None,
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[Vanish])])); Wait (numExpr "5")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "center");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
           Some (Speed (None,numExpr "5")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                   Repeat
                     (Times (numExpr "2"),
                      Action
                        ({actionLabel = None;},
                         [Action.ActionRef ({actionRefLabel = ActionLabel "center3";},["-4"]);
                          Wait (numExpr "30"); Action.ActionRef ({actionRefLabel = ActionLabel "center3";},["4"]);
                          Wait (numExpr "30")])); Vanish])]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "armSrc");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "12"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1");
                FireRef ({fireRefLabel = FireLabel "arm";},["8-16*$1"; "0"]); Wait (numExpr "2");
                FireRef ({fireRefLabel = FireLabel "arm";},["8-16*$1"; "90"]); Wait (numExpr "2");
                FireRef ({fireRefLabel = FireLabel "arm";},["8-16*$1"; "180"]); Wait (numExpr "2");
                FireRef ({fireRefLabel = FireLabel "arm";},["8-16*$1"; "270"]); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "arm");},None,Some (Speed (None,numExpr "0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                      Some (Speed (None,numExpr "1.5")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "80+$rank*80"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "480/(80+$rank*80)");
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                             Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                             Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]))])

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss.xml
  let round_4_boss =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その三 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = BulletLabel "armSrc";},["1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               Some (Speed (None,numExpr "3")),
               BulletRef ({bulletRefLabel = BulletLabel "armSrc";},["0"])); Wait (numExpr "400")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "armSrc");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "12");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "1"]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "60")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "1"]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-60")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "1"]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "-1"]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "60")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "-1"]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-60")),
                   Some (Speed (None,numExpr "1")),
                   BulletRef ({bulletRefLabel = BulletLabel "arm";},["$1"; "-1"])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "arm");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "12");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "180*$1")),None,
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Repeat
                  (Times (numExpr "400/(6-$rank*2)"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "6-$rank*2+$rand");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "11*$2")),
                          Some (Speed (None,numExpr "1.5+$rank*0.5")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_1.xml
  let round_4_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その一 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "20"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "20");
                       Repeat
                         (Times (numExpr "3"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "116+$rand*6-$rank*15")),
                                 Some (Speed (None,numExpr "1.5")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Repeat
                                (Times (numExpr "3.5+$rank*5"),
                                 Action
                                   ({actionLabel = None;},
                                    [Fire
                                       ({fireLabel = None;},
                                        Some
                                          (Direction
                                             (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                                        Some
                                          (Speed
                                             (None,numExpr "1.5")),
                                        Bullet ({bulletLabel = None;},None,None,[]))]))]))]));
                Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "xway");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-7*$1-7")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "$1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (None,numExpr "1.3")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])]));
                   Repeat
                     (Times (numExpr "4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                             Some
                               (Speed
                                  (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action ({actionLabel = None;},[])]))]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               None,BulletRef ({bulletRefLabel = BulletLabel "seed";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               None,BulletRef ({bulletRefLabel = BulletLabel "seed";},[])); Wait (numExpr "400")])])

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_2.xml
  let round_4_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その二 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "blue");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "16+$rank*16"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "10-$rank*4+$rand");
                       Repeat
                         (Times (numExpr "3"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "95")),
                                 Some (Speed (None,numExpr "1.4")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Repeat
                                (Times (numExpr "3"),
                                 Action
                                   ({actionLabel = None;},
                                    [Fire
                                       ({fireLabel = None;},
                                        Some
                                          (Direction
                                             (Some {directionType = DirectionType.Sequence;},numExpr "10")),
                                        Some
                                          (Speed
                                             (None,numExpr "1.4")),
                                        Bullet ({bulletLabel = None;},None,None,[]))]))]))]));
                Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "xway");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-7*$1-7")),
               None,BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Repeat
              (Times (numExpr "$1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (None,numExpr "1.3")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])]));
                   Repeat
                     (Times (numExpr "4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                             Some
                               (Speed
                                  (Some {speedType = SpeedType.Sequence;},numExpr "0.08+$rank*0.08")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action ({actionLabel = None;},[])]))]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "red");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Action.ActionRef
                         ({actionRefLabel = ActionLabel "xway";},
                          ["$rand*3+$rank*2"]);
                       Wait (numExpr "40")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "120")),
               None,BulletRef ({bulletRefLabel = BulletLabel "blue";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-120")),
               None,BulletRef ({bulletRefLabel = BulletLabel "red";},[])); Wait (numExpr "200");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-120")),
               None,BulletRef ({bulletRefLabel = BulletLabel "blue";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "120")),
               None,BulletRef ({bulletRefLabel = BulletLabel "red";},[])); Wait (numExpr "200")])])

  /// 怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_4.xml
  let round_4_boss_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第一形態その四 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "XWayFan";},
               ["$1"; "$2"; "0"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWayFan");},
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
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "fan");},
           [Wait (numExpr "30");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
               Some (Speed (None,numExpr "1.2+$rank")),
               BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
            Action.ActionRef ({actionRefLabel = ActionLabel "XWay";},["$2"; "$3"]);
            Repeat
              (Times (numExpr "6"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "30");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$4")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["$2"; "$3"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["220"; "8"; "5"; "-42.5"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["150"; "8"; "-5"; "42.5"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["200"; "7"; "2.5"; "-22.5"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["170"; "7"; "-2.5"; "22.5"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["160"; "8"; "5"; "-42.5"]);
            Action.ActionRef
              ({actionRefLabel = ActionLabel "fan";},
               ["210"; "8"; "-5"; "42.5"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top4");},
           [Wait (numExpr "20");
            Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "36+$rank*20"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (None,numExpr "$rand*360")),
                             Some (Speed (None,numExpr "2")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action
                                   ({actionLabel = None;},
                                    [Wait (numExpr "10*$rand");
                                     Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                                     Wait (numExpr "60");
                                     ChangeDirection
                                       (Direction
                                          (Some {directionType = Aim;},numExpr "0"),
                                        Term (numExpr "1"));
                                     ChangeSpeed
                                       (Speed (None,numExpr "2.4"),
                                        Term (numExpr "1"))])]));
                          Wait (numExpr "3")]));
                   Wait (numExpr "60-$rank*60")]))])])

  /// 怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_4_boss_5.xml
  let round_4_boss_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生四面ボス「逝流」第二形態その一 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
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
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "blueFan");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "6"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "120+$1*2")),
                          Some (Speed (None,numExpr "1.6")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = ActionLabel "XWay";},
                          ["3"; "120"]);
                       Repeat
                         (Times (numExpr "6+$rank*6"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "56/(6+$rank*6)");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "120+$1")),
                                 Some (Speed (None,numExpr "1.6")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Action.ActionRef
                                ({actionRefLabel = ActionLabel "XWay";},
                                 ["3"; "120"])]));
                       Wait (numExpr "14")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "singleRedAim");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "0")),
               Some (Speed (None,numExpr "2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[])]));
            Repeat
              (Times (numExpr "15"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "4");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (None,numExpr "2")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "doubleRedAim");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-5*$1")),
               Some (Speed (None,numExpr "2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[])]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "20*$1")),
               Some (Speed (None,numExpr "2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [Action ({actionLabel = None;},[])]));
            Repeat
              (Times (numExpr "15"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "4");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "-20*$1")),
                      Some (Speed (None,numExpr "2")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])]));
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "20*$1")),
                      Some (Speed (None,numExpr "2")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "redAim2");},None,Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Wait (numExpr "100");
                Action.ActionRef ({actionRefLabel = ActionLabel "singleRedAim";},[]);
                Wait (numExpr "60");
                Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim";},["-1"]);
                Wait (numExpr "20");
                Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim";},["-1"]);
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "redAim1");},None,Some (Speed (None,numExpr "1")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "20"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Wait (numExpr "40");
                Action.ActionRef ({actionRefLabel = ActionLabel "singleRedAim";},[]);
                Wait (numExpr "60");
                Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim";},["1"]);
                Wait (numExpr "80");
                Action.ActionRef ({actionRefLabel = ActionLabel "doubleRedAim";},["1"]);
                Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "blueFan";},["4"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "blueFan";},["-4"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "redAim2";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "redAim1";},[]));
            Wait (numExpr "400")])])

  /// 怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_1.xml
  let round_5_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生五面ボス「黄流」第一形態その一 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "XWay");},
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
          ({bulletLabel = Some (BulletLabel "aim3");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = BulletLabel "aim3Impl";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "aim3Impl");},None,None,
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
                               (Some {directionType = Aim;},
                                numExpr "-33+$rand*6")),
                          Some (Speed (None,numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef
                         ({actionRefLabel = ActionLabel "XWay";},
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
                                ({actionRefLabel = ActionLabel "XWay";},
                                 ["3"; "30"])]));
                       Wait (numExpr "54-$rank*9")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "aim");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                Fire
                  ({fireLabel = None;},None,Some (Speed (None,numExpr "0")),
                   BulletRef ({bulletRefLabel = BulletLabel "aimImpl";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "aimImpl");},None,None,
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
                               (Some {directionType = Aim;},
                                numExpr "-3+$rand*6")),
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
          ({bulletLabel = Some (BulletLabel "fan");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
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
                         ({actionRefLabel = ActionLabel "XWay";},
                          ["7"; "10"]);
                       Wait (numExpr "420/(3+$rank*4)")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = BulletLabel "aim3";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = BulletLabel "aim3";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "125")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = BulletLabel "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-125")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = BulletLabel "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "150")),
               Some (Speed (None,numExpr "7")),
               BulletRef ({bulletRefLabel = BulletLabel "aim";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-150")),
               Some (Speed (None,numExpr "7")),
               BulletRef ({bulletRefLabel = BulletLabel "aim";},[])); Wait (numExpr "10");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "6")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["-135"; "10"; "1.3"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               Some (Speed (None,numExpr "6")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["135"; "10"; "1.3"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "110")),
               Some (Speed (None,numExpr "4")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["-164"; "8"; "1.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-110")),
               Some (Speed (None,numExpr "4")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["156"; "8"; "1.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "130")),
               Some (Speed (None,numExpr "2")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["180"; "8"; "1.1"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-130")),
               Some (Speed (None,numExpr "2")),
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["180"; "5"; "1.1"]));
            Wait (numExpr "430")])])
  
  /// 怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん
  /// [Daiouzyou]_round_5_boss_2.xml
  let round_5_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生五面ボス「黄流」第一形態その二 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Red");},None,None,[Action ({actionLabel = None;},[])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "Stop");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "Dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seven");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
           Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Repeat
                  (Times (numExpr "5+$rank*4"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-10")),
                          Some (Speed (None,numExpr "1.5")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "10")),
                          Some (Speed (None,numExpr "1.5")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-5")),
                          Some (Speed (None,numExpr "1.3")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "5")),
                          Some (Speed (None,numExpr "1.3")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "-5")),
                          Some (Speed (None,numExpr "1.7")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "5")),
                          Some (Speed (None,numExpr "1.7")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "0")),
                          Some (Speed (None,numExpr "1.5")),
                          BulletRef ({bulletRefLabel = BulletLabel "Red";},[]));
                       Wait (numExpr "360/(5+$rank*4)")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "fan");},None,Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10"); Action.ActionRef ({actionRefLabel = ActionLabel "Stop";},[]);
                Fire
                  ({fireLabel = None;},Some (Direction (None,numExpr "$1")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]));
                Repeat
                  (Times (numExpr "35+$rank*35"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                          Some (Speed (None,numExpr "$3")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "10/(1+$rank)+$rand")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "seven";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["55"; "10"; "1.8+$rank*0.4"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["60"; "10"; "1+$rank*0.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["225"; "10"; "1.4+$rank*0.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["250"; "10"; "1.3+$rank*0.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["55"; "-10"; "1.8+$rank*0.4"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["60"; "-10"; "1+$rank*0.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["225"; "-10"; "1.4+$rank*0.2"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-170")),
               None,
               BulletRef
                 ({bulletRefLabel = BulletLabel "fan";},
                  ["250"; "-10"; "1.3+$rank*0.2"]));
            Wait (numExpr "360")])])
  
  /// 怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_1.xml
  let round_6_boss_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その一 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "64"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "2"); Action.ActionRef ({actionRefLabel = ActionLabel "four";},["$rand*90+135"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "four");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               Some (Speed (None,numExpr "6")),BulletRef ({bulletRefLabel = BulletLabel "rb";},["$1"]));
            Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "60")),
                      Some (Speed (None,numExpr "6")),
                      BulletRef ({bulletRefLabel = BulletLabel "rb";},["$1"]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "rb");},None,None,
           [ActionRef ({actionRefLabel = ActionLabel "red";},["$1+$rand*20-10"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "red");},
           [Wait (numExpr "1");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
               Some (Speed (None,numExpr "1.2+$rank")),
               Bullet ({bulletLabel = None;},None,None,[])); Vanish])])
  
  /// 怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_2.xml
  let round_6_boss_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その二 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeSpeed (Speed (None,numExpr "4"),Term (numExpr "1"));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            Wait (numExpr "10"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "5"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "bl_seed";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "bl_seed";},[])); Wait (numExpr "80")]));
            ChangeSpeed (Speed (None,numExpr "4"),Term (numExpr "1"));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0"),Term (numExpr "1")); Wait (numExpr "10");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bl_seed");},None,Some (Speed (None,numExpr "24")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "1");
                Fire
                  ({fireLabel = None;},None,None,
                   Bullet
                     ({bulletLabel = None;},None,Some (Speed (None,numExpr "0")),
                      [ActionRef ({actionRefLabel = ActionLabel "bl";},[])])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "bl");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-30")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "4"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))])); Wait (numExpr "4");
            Repeat
              (Times (numExpr "3+$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-30")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.4")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Repeat
                     (Times (numExpr "4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                             Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                             Bullet ({bulletLabel = None;},None,None,[]))]));
                   Wait (numExpr "4")])); Vanish])])

  /// 怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_3.xml
  let round_6_boss_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その三 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "270")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "bm_seed";},["-25"])); Wait (numExpr "20");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
                      BulletRef ({bulletRefLabel = BulletLabel "bm_seed";},["25"])); Wait (numExpr "100")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bm_seed");},None,Some (Speed (None,numExpr "24")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "1");
                Fire
                  ({fireLabel = None;},None,None,
                   Bullet
                     ({bulletLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                      Some (Speed (None,numExpr "3")),
                      [ActionRef ({actionRefLabel = ActionLabel "bm";},[])]));
                Fire
                  ({fireLabel = None;},None,None,
                   Bullet
                     ({bulletLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "2")),
                      [ActionRef ({actionRefLabel = ActionLabel "bm";},[])])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "bm");},
           [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "50")); Wait (numExpr "45");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round";},["1.5"; "0"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round";},["1.25"; "7"]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round";},["1"; "14"])); Vanish]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "round");},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                   Some (Speed (None,numExpr "$1")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "10+$rank*10"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "360/(10+$rank*10)")),
                          Some (Speed (None,numExpr "$1")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])])])

  /// 怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_4.xml
  let round_6_boss_4 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その四 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "sht";},["0.8"])); Wait (numExpr "20");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[])); Wait (numExpr "100");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "sht";},["1"])); Wait (numExpr "20");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[])); Wait (numExpr "100");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[]));
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "sht";},["1.2"])); Wait (numExpr "20");
            Fire
              ({fireLabel = None;},None,None,
               BulletRef ({bulletRefLabel = BulletLabel "round_seed";},[])); Wait (numExpr "25")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "sht");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "16"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},Some (Direction (None,numExpr "$rand*16-8")),
                          Some (Speed (None,numExpr "($1+$rand*$1)*(1+$rank*$rank)")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "round_seed");},None,Some (Speed (None,numExpr "0")),
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},Some (Direction (None,numExpr "0")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "two";},[]));
                Repeat
                  (Times (numExpr "15"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "22.5")),
                          None,BulletRef ({bulletRefLabel = BulletLabel "two";},[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "two");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-4")),
                   Some (Speed (None,numExpr "1+$rank")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "4")),
                   Some (Speed (None,numExpr "1+$rank")),
                   Bullet ({bulletLabel = None;},None,None,[])); Vanish])])])

  /// 怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん
  /// [Daiouzyou]_round_6_boss_5.xml
  let round_6_boss_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "怒首領蜂大往生二周目一面ボス、その五 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                      Some (Speed (None,numExpr "4")),
                      BulletRef ({bulletRefLabel = BulletLabel "seed";},[])); Wait (numExpr "500")]));
            Wait (numExpr "200")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "9");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed2";},[]));
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "180")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed2";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed2");},None,Some (Speed (None,numExpr "18")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "1");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "90")),None,
                   BulletRef ({bulletRefLabel = BulletLabel "seed3";},[])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed3");},None,Some (Speed (None,numExpr "0.8")),
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1.2"),Term (numExpr "9999"))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "62+$rank*100"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "40-10")),
                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "140")),

                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "40")),
                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "140")),

                          None,Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "8-$rank*6")]))]);
            Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "5"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},None,None,
                          BulletRef ({bulletRefLabel = BulletLabel "tw";},[])); Wait (numExpr "138")]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "tw");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},None,None,
                   Bullet
                     ({bulletLabel = None;},Some (Direction (None,numExpr "-12")),None,
                      [Action ({actionLabel = None;},[])]));
                Repeat
                  (Times (numExpr "3.5+$rank*5+$rand"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},None,None,
                          Bullet
                            ({bulletLabel = None;},
                             Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                             None,[Action ({actionLabel = None;},[])]))])); Vanish])])])
