namespace FsBulletML2.Front

open System
open System.Collections.Generic
open FsBulletML2
open FsBulletML2.Domain

/// ゲームの側だけが知っていることを、エンジンの手前で聞く口。
/// 敵の一覧は要求しない。一覧を持つフロントは `NearestEnemy` で答えればよい。
type IFrontEnv =

  /// 乱数。1 個 だけ作って使い回すこと。
  /// 毎コマ 関数値を作ると弾数 × コマ数 だけヒープを踏む（実測 48 B / 回）。
  /// C# からは `FuncConvert.FromFunc` を 1 度 だけ通して static に持つ
  abstract Rand : (unit -> float32)

  abstract Rank : float32

  abstract PlayerX : float32
  abstract PlayerY : float32

  /// この弾が狙う相手の位置。居なければ false（そのとき向きは 0）。
  /// 一度選んだ相手は覚えること。毎コマ選び直すと軌跡が変わる。
  abstract TryTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

  /// これから産まれる弾が狙う相手の位置。居なければ false。
  /// `TryTargetFrom` と分けてある。1 本にまとめると、どちらかの軌跡が動く。
  abstract TrySpawnTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

/// 一覧を持つフロント向けの部品。任意。
/// いちばん近いのを選んで覚える。弾 1 個 につき 1 個。
[<Sealed>]
type NearestEnemy<'E>(enemies: Func<IReadOnlyList<'E>>,
                      ex: Func<'E, float32>,
                      ey: Func<'E, float32>) =

  let ex e = ex.Invoke e
  let ey e = ey.Invoke e

  let mutable target : 'E = Unchecked.defaultof<'E>
  let mutable hasTarget = false

  /// 距離の測り方。float32 で 2 乗して double で sqrt する。
  /// 同梱の Distance と同じ順。2 乗のままだと、丸めで並ぶ組の選び方が変わる。
  static member private Dist (dx: float32) (dy: float32) =
    float32 (System.Math.Sqrt(float (dx * dx + dy * dy)))

  /// 覚えている相手を捨てる。次に聞かれたら選び直す
  member _.Forget () =
    target <- Unchecked.defaultof<'E>
    hasTarget <- false

  /// `(x, y)` からいちばん近い相手を 1 度 だけ 選び、以後はそれを返す
  member _.TryFrom (x: float32, y: float32, outX: outref<float32>, outY: outref<float32>) : bool =
    if not hasTarget then
      let list = enemies.Invoke ()
      let mutable best = Single.MaxValue
      for i in 0 .. list.Count - 1 do
        let e = list.[i]
        let d = NearestEnemy<'E>.Dist (ex e - x) (ey e - y)
        if best > d then
          best <- d
          target <- e
          hasTarget <- true
    if hasTarget then
      outX <- ex target
      outY <- ey target
      true
    else
      false

  /// 覚えずに、その場でいちばん近い相手を選ぶ
  member _.TryNearest (x: float32, y: float32, outX: outref<float32>, outY: outref<float32>) : bool =
    let list = enemies.Invoke ()
    let mutable best = Single.MaxValue
    let mutable found = false
    let mutable near = Unchecked.defaultof<'E>
    for i in 0 .. list.Count - 1 do
      let e = list.[i]
      let d = NearestEnemy<'E>.Dist (ex e - x) (ey e - y)
      if best > d then
        best <- d
        near <- e
        found <- true
    if found then
      outX <- ex near
      outY <- ey near
      true
    else
      false

/// このコマの `Env` を組む。aim を入れる場所はここだけ。
module FrontEnv =

  /// aim を読まないと分かっているコマの `Env`。aim 4 本 を 0 に。
  /// 使ってよい条件は `BulletRun.HasNoScript` の但し書き。
  [<CompiledName "NoAim">]
  let noAim (front: IFrontEnv) : Env =
    { Rand = front.Rand
      Rank = front.Rank
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

  /// いまの位置から組む。呼ぶ側は step の直前（差分を足す前）に組むこと。
  /// 走らせ直しの前は、差分を足したあとに組む。
  [<CompiledName "At">]
  let at (front: IFrontEnv) (space: Space) (origin: SpawnOrigin) (x: float32) (y: float32) : Env =
    let struct (sx, sy) = Aiming.spawnPoint origin x y
    let mutable tx = 0.0f
    let mutable ty = 0.0f
    let toEnemy =
      if front.TryTargetFrom(x, y, &tx, &ty) then Aiming.toward space x y tx ty else 0.0f
    let mutable stx = 0.0f
    let mutable sty = 0.0f
    let spawnToEnemy =
      if front.TrySpawnTargetFrom(sx, sy, &stx, &sty) then Aiming.toward space sx sy stx sty else 0.0f
    { Rand = front.Rand
      Rank = front.Rank
      Aim = { ToPlayer = Aiming.toward space x y front.PlayerX front.PlayerY
              ToEnemy = toEnemy }
      Spawn = { ToPlayer = Aiming.toward space sx sy front.PlayerX front.PlayerY
                ToEnemy = spawnToEnemy } }

  /// 台本が無い弾は aim を読まないので、そのときは `noAim`。
  /// 通し忘れても答えは同じで速さだけ落ちる。忘れたことが門に出ない。
  [<CompiledName "ForRun">]
  let forRun (front: IFrontEnv) (space: Space) (origin: SpawnOrigin)
             (run: BulletRun) (x: float32) (y: float32) : Env =
    if run.HasNoScript then noAim front else at front space origin x y
