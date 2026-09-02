namespace FsBulletML2

open FsBulletML2.DTD
open FsBulletML2.Domain

/// 弾の物理量。フロントが持ち、毎コマ渡して毎コマ受け取る。
///
/// BulletState からエンジンの内部（台本と実行位置）を抜いた残り。
/// 数だけなので公開してよい。前のコマの答えをそのまま返す必要はなく、
/// **フロントが自分で動かした結果を入れてよい**（画面外へ弾いた、
/// 親の位置へ移した、など）。旧の runWithEnv も毎コマ弾から読み直していて、
/// その規約をここへ移してある。
type Body =
  { Pos : Vec2
    Speed : float32
    Dir : float32
    Accel : Vec2
    Kind : BulletType
    /// 撃たれた弾か。根の敵は false
    IsBullet : bool
    /// 自分も子を撃ったか。旧の BulletRoot
    HasFired : bool }

module Body =

  let zero =
    { Pos = { X = 0.0f; Y = 0.0f }
      Speed = 0.0f
      Dir = 0.0f
      Accel = { X = 0.0f; Y = 0.0f }
      Kind = BulletType.Enemy
      IsBullet = false
      HasFired = false }

/// 1 体の実行状態。**中身は不透明。**
///
/// 台本（RecActionElm）と実行位置（Progress）を持つが、どちらもエンジンの
/// 内部形なので外へ出さない。出すと、木の形を変えるたびに公開 API の
/// 破壊的変更になる。フロントは受け取って持ち歩き、次のコマでそのまま返す。
///
/// 物理量だけは Body で出し入れできる。
[<Sealed>]
type BulletRun internal (state: BulletState) =

  member internal _.State = state

  member _.Body : Body =
    { Pos = state.Pos
      Speed = state.Speed
      Dir = state.Dir
      Accel = state.Accel
      Kind = state.Kind
      IsBullet = state.IsBullet
      HasFired = state.HasFired }

  /// 物理量を差し替える。台本と実行位置はそのまま持ち越す
  member _.WithBody (b: Body) =
    BulletRun
      { state with
          Pos = b.Pos
          Speed = b.Speed
          Dir = b.Dir
          Accel = b.Accel
          Kind = b.Kind
          IsBullet = b.IsBullet
          HasFired = b.HasFired }

/// 読み込んだ弾幕。**中身は不透明。**
///
/// 輪を解く入口（bulletRef / actionRef を 1 段だけ解く）と、根の top* と、
/// bulletml の type を持つ。1 本 の弾幕につき 1 個 作って、そこから出た弾
/// 全部で使い回す —— 撃たれた弾の中に残った参照も、同じ入口で解ける。
[<Sealed>]
type BulletmlScript internal (resolvers: Step.Resolvers,
                              shootingDirection: ShootingDirection,
                              rootState: BulletState) =

  member internal _.Resolvers = resolvers

  /// bulletml の type。弾の見た目や向きの基準にフロントが使う
  member _.ShootingDirection = shootingDirection

  member internal _.RootState = rootState

/// 1 コマの結果。
type Frame =
  { /// 次のコマへ持ち越す実行状態
    Run : BulletRun
    /// 座標の**差分**。呼ぶ側が足す。絶対値ではない
    Delta : Vec2
    /// このコマで撃たれた弾。**このコマでは回さない**（旧の決めと同じで、
    /// 産まれた弾は次のコマから回る）
    Spawned : BulletRun list
    /// vanish された。フロントはこの弾を消す
    Vanished : bool
    /// 全 top が終わった。旧の RunResult.Processed
    Finished : bool
    /// 終わったうえで、撃たれた弾でありかつ自分も撃った。回収してよい。
    /// 旧の Used <- false
    Retired : bool }

/// 走らせる入口。**これが Core の顔。**
///
/// 旧 API（IBulletmlObject の 19 メンバを実装して BulletRunner.run に渡す）は
/// エンジンがフロントを呼び返す形だったので、実装する側が「いつ呼ばれるか」を
/// 知らないと書けなかった。ここはフロントが値を渡して値を受け取るだけで、
/// 呼び返しが無い。
///
///   let script = Runner.load (readXmlString xml)
///   let mutable run = Runner.newRoot script
///   // 毎コマ
///   let env = { Rand = ...; Rank = ...; AimDir = ...; ... }
///   let f = Runner.step script env (run.WithBody { run.Body with Pos = myPos })
///   myPos <- myPos + f.Delta
///   run <- f.Run
///   for child in f.Spawned do ...
module Runner =

  /// 弾幕を読む。1 本 につき 1 回。
  ///
  /// **Env を受け取るのは、木を組む段が wait の term をその場で引くため。**
  /// 引く回数と順が乱数の並びを決めるので、ここを省くとグローバルから
  /// こっそり引くことになる（旧 BulletRunner.buildRootTops がそうだった）。
  /// この段では撃つ弾ごとの位置がまだ無いので、AimDir / EnemyAimDir は
  /// 読まれない —— 引かれるのは wait だけ（設計文書 5.6）。
  [<CompiledName "Load">]
  let load (rootEnv: Env) (bulletml: Bulletml) : BulletmlScript =
    let rec' = IntermediateParser.convertRecBulletml bulletml
    let resolvers : Step.Resolvers =
      { Bullet = IntermediateParser.expandBulletRefOnceRec rec'
        Action = IntermediateParser.expandActionRefOnceRec rec' }
    // 根は bulletml しかない（RecBulletml の腕が 1 つ）
    let shootingDirection =
      match rec' with
      | RecBulletml.Bulletml (attrs, _) ->
          match attrs.bulletmlType with
          | Some x -> x
          | None -> ShootingDirection.BulletVertical
    // top* の並びは旧の toProcessable と同じ選び方（label が top で始まる action）
    let scripts =
      rec'
      |> IntermediateParser.getAction
      |> List.filter (function
        | RecActionElm.Action (attrs, _) ->
            match attrs.actionLabel with
            | Some label -> (ActionLabel.text label).StartsWith "top"
            | None -> false
        | _ -> false)
      |> List.map (IntermediateParser.convertRefActionElm rec')
    let rootState =
      { Pos = { X = 0.0f; Y = 0.0f }
        Speed = 0.0f
        Dir = 0.0f
        Accel = { X = 0.0f; Y = 0.0f }
        Kind = BulletType.Enemy
        IsBullet = false
        HasFired = false
        Tops =
          scripts
          |> List.map (fun s -> s, Step.rootProgressActionElm rootEnv s, FireContext.zero) }
    BulletmlScript (resolvers, shootingDirection, rootState)

  /// 根の弾（敵そのもの）の実行状態
  [<CompiledName "NewRoot">]
  let newRoot (script: BulletmlScript) : BulletRun =
    BulletRun script.RootState

  /// 全 top が終わった弾を、最初から走らせ直す。旧の task.Init(env)。
  ///
  /// **Core からは自動で呼ばない。** 走らせ直すかどうかはフロントの決めごとで、
  /// 同梱のフロント 2 つ は Finished のコマで呼んでいる。
  ///
  /// 走らせ直しは wait / changeDirection / changeSpeed の term を引き直す
  /// （accel は引かない。設計文書 5.3 / 5.6）ので、**呼ぶか呼ばないかで
  /// 乱数の並びが変わる。** 途中で方針を変えると全弾幕の軌跡が動く。
  [<CompiledName "Restart">]
  let restart (env: Env) (run: BulletRun) : BulletRun =
    let st = run.State
    BulletRun
      { st with
          Tops =
            st.Tops
            |> List.map (fun (s, _, fc) -> s, Step.resetChildActionElm env s, fc) }

  /// 1 コマ進める
  [<CompiledName "Step">]
  let step (script: BulletmlScript) (env: Env) (run: BulletRun) : Frame =
    let r = Step.step script.Resolvers env run.State
    let mutable vanished = false
    let spawned = ResizeArray<BulletRun>()
    for effect in r.Effects do
      match effect with
      | Vanished -> vanished <- true
      | Spawn child -> spawned.Add (BulletRun child)
    { Run = BulletRun r.State
      Delta = r.Delta
      Spawned = List.ofSeq spawned
      Vanished = vanished
      Finished = r.Finished
      Retired = r.Retired }
