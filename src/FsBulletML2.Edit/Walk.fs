/// 編集 の各所 が 同じ 骨組み で 木 を歩く。その 骨組み を 1 本 に置く。
/// `actionRef` / `fireRef` / `bulletRef` の先 は 辿らない。
module internal FsBulletML2.Walk

open FsBulletML2

/// 上 から 順 に 1 度 ずつ 見る。見る だけ で 木 は変えない
type Visit =
    {
        onAction: Action -> unit
        onElm: ActionElm -> unit
        onBullet: BulletElm -> unit
        onTop: BulletmlElm -> unit
    }

let see =
    {
        onAction = ignore
        onElm = ignore
        onBullet = ignore
        onTop = ignore
    }

let iter (v: Visit) (elms: BulletmlElm list) =
    let rec inAction (a: Action) =
        v.onAction a

        match a with
        | Action.Action(_, xs) -> List.iter inAction xs
        | Action.Repeat(_, e) -> inElm e
        | Action.Fire(_, _, _, b) -> inBullet b
        | _ -> ()

    and inElm (e: ActionElm) =
        v.onElm e

        match e with
        | ActionElm.Action(_, xs) -> List.iter inAction xs
        | ActionElm.ActionRef _ -> ()

    and inBullet (b: BulletElm) =
        v.onBullet b

        match b with
        | BulletElm.Bullet(_, _, _, xs) -> List.iter inElm xs
        | BulletElm.BulletRef _ -> ()

    for e in elms do
        v.onTop e

        match e with
        | BulletmlElm.Action(_, xs) -> List.iter inAction xs
        | BulletmlElm.Bullet(_, _, _, xs) -> List.iter inElm xs
        | BulletmlElm.Fire(_, _, _, b) -> inBullet b

/// 子 を 先 に写して、その あと 自分 を写す。
/// 自分 の写し は 子 を見ない 手 だけ を置く —— 見る と 順 で答え が変わる
type Rewrite =
    {
        action: Action -> Action
        elm: ActionElm -> ActionElm
        bullet: BulletElm -> BulletElm
        top: BulletmlElm -> BulletmlElm
    }

let keep =
    {
        action = id
        elm = id
        bullet = id
        top = id
    }

let rec rewriteAction (r: Rewrite) (a: Action) : Action =
    let inner =
        match a with
        | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (rewriteAction r) xs)
        | Action.Repeat(t, e) -> Action.Repeat(t, rewriteElm r e)
        | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, d, s, rewriteBullet r b)
        | _ -> a

    r.action inner

and private rewriteElm (r: Rewrite) (e: ActionElm) : ActionElm =
    let inner =
        match e with
        | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (rewriteAction r) xs)
        | ActionElm.ActionRef _ -> e

    r.elm inner

and private rewriteBullet (r: Rewrite) (b: BulletElm) : BulletElm =
    let inner =
        match b with
        | BulletElm.Bullet(attrs, d, s, xs) -> BulletElm.Bullet(attrs, d, s, List.map (rewriteElm r) xs)
        | BulletElm.BulletRef _ -> b

    r.bullet inner

let private rewriteTop (r: Rewrite) (e: BulletmlElm) : BulletmlElm =
    let inner =
        match e with
        | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (rewriteAction r) xs)
        | BulletmlElm.Bullet(attrs, d, s, xs) -> BulletmlElm.Bullet(attrs, d, s, List.map (rewriteElm r) xs)
        | BulletmlElm.Fire(attrs, d, s, b) -> BulletmlElm.Fire(attrs, d, s, rewriteBullet r b)

    r.top inner

let rewrite (r: Rewrite) (elms: BulletmlElm list) : BulletmlElm list = List.map (rewriteTop r) elms

/// 撃つ 枝 を 1 つ でも 持って いるか。持って いない 弾 が 段 の終点
let rec firesIn (a: Action) =
    match a with
    | Action.Fire _
    | Action.FireRef _ -> true
    | Action.Repeat(_, e) -> firesInElm e
    | Action.Action(_, xs) -> List.exists firesIn xs
    | _ -> false

and firesInElm (e: ActionElm) =
    match e with
    | ActionElm.Action(_, xs) -> List.exists firesIn xs
    | ActionElm.ActionRef _ -> false

/// 名前 が `top` で始まる `action`。走らせる 側 が 並行 に走らせる もの
let isTop (e: BulletmlElm) =
    match e with
    | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> l.StartsWith "top"
    | _ -> false

/// 名前 を落とす。同じ 動き を 何か所 にも 配る ので、
/// label を持った まま だと 同じ 名前 が 何度 も 現れる
let rec unlabel (a: Action) : Action =
    match a with
    | Action.Action(_, xs) -> Action.Action({ actionLabel = None }, List.map unlabel xs)
    | Action.Repeat(t, e) -> Action.Repeat(t, unlabelElm e)
    | _ -> a

and unlabelElm (e: ActionElm) : ActionElm =
    match e with
    | ActionElm.Action(_, xs) -> ActionElm.Action({ actionLabel = None }, List.map unlabel xs)
    | ActionElm.ActionRef _ -> e
