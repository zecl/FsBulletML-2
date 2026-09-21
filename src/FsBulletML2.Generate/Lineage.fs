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

  /// `$rank = 1` で 生まれる 弾 の 合計 を ここ まで 削る。面 の 天井 10,000 との 差 は 見積もり の 誤差
  let BUDGET = 6000.0

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
      Seed = 0 }

  let private at (r: Ranked) = float (r.Base + r.Rank)

  /// 1 段目 の 弾 の 数。Bar は 一列 ごと に 先頭 + `Line`
  let private roots (s: LineageSpec) =
    match s.Root with
    | Root.Radial -> float s.Ways * at s.Waves
    | Root.Bar -> float s.Ways * at s.BarSteps * (1.0 + at s.Line)

  let private factor (s: LineageSpec) (g: Spawner) =
    match g with
    | Spawner.Trail -> 2.0 * float s.TrailTimes
    | Spawner.Burst -> (1.0 + at s.Line) * float s.Columns

  /// 段 ごと の 数 の 和。同時 に 居る 数 は これ を 超えない
  let spawnBound (s: LineageSpec) : float =
    s.Chain
    |> List.scan (fun n g -> n * factor s g) (roots s)
    |> List.sum

  /// Trail を 撃つ 側 の 速さ（`$rank = 0`）。画面 を 横切る コマ数 を 出す のに 使う
  let private trailParentSpeed (s: LineageSpec) =
    match List.tryFindIndex ((=) Spawner.Trail) s.Chain with
    | Some 0 -> (match s.Root with Root.Radial -> s.RootSpeed | Root.Bar -> BAR_LINE_SPEED)
    | _ -> BURST_SPEED

  let private shrink (s: LineageSpec) : LineageSpec option =
    if List.contains Spawner.Trail s.Chain && s.TrailTimes > 2 then Some { s with TrailTimes = s.TrailTimes / 2 }
    elif s.Waves <> { Base = 1; Rank = 0 } then Some { s with Waves = { Base = 1; Rank = 0 } }
    elif s.Root = Root.Bar && s.BarSteps.Base > 2 then
      Some { s with BarSteps = { Base = s.BarSteps.Base / 2; Rank = s.BarSteps.Rank / 2 } }
    elif s.Line.Rank > 0 then Some { s with Line = { s.Line with Rank = s.Line.Rank - 1 } }
    elif s.Line.Base > 1 then Some { s with Line = { s.Line with Base = s.Line.Base - 1 } }
    elif s.Columns = 2 then Some { s with Columns = 1 }
    else None

  let rec private fit (s: LineageSpec) (tries: int) =
    if tries <= 0 || spawnBound s <= BUDGET then s
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
        Hold = 10 + 10 * step still
        RelaunchHold = { Base = relaunch; Rank = relaunch / 2 }
        BarSteps = { Base = 12; Rank = 8 }
        Seed = a.Seed }
    // 画面 を 抜けた 弾 は 撒かない ので、横切る コマ数 より 長く 書かない
    let screen = int (ceil (FIELD_SPAN / trailParentSpeed s / float s.TrailWait.Base))
    fit { s with TrailTimes = min MAX_REPEAT (max 1 screen) } 30
