namespace FsBulletML2.Parser.Tests.Snippet
open FsBulletML2.DTD 

// bulletml snippet

[<AutoOpen>]
module Attribute = 
  let actionAttr = { actionLabel = Some (ActionLabel "actionName") } 
  let actionRefAttr = { actionRefLabel = ActionLabel "actionRefName" } 
  let bulletAttr = { bulletLabel = Some (BulletLabel "bulletName") } 
  let bulletmlAttr = { bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; 
                       bulletmlType = Some ShootingDirection.BulletVertical 
                       bulletmlName = Some "No Name"
                       bulletmlDescription = None} 
  let bulletRefAttr = { bulletRefLabel = BulletLabel "bulletRefName" } 
  let directionAttr = { directionType = DirectionType.Aim } 
  let fireAttr = { fireLabel = Some (FireLabel "fireName") } 
  let fireRefAttr = { fireRefLabel = FireLabel "fireRefName" } 
  let horizontalAttr = { horizontalType = HorizontalType.Absolute } 
  let speedAttr = { speedType = SpeedType.Absolute } 
  let verticalAttr = { verticalType = VerticalType.Absolute } 

[<AutoOpen>]
module Top = 
  let bulletml = Bulletml({ bulletmlXmlns = Some "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml"; bulletmlType = Some ShootingDirection.BulletVertical; bulletmlName = Some "No Name"; bulletmlDescription = None}, 
                    []) 
  let horizontal = Horizontal ( Some { horizontalType = HorizontalType.Absolute }, numExpr "1") 
  let vertical = Vertical ( Some { verticalType = VerticalType.Absolute }, numExpr "1") 
  let term = Term (numExpr "1") 
  let times = Times (numExpr "1") 
  let direction = Direction( Some { directionType = DirectionType.Aim } , numExpr "1") 
  let speed = Speed ( Some { speedType = SpeedType.Absolute } , numExpr "1") 

[<AutoOpen>]
module BulletElm = 
  let bulletElm_bullet = BulletElm.Bullet ({ bulletLabel = Some (BulletLabel "bulletName") }, 
                             None, 
                             None, 
                             []) 
  let bulletElm_bulletRef = BulletElm.BulletRef ({ bulletRefLabel = BulletLabel "bulletRefName" },
                                []) 
  
[<AutoOpen>]
module BulletmlElm = 
  let bulletmlElm_action= BulletmlElm.Action ({ actionLabel = Some (ActionLabel "actionName") }, 
                              []) 

  let bulletmlElm_bullet = BulletmlElm.Bullet ({ bulletLabel = Some (BulletLabel "bulletName") },
                               None,
                               None,
                               []) 

  let bulletmlElm_fire = BulletmlElm.Fire  ({ fireLabel = Some (FireLabel "fireName") }, 
                             None,
                             None,
                             bulletElm_bullet) 

[<AutoOpen>]
module ActionElm =
  let actionElm_action = ActionElm.Action ({ actionLabel = Some (ActionLabel "actionName") },
                             []) 
  let actionElm_actionRef =  ActionElm.ActionRef ({ actionRefLabel = ActionLabel "actionRefName" },
                                 []) 
  
[<AutoOpen>]
module Action = 
  let action_accel = Action.Accel (
                         None,
                         None,
                         Term (numExpr "1")) 

  let action_action = Action.Action ({ actionLabel = Some (ActionLabel "actionName") },
                          []) 

  let action_actionRef = Action.ActionRef ({ actionRefLabel = ActionLabel "actionRefName" },
                             []) 

  let action_changeDirection = Action.ChangeDirection ( 
                                   Direction( Some directionAttr, numExpr "1"),
                                   Term (numExpr "1")) 

  let aciton_changeSpeed = Action.ChangeSpeed ( 
                               Speed ( Some speedAttr, numExpr "1"), 
                               Term (numExpr "1")) 

  let action_fire = Action.Fire ({ fireLabel = Some (FireLabel "fireName") },
                        None, 
                        None, 
                        bulletElm_bullet) 

  let action_fireRef = Action.FireRef ({ fireRefLabel = FireLabel "fireRefName" },
                           []) 

  let repeat = Action.Repeat ( 
                   Times(numExpr "1"), 
                   actionElm_action) 

  let action_vanish = Action.Vanish   
  let action_wait = Action.Wait(numExpr "1") 

