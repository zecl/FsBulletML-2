/// 2 つ の弾幕 を 1 つ に混ぜる。
///
/// 繋ぎ方 は 5 通り —— どれ も 名前 の付け直し が本体 で、
/// 違う のは 相手 を どこ へ 繋ぐ か だけ。
///
///     Beside   根 の下 へ置く。2 つ が同時 に走る
///     Mirror   根 の下 へ置いて、B の絶対角 を 180 度 回す
///     After    根 の下 へ置いて、B を 遅らせて 始める
///     Inside   A の終点 の弾 へ入れる。A の弾 が飛んだ 先 で B が咲く
///     Borrow   A の終点 の弾 を B の弾 に差し替える。撒き方 は A のまま
///
/// 弾数 の増え方 が 違う。根 の下 へ置く 3 つ は 足し算、`Borrow` は A のまま、
/// `Inside` だけ が 掛け算（A の終点弾 の数 × B の 1 波）——
/// あそこ は 1 波 に絞った 上 で 4 発 に 1 発 だけ 咲かせて いる
///
/// ## 名前 がぶつかる
///
/// どちら の弾幕 も `<action label="top">` と `<bullet label="core">` を
/// 持って いる ことが多い。素 で繋ぐ と、**あと から 引いた ほう が
/// 両方 の参照 を持って いく**（`BulletmlOps` は名前 で引く）。
///
/// 相手 の名前 を ぜんぶ 付け直して から 繋ぐ。
/// 名前空間 は 3 つ（action / fire / bullet）で、型 が別 なので
/// `action:core` と `bullet:core` は ぶつからない。
///
/// ## `top` で始まる 名前 は 特別
///
/// 走らせる 側 は「名前 が `top` で始まる `action`」を 並行 に走らせる
/// （`Api.fs`）。相手 の top を `b-top` にする と 走らなく なる ので、
/// そちら だけ は 頭 に `top` を残した まま 付け直す
module FsBulletML2.Combine

open FsBulletML2

type Join =
  /// 根 の下 へ置く。2 つ が同時 に走る
  | Beside
  /// 根 の下 へ置いて、B の絶対角 を 180 度 回す。
  /// 上 から 降る 幕 と 下 から 湧く 渦 の ように、面 の両端 から 来る
  | Mirror
  /// 根 の下 へ置いて、B を 遅らせて 始める。
  /// 同時 に出す と 1 層 に見える ので、時差 で割る
  | After
  /// A の終点 の弾 へ入れる。A の弾 が飛んだ 先 で B が咲く
  | Inside
  /// A の撒き方 に B の弾 を載せる。弾数 は A のまま
  | Borrow

let ofString (s: string) : Join option =
  match s with
  | "beside" -> Some Beside
  | "mirror" -> Some Mirror
  | "after" -> Some After
  | "inside" -> Some Inside
  | "borrow" -> Some Borrow
  | _ -> None

/// `After` が B を遅らせる コマ。短い と 同時 に出た のと 見分け が つかない
let [<Literal>] LAG = "90"

/// 混ぜた もの に付ける 印。外せる ように する ため ——
/// `Tune` の段 と同じ 手
let [<Literal>] MARK = "mixed"

// --- 名前 を集める ---------------------------------------------------------

let private actionLabels (bulletml: Bulletml) =
  let names = System.Collections.Generic.HashSet<string>()
  let rec inAction (a: Action) =
    match a with
    | Action.Action({ actionLabel = Some(ActionLabel l) }, xs) ->
      names.Add l |> ignore
      List.iter inAction xs
    | Action.Action(_, xs) -> List.iter inAction xs
    | Action.Repeat(_, e) -> inElm e
    | Action.Fire(_, _, _, b) -> inBullet b
    | _ -> ()
  and inElm (e: ActionElm) =
    match e with
    | ActionElm.Action({ actionLabel = Some(ActionLabel l) }, xs) ->
      names.Add l |> ignore
      List.iter inAction xs
    | ActionElm.Action(_, xs) -> List.iter inAction xs
    | ActionElm.ActionRef _ -> ()
  and inBullet (b: BulletElm) =
    match b with
    | BulletElm.Bullet(_, _, _, xs) -> List.iter inElm xs
    | BulletElm.BulletRef _ -> ()
  match bulletml with
  | Bulletml(_, elms) ->
    for e in elms do
      match e with
      | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, xs) ->
        names.Add l |> ignore
        List.iter inAction xs
      | BulletmlElm.Action(_, xs) -> List.iter inAction xs
      | BulletmlElm.Bullet(_, _, _, xs) -> List.iter inElm xs
      | BulletmlElm.Fire(_, _, _, b) -> inBullet b
    names

/// ぶつからない 頭 を選ぶ。A が 既 に `b-` を持って いたら `b2-` へ ——
/// 数える のは 名前 の頭 だけ なので、深く 歩かなくて よい
let private freePrefix (taken: System.Collections.Generic.HashSet<string>) =
  let rec go (n: int) =
    let p = if n = 1 then "b-" else sprintf "b%d-" n
    if taken |> Seq.exists (fun (s: string) -> s.StartsWith p) then go (n + 1) else p
  go 1

// --- 名前 を付け直す -------------------------------------------------------

/// `top` で始まる 名前 は 頭 を残す。走らせる 側 が そこ を見る
let private rename (prefix: string) (s: string) =
  if s.StartsWith "top" then "top" + prefix + s.Substring 3 else prefix + s

let private aLabel p (ActionLabel s) = ActionLabel(rename p s)
let private fLabel p (FireLabel s) = FireLabel(p + s)
let private bLabel p (BulletLabel s) = BulletLabel(p + s)

let rec private mapAction p (a: Action) : Action =
  match a with
  | Action.Action(attrs, xs) ->
    Action.Action({ actionLabel = Option.map (aLabel p) attrs.actionLabel }, List.map (mapAction p) xs)
  | Action.ActionRef(attrs, ps) ->
    Action.ActionRef({ actionRefLabel = aLabel p attrs.actionRefLabel }, ps)
  | Action.Repeat(t, e) -> Action.Repeat(t, mapElm p e)
  | Action.Fire(attrs, d, s, b) ->
    Action.Fire({ fireLabel = Option.map (fLabel p) attrs.fireLabel }, d, s, mapBullet p b)
  | Action.FireRef(attrs, ps) ->
    Action.FireRef({ fireRefLabel = fLabel p attrs.fireRefLabel }, ps)
  | _ -> a

and private mapElm p (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(attrs, xs) ->
    ActionElm.Action({ actionLabel = Option.map (aLabel p) attrs.actionLabel }, List.map (mapAction p) xs)
  | ActionElm.ActionRef(attrs, ps) ->
    ActionElm.ActionRef({ actionRefLabel = aLabel p attrs.actionRefLabel }, ps)

and private mapBullet p (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    BulletElm.Bullet({ bulletLabel = Option.map (bLabel p) attrs.bulletLabel }, d, s, List.map (mapElm p) xs)
  | BulletElm.BulletRef(attrs, ps) ->
    BulletElm.BulletRef({ bulletRefLabel = bLabel p attrs.bulletRefLabel }, ps)

let private mapTop p (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Action(attrs, xs) ->
    BulletmlElm.Action({ actionLabel = Option.map (aLabel p) attrs.actionLabel }, List.map (mapAction p) xs)
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    BulletmlElm.Bullet({ bulletLabel = Option.map (bLabel p) attrs.bulletLabel }, d, s, List.map (mapElm p) xs)
  | BulletmlElm.Fire(attrs, d, s, b) ->
    BulletmlElm.Fire({ fireLabel = Option.map (fLabel p) attrs.fireLabel }, d, s, mapBullet p b)

/// 相手 の名前 を ぜんぶ 付け直す。**参照 も一緒 に** ——
/// 定義 だけ 直す と、参照 が 元 の名前 を指した まま 迷子 になる
let private prefixed (p: string) (bulletml: Bulletml) =
  match bulletml with
  | Bulletml(attrs, elms) -> Bulletml(attrs, List.map (mapTop p) elms)

// --- 並べ方 を変える -------------------------------------------------------

/// 名前 が `top` で始まる `action`。走らせる 側 が 並行 に走らせる もの
let private isTop (e: BulletmlElm) =
  match e with
  | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> l.StartsWith "top"
  | _ -> false

/// 絶対角 だけ を回す。
///
/// `sequence` と `relative` は 直前 から の差分 で、`aim` は 自機 を見る ——
/// どれ も 回す と 形 が壊れる。基準 を持って いる のは `absolute` だけ で、
/// BulletML の既定 は `aim`（`DTD.fs`）なので 型 を書いて いない 向き も 触らない
let private turned (deg: int) (d: Direction option) =
  match d with
  | Some(Direction(Some a, e)) when a.directionType = DirectionType.Absolute ->
    Some(Direction(Some a, Expr.NumExpr.ofString (sprintf "(%s) + %d" e.Source deg)))
  | _ -> d

let rec private turnAction deg (a: Action) : Action =
  match a with
  | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, turned deg d, s, turnBullet deg b)
  | Action.Repeat(t, e) -> Action.Repeat(t, turnElm deg e)
  | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (turnAction deg) xs)
  | Action.ChangeDirection(d, t) ->
    match turned deg (Some d) with
    | Some d' -> Action.ChangeDirection(d', t)
    | None -> a
  | _ -> a

and private turnElm deg (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (turnAction deg) xs)
  | ActionElm.ActionRef _ -> e

and private turnBullet deg (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    BulletElm.Bullet(attrs, turned deg d, s, List.map (turnElm deg) xs)
  | BulletElm.BulletRef _ -> b

let private turnTop deg (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (turnAction deg) xs)
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    BulletmlElm.Bullet(attrs, turned deg d, s, List.map (turnElm deg) xs)
  | BulletmlElm.Fire(attrs, d, s, b) -> BulletmlElm.Fire(attrs, turned deg d, s, turnBullet deg b)

/// B の top の頭 に 待ち を挿す。当てる のは `top` だけ ——
/// 弾 の中 の action に入れる と、1 発 ごと に 遅れて 形 が崩れる
let private lagged (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Action(attrs, xs) when isTop e ->
    BulletmlElm.Action(attrs, Action.Wait(Expr.NumExpr.ofString LAG) :: xs)
  | _ -> e

// --- 繋ぐ -----------------------------------------------------------------


/// 撃つ 枝 を 1 つ でも 持って いるか。持って いない 弾 が 段 の終点
let rec private firesIn (a: Action) =
  match a with
  | Action.Fire _ | Action.FireRef _ -> true
  | Action.Repeat(_, e) -> firesInElm e
  | Action.Action(_, xs) -> List.exists firesIn xs
  | _ -> false

and private firesInElm (e: ActionElm) =
  match e with
  | ActionElm.Action(_, xs) -> List.exists firesIn xs
  | ActionElm.ActionRef _ -> false

/// B の top から 外側 の繰り返し を剥がす。1 波 だけ にする。
///
/// top は「面 1 枚 ぶん の弾幕」で、外側 に「何十 波 繰り返す」が付いて いる。
/// それ を A の弾 1 つ ごと に 丸ごと 走らせる と 掛け算 で増える ——
/// 実機 で 同時 15,319 発 / 5 FPS だった。
///
/// A の弾 が「B の 1 波 になって 咲く」形 にする。
/// 内側 の繰り返し（腕 を撒く `repeat`）は 残す —— あれ は 1 波 の中身
let rec private oneWave (xs: Action list) : Action list =
  xs
  |> List.collect (fun a ->
      match a with
      | Action.Repeat(_, ActionElm.Action(_, inner)) -> oneWave inner
      | _ -> [ a ])

/// 咲く のは A の終点弾 の 4 発 に 1 発。
///
/// 掛け算 そのもの は消せない —— A の終点弾 の数 × B の 1 波 が Inside の意味 で、
/// 1 波 に絞った あと も 実機 で フリーズ した。掛ける 側 の数 を減らす。
///
/// `repeat` の `times` は `max 0` で丸める ので、int が 1 に届く とき だけ 咲く ——
/// `$rand * 4 / 3` は rand ∈ [0, 1) を [0, 1.33) にする ので、
/// 1 に届く のは rand ≥ 0.75 の 4 発 に 1 発。
///
/// 600 コマ の最大同時（10Way に Progear を咲かせる）——
/// 間引き なし 377,318 発 / 4 発 に 1 発 36,895 発 / 16 発 に 1 発 14,667 発。
/// 確率 だけ では 上 が決まらない ので、`LIFE` と 2 本 で 抑える
let [<Literal>] BLOOM = "$rand * 4 / 3"

/// 終点 の弾 に入れる 枝。少し 飛んで から B の top を引いて 消える
let private branch (tops: ActionLabel list) =
  ActionElm.Action(
    { actionLabel = Some(ActionLabel MARK) },
    [ yield Action.Wait(Expr.NumExpr.ofString "30")
      for t in tops ->
        Action.Repeat(
          Times(Expr.NumExpr.ofString BLOOM),
          ActionElm.Action({ actionLabel = None }, [ Action.ActionRef({ actionRefLabel = t }, []) ]))
      yield Action.Vanish ])

/// 終点 の弾 を見つけて、そこ に だけ 手 を入れる。
///
/// `Inside` と `Borrow` が 同じ 歩き方 を要る ので 1 本 にした ——
/// 分けて 書く と、片方 だけ 直した とき に どちら が正 か が言えなく なる。
/// 違う のは 葉 で何 を返す か だけ
type OnLeaf = BulletAttrs -> Direction option -> Speed option -> ActionElm list -> BulletElm

let rec atLeafBullet (onLeaf: OnLeaf) (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    let deeper = List.map (atLeafElm onLeaf) xs
    if List.exists firesInElm xs then BulletElm.Bullet(attrs, d, s, deeper)
    else onLeaf attrs d s deeper
  | BulletElm.BulletRef _ -> b

and private atLeafAction (onLeaf: OnLeaf) (a: Action) : Action =
  match a with
  | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, d, s, atLeafBullet onLeaf b)
  | Action.Repeat(t, e) -> Action.Repeat(t, atLeafElm onLeaf e)
  | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (atLeafAction onLeaf) xs)
  | _ -> a

and private atLeafElm (onLeaf: OnLeaf) (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (atLeafAction onLeaf) xs)
  | ActionElm.ActionRef _ -> e

/// 根 の直下 の `<bullet label="...">` も 終点 になりうる。
///
/// 撃つ のが `<bulletRef>` の弾幕 —— 同梱 の多く が そう —— は、
/// 終点 が 撃つ 場所 でなく 定義 の側 に在る。
/// `BulletElm.BulletRef` を辿らない ので、ここ で見ない と 1 つ も 入らない
let atLeafTop (onLeaf: OnLeaf) (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (atLeafAction onLeaf) xs)
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    let deeper = List.map (atLeafElm onLeaf) xs
    if List.exists firesInElm xs then BulletmlElm.Bullet(attrs, d, s, deeper)
    else
      match onLeaf attrs d s deeper with
      | BulletElm.Bullet(a2, d2, s2, xs2) -> BulletmlElm.Bullet(a2, d2, s2, xs2)
      | BulletElm.BulletRef _ -> BulletmlElm.Bullet(attrs, d, s, deeper)
  | BulletmlElm.Fire(attrs, d, s, b) -> BulletmlElm.Fire(attrs, d, s, atLeafBullet onLeaf b)

let private insideLeaf (tops: ActionLabel list) : OnLeaf =
  fun attrs d s xs -> BulletElm.Bullet(attrs, d, s, xs @ [ branch tops ])

/// 咲いた 弾 が 消える まで の コマ。
///
/// 重い のは 溜まる から で、ピーク は どれ も 終盤 に来て いた。
/// 咲いた 弾 に 寿命 を付ける と 蓄積 が 頭打ち に なる ——
/// 確率 と違って、A が 何発 撃とう と 同時数 の上 が 決まる
let [<Literal>] LIFE = "60"

/// B の葉 の弾 に「しばらく 飛んで 消える」を足す。
///
/// 当てる のは B の側 だけ。A は 混ぜる 先 なので そのまま 残す
let private lifeLeaf : OnLeaf =
  fun attrs d s xs ->
    BulletElm.Bullet(
      attrs, d, s,
      xs @ [ ActionElm.Action({ actionLabel = None },
                              [ Action.Wait(Expr.NumExpr.ofString LIFE); Action.Vanish ]) ])

// --- 弾 を借りる -----------------------------------------------------------

/// B の中 の 1 発 目。速さ・向き と、その弾 が持つ 動き。
///
/// 根 の直下 の `<bullet>` を 先 に見る —— `<bulletRef>` の弾幕 は
/// そちら が本体 で、撃つ 場所 には 名前 しか 無い
let private firstBullet (bulletml: Bulletml) =
  let mutable found = ValueNone
  let rec inAction (a: Action) =
    if found.IsNone then
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
    if found.IsNone then
      match b with
      | BulletElm.Bullet(_, d, s, xs) -> found <- ValueSome(d, s, xs)
      | BulletElm.BulletRef _ -> ()
  match bulletml with
  | Bulletml(_, elms) ->
    for e in elms do
      match e with
      | BulletmlElm.Bullet(_, d, s, xs) -> if found.IsNone then found <- ValueSome(d, s, xs)
      | _ -> ()
    for e in elms do
      match e with
      | BulletmlElm.Action(_, xs) -> List.iter inAction xs
      | BulletmlElm.Fire(_, _, _, b) -> inBullet b
      | BulletmlElm.Bullet _ -> ()
  found

/// 名前 を落とす。同じ 動き を A の葉 ぜんぶ に配る ので、
/// label を持った まま だと 同じ 名前 が 何度 も 現れる
let rec private unlabel (a: Action) : Action =
  match a with
  | Action.Action(_, xs) -> Action.Action({ actionLabel = None }, List.map unlabel xs)
  | Action.Repeat(t, e) -> Action.Repeat(t, unlabelElm e)
  | _ -> a

and private unlabelElm (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(_, xs) -> ActionElm.Action({ actionLabel = None }, List.map unlabel xs)
  | ActionElm.ActionRef _ -> e

/// 撃つ 枝 を落とす。残す と `Inside` と同じ 掛け算 に戻る ——
/// 借りる のは 動き だけ で、撒き方 は A のもの
let private motionOnly (xs: ActionElm list) : ActionElm list =
  xs
  |> List.choose (fun e ->
      match e with
      | ActionElm.Action(attrs, inner) ->
        match inner |> List.filter (firesIn >> not) with
        | [] -> None
        | kept -> Some(unlabelElm (ActionElm.Action(attrs, kept)))
      | ActionElm.ActionRef _ -> None)

/// A の撒き方 に B の弾 を載せる。弾数 は A のまま —— 掛け算 が起きない。
///
/// B が 速さ も向き も動き も持たない 弾幕 なら、A は そのまま
let private borrowLeaf (bd: Direction option) (bs: Speed option) (motion: ActionElm list) : OnLeaf =
  fun attrs d s xs ->
    let d' = if bd.IsSome then bd else d
    let s' = if bs.IsSome then bs else s
    BulletElm.Bullet(attrs, d', s', xs @ motion)

/// 2 つ を 1 つ に混ぜる。`a` の向き と 名前 を 正 にする ——
/// `<bulletml type="...">` が 2 つ 在って も 面 は 1 つ
let apply (join: Join) (a: Bulletml) (b: Bulletml) : Bulletml =
  let p = freePrefix (actionLabels a)
  let renamed = prefixed p b

  match a, renamed with
  | Bulletml(attrs, aElms), Bulletml(_, bElms) ->
    match join with
    | Beside -> Bulletml(attrs, aElms @ bElms)
    // 回す のは B だけ。A は 混ぜる 先 なので そのまま 残す
    | Mirror -> Bulletml(attrs, aElms @ List.map (turnTop 180) bElms)
    | After -> Bulletml(attrs, aElms @ List.map lagged bElms)
    | Inside ->
      // B の top は 自分 では 走らせない。A の終点 から 引く ——
      // 置いた まま だと 2 通り（並行 と 終点）で 二重 に走る
      let tops =
        bElms
        |> List.choose (fun e ->
            match e with
            | BulletmlElm.Action({ actionLabel = Some l }, _) when isTop e -> Some l
            | _ -> None)
      // 名前 を `top` から 外す。引ける まま、並行 では 走らなく なる。
      // 併せて 外側 の繰り返し を剥がし（1 波 だけ にする）、
      // 咲く 側 の葉 に 寿命 を付ける
      let hidden =
        bElms
        |> List.map (fun e ->
            match e with
            | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, xs) when isTop e ->
              BulletmlElm.Action({ actionLabel = Some(ActionLabel(MARK + "-" + l)) }, oneWave xs)
            | _ -> e)
        |> List.map (atLeafTop lifeLeaf)
      let tops = tops |> List.map (fun (ActionLabel l) -> ActionLabel(MARK + "-" + l))
      Bulletml(attrs, List.map (atLeafTop (insideLeaf tops)) aElms @ hidden)
    | Borrow ->
      // B は 1 つ も 残さない。借りる のは 弾 の形 だけ で、
      // B の撒き方 を 置いて おく と そちら が 並行 に走る
      match firstBullet renamed with
      | ValueNone -> a
      | ValueSome(bd, bs, bxs) ->
        Bulletml(attrs, List.map (atLeafTop (borrowLeaf bd bs (motionOnly bxs))) aElms)
