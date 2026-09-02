namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// KetuiLt
[<RequireQualifiedAccess>]
module KetuiLt =

  /// ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん
  /// [Ketui_LT]_1boss_bit.xml
  let b1boss_bit =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ケツイロケテより、一面ボスのビット攻撃 by 白い弾幕くん";
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
          ({actionLabel = Some (ActionLabel "3way");},
           [Repeat
              (Times (numExpr "2"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "30");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction (Some {directionType = Aim;},numExpr "-3")),
                      Some (Speed (None,numExpr "1.4")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "XWay";},
                      ["3"; "2"])]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "bit");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "3"),
                   Action
                     ({actionLabel = None;},
                      [Accel
                         (Some
                            (Horizontal
                               (Some {horizontalType = Absolute;},numExpr "0")),
                          Some
                            (Vertical
                               (Some {verticalType = VerticalType.Absolute;},numExpr "1")),
                          Term (numExpr "60"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "3way";},[]);
                       Accel
                         (Some
                            (Horizontal
                               (Some {horizontalType = Absolute;},numExpr "-2")),
                          Some
                            (Vertical
                               (Some {verticalType = VerticalType.Absolute;},numExpr "0")),
                          Term (numExpr "60"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "3way";},[]);
                       Accel
                         (Some
                            (Horizontal
                               (Some {horizontalType = Absolute;},numExpr "0")),
                          Some
                            (Vertical
                               (Some {verticalType = VerticalType.Absolute;},numExpr "-1")),
                          Term (numExpr "60"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "3way";},[]);
                       Accel
                         (Some
                            (Horizontal
                               (Some {horizontalType = Absolute;},numExpr "2")),
                          Some
                            (Vertical
                               (Some {verticalType = VerticalType.Absolute;},numExpr "0")),
                          Term (numExpr "60"));
                       Action.ActionRef ({actionRefLabel = ActionLabel "3way";},[])]))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "4+$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Absolute;},numExpr "90")),
                      Some (Speed (None,numExpr "2")),
                      BulletRef ({bulletRefLabel = BulletLabel "bit";},[]));
                   Wait (numExpr "245/(4+$rank*6)")])); Wait (numExpr "550")])])

  /// ケツイロケテより、三ボスのくねくね by 白い弾幕くん
  /// [Ketui_LT]_3boss_kunekune.xml
  let b3boss_kunekune =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ケツイロケテより、三ボスのくねくね by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "aimSrc");},None,Some (Speed (None,numExpr "3")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
                Repeat
                  (Times (numExpr "5+$rank*10"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "340/(5+$rank*10)");
                       Repeat
                         (Times (numExpr "3"),
                          Action
                            ({actionLabel = None;},
                             [Wait (numExpr "2");
                              Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = Aim;},numExpr "0")),
                                 Some (Speed (None,numExpr "2")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]))]));
                Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "circleSrc");},None,Some (Speed (None,numExpr "4")),
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "10");
                ChangeSpeed
                  (Speed (None,numExpr "0.5+$rank"),Term (numExpr "1"));
                ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "5"),
                   Term (numExpr "9999"));
                Repeat
                  (Times (numExpr "200"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "2");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                          Some (Speed (None,numExpr "3+$rand*0.02")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "circleSrc";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-90")),
               None,BulletRef ({bulletRefLabel = BulletLabel "circleSrc";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "135")),
               None,BulletRef ({bulletRefLabel = BulletLabel "aimSrc";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-135")),
               None,BulletRef ({bulletRefLabel = BulletLabel "aimSrc";},[]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Wait (numExpr "12-$rank*8");
                   Repeat
                     (Times (numExpr "4+$rank*4"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "2");
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = DirectionType.Absolute;},numExpr "180")),
                             Some (Speed (None,numExpr "4")),
                             Bullet
                               ({bulletLabel = None;},None,None,
                                [Action
                                   ({actionLabel = None;},
                                    [Wait (numExpr "6");
                                     ChangeSpeed
                                       (Speed (None,numExpr "1"),
                                        Term (numExpr "5"));
                                     Wait (numExpr "20");
                                     ChangeDirection
                                       (Direction
                                          (Some {directionType = Aim;},numExpr "0"),
                                        Term (numExpr "1"));
                                     ChangeSpeed
                                       (Speed (None,numExpr "2.2"),
                                        Term (numExpr "1"))])]))]))]))])])

  /// ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん
  /// [Ketui_LT]_3boss_roll_and_aim.xml
  let b3boss_roll_and_aim =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ケツイロケテより、三ボスの自機狙い弾と横殴り弾 by 白い弾幕くん";
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
           [Wait (numExpr "$rand * 30");
            Repeat
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
                                  (Some {directionType = DirectionType.Sequence;},numExpr "$1*5")),
                             Some
                               (Speed (None,numExpr "1.5+$rank*$rank*1.5")),
                             BulletRef
                               ({bulletRefLabel = BulletLabel "curve";},
                                ["$1"]));
                          Repeat
                            (Times (numExpr "4"),
                             Action
                               ({actionLabel = None;},
                                [Fire
                                   ({fireLabel = None;},
                                    Some
                                      (Direction
                                         (Some {directionType = DirectionType.Sequence;},numExpr "90")),
                                    Some
                                      (Speed
                                         (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                                    BulletRef
                                      ({bulletRefLabel = BulletLabel "curve";},
                                       ["$1"]))]));
                          Wait (numExpr "6 + $rand * 3")]));
                   Wait (numExpr "6");
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "1+$rank*2")),
                      BulletRef ({bulletRefLabel = BulletLabel "Dummy";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "twoWay");},
           [Repeat
              (Times (numExpr "5+$rank*4"),
               Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "3+$rank*4"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = Aim;},numExpr "3")),
                             Some (Speed (None,numExpr "1.8")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = Aim;},numExpr "-3")),
                             Some (Speed (None,numExpr "1.8")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Wait (numExpr "5")])); 
                   Wait (numExpr "20")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "spiral";},["-2"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "spiral";},["2"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "twoWay";},[])])])


  /// ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん
  /// [Ketui_LT]_2boss_winder_crash.xml
  let b2boss_winder_crash =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = None;
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ケツイロケテより、二面ボスのワインダー？ by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "pre");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-20")),
               Some (Speed (None,numExpr "2")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "40")),
                      Some (Speed (None,numExpr "4")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "-40")),
                      Some (Speed (None,numExpr "4")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Wait (numExpr "2")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "missile");},None,None,
           [Action
              ({actionLabel = None;},
               [Repeat
                  (Times (numExpr "9999"),
                   Action
                     ({actionLabel = None;},
                      [Wait (numExpr "5-$rank*2+$rand");
                       Fire
                         ({fireLabel = None;},
                          Some
                            (Direction
                               (Some {directionType = Aim;},numExpr "0")),
                          Some (Speed (None,numExpr "0.0000001")),
                          Bullet
                            ({bulletLabel = None;},None,None,
                             [Action
                                ({actionLabel = None;},
                                 [Wait (numExpr "60");
                                  ChangeSpeed
                                    (Speed (None,numExpr "3"),
                                     Term (numExpr "30"))])]))]))])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "missiles");},
           [Fire
              ({fireLabel = None;},
               Some
                 (Direction
                    (Some {directionType = DirectionType.Sequence;},numExpr "-($1-1)*1.5")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = BulletLabel "missile";},[]));
            Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                      Some (Speed (None,numExpr "4")),
                      BulletRef ({bulletRefLabel = BulletLabel "missile";},[]))]));
            Fire
              ({fireLabel = None;},
               Some
                 (Direction
                    (Some {directionType = DirectionType.Sequence;},numExpr "40-($1-1)*3")),
               Some (Speed (None,numExpr "4")),
               BulletRef ({bulletRefLabel = BulletLabel "missile";},[]));
            Repeat
              (Times (numExpr "$1-1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "3")),
                      Some (Speed (None,numExpr "4")),
                      BulletRef ({bulletRefLabel = BulletLabel "missile";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "pre";},[]);
            Action.ActionRef ({actionRefLabel = ActionLabel "missiles";},["3+$rank*4"]);
            Wait (numExpr "160"); 
            Action.ActionRef ({actionRefLabel = ActionLabel "pre";},[]);
            Action.ActionRef ({actionRefLabel = ActionLabel "missiles";},["4+$rank*6"]);
            Wait (numExpr "160")])])
