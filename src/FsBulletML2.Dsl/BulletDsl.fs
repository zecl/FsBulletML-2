namespace FsBulletML2

/// BulletML 0.21 の書く側を、コンピュテーション式にしたもの。
/// 出る値は公開 DU（`Bulletml` / `Action` / `BulletElm`）で、エンジンは触らない。
///
/// ビルダは DTD の内容モデルで分かれている。
/// action は命令の列、fire / accel は袋、bullet は袋に action を足す。
/// 1 個の CE にすると fire の中に wait が書けて、DTD より緩くなる。
///
/// **`namespace FsBulletML2` に置いてある。** 元は `FsBulletML2.Bullets`
/// （書かれた弾幕と同じアセンブリ）に居たが、**書く道具と、書かれたもの**は
/// 別なので分けた。この namespace を選んだのは呼び方を変えないため ——
/// 弾幕の各ファイルはどれも `open FsBulletML2` しているので、
/// `Dsl.fire` の書き味が分ける前と同じになる。
module Dsl =

  let private expr s = numExpr s

  let private dir typ s =
    Some (Direction (Some { directionType = typ }, expr s))

  let private spd typ s =
    Some (Speed (Some { speedType = typ }, expr s))

  let private spdDefault s = Some (Speed (None, expr s))

  let private emptyBullet =
    BulletElm.Bullet ({ bulletLabel = None }, None, None, [])

  let private wrapAction (label: string option) (cmds: Action list) =
    ActionElm.Action ({ actionLabel = label |> Option.map ActionLabel }, cmds)

  // ---- 命令（action の列に積む値） --------------------------------------

  let wait s = Action.Wait (expr s)
  let vanish = Action.Vanish

  let changeDirection typ s term =
    Action.ChangeDirection (Direction (Some { directionType = typ }, expr s), Term (expr term))

  let changeSpeed typ s term =
    Action.ChangeSpeed (Speed (Some { speedType = typ }, expr s), Term (expr term))

  let fireRef label (ps: string list) =
    Action.FireRef ({ fireRefLabel = FireLabel label }, ps)

  let actionRef label (ps: string list) =
    Action.ActionRef ({ actionRefLabel = ActionLabel label }, ps)

  // ---- action 列 ----------------------------------------------------------
  //
  // `action { wait "1"; vanish }` の型は Action list。
  // finish を差し替えると、同じ書き方で Repeat / 根の Action / 入れ子 Action になる。

  type ActionBuilder<'T>(finish: Action list -> 'T) =
    member _.Yield(x: Action) : Action list = [x]
    member _.YieldFrom(xs: Action list) = xs
    member _.Zero() : Action list = []
    member _.Combine(a: Action list, b: Action list) = a @ b
    member _.Delay(f: unit -> Action list) = f ()
    member _.Run(xs: Action list) : 'T = finish xs

  let action = ActionBuilder id

  /// 入れ子の無名 action（action の子になれる）
  let nest = ActionBuilder (fun xs -> Action.Action ({ actionLabel = None }, xs))

  let nestAs name =
    ActionBuilder (fun xs -> Action.Action ({ actionLabel = Some (ActionLabel name) }, xs))

  /// `repeat "4" { wait "1"; fire { plain } }`
  let repeat times =
    ActionBuilder (fun xs -> Action.Repeat (Times (expr times), wrapAction None xs))

  /// 根の `<action label="top">`
  let top =
    ActionBuilder (fun xs -> BulletmlElm.Action ({ actionLabel = Some (ActionLabel "top") }, xs))

  let defAction name =
    ActionBuilder (fun xs -> BulletmlElm.Action ({ actionLabel = Some (ActionLabel name) }, xs))

  /// bullet の子（action | actionRef）
  let body = ActionBuilder (fun xs -> wrapAction None xs)

  let bodyAs name =
    ActionBuilder (fun xs -> wrapAction (Some name) xs)

  let bodyRef label (ps: string list) =
    ActionElm.ActionRef ({ actionRefLabel = ActionLabel label }, ps)

  // ---- fire（袋。direction? speed? bullet|bulletRef） ----------------------

  type FireSpec =
    { Label: FireLabel option
      Dir: Direction option
      Speed: Speed option
      Body: BulletElm option }

  type FireBuilder<'T>(label: string option, finish: FireAttrs * Direction option * Speed option * BulletElm -> 'T) =
    member _.Zero() : FireSpec =
      { Label = label |> Option.map FireLabel
        Dir = None
        Speed = None
        Body = None }
    member _.Yield(()) = FireBuilder(label, finish).Zero()
    member _.Delay(f: unit -> FireSpec) = f ()
    member _.Run(s: FireSpec) : 'T =
      // DTD は bulletElm 必須。省略したら空の <bullet/> と同じ
      let body = match s.Body with Some b -> b | None -> emptyBullet
      finish ({ fireLabel = s.Label }, s.Dir, s.Speed, body)

    /// type 省略。DTD の既定は aim
    [<CustomOperation("dir")>]
    member _.Dir(s: FireSpec, e) = { s with Dir = Some (Direction (None, expr e)) }
    [<CustomOperation("aim")>]
    member _.Aim(s: FireSpec, e) = { s with Dir = dir DirectionType.Aim e }
    [<CustomOperation("absolute")>]
    member _.Absolute(s: FireSpec, e) = { s with Dir = dir DirectionType.Absolute e }
    [<CustomOperation("relative")>]
    member _.Relative(s: FireSpec, e) = { s with Dir = dir DirectionType.Relative e }
    [<CustomOperation("sequence")>]
    member _.Sequence(s: FireSpec, e) = { s with Dir = dir DirectionType.Sequence e }

    [<CustomOperation("speed")>]
    member _.Speed(s: FireSpec, e) = { s with Speed = spdDefault e }
    [<CustomOperation("speedAbs")>]
    member _.SpeedAbs(s: FireSpec, e) = { s with Speed = spd SpeedType.Absolute e }
    [<CustomOperation("speedRel")>]
    member _.SpeedRel(s: FireSpec, e) = { s with Speed = spd SpeedType.Relative e }
    [<CustomOperation("speedSeq")>]
    member _.SpeedSeq(s: FireSpec, e) = { s with Speed = spd SpeedType.Sequence e }

    /// 空の <bullet/>
    [<CustomOperation("plain")>]
    member _.Plain(s: FireSpec) = { s with Body = Some emptyBullet }

    [<CustomOperation("refBullet")>]
    member _.RefBullet(s: FireSpec, label: string, ps: string list) =
      { s with Body = Some (BulletElm.BulletRef ({ bulletRefLabel = BulletLabel label }, ps)) }

    /// 中身のある bullet を載せる。`ofBullet (bullet "x" { ... })`
    [<CustomOperation("ofBullet")>]
    member _.OfBullet(s: FireSpec, b: BulletElm) = { s with Body = Some b }

  let fire = FireBuilder(None, Action.Fire)
  let fireAs name = FireBuilder(Some name, Action.Fire)
  let topFire = FireBuilder(None, BulletmlElm.Fire)
  let topFireAs name = FireBuilder(Some name, BulletmlElm.Fire)

  // ---- accel（horizontal? vertical? term。term は DTD 必須なので引数） ----

  type AccelSpec =
    { H: Horizontal option
      V: Vertical option }

  type AccelBuilder(term: string) =
    member _.Zero() : AccelSpec = { H = None; V = None }
    member _.Yield(()) = AccelBuilder(term).Zero()
    member _.Delay(f: unit -> AccelSpec) = f ()
    member _.Run(s: AccelSpec) =
      Action.Accel (s.H, s.V, Term (expr term))

    [<CustomOperation("horizontal")>]
    member _.Horizontal(s: AccelSpec, typ, e) =
      { s with H = Some (Horizontal (Some { horizontalType = typ }, expr e)) }
    [<CustomOperation("vertical")>]
    member _.Vertical(s: AccelSpec, typ, e) =
      { s with V = Some (Vertical (Some { verticalType = typ }, expr e)) }

  let accel term = AccelBuilder term

  // ---- bullet（direction? speed? (action|actionRef)*） --------------------
  //
  // fire の CustomOperation 名 `plain` とぶつからないよう、こちらは関数
  // `bullet "label" { ... }`。中の action は `doActs (body { ... })`。
  // Yield と CustomOperation を混ぜると F# が CE の翻訳を別物にするので、
  // 子は custom op に閉じる。

  type BulletSpec =
    { Label: BulletLabel option
      Dir: Direction option
      Speed: Speed option
      Actions: ActionElm list }

  type BulletBuilder<'T>(label: string option, finish: BulletAttrs * Direction option * Speed option * ActionElm list -> 'T) =
    member _.Zero() : BulletSpec =
      { Label = label |> Option.map BulletLabel
        Dir = None
        Speed = None
        Actions = [] }
    member _.Yield(()) = BulletBuilder(label, finish).Zero()
    member _.Delay(f: unit -> BulletSpec) = f ()
    member _.Run(s: BulletSpec) : 'T =
      finish ({ bulletLabel = s.Label }, s.Dir, s.Speed, s.Actions)

    [<CustomOperation("aim")>]
    member _.Aim(s: BulletSpec, e) = { s with Dir = dir DirectionType.Aim e }
    [<CustomOperation("absolute")>]
    member _.Absolute(s: BulletSpec, e) = { s with Dir = dir DirectionType.Absolute e }
    [<CustomOperation("relative")>]
    member _.Relative(s: BulletSpec, e) = { s with Dir = dir DirectionType.Relative e }
    [<CustomOperation("sequence")>]
    member _.Sequence(s: BulletSpec, e) = { s with Dir = dir DirectionType.Sequence e }
    [<CustomOperation("dir")>]
    member _.Dir(s: BulletSpec, typ, e) = { s with Dir = dir typ e }

    [<CustomOperation("speed")>]
    member _.Speed(s: BulletSpec, e) = { s with Speed = spdDefault e }
    [<CustomOperation("speedAbs")>]
    member _.SpeedAbs(s: BulletSpec, e) = { s with Speed = spd SpeedType.Absolute e }
    [<CustomOperation("speedRel")>]
    member _.SpeedRel(s: BulletSpec, e) = { s with Speed = spd SpeedType.Relative e }
    [<CustomOperation("speedSeq")>]
    member _.SpeedSeq(s: BulletSpec, e) = { s with Speed = spd SpeedType.Sequence e }

    [<CustomOperation("doActs")>]
    member _.DoActs(s: BulletSpec, a: ActionElm) = { s with Actions = s.Actions @ [a] }

    [<CustomOperation("refActs")>]
    member _.RefActs(s: BulletSpec, label: string, ps: string list) =
      { s with Actions = s.Actions @ [bodyRef label ps] }

  let bullet name = BulletBuilder(Some name, BulletElm.Bullet)
  let bulletAnon = BulletBuilder(None, BulletElm.Bullet)
  let defBullet name = BulletBuilder(Some name, BulletmlElm.Bullet)
  let defBulletAnon = BulletBuilder(None, BulletmlElm.Bullet)

  let bulletRef label (ps: string list) =
    BulletElm.BulletRef ({ bulletRefLabel = BulletLabel label }, ps)

  // ---- 根 bulletml (action | bullet | fire)* ------------------------------
  //
  // xmlns / description は引数。CE の中は BulletmlElm を積むだけ。
  // 根で wait を Yield しようとすると型が合わない。

  type BulletmlBuilder(typ: ShootingDirection option, name: string option, xmlns: string option, desc: string option) =
    member _.Yield(e: BulletmlElm) : BulletmlElm list = [e]
    member _.YieldFrom(xs: BulletmlElm list) = xs
    member _.Zero() : BulletmlElm list = []
    member _.Combine(a, b) = a @ b
    member _.Delay(f: unit -> BulletmlElm list) = f ()
    member _.Run(elms: BulletmlElm list) =
      Bulletml.Bulletml(
        { bulletmlXmlns = xmlns
          bulletmlType = typ
          bulletmlName = name
          bulletmlDescription = desc },
        elms)

  let vertical name = BulletmlBuilder(Some ShootingDirection.BulletVertical, Some name, None, None)
  let horizontal name = BulletmlBuilder(Some ShootingDirection.BulletHorizontal, Some name, None, None)
  let none name = BulletmlBuilder(Some ShootingDirection.BulletNone, Some name, None, None)

  let verticalXmlns xmlns name =
    BulletmlBuilder(Some ShootingDirection.BulletVertical, Some name, Some xmlns, None)

  /// ビルダが DTD の書く側を全部受けられることの見本。走らせる門ではない。
  module Examples =

    let twoWayLeft =
      vertical "2Way Left" {
          top {
              fire { absolute "-10"; speed "20"; plain }
              fire { absolute "0";  speed "20"; plain }
          }
      }

    let homing =
      horizontal "ホーミング弾" {
          top {
              repeat "2" {
                  fire { dir "(-30+$rand*120)"; refBullet "hmgLsr" [] }
                  repeat "5" {
                      wait "1"
                      fire { sequence "0"; refBullet "hmgLsr" [] }
                  }
                  wait "10"
              }
          }
          defBullet "hmgLsr" {
              speed "2"
              doActs (body {
                  changeSpeed SpeedType.Absolute "0.3" "40"
                  wait "100"
                  changeSpeed SpeedType.Absolute "5" "90"
              })
              doActs (body {
                  repeat "9999" {
                      changeDirection DirectionType.Aim "0" "40-$rank*20"
                      wait "5"
                  }
              })
          }
      }

    let fullSyntax =
      verticalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "full-syntax-demo" {
          defAction "top" {
              wait "10"
              fireAs "firstShot" { absolute "180"; speed "2"; plain }
              fire {
                  aim "0"
                  speedSeq "0.5"
                  refBullet "bit" ["$rank*2"; "1+2"]
              }
              fireRef "firstShot" []
              repeat "4" {
                  fire { sequence "10"; plain }
                  wait "2"
              }
              changeDirection DirectionType.Aim "0" "20-$rank*10"
              changeSpeed SpeedType.Absolute "0.3" "40"
              accel "30" {
                  horizontal HorizontalType.Relative "1"
                  vertical VerticalType.Sequence "0"
              }
              nest { wait "1" }
              actionRef "top" []
              vanish
          }
          topFire { absolute "0"; speed "1"; ofBullet (bullet "plain" { speed "1" }) }
          defBullet "bit" {
              dir DirectionType.Absolute "90"
              speed "0.9"
              doActs (body {
                  wait "$1"
                  fire { relative "0"; plain }
              })
              refActs "top" ["40"]
          }
      }
