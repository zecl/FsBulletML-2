namespace FsBulletML2.Core.Tests

open FsBulletML2
open FsBulletML2.Domain

/// 命令 1 つを、台本まるごとの形で走らせるための試験用の入口。
///
/// Step の各命令は**中身をほどいて受ける**。台本まるごとを受けていた頃は、
/// 受ける側それぞれが「自分の腕でなければ既定値」という届かない match を
/// 持っていて、壊れた木を渡されても落ちずに黙って違うことをしていた。
/// その match を消したので、ほどくのは呼ぶ側の仕事になる。
///
/// **ほどけない組み合わせはここで落とす。** 試験が自分で組んだ台本を
/// 自分で渡しているので、食い違いは試験の書き間違いであり、
/// 既定値で進めてよい場面ではない
[<AutoOpen>]
module internal StepEntry =

  let private wrong name = failwithf "この試験は %s を渡すつもりで別の命令を組んでいます" name

  let stepWait (script: Action) (p: Progress) =
    match script with
    | Action.Wait s -> Step.wait s p
    | _ -> wrong "wait"

  let stepAccel (script: Action) (p: Progress) =
    match script with
    | Action.Accel (h, v, term) -> Step.accel h v term p
    | _ -> wrong "accel"

  let stepChangeDirection (script: Action) (p: Progress) =
    match script with
    | Action.ChangeDirection (dir, term) -> Step.changeDirection dir term p
    | _ -> wrong "changeDirection"

  let stepChangeSpeed (script: Action) (p: Progress) =
    match script with
    | Action.ChangeSpeed (spd, term) -> Step.changeSpeed spd term p
    | _ -> wrong "changeSpeed"

  let stepRepeat rs (script: Action) (p: Progress) (fc: FireContext) =
    match script with
    | Action.Repeat (times, body) -> Step.repeat rs times body p fc
    | _ -> wrong "repeat"

  let stepFire rs (script: Action) (p: Progress) (fc: FireContext) =
    match script with
    | Action.Fire (attrs, d, s, b) -> Step.fire rs attrs d s b p fc
    | _ -> wrong "fire"

  let stepAction rs (script: Action) (p: Progress) (fc: FireContext) =
    match script with
    | Action.Action (attrs, children) -> Step.action rs attrs children p fc
    | _ -> wrong "action"
