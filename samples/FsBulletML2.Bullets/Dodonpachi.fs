namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Dodonpachi
[<RequireQualifiedAccess>]
module Dodonpachi =

  /// 怒首領蜂、火蜂。by 白い弾幕くん
  /// [Dodonpachi]_hibachi.xml
  let hibachi =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂、火蜂。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "allWay";},
           [Fire
              ({fireLabel = None;},Some (Direction (None,numExpr "-50+$rand*20")),
               Some (Speed (None,numExpr "1+$rank")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "15+16*$rank*$rank"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Sequence;},numExpr "24-$rank*12")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "right";},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "25"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "allWay";},[]); Wait (numExpr "3")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "left";},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90"),Term (numExpr "1"));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "25"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "allWay";},[]); Wait (numExpr "3")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = "right";},[]);
                   Action.ActionRef ({actionRefLabel = "left";},[]);
                   Action.ActionRef ({actionRefLabel = "left";},[]);
                   Action.ActionRef ({actionRefLabel = "right";},[])]));
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1")])])
  
  /// 怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_1.xml
  let kitiku_1 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂、最終鬼畜兵器その一。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "fast";},None,Some (Speed (None,numExpr "10")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "6"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "20");
                Repeat
                  (Times (numExpr "10+$rank*18"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "-11-$rand*2")),
                          Some (Speed (None,numExpr "1.5")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Action.ActionRef ({actionRefLabel = "add3";},[]);
                       Repeat
                         (Times (numExpr "4"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                                 Some
                                   (Speed
                                      (Some {speedType = SpeedType.Sequence;},numExpr "0.1+$rank*0.2")),
                                 Bullet ({bulletLabel = None;},None,None,[]));
                              Action.ActionRef ({actionRefLabel = "add3";},[])]));
                       Wait (numExpr "336/(10+$rank*18)")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "add3";},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Fire
          ({fireLabel = Some "slowColorChange";},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+45*$1")),
           Some (Speed (None,numExpr "7")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "6"); ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                   Repeat
                     (Times (numExpr "50+$rank*50"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Sequence;},numExpr "(8-$rank*4)*$1")),
                             Some (Speed (None,numExpr "1.2")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Action.ActionRef ({actionRefLabel = "add3";},[]);
                          Wait (numExpr "8-$rank*4+$rand")])); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some "slow";},None,None,
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = "slowColorChange";},["$1"]); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-85")),None,
               BulletRef ({bulletRefLabel = "fast";},[])); Wait (numExpr "1");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "85")),None,
               BulletRef ({bulletRefLabel = "fast";},[])); Wait (numExpr "1");
            FireRef ({fireRefLabel = "slow";},["1"]); Wait (numExpr "1");
            FireRef ({fireRefLabel = "slow";},["-1"]); Wait (numExpr "430")])])

  /// 怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_2.xml
  let kitiku_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂、最終鬼畜兵器その二。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "feather";},None,Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "6");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),None,
                   Bullet
                     ({bulletLabel = None;},None,None,
                      [Action ({actionLabel = None;},[Vanish])]));
                ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1")); Wait (numExpr "1");
                Repeat
                  (Times (numExpr "100"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Sequence;},numExpr "10-(3+$rank*6)*3")),
                          Some (Speed (None,numExpr "1")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Repeat
                         (Times (numExpr "3+$rank*6"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                                 Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.15")),

                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "4")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),None,
               BulletRef ({bulletRefLabel = "feather";},[])); Wait (numExpr "1");
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),None,
               BulletRef ({bulletRefLabel = "feather";},[])); Wait (numExpr "430")])])

  /// 怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん
  /// [Dodonpachi]_kitiku_3.xml
  let kitiku_3 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂、最終鬼畜兵器その三。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top1";},
           [Repeat
              (Times (numExpr "200+$rank*200"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-50+$rand*100")),
                      Some (Speed (None,numExpr "1.6")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "2-$rank+$rand")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "kobati";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "$1");
                Repeat
                  (Times (numExpr "20"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = Aim;},numExpr "0")),
                          Some (Speed (None,numExpr "1.6")),
                          Bullet ({bulletLabel = None;},None,None,[]));
                       Wait (numExpr "(16-$rank*8)*3")])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some "top2";},
           [Repeat
              (Times (numExpr "8+$rank*8"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["(16-$rank*8)*3"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["(16+$rank*8)*3"]));
                   Wait (numExpr "16-$rank*8");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["(16-$rank*8)*2"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["(16-$rank*8)*2"]));
                   Wait (numExpr "16-$rank*8");
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["16-$rank*8"]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-80")),
                      Some (Speed (None,numExpr "1.5")),
                      BulletRef ({bulletRefLabel = "kobati";},["16-$rank*8"]));
                   Wait (numExpr "16-$rank*8")])); Wait (numExpr "120")])])

  /// 怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん
  /// [Dodonpachi]_kitiku_5.xml
  let kitiku_5 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = None;
        bulletmlName = Some "怒首領蜂、最終鬼畜兵器その五。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top1";},
           [Repeat
              (Times (numExpr "30+$rank*30"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = DirectionType.Sequence;},numExpr "220+$rand*2")),
                      Some (Speed (None,numExpr "1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (None,numExpr "1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "120")),
                      Some (Speed (None,numExpr "1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "15")),
                      Some (Speed (None,numExpr "1.2")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "20-$rank*10")]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top2";},
           [Repeat
              (Times (numExpr "30+$rank*30"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = Aim;},numExpr "-30+$rand*60")),
                      Some (Speed (None,numExpr "1.3")),
                      Bullet
                        ({bulletLabel = None;},None,None,
                         [Action ({actionLabel = None;},[])])); Wait (numExpr "20-$rank*10")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some "kobati";},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "5");
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
                ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.2"),Term (numExpr "1"));
                Wait (numExpr "1");
                Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "30-$rank*10");
                       Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "45")),
                          Some (Speed (None,numExpr "0.4+$rank*0.2")),
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
                                 Some (Speed (None,numExpr "0.4+$rank*0.2")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]))]))])]);
        BulletmlElm.Action
          ({actionLabel = Some "top3";},
           [Repeat
              (Times (numExpr "5+$rank*5"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                      Some (Speed (None,numExpr "10")),
                      BulletRef ({bulletRefLabel = "kobati";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                      Some (Speed (None,numExpr "5")),
                      BulletRef ({bulletRefLabel = "kobati";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
                      Some (Speed (None,numExpr "10")),
                      BulletRef ({bulletRefLabel = "kobati";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
                      Some (Speed (None,numExpr "5")),
                      BulletRef ({bulletRefLabel = "kobati";},[]));
                   Wait (numExpr "120-$rank*60")])); Wait (numExpr "120")])])
