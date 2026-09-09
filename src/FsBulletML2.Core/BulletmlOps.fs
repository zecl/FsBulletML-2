namespace FsBulletML2

/// BulletML の木の上の操作。**BulletmlRead から切り出したもの。**
///
/// 3 つ ある。
///
///     集める / 探す      collect / getAction / getFire / getBullet / tryFind*
///     param を差し込む   substCommand / refAction / refFire / refBullet
///     輪を 1 段 解く     resolveActionRef / expand* / convertRef*
///
/// **二重木を畳んでも、ここは丸ごと残った。** 木が 1 つ になっても
/// 「名前で引く」「実引数を入れる」「輪を 1 段 だけ解く」は要る。
module internal BulletmlOps =

  let internal convertDirectionOption  = fun prams -> function
    | Some (Direction(attrs,s)) -> Direction(attrs, Param.replaceIn prams s) |> Some
    | None -> None

  let internal convertDirection  = fun prams -> function Direction(attrs,s) -> Direction(attrs, Param.replaceIn prams s) 

  let internal convertSpeedOption = fun prams -> function
    | Some (Speed(attrs,s)) -> Speed(attrs, Param.replaceIn prams s) |> Some
    | None -> None

  let internal convertSpeed = fun prams -> function Speed(attrs,s) -> Speed(attrs, Param.replaceIn prams s) 
  let internal convertTerm = fun prams -> function Term(s) -> Term(Param.replaceIn prams s)
  let internal convertTimes = fun prams -> function | Times(s) -> Times(Param.replaceIn prams s)
  let internal convertParam = fun prams -> List.map (fun s -> Param.replace s prams) 
  let internal convertWait = fun prams -> function | s -> Param.replaceIn prams s

  let internal convertHorizontalOption = fun prams -> function 
    | Some(Horizontal(attrs,s)) -> Horizontal(attrs, Param.replaceIn prams s) |> Some
    | None -> None
  let internal convertVerticalOption = fun prams -> function 
    | Some(Vertical(attrs,s)) -> Vertical(attrs, Param.replaceIn prams s) |> Some
    | None -> None

  /// 木を隅々まで歩いて、名前の付いた要素を集める。
  ///
  /// 以前は同じ形の走査を getAction / getFire / getBullet で 3 回 書いていた
  /// （どれも 14 の腕を並べ、拾う 1 腕だけが違った）。位置ごとに型が
  /// 分かれたので、歩き方を 1 本 にして「拾うもの」だけを差し替える。
  ///
  /// 拾う順は変えていない —— 自分を先に入れてから子へ降りる
  let private collect
      (fromAction: obj -> ActionAttrs * Action list -> 'a list)
      (fromFire: FireAttrs * Direction option * Speed option * BulletElm -> 'a list)
      (fromBullet: BulletAttrs * Direction option * Speed option * ActionElm list -> 'a list)
      (root: Bulletml) : 'a list =
    let rec command (c: Action) =
      match c with
      | Action.Action (attrs, children) ->
        fromAction (box c) (attrs, children) @ (children |> List.collect command)
      | Action.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | Action.Repeat (_, child) -> actionElm child
      // 要素の子を持たない腕。以前はここに NotCommand と、命令の位置には
      // 来られない Bulletml / Bullet / BulletRef も並んでいた
      | Action.ActionRef _ | Action.FireRef _
      | Action.ChangeDirection _ | Action.ChangeSpeed _
      | Action.Accel _ | Action.Wait _ | Action.Vanish -> []
    and actionElm (a: ActionElm) =
      match a with
      | ActionElm.Action (attrs, children) ->
        fromAction (box a) (attrs, children) @ (children |> List.collect command)
      | ActionElm.ActionRef _ -> []
    and bulletElm (b: BulletElm) =
      match b with
      | BulletElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | BulletElm.BulletRef _ -> []
    let topElm (t: BulletmlElm) =
      match t with
      | BulletmlElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | BulletmlElm.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | BulletmlElm.Action (attrs, children) ->
        fromAction (box t) (attrs, children) @ (children |> List.collect command)
    match root with
    | Bulletml.Bulletml (_, elms) -> elms |> List.collect topElm

  /// 名前の付いた action。actionRef が指す先になれるので ActionElm で返す
  let internal getAction (bulletml: Bulletml) : ActionElm list =
    bulletml |> collect
      (fun src (attrs, children) ->
        match attrs.actionLabel with
        | Some _ ->
            // **ここで新しい ActionElm ができる。** 元は 3 通り（BulletmlElm.Action /
            // ActionElm.Action / Action.Action）で、どれも collect が分解して渡す
            let e = ActionElm.Action (attrs, children)
            if NodeOrigin.enabled && not (obj.ReferenceEquals(box e, src)) then
              NodeOrigin.pair (box e) src
            [ e ]
        | None -> [])
      (fun _ -> [])
      (fun _ -> [])

  let internal tryFindAction bulletml (targetLabel: ActionLabel) =
    getAction bulletml |> List.tryFind (function
      | ActionElm.Action (attrs, _) ->
        // 以前は tryFindLabelValue [("label", v)] を通していたが、
        // 1 要素の連想リストから同じキーを引くだけで、常に Some v を返す
        // 空回りだった。型が付いたのでそのまま比べる
        (match attrs.actionLabel with Some v -> v = targetLabel | None -> false)
      | ActionElm.ActionRef _ -> false)

  /// 名前の付いた fire。fireRef が指す先は命令の位置へ差し込まれるので
  /// Action.Fire で返す（根の直下にある fire も同じ形にして返す）
  let internal getFire (bulletml: Bulletml) : Action list =
    bulletml |> collect
      (fun _ _ -> [])
      (fun (attrs, d, s, child) ->
        match attrs.fireLabel with
        | Some _ -> [ Action.Fire (attrs, d, s, child) ]
        | None -> [])
      (fun _ -> [])

  let internal tryFindFire bulletml (targetLabel: FireLabel) =
    getFire bulletml |> List.tryFind (function
      | Action.Fire (attrs, _, _, _) ->
        (match attrs.fireLabel with Some v -> v = targetLabel | None -> false)
      | _ -> false)

  /// 名前の付いた bullet。bulletRef が指す先になれるので BulletElm で返す
  let internal getBullet (bulletml: Bulletml) : BulletElm list =
    bulletml |> collect
      (fun _ _ -> [])
      (fun _ -> [])
      (fun (attrs, d, s, children) ->
        match attrs.bulletLabel with
        | Some _ -> [ BulletElm.Bullet (attrs, d, s, children) ]
        | None -> [])

  let internal tryFindBullet bulletml (targetLabel: BulletLabel) =
    getBullet bulletml |> List.tryFind (function
      | BulletElm.Bullet (attrs, _, _, _) ->
        (match attrs.bulletLabel with Some v -> v = targetLabel | None -> false)
      | BulletElm.BulletRef _ -> false)

  /// 実引数を差し込む走査。位置ごとに分ける。
  ///
  /// 以前は 1 本の convert が平らな DU を歩き、最後に `| x -> x` で
  /// 「触らない腕」をまとめて受けていた。その `x` には
  /// Vanish（触らなくてよい）と Bulletml / 当時あった NotCommand（そもそも
  /// ここへ来ない）が混ざっていた
  let rec private substCommandCore prams (c: Action) : Action =
    match c with
    | Action.ChangeDirection (direction, term) ->
      Action.ChangeDirection (convertDirection prams direction, convertTerm prams term)
    | Action.ChangeSpeed (speed, term) ->
      Action.ChangeSpeed (convertSpeed prams speed, convertTerm prams term)
    | Action.Accel (horizontal, vertical, term) ->
      Action.Accel (convertHorizontalOption prams horizontal,
                        convertVerticalOption prams vertical, convertTerm prams term)
    | Action.Wait s -> Action.Wait (convertWait prams s)
    | Action.Vanish -> Action.Vanish
    | Action.Repeat (times, child) ->
      Action.Repeat (convertTimes prams times, substActionElm prams child)
    | Action.Fire (attrs, direction, speed, child) ->
      Action.Fire (attrs, convertDirectionOption prams direction,
                       convertSpeedOption prams speed, substBulletElm prams child)
    | Action.FireRef (attrs, param) -> Action.FireRef (attrs, convertParam prams param)
    | Action.Action (attrs, children) ->
      Action.Action (attrs, children |> List.map (substCommand prams))
    | Action.ActionRef (attrs, param) -> Action.ActionRef (attrs, convertParam prams param)

  and private substActionElmCore prams (a: ActionElm) : ActionElm =
    match a with
    | ActionElm.Action (attrs, children) ->
      ActionElm.Action (attrs, children |> List.map (substCommand prams))
    | ActionElm.ActionRef (attrs, param) -> ActionElm.ActionRef (attrs, convertParam prams param)

  and private substBulletElm prams (b: BulletElm) : BulletElm =
    match b with
    | BulletElm.Bullet (attrs, direction, speed, children) ->
      BulletElm.Bullet (attrs, convertDirectionOption prams direction,
                           convertSpeedOption prams speed,
                           children |> List.map (substActionElm prams))
    | BulletElm.BulletRef (attrs, param) -> BulletElm.BulletRef (attrs, convertParam prams param)

  // --- param を差し込んで作った物を、元の物と対にする -------------------------
  //
  // **覆いで、中身（*Core）は 1 行 も触っていない。**
  // **同じ物が返ったときは対にしない** —— vanish の腕は引数なしなので
  // singleton で、作り直しても同じ物が返る（同梱 176 本 で 265 件）。

  and private substCommand prams (c: Action) : Action =
    let r = substCommandCore prams c
    if NodeOrigin.enabled && not (obj.ReferenceEquals(r, c)) then NodeOrigin.pair (box r) (box c)
    r

  and private substActionElm prams (a: ActionElm) : ActionElm =
    let r = substActionElmCore prams a
    if NodeOrigin.enabled && not (obj.ReferenceEquals(r, a)) then NodeOrigin.pair (box r) (box a)
    r

  /// 参照先の要素へ実引数を差し込む。**種別ごとに 1 本 ずつ。**
  ///
  /// 以前は target を 1 つの平らな型で受け、target と label の種別が
  /// 揃っていることを型で言えなかった（前の段で RefKey を入れて
  /// 「揃っている腕だけ」を書ける形にしたが、まだ 1 本 の関数だった）。
  /// 位置ごとに型が分かれたので、関数そのものが 3 本 に割れて、
  /// 揃わない呼び方が書けなくなる。
  ///
  /// 名前の一致を確かめてから差し込むのは以前と同じ。呼ぶ側は
  /// tryFind* が返したものを渡すので必ず一致するが、確認は残す
  let internal refAction (target: ActionElm) (label: ActionLabel) prams : ActionElm =
    let prams = prams |> Param.ofList
    match target with
    | ActionElm.Action (attrs, _) when attrs.actionLabel = Some label ->
      substActionElm prams target
    | _ -> target

  let internal refFire (target: Action) (label: FireLabel) prams : Action =
    let prams = prams |> Param.ofList
    match target with
    | Action.Fire (attrs, _, _, _) when attrs.fireLabel = Some label ->
      substCommand prams target
    | _ -> target

  let internal refBullet (target: BulletElm) (label: BulletLabel) prams : BulletElm =
    let prams = prams |> Param.ofList
    match target with
    | BulletElm.Bullet (attrs, _, _, _) when attrs.bulletLabel = Some label ->
      substBulletElm prams target
    | _ -> target

  /// 展開中の参照は DTD.RefKey が表す（action:foo と bullet:foo は別物）。
  /// 以前はここに refKey kind label = kind + ":" + label があり、種別を
  /// 文字で足していた。型にしたので、足し忘れも綴り違いも起きない

  /// 参照を解いて木へ展開する。
  ///
  /// lastAction は「直近に展開した action の label」。
  /// action の輪を残してよいのは、その輪が直近に展開した action 自身へ戻るときだけ。
  /// 別の action を経由する輪は、解いた結果の中に action が挟まるので、
  /// 走らせる側が 1 段ずつ解くと呼び出しがフレームごとに深くなる。
  ///
  /// 位置ごとに関数が分かれた。actionRef は「命令の位置」と
  /// 「repeat / bullet の子の位置」の両方に出るので、解く判断だけを
  /// resolveActionRef に出して両方から使う
  let rec private resolveActionRef visiting lastAction top (attrs: ActionRefAttrs) prams
      : ActionElm option =
    // None は「輪なので、そのまま残す」
    let key = ActionKey attrs.actionRefLabel
    if Set.contains key visiting then
      if lastAction = Some attrs.actionRefLabel then None
      else
        new BulletmlDTDViolationException(
              sprintf "circular reference detected:[%s] 参照が輪になっているため展開できません" (RefKey.text key)) |> raise
    else
      match tryFindAction top attrs.actionRefLabel with
      | Some action ->
        let newAction = refAction action attrs.actionRefLabel prams
        Some (expandActionElm (Set.add key visiting) (Some attrs.actionRefLabel) top newAction)
      | None ->
        new BulletmlDTDViolationException(
              sprintf "not found target Action element:%s" (ActionLabel.text attrs.actionRefLabel)) |> raise

  and private expandCommandCore visiting lastAction top (c: Action) : Action =
    match c with
    | Action.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> c
      // 解いた結果は action。ActionRef が居たのは命令の位置なので、命令として置く
      | Some (ActionElm.Action (a, cs)) -> Action.Action (a, cs)
      | Some (ActionElm.ActionRef (a, p)) -> Action.ActionRef (a, p)
    | Action.FireRef (attrs, prams) ->
      let key = FireKey attrs.fireRefLabel
      if Set.contains key visiting then
        new BulletmlDTDViolationException(
              sprintf "circular reference detected:[%s] 参照が輪になっているため展開できません" (RefKey.text key)) |> raise
      let visiting = Set.add key visiting
      match tryFindFire top attrs.fireRefLabel with
      | Some fire ->
        let newFire = refFire fire attrs.fireRefLabel prams
        expandCommand visiting None top newFire
      | None ->
        new BulletmlDTDViolationException(
              sprintf "not found target Fire element:%s" (FireLabel.text attrs.fireRefLabel)) |> raise
    | Action.Action (attrs, children) ->
      Action.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | Action.Fire (attrs, d, s, child) ->
      Action.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | Action.Repeat (times, child) ->
      Action.Repeat (times, expandActionElm visiting lastAction top child)
    | Action.ChangeDirection _ | Action.ChangeSpeed _
    | Action.Accel _ | Action.Wait _ | Action.Vanish -> c

  and private expandActionElmCore visiting lastAction top (a: ActionElm) : ActionElm =
    match a with
    | ActionElm.Action (attrs, children) ->
      ActionElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | ActionElm.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> a
      | Some expanded -> expanded


  // --- 参照の解決で作った物を、元の物と対にする -----------------------------
  //
  // **覆いで、中身（*Core）は 1 行 も触っていない。**
  // **同じ物が返ったときは対にしない** —— expandCommandCore の `| None -> c`
  // （輪はそのまま残す）が該当する。

  and private expandCommand visiting lastAction top (c: Action) : Action =
    let r = expandCommandCore visiting lastAction top c
    if NodeOrigin.enabled && not (obj.ReferenceEquals(r, c)) then NodeOrigin.pair (box r) (box c)
    r

  and private expandActionElm visiting lastAction top (a: ActionElm) : ActionElm =
    let r = expandActionElmCore visiting lastAction top a
    if NodeOrigin.enabled && not (obj.ReferenceEquals(r, a)) then NodeOrigin.pair (box r) (box a)
    r
  and private expandBulletElm visiting lastAction top (b: BulletElm) : BulletElm =
    match b with
    | BulletElm.Bullet (attrs, d, s, children) ->
      BulletElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | BulletElm.BulletRef (attrs, prams) ->
      let key = BulletKey attrs.bulletRefLabel
      if Set.contains key visiting then
        // 輪。展開せず残し、走らせる側が 1 段ずつ解く
        b
      else
        match tryFindBullet top attrs.bulletRefLabel with
        | Some bullet ->
          let newBullet = refBullet bullet attrs.bulletRefLabel prams
          expandBulletElm (Set.add key visiting) None top newBullet
        | None ->
          new BulletmlDTDViolationException(
                sprintf "not foun target Bullet element:%s" (BulletLabel.text attrs.bulletRefLabel)) |> raise

  let private expandTopElm visiting lastAction top (t: BulletmlElm) : BulletmlElm =
    match t with
    | BulletmlElm.Bullet (attrs, d, s, children) ->
      BulletmlElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | BulletmlElm.Fire (attrs, d, s, child) ->
      BulletmlElm.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | BulletmlElm.Action (attrs, children) ->
      BulletmlElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))

  /// 木を丸ごと展開する。根から入る唯一の入口
  let internal convertRefBulletml (top: Bulletml) (bulletml: Bulletml) : Bulletml =
    match bulletml with
    | Bulletml.Bulletml (attrs, elms) ->
      Bulletml.Bulletml (attrs, elms |> List.map (expandTopElm Set.empty None top))

  /// top* の台本 1 本 を展開する。`Runner.load` が使う
  /// （旧は落とした `BulletRunner.buildRootTops` の側から呼ばれていた）
  let internal convertRefActionElm (top: Bulletml) (a: ActionElm) : ActionElm =
    expandActionElm Set.empty None top a

  /// 輪のために展開を止めた bulletRef を、走らせる側から 1 段だけ解く。
  /// 中にまた同じ参照が残るので、次に撃たれたときに次の 1 段が解かれる。
  ///
  /// 解く前から自分の key を visiting に入れておくこと。空から始めると
  /// 解いた中身の同じ参照がもう 1 段 展開され、1 段のつもりが 2 段になる。
  /// 新経路（Step.Resolvers）は木を組まないのでこちらを直に使う
  let internal expandBulletRefOnce top (label: BulletLabel) prams : BulletElm option =
    match tryFindBullet top label with
    | Some bullet ->
      // param は文字のまま渡す（Params は string list で、Param.replace も
      // 文字の置き換えなので、ここで数へ潰すと $rank / $rand が凍る）
      refBullet bullet label prams
      |> expandBulletElm (Set.singleton (BulletKey label)) None top
      |> Some
    | None -> None

  /// 輪のために展開を止めた actionRef を、走らせる側から 1 段だけ解く。
  /// 中にまた同じ参照が残るので、そこへ届いたときに次の 1 段が解かれる。
  ///
  /// bulletRef と違って fire を挟まないので、2 段 解くと走らせる側の
  /// 呼び出しが 1 フレームごとに深くなり、スタックを使い切る
  let internal expandActionRefOnce top (label: ActionLabel) prams : ActionElm option =
    match tryFindAction top label with
    | Some action ->
      refAction action label prams
      |> expandActionElm (Set.singleton (ActionKey label)) (Some label) top
      |> Some
    | None -> None
