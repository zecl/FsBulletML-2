namespace FsBulletML2.Front

open System
open System.Collections.Generic
open FsBulletML2
open FsBulletML2.Domain

/// ゲームの側だけが知っていることを、エンジンの手前で聞く口。
///
/// 敵の一覧は要求しない。 同梱の狙い方 7 通り のうち一覧を持つのは
/// 3 つ だけで、ECS 2 つ とベンチと門の控えは「敵 1 体」か「固定の 1 点」
/// しか持たない。一覧を要求すると、その 4 つ が 1 要素 の一覧を毎コマ
/// 用意することになる。
///
/// 一覧を持つフロントは `NearestEnemy` を使って答えればよい。任意。
type IFrontEnv =

  /// 乱数。1 個 だけ作って使い回すこと。
  /// 毎コマ 関数値を作ると弾数 × コマ数 だけヒープを踏む（実測 48 B / 回）。
  /// C# からは `FuncConvert.FromFunc` を 1 度 だけ通して static に持つ
  abstract Rand : (unit -> float32)

  abstract Rank : float32

  abstract PlayerX : float32
  abstract PlayerY : float32

  /// この弾が狙う相手の位置。居なければ false（そのとき向きは 0）。
  ///
  /// 一度 選んだ相手は覚えること。 毎コマ 選び直すと相手が入れ替わって
  /// 軌跡が変わる。この不変条件は口ではなく実装の側が持つ（`NearestEnemy`）。
  ///
  /// `voption` でも `Vec2` でもなく out 2 本。 値を包むと 24 B / 回、
  /// `Vec2` を口ごしに返すと 1.3 ns / 回 の差が出る
  abstract TryTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

  /// これから産まれる弾が狙う相手の位置。居なければ false。
  ///
  /// `TryTargetFrom` と分けてある。同梱の 2 つ で振る舞いが違う。
  /// MonoGame は産まれる位置からその場で選び直し（産まれたばかりの弾は
  /// まだ相手を覚えていない）、Unity2D は撃った側が覚えている相手を
  /// そのまま使う。1 本 にまとめると、どちらかの軌跡が動く
  abstract TrySpawnTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

/// 一覧を持つフロント向けの部品。任意。
///
/// 「いちばん近いのを選んで覚える」の不変条件はここが持つ。
/// 弾 1 個 につき 1 個 作る（覚える相手が弾ごとに違うため）。
[<Sealed>]
type NearestEnemy<'E>(enemies: Func<IReadOnlyList<'E>>,
                      ex: Func<'E, float32>,
                      ey: Func<'E, float32>) =

  let ex e = ex.Invoke e
  let ey e = ey.Invoke e

  let mutable target : 'E = Unchecked.defaultof<'E>
  let mutable hasTarget = false

  /// 距離の測り方。同梱 2 つ の `Vector2.Distance` / `Vector3.Distance` と
  /// 同じ順で同じ答えを出すために、float32 で 2 乗 して double で sqrt する。
  /// 2 乗 のまま比べても選ぶ相手はふつう同じだが、丸めで並ぶ組が出たときに
  /// 選び方が変わる
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
///
/// 以前は同梱の 4 つ のフロントがそれぞれ同じ形を写していた。写しは
/// 「片方だけ直す」ができるので、`Aim` と `Spawn` の食い違いが門に出ない。
module FrontEnv =

  /// aim を読まないと分かっているコマの `Env`。aim 4 本 を 0 に。
  ///
  /// 使ってよい条件は `BulletRun.HasNoScript` の但し書き。
  /// `at` と欄が 1 つ でもずれたら、片方だけ直したということ
  [<CompiledName "NoAim">]
  let noAim (front: IFrontEnv) : Env =
    { Rand = front.Rand
      Rank = front.Rank
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

  /// いまの位置から組む。
  ///
  /// 組む位置が変わると aim がずれるので、呼ぶ側は step の直前
  /// （差分を足す前）に組むこと。走らせ直しの前は、差分を足したあとに組む
  /// （旧 `BaseBullet` が apply のあとで `envOfGlobal` を呼ぶのと同じ順）。
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
  ///
  /// この枝を既定にしてある。 同梱のフロントは全部 これを通していたが、
  /// 通し忘れても答えは同じで速さだけ落ちる（5way で 24%）ので、
  /// 忘れたことが門に出ない
  [<CompiledName "ForRun">]
  let forRun (front: IFrontEnv) (space: Space) (origin: SpawnOrigin)
             (run: BulletRun) (x: float32) (y: float32) : Env =
    if run.HasNoScript then noAim front else at front space origin x y
