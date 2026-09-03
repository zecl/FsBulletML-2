namespace FsBulletML2
open FsBulletML2.DTD
// 役目 2（公開 → Rec* の変換）の小さい関数を、param の差し込みでも使う。
// module をまたぐので開いておく —— **呼び出しの字は 1 文字 も変えていない**
open FsBulletML2.IntermediateParser

/// 走らせる木（Rec*）の上の操作。**IntermediateParser から切り出したもので、
/// 中身は動かしていない。**
///
/// 3 つ ある。
///
///     集める / 探す      collect / getAction / getFire / getBullet / tryFind*
///     param を差し込む   substCommand / refAction / refFire / refBullet
///     輪を 1 段 解く     resolveActionRef / expand* / convertRef*
///
/// **二重木を畳んでも、ここは丸ごと残る。** 木が 1 つ になっても
/// 「名前で引く」「実引数を入れる」「輪を 1 段 だけ解く」は要る。
///
/// 切り出す前は 1 ファイル 1,054 行 で、この 3 つ が
/// 「XML を読む」「公開 → Rec へ写す」と同じ場所に並んでいた。
module internal RecOps =

  /// 木を隅々まで歩いて、名前の付いた要素を集める。
  ///
  /// 以前は同じ形の走査を getAction / getFire / getBullet で 3 回 書いていた
  /// （どれも 14 の腕を並べ、拾う 1 腕だけが違った）。位置ごとに型が
  /// 分かれたので、歩き方を 1 本 にして「拾うもの」だけを差し替える。
  ///
  /// 拾う順は変えていない —— 自分を先に入れてから子へ降りる
  let private collect
      (fromAction: ActionAttrs * RecCommand list -> 'a list)
      (fromFire: FireAttrs * Direction option * Speed option * RecBulletElm -> 'a list)
      (fromBullet: BulletAttrs * Direction option * Speed option * RecActionElm list -> 'a list)
      (root: RecBulletml) : 'a list =
    let rec command (c: RecCommand) =
      match c with
      | RecCommand.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
      | RecCommand.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | RecCommand.Repeat (_, child) -> actionElm child
      // 要素の子を持たない腕。以前はここに NotCommand と、命令の位置には
      // 来られない Bulletml / Bullet / BulletRef も並んでいた
      | RecCommand.ActionRef _ | RecCommand.FireRef _
      | RecCommand.ChangeDirection _ | RecCommand.ChangeSpeed _
      | RecCommand.Accel _ | RecCommand.Wait _ | RecCommand.Vanish -> []
    and actionElm (a: RecActionElm) =
      match a with
      | RecActionElm.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
      | RecActionElm.ActionRef _ -> []
    and bulletElm (b: RecBulletElm) =
      match b with
      | RecBulletElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | RecBulletElm.BulletRef _ -> []
    let topElm (t: RecTopElm) =
      match t with
      | RecTopElm.Bullet (attrs, d, s, children) ->
        fromBullet (attrs, d, s, children) @ (children |> List.collect actionElm)
      | RecTopElm.Fire (attrs, d, s, child) ->
        fromFire (attrs, d, s, child) @ bulletElm child
      | RecTopElm.Action (attrs, children) ->
        fromAction (attrs, children) @ (children |> List.collect command)
    match root with
    | RecBulletml.Bulletml (_, elms) -> elms |> List.collect topElm

  /// 名前の付いた action。actionRef が指す先になれるので RecActionElm で返す
  let internal getAction (recBulletml: RecBulletml) : RecActionElm list =
    recBulletml |> collect
      (fun (attrs, children) ->
        match attrs.actionLabel with
        | Some _ -> [ RecActionElm.Action (attrs, children) ]
        | None -> [])
      (fun _ -> [])
      (fun _ -> [])

  let internal tryFindAction recBulletml (targetLabel: ActionLabel) =
    getAction recBulletml |> List.tryFind (function
      | RecActionElm.Action (attrs, _) ->
        // 以前は tryFindLabelValue [("label", v)] を通していたが、
        // 1 要素の連想リストから同じキーを引くだけで、常に Some v を返す
        // 空回りだった。型が付いたのでそのまま比べる
        (match attrs.actionLabel with Some v -> v = targetLabel | None -> false)
      | RecActionElm.ActionRef _ -> false)

  /// 名前の付いた fire。fireRef が指す先は命令の位置へ差し込まれるので
  /// RecCommand.Fire で返す（根の直下にある fire も同じ形にして返す）
  let internal getFire (recBulletml: RecBulletml) : RecCommand list =
    recBulletml |> collect
      (fun _ -> [])
      (fun (attrs, d, s, child) ->
        match attrs.fireLabel with
        | Some _ -> [ RecCommand.Fire (attrs, d, s, child) ]
        | None -> [])
      (fun _ -> [])

  let internal tryFindFire recBulletml (targetLabel: FireLabel) =
    getFire recBulletml |> List.tryFind (function
      | RecCommand.Fire (attrs, _, _, _) ->
        (match attrs.fireLabel with Some v -> v = targetLabel | None -> false)
      | _ -> false)

  /// 名前の付いた bullet。bulletRef が指す先になれるので RecBulletElm で返す
  let internal getBullet (recBulletml: RecBulletml) : RecBulletElm list =
    recBulletml |> collect
      (fun _ -> [])
      (fun _ -> [])
      (fun (attrs, d, s, children) ->
        match attrs.bulletLabel with
        | Some _ -> [ RecBulletElm.Bullet (attrs, d, s, children) ]
        | None -> [])

  let internal tryFindBullet recBulletml (targetLabel: BulletLabel) =
    getBullet recBulletml |> List.tryFind (function
      | RecBulletElm.Bullet (attrs, _, _, _) ->
        (match attrs.bulletLabel with Some v -> v = targetLabel | None -> false)
      | RecBulletElm.BulletRef _ -> false)

  /// 実引数を差し込む走査。位置ごとに分ける。
  ///
  /// 以前は 1 本の convert が平らな DU を歩き、最後に `| x -> x` で
  /// 「触らない腕」をまとめて受けていた。その `x` には
  /// Vanish（触らなくてよい）と Bulletml / NotCommand（そもそも
  /// ここへ来ない）が混ざっていた
  let rec private substCommand prams (c: RecCommand) : RecCommand =
    match c with
    | RecCommand.ChangeDirection (direction, term) ->
      RecCommand.ChangeDirection (convertDirection prams direction, convertTerm prams term)
    | RecCommand.ChangeSpeed (speed, term) ->
      RecCommand.ChangeSpeed (convertSpeed prams speed, convertTerm prams term)
    | RecCommand.Accel (horizontal, vertical, term) ->
      RecCommand.Accel (convertHorizontalOption prams horizontal,
                        convertVerticalOption prams vertical, convertTerm prams term)
    | RecCommand.Wait s -> RecCommand.Wait (convertWait prams s)
    | RecCommand.Vanish -> RecCommand.Vanish
    | RecCommand.Repeat (times, child) ->
      RecCommand.Repeat (convertTimes prams times, substActionElm prams child)
    | RecCommand.Fire (attrs, direction, speed, child) ->
      RecCommand.Fire (attrs, convertDirectionOption prams direction,
                       convertSpeedOption prams speed, substBulletElm prams child)
    | RecCommand.FireRef (attrs, param) -> RecCommand.FireRef (attrs, convertParam prams param)
    | RecCommand.Action (attrs, children) ->
      RecCommand.Action (attrs, children |> List.map (substCommand prams))
    | RecCommand.ActionRef (attrs, param) -> RecCommand.ActionRef (attrs, convertParam prams param)

  and private substActionElm prams (a: RecActionElm) : RecActionElm =
    match a with
    | RecActionElm.Action (attrs, children) ->
      RecActionElm.Action (attrs, children |> List.map (substCommand prams))
    | RecActionElm.ActionRef (attrs, param) -> RecActionElm.ActionRef (attrs, convertParam prams param)

  and private substBulletElm prams (b: RecBulletElm) : RecBulletElm =
    match b with
    | RecBulletElm.Bullet (attrs, direction, speed, children) ->
      RecBulletElm.Bullet (attrs, convertDirectionOption prams direction,
                           convertSpeedOption prams speed,
                           children |> List.map (substActionElm prams))
    | RecBulletElm.BulletRef (attrs, param) -> RecBulletElm.BulletRef (attrs, convertParam prams param)

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
  let internal refAction (target: RecActionElm) (label: ActionLabel) prams : RecActionElm =
    let prams = prams |> Param.ofList
    match target with
    | RecActionElm.Action (attrs, _) when attrs.actionLabel = Some label ->
      substActionElm prams target
    | _ -> target

  let internal refFire (target: RecCommand) (label: FireLabel) prams : RecCommand =
    let prams = prams |> Param.ofList
    match target with
    | RecCommand.Fire (attrs, _, _, _) when attrs.fireLabel = Some label ->
      substCommand prams target
    | _ -> target

  let internal refBullet (target: RecBulletElm) (label: BulletLabel) prams : RecBulletElm =
    let prams = prams |> Param.ofList
    match target with
    | RecBulletElm.Bullet (attrs, _, _, _) when attrs.bulletLabel = Some label ->
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
      : RecActionElm option =
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

  and private expandCommand visiting lastAction top (c: RecCommand) : RecCommand =
    match c with
    | RecCommand.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> c
      // 解いた結果は action。ActionRef が居たのは命令の位置なので、命令として置く
      | Some (RecActionElm.Action (a, cs)) -> RecCommand.Action (a, cs)
      | Some (RecActionElm.ActionRef (a, p)) -> RecCommand.ActionRef (a, p)
    | RecCommand.FireRef (attrs, prams) ->
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
    | RecCommand.Action (attrs, children) ->
      RecCommand.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | RecCommand.Fire (attrs, d, s, child) ->
      RecCommand.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | RecCommand.Repeat (times, child) ->
      RecCommand.Repeat (times, expandActionElm visiting lastAction top child)
    | RecCommand.ChangeDirection _ | RecCommand.ChangeSpeed _
    | RecCommand.Accel _ | RecCommand.Wait _ | RecCommand.Vanish -> c

  and private expandActionElm visiting lastAction top (a: RecActionElm) : RecActionElm =
    match a with
    | RecActionElm.Action (attrs, children) ->
      RecActionElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))
    | RecActionElm.ActionRef (attrs, prams) ->
      match resolveActionRef visiting lastAction top attrs prams with
      | None -> a
      | Some expanded -> expanded

  and private expandBulletElm visiting lastAction top (b: RecBulletElm) : RecBulletElm =
    match b with
    | RecBulletElm.Bullet (attrs, d, s, children) ->
      RecBulletElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | RecBulletElm.BulletRef (attrs, prams) ->
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

  let private expandTopElm visiting lastAction top (t: RecTopElm) : RecTopElm =
    match t with
    | RecTopElm.Bullet (attrs, d, s, children) ->
      RecTopElm.Bullet (attrs, d, s, children |> List.map (expandActionElm visiting lastAction top))
    | RecTopElm.Fire (attrs, d, s, child) ->
      RecTopElm.Fire (attrs, d, s, expandBulletElm visiting lastAction top child)
    | RecTopElm.Action (attrs, children) ->
      RecTopElm.Action (attrs, children |> List.map (expandCommand visiting lastAction top))

  /// 木を丸ごと展開する。根から入る唯一の入口
  let internal convertRefBulletml (top: RecBulletml) (recBulletml: RecBulletml) : RecBulletml =
    match recBulletml with
    | RecBulletml.Bulletml (attrs, elms) ->
      RecBulletml.Bulletml (attrs, elms |> List.map (expandTopElm Set.empty None top))

  /// top* の台本 1 本 を展開する。BulletRunner.buildRootTops が使う
  let internal convertRefActionElm (top: RecBulletml) (a: RecActionElm) : RecActionElm =
    expandActionElm Set.empty None top a

  /// 輪のために展開を止めた bulletRef を、走らせる側から 1 段だけ解く。
  /// 中にまた同じ参照が残るので、次に撃たれたときに次の 1 段が解かれる。
  ///
  /// 解く前から自分の key を visiting に入れておくこと。空から始めると
  /// 解いた中身の同じ参照がもう 1 段 展開され、1 段のつもりが 2 段になる。
  /// 新経路（Step.Resolvers）は木を組まないのでこちらを直に使う
  let internal expandBulletRefOnceRec top (label: BulletLabel) prams : RecBulletElm option =
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
  let internal expandActionRefOnceRec top (label: ActionLabel) prams : RecActionElm option =
    match tryFindAction top label with
    | Some action ->
      refAction action label prams
      |> expandActionElm (Set.singleton (ActionKey label)) (Some label) top
      |> Some
    | None -> None
