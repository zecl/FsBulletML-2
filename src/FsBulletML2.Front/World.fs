namespace FsBulletML2.Front

open System.Collections.Generic

/// ゲームの側だけが知っていることを、エンジンの手前で聞く口。
///
/// **敵の一覧は要求しない。** 同梱の狙い方 7 通り のうち一覧を持つのは
/// 3 つ だけで、ECS 2 つ とベンチと門の控えは「敵 1 体」か「固定の 1 点」
/// しか持たない。一覧を要求すると、その 4 つ が 1 要素 の一覧を毎コマ
/// 用意することになる。
///
/// 一覧を持つフロントは `NearestEnemy` を使って答えればよい。**任意。**
type IWorld =

  /// 乱数。**1 個 だけ作って使い回すこと。**
  /// 毎コマ 関数値を作ると弾数 × コマ数 だけヒープを踏む（実測 48 B / 回）。
  /// C# からは `FuncConvert.FromFunc` を 1 度 だけ通して static に持つ
  abstract Rand : (unit -> float32)

  abstract Rank : float32

  abstract PlayerX : float32
  abstract PlayerY : float32

  /// この弾が狙う相手の位置。居なければ false（そのとき向きは 0）。
  ///
  /// **一度 選んだ相手は覚えること。** 毎コマ 選び直すと相手が入れ替わって
  /// 軌跡が変わる。この不変条件は口ではなく実装の側が持つ（`NearestEnemy`）。
  ///
  /// **`voption` でも `Vec2` でもなく out 2 本。** 値を包むと 24 B / 回、
  /// `Vec2` を口ごしに返すと 1.3 ns / 回 の差が出る
  abstract TryTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

  /// **これから産まれる弾**が狙う相手の位置。居なければ false。
  ///
  /// **`TryTargetFrom` と分けてある。同梱の 2 つ で振る舞いが違う。**
  /// MonoGame は産まれる位置からその場で選び直し（産まれたばかりの弾は
  /// まだ相手を覚えていない）、Unity2D は撃った側が覚えている相手を
  /// そのまま使う。1 本 にまとめると、どちらかの軌跡が動く
  abstract TrySpawnTargetFrom : x: float32 * y: float32 * ex: outref<float32> * ey: outref<float32> -> bool

/// 一覧を持つフロント向けの部品。**任意。**
///
/// 「いちばん近いのを選んで覚える」の不変条件はここが持つ。
/// 弾 1 個 につき 1 個 作る（覚える相手が弾ごとに違うため）。
///
/// **`IReadOnlyList` であって `seq` ではない。** `seq` を毎コマ 列挙すると
/// 列挙子が出る（実測 40 B / 回）。`IReadOnlyList` と添字は 0 B。
/// この線は但し書きではなく門で見ている（`PublicSurface`）。
///
/// **`PosOf` でなく `ex` / `ey` の 2 本。** `Vec2` を口ごしに返すほうが
/// 1.3 ns / 回 遅い。5way 1 走行 では 0.04% 相当なので速さでは決まらないが、
/// 遅いほうを選ぶ理由も無い。
[<Sealed>]
type NearestEnemy<'E>(enemies: IReadOnlyList<'E>, ex: 'E -> float32, ey: 'E -> float32) =

  let mutable target : 'E = Unchecked.defaultof<'E>
  let mutable hasTarget = false

  /// 距離の測り方。**同梱 2 つ の `Vector2.Distance` / `Vector3.Distance` と
  /// 同じ順で同じ答えを出すために、float32 で 2 乗 して double で sqrt する。**
  /// 2 乗 のまま比べても選ぶ相手はふつう同じだが、丸めで並ぶ組が出たときに
  /// 選び方が変わる
  static member private Dist (dx: float32) (dy: float32) =
    float32 (System.Math.Sqrt(float (dx * dx + dy * dy)))

  /// 覚えている相手を捨てる。次に聞かれたら選び直す
  member _.Forget () =
    target <- Unchecked.defaultof<'E>
    hasTarget <- false

  /// `(x, y)` からいちばん近い相手を **1 度 だけ** 選び、以後はそれを返す
  member _.TryFrom (x: float32, y: float32, outX: outref<float32>, outY: outref<float32>) : bool =
    if not hasTarget then
      let mutable best = System.Single.MaxValue
      for i in 0 .. enemies.Count - 1 do
        let e = enemies.[i]
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

  /// **覚えずに**、その場でいちばん近い相手を選ぶ
  member _.TryNearest (x: float32, y: float32, outX: outref<float32>, outY: outref<float32>) : bool =
    let mutable best = System.Single.MaxValue
    let mutable found = false
    let mutable near = Unchecked.defaultof<'E>
    for i in 0 .. enemies.Count - 1 do
      let e = enemies.[i]
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
