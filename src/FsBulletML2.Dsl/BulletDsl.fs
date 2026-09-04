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

  // changeDirection / changeSpeed の中の direction / speed も、
  // fire の `dir` / `speed` と同じ規則にする ——
  // **type を書かないのが短い名前**で、型ごとに接尾辞。
  //
  // **DTD の既定（direction は aim、speed は absolute）とは別物。**
  // 属性を書かないことを attrs = None で持つので、`changeSpeed "0" "1"` と
  // `changeSpeedAbs "0" "1"` は違う値になる —— XML へ書き戻したときに
  // type 属性が出るかどうかが変わる。

  /// type を書かない changeDirection
  let changeDirection s term =
    Action.ChangeDirection (Direction (None, expr s), Term (expr term))

  let private changeDirOf typ s term =
    Action.ChangeDirection (Direction (Some { directionType = typ }, expr s), Term (expr term))

  let changeDirectionAim s term = changeDirOf DirectionType.Aim s term
  let changeDirectionAbs s term = changeDirOf DirectionType.Absolute s term
  let changeDirectionRel s term = changeDirOf DirectionType.Relative s term
  let changeDirectionSeq s term = changeDirOf DirectionType.Sequence s term

  /// type を書かない changeSpeed
  let changeSpeed s term =
    Action.ChangeSpeed (Speed (None, expr s), Term (expr term))

  let private changeSpdOf typ s term =
    Action.ChangeSpeed (Speed (Some { speedType = typ }, expr s), Term (expr term))

  let changeSpeedAbs s term = changeSpdOf SpeedType.Absolute s term
  let changeSpeedRel s term = changeSpdOf SpeedType.Relative s term
  let changeSpeedSeq s term = changeSpdOf SpeedType.Sequence s term

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
    /// 中身が空の action。**DTD の action は `(...)*` で 0 個 も許す。**
    /// `action { () }` と書く（F# は完全に空の CE を書けない）
    member _.Yield(_: unit) : Action list = []
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

  // DTD の repeat は `(times, (action | actionRef))`。子は 3 通り 書ける ——
  // label の無い action、label のある action、actionRef。

  /// `repeat "4" { wait "1"; fire { plain } }`
  let repeat times =
    ActionBuilder (fun xs -> Action.Repeat (Times (expr times), wrapAction None xs))

  /// 子の action に label を付ける形
  let repeatAs times name =
    ActionBuilder (fun xs -> Action.Repeat (Times (expr times), wrapAction (Some name) xs))

  /// 子が actionRef の形。**CE ではなく関数**（中に積むものが無い）
  let repeatRef times label (ps: string list) =
    Action.Repeat (Times (expr times), ActionElm.ActionRef ({ actionRefLabel = ActionLabel label }, ps))

  /// 根の `<action label="top">`
  let top =
    ActionBuilder (fun xs -> BulletmlElm.Action ({ actionLabel = Some (ActionLabel "top") }, xs))

  let defAction name =
    ActionBuilder (fun xs -> BulletmlElm.Action ({ actionLabel = Some (ActionLabel name) }, xs))

  /// 根の action に label を書かない形。**DTD では label は #IMPLIED**
  /// （エンジンからは名前で引けなくなるので、実際に使うことは少ない）
  let defActionAnon =
    ActionBuilder (fun xs -> BulletmlElm.Action ({ actionLabel = None }, xs))

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

    // horizontal / vertical も type 省略が短い名前（fire の speed と同じ規則）

    [<CustomOperation("horizontal")>]
    member _.Horizontal(s: AccelSpec, e) =
      { s with H = Some (Horizontal (None, expr e)) }
    [<CustomOperation("horizontalAbs")>]
    member _.HorizontalAbs(s: AccelSpec, e) =
      { s with H = Some (Horizontal (Some { horizontalType = HorizontalType.Absolute }, expr e)) }
    [<CustomOperation("horizontalRel")>]
    member _.HorizontalRel(s: AccelSpec, e) =
      { s with H = Some (Horizontal (Some { horizontalType = HorizontalType.Relative }, expr e)) }
    [<CustomOperation("horizontalSeq")>]
    member _.HorizontalSeq(s: AccelSpec, e) =
      { s with H = Some (Horizontal (Some { horizontalType = HorizontalType.Sequence }, expr e)) }

    [<CustomOperation("vertical")>]
    member _.Vertical(s: AccelSpec, e) =
      { s with V = Some (Vertical (None, expr e)) }
    [<CustomOperation("verticalAbs")>]
    member _.VerticalAbs(s: AccelSpec, e) =
      { s with V = Some (Vertical (Some { verticalType = VerticalType.Absolute }, expr e)) }
    [<CustomOperation("verticalRel")>]
    member _.VerticalRel(s: AccelSpec, e) =
      { s with V = Some (Vertical (Some { verticalType = VerticalType.Relative }, expr e)) }
    [<CustomOperation("verticalSeq")>]
    member _.VerticalSeq(s: AccelSpec, e) =
      { s with V = Some (Vertical (Some { verticalType = VerticalType.Sequence }, expr e)) }

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
    /// type 省略。**fire の `dir` と同じ**（型を書くなら aim / absolute /
    /// relative / sequence を使う）
    [<CustomOperation("dir")>]
    member _.Dir(s: BulletSpec, e) = { s with Dir = Some (Direction (None, expr e)) }

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
    /// 中身が空の bulletml。**DTD の bulletml は `(bullet|fire|action)*` で
    /// 0 個 も許す。** `vertical "x" { () }` と書く
    member _.Yield(_: unit) : BulletmlElm list = []
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

  /// 属性を直に渡す一般形。**短い入口で足りない組み合わせはこれで書く。**
  ///
  /// DTD の bulletml は xmlns も type も #IMPLIED で、name はこのエンジンが
  /// 足した属性（description も）。どれも省けるので組み合わせは 4 x 2 x 2 x 2。
  /// **全部 に名前は付けない** —— よく使う形だけ下に短い入口を置いて、
  /// 残りはここを通す。
  let bulletmlOf typ name xmlns desc = BulletmlBuilder(typ, name, xmlns, desc)

  let vertical name = BulletmlBuilder(Some ShootingDirection.BulletVertical, Some name, None, None)
  let horizontal name = BulletmlBuilder(Some ShootingDirection.BulletHorizontal, Some name, None, None)
  let none name = BulletmlBuilder(Some ShootingDirection.BulletNone, Some name, None, None)

  /// type 属性を書かない bulletml。**DTD の既定は none だが、値としては別物**
  /// （書かないことを None で持つので、往復で type 属性が出なくなる）
  let untyped name = BulletmlBuilder(None, Some name, None, None)

  let verticalXmlns xmlns name =
    BulletmlBuilder(Some ShootingDirection.BulletVertical, Some name, Some xmlns, None)

  let horizontalXmlns xmlns name =
    BulletmlBuilder(Some ShootingDirection.BulletHorizontal, Some name, Some xmlns, None)

  let noneXmlns xmlns name =
    BulletmlBuilder(Some ShootingDirection.BulletNone, Some name, Some xmlns, None)

  let untypedXmlns xmlns name = BulletmlBuilder(None, Some name, Some xmlns, None)

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
                  changeSpeedAbs "0.3" "40"
                  wait "100"
                  changeSpeedAbs "5" "90"
              })
              doActs (body {
                  repeat "9999" {
                      changeDirectionAim "0" "40-$rank*20"
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
              changeDirectionAim "0" "20-$rank*10"
              changeSpeedAbs "0.3" "40"
              accel "30" {
                  horizontalRel "1"
                  verticalSeq "0"
              }
              nest { wait "1" }
              actionRef "top" []
              vanish
          }
          topFire { absolute "0"; speed "1"; ofBullet (bullet "plain" { speed "1" }) }
          defBullet "bit" {
              absolute "90"
              speed "0.9"
              doActs (body {
                  wait "$1"
                  fire { relative "0"; plain }
              })
              refActs "top" ["40"]
          }
      }

    /// **type 属性を書かない側**の見本。`fullSyntax` が書いていない腕を通す。
    ///
    /// DTD では direction / speed / horizontal / vertical のどれも type を
    /// 省ける。省いた形は既定値（direction は aim、speed 系は absolute）と
    /// **同じ意味だが別の値** —— 書き戻したときに type 属性が出ない。
    let omittedTypes =
      untyped "type を書かない形" {
          defActionAnon {
              changeDirection "0" "10"
              changeSpeed "1" "10"
              accel "30" {
                  horizontal "1"
                  vertical "0"
              }
              fire { dir "0"; speed "1"; plain }
              repeatAs "3" "namedBody" {
                  wait "1"
              }
              repeatRef "2" "top" ["$1"]
          }
          defBulletAnon {
              dir "90"
              speed "1"
              refActs "top" []
          }
      }

    /// 属性の組み合わせ側。**短い入口で足りないものは一般形で書く。**
    let attributeShapes =
      [ untypedXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "type なし + xmlns" {
            top { vanish }
        }
        horizontalXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "horizontal + xmlns" {
            top { vanish }
        }
        noneXmlns "http://www.asahi-net.or.jp/~cs8k-cyu/bulletml" "none + xmlns" {
            top { vanish }
        }
        // name を書かない形と description を入れる形は、短い入口を置いていない
        bulletmlOf (Some ShootingDirection.BulletVertical) None None None {
            top { vanish }
        }
        bulletmlOf None (Some "説明つき") None (Some "description は BulletML 公式の属性ではない") {
            top { vanish }
        } ]

    /// direction / speed / horizontal / vertical の **型を全通り**書く見本。
    ///
    /// DTD ではこの 4 つ がどれも type を省ける。省いた形は既定値
    /// （direction は aim、他は absolute）と同じ意味だが **別の値** ——
    /// 書き戻したときに type 属性が出ない。だから省略も 1 通り として数える。
    ///
    ///   direction   省略 / aim / absolute / relative / sequence   5 通り
    ///   speed       省略 / absolute / relative / sequence         4 通り
    ///   horizontal  省略 / absolute / relative / sequence         4 通り
    ///   vertical    省略 / absolute / relative / sequence         4 通り
    let allTypeVariants =
      vertical "型の全通り" {
          top {
              fire { dir "0"; plain }
              fire { aim "0"; plain }
              fire { absolute "0"; plain }
              fire { relative "0"; plain }
              fire { sequence "0"; plain }

              fire { speed "1"; plain }
              fire { speedAbs "1"; plain }
              fire { speedRel "1"; plain }
              fire { speedSeq "1"; plain }

              changeDirection "0" "1"
              changeDirectionAim "0" "1"
              changeDirectionAbs "0" "1"
              changeDirectionRel "0" "1"
              changeDirectionSeq "0" "1"

              changeSpeed "1" "1"
              changeSpeedAbs "1" "1"
              changeSpeedRel "1" "1"
              changeSpeedSeq "1" "1"

              accel "1" { horizontal "1" }
              accel "1" { horizontalAbs "1" }
              accel "1" { horizontalRel "1" }
              accel "1" { horizontalSeq "1" }

              accel "1" { vertical "1" }
              accel "1" { verticalAbs "1" }
              accel "1" { verticalRel "1" }
              accel "1" { verticalSeq "1" }

              // 参照を作る関数（custom operation ではないほう）
              fire { ofBullet (bulletRef "b" ["1"]) }
          }
          defBullet "b" {
              doActs (bodyRef "top" [])
          }
      }

    /// 中身が空の形。**DTD はどれも 0 個 を許す** ——
    /// bulletml の `(bullet|fire|action)*`、action の `(...)*`、
    /// bullet の `(action|actionRef)*`、accel の `horizontal? vertical?`。
    ///
    /// **F# は完全に空の CE を書けない**ので `{ () }` と置く。
    /// 実用の弾幕には出ないが、DTD が許す以上 CE でも書けなければならない。
    let emptyShapes =
      [ vertical "中身が空" { () }
        vertical "空の action" {
            top { () }
        }
        vertical "空の bullet と accel" {
            top {
                // 中身の無い bullet（fire の子）
                fire { ofBullet (bullet "からっぽ" { () }) }
                // horizontal も vertical も無い accel（term だけ）
                accel "30" { () }
                // direction も speed も bullet も書かない fire
                // （DTD は bulletElm 必須なので、空の <bullet/> が入る）
                fire { () }
            }
        } ]

    /// fire の子と bullet の中身。**`fullSyntax` が通っていない腕**を埋める
    let bulletShapes =
      vertical "bullet の形" {
          top {
              // 中身のある無名 bullet
              fire { ofBullet (bulletAnon { speed "1"; refActs "top" [] }) }
              // 名前つき fire と、名前つきの入れ子 action
              fireAs "named" { plain }
              nestAs "namedNest" { wait "1" }
          }
          // 名前つきの action を bullet の子に置く
          defBullet "labelled" {
              doActs (bodyAs "inner" { wait "1" })
          }
          topFireAs "topNamed" { plain }
      }
