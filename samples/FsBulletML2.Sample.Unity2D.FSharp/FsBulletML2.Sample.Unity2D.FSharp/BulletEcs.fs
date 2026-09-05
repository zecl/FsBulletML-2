namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open Unity.Entities
open UnityEngine
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front
// Settings.Display.PixcelsToUnits（座標の係数）はこちら
open FsBulletML2.Unity2D

/// 弾がどちら側のものか。**エンジンの BulletType とは別**
/// —— あちらは弾幕の意味（aim の向き）、こちらは当たり判定と見た目の分け。
type BulletKind =
  | Enemy = 0uy
  | Player = 1uy

/// Entity に付ける印。当たり判定の半径もここ。
/// **struct なので Burst からも触れる**（いまは触っていない）
[<Struct>]
type BulletTag =
  val mutable Kind : BulletKind
  val mutable Radius : float32
  interface IComponentData

/// 走らせる側から見た「いまの場面」。**シーンに 1 つ しかない位置を置く場所。**
///
/// 毎コマ FindAnyObjectByType を呼ぶと弾の数だけ探索が走るので、
/// Bootstrap が 1 回 だけ入れる。
///
/// **Transform で持つ（Enemy / Player 型では持たない）。**
/// F# はファイル順に型を解決するので、ここが `Enemy` を知ると
/// `BulletSim` -> `Enemy` -> `BulletEntityFactory` -> `BulletSim` の輪ができる。
/// 弾が要るのは位置だけなので、型を知らずに済ませてある。
/// ダメージの通知は `BulletEcsDriver`（Enemy / Player の後でコンパイルされる）がやる。
[<AbstractClass; Sealed>]
type BulletEcsRuntime private () =
  static member val PlayerTransform : Transform = null with get, set
  static member val EnemyTransform : Transform = null with get, set
  static member val PlayerRadius = 0.15f with get, set
  static member val EnemyRadius = 0.25f with get, set

/// この ECS の弾が `FsBulletML2.Front` の口に答えるところ。
///
/// 旧は `FsBulletML2.Unity2D.DefaultBullet` が組んでいたが、あれは
/// `Transform` を持つ GameObject 前提。ECS の弾は Transform を持たない
/// （位置は float で持ち、描画のときだけ `LocalTransform` へ写す）ので、
/// ここで答え直す。
///
/// **式そのものはここに無い。** `Aiming.toward` の `Space` 違いで、
/// MonoGame の同じ関数と 1 ビット しか違わなかった。
///
/// **敵は 1 体 しか居ない。** だから `NearestEnemy` は使わず、
/// `BulletEcsRuntime.EnemyTransform` をそのまま答える ——
/// **一覧を要求しない口にしてあるのはこのため**（一覧を要求すると、
/// この側が 1 要素 の一覧を毎コマ 用意することになる）。
[<Sealed>]
type EcsWorld() =

  /// `Env.Rand` に入れる関数値。**1 個 だけ作って使い回す。**
  /// 中身はグローバル（BulletMLManager）を読むだけなので、いつ作っても同じ。
  /// 毎コマ 作ると弾数 × コマ数 だけヒープを踏む
  static let randFunc : unit -> float32 = fun () -> BulletMLManager.GetRandom()

  static member RandFunc = randFunc

  interface IWorld with
    member _.Rand = randFunc
    member _.Rank = BulletMLManager.GetRank ()
    member _.PlayerX = BulletMLManager.GetPlayerPosX ()
    member _.PlayerY = BulletMLManager.GetPlayerPosY ()

    member _.TryTargetFrom (_x, _y, ex, ey) =
      let t = BulletEcsRuntime.EnemyTransform
      if isNull (box t) then false
      else
        let p = t.position
        ex <- p.x
        ey <- p.y
        true

    /// **撃った側と同じ相手。** このフロントは撃った側と同じ場所に弾を作る
    member this.TrySpawnTargetFrom (x, y, ex, ey) =
      (this :> IWorld).TryTargetFrom (x, y, &ex, &ey)

/// このフロントの並び。**2 つ とも 1 か所 だけに書く。**
module EcsFront =

  /// Unity は Y が上向き
  let space = Space.YUp

  /// 撃った弾は撃った側と同じ場所に作る（SpawnChild が親の位置を渡す）
  let origin = SpawnOrigin.AtShooter

/// Entity 1 個 ぶんの弾。**Transform を持たない**（位置は float）。
///
/// 旧は GameObject 1 個 ＋ `DefaultBullet` だった。ECS では
/// 描画も当たり判定も別の仕組みが持つので、ここに残るのは
/// 物理量と、走らせている弾幕（Script）とその実行位置（Run）だけ。
///
/// **managed component（class）。** 中に `BulletmlScript`（参照型）を持つので
/// struct にはできない。Burst もジョブ化も効かないが、C# サンプルも同じ形で、
/// 実測では ECS 側の費用は 1 コマ の 9% ほど（残りは描画）。
///
/// **`Sealed` にしてある。** ただし、それでも TypeManager は
/// 「polymorphic non-sealed class の参照は辿れない」と警告を出す ——
/// **中に入れ子で持つ F# の型が non-sealed だから。**
/// `BulletRun option` も `BulletType` も `ShootingDirection` も判別共用体で、
/// F# はそれを「基底クラス＋サブクラス」に落とす。**構造的に封じられない。**
///
/// **実害は無い**（このサンプルでは）。警告が言っているのは
/// 「中の Entity / Blob / UnityEngine.Object 参照を remap できない」で、
/// remap が要るのは `EntityManager.Instantiate` と SubScene の
/// シリアライズのとき。**どちらもしていない** —— 弾は毎回
/// `CreateEntity` で作り、シーンには保存しない。
///
/// 数は出る（1 走行 で 129 件 を数えた。型の入れ子を再帰で辿るたびに 1 件）。
/// Unity の Console は同じ行を畳むので、Collapse を入れておくとよい。
[<Sealed>]
type BulletSim () =

  /// この弾から見た世界。**弾 1 個 につき 1 個**（口の約束に合わせる）
  let world = EcsWorld () :> IWorld

  member val Entity = Entity.Null with get, set
  member val Kind = BulletKind.Enemy with get, set
  /// 根の弾か。撃たれた弾は false。**当たっても消さない**判定に使う
  member val Root = false with get, set

  member val X = 0.0f with get, set
  member val Y = 0.0f with get, set
  member val AccelerationX = 0.0f with get, set
  member val AccelerationY = 0.0f with get, set
  member val Dir = 0.0f with get, set
  member val Speed = 0.0f with get, set
  member val Used = false with get, set
  member val IsBullet = false with get, set
  /// 自分も子を撃ったか。旧 BulletRoot。**フロントの印で、エンジンは見ない**
  member val BulletRoot = false with get, set

  /// 敵の弾か自機の弾か。**既定値を入れておくこと。**
  /// F# の判別共用体は参照型なので、既定が 0 ではなく null になりうる
  /// （C# サンプルで実際に踏んで NullReferenceException になった）
  member val BulletType = BulletType.Enemy with get, set
  member val ShootingDirection = ShootingDirection.BulletVertical with get, set

  /// 走らせている弾幕。撃たれた弾は親と同じものを引き継ぐ。
  /// **F# の型に null は入れられない**ので defaultof で置く
  member val Script : BulletmlScript = Unchecked.defaultof<BulletmlScript> with get, set
  /// この弾 1 体 の実行位置。**台本が無いあいだは None**
  member val Run : BulletRun option = None with get, set
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish
  member val Finished = false with get, set

  interface IComponentData

  /// 弾幕を割り当てる。根から始めるときは `run` を None にする。
  ///
  /// **根の立場（狙う先と、撃たれた弾か）はここで 1 回 だけ決まる。**
  /// Core へは毎コマ渡らないので、BulletType と IsBullet はこれを呼ぶ前に
  /// 立てておくこと（Spawn が Init の前後で両方 立てている）
  member this.SetScript (script: BulletmlScript, run: BulletRun option) =
    this.Script <- script
    this.Finished <- false
    this.Run <-
      match run with
      | Some _ -> run
      | None ->
          if isNull (box script) then None
          elif this.IsBullet then Some (Runner.newShot this.BulletType script)
          else Some (Runner.newRoot this.BulletType script)

  member this.Init () =
    this.Root <- false
    this.Used <- true
    this.BulletRoot <- false
    this.AccelerationX <- 0.0f
    this.AccelerationY <- 0.0f
    this.Speed <- 0.0f
    this.Dir <- 0.0f
    this.IsBullet <- true
    // 旧はここで task.Init(envOfGlobal this) を呼んで木を歩き直していた
    this.Run <-
      this.Run |> Option.map (fun r ->
        Driver.restart world EcsFront.space EcsFront.origin r this.X this.Y)

  member this.Vanish () = this.Used <- false


  /// 1 コマ 進める。**座標は差分を足す**（`Frame.Delta` は差分で、絶対値ではない）。
  ///
  /// 撃たれた弾は `spawn` へ渡す。旧はエンジンが `GetNewBullet` を呼び返して
  /// 実体を要求していたが、いまは値で受け取るので**フロントが自分の都合で
  /// 実体を作る**（弾プールが尽きたら捨ててよい）。
  member this.Step (spawn: Action<BulletSim, BulletRun>) =
    match this.Run with
    | None -> ()
    | Some rn ->
      if isNull (box this.Script) then () else

      this.ShootingDirection <- this.Script.ShootingDirection
      // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）
      let motion : Motion =
        { Pos = { X = this.X; Y = this.Y }
          Speed = this.Speed
          Dir = this.Dir
          Accel = { X = this.AccelerationX; Y = this.AccelerationY } }

      // Env を組む位置も、台本が無い弾の枝も Driver が持っている
      let f = Driver.step this.Script world EcsFront.space EcsFront.origin rn motion
      let after = f.Run.Motion
      this.Speed <- after.Speed
      this.Dir <- after.Dir
      this.AccelerationX <- after.Accel.X
      this.AccelerationY <- after.Accel.Y
      // **Unity2D の係数と符号。** 1/100 で縮め、Y は反転する
      // （Unity は上が正。MonoGame は 1 倍 で反転しない）
      this.X <- this.X + (f.Delta.X / Settings.Display.PixcelsToUnits)
      this.Y <- this.Y - (f.Delta.Y / Settings.Display.PixcelsToUnits)
      this.Finished <- f.Finished

      for child in f.Spawned do
        this.BulletRoot <- true
        spawn.Invoke(this, child)

      if f.Vanished then this.Vanish ()
      if f.Retired then this.Used <- false

      // 走らせ直しの Env は、位置を更新したあとの自分から組む
      // （旧 DefaultBullet が apply のあとで envOfGlobal を呼ぶのと同じ順）
      this.Run <-
        if f.Finished then
          Some (Driver.restart world EcsFront.space EcsFront.origin f.Run this.X this.Y)
        else Some f.Run
