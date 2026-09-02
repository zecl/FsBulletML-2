namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// Psyvariar
[<RequireQualifiedAccess>]
module Psyvariar =

  /// サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん
  /// [Psyvariar]_4-D_boss_MZIQ.xml
  let b4_D_boss_MZIQ =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "サイヴァリア4-Dボス、MZIQかも。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "add11";},
           [Repeat
              (Times (numExpr "11"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "30")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "30"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-11")),
                      Some (Speed (None,numExpr "1+$rank")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = "add11";},[]);
                   Repeat
                     (Times (numExpr "3"),
                      Action
                        ({actionLabel = None;},
                         [Wait (numExpr "4-$rank*2+$rand");
                          Fire
                            ({fireLabel = None;},
                             Some
                               (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-5+30")),
                             Some (Speed (None,numExpr "1+$rank")),
                             Bullet ({bulletLabel = None;},None,None,[]));
                          Action.ActionRef ({actionRefLabel = "add11";},[])]));
                   Wait (numExpr "4-$rank*2+$rand")])); Wait (numExpr "30-$rank*30")])])

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_opening.xml
  let X_A_boss_opening =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "サイヴァリア、多分最終面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some "top";},
           [Repeat
              (Times (numExpr "600"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},Some (Direction (None,numExpr "-45+$rand*90")),
                      Some (Speed (None,numExpr "(0.3+$rand*0.5)*($rank+1)")),
                      Bullet ({bulletLabel = None;},None,None,[])); Wait (numExpr "1")]));
            Wait (numExpr "100")])])

  /// サイヴァリア、多分最終面ボス。by 白い弾幕くん
  /// [Psyvariar]_X-A_boss_winder.xml
  let X_A_boss_winder =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "サイヴァリア、多分最終面ボス。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Bullet
          ({bulletLabel = Some "winderBullet";},None,Some (Speed (None,numExpr "3")),[]);
        BulletmlElm.Fire
          ({fireLabel = Some "fireWinder";},
           Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "$1")),None,
           BulletRef ({bulletRefLabel = "winderBullet";},[]));
        BulletmlElm.Action
          ({actionLabel = Some "roundWinder";},
           [FireRef ({fireRefLabel = "fireWinder";},["$1"]);
            Repeat
              (Times (numExpr "11"),
               Action
                 ({actionLabel = None;},
                  [FireRef ({fireRefLabel = "fireWinder";},["30"])])); Wait (numExpr "5")]);
        BulletmlElm.Action
          ({actionLabel = Some "winderSequence";},
           [Repeat (Times (numExpr "12"),ActionRef ({actionRefLabel = "roundWinder";},["30"]));
            Repeat (Times (numExpr "12"),ActionRef ({actionRefLabel = "roundWinder";},["$1"]));
            Repeat (Times (numExpr "12"),ActionRef ({actionRefLabel = "roundWinder";},["30"]))]);
        BulletmlElm.Action
          ({actionLabel = Some "top1";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "2")),None,
               BulletRef ({bulletRefLabel = "winderBullet";},[]));
            Action.ActionRef ({actionRefLabel = "winderSequence";},["30.9+0.1*$rank"])]);
        BulletmlElm.Action
          ({actionLabel = Some "top2";},
           [Fire
              ({fireLabel = None;},
               Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-2")),None,
               BulletRef ({bulletRefLabel = "winderBullet";},[]));
            Action.ActionRef ({actionRefLabel = "winderSequence";},["29.1-0.1*$rank"])])])
