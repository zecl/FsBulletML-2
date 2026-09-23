/// 弾幕 を 種 に載せ、飛んだ 先 の n か所 で 咲かせる。
/// 中身 は読まない。弾数 は 撒いた 数 だけ 増える。
module FsBulletML2.Scatter

open FsBulletML2

/// 茎 が伸びる コマ と、その 距離（px）。n 角形 の中心 を ここ まで 下げる。
/// 面 の寸法 を知って いる のは この 2 つ の定数 だけ。
let [<Literal>] STEM_TERM = 40

let DROP = 120.0

/// 種 が飛ぶ コマ と、n 角形 の半径（px）
let [<Literal>] SEED_TERM = 40

let REACH = 120.0

let [<Literal>] MARK = "scattered"

/// n 角形 の頂点 の向き（度）。0 は 真上、180 が 真下。
/// 半 目盛り ずらす。頂点 を 真上 に置く と 茎 を遡って 敵 の高さ へ戻る。
let vertexDeg (count: int) (j: int) = 360.0 * (float j + 0.5) / float count

// --- 名前 -----------------------------------------------------------------

let private bulletLabels (elms: BulletmlElm list) =
  let names = System.Collections.Generic.HashSet<string>()
  let add (attrs: BulletAttrs) =
    match attrs.bulletLabel with
    | Some(BulletLabel l) -> names.Add l |> ignore
    | None -> ()
  let rec inAction (a: Action) =
    match a with
    | Action.Fire(_, _, _, b) -> inBullet b
    | Action.Repeat(_, e) -> inElm e
    | Action.Action(_, xs) -> List.iter inAction xs
    | _ -> ()
  and inElm (e: ActionElm) =
    match e with
    | ActionElm.Action(_, xs) -> List.iter inAction xs
    | ActionElm.ActionRef _ -> ()
  and inBullet (b: BulletElm) =
    match b with
    | BulletElm.Bullet(attrs, _, _, xs) ->
      add attrs
      List.iter inElm xs
    | BulletElm.BulletRef _ -> ()
  for e in elms do
    match e with
    | BulletmlElm.Action(_, xs) -> List.iter inAction xs
    | BulletmlElm.Bullet(attrs, _, _, xs) ->
      add attrs
      List.iter inElm xs
    | BulletmlElm.Fire(_, _, _, b) -> inBullet b
  names

let private freeName (taken: System.Collections.Generic.HashSet<string>) (stem: string) =
  let rec go (n: int) =
    let s = if n = 1 then stem else sprintf "%s%d" stem n
    if taken.Contains s then go (n + 1) else s
  go 1

// --- 組み立て -------------------------------------------------------------

let private isTop (e: BulletmlElm) =
  match e with
  | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> l.StartsWith "top"
  | _ -> false

/// 末尾 の `wait` を切り離す。波 と波 の間合い は 撒く 側 に残す ——
/// 種 に付ける と、待つ のが 咲いた あと になって 波 が詰まる
let private splitTailWaits (xs: Action list) =
  let isWait a = match a with Action.Wait _ -> true | _ -> false
  let rev = List.rev xs
  List.rev (List.skipWhile isWait rev), List.rev (List.takeWhile isWait rev)

let private fireAt (deg: float) (speed: float) (name: string) =
  Action.Fire(
    { fireLabel = None },
    Some(Direction(Some { directionType = DirectionType.Absolute }, Expr.NumExpr.ofString (sprintf "%.2f" deg))),
    Some(Speed(Some { speedType = SpeedType.Absolute }, Expr.NumExpr.ofString (sprintf "%.2f" speed))),
    BulletElm.BulletRef({ bulletRefLabel = BulletLabel name }, []))

/// 敵 から 真下 へ 1 本。n 角形 の中心 を ここ まで 運ぶ
let private stemFire (stem: string) = fireAt 180.0 (DROP / float STEM_TERM) stem

/// 茎 の先 で n 角形 に撒いて 消える。茎 を残す と、面 が 頭 から 走らせ直して また 撒く
let private stemBullet (stem: string) (seed: string) (count: int) =
  let speed = REACH / float SEED_TERM
  BulletmlElm.Bullet(
    { bulletLabel = Some(BulletLabel stem) }, None, None,
    [ ActionElm.Action(
        { actionLabel = Some(ActionLabel MARK) },
        [ yield Action.Wait(Expr.NumExpr.ofString(string STEM_TERM))
          for j in 0 .. count - 1 -> fireAt (vertexDeg count j) speed seed
          yield Action.Vanish ]) ])

/// 咲いた あと の 種 も消す
let private seedBullet (name: string) (wave: Action list) =
  BulletmlElm.Bullet(
    { bulletLabel = Some(BulletLabel name) }, None, None,
    [ ActionElm.Action(
        { actionLabel = Some(ActionLabel MARK) },
        [ yield Action.Wait(Expr.NumExpr.ofString(string SEED_TERM))
          yield! wave
          yield Action.Vanish ]) ])

/// 実引数 が残って いない か。`$rand` と `$rank` 以外 の `$` は 埋まらなかった 印。
/// `Param.replaceIn` は 足りない 番号 で 例外 を投げない。余った `$2` は 0 に評価 される。
let private filled (e: Expr.NumExpr) =
  not ((Expr.NumExpr.text e).Replace("$rand", "").Replace("$rank", "").Contains "$")

/// 実引数 を 式 に入れる。写し が見る のは `wait` と `term` と `times` だけ だが、
/// 入れ子 の `actionRef` の 実引数 も 字 のまま 残る ので そこ も置き換える
let rec private fillIn (param: Map<string, string>) (a: Action) : Action =
  let e (x: Expr.NumExpr) = Param.replaceIn param x
  match a with
  | Action.Wait x -> Action.Wait(e x)
  | Action.ChangeSpeed(s, Term t) -> Action.ChangeSpeed(s, Term(e t))
  | Action.ChangeDirection(d, Term t) -> Action.ChangeDirection(d, Term(e t))
  | Action.Accel(h, v, Term t) -> Action.Accel(h, v, Term(e t))
  | Action.Repeat(Times t, ActionElm.Action(x, ys)) ->
    Action.Repeat(Times(e t), ActionElm.Action(x, List.map (fillIn param) ys))
  | Action.Repeat(Times t, elm) -> Action.Repeat(Times(e t), elm)
  | Action.Action(x, ys) -> Action.Action(x, List.map (fillIn param) ys)
  | Action.ActionRef(x, ps) -> Action.ActionRef(x, List.map (fun p -> Param.replace p param) ps)
  | _ -> a

/// 波 の木 から 撃つ ところ を抜いた 写し。間合い だけ が残る。
/// 止める のは `changeSpeed` と `changeDirection`。`relative 0` に差し替える。
let rec private ghostIn (actions: Map<string, Action list>) (seen: string list) (xs: Action list) : Action list option =
  let zero = Expr.NumExpr.ofString "0"
  let ok (e: Expr.NumExpr) = not e.NeedRand && filled e
  let rec one (a: Action) : Action list option =
    match a with
    // 写し の `repeat` は label を持たない。元 の action の label を そのまま 使う と 名前 がぶつかる
    | Action.Repeat(Times t, elm) when ok t ->
      match elmOf elm with
      | None -> None
      | Some [] -> Some []
      | Some ys -> Some [ Action.Repeat(Times t, ActionElm.Action({ actionLabel = None }, ys)) ]
    | Action.Action(a, ys) ->
      match ghostIn actions seen ys with
      | None -> None
      | Some [] -> Some []
      | Some zs -> Some [ Action.Action(a, zs) ]
    | Action.ActionRef(attrs, ps) -> refOf attrs ps
    | Action.Wait e -> if ok e then Some [ a ] else None
    | Action.ChangeSpeed(_, Term t) ->
      if ok t then
        Some [ Action.ChangeSpeed(Speed(Some { speedType = SpeedType.Relative }, zero), Term t) ]
      else None
    | Action.ChangeDirection(_, Term t) ->
      if ok t then
        Some [ Action.ChangeDirection(Direction(Some { directionType = DirectionType.Relative }, zero), Term t) ]
      else None
    | Action.Accel _ | Action.Fire _ | Action.FireRef _ -> Some []
    | Action.Vanish | Action.Repeat _ -> None
  and elmOf (e: ActionElm) : Action list option =
    match e with
    | ActionElm.Action(_, ys) -> ghostIn actions seen ys
    | ActionElm.ActionRef(attrs, ps) -> refOf attrs ps
  // 同じ label を 2 度 通ったら 止まらない。辿れない label は 所要 コマ が決まらない
  and refOf (attrs: ActionRefAttrs) (ps: Params) : Action list option =
    let (ActionLabel l) = attrs.actionRefLabel
    if List.contains l seen then None
    else
      match Map.tryFind l actions with
      | None -> None
      | Some body ->
        let param = Param.ofList ps
        let filledBody = if Map.isEmpty param then body else List.map (fillIn param) body
        ghostIn actions (l :: seen) filledBody
  let rec go (acc: Action list) (rest: Action list) =
    match rest with
    | Action.Vanish :: _ -> Some(List.rev acc)
    | [] -> Some(List.rev acc)
    | a :: tl ->
      match one a with
      | None -> None
      | Some ys -> go (List.rev ys @ acc) tl
  go [] xs

/// この 写し が コマ を食う か。`repeat` の中 の `wait` も 数える
let rec private spends (xs: Action list) =
  xs
  |> List.exists (fun a ->
      match a with
      | Action.Wait _ | Action.ChangeSpeed _ | Action.ChangeDirection _ -> true
      | Action.Action(_, ys) -> spends ys
      | Action.Repeat(_, ActionElm.Action(_, ys)) -> spends ys
      | _ -> false)

/// `top` から 引ける action の表。`actionRef` の 行き先 を辿る のに要る
let private topActions (elms: BulletmlElm list) : Map<string, Action list> =
  elms
  |> List.choose (fun e ->
      match e with
      | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, xs) -> Some(l, xs)
      | _ -> None)
  |> Map.ofList

/// 外側 の繰り返し は 撒く 側 に残す。1 波 の中身 だけ を 種 へ移す。
/// 間合い が残らない なら 散らさない。待ち が無い と 毎コマ 茎 を撒き 直す。
let private split (actions: Map<string, Action list>) (stem: string) (xs: Action list) =
  let keep (inner: Action list) (wrap: Action list -> Action list) =
    let wave, _ = splitTailWaits inner
    match ghostIn actions [] inner with
    | Some g when spends g -> Some(wrap (stemFire stem :: g), wave)
    | _ -> None
  match xs with
  | [ Action.Repeat(times, ActionElm.Action(a, inner)) ] ->
    keep inner (fun body -> [ Action.Repeat(times, ActionElm.Action(a, body)) ])
  | _ -> keep xs id

/// 実際 に咲く 数。散らせない 木 は 1 ——
/// 取り分 を `count` で割る 側 が、散らなかった とき に 弾 を 1/n に薄めない ため
let places (count: int) (bulletml: Bulletml) : int =
  if count <= 1 then 1
  else
    match bulletml with
    | Bulletml(_, elms) ->
      let splits e =
        match e with
        | BulletmlElm.Action(_, xs) when isTop e ->
          match split (topActions elms) (MARK + "-stem") xs with
          | None | Some(_, []) -> false
          | Some _ -> true
        | _ -> false
      if List.exists splits elms then count else 1

/// 1 以下 は そのまま。撃つ もの を持たない 台本 も そのまま ——
/// 「作れなかった」に しない
let apply (count: int) (bulletml: Bulletml) : Bulletml =
  if count <= 1 then bulletml
  else
    match bulletml with
    | Bulletml(attrs, elms) ->
      let taken = bulletLabels elms
      let seeds = ResizeArray<BulletmlElm>()
      let outs =
        elms
        |> List.map (fun e ->
            match e with
            | BulletmlElm.Action(a, xs) when isTop e ->
              let stem = freeName taken (MARK + "-stem")
              let seed = freeName taken (MARK + "-seed")
              match split (topActions elms) stem xs with
              | None | Some(_, []) -> e
              | Some(top, wave) ->
                taken.Add stem |> ignore
                taken.Add seed |> ignore
                seeds.Add(stemBullet stem seed count)
                seeds.Add(seedBullet seed wave)
                BulletmlElm.Action(a, top)
            | _ -> e)
      Bulletml(attrs, outs @ List.ofSeq seeds)
