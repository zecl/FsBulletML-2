/// 書きかけ の木 から、1 度 も 走らない 枝 を落とす。
/// `Tune` の `DropSplit` と違って 印 を見ない。
///
/// 入口 は 名前 が `top` で始まる `action`（`Api.fs` の `scripts`）。
/// あそこ は `BulletmlOps.getAction` を通す ので、入れ子 の `top2` も 入口。
///
/// 落とす のは 根直下 だけ —— 入れ子 の `<action label>` は 親 から 順 に走る ので、
/// 名前 が浮いて いる ことと その枝 が走らない ことは 別
module FsBulletML2.Prune

open FsBulletML2

/// その要素 が 定義 する 名前。入れ子 も 入る ——
/// `BulletmlOps.getAction` が 隅々 まで 歩く ので、
/// 深い ところ の `label` も `actionRef` から 引ける
let private labelsIn (elm: BulletmlElm) : Set<RefKey> =
  let found = System.Collections.Generic.HashSet<RefKey>()

  let rec inAction (a: Action) =
    match a with
    | Action.Action(attrs, xs) ->
      attrs.actionLabel |> Option.iter (ActionKey >> found.Add >> ignore)
      List.iter inAction xs
    | Action.Repeat(_, e) -> inElm e
    | Action.Fire(attrs, _, _, b) ->
      attrs.fireLabel |> Option.iter (FireKey >> found.Add >> ignore)
      inBullet b
    | _ -> ()

  and inElm (e: ActionElm) =
    match e with
    | ActionElm.Action(attrs, xs) ->
      attrs.actionLabel |> Option.iter (ActionKey >> found.Add >> ignore)
      List.iter inAction xs
    | ActionElm.ActionRef _ -> ()

  and inBullet (b: BulletElm) =
    match b with
    | BulletElm.Bullet(attrs, _, _, xs) ->
      attrs.bulletLabel |> Option.iter (BulletKey >> found.Add >> ignore)
      List.iter inElm xs
    | BulletElm.BulletRef _ -> ()

  match elm with
  | BulletmlElm.Action(attrs, xs) ->
    attrs.actionLabel |> Option.iter (ActionKey >> found.Add >> ignore)
    List.iter inAction xs
  | BulletmlElm.Bullet(attrs, _, _, xs) ->
    attrs.bulletLabel |> Option.iter (BulletKey >> found.Add >> ignore)
    List.iter inElm xs
  | BulletmlElm.Fire(attrs, _, _, b) ->
    attrs.fireLabel |> Option.iter (FireKey >> found.Add >> ignore)
    inBullet b

  Set.ofSeq found

/// その要素 が 引く 名前
let private refsIn (elm: BulletmlElm) : Set<RefKey> =
  let found = System.Collections.Generic.HashSet<RefKey>()

  let rec inAction (a: Action) =
    match a with
    | Action.ActionRef(attrs, _) -> found.Add(ActionKey attrs.actionRefLabel) |> ignore
    | Action.FireRef(attrs, _) -> found.Add(FireKey attrs.fireRefLabel) |> ignore
    | Action.Action(_, xs) -> List.iter inAction xs
    | Action.Repeat(_, e) -> inElm e
    | Action.Fire(_, _, _, b) -> inBullet b
    | _ -> ()

  and inElm (e: ActionElm) =
    match e with
    | ActionElm.Action(_, xs) -> List.iter inAction xs
    | ActionElm.ActionRef(attrs, _) -> found.Add(ActionKey attrs.actionRefLabel) |> ignore

  and inBullet (b: BulletElm) =
    match b with
    | BulletElm.Bullet(_, _, _, xs) -> List.iter inElm xs
    | BulletElm.BulletRef(attrs, _) -> found.Add(BulletKey attrs.bulletRefLabel) |> ignore

  match elm with
  | BulletmlElm.Action(_, xs) -> List.iter inAction xs
  | BulletmlElm.Bullet(_, _, _, xs) -> List.iter inElm xs
  | BulletmlElm.Fire(_, _, _, b) -> inBullet b

  Set.ofSeq found

/// 入口 を 持つ か。名前 が `top` で始まる `action` が 木 の どこ か に在る
let private hasEntry (elm: BulletmlElm) =
  labelsIn elm
  |> Set.exists (fun k ->
      match k with
      | ActionKey l -> (ActionLabel.text l).StartsWith "top"
      | _ -> false)

/// 生き残る 根直下 の要素。
///
/// 入口 から 始めて、引いた 先 を 足して いく。
/// 足した 要素 が また 別 を引く ので、増えなく なる まで 回す ——
/// 1 周 で止める と、入口 から 2 段 離れた 定義 が 落ちる
let private survivors (elms: BulletmlElm list) : BulletmlElm list =
  let defs = elms |> List.map labelsIn
  let uses = elms |> List.map refsIn
  let live = Array.init (List.length elms) (fun i -> hasEntry (List.item i elms))

  let mutable again = true
  while again do
    again <- false
    let wanted =
      Seq.zip live uses
      |> Seq.filter fst
      |> Seq.map snd
      |> Seq.fold Set.union Set.empty
    defs
    |> List.iteri (fun i d ->
        if not live.[i] && not (Set.isEmpty (Set.intersect d wanted)) then
          live.[i] <- true
          again <- true)

  elms |> List.indexed |> List.filter (fun (i, _) -> live.[i]) |> List.map snd

/// 走らない 根直下 の要素 を 落とす
let apply (bulletml: Bulletml) : Bulletml =
  match bulletml with
  | Bulletml(attrs, elms) -> Bulletml(attrs, survivors elms)

/// 落ちる 数。押す 前 に 見せる ため ——
/// 見た目 が 変わらない ので、数 が出ない と 何 が起きた か 分からない
let count (bulletml: Bulletml) : int =
  match bulletml with
  | Bulletml(_, elms) -> List.length elms - List.length (survivors elms)
