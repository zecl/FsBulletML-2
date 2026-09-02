namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// MDA
[<RequireQualifiedAccess>]
module MAD =
  
  /// 紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん
  /// [MDA]_2f.xml
  let b2f =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、地形トラップ風味 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [FireRef
              ({fireRefLabel = FireLabel "seed";},["0"; "57"; "0.8"; "0.8"; "0"; "-0.8"; "0"]);
            FireRef
              ({fireRefLabel = FireLabel "seed";},
               ["270"; "206"; "1.73"; "0"; "-1.2"; "0"; "1.2"]);
            FireRef ({fireRefLabel = FireLabel "seed2";},[]); Wait (numExpr "3*(260+(60-($rank*60)))")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$3")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "90");
                   FireRef
                     ({fireRefLabel = FireLabel "leaf";},["1"; "$1"; "$4"; "$5"; "$6"; "$7"]);
                   FireRef
                     ({fireRefLabel = FireLabel "leaf";},["-1"; "$1"; "$4"; "$5"; "$6"; "$7"]);
                   Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "leaf");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "50")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                      Some (Speed (None,numExpr "5.1")),
                      BulletRef ({bulletRefLabel = BulletLabel "curve";},["$1"]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["35"; "$1"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["120"; "$1/2"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["45"; "0"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["90"; "$3/2"; "$1"]);
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "move";},["60-($rank*60)"; "0"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["120"; "$4*3/8"; "$1"]);
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "move";},["60-($rank*60)"; "0"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["90"; "$5/2"; "$1"]);
                   Action.ActionRef
                     ({actionRefLabel = ActionLabel "move";},["60-($rank*60)"; "0"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["120"; "$6*3/8"; "$1"]);
                   Action.ActionRef ({actionRefLabel = ActionLabel "move";},["45"; "0"; "$1"]); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "move");},
           [Repeat
              (Times (numExpr "$1"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$2")),
                      Some (Speed (None,numExpr "5.1")),
                      BulletRef ({bulletRefLabel = BulletLabel "curve";},["$3"])); Wait (numExpr "1")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "curve");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1*85"),
                   Term (numExpr "9-($rank*5)"))])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed2");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "131.5")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.05")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "90");
                   ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
                   Repeat
                     (Times (numExpr "10"),
                      Action
                        ({actionLabel = None;},
                         [Fire
                            ({fireLabel = None;},
                             Some (Direction (Some {directionType = Aim;},numExpr "0")),
                             Some (Speed (None,numExpr "1.4+(0.4*$rank*$rank)")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = Aim;},numExpr "3*$rand*$rank")),
                             Some (Speed (None,numExpr "1.4+(0.4*$rand*$rank*$rank)")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction
                                  (Some {directionType = Aim;},numExpr "-3*$rand*$rank")),
                             Some (Speed (None,numExpr "1.4+(0.4*$rand*$rank*$rank)")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Wait (numExpr "3*(260+(60-($rank*60)))/10")])); Vanish])]))])

  /// 紫月飴さんのオリジナル、花。by 白い弾幕くん
  /// [MDA]_10flower_2.xml
  let b10flower_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、花。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = BulletLabel "seed";},["36.1"; "144"]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "5")),
               BulletRef ({bulletRefLabel = BulletLabel "seed";},["-36.4"; "3.4+144"]));
            Wait (numExpr "164+316*$rank")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "seed");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "4")); Wait (numExpr "4");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$2")),
                   Some (Speed (None,numExpr "1.1")),
                   BulletRef ({bulletRefLabel = BulletLabel "dummy";},[]));
                Repeat
                  (Times (numExpr "21+79*$rank"),
                   Action
                     ({actionLabel = None;},
                      [Repeat
                         (Times (numExpr "10"),
                          Action
                            ({actionLabel = None;},
                             [Fire
                                ({fireLabel = None;},
                                 Some
                                   (Direction
                                      (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                                 Some (Speed (None,numExpr "1.1")),
                                 Bullet ({bulletLabel = None;},None,None,[]))]));
                       Wait (numExpr "4")])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "dummy");},None,None,
           [Action ({actionLabel = None;},[Vanish])])])

  /// 紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん
  /// [MDA]_14b_2-3w.xml
  let b14b_2_3w =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、棒状バラマキと変則3way by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [FireRef ({fireRefLabel = FireLabel "seed_a";},[]);
            Repeat (Times (numExpr "35+$rank*21"),ActionRef ({actionRefLabel = ActionLabel "seed_b";},[]));
            Wait (numExpr "110")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "seed_b");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "7")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.4")),
               BulletRef ({bulletRefLabel = BulletLabel "shoot";},[]));
            Repeat
              (Times (numExpr "$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.14")),
                      BulletRef ({bulletRefLabel = BulletLabel "shoot";},[]))]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "180")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.4")),
               BulletRef ({bulletRefLabel = BulletLabel "shoot";},[]));
            Repeat
              (Times (numExpr "$rank*6"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "-0.14")),
                      BulletRef ({bulletRefLabel = BulletLabel "shoot";},[]))])); Wait (numExpr "11")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "shoot");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "18");
                Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "1.4")),
                   Bullet ({bulletLabel = None;},None,None,[]));
                Repeat
                  (Times (numExpr "7-1"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some
                            (Direction (Some {directionType = DirectionType.Sequence;},numExpr "360/7")),
                          Some (Speed (None,numExpr "1.4")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed_a");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Repeat
                     (Times (numExpr "3+$rank*7"),
                      Action
                        ({actionLabel = None;},
                         [FireRef ({fireRefLabel = FireLabel "fire1";},["1"; "1"]);
                          FireRef ({fireRefLabel = FireLabel "fire1";},["-1"; "1"]);
                          FireRef ({fireRefLabel = FireLabel "fire1";},["0.5"; "-2"]);
                          FireRef ({fireRefLabel = FireLabel "fire1";},["-0.5"; "-2"]);
                          Wait (numExpr "63")])); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "fire1");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1*90")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.5")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10");
                   ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.5"),Term (numExpr "1"));
                   Wait (numExpr "1");
                   Repeat
                     (Times (numExpr "4+$rank*5"),
                      Action
                        ({actionLabel = None;},
                         [FireRef ({fireRefLabel = FireLabel "fire2";},["$2*$1"; "-1"]);
                          FireRef ({fireRefLabel = FireLabel "fire2";},["$2*$1"; "1"]);
                          Wait (numExpr "3")])); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "fire2");},
           Some (Direction (Some {directionType = Aim;},numExpr "($1+$2)*7")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.2+$rank*1")),
           Bullet
             ({bulletLabel = Some (BulletLabel "dummy");},None,None,
              [Action ({actionLabel = None;},[])]))])

  /// 紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん
  /// [MDA]_75l-42.xml
  let b75l_42 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、糸が降ってきた。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Fire
              ({fireLabel = None;},None,None,
               Bullet
                 ({bulletLabel = None;},
                  Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "120")),
                  Some (Speed (None,numExpr "9.2-$rank*4")),
                  [ActionRef ({actionRefLabel = ActionLabel "right";},[])]));
            Fire
              ({fireLabel = None;},None,None,
               Bullet
                 ({bulletLabel = None;},
                  Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "240")),
                  Some (Speed (None,numExpr "9.2-$rank*4")),
                  [ActionRef ({actionRefLabel = ActionLabel "left";},[])])); Wait (numExpr "40");
            Fire
              ({fireLabel = None;},None,None,
               Bullet
                 ({bulletLabel = None;},
                  Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "240")),
                  Some (Speed (None,numExpr "9.2-$rank*4")),
                  [ActionRef ({actionRefLabel = ActionLabel "right";},[])]));
            Fire
              ({fireLabel = None;},None,None,
               Bullet
                 ({bulletLabel = None;},
                  Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "120")),
                  Some (Speed (None,numExpr "9.2-$rank*4")),
                  [ActionRef ({actionRefLabel = ActionLabel "left";},[])])); Wait (numExpr "100")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "right");},
           [Repeat
              (Times (numExpr "32"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "shoot";},["0+1"; "1.4"]);
                   FireRef ({fireRefLabel = FireLabel "shoot";},["60+1"; "0.7"]);
                   FireRef ({fireRefLabel = FireLabel "shoot";},["300+1"; "2.1"]); Wait (numExpr "1")]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "left");},
           [Repeat
              (Times (numExpr "32"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "shoot";},["360-1"; "1.4"]);
                   FireRef ({fireRefLabel = FireLabel "shoot";},["300-1"; "0.7"]);
                   FireRef ({fireRefLabel = FireLabel "shoot";},["60-1"; "2.1"]); Wait (numExpr "1")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "shoot");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
           Some (Speed (None,numExpr "$2")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "45"); Accel (None,Some (Vertical (None,numExpr "4.2")),Term (numExpr "120"))])]))])

  /// 紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん
  /// [MDA]_acc_n_dec.xml
  let acc_n_dec =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、加速弾と減速弾 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "5+(20*$rank)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "seed";},["90"]);
                   FireRef ({fireRefLabel = FireLabel "seed";},["270"]); Wait (numExpr "55-($rank*30)")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (None,numExpr "2.0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "20")); Wait (numExpr "20");
                   Action.ActionRef ({actionRefLabel = ActionLabel "way";},[]); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "way");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "$rand*60-30-70")),
               Some (Speed (None,numExpr "4.2")),BulletRef ({bulletRefLabel = BulletLabel "br";},[]));
            Repeat
              (Times (numExpr "7"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "8.5")),
                      Some (Speed (None,numExpr "1.05")),
                      BulletRef ({bulletRefLabel = BulletLabel "ac";},[]));
                   Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "8.5")),
                      Some (Speed (None,numExpr "4.2")),
                      BulletRef ({bulletRefLabel = BulletLabel "br";},[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "br");},None,None,
           [Action
              ({actionLabel = None;},[ChangeSpeed (Speed (None,numExpr "1.05"),Term (numExpr "25"))])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "ac");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "8.4"),Term (numExpr "150")); Wait (numExpr "9999");
                Fire
                  ({fireLabel = None;},None,None,
                   Bullet ({bulletLabel = None;},None,None,[]))])])])

  /// 紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん
  /// [MDA]_circular.xml
  let circular =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、四方からと自機狙い。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "$rank*10"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "seed";},["90"; "2"; "355"]);
                   FireRef ({fireRefLabel = FireLabel "seed";},["270"; "358"; "5"]);
                   FireRef ({fireRefLabel = FireLabel "aimbl";},[]); Wait (numExpr "20")]));
            Repeat
              (Times (numExpr "9"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "aimbl";},[]); Wait (numExpr "20")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (None,numExpr "2.8")),
           BulletRef ({bulletRefLabel = BulletLabel "roll";},["$2"; "$3"]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "roll");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1"),Term (numExpr "9999"));
                Action.ActionRef ({actionRefLabel = ActionLabel "shoot";},["$2"]); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "shoot");},
           [Repeat
              (Times (numExpr "22"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "1.4")),
                      Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "4+$rand*8")]))]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "aimbl");},
           Some (Direction (Some {directionType = Aim;},numExpr "0")),
           Some (Speed (None,numExpr "2.8")),Bullet ({bulletLabel = None;},None,None,[]))])

  /// 紫月飴さんのオリジナル、四方から。 by 白い弾幕くん
  /// [MDA]_circular_model.xml
  let circular_model =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、四方から。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [FireRef ({fireRefLabel = FireLabel "seed";},["90"; "2"; "355"]);
            FireRef ({fireRefLabel = FireLabel "seed";},["270"; "358"; "5"]);
            Wait (numExpr "380-$rank*200")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (None,numExpr "2.8")),
           BulletRef ({bulletRefLabel = BulletLabel "roll";},["$2"; "$3"]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "roll");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1"),Term (numExpr "9999"));
                Action.ActionRef ({actionRefLabel = ActionLabel "shoot";},["$2"]); Vanish])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "shoot");},
           [Repeat
              (Times (numExpr "22*8"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
                      Some (Speed (None,numExpr "0.4+$rank")),
                      Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "1")]))])])

  /// 紫月飴さんのオリジナル、春っぽい by 白い弾幕くん
  /// [MDA]_circular_sun.xml
  let circular_sun =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、春っぽい by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.75"),Term (numExpr "1"));
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "90"),Term (numExpr "1")); Wait (numExpr "1");
            ChangeDirection
              (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0.7"),Term (numExpr "514"));
            Wait (numExpr "2");
            Repeat
              (Times (numExpr "32"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "shoot";},[]); Wait (numExpr "16")]));
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "1"));
            Wait (numExpr "120")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "shoot");},
           [Repeat
              (Times (numExpr "1+(63*$rank)"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some
                        (Direction
                           (Some {directionType = DirectionType.Sequence;},numExpr "360/(1+(63*$rank))")),
                      Some (Speed (None,numExpr "1.28+(0.08*$rand)")),
                      BulletRef ({bulletRefLabel = BulletLabel "curve";},[]))]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "curve");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeDirection
                  (Direction (Some {directionType = DirectionType.Sequence;},numExpr "1.25-(1.6*$rand)"),
                   Term (numExpr "360")); Wait (numExpr "360"); Vanish])])])

  /// 紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん             
  /// [MDA]_double_w.xml
  let double_w =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、全方位弾二回。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "seed";},["0.31"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "seed";},["0.00"]); Wait (numExpr "30")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "seed");},
           [Repeat
              (Times (numExpr "($rank*$rank*50+21)*(2-$1)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "shoot1";},["$1"]);
                   Repeat
                     (Times (numExpr "10*(1+$1)"),
                      Action
                        ({actionLabel = None;},
                         [FireRef ({fireRefLabel = FireLabel "shoot2";},[])])); Wait (numExpr "1")]));
            Wait (numExpr "60")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "shoot1");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "41")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.0-$1")),
           Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "shoot2");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-19")),
           Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0.1")),
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// 紫月飴さんのオリジナル、袋詰め by 白い弾幕くん
  /// [MDA]_fukuro.xml
  let fukuro =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、袋詰め by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [ChangeDirection
              (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180"),Term (numExpr "1"));
            ChangeSpeed (Speed (None,numExpr "1"),Term (numExpr "1")); Wait (numExpr "30");
            ChangeSpeed (Speed (None,numExpr "0"),Term (numExpr "1"));
            Repeat
              (Times (numExpr "$rank*17+1"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "seed";},["3"; "$rank*18+1"]);
                   FireRef ({fireRefLabel = FireLabel "seed";},["2"; "$rank*18+1"]);
                   FireRef ({fireRefLabel = FireLabel "seed";},["1"; "$rank*18+1"]); Wait (numExpr "10")]));
            Wait (numExpr "(3*($rank*18*10))")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "seed");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "360/($2*3)")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "1.75*$1")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "10");
                   ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "10"));
                   Wait (numExpr "(($1-1)*($2*10))+30");
                   Action.ActionRef ({actionRefLabel = ActionLabel "n_way";},[]); Vanish])]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "n_way");},
           [FireRef ({fireRefLabel = FireLabel "curve";},["2.00"; "60"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["2.00"; "-60"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.64"; "52.5"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.64"; "-52.5"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.41"; "45"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.41"; "-45"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.16"; "30"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.16"; "-30"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.04"; "15"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.04"; "-15"]);
            FireRef ({fireRefLabel = FireLabel "curve";},["1.00"; "0"])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "curve");},
           Some (Direction (Some {directionType = Aim;},numExpr "$2")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$1*2.0")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "5");
                   ChangeDirection
                     (Direction (Some {directionType = Aim;},numExpr "0"),Term (numExpr "5"))])]))])

  /// 紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん
  /// [MDA]_gnnnyari.xml
  let gnnnyari =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、なんか生々しい。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "seed";},[]); Wait (numExpr "120")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "seed");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180")),
               Some (Speed (None,numExpr "7.5")),BulletRef ({bulletRefLabel = BulletLabel "shoot";},[]));
            Wait (numExpr "2");
            Repeat
              (Times (numExpr "30+$rank*80"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "27")),
                      Some (Speed (None,numExpr "7.5")),
                      BulletRef ({bulletRefLabel = BulletLabel "shoot";},[])); Wait (numExpr "2")]))]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "shoot");},None,None,
           [Action
              ({actionLabel = None;},
               [Fire
                  ({fireLabel = None;},
                   Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "0")),
                   Some (Speed (None,numExpr "1.0+0.4*$rank")),
                   BulletRef ({bulletRefLabel = BulletLabel "dummy";},[]));
                Repeat
                  (Times (numExpr "11"),
                   Action
                     ({actionLabel = None;},
                      [Fire
                         ({fireLabel = None;},
                          Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                          Some (Speed (None,numExpr "1.0+0.4*$rank")),
                          Bullet ({bulletLabel = None;},None,None,[]))])); Vanish])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "dummy");},None,None,
           [Action ({actionLabel = None;},[])])])

  /// 紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん
  /// [MDA]_mojya.xml
  let mojya =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、もじゃ。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Repeat
              (Times (numExpr "15+25*$rank"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "first";},["15"]);
                   FireRef ({fireRefLabel = FireLabel "first";},["153"]); Wait (numExpr "3")]));
            Wait (numExpr "240")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "first");},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),
           Some (Speed (None,numExpr "0.54")),BulletRef ({bulletRefLabel = BulletLabel "second";},[]));
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "second");},None,None,
           [Action
              ({actionLabel = None;},
               [Wait (numExpr "60"); FireRef ({fireRefLabel = FireLabel "third";},["21+$rand*84"]);
                FireRef ({fireRefLabel = FireLabel "third";},["-21-$rand*84"]);
                FireRef ({fireRefLabel = FireLabel "third";},["7+$rand*28"]);
                FireRef ({fireRefLabel = FireLabel "third";},["-7-$rand*28"]);
                FireRef ({fireRefLabel = FireLabel "third";},["$rand*14"]);
                FireRef ({fireRefLabel = FireLabel "third";},["$rand*(-14)"]);
                FireRef ({fireRefLabel = FireLabel "third";},["0"]); Vanish])]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "third");},
           Some (Direction (Some {directionType = Aim;},numExpr "$1")),
           Some (Speed (None,numExpr "0.4+$rand*1.4")),
           Bullet ({bulletLabel = None;},None,None,[]))])

  /// 紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん
  /// [MDA]_mossari.xml
  let mossari =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、もっさり。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "seed";},["-2"; "0"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "seed";},["25"; "10"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "seed";},["41"; "-10"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "center";},[]); Wait (numExpr "180")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "seed");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+$1")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$1/4-2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [ActionRef ({actionRefLabel = ActionLabel "shoot";},["-1*$2"])]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180-$1")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "$1/4-2")),
               Bullet
                 ({bulletLabel = None;},None,None,
                  [ActionRef ({actionRefLabel = ActionLabel "shoot";},["$2"])]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "shoot");},
           [Wait (numExpr "9");
            ChangeSpeed (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0"),Term (numExpr "4"));
            Wait (numExpr "4");
            Repeat
              (Times (numExpr "10+($rank*30)"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = FireLabel "shoot2";},["0+($rand*30)"; "$1"]);
                   FireRef ({fireRefLabel = FireLabel "shoot2";},["0-($rand*30)"; "$1"]);
                   Wait (numExpr "24-($rand*12)")])); Vanish]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "shoot2");},
           Some (Direction (Some {directionType = Aim;},numExpr "$1+$2")),
           Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.6")),
           Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "center");},
           [Wait (numExpr "10");
            Repeat
              (Times (numExpr "12+($rank*20)"),
               Action
                 ({actionLabel = None;},
                  [Action.ActionRef ({actionRefLabel = ActionLabel "center2";},[]);
                   Repeat (Times (numExpr "7-1"),ActionRef ({actionRefLabel = ActionLabel "center3";},[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "center2");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = Aim;},numExpr "-60")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.6")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Repeat
              (Times (numExpr "12"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "10")),
                      Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "0.6")),
                      Bullet ({bulletLabel = None;},None,None,[]))])); Wait (numExpr "4")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "center3");},
           [Action.ActionRef ({actionRefLabel = ActionLabel "wind";},["46"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "wind";},["16"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "wind";},["47.5"]);
            Action.ActionRef ({actionRefLabel = ActionLabel "wind";},["15"])]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "wind");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180+$1")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.7")),
               Bullet ({bulletLabel = None;},None,None,[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "180-$1")),
               Some (Speed (Some {speedType = SpeedType.Absolute;},numExpr "2.7")),
               Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "1")])])

  /// 紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん
  /// [MDA]_wind_cl.xml
  let wind_cl =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "紫月飴さんのオリジナル、どっちも奇数弾。 by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top");},
           [FireRef ({fireRefLabel = FireLabel "side";},["120"]);
            FireRef ({fireRefLabel = FireLabel "side";},["240"]); Wait (numExpr "31");
            Repeat
              (Times (numExpr "5+$rank*20"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},None,None,
                      BulletRef ({bulletRefLabel = BulletLabel "center";},[])); Wait (numExpr "30")]));
            Wait (numExpr "100")]);
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "side");},
           Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "$1")),
           Some (Speed (None,numExpr "18.6")),
           Bullet
             ({bulletLabel = None;},None,None,
              [Action
                 ({actionLabel = None;},
                  [Wait (numExpr "1"); ChangeSpeed (Speed (None,numExpr "0.02"),Term (numExpr "2")); Wait (numExpr "30");
                   ChangeDirection (Direction (None,numExpr "0"),Term (numExpr "1"));
                   Repeat
                     (Times (numExpr "77+$rank*306"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "2"); ChangeDirection (Direction (None,numExpr "0"),Term (numExpr "30"));
                          FireRef ({fireRefLabel = FireLabel "3way";},["0"]);
                          FireRef ({fireRefLabel = FireLabel "3way";},["20"]);
                          FireRef ({fireRefLabel = FireLabel "3way";},["-20"])])); Vanish])]));
        BulletmlElm.Fire
          ({fireLabel = Some (FireLabel "3way");},
           Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
           Some (Speed (None,numExpr "4.9")),Bullet ({bulletLabel = None;},None,None,[]));
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "2way");},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "$1")),
               Some (Speed (None,numExpr "2.3")),BulletRef ({bulletRefLabel = BulletLabel "dummy";},[]));
            Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Relative;},numExpr "-$1")),
               Some (Speed (None,numExpr "2.3")),BulletRef ({bulletRefLabel = BulletLabel "dummy";},[]));
            Wait (numExpr "5")]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "dummy");},None,None,
           [Action ({actionLabel = None;},[])]);
        BulletmlElm.Bullet
          ({bulletLabel = Some (BulletLabel "center");},None,None,
           [Action
              ({actionLabel = None;},
               [ChangeSpeed (Speed (None,numExpr "0.01"),Term (numExpr "1"));
                Action.ActionRef ({actionRefLabel = ActionLabel "2way";},["0"]);
                Action.ActionRef ({actionRefLabel = ActionLabel "2way";},["8-$rank*4"]);
                Action.ActionRef ({actionRefLabel = ActionLabel "2way";},["16-$rank*8"]); Vanish])])])
