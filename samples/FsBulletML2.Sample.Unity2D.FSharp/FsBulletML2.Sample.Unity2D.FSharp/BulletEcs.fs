namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open Unity.Entities
open UnityEngine
open FsBulletML2
open FsBulletML2.Domain
open FsBulletML2.Front
// Settings.Display.PixcelsToUnits（座標の係数）はこちら
open FsBulletML2.Unity2D

/// 弾がどちら側のものか。エンジンの BulletType とは別
/// —— あちらは弾幕の意味（aim の向き）、こちらは当たり判定と見た目の分け。
type BulletKind =
  | Enemy = 0uy
  | Player = 1uy

/// Entity に付ける印。当たり判定の半径もここ。
/// struct なので Burst からも触れる（いまは触っていない）
[<Struct>]
type BulletTag =
  val mutable Kind : BulletKind
  val mutable Radius : float32
  interface IComponentData

/// 毎コマ `FindAnyObjectByType` しない。Bootstrap が 1 回 入れる。
[<AbstractClass; Sealed>]
type BulletEcsRuntime private () =
  static member val PlayerTransform : Transform = null with get, set
  static member val EnemyTransform : Transform = null with get, set
  static member val PlayerRadius = 0.15f with get, set
  static member val EnemyRadius = 0.25f with get, set

/// この ECS の弾が `FsBulletML2.Front` の口に答えるところ。
[<Sealed>]
type EcsEnv() =

  /// `Env.Rand` に入れる関数値。
  /// 毎コマ 作ると弾数 × コマ数 だけヒープを踏む
  static let randFunc : unit -> float32 = fun () -> BulletMLManager.GetRandom()

  static member RandFunc = randFunc

  interface IFrontEnv with
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

    /// 撃った側と同じ相手。 このフロントは撃った側と同じ場所に弾を作る
    member this.TrySpawnTargetFrom (x, y, ex, ey) =
      (this :> IFrontEnv).TryTargetFrom (x, y, &ex, &ey)

/// このフロントの並び。2 つ とも 1 か所 だけに書く。
module EcsFront =

  /// Unity は Y が上向き
  let space = Space.YUp

  /// 撃った弾は撃った側と同じ場所に作る（SpawnChild が親の位置を渡す）
  let origin = SpawnOrigin.AtShooter

/// Entity 1 個 ぶんの弾。
[<Sealed>]
type BulletSim () =

  /// この弾から見た世界。
  /// interface や非 sealed の class が居ると「判断できない」と警告が出る。
  let front = EcsEnv ()

  member val Entity = Entity.Null with get, set
  member val Kind = BulletKind.Enemy with get, set
  /// 根の弾か。撃たれた弾は false。当たっても消さない判定に使う
  member val Root = false with get, set

  member val X = 0.0f with get, set
  member val Y = 0.0f with get, set
  member val AccelerationX = 0.0f with get, set
  member val AccelerationY = 0.0f with get, set
  member val Dir = 0.0f with get, set
  member val Speed = 0.0f with get, set
  member val Used = false with get, set
  member val IsBullet = false with get, set
  /// 自分も子を撃ったか。旧 BulletRoot。フロントの印で、エンジンは見ない
  member val BulletRoot = false with get, set

  /// 敵の弾か自機の弾か。
  /// F# の判別共用体は参照型なので、既定が 0 ではなく null になりうる （C# サンプルで実際に踏んで NullReferenceException になった）
  member val BulletType = BulletType.Enemy with get, set
  member val ShootingDirection = ShootingDirection.BulletVertical with get, set

  /// 走らせている弾幕。実行位置の中に居る（`BulletRun.Script`）
  member this.Script =
    match this.Run with
    | Some r -> r.Script
    | None -> Unchecked.defaultof<BulletmlScript>
  /// この弾 1 体 の実行位置。台本が無いあいだは None
  member val Run : BulletRun option = None with get, set
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish
  member val Finished = false with get, set

  interface IComponentData

  /// 弾幕を割り当てて根から始める。
  member this.SetScript (script: BulletmlScript) =
    this.Finished <- false
    this.Run <-
      if isNull (box script) then None
      elif this.IsBullet then Some (Runner.newShot this.BulletType script)
      else Some (Runner.newRoot this.BulletType script)

  /// 撃たれた弾を、エンジンから受け取った実行位置で始める。
  /// 弾幕を渡す口が無い —— `BulletRun` が親のものを持っている
  member this.SetRun (run: BulletRun) =
    this.Finished <- false
    this.Run <- Some run

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
        Driver.restart (front :> IFrontEnv) r)

  member this.Vanish () = this.Used <- false


  /// 1 コマ 進める。
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
      let f = Driver.step (front :> IFrontEnv) EcsFront.space EcsFront.origin rn motion
      let after = f.Run.Motion
      this.Speed <- after.Speed
      this.Dir <- after.Dir
      this.AccelerationX <- after.Accel.X
      this.AccelerationY <- after.Accel.Y
      // Unity2D の係数と符号。 1/100 で縮め、Y は反転する
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
          Some (Driver.restart (front :> IFrontEnv) f.Run)
        else Some f.Run
