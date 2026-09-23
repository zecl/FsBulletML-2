/// 手書き の弾幕 を、その場 で 少し 変える。
/// 数 を振る 側 は 式 に倍率 を掛け直す。形 を足す 側 は 印 を付けて 外せる。
module FsBulletML2.Tune

open System
open FsBulletML2

/// つまみ。対 で持つ —— 押し戻せない つまみ は 試せない
type Knob =
  /// 弾 が速く なる
  | Faster
  | Slower
  /// 間 が詰まって 弾 が増える
  | Denser
  | Sparser
  /// 撃つ 向き の刻み が大きく なる
  | Spinnier
  | Steadier
  /// 腕 が増える。刻み を逆 に振る ので リング は閉じた まま
  | MoreArms
  | FewerArms
  /// 弾 の加速 が強く なる
  | MoreAccel
  | LessAccel
  /// 向き や 速さ の変化 が 短い コマ で終わる
  | Snappier
  | Lazier
  /// 並行 の層 を 1 つ 足す / 外す。要素 が増える
  | AddLayer
  | DropLayer
  /// 弾 が割れる 段 を 1 つ 足す / 外す。要素 が増える
  | AddSplit
  | DropSplit
  /// 終点 の弾 に 寿命 を付ける / 外す。
  /// `Sparser`（待ち を伸ばす）では 1 回 の塊 が減らない。
  | Thinner
  | Thicker
  /// いちばん 深い 段 を 黙らせる / 戻す。
  ///
  /// 消さずに `repeat times 0` で包む ので、押し戻せば 戻る
  | Hush
  | Unhush

/// 軸 の名 から 上げ と 下げ の対 を引く。
/// 押す 側 と 頼む 側 が 同じ 表。2 か所 に書く と 片方 だけ 残る。
let axis (name: string) : (Knob * Knob) option =
  match name with
  | "speed" -> Some(Faster, Slower)
  | "density" -> Some(Denser, Sparser)
  | "spin" -> Some(Spinnier, Steadier)
  | "arms" -> Some(MoreArms, FewerArms)
  | "accel" -> Some(MoreAccel, LessAccel)
  | "term" -> Some(Snappier, Lazier)
  | "split" -> Some(AddSplit, DropSplit)
  | "layer" -> Some(AddLayer, DropLayer)
  | "thin" -> Some(Thinner, Thicker)
  | "hush" -> Some(Hush, Unhush)
  | _ -> None

/// 形 を足す 軸。先 に当てる —— あと から 数 を振る と、
/// 足した 段 や 層 にも 同じ 倍率 が掛かる（別 の扱い に しない）
let private STRUCTURAL = [ "split"; "layer" ]

/// 振る 順。形 を足す 側 が先、減らす 側 が後。
/// 寿命 を先 に乗せ ないと 足した 段 が消える。先 に黙らせる と `times 0` に 倍率 が掛かる。
let order (name: string) =
  if List.contains name STRUCTURAL then 0
  elif name = "hush" then 2
  elif name = "thin" then 3
  else 1

/// 1 段 の倍率。上げ と 下げ は 逆数 —— 押し戻す と 元 の字 に戻る
let [<Literal>] private STEP = 1.25

/// 掛けた 数 を丸める 桁。`1.25 * 0.8` が 1.0 に戻らない と、
/// 押し戻して も `(元) * 1.0000000000000002` が残る
let [<Literal>] private DIGITS = 6

let private fmt (v: float) =
  let s = Math.Round(v, DIGITS).ToString("0.######", Globalization.CultureInfo.InvariantCulture)
  s

/// `(中身) * 1.25` の形 なら 中身 と 倍率 に割る。
/// 開き 括弧 が その閉じ で閉じる ことを 数える。頭 の括弧 だけ 拾う と 別 の式 を読む。
let private split (s: string) : (string * float) option =
  if s.Length < 2 || s.[0] <> '(' then None
  else
    let mutable depth = 0
    let mutable close' = -1
    let mutable i = 0
    while close' < 0 && i < s.Length do
      match s.[i] with
      | '(' -> depth <- depth + 1
      | ')' ->
        depth <- depth - 1
        if depth = 0 then close' <- i
      | _ -> ()
      i <- i + 1
    if close' < 0 then None
    else
      let tail = s.Substring(close' + 1)
      if not (tail.StartsWith " * ") then None
      else
        match Double.TryParse(tail.Substring 3, Globalization.NumberStyles.Float,
                              Globalization.CultureInfo.InvariantCulture) with
        | true, v -> Some(s.Substring(1, close' - 1), v)
        | _ -> None

/// 式 に 倍率 を掛ける。既 に掛かって いれば 掛け直す
let private scale (k: float) (e: Expr.NumExpr) : Expr.NumExpr =
  let inner, had =
    match split (Expr.NumExpr.text e) with
    | Some(i, v) -> i, v
    | None -> Expr.NumExpr.text e, 1.0
  let v = Math.Round(had * k, DIGITS)
  if v = 1.0 then Expr.NumExpr.ofString inner
  else Expr.NumExpr.ofString (sprintf "(%s) * %s" inner (fmt v))

/// どの `<direction>` を振る か。`absolute` と `aim` は振らない ——
/// あれ は「どこ を向く か」で、掛ける と 狙い が別 の方 を指す。
/// `sequence` と `relative` は「前 から 何度 ずらす か」なので 振れる
let private spinnable (d: Direction) =
  match d with
  | Direction(Some a, _) ->
    a.directionType = DirectionType.Sequence || a.directionType = DirectionType.Relative
  | Direction(None, _) -> false

let private mapDir k (d: Direction) =
  match d with
  | Direction(a, e) -> Direction(a, if spinnable d then scale k e else e)

let private mapSpeed k (Speed(a, e)) = Speed(a, scale k e)

let private mapTimes k (Times e) = Times(scale k e)

let private mapTerm k (Term e) = Term(scale k e)

let private mapAccel k (h: Horizontal option) (v: Vertical option) =
  let mh = h |> Option.map (fun (Horizontal(a, e)) -> Horizontal(a, scale k e))
  let mv = v |> Option.map (fun (Vertical(a, e)) -> Vertical(a, scale k e))
  mh, mv

/// つまみ ごと に、どの 欄 を どちら向き に振る か。
///
/// `None` は「触らない」。1 つ の欄 を 2 つ の つまみ が同じ 向き に振る と、
/// 押した とき の手応え が 区別 できなくなる
let private factor (knob: Knob) (slot: string) : float option =
  let up = STEP
  let down = 1.0 / STEP
  match knob, slot with
  | Faster, "speed" -> Some up
  | Slower, "speed" -> Some down
  // 「密に」は 待ち を短く する こと。逆数 で振る
  | Denser, "wait" -> Some down
  | Sparser, "wait" -> Some up
  | Spinnier, "direction" -> Some up
  | Steadier, "direction" -> Some down
  // 腕 を増やす と 刻み は細かく なる。リング が閉じた まま 本数 だけ 増える
  | MoreArms, "times" -> Some up
  | MoreArms, "direction" -> Some down
  | FewerArms, "times" -> Some down
  | FewerArms, "direction" -> Some up
  | MoreAccel, "accel" -> Some up
  | LessAccel, "accel" -> Some down
  // 「変化 を速く」は 掛かる コマ数 を短く する こと
  | Snappier, "term" -> Some down
  | Lazier, "term" -> Some up
  | _ -> None

let private on knob slot (f: float -> 'a) (unchanged: 'a) =
  match factor knob slot with
  | Some k -> f k
  | None -> unchanged

let rec private mapAction (knob: Knob) (a: Action) : Action =
  match a with
  | Action.ChangeDirection(d, t) ->
    let d' = on knob "direction" (fun k -> mapDir k d) d
    let t' = on knob "term" (fun k -> mapTerm k t) t
    Action.ChangeDirection(d', t')
  | Action.ChangeSpeed(s, t) ->
    let s' = on knob "speed" (fun k -> mapSpeed k s) s
    let t' = on knob "term" (fun k -> mapTerm k t) t
    Action.ChangeSpeed(s', t')
  | Action.Accel(h, v, t) ->
    let h', v' = on knob "accel" (fun k -> mapAccel k h v) (h, v)
    let t' = on knob "term" (fun k -> mapTerm k t) t
    Action.Accel(h', v', t')
  | Action.Wait e -> Action.Wait(on knob "wait" (fun k -> scale k e) e)
  | Action.Repeat(t, e) ->
    Action.Repeat(on knob "times" (fun k -> mapTimes k t) t, mapActionElm knob e)
  | Action.Fire(attrs, d, s, b) ->
    Action.Fire(attrs, mapDirOpt knob d, mapSpeedOpt knob s, mapBullet knob b)
  | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (mapAction knob) xs)
  | _ -> a

and private mapDirOpt knob (d: Direction option) =
  d |> Option.map (fun x -> on knob "direction" (fun k -> mapDir k x) x)

and private mapSpeedOpt knob (s: Speed option) =
  s |> Option.map (fun x -> on knob "speed" (fun k -> mapSpeed k x) x)

and private mapActionElm knob (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (mapAction knob) xs)
  | ActionElm.ActionRef _ -> e

and private mapBullet knob (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    BulletElm.Bullet(attrs, mapDirOpt knob d, mapSpeedOpt knob s, List.map (mapActionElm knob) xs)
  | BulletElm.BulletRef _ -> b

let private mapTop knob (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    BulletmlElm.Bullet(attrs, mapDirOpt knob d, mapSpeedOpt knob s, List.map (mapActionElm knob) xs)
  | BulletmlElm.Fire(attrs, d, s, b) ->
    BulletmlElm.Fire(attrs, mapDirOpt knob d, mapSpeedOpt knob s, mapBullet knob b)
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (mapAction knob) xs)

// --- 形 を足す 側 --------------------------------------------------------
// 足す 要素 に 印 を付ける。外す 側 は その印 だけ を見る。

/// 足した 段 の印。`<action label="...">` に置く
let [<Literal>] SPLIT_MARK = "tuned-split"

/// 足した 層 の印。`top` で始める —— 走らせる 側 は
/// 名前 が `top` で始まる `action` を 並行 に走らせる
let [<Literal>] private LAYER_PREFIX = "tuned-top"

/// 足せる 段 の数。掛け算 で増える ので、青天井 にすると
/// 3 段 目 で 数千発 になって 面 が止まる
let [<Literal>] private MAX_SPLIT = 3

/// 足せる 層 の数
let [<Literal>] private MAX_LAYER = 3

let private num (s: string) = Expr.NumExpr.ofString s

let private labelOf (e: BulletmlElm) =
  match e with
  | BulletmlElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> Some l
  | _ -> None

let private isTunedLayer (e: BulletmlElm) =
  match labelOf e with
  | Some l -> l.StartsWith LAYER_PREFIX
  | None -> false

/// 走らせる 側 が 並行 に走らせる のは 名前 が `top` で始まる `action`。
/// 元 の弾幕 の 1 本 目 を 種 にする
let private firstTop (elms: BulletmlElm list) =
  elms
  |> List.tryFind (fun e ->
      match labelOf e with
      | Some l -> l.StartsWith "top" && not (l.StartsWith LAYER_PREFIX)
      | None -> false)

/// 層 を 1 つ 足す。種 の写し を 少し ずらして 置く ——
/// 同じ コマ に出す と 重なって 1 層 に見える。
/// 偶数 の層 は 刻み を逆 に振る（干渉縞）
let private addLayer (bulletml: Bulletml) : Bulletml =
  match bulletml with
  | Bulletml(attrs, elms) ->
    let existing = elms |> List.filter isTunedLayer |> List.length
    match firstTop elms with
    | Some(BulletmlElm.Action(_, body)) when existing < MAX_LAYER ->
      let n = existing + 1
      let spun = if n % 2 = 0 then List.map (mapAction Steadier >> mapAction Steadier) body else body
      let layer =
        BulletmlElm.Action(
          { actionLabel = Some(ActionLabel(sprintf "%s%d" LAYER_PREFIX n)) },
          Action.Wait(num (string (n * 20))) :: spun)
      Bulletml(attrs, elms @ [ layer ])
    | _ -> bulletml

let private dropLayer (bulletml: Bulletml) : Bulletml =
  match bulletml with
  | Bulletml(attrs, elms) ->
    match elms |> List.filter isTunedLayer with
    | [] -> bulletml
    | added ->
      let last = List.last added
      Bulletml(attrs, elms |> List.filter (fun e -> not (Object.ReferenceEquals(e, last))))

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

let private splitMarks (xs: ActionElm list) =
  xs
  |> List.filter (fun e ->
      match e with
      | ActionElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> l = SPLIT_MARK
      | _ -> false)

/// 終点 の弾 に「少し 飛んで から 割れて 消える」枝 を足す
let private splitBranch () =
  ActionElm.Action(
    { actionLabel = Some(ActionLabel SPLIT_MARK) },
    [ Action.Wait(num "30")
      Action.Repeat(
        Times(num "4"),
        ActionElm.Action(
          { actionLabel = None },
          [ Action.Fire(
              { fireLabel = None },
              Some(Direction(Some { directionType = DirectionType.Sequence }, num "90")),
              Some(Speed(None, num "1.2")),
              BulletElm.Bullet({ bulletLabel = None }, None, None, [])) ]))
      Action.Vanish ])

let rec private splitBullet (depth: int) (b: BulletElm) : BulletElm =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    let deeper = List.map (splitElm depth) xs
    // 撃つ 枝 が 1 つ も無い 弾 が 段 の終点。そこ に だけ 足す
    let leaf = not (List.exists (fun e -> firesInElm e) xs)
    if leaf && depth < MAX_SPLIT then BulletElm.Bullet(attrs, d, s, deeper @ [ splitBranch () ])
    else BulletElm.Bullet(attrs, d, s, deeper)
  | BulletElm.BulletRef _ -> b

and private splitAction depth (a: Action) : Action =
  match a with
  | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, d, s, splitBullet depth b)
  | Action.Repeat(t, e) -> Action.Repeat(t, splitElm depth e)
  | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (splitAction depth) xs)
  | _ -> a

and private splitElm depth (e: ActionElm) : ActionElm =
  match e with
  | ActionElm.Action({ actionLabel = Some(ActionLabel l) } as attrs, xs) when l = SPLIT_MARK ->
    // 印 の中 へ入る ときだけ 段 を 1 つ 数える
    ActionElm.Action(attrs, List.map (splitAction (depth + 1)) xs)
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (splitAction depth) xs)
  | ActionElm.ActionRef _ -> e

/// 根 の直下 の `<bullet label="...">` も 終点 になりうる。
/// `<bulletRef>` を辿らない ので、ここ で見ない と 1 段 も 足せない。
let private splitTop depth (e: BulletmlElm) : BulletmlElm =
  match e with
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    let deeper = List.map (splitElm depth) xs
    let marks = splitMarks xs |> List.length
    if List.exists firesInElm xs || depth + marks >= MAX_SPLIT then
      BulletmlElm.Bullet(attrs, d, s, deeper)
    else BulletmlElm.Bullet(attrs, d, s, deeper @ [ splitBranch () ])
  | BulletmlElm.Fire(attrs, d, s, b) -> BulletmlElm.Fire(attrs, d, s, splitBullet depth b)
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (splitAction depth) xs)

let private addSplit (bulletml: Bulletml) =
  match bulletml with
  | Bulletml(attrs, elms) -> Bulletml(attrs, List.map (splitTop 0) elms)

/// 弾 の中 から 印 を 1 つ 外す。中 に もっと 深い 印 が在れば そちら が先。
/// 外側 から 消す と 中 の段 ごと 消える。`<bulletRef>` の定義 側 も 同じ 手。
let rec private dropInside (xs: ActionElm list) : ActionElm list * bool =
  let mutable hit = false
  let ys =
    xs
    |> List.map (fun e ->
        let e', h = dropDeepestElm e
        hit <- hit || h
        e')
  if hit then ys, true
  else
    match splitMarks xs with
    | [] -> xs, false
    | marks ->
      let last = List.last marks
      xs |> List.filter (fun e -> not (Object.ReferenceEquals(e, last))), true

and private dropDeepest (b: BulletElm) : BulletElm * bool =
  match b with
  | BulletElm.Bullet(attrs, d, s, xs) ->
    let ys, hit = dropInside xs
    BulletElm.Bullet(attrs, d, s, ys), hit
  | BulletElm.BulletRef _ -> b, false

and private dropDeepestAction (a: Action) : Action * bool =
  match a with
  | Action.Fire(attrs, d, s, b) ->
    let b', h = dropDeepest b
    Action.Fire(attrs, d, s, b'), h
  | Action.Repeat(t, e) ->
    let e', h = dropDeepestElm e
    Action.Repeat(t, e'), h
  | Action.Action(attrs, xs) ->
    let mutable hit = false
    let ys = xs |> List.map (fun x -> let x', h = dropDeepestAction x in (hit <- hit || h); x')
    Action.Action(attrs, ys), hit
  | _ -> a, false

and private dropDeepestElm (e: ActionElm) : ActionElm * bool =
  match e with
  | ActionElm.Action(attrs, xs) ->
    let mutable hit = false
    let ys = xs |> List.map (fun x -> let x', h = dropDeepestAction x in (hit <- hit || h); x')
    ActionElm.Action(attrs, ys), hit
  | ActionElm.ActionRef _ -> e, false

let private dropSplit (bulletml: Bulletml) =
  match bulletml with
  | Bulletml(attrs, elms) ->
    let top (e: BulletmlElm) =
      match e with
      // 定義 の弾 も 終点 になりうる（`<bulletRef>` で撃つ 弾幕）。
      // 撃つ 場所 と同じ 手 を通す
      | BulletmlElm.Bullet(a, d, s, xs) -> BulletmlElm.Bullet(a, d, s, fst (dropInside xs))
      | BulletmlElm.Fire(a, d, s, b) -> BulletmlElm.Fire(a, d, s, fst (dropDeepest b))
      | BulletmlElm.Action(a, xs) ->
        BulletmlElm.Action(a, xs |> List.map (fun x -> fst (dropDeepestAction x)))
    Bulletml(attrs, List.map top elms)

// --- 刈り込む 側 ----------------------------------------------------------

/// 黙らせた 枝 の印。段数 を後ろ に付ける（`LAYER_PREFIX` と同じ 手）
let [<Literal>] private HUSH_PREFIX = "tuned-hush"

let [<Literal>] private MAX_HUSH = 3

let private isHushed (a: Action) =
  match a with
  | Action.Action({ actionLabel = Some(ActionLabel l) }, _) -> l.StartsWith HUSH_PREFIX
  | _ -> false

/// `repeat` の `times` は `max 0` で丸める ので、0 なら 1 度 も 走らない。
/// 消さずに 包む —— 押し戻せば 戻る
let private husk (n: int) (inner: Action) =
  Action.Action(
    { actionLabel = Some(ActionLabel(sprintf "%s%d" HUSH_PREFIX n)) },
    [ Action.Repeat(Times(num "0"), ActionElm.Action({ actionLabel = None }, [ inner ])) ])

/// 根 から 何段 目 の `fire` か。黙らせた 枝 の中 は 数えない。
/// `actionRef` / `bulletRef` の先 は 辿らない ので、あちら には 効かない。
let rec private deepest (d: int) (a: Action) : int =
  if isHushed a then 0
  else
    match a with
    | Action.Fire(_, _, _, b) -> max (d + 1) (deepestInBullet (d + 1) b)
    | Action.Repeat(_, e) -> deepestInElm d e
    | Action.Action(_, xs) -> xs |> List.fold (fun m x -> max m (deepest d x)) 0
    | _ -> 0

and private deepestInElm (d: int) (e: ActionElm) =
  match e with
  | ActionElm.Action(_, xs) -> xs |> List.fold (fun m x -> max m (deepest d x)) 0
  | ActionElm.ActionRef _ -> 0

and private deepestInBullet (d: int) (b: BulletElm) =
  match b with
  | BulletElm.Bullet(_, _, _, xs) -> xs |> List.fold (fun m x -> max m (deepestInElm d x)) 0
  | BulletElm.BulletRef _ -> 0

let rec private hushAt (target: int) (n: int) (d: int) (a: Action) : Action =
  if isHushed a then a
  else
    match a with
    | Action.Fire(_, _, _, _) when d + 1 = target -> husk n a
    | Action.Fire(attrs, dir, s, b) -> Action.Fire(attrs, dir, s, hushInBullet target n (d + 1) b)
    | Action.Repeat(t, e) -> Action.Repeat(t, hushInElm target n d e)
    | Action.Action(attrs, xs) -> Action.Action(attrs, List.map (hushAt target n d) xs)
    | _ -> a

and private hushInElm target n d (e: ActionElm) =
  match e with
  | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map (hushAt target n d) xs)
  | ActionElm.ActionRef _ -> e

and private hushInBullet target n d (b: BulletElm) =
  match b with
  | BulletElm.Bullet(attrs, dir, s, xs) ->
    BulletElm.Bullet(attrs, dir, s, List.map (hushInElm target n d) xs)
  | BulletElm.BulletRef _ -> b

let private hushTop target n (e: BulletmlElm) =
  match e with
  | BulletmlElm.Action(attrs, xs) -> BulletmlElm.Action(attrs, List.map (hushAt target n 0) xs)
  | BulletmlElm.Bullet(attrs, d, s, xs) ->
    BulletmlElm.Bullet(attrs, d, s, List.map (hushInElm target n 0) xs)
  | BulletmlElm.Fire(attrs, d, s, b) ->
    BulletmlElm.Fire(attrs, d, s, hushInBullet target n 1 b)

/// 印 の数 を数える。次 の段数 と、外す 先 を決める。
/// 印 の中 へも 降りる。外側 で止める と 数 が 1 つ 足りず、外す 先 が ずれる。
let private hushCount (bulletml: Bulletml) =
  let mutable n = 0
  let rec inAction (a: Action) =
    if isHushed a then n <- n + 1
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
    | BulletElm.Bullet(_, _, _, xs) -> List.iter inElm xs
    | BulletElm.BulletRef _ -> ()
  match bulletml with
  | Bulletml(_, elms) ->
    for e in elms do
      match e with
      | BulletmlElm.Action(_, xs) -> List.iter inAction xs
      | BulletmlElm.Bullet(_, _, _, xs) -> List.iter inElm xs
      | BulletmlElm.Fire(_, _, _, b) -> inBullet b
  n

/// いちばん 深い `fire` を 黙らせる。深さ 1 は 触らない ——
/// 根 の `fire` を黙らせる と 弾幕 が丸ごと 止まる
let private hush (bulletml: Bulletml) =
  match bulletml with
  | Bulletml(attrs, elms) ->
    let target = elms |> List.fold (fun m e ->
                    match e with
                    | BulletmlElm.Action(_, xs) -> xs |> List.fold (fun m2 x -> max m2 (deepest 0 x)) m
                    | _ -> m) 0
    let n = hushCount bulletml + 1
    if target <= 1 || n > MAX_HUSH then bulletml
    else Bulletml(attrs, List.map (hushTop target n) elms)

/// 印 を 1 つ 外す。最後 に付けた もの から ——
/// 浅い ほう から 外す と、深い 印 が 中 に残った まま 段 が戻る
let private unhush (bulletml: Bulletml) =
  let last = hushCount bulletml
  if last = 0 then bulletml
  else
    let name = sprintf "%s%d" HUSH_PREFIX last
    let strip (a: Action) =
      match a with
      | Action.Action({ actionLabel = Some(ActionLabel l) },
                      [ Action.Repeat(_, ActionElm.Action(_, [ inner ])) ]) when l = name -> inner
      | _ -> a
    let rec inAction (a: Action) =
      let a = strip a
      match a with
      | Action.Fire(attrs, d, s, b) -> Action.Fire(attrs, d, s, inBullet b)
      | Action.Repeat(t, e) -> Action.Repeat(t, inElm e)
      | Action.Action(attrs, xs) -> Action.Action(attrs, List.map inAction xs)
      | _ -> a
    and inElm (e: ActionElm) =
      match e with
      | ActionElm.Action(attrs, xs) -> ActionElm.Action(attrs, List.map inAction xs)
      | ActionElm.ActionRef _ -> e
    and inBullet (b: BulletElm) =
      match b with
      | BulletElm.Bullet(attrs, d, s, xs) -> BulletElm.Bullet(attrs, d, s, List.map inElm xs)
      | BulletElm.BulletRef _ -> b
    match bulletml with
    | Bulletml(attrs, elms) ->
      let top (e: BulletmlElm) =
        match e with
        | BulletmlElm.Action(a, xs) -> BulletmlElm.Action(a, List.map inAction xs)
        | BulletmlElm.Bullet(a, d, s, xs) -> BulletmlElm.Bullet(a, d, s, List.map inElm xs)
        | BulletmlElm.Fire(a, d, s, b) -> BulletmlElm.Fire(a, d, s, inBullet b)
      Bulletml(attrs, List.map top elms)

// --- 間引く 側 ------------------------------------------------------------

/// 足した 寿命 の印
let [<Literal>] THIN_MARK = "tuned-thin"

/// 終点 の弾 が 消える まで の コマ。`Combine.LIFE` と同じ 数 で、口 が違う
let [<Literal>] private THIN_LIFE = "60"

let private isThin (e: ActionElm) =
  match e with
  | ActionElm.Action({ actionLabel = Some(ActionLabel l) }, _) -> l = THIN_MARK
  | _ -> false

let private thinBranch () =
  ActionElm.Action(
    { actionLabel = Some(ActionLabel THIN_MARK) },
    [ Action.Wait(num THIN_LIFE); Action.Vanish ])

/// 印 の中 の 寿命 を振る。1 段 目 は 枝 を足す だけ で 倍率 を掛けない ので、
/// 素 の値 の まま 濃く する と 枝 ごと 外れる
let private tweak (k: float) (e: ActionElm) : ActionElm option =
  match e with
  | ActionElm.Action(attrs, xs) when isThin e ->
    let bare =
      xs |> List.exists (fun a ->
        match a with Action.Wait w -> Expr.NumExpr.text w = THIN_LIFE | _ -> false)
    if bare && k > 1.0 then None
    else
      let ys =
        xs |> List.map (fun a -> match a with Action.Wait w -> Action.Wait(scale k w) | _ -> a)
      Some(ActionElm.Action(attrs, ys))
  | _ -> Some e

/// 印 が無い 弾 に `Thicker` を当てて も 何 も しない ——
/// 「長く 生きる ように する」が 寿命 を生やす と 逆 に弾 が消える
let private thinLeaf (k: float) : Combine.OnLeaf =
  fun attrs d s xs ->
    if xs |> List.exists isThin then
      BulletElm.Bullet(attrs, d, s, xs |> List.choose (tweak k))
    elif k < 1.0 then
      BulletElm.Bullet(attrs, d, s, xs @ [ thinBranch () ])
    else
      BulletElm.Bullet(attrs, d, s, xs)

let private thin (k: float) (bulletml: Bulletml) =
  match bulletml with
  | Bulletml(attrs, elms) -> Bulletml(attrs, List.map (Combine.atLeafTop (thinLeaf k)) elms)

/// 1 段 振った 弾幕 を返す。
/// 数 を振る つまみ は 要素 を増やさない。形 を足す つまみ は 印 付き を 1 つ 足す / 外す。
let apply (knob: Knob) (bulletml: Bulletml) : Bulletml =
  match knob with
  | AddLayer -> addLayer bulletml
  | DropLayer -> dropLayer bulletml
  | AddSplit -> addSplit bulletml
  | DropSplit -> dropSplit bulletml
  | Thinner -> thin (1.0 / STEP) bulletml
  | Thicker -> thin STEP bulletml
  | Hush -> hush bulletml
  | Unhush -> unhush bulletml
  | _ ->
    match bulletml with
    | Bulletml(attrs, elms) -> Bulletml(attrs, List.map (mapTop knob) elms)

/// 軸 と 段数 を 1 回 で 当てる。途中 で落ちる と 中途半端 な形 で止まる。
/// 知らない 軸 は 飛ばす。段数 の上限 は ここ（`MAX_STEPS`）。
let [<Literal>] MAX_STEPS = 4

let applySteps (steps: (string * int) list) (bulletml: Bulletml) : Bulletml =
  steps
  |> List.choose (fun (name, n) ->
      match axis name with
      | Some pair when n <> 0 -> Some(order name, pair, n)
      | _ -> None)
  |> List.sortBy (fun (o, _, _) -> o)
  |> List.fold
      (fun acc (_, (up, down), n) ->
        let knob = if n > 0 then up else down
        let times = min MAX_STEPS (abs n)
        List.fold (fun x _ -> apply knob x) acc [ 1 .. times ])
      bulletml
