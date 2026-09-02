namespace FsBulletML2.Bullets.PlayerBullet
open FsBulletML2

/// その他
[<RequireQualifiedAccess>]
module PlayerBullet = 

  /// 2Way Left
  let b2wayLeftBullet = 
    Bulletml.Bulletml ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "2Way Left"; bulletmlDescription = None},
      [BulletmlElm.Action ({actionLabel = Some "top";},
        [Action.Fire ({fireLabel = None;}, Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "-10")), Some (Speed (None,numExpr "20")), BulletElm.Bullet ({bulletLabel = None;}, None, None, []));
         Action.Fire ({fireLabel = None;},  Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")), Some (Speed (None,numExpr "20")), BulletElm.Bullet ({bulletLabel = None;}, None, None, []))])])

  /// 2Way Right
  let b2wayRightBullet = 
    Bulletml.Bulletml ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "2Way Right"; bulletmlDescription = None},
      [BulletmlElm.Action ({actionLabel = Some "top";}, 
        [Action.Fire ({fireLabel = None;}, Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "10")), Some (Speed (None,numExpr "20")), BulletElm.Bullet ({bulletLabel = None;}, None, None, []));
         Action.Fire ({fireLabel = None;}, Some (Direction (Some {directionType = DirectionType.Absolute;},numExpr "0")), Some (Speed (None,numExpr "20")), BulletElm.Bullet ({bulletLabel = None;}, None, None, []))])])

  /// ホーミング弾
  let homing = 
    Bulletml.Bulletml ({bulletmlXmlns = None; bulletmlType = Some ShootingDirection.BulletHorizontal; bulletmlName = Some "ホーミング弾"; bulletmlDescription = None},
      [BulletmlElm.Action ({actionLabel = Some "top";},
        [Action.Repeat (Times (numExpr "2"), ActionElm.Action ({actionLabel = None;}, 
                         [Action.Fire ({fireLabel = None;}, Some (Direction (None,numExpr "(-30+$rand*120)")), None, BulletElm.BulletRef ({bulletRefLabel = "hmgLsr";}, [])); 
                          Action.Repeat (Times (numExpr "5"), ActionElm.Action ({actionLabel = None;},
                                                       [Action.Wait (numExpr "1");
                                                        Action.Fire ({fireLabel = None;}, Some (Direction (Some {directionType = DirectionType.Sequence;},numExpr "0")), None, BulletElm.BulletRef ({bulletRefLabel = "hmgLsr";}, []))]));
                                                        Action.Wait (numExpr "10")]))]);
       BulletmlElm.Bullet ({bulletLabel = Some "hmgLsr";}, None, Some (Speed (None,numExpr "2")), 
                            [ActionElm.Action ({actionLabel = None;},
                              [Action.ChangeSpeed (Speed (None,numExpr "0.3"), Term (numExpr "40")); Action.Wait (numExpr "100");
                               Action.ChangeSpeed (Speed (None,numExpr "5"), Term (numExpr "90"))]); 
                             ActionElm.Action ({actionLabel = None;},
                              [Action.Repeat (Times (numExpr "9999"), ActionElm.Action ({actionLabel = None;},
                                                              [Action.ChangeDirection (Direction (Some {directionType = DirectionType.Aim;},numExpr "0"), Term (numExpr "40-$rank*20"));
                                                               Action.Wait (numExpr "5")]))])])])

