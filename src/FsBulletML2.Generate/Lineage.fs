namespace FsBulletML2.Generate

open System
open FsBulletML2
open FsBulletML2.Dsl
open FsBulletML2.Generate.Consts

/// 根。`Radial` は 放射、`Bar` は 止まった 発射台 が 回りながら 直線 を 敷く。
/// 腕 の 名前 が `PatternSpec` の `Radial` と ぶつかる ので 型 名 で 書かせる
[<RequireQualifiedAccess>]
type Root =
  | Radial
  | Bar

/// 子 を 生む 遺伝子。`Trail` は 飛びながら 左右 へ 撒き、`Burst` は 止まって 横 へ 一列 撃って 消える
[<RequireQualifiedAccess>]
type Spawner =
  | Trail
  | Burst

/// 系譜 の 終わり。`Relaunch` は 止まって、待って、加速 する
[<RequireQualifiedAccess>]
type Terminal =
  | Plain
  | Relaunch

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
  /// 止まる 段 が 飛ぶ 距離 の 合計（px）。1 段 ずつ 遠く へ 出る ので、段 が 多い ほど 1 段 を 短く する ——
  /// 1 段 60 コマ で 固定 した とき、3 段 目 は 画面 の 外 で 止まって 撒かなかった
  let internal FLY_SPAN = 180.0
  let internal BAR_WAIT = 5
  let internal RELAUNCH_SPEED = 2.5
  let internal RELAUNCH_TERM = 60

  /// `$rank = 1` で 同時 に 居る 弾 の 上界 を ここ まで 削る。面 の 天井 と 同じ 10,000 ——
  /// 上界 は 実測 を 下回らない（36 通り の 試験 が 見る）ので、余白 を 別 に 取らない。
  /// 6,000 に した とき は 上界 が 実測 の 1.5〜6 倍 で、Trail が 2 回 まで 削られた
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
      Seed = 0 }

  /// 回数 の `$rank = 1`
  let private at (r: Ranked) = float (r.Base + r.Rank)
  /// 待ち の `$rank = 1`
  let private at1 (r: Ranked) = float (r.Base - r.Rank)
  let private fast (v: float) = v * 1.3

  /// 1 段目 の 弾 の 数。Bar は 最初 の 一列 と `BarSteps` 回 の 列 で、一列 は 先頭 + `Line`
  let private roots (s: LineageSpec) =
    match s.Root with
    | Root.Radial -> float s.Ways * at s.Waves
    | Root.Bar -> float s.Ways * (1.0 + at s.BarSteps) * (1.0 + at s.Line)

  let private factor (s: LineageSpec) (g: Spawner) =
    match g with
    | Spawner.Trail -> 2.0 * float s.TrailTimes
    | Spawner.Burst -> (1.0 + at s.Line) * float s.Columns

  /// 根 の 1 周 の コマ数。`top` は 終える と 頭 から 走り 直す
  let private cycle (s: LineageSpec) =
    match s.Root with
    | Root.Radial -> at s.Waves * at1 s.WaveWait
    | Root.Bar -> at s.BarSteps * float BAR_WAIT + at1 s.WaveWait

  /// 段 `i` の 弾 の 速さ（`$rank = 0`）。0 は 根 が 撃つ 弾、ほか は 1 つ 上 の 遺伝子 が 撃つ 弾
  let internal stageSpeed (s: LineageSpec) (i: int) =
    if i = 0 then (match s.Root with Root.Radial -> s.RootSpeed | Root.Bar -> BAR_LINE_SPEED)
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
      // 狙い 直す 弾 は Relaunch と 同じ 動き
      if s.Seekers then max leaf relaunch else leaf

  /// 同時 に 居る 弾 の 上界（`$rank = 1`）。根 は 繰り返す ので、段 ごと に 1 周 の 数 と 重なる 周 の 数 を 掛けて 足す。
  /// 系譜 まるごと の 寿命 で 掛ける と 実測 / 見積もり が 0.3 まで 下がり、`fit` が 削り すぎた。
  /// 画面 を 抜ける 距離 を `FIELD_SPAN` で 取る ので、下向き に 長く 飛ぶ ぶん は `SAFETY` が 持つ
  let aliveBound (s: LineageSpec) : float =
    let c = cycle s
    List.scan (fun n g -> n * factor s g) (roots s) s.Chain
    |> List.mapi (fun i n -> n * ceil (lifeOf s i / c))
    |> List.sum
    |> (*) Bound.SAFETY

  /// Trail を 撃つ 側 の 速さ（`$rank = 0`）。画面 を 横切る コマ数 を 出す のに 使う
  let private trailParentSpeed (s: LineageSpec) =
    match List.tryFindIndex ((=) Spawner.Trail) s.Chain with
    | Some 0 -> (match s.Root with Root.Radial -> s.RootSpeed | Root.Bar -> BAR_LINE_SPEED)
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
      if a.Ways <= 0 then (match a.Root with Root.Radial -> 12 | Root.Bar -> 2)
      else max 1 (min 24 a.Ways)
    let chain =
      if a.Seed > 0 then CHAIN_TABLE.[(a.Seed - 1) % 9]
      else
        let g = max 1 (min 3 a.Generations)
        if streak >= 1.0 then Spawner.Trail :: List.replicate (g - 1) Spawner.Burst
        else List.replicate g Spawner.Burst
    let trailWait = [| 8; 6; 4 |].[step streak]
    let relaunch = 30 + 20 * step still
    let s : LineageSpec =
      { Root = a.Root
        Ways = ways
        RootSpeed = min SPEED_HI (1.0 + 1.2 * onScale a.Speed)
        Chain = chain
        Leaf = if still >= 1.0 then Terminal.Relaunch else Terminal.Plain
        Waves = (match a.Root with Root.Radial -> { Base = 1; Rank = 1 } | Root.Bar -> { Base = 1; Rank = 0 })
        WaveWait = { Base = 90; Rank = 30 }
        TrailTimes = 0
        TrailWait = { Base = trailWait; Rank = trailWait / 2 }
        Line = { Base = [| 2; 3; 4 |].[step spread]; Rank = 3 }
        Columns = if spread >= 1.5 then 2 else 1
        Seekers = onScale a.Homing >= 1.0
        Hold = 10 + 10 * step still
        RelaunchHold = { Base = relaunch; Rank = relaunch / 2 }
        BarSteps = { Base = 12; Rank = 8 }
        Seed = a.Seed }
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

  let private speedOf (v: float) = sprintf "%.2f + $rank * %.2f" v (v * 0.3)
  let private waitOf (r: Ranked) = sprintf "%d - %d * $rank" r.Base r.Rank
  let private timesOf (r: Ranked) = sprintf "%d + %d * $rank" r.Base r.Rank

  let private label (i: int) = sprintf "g%d" (i + 1)
  let private childOf (s: LineageSpec) (i: int) = if i + 1 < s.Chain.Length then label (i + 1) else "leaf"

  /// 最後 の 世代 の 子 を 狙い 直す 弾 に 替える か
  let private seeks (s: LineageSpec) (child: string) = s.Seekers && child = "leaf"

  let private trailBody (s: LineageSpec) (child: string) =
    let pair (c: string) =
      [ fire { relative "90"; speed (speedOf LineageSpec.CHILD_SPEED); refBullet c [] }
        fire { relative "-90"; speed (speedOf LineageSpec.CHILD_SPEED); refBullet c [] }
        wait (waitOf s.TrailWait) ]
    let groups, rest = s.TrailTimes / SEEK_EVERY, s.TrailTimes % SEEK_EVERY
    body {
      if not (seeks s child) then
        repeat (string s.TrailTimes) { yield! pair child }
      else
        if groups > 0 then
          repeat (string groups) {
            repeat (string (SEEK_EVERY - 1)) { yield! pair child }
            yield! pair "seeker"
          }
        if rest > 0 then
          repeat (string rest) { yield! pair child }
      // 面 は 終えた 台本 を 頭 から 走らせ 直す。終わらせる と 撒き 続ける
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
      fire { relative "90"; speed (speedOf LineageSpec.BURST_SPEED); refBullet head [] }
      repeat (timesOf s.Line) { fire { sequence "0"; speedSeq step; refBullet child [] } }
      if s.Columns = 2 then
        fire { sequence "180"; speed (speedOf LineageSpec.BURST_SPEED); refBullet head [] }
        repeat (timesOf s.Line) { fire { sequence "0"; speedSeq step; refBullet child [] } }
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

  let private radialTop (s: LineageSpec) =
    let gap = 360.0 / float s.Ways
    // 刻み の 半分 ずらす と 自機 の 真上 が 隙間 に なる（見本 は aim 15 / 刻み 30）
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
    top {
      fire { aim "0"; speed STOP; refBullet "bar" [] }
      if s.Ways > 1 then
        repeat (string (s.Ways - 1)) { fire { sequence (sprintf "%.2f" gap); speed STOP; refBullet "bar" [] } }
      wait (barCycle s)
    }

  /// 発射台。一列 の 頭 は 1 本目 だけ `relative 0`、2 本目 から は 前 の 列 から `sequence` で 回す
  let private barDef (s: LineageSpec) =
    let line (head: Action) =
      [ head
        repeat (timesOf s.Line) {
          fire { sequence "0"; speedSeq (sprintf "%.2f" BAR_LINE_STEP); refBullet "g1" [] }
        } ]
    defBullet "bar" {
      doActs (
        body {
          yield! line (fire { relative "0"; speed (speedOf LineageSpec.BAR_LINE_SPEED); refBullet "g1" [] })
          repeat (timesOf s.BarSteps) {
            wait (string BAR_WAIT)
            yield! line (fire { sequence (string BAR_TURN); speed (speedOf LineageSpec.BAR_LINE_SPEED); refBullet "g1" [] })
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
    createBulletmlInfo
    <| vertical "lineage" {
         match s.Root with
         | Root.Radial -> radialTop s
         | Root.Bar ->
           barTop s
           barDef s
         yield! defs
         leaf
         if s.Seekers then defBullet "seeker" { doActs (seekerBody s) }
       }
