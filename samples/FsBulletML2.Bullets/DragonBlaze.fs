namespace FsBulletML2.Bullets.EnemyBullet.Sdmkun
open FsBulletML2

/// 白い弾幕くんより
/// DragonBlaze
[<RequireQualifiedAccess>]
module DragonBlaze =

  /// ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん
  /// [DragonBlaze]_nebyurosu_2.xml
  let nebyurosu_2 =
    createBulletmlInfo <|
    Bulletml
      ({bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml";
        bulletmlType = Some BulletVertical;
        bulletmlName = Some "ドラゴンブレイズのネビュロス第二形態かも。by 白い弾幕くん";
        bulletmlDescription = None},
       [BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "add3");},
           [Repeat
              (Times (numExpr "3"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "90")),
                      Some (Speed (Some {speedType = SpeedType.Sequence;},numExpr "0")),
                      Bullet ({bulletLabel = None;},None,None,[]))]))]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top1");},
           [Repeat
              (Times (numExpr "150"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "4")),
                      Some (Speed (None,numExpr "1+$rank")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "add3";},[]); Wait (numExpr "2")]));
            Wait (numExpr "60-$rank*30")]);
        BulletmlElm.Action
          ({actionLabel = Some (ActionLabel "top2");},
           [Repeat
              (Times (numExpr "150"),
               Action
                 ({actionLabel = None;},
                  [Fire
                     ({fireLabel = None;},
                      Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "-5")),
                      Some (Speed (None,numExpr "1+$rank")),
                      Bullet ({bulletLabel = None;},None,None,[]));
                   Action.ActionRef ({actionRefLabel = ActionLabel "add3";},[]); Wait (numExpr "2")]));
            Wait (numExpr "60-$rank*30")])])
