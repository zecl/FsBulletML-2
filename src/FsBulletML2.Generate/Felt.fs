/// 生成 した 弾幕 を走らせて、形 の「感じ」を数 にする
/// 見て いる のは「形 が違えば 数 が割れる か」だけ。閾値 は呼ぶ側 が持つ
module FsBulletML2.Generate.Felt

open FsBulletML2
open FsBulletML2.Domain

type Snapshot =
  { Frame: int
    /// そのコマ に生きて いる 弾 の (x, y)。敵 本体 は入れず、面 の外 に出た 弾 は間引く ——
    /// 長さ が そのまま 同時数 になる
    Positions: (float * float) list
    /// `Positions` と同じ 順 の、その 弾 が生まれた コマ
    Born: int list
    /// そのコマ に撃たれた 弾 の発射角（rad、0..2π に畳む）
    Headings: float list
    /// `Headings` と同じ 順 の、撃たれた 弾 の速さ
    Speeds: float list }

let [<Literal>] PlayerX = 240.0f
let [<Literal>] PlayerY = 600.0f
let [<Literal>] EnemyX = 240.0f
let [<Literal>] EnemyY = 80.0f
let [<Literal>] FieldW = 480.0f
let [<Literal>] FieldH = 640.0f

let private twoPi = 2.0 * System.Math.PI

/// 0..2π に畳む。Core の `calcDir` は 1 周 しか 戻さず、sequence の角 は 波 ごと に伸び続ける
let private wrap (a: float) =
  let r = a % twoPi
  let r = if r < 0.0 then r + twoPi else r
  if r >= twoPi then 0.0 else r

let private aimDirAt (player: Vec2) (x: float32) (y: float32) =
  float32 (System.Math.Atan2(float (player.X - x), float -(player.Y - y)))

/// 撃たれた 弾 は 親 の位置 に産まれる（Step の `child.Pos = self.Pos`）ので、Spawn も 親 の位置 から組む
let private envOf (player: Vec2) (rand: unit -> float32) (rank: float32) (run: BulletRun) (p: Vec2) : Env =
  if run.HasNoScript then
    { Rand = rand; Rank = rank
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }
  else
    let a = aimDirAt player p.X p.Y
    { Rand = rand; Rank = rank
      Aim = { ToPlayer = a; ToEnemy = 0.0f }
      Spawn = { ToPlayer = a; ToEnemy = 0.0f } }

/// 走らせ直し の Env。面 の `Driver.restart` は位置 を取らず aim を 0 で渡す
let private noAim : Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
let private noSpawn : SpawnAim = { ToPlayer = 0.0f; ToEnemy = 0.0f }

let private inside (p: Vec2) =
  p.X >= 0.0f && p.X <= FieldW && p.Y >= 0.0f && p.Y <= FieldH

/// rand=0.5 固定。敵 は (240, 80) に居て 動かない。
/// 自機 (playerX, playerY) も動かず、そこ への Aim を毎コマ 弾 ごと に組む
let runWithRank (rank: float32) (playerX: float) (playerY: float) (frames: int) (bulletml: Bulletml) : Snapshot list =
  let envOf = envOf { X = float32 playerX; Y = float32 playerY }
  let rand () = 0.5f
  let script = Runner.load rand rank bulletml
  let enemy = { X = EnemyX; Y = EnemyY }
  let mutable root = Runner.newRoot BulletType.Enemy script
  let mutable rootAlive = true
  let mutable live = ResizeArray<struct (BulletRun * FsBulletML2.Motion * int)>()
  let acc = ResizeArray<Snapshot>()
  for frame in 1 .. frames do
    let next = ResizeArray<struct (BulletRun * FsBulletML2.Motion * int)>()
    let pos = ResizeArray<float * float>()
    let born = ResizeArray<int>()
    let heads = ResizeArray<float>()
    let speeds = ResizeArray<float>()
    // Core は位置 を積分 しない。産まれた 弾 の Pos は 撃った 側 の そのコマ の位置
    let spawn (children: BulletRun list) =
      for child in children do
        let m = child.Motion
        next.Add(struct (child, m, frame))
        heads.Add(wrap (float m.Dir))
        speeds.Add(float m.Speed)
    // 台本 を終えた 弾 は頭 から走らせ直す。面（Playfield）が根 も撃たれた 弾 も そうして いる
    // 走らせ直さない と 面 と振る舞い が割れ、面 で届かない 弾幕 でも 試験 が緑 になる
    let again (f: Frame) =
      if f.Finished then Runner.restart { Rand = rand; Rank = rank; Aim = noAim; Spawn = noSpawn } f.Run
      else f.Run
    if rootAlive then
      let f = Runner.stepWith (envOf rand rank root enemy) root { root.Motion with Pos = enemy }
      root <- again f
      if f.Vanished then rootAlive <- false
      spawn f.Spawned
    for struct (r, mot, b) in live do
      let f = Runner.stepWith (envOf rand rank r mot.Pos) r mot
      let p = { X = mot.Pos.X + f.Delta.X; Y = mot.Pos.Y + f.Delta.Y }
      if not f.Vanished && not f.Retired && inside p then
        next.Add(struct (again f, { f.Run.Motion with Pos = p }, b))
        pos.Add(float p.X, float p.Y)
        born.Add b
      spawn f.Spawned
    live <- next
    acc.Add
      { Frame = frame
        Positions = List.ofSeq pos
        Born = List.ofSeq born
        Headings = List.ofSeq heads
        Speeds = List.ofSeq speeds }
  List.ofSeq acc

/// rank=1.0 の `runWithRank`
let runWith (playerX: float) (playerY: float) (frames: int) (bulletml: Bulletml) : Snapshot list =
  runWithRank 1.0f playerX playerY frames bulletml

/// 自機 を (240, 600) に置いた `runWithRank`
let runAt (rank: float32) (frames: int) (bulletml: Bulletml) : Snapshot list =
  runWithRank rank (float PlayerX) (float PlayerY) frames bulletml

/// 自機 を (240, 600) に置いた `runWith`
let run (frames: int) (bulletml: Bulletml) : Snapshot list =
  runWith (float PlayerX) (float PlayerY) frames bulletml

/// 生まれた ばかり の弾 を 半径 `radius` で束ねた 群 の、中心 と 発数
/// `Positions` は 1 コマ 進んだ 後 の並び なので、見る のは `Born = Frame - 1`
let bloomCenters (radius: float) (s: Snapshot) : ((float * float) * int) list =
  let pts =
    List.zip s.Positions s.Born
    |> List.filter (fun (_, b) -> b = s.Frame - 1)
    |> List.map fst
    |> Array.ofList
  if pts.Length = 0 then []
  else
    let parent = Array.init pts.Length id
    let rec find i = if parent.[i] = i then i else (parent.[i] <- find parent.[i]; parent.[i])
    for i in 0 .. pts.Length - 1 do
      for j in i + 1 .. pts.Length - 1 do
        let (xi, yi), (xj, yj) = pts.[i], pts.[j]
        if (xi - xj) * (xi - xj) + (yi - yj) * (yi - yj) <= radius * radius then
          let a, b = find i, find j
          if a <> b then parent.[a] <- b
    pts
    |> Array.indexed
    |> Array.groupBy (fst >> find)
    |> Array.map (fun (_, g) ->
        (g |> Array.averageBy (fun (_, (x, _)) -> x), g |> Array.averageBy (fun (_, (_, y)) -> y)), g.Length)
    |> List.ofArray

let private variance (xs: float[]) =
  let m = Array.average xs
  xs |> Array.averageBy (fun x -> (x - m) * (x - m))

let private median (xs: float list) =
  let a = xs |> List.sort |> Array.ofList
  let n = a.Length
  if n % 2 = 1 then a.[n / 2] else (a.[n / 2 - 1] + a.[n / 2]) / 2.0

/// a から b への 最短 の符号付き 角（-π..π）
let private shortest (a: float) (b: float) =
  let d = (b - a) % twoPi
  if d > System.Math.PI then d - twoPi
  elif d <= -System.Math.PI then d + twoPi
  else d

/// 波 を跨いだ 発射角 の中央値 が 同じ 向き に回って いる 割合。1 に近いほど 渦
/// 撃った コマ だけ を並べ、隣 との差 を数える。差 0 は 回って いない 側 に数える（分母 には入る）
let rotationScore (snaps: Snapshot list) : float =
  let diffs =
    snaps
    |> List.filter (fun s -> not s.Headings.IsEmpty)
    |> List.map (fun s -> median s.Headings)
    |> List.pairwise
    |> List.map (fun (a, b) -> shortest a b)
  if diffs.IsEmpty then 0.0
  else
    let still = 1e-6
    let up = diffs |> List.filter (fun d -> d > still) |> List.length
    let down = diffs |> List.filter (fun d -> d < -still) |> List.length
    float (max up down) / float diffs.Length

/// 同じ コマ に生まれた 群 の、y の分散 が x の分散 より 小さい ほど 1。1 に近いほど 幕
/// 測る のは 生まれて 8 コマ 以上・8 発 以上 残る 群 のうち いちばん 大きい 1 つ（無ければ 0）
let bandScore (s: Snapshot) : float =
  let groups =
    List.zip s.Positions s.Born
    |> List.filter (fun (_, b) -> s.Frame - b >= 8)
    |> List.groupBy snd
    |> List.filter (fun (_, xs) -> xs.Length >= 8)
  if groups.IsEmpty then 0.0
  else
    let _, best = groups |> List.maxBy (fun (_, xs) -> xs.Length)
    let vx = best |> List.map (fun ((x, _), _) -> x) |> Array.ofList |> variance
    let vy = best |> List.map (fun ((_, y), _) -> y) |> Array.ofList |> variance
    if vx + vy < 1e-9 then 0.0
    else 1.0 - min 1.0 (vy / vx)

/// 並べた 発射角 の 最大 の隙間（0 と 2π の継ぎ目 を含む）
let private maxGapOf (hs: float list) =
  let a = hs |> List.map wrap |> List.sort |> Array.ofList
  let wrapGap = a.[0] + twoPi - a.[a.Length - 1]
  Seq.fold max wrapGap (Seq.pairwise a |> Seq.map (fun (p, q) -> q - p))

/// 等間隔 の隙間 2π/n（n は重複 込み の本数）を 最大 の隙間 で割った 値。2 本 未満 は 0。1 に近いほど 放射
let ringScore (s: Snapshot) : float =
  if s.Headings.Length < 2 then 0.0
  else min 1.0 (twoPi / float s.Headings.Length / maxGapOf s.Headings)

/// すべて の発射角 を含む 最小 の弧 の幅
let private spanOf (hs: float list) = twoPi - maxGapOf hs

/// 発射角 の平均 合成 長（1 - 円分散）。開き が半周 以上 なら 0。1 に近いほど 扇
let fanScore (s: Snapshot) : float =
  if s.Headings.IsEmpty then 0.0
  elif spanOf s.Headings >= System.Math.PI then 0.0
  else
    let c = s.Headings |> List.averageBy cos
    let n = s.Headings |> List.averageBy sin
    sqrt (c * c + n * n)

/// 敵 (240, 80) から見た 自機 方位 と 発射角 の差 の cos（負 は 0）の平均。1 に近いほど 狙い
let aimScore (playerX: float) (playerY: float) (s: Snapshot) : float =
  if s.Headings.IsEmpty then 0.0
  else
    let target = System.Math.Atan2(playerX - float EnemyX, -(playerY - float EnemyY))
    s.Headings |> List.averageBy (fun h -> max 0.0 (cos (h - target)))

/// 3 元 の連立 を掃き出し で解く。枢軸 が潰れたら None
let private solve3 (a: float[,]) (b: float[]) : float[] option =
  let m = Array2D.init 3 4 (fun i j -> if j < 3 then a.[i, j] else b.[i])
  let mutable ok = true
  for c in 0 .. 2 do
    if ok then
      let p = [ c .. 2 ] |> List.maxBy (fun r -> abs m.[r, c])
      if abs m.[p, c] < 1e-12 then ok <- false
      else
        for j in 0 .. 3 do
          let t = m.[c, j]
          m.[c, j] <- m.[p, j]
          m.[p, j] <- t
        for r in 0 .. 2 do
          if r <> c then
            let k = m.[r, c] / m.[c, c]
            for j in c .. 3 do
              m.[r, j] <- m.[r, j] - k * m.[c, j]
  if ok then Some [| for i in 0 .. 2 -> m.[i, 3] / m.[i, i] |] else None

/// 速さ を `f` で写した 値 を `a + b sin kθ + c cos kθ` に最小二乗 で当てた 決定係数 R²
/// 写した 値 の分散 が ~0 なら 0（0/0 の NaN は どの 比較 も偽 になる）
let fitScore (k: int) (f: float -> float) (s: Snapshot) : float =
  let pts =
    List.zip s.Headings s.Speeds
    |> List.filter (fun (_, v) -> System.Double.IsFinite(f v))
    |> Array.ofList
  if pts.Length < 3 then 0.0
  else
    let ys = pts |> Array.map (snd >> f)
    let sst = variance ys * float ys.Length
    if sst < 1e-12 then 0.0
    else
      let row (t: float) = [| 1.0; sin (float k * t); cos (float k * t) |]
      let rows = pts |> Array.map (fst >> row)
      let a = Array2D.init 3 3 (fun i j -> rows |> Array.sumBy (fun r -> r.[i] * r.[j]))
      let b = Array.init 3 (fun i -> Array.fold2 (fun acc (r: float[]) y -> acc + r.[i] * y) 0.0 rows ys)
      match solve3 a b with
      | None -> 0.0
      | Some w ->
          let sse =
            Array.fold2 (fun acc (r: float[]) y ->
              let e = y - (w.[0] * r.[0] + w.[1] * r.[1] + w.[2] * r.[2])
              acc + e * e) 0.0 rows ys
          max 0.0 (min 1.0 (1.0 - sse / sst))

/// 速さ そのもの の当てはまり。花（`r0 + A sin kθ`）は 1 に張り付く
let foldScore (k: int) (s: Snapshot) : float = fitScore k id s

/// 速さ の逆数 の当てはまり。星 は 逆数 が ちょうど 正弦 なので 1 に張り付く
/// 単独 では 割れない（浅い 花 も高い）。星 を花 から 剥がす のは `recipScore - foldScore` の符号
let recipScore (k: int) (s: Snapshot) : float =
  fitScore k (fun v -> if abs v < 1e-6 then nan else 1.0 / v) s

/// 走行 を 絵 にする。形 を 目 で見る ためだけ の 口 で、門 は ここ を通らない
module Draw =

  /// いちばん 多く 撃った コマ。1 波 が まるごと そこ に 出る
  let fullest (snaps: Snapshot list) : Snapshot = snaps |> List.maxBy (fun s -> s.Speeds.Length)

  /// 撃った 弾 を 向き と 速さ から 点 に する。0 rad が 真上（y は 下向き）
  /// `Positions` は 使わない（上 に飛んだ 弾 は すぐ 間引かれる）。同じ コマ の弾 は 速さ の比 が 半径 の比
  let outline (s: Snapshot) : (float * float) list =
    List.zip s.Headings s.Speeds |> List.map (fun (h, v) -> v * sin h, -(v * cos h))

  let [<Literal>] private CELL = 200.0

  /// 倍率 は 枡 ごと に取る。揃える と 速い 札 だけ が 枠 いっぱい に なり、遅い 札 の 輪郭 が 潰れる
  let private cell (ox: float) (oy: float) (label: string) (s: Snapshot) =
    let c = CELL / 2.0
    let pts = outline s
    let span =
      match pts with
      | [] -> 1.0
      | ps -> ps |> List.collect (fun (x, y) -> [ abs x; abs y ]) |> List.max |> max 1e-6
    let k = (c - 12.0) / span
    let dots =
      pts
      |> List.map (fun (x, y) -> sprintf "<circle cx=\"%.1f\" cy=\"%.1f\" r=\"1.6\"/>" (ox + c + x * k) (oy + c + y * k))
      |> String.concat ""
    sprintf
      "<g><rect x=\"%.1f\" y=\"%.1f\" width=\"%.1f\" height=\"%.1f\" fill=\"none\" stroke=\"#ccc\"/>%s<text x=\"%.1f\" y=\"%.1f\" font-size=\"11\" font-family=\"monospace\">%s (%d / %.2f)</text></g>"
      ox oy CELL CELL dots (ox + 6.0) (oy + 14.0) label pts.Length span

  /// `cols` 枚 ずつ 折り返して 1 枚 に並べる
  let grid (cols: int) (cells: (string * Snapshot) list) : string =
    let cols = max 1 cols
    let rows = (cells.Length + cols - 1) / cols
    let w = float cols * CELL
    let hgt = float (max 1 rows) * CELL
    let body =
      cells
      |> List.mapi (fun i (label, s) -> cell (float (i % cols) * CELL) (float (i / cols) * CELL) label s)
      |> String.concat ""
    sprintf
      "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"%.0f\" height=\"%.0f\" viewBox=\"0 0 %.0f %.0f\"><rect width=\"%.0f\" height=\"%.0f\" fill=\"#fff\"/>%s</svg>\n"
      w hgt w hgt w hgt body
