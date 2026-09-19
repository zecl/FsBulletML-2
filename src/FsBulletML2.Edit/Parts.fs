/// 同梱 の弾幕 から「仕掛け」を抜いて、別 の弾幕 の終点 に足す。
///
/// 生成器 の型 は 5 つ しか 無く、骨格 が そこ で止まる。
/// 同梱 には 本物 の弾幕 が在る ので、その中 の 動き を 部品 として 借りる。
///
/// ## 抜く のは 自己完結 した 動き だけ
///
/// 撃つ 枝（`fire`）は 入れない —— `Combine.Inside` と同じ 掛け算 に なる。
/// 外 を指す 参照（`actionRef` / `bulletRef`）も 入れない ——
/// 抜いた 先 に その 名前 が無く、迷子 に なる。
///
/// 残る のは `changeDirection` / `changeSpeed` / `accel` / `wait` / `vanish` と、
/// それ だけ を包む `repeat`。どれ も 弾 を増やさない ので、
/// 足して も 同時数 は 変わらない
module FsBulletML2.Parts

open FsBulletML2

/// 仕掛け の種類。1 つ の枝 は 1 つ に落ちる ——
/// 複数 に当たる とき は 下 の順 で 先 に来た ほう
type Kind =
  /// 借りない。抜く 側 は この種類 に落とさない ので `find` は いつも 空
  | Idle
  /// 曲がる
  | Curve
  /// 加速 する
  | Accelerate
  /// 溜めて から 伸びる（速さ を 2 回 以上 変える）
  | Charge
  /// しばらく 飛んで 消える
  | Fade
  /// 速さ を 1 度 だけ 変える
  | Drift

/// 空 も `none` も 知らない 字 も `Idle`。
///
/// `Curve` に倒して いた とき、頼んで いない のに 全部 の弾幕 の終点 が曲がり、
/// どれ も 同じ 骨格 に見えた
let ofString (s: string) : Kind =
  match s with
  | "curve" -> Curve
  | "accel" -> Accelerate
  | "charge" -> Charge
  | "fade" -> Fade
  | "drift" -> Drift
  | _ -> Idle

/// 仕掛け を どこ に仕込む か。
///
/// 終点 だけ に置く と、段 を持つ 弾幕 では 割れた あと の 破片 しか 動かない。
/// どこ に置く か で 見え方 が まるで 変わる ので、jev に選ばせる
type Where =
  /// 終点 の弾 に、撃たれて すぐ
  | Leaf
  /// 終点 の弾 に、少し 飛んで から
  | Late
  /// 段 の途中 の弾 にも。撃つ 枝 を持つ 弾 も 動き出す
  | Every

/// 知らない 字 は `Leaf`
let whereOfString (s: string) : Where =
  match s with
  | "late" -> Late
  | "every" -> Every
  | _ -> Leaf

/// `Late` が待つ コマ。撃たれて から 仕掛け が始まる まで ——
/// 短い と `Leaf` と見分け が つかない
let [<Literal>] LATE_WAIT = "40"

/// 借りた ところ に付ける 印。外せる ように する ため ——
/// `Tune` の段 や `Combine` と同じ 手
let [<Literal>] MARK = "part"

// --- 抜く -----------------------------------------------------------------

/// 引数 への参照（`$1`）を持つ 字 か。
///
/// 参照（`actionRef`）は 弾いて いた のに、その 引数 だけ が 残って いた ——
/// 抜いた 先 に 引数 が無い ので 値 が定まらない。
/// 実測 で 借りた 枝 に `<wait>$1</wait>` が 入って いた。
///
/// 式 の AST に 引数 の腕 は 無い（`Param.replace` が 字 で置き換える）ので、
/// 元 の字 を見る
let private hasParam (s: string) =
  let mutable found = false
  for i in 0 .. s.Length - 2 do
    if s.[i] = '$' && System.Char.IsDigit s.[i + 1] then found <- true
  found

/// その 1 手 が持つ 式 を ぜんぶ。`$1` が どこ に在って も 拾う ——
/// `wait` だけ 見て いた のでは `term` に潜った 引数 を 見落とす
let private exprsOf (a: Action) : string list =
  match a with
  | Action.Wait e -> [ e.Source ]
  | Action.ChangeDirection(Direction(_, e), Term t) -> [ e.Source; t.Source ]
  | Action.ChangeSpeed(Speed(_, e), Term t) -> [ e.Source; t.Source ]
  | Action.Accel(h, v, Term t) ->
    [ yield t.Source
      match h with
      | Some(Horizontal(_, e)) -> yield e.Source
      | None -> ()
      match v with
      | Some(Vertical(_, e)) -> yield e.Source
      | None -> () ]
  | Action.Repeat(Times t, _) -> [ t.Source ]
  | _ -> []

/// 弾 を増やさない 動き だけ か。
///
/// `repeat` は 中身 が 同じ 条件 を満たす とき だけ 通す ——
/// 中 に `fire` が在れば 掛け算 に なる
let rec private motionOnly (xs: Action list) =
  not (List.isEmpty xs)
  && xs
     |> List.forall (fun a ->
         exprsOf a |> List.forall (hasParam >> not)
         && match a with
            | Action.ChangeDirection _
            | Action.ChangeSpeed _
            | Action.Accel _
            | Action.Wait _
            | Action.Vanish -> true
            | Action.Repeat(_, ActionElm.Action(_, inner))
            | Action.Action(_, inner) -> motionOnly inner
            | _ -> false)

let private countOf (pick: Action -> bool) (xs: Action list) =
  let rec go acc (ys: Action list) =
    ys
    |> List.fold
        (fun n a ->
          match a with
          | Action.Repeat(_, ActionElm.Action(_, inner))
          | Action.Action(_, inner) -> go n inner
          | _ -> if pick a then n + 1 else n)
        acc
  go 0 xs

let private kindOf (xs: Action list) : Kind option =
  let speeds = countOf (fun a -> match a with Action.ChangeSpeed _ -> true | _ -> false) xs
  let turns = countOf (fun a -> match a with Action.ChangeDirection _ -> true | _ -> false) xs
  let accels = countOf (fun a -> match a with Action.Accel _ -> true | _ -> false) xs
  let gone = countOf (fun a -> match a with Action.Vanish -> true | _ -> false) xs

  if turns > 0 then Some Curve
  elif accels > 0 then Some Accelerate
  elif speeds >= 2 then Some Charge
  elif gone > 0 then Some Fade
  elif speeds = 1 then Some Drift
  else None

/// 名前 を落とす。同じ 動き を 終点 ぜんぶ に配る ので、
/// label を持った まま だと 同じ 名前 が 何度 も 現れる
let rec private unlabel (a: Action) : Action =
  match a with
  | Action.Action(_, xs) -> Action.Action({ actionLabel = None }, List.map unlabel xs)
  | Action.Repeat(t, ActionElm.Action(_, xs)) ->
    Action.Repeat(t, ActionElm.Action({ actionLabel = None }, List.map unlabel xs))
  | _ -> a

/// 木 の中 の 仕掛け を ぜんぶ 拾う。深い ところ から でも 拾う ——
/// 本物 の仕掛け は 弾 の中 の さらに 弾 に在る ことが多い
let extract (bulletml: Bulletml) : (Kind * Action list) list =
  let found = System.Collections.Generic.List<Kind * Action list>()

  let take (xs: Action list) =
    if motionOnly xs then
      match kindOf xs with
      | Some k -> found.Add(k, List.map unlabel xs)
      | None -> ()

  let rec inAction (a: Action) =
    match a with
    | Action.Action(_, xs) ->
      take xs
      List.iter inAction xs
    | Action.Repeat(_, e) -> inElm e
    | Action.Fire(_, _, _, b) -> inBullet b
    | _ -> ()

  and inElm (e: ActionElm) =
    match e with
    | ActionElm.Action(_, xs) ->
      take xs
      List.iter inAction xs
    | ActionElm.ActionRef _ -> ()

  and inBullet (b: BulletElm) =
    match b with
    | BulletElm.Bullet(_, _, _, xs) -> List.iter inElm xs
    | BulletElm.BulletRef _ -> ()

  match bulletml with
  | Bulletml(_, elms) ->
    for e in elms do
      match e with
      | BulletmlElm.Action(_, xs) ->
        take xs
        List.iter inAction xs
      | BulletmlElm.Bullet(_, _, _, xs) -> List.iter inElm xs
      | BulletmlElm.Fire(_, _, _, b) -> inBullet b

  List.ofSeq found

/// 何本 か の弾幕 から、その種類 の仕掛け を 1 つ 選ぶ。
///
/// いちばん 長い もの を採る —— 1 手 だけ の枝 は どの弾幕 にも在り、
/// 借りて も 形 が変わらない
let find (kind: Kind) (sources: Bulletml seq) : Action list option =
  sources
  |> Seq.collect extract
  |> Seq.filter (fun (k, _) -> k = kind)
  |> Seq.sortByDescending (fun (_, xs) -> List.length xs)
  |> Seq.tryHead
  |> Option.map snd

// --- 足す -----------------------------------------------------------------

/// 撃つ 枝 を持つ 弾 にも 足す 歩き方。`Combine.atLeafTop` は 終点 だけ を見る ので、
/// `Every` は ここ を通る —— 判定 を外した だけ の 別 の歩き方 に なる
let rec private everyBullet (branch: ActionElm) (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    BulletElm.Bullet(attrs, d, s, List.map (everyElm branch) xs @ [ branch ])
  | BulletElm.BulletRef _ -> b

and private everyAction (branch: ActionElm) (a: Action) : Action =
  match a with
  | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, d, s, everyBullet branch b)
  | Action.Repeat(t, e) -> Action.Repeat(t, everyElm branch e)
  | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (everyAction branch) xs)
  | _ -> a

and private everyElm (branch: ActionElm) (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (everyAction branch) xs)
  | ActionElm.ActionRef _ -> e

let private everyTop (branch: ActionElm) (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (everyAction branch) xs)
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    BulletmlElm.Bullet(attrs, d, s, List.map (everyElm branch) xs @ [ branch ])
  | BulletmlElm.Fire(attrs, d, s, b) -> BulletmlElm.Fire(attrs, d, s, everyBullet branch b)

/// 弾 に仕掛け を足す。どこ に置く か は `Where`。
///
/// `Leaf` と `Late` の歩き方 は `Combine` と共通 —— 分けて 書く と、
/// 片方 だけ 直した とき に どちら が正 か が言えなく なる
let graft (where: Where) (body: Action list) (target: Bulletml) : Bulletml =
  if List.isEmpty body then target
  else
    let inner =
      match where with
      | Late -> Action.Wait(Expr.NumExpr.ofString LATE_WAIT) :: body
      | Leaf | Every -> body

    let branch = ActionElm.Action({ actionLabel = Some(ActionLabel MARK) }, inner)

    match target, where with
    | Bulletml(attrs, elms), Every -> Bulletml(attrs, List.map (everyTop branch) elms)
    | Bulletml(attrs, elms), _ ->
      let onLeaf : Combine.OnLeaf =
        fun a d s xs -> BulletElm.Bullet(a, d, s, xs @ [ branch ])

      Bulletml(attrs, List.map (Combine.atLeafTop onLeaf) elms)
