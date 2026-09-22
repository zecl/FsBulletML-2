namespace FsBulletML2.Generate

open System
open FsBulletML2
open FsBulletML2.Dsl
open FsBulletML2.Generate.Consts

/// 根。`Radial` は 放射、`Bar` は 止まった 発射台 が 回りながら 直線 を 敷く、`Fan` は 自機 の 方 へ 幅 120° の 扇。
/// 腕 の 名前 が `PatternSpec` の `Radial` と ぶつかる ので 型 名 で 書かせる
[<RequireQualifiedAccess>]
type Root =
  | Radial
  | Bar
  | Fan

/// 子 を 生む 遺伝子。`Trail` は 飛びながら 左右 へ 撒き、`Burst` は 止まって 横 へ 一列 撃って 消える
[<RequireQualifiedAccess>]
type Spawner =
  | Trail
  | Burst

/// 系譜 の 終わり。`Relaunch` は 止まって、待って、加速 する。`Fall` は 待って から 下 へ 引かれる
[<RequireQualifiedAccess>]
type Terminal =
  | Plain
  | Relaunch
  | Fall

type LineageAxes =
  { Root: Root
    /// 0 は 根 ごと の 既定
    Ways: int
    Speed: float
    Generations: int
    Streak: float
    Stillness: float
    Spread: float
    /// 1 以上 で 最後 の 世代 に 自機 を 狙い 直す 弾 が 混ざる
    Homing: float
    /// 0..2。Trail の 撒く 向き を 回す
    Drift: float
    /// 0..2。Trail の 糸 の 本数 1 / 2 / 3
    Strands: float
    /// 1 以上 で 終わり を 落ち に
    Fall: float
    /// 振り の 向き を 筋 と 波 で 入れ替える
    Alternate: bool
    Seed: int }

/// 待ち は `Base - Rank * $rank`、回数 は `Base + Rank * $rank`
type Ranked = { Base: int; Rank: int }

type LineageSpec =
  { Root: Root
    Ways: int
    /// `$rank = 0` の 速さ
    RootSpeed: float
    Chain: Spawner list
    Leaf: Terminal
    Waves: Ranked
    WaveWait: Ranked
    TrailTimes: int
    TrailWait: Ranked
    /// 一列 の 発数。先頭 の 1 発 を 除く
    Line: Ranked
    Columns: int
    /// 最後 の 世代 に 狙い 直す 弾 を 混ぜる
    Seekers: bool
    /// Trail が 1 本 の 間 に 回る 角（度）
    Sweep: float
    Strands: int
    /// 落ち の `accel vertical` の 行き先
    Gravity: float
    FallHold: int
    Alternate: bool
    Hold: int
    RelaunchHold: Ranked
    BarSteps: Ranked
    Seed: int }

module LineageSpec =

  let private onScale (v: float) = max 0.0 (min 2.0 v)
  let private step (v: float) = int (Math.Round v)

  let internal SPEED_HI = MAX_SPEED / 1.3
  let internal CHILD_SPEED = 1.0
  let internal BURST_SPEED = 1.3
  let internal BAR_LINE_SPEED = 1.5
  /// 止まる 段 が 飛ぶ 距離 の 合計（px）。1 段 ずつ 遠く へ 出る ので、段 が 多い ほど 1 段 を 短く する
  let internal FLY_SPAN = 180.0
  let internal BAR_WAIT = 5
  let internal RELAUNCH_SPEED = 2.5
  let internal RELAUNCH_TERM = 60
  /// 落ちる 葉 が 画面 の 下 を 抜ける まで の 距離（px）。縦 の 端 から 端
  let internal FALL_SPAN = 640.0
  let internal FALL_TERM = 120
  /// 真上 へ 撃たれた 葉 が 上がって よい 高さ。敵 の 高さ 80 から 余白 16
  let internal RISE_ROOM = 64.0
  let internal FALL_HOLD_MAX = 45

  /// `$rank = 1` で 同時 に 居る 弾 の 上界 を ここ まで 削る（面 の 天井 と 同じ）
  let BUDGET = 10000.0

  let private CHAIN_TABLE =
    let t, b = Spawner.Trail, Spawner.Burst
    [| [ t ]; [ b ]; [ t; b ]; [ b; t ]; [ b; b ]; [ t; b; b ]; [ b; t; b ]; [ b; b; t ]; [ b; b; b ] |]

  let zero : LineageAxes =
    { Root = Root.Radial
      Ways = 0
      Speed = 0.0
      Generations = 1
      Streak = 0.0
      Stillness = 0.0
      Spread = 0.0
      Homing = 0.0
      Drift = 0.0
      Strands = 0.0
      Fall = 0.0
      Alternate = false
      Seed = 0 }

  /// 回数 の `$rank = 1`
  let private at (r: Ranked) = float (r.Base + r.Rank)
  /// 待ち の `$rank = 1`
  let private at1 (r: Ranked) = float (r.Base - r.Rank)
  let private fast (v: float) = v * 1.3

  let strandMul (n: int) =
    match n with
    | 3 -> [ 0.5; 1.0; 1.5 ]
    | 2 -> [ 2.0 / 3.0; 4.0 / 3.0 ]
    | _ -> [ 1.0 ]

  /// `accel vertical` は 縦 の 速さ を `FALL_TERM` で -v から g まで 直線 に 変える。折り返す まで に 上がる のは v × tr / 2
  let fallHoldOf (v: float) (g: float) =
    let tr = float FALL_TERM * v / (v + g)
    let h = (RISE_ROOM - v * tr / 2.0) / v
    max 0 (min FALL_HOLD_MAX (int (floor h)))

  /// 入れ替え の 根 は 符号 の 違う 2 波 を 1 組 に して `(n + g × $rank + 1) / 2` 回。走らせ役 は 回数 を 切り捨てる ので、端数 の とき 1 波 増える
  let private waves (s: LineageSpec) =
    if s.Alternate && s.Root <> Root.Bar then 2.0 * floor ((at s.Waves + 1.0) / 2.0) else at s.Waves

  /// 1 段目 の 弾 の 数。Bar は 最初 の 一列 と `BarSteps` 回 の 列 で、一列 は 先頭 + `Line`
  let private roots (s: LineageSpec) =
    match s.Root with
    | Root.Radial | Root.Fan -> float s.Ways * waves s
    | Root.Bar -> float s.Ways * (1.0 + at s.BarSteps) * (1.0 + at s.Line)

  let private factor (s: LineageSpec) (g: Spawner) =
    match g with
    | Spawner.Trail -> 2.0 * float s.TrailTimes * float s.Strands
    | Spawner.Burst -> (1.0 + at s.Line) * float s.Columns

  /// 根 の 1 周 の コマ数。`top` は 終える と 頭 から 走り 直す
  let private cycle (s: LineageSpec) =
    match s.Root with
    | Root.Radial | Root.Fan -> waves s * at1 s.WaveWait
    | Root.Bar -> at s.BarSteps * float BAR_WAIT + at1 s.WaveWait

  /// 段 `i` の 弾 の 速さ（`$rank = 0`）。0 は 根 が 撃つ 弾、ほか は 1 つ 上 の 遺伝子 が 撃つ 弾
  let internal stageSpeed (s: LineageSpec) (i: int) =
    if i = 0 then (match s.Root with Root.Radial | Root.Fan -> s.RootSpeed | Root.Bar -> BAR_LINE_SPEED)
    else (match s.Chain.[i - 1] with Spawner.Trail -> CHILD_SPEED | Spawner.Burst -> BURST_SPEED)

  /// 段 `i` が 止まる まで に 飛ぶ コマ数（`$rank = 1` の 速さ で 数える）
  let flyOf (s: LineageSpec) (i: int) =
    let stops = (s.Chain |> List.filter ((=) Spawner.Burst) |> List.length) + (if s.Leaf = Terminal.Relaunch then 1 else 0)
    max 10 (int (ceil (FLY_SPAN / float (max 1 stops) / fast (stageSpeed s i))))

  /// 段 `i` の 弾 の 寿命。止まって 撃つ 弾 は 撃ったら 消え、ほか は 画面 を 抜ける まで 生きる
  let private lifeOf (s: LineageSpec) (i: int) =
    let speed = stageSpeed s i
    if i < s.Chain.Length then
      match s.Chain.[i] with
      | Spawner.Burst -> float (flyOf s i + s.Hold)
      | Spawner.Trail -> FIELD_SPAN / fast speed
    else
      let relaunch = float (flyOf s i) + at1 s.RelaunchHold + float RELAUNCH_TERM + FIELD_SPAN / fast RELAUNCH_SPEED
      let leaf =
        match s.Leaf with
        | Terminal.Plain -> FIELD_SPAN / fast speed
        | Terminal.Relaunch -> relaunch
        | Terminal.Fall -> float s.FallHold + float FALL_TERM + FALL_SPAN / s.Gravity
      // 狙い 直す 弾 は Relaunch と 同じ 動き
      if s.Seekers then max leaf relaunch else leaf

  /// 同時 に 居る 弾 の 上界（`$rank = 1`）。段 ごと に 1 周 の 数 × 重なる 周 の 数 を 足す
  let aliveBound (s: LineageSpec) : float =
    let c = cycle s
    List.scan (fun n g -> n * factor s g) (roots s) s.Chain
    |> List.mapi (fun i n -> n * ceil (lifeOf s i / c))
    |> List.sum
    |> (*) Bound.SAFETY

  /// Trail を 撃つ 側 の 速さ（`$rank = 0`）。画面 を 横切る コマ数 を 出す のに 使う
  let private trailParentSpeed (s: LineageSpec) =
    match List.tryFindIndex ((=) Spawner.Trail) s.Chain with
    | Some 0 -> (match s.Root with Root.Radial | Root.Fan -> s.RootSpeed | Root.Bar -> BAR_LINE_SPEED)
    | _ -> BURST_SPEED

  /// 波 を 減らす 手 は 無い。1 周 が 半分 に なる と 周 も 半分 に なり、重なる 周 が 倍 に なる
  let private shrink (s: LineageSpec) : LineageSpec option =
    let trail = List.contains Spawner.Trail s.Chain
    // 軌跡 は 4 回 まで 残して 後回し。先 に 削る と 3 世代 の 並び で 2 回 に なり、線 に 見えなかった
    if s.WaveWait.Base < 360 then Some { s with WaveWait = { Base = s.WaveWait.Base * 2; Rank = s.WaveWait.Rank * 2 } }
    elif trail && s.TrailTimes > 4 then Some { s with TrailTimes = s.TrailTimes / 2 }
    elif s.Root = Root.Bar && s.BarSteps.Base > 2 then
      Some { s with BarSteps = { Base = s.BarSteps.Base / 2; Rank = s.BarSteps.Rank / 2 } }
    elif s.Line.Rank > 0 then Some { s with Line = { s.Line with Rank = s.Line.Rank - 1 } }
    elif s.Line.Base > 1 then Some { s with Line = { s.Line with Base = s.Line.Base - 1 } }
    elif s.Columns = 2 then Some { s with Columns = 1 }
    elif trail && s.TrailTimes > 2 then Some { s with TrailTimes = s.TrailTimes / 2 }
    elif s.WaveWait.Base < 1440 then Some { s with WaveWait = { Base = s.WaveWait.Base * 2; Rank = s.WaveWait.Rank * 2 } }
    else None

  let rec private fit (s: LineageSpec) (tries: int) =
    if tries <= 0 || aliveBound s <= BUDGET then s
    else
      match shrink s with
      | Some t -> fit t (tries - 1)
      | None -> s

  let create (f: LineageAxes -> LineageAxes) : LineageSpec =
    let a = f zero
    let streak, still, spread = onScale a.Streak, onScale a.Stillness, onScale a.Spread
    let ways =
      if a.Ways <= 0 then (match a.Root with Root.Radial -> 12 | Root.Bar | Root.Fan -> 2)
      else max 1 (min 24 a.Ways)
    let chain =
      if a.Seed > 0 then CHAIN_TABLE.[(a.Seed - 1) % 9]
      else
        let g = max 1 (min 3 a.Generations)
        if streak >= 1.0 then Spawner.Trail :: List.replicate (g - 1) Spawner.Burst
        else List.replicate g Spawner.Burst
    let trailWait = [| 8; 6; 4 |].[step streak]
    let relaunch = 30 + 20 * step still
    let fall = onScale a.Fall
    let leaf =
      if fall >= 1.0 then Terminal.Fall
      elif still >= 1.0 then Terminal.Relaunch
      else Terminal.Plain
    let s : LineageSpec =
      { Root = a.Root
        Ways = ways
        RootSpeed = min SPEED_HI (1.0 + 1.2 * onScale a.Speed)
        Chain = chain
        Leaf = leaf
        Waves = (match a.Root with Root.Radial | Root.Fan -> { Base = 1; Rank = 1 } | Root.Bar -> { Base = 1; Rank = 0 })
        WaveWait = { Base = 90; Rank = 30 }
        TrailTimes = 0
        TrailWait = { Base = trailWait; Rank = trailWait / 2 }
        Line = { Base = [| 2; 3; 4 |].[step spread]; Rank = 3 }
        Columns = if spread >= 1.5 then 2 else 1
        Seekers = onScale a.Homing >= 1.0
        Sweep = [| 0.0; 48.0; 96.0 |].[step (onScale a.Drift)]
        Strands = 1 + step (onScale a.Strands)
        Gravity = 2.1 * float (step fall)
        FallHold = 0
        Alternate = a.Alternate
        Hold = 10 + 10 * step still
        RelaunchHold = { Base = relaunch; Rank = relaunch / 2 }
        BarSteps = { Base = 12; Rank = 8 }
        Seed = a.Seed }
    let leafSpeed =
      let v = fast (stageSpeed s s.Chain.Length)
      match List.tryLast s.Chain with
      | Some Spawner.Trail -> v * List.max (strandMul s.Strands)
      | _ -> v
    let s = { s with FallHold = if leaf = Terminal.Fall then fallHoldOf leafSpeed s.Gravity else 0 }
    // 画面 を 抜けた 弾 は 撒かない ので、横切る コマ数 より 長く 書かない
    let screen = int (ceil (FIELD_SPAN / trailParentSpeed s / float s.TrailWait.Base))
    fit { s with TrailTimes = min MAX_REPEAT (max 1 screen) } 30

module Lineage =

  let SPEED_HI = LineageSpec.SPEED_HI

  let private STOP = "0.0001"
  /// 一列 の 中 の 速さ の 刻み。0.1 だと 止まる 頃 に 5 px しか 離れず、四角 い 塊 に 見えた
  let private LINE_STEP = 0.3
  /// 最後 が Trail の とき、何 組 に 1 組 を 狙い 直す 弾 に する か
  let private SEEK_EVERY = 4
  let private BAR_LINE_STEP = 0.4
  let private BAR_TURN = 10
  let private BAR_WAIT = LineageSpec.BAR_WAIT
  let private RELAUNCH_SPEED = LineageSpec.RELAUNCH_SPEED
  let private RELAUNCH_TERM = LineageSpec.RELAUNCH_TERM
  let private FALL_TERM = LineageSpec.FALL_TERM

  let private speedOf (v: float) = sprintf "%.2f + $rank * %.2f" v (v * 0.3)
  let private waitOf (r: Ranked) = sprintf "%d - %d * $rank" r.Base r.Rank
  let private timesOf (r: Ranked) = sprintf "%d + %d * $rank" r.Base r.Rank

  let private label (i: int) = sprintf "g%d" (i + 1)
  let private childOf (s: LineageSpec) (i: int) = if i + 1 < s.Chain.Length then label (i + 1) else "leaf"

  /// 最後 の 世代 の 子 を 狙い 直す 弾 に 替える か
  let private seeks (s: LineageSpec) (child: string) = s.Seekers && child = "leaf"

  /// 入れ替え の 符号 を 世代 に 渡す。Alternate が 無ければ 何 も 渡さない（字 を いま の まま に する）
  let private passOn (s: LineageSpec) = if s.Alternate then [ "$1" ] else []

  let private trailBody (s: LineageSpec) (child: string) =
    let args = passOn s
    let plainPair (c: string) =
      [ fire { relative "90"; speed (speedOf LineageSpec.CHILD_SPEED); refBullet c args }
        fire { relative "-90"; speed (speedOf LineageSpec.CHILD_SPEED); refBullet c args }
        wait (waitOf s.TrailWait) ]
    let groups, rest = s.TrailTimes / SEEK_EVERY, s.TrailTimes % SEEK_EVERY
    if s.Sweep = 0.0 && s.Strands = 1 then
      body {
        if not (seeks s child) then
          repeat (string s.TrailTimes) { yield! plainPair child }
        else
          if groups > 0 then
            repeat (string groups) {
              repeat (string (SEEK_EVERY - 1)) { yield! plainPair child }
              yield! plainPair "seeker"
            }
          if rest > 0 then
            repeat (string rest) { yield! plainPair child }
        // 面 は 終えた 台本 を 頭 から 走らせ 直す。終わらせる と 撒き 続ける
        wait "9999"
      }
    else
      let d = if s.TrailTimes > 1 then s.Sweep / float (s.TrailTimes - 1) else 0.0
      // 1 組目 だけ relative で 撃つ。1 発目 の sequence の 起点 は 走らせ役 で 違う
      let turn = if s.Alternate then sprintf "180 + $1 * %.3f" d else sprintf "%.3f" (180.0 + d)
      let muls = LineageSpec.strandMul s.Strands
      let speeds = [ speedOf (LineageSpec.CHILD_SPEED * muls.Head) ]
      let stepOf = if muls.Length > 1 then speedOf (LineageSpec.CHILD_SPEED * (muls.[1] - muls.[0])) else ""
      // 狙い 直す 弾 は 止まって 自機 へ 向き 直す ので、撚って も 同じ 所 に 集まる。1 本 だけ 撃つ
      let side (head: Action) (c: string) =
        if muls.Length = 1 || c = "seeker" then [ head ]
        else [ head; repeat (string (muls.Length - 1)) { fire { sequence "0"; speedSeq stepOf; refBullet c args } } ]
      let firstPair (c: string) =
        side (fire { relative "90"; speed speeds.Head; refBullet c args }) c
        @ side (fire { relative "-90"; speed speeds.Head; refBullet c args }) c
        @ [ wait (waitOf s.TrailWait) ]
      let nextPair (c: string) =
        side (fire { sequence turn; speed speeds.Head; refBullet c args }) c
        @ side (fire { sequence "180"; speed speeds.Head; refBullet c args }) c
        @ [ wait (waitOf s.TrailWait) ]
      let more = s.TrailTimes - 1
      let g, r = more / SEEK_EVERY, more % SEEK_EVERY
      body {
        yield! firstPair child
        if not (seeks s child) then
          if more > 0 then
            repeat (string more) { yield! nextPair child }
        else
          // 2 組目 から 4 組 に 1 組
          if g > 0 then
            repeat (string g) {
              repeat (string (SEEK_EVERY - 1)) { yield! nextPair child }
              yield! nextPair "seeker"
            }
          if r > 0 then
            repeat (string r) { yield! nextPair child }
        wait "9999"
      }

  let private burstBody (s: LineageSpec) (i: int) (child: string) =
    let step = sprintf "%.2f" LINE_STEP
    let head = if seeks s child then "seeker" else child
    body {
      wait (string (LineageSpec.flyOf s i))
      changeSpeedAbs STOP "1"
      wait (string s.Hold)
      // 一列 の 先頭 を 狙い 直す 弾 に する
      fire { relative "90"; speed (speedOf LineageSpec.BURST_SPEED); refBullet head (passOn s) }
      repeat (timesOf s.Line) { fire { sequence "0"; speedSeq step; refBullet child (passOn s) } }
      if s.Columns = 2 then
        fire { sequence "180"; speed (speedOf LineageSpec.BURST_SPEED); refBullet head (passOn s) }
        repeat (timesOf s.Line) { fire { sequence "0"; speedSeq step; refBullet child (passOn s) } }
      vanish
    }

  /// 止まって、待って、自機 へ 向き 直して から 加速 する
  let private seekerBody (s: LineageSpec) =
    body {
      wait (string (LineageSpec.flyOf s s.Chain.Length))
      changeSpeedAbs STOP "1"
      wait (waitOf s.RelaunchHold)
      changeDirectionAim "0" "1"
      changeSpeedAbs (speedOf RELAUNCH_SPEED) (string RELAUNCH_TERM)
      wait "9999"
    }

  let private relaunchBody (s: LineageSpec) =
    body {
      wait (string (LineageSpec.flyOf s s.Chain.Length))
      changeSpeedAbs STOP "1"
      wait (waitOf s.RelaunchHold)
      changeSpeedAbs (speedOf RELAUNCH_SPEED) (string RELAUNCH_TERM)
      wait "9999"
    }

  /// 待って、下 へ 引かれる。accel は 面 の 縦横 で 効く ので、どの 向き に 撃たれて も 下 へ 落ちる
  let private fallBody (s: LineageSpec) =
    body {
      wait (string s.FallHold)
      accel (string FALL_TERM) { vertical (sprintf "%.2f" s.Gravity) }
      wait "9999"
    }

  let private sign (i: int) (w: int) = if (i + w) % 2 = 0 then 1 else -1
  let private argsAt (s: LineageSpec) (i: int) (w: int) = if s.Alternate then [ string (sign i w) ] else []
  let private pairsOf (r: Ranked) = sprintf "(%d + %d * $rank + 1) / 2" r.Base r.Rank

  /// 筋 の 1 本目 の 向き（aim から の 度）と 刻み
  let private raysOf (s: LineageSpec) =
    match s.Root with
    | Root.Fan -> if s.Ways = 1 then 0.0, 0.0 else -60.0, 120.0 / float (s.Ways - 1)
    | _ ->
      let gap = 360.0 / float s.Ways
      (if s.Ways >= 2 then gap / 2.0 else 0.0), gap

  /// 1 波。入れ替え の とき は `wave` の action に 波 の 符号 を `$1` で 渡し、奇数 本目 は `0 - $1`
  let private waveBody (s: LineageSpec) =
    let first, step = raysOf s
    let args (odd: bool) = if not s.Alternate then [] elif odd then [ "0 - $1" ] else [ "$1" ]
    let shot (odd: bool) = fire { sequence (sprintf "%.2f" step); speed (speedOf s.RootSpeed); refBullet "g1" (args odd) }
    let more = s.Ways - 1
    [ yield fire { aim (sprintf "%.2f" first); speed (speedOf s.RootSpeed); refBullet "g1" (args false) }
      if more >= 2 then yield repeat (string (more / 2)) { yield! [ shot true; shot false ] }
      if more % 2 = 1 then yield shot true
      yield wait (waitOf s.WaveWait) ]

  let private spreadTop (s: LineageSpec) =
    top {
      if s.Alternate then
        repeat (pairsOf s.Waves) {
          yield! [ actionRef "wave" [ "1" ]; actionRef "wave" [ "-1" ] ]
        }
      else
        repeat (timesOf s.Waves) { yield! waveBody s }
    }

  let private waveDef (s: LineageSpec) = defAction "wave" { yield! waveBody s }

  let private radialTop (s: LineageSpec) =
    let gap = 360.0 / float s.Ways
    // 刻み の 半分 ずらす と 自機 の 真上 が 隙間 に なる
    let first = if s.Ways >= 2 then gap / 2.0 else 0.0
    top {
      repeat (timesOf s.Waves) {
        fire { aim (sprintf "%.2f" first); speed (speedOf s.RootSpeed); refBullet "g1" [] }
        if s.Ways > 1 then
          repeat (string (s.Ways - 1)) {
            fire { sequence (sprintf "%.2f" gap); speed (speedOf s.RootSpeed); refBullet "g1" [] }
          }
        wait (waitOf s.WaveWait)
      }
    }

  /// 発射台 が 消える まで と 波 の 間。`top` は 終える と 頭 から 走り 直す ので、待たない と 毎コマ 発射台 を 出す
  let private barCycle (s: LineageSpec) =
    let b = s.BarSteps.Base * BAR_WAIT + s.WaveWait.Base
    let r = s.BarSteps.Rank * BAR_WAIT - s.WaveWait.Rank
    if r >= 0 then sprintf "%d + %d * $rank" b r else sprintf "%d - %d * $rank" b (-r)

  let private barTop (s: LineageSpec) =
    let gap = 360.0 / float s.Ways
    let pads (w: int) =
      if s.Alternate then
        [ for i in 0 .. s.Ways - 1 -> fire { aim (sprintf "%.2f" (gap * float i)); speed STOP; refBullet "bar" (argsAt s i w) } ]
        @ [ wait (barCycle s) ]
      else
        [ yield fire { aim "0"; speed STOP; refBullet "bar" [] }
          if s.Ways > 1 then
            yield repeat (string (s.Ways - 1)) { fire { sequence (sprintf "%.2f" gap); speed STOP; refBullet "bar" [] } }
          yield wait (barCycle s) ]
    top {
      yield! pads 0
      // top は 終える と 頭 から 走り 直す ので、周 を またぐ 符号 は 2 周 を 1 つ に 書いて 持つ
      if s.Alternate then yield! pads 1
    }

  /// 発射台。一列 の 頭 は 1 本目 だけ `relative 0`、2 本目 から は 前 の 列 から `sequence` で 回す
  let private barDef (s: LineageSpec) =
    let line (head: Action) =
      [ head
        repeat (timesOf s.Line) {
          fire { sequence "0"; speedSeq (sprintf "%.2f" BAR_LINE_STEP); refBullet "g1" (passOn s) }
        } ]
    defBullet "bar" {
      doActs (
        body {
          yield! line (fire { relative "0"; speed (speedOf LineageSpec.BAR_LINE_SPEED); refBullet "g1" (passOn s) })
          repeat (timesOf s.BarSteps) {
            wait (string BAR_WAIT)
            yield! line (fire { sequence (string BAR_TURN); speed (speedOf LineageSpec.BAR_LINE_SPEED); refBullet "g1" (passOn s) })
          }
          vanish
        })
    }

  let generate (s: LineageSpec) : BulletmlInfo =
    let defs =
      s.Chain
      |> List.mapi (fun i gene ->
          let child = childOf s i
          defBullet (label i) {
            doActs (
              match gene with
              | Spawner.Trail -> trailBody s child
              | Spawner.Burst -> burstBody s i child)
          })
    let leaf =
      match s.Leaf with
      | Terminal.Plain -> defBullet "leaf" { () }
      | Terminal.Relaunch -> defBullet "leaf" { doActs (relaunchBody s) }
      | Terminal.Fall -> defBullet "leaf" { doActs (fallBody s) }
    createBulletmlInfo
    <| vertical "lineage" {
         match s.Root with
         | Root.Radial when not s.Alternate -> radialTop s
         | Root.Radial | Root.Fan ->
           spreadTop s
           if s.Alternate then waveDef s
         | Root.Bar ->
           barTop s
           barDef s
         yield! defs
         leaf
         if s.Seekers then defBullet "seeker" { doActs (seekerBody s) }
       }
