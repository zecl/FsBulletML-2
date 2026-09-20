/// 弾幕 を 種 に載せ、飛んだ 先 の n か所 で 咲かせる。
///
/// 撃つ のは 敵 1 点 だけ なので、同じ 弾幕 を 2 つ 重ねて も 1 つ にしか 見えない ——
/// 茎 を 1 本 下 へ伸ばし、その先 から 種 を n 角形 に撒いて、種 の位置 で 1 波 を撃たせる。
/// 中身 は読まない ので、花 でも 渦 でも 幕 でも 同じ 手 が効く。
///
/// 弾数 は 撒いた 数 だけ 増える。減らす のは 作る 側 の仕事 ——
/// 花 なら `HarmonicSpec.Blooms` が 1 輪 の腕 を割る
module FsBulletML2.Scatter

open FsBulletML2

/// 茎 が伸びる コマ と、その 距離（px）。
///
/// n 角形 の中心 を ここ まで 下げる。敵 (240, 80) を中心 に する と、
/// 上 の頂点 が y = 20 に来て 咲いた 弾幕 の上半分 が 天井 で切れた（面 は 480x640）。
/// 面 の寸法 を知って いる のは この 2 つ の定数 だけ で、`Scatter` の他 は AST しか 見ない
let [<Literal>] STEM_TERM = 40

let DROP = 120.0

/// 種 が飛ぶ コマ と、n 角形 の半径（px）
let [<Literal>] SEED_TERM = 40

let REACH = 120.0

let [<Literal>] MARK = "scattered"

/// n 角形 の頂点 の向き（度）。0 は 真上、180 が 真下。
///
/// 半 目盛り ずらす ので、真上 は いつも 辺 の真ん中 に来る ——
/// 頂点 を 真上 に置く と、その 種 が 茎 を遡って 敵 の高さ へ戻る。
/// n が奇数 なら 真下 に 1 つ 来る
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

/// 外側 の繰り返し は 撒く 側 に残す。1 波 の中身 だけ を 種 へ移す。
///
/// 間合い が 1 つ も残らない なら 散らさない。待ち を持たない 台本 は 1 コマ で終わり、
/// 面 が 頭 から 走らせ直す ので 毎コマ 茎 を撒き 直す ——
/// 待ち が 内側 の `repeat` の中 にしか 無い 輪 を 3 か所 に散らして 100 コマ 走らせる と
/// 221,390 発 まで 増えた（散らさなければ 1,482 発）
let private split (stem: string) (xs: Action list) =
  let keep (inner: Action list) (wrap: Action list -> Action list) =
    match splitTailWaits inner with
    | _, [] -> None
    | wave, tail -> Some(wrap (stemFire stem :: tail), wave)
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
          match split (MARK + "-stem") xs with
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
              match split stem xs with
              | None | Some(_, []) -> e
              | Some(top, wave) ->
                taken.Add stem |> ignore
                taken.Add seed |> ignore
                seeds.Add(stemBullet stem seed count)
                seeds.Add(seedBullet seed wave)
                BulletmlElm.Action(a, top)
            | _ -> e)
      Bulletml(attrs, outs @ List.ofSeq seeds)
