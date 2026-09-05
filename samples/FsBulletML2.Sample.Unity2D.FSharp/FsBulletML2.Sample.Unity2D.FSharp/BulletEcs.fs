namespace FsBulletML2.Sample.Unity2D.FSharp

open System
open Unity.Entities
open UnityEngine
open FsBulletML2
open FsBulletML2.Domain
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

/// このサンプルが `Env` を組むところ。**4 本 の aim を入れる場所はここだけ。**
///
/// 旧は `FsBulletML2.Unity2D.DefaultBullet` が組んでいたが、あれは
/// `Transform` を持つ GameObject 前提。ECS の弾は Transform を持たない
/// （位置は float で持ち、描画のときだけ `LocalTransform` へ写す）ので、
/// ここで組み直す。
///
/// **式は Unity2D のフロント固有。** MonoGame とは 2 つ 違う。
///
///   Y の符号     こちらは反転しない（Unity は上が正）
///   Spawn の元   こちらは撃った側と同じ場所に作るので AimDir と同値
///
/// 散らすと `AimDir` に `SpawnAimDir` を入れるような取り違えを門で当てられない
/// （型はどれも float32 なので通ってしまう）。
[<AbstractClass; Sealed>]
type FrontEnv private () =

  /// `Env.Rand` に入れる関数値。**1 個 だけ作って使い回す。**
  /// 中身はグローバル（BulletMLManager）を読むだけなので、いつ作っても同じ。
  /// 毎コマ 作ると弾数 × コマ数 だけヒープを踏む
  static let randFunc : unit -> float32 = fun () -> BulletMLManager.GetRandom()

  /// 自機を狙う向き。旧 DefaultBullet.AimDir の式そのまま
  static member AimAtPlayer (x: float32) (y: float32) : Vec2 =
    { X = BulletMLManager.GetPlayerPosX() - x; Y = BulletMLManager.GetPlayerPosY() - y }

  /// このコマの Env を、いまの位置から組む。
  /// **組む位置が変わると aim がずれる**ので、step の直前（差分を足す前）に組む。
  ///
  /// `enemyAim` を引数で受けるのは、狙う相手の選び方が場面で違うため。
  /// 残り 3 本 はグローバルと位置だけで決まるので、ここに閉じている。
  static member At (x: float32) (y: float32) (enemyAim: Vec2) : Env =
    let aim = FrontEnv.AimAtPlayer x y
    { Rand = randFunc
      Rank = BulletMLManager.GetRank ()
      AimVec = aim
      EnemyAimVec = enemyAim
      // 産まれた弾は撃った側と同じ場所に作る（SpawnChild が親の位置を渡す）
      SpawnAimVec = aim
      SpawnEnemyAimVec = enemyAim }

  /// aim を読まないと分かっているコマの Env。差分 4 本 を 0 に。
  /// 使ってよい条件は `BulletRun.HasNoScript` の但し書き。
  /// **`At` と欄が 1 つ でもずれたら、片方だけ直したということ**
  static member NoAim () : Env =
    { Rand = randFunc
      Rank = BulletMLManager.GetRank ()
      AimVec = { X = 0.0f; Y = 0.0f }
      EnemyAimVec = { X = 0.0f; Y = 0.0f }
      SpawnAimVec = { X = 0.0f; Y = 0.0f }
      SpawnEnemyAimVec = { X = 0.0f; Y = 0.0f } }

  /// 弾幕を読む段の Env。中身は NoAim と同じだが**意味が違うので名前を分ける**。
  /// 木を組む段は撃つ弾ごとの位置がまだ無いので aim を読まない
  static member Load () : Env = FrontEnv.NoAim ()

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

  /// 弾幕を割り当てる。根から始めるときは `run` を None にする
  member this.SetScript (script: BulletmlScript, run: BulletRun option) =
    this.Script <- script
    this.Finished <- false
    this.Run <-
      match run with
      | Some _ -> run
      | None -> if isNull (box script) then None else Some (Runner.newRoot script)

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
    this.Run <- this.Run |> Option.map (fun r -> Runner.restart (this.EnvNow r) r)

  member this.Vanish () = this.Used <- false

  /// いちばん近い敵を狙う向き。旧 DefaultBullet.EnemyAimDir の式そのまま。
  /// この場面では敵が 1 体 しか居ないので、毎コマ 選び直しても相手が変わらない
  member this.EnemyAimDir () : Vec2 =
    let t = BulletEcsRuntime.EnemyTransform
    if isNull (box t) then { X = 0.0f; Y = 0.0f }
    else
      let p = t.position
      { X = p.x - this.X; Y = p.y - this.Y }

  /// このコマの Env。**台本が無い弾は aim を読まない**ので、
  /// そのときは Atan2 を 4 本 とも省く（`BulletRun.HasNoScript` の但し書き）
  member this.EnvNow (run: BulletRun) : Env =
    if run.HasNoScript then FrontEnv.NoAim ()
    else FrontEnv.At this.X this.Y (this.EnemyAimDir ())

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
      let body =
        { Pos = { X = this.X; Y = this.Y }
          Speed = this.Speed
          Dir = this.Dir
          Accel = { X = this.AccelerationX; Y = this.AccelerationY }
          Kind = this.BulletType
          IsBullet = this.IsBullet
          HasFired = this.BulletRoot }

      let f = Runner.stepWith this.Script (this.EnvNow rn) rn body
      let after = f.Run.Body
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
        if f.Finished then Some (Runner.restart (this.EnvNow f.Run) f.Run)
        else Some f.Run
