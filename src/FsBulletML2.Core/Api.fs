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
///
/// **[<Struct>] にしてある。** フロントは 1 弾 1 コマ ごとに run.Body で読み、
/// WithBody で書き戻す。参照型のままだとその往復で毎回 ヒープを踏み、
/// 弾の数に比例して確保が増える。
///
/// 実測（同じプロセスで旧 API と並べた）: struct にする前は 5way で確保が
/// 旧の +33%、時間が +25% だった。**確保の増え方と遅さが同じ台本に同じ形で
/// 出て、弾 1 個 の move では逆に減っていた**ので、往復の割り当てが
/// 出どころと読んだ。
[<Struct>]
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
///
/// **[<Struct>] にしてある。** step が毎コマ 1 個、WithBody / restart も
/// 1 個ずつ作る型で、弾数に比例する。中身は BulletState への参照 1 本 なので
/// 箱は 8 バイト。struct は暗黙に sealed なので [<Sealed>] は付けない。
///
/// **値型なので既定値（state が null）を作れてしまう。** 外からは
/// コンストラクタが internal なので届かず、Core の中でも既定値は作っていない。
/// Runner.newRoot / step / restart / WithBody のどれかを通ったものだけが
/// フロントへ出る。
[<Struct>]
type BulletRun internal (state: BulletState) =

  member internal _.State = state

  /// 走らせる台本が 1 本 も無い。**このコマは aim を読まない。**
  ///
  /// 撃たれただけで自分の action を持たない弾（5way / 10Way では 96.7%）が
  /// これに当たる。フロントは Env を組む前にここを見て、**読まれないと
  /// 分かっている aim を計算しないで済む** ——
  ///
  ///   let env =
  ///     if run.HasNoScript then { Rand = r; Rank = k
  ///                               AimDir = 0.0f; EnemyAimDir = 0.0f
  ///                               SpawnAimDir = 0.0f; SpawnEnemyAimDir = 0.0f }
  ///     else 本物の aim を組む
  ///
  /// **なぜ「生きている top が無い」ではなく「台本が無い」なのか。**
  /// step だけなら前者でよい（StepTop の門がその前提を留めている）が、
  /// フロントは Finished のコマで restart も呼ぶ。restart は
  /// changeDirection type="aim" の term を引き直すので **aim を読みうる**。
  /// 台本が空なら restart は空を歩くだけなので、両方 まとめて安全。
  ///
  /// 効きの大きさ（実測・5way 60 コマ）: Env を 1 回 組むのが 23.02 ns。
  /// 1 走行の Env 構築が 17,820 回 で、うち 96.7% がこれに当たるので、
  /// 積は 397 us。走行そのものが 1,578 us なので **時間の 25%** が上界。
  /// 実測は 1,578 → 1,197 us（−24.2%）で、ほぼ天井まで取れた。
  ///
  /// **確保は 1 バイト も減らない。** 同梱フロントの noAimEnv は「aim を 0 に
  /// した Env を組む」ので、record の割り当てはそのまま残る。省けているのは
  /// Atan2 4 本 の計算だけ。実測でも 5,722.33 → 5,722.34 KB と動いていない。
  /// **確保は決定的な数なので、この「動かなかった」は結果として読める**
  /// —— 動いていたら skip 以外の何かも一緒に変わっている。
  ///
  /// move と homing は死んだコマが 0 なので効かない（`--counts` の「対照」）。
  /// 実測も −4.2% / −1.2% で、この台のノイズ床のうち。
  ///
  /// **時間だけが消えて確保が動かない**のは、費用の中身がレコードではなく
  /// Atan2 だから —— aim 4 本 を組む 29.8 ns のうち 28.5 ns（96%）が Atan2 で、
  /// レコードの確保は 1.3 ns。内訳は bench/FsBulletML2.Benchmarks/BREAKDOWN.md
  member _.HasNoScript = List.isEmpty state.Tops

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
///
/// **[<Struct>] にしてある。** 弾 1 個 × 1 コマ ごとに必ず 1 個 出るので、
/// 参照型だと弾数に比例してヒープを踏む（Body / Vec2 と同じ理由）。
/// 中の Run と Spawned は参照なので、この箱自体は小さい。
[<Struct>]
type Frame =
  { /// 次のコマへ持ち越す実行状態
    Run : BulletRun
    /// 座標の**差分**。呼ぶ側が足す。絶対値ではない
    Delta : Vec2
    /// このコマで撃たれた弾。**このコマでは回さない**（旧の決めと同じで、
    /// 産まれた弾は次のコマから回る）
    ///
    /// **フロントが「撃つのを断る」口は無い。** 弾プールが尽きても、エンジンは
    /// 撃った弾を値で返しきる。捨てるかどうかはフロントの仕事。
    ///
    /// 旧 API（IBulletmlObject.GetNewBullet）は null を返せて、そのとき fire の
    /// sequence の累積（FireContext.SrcSpeed）を進めなかった。**その意味論は
    /// ここへ持ってこないと決めた。** 参照実装を 2 本 当たった結果:
    ///
    ///   libbulletml (C++)   createBullet / createSimpleBullet は戻り値なし。
    ///                       **断る口がそもそも無い。** runFire は setSpeed /
    ///                       setDirection を無条件に先に呼ぶ
    ///   BulletMLLib (C#)    CreateBullet() は null を返せて、null なら
    ///                       TaskFinished <- true; End（旧 API と同じ形）。
    ///                       だが向きと速さは手前の SetupTask で計算済みで、
    ///                       **断ったかどうかと無関係**
    ///
    /// **どちらも「断ると累積が止まる」ようにはなっていない。** 旧 API のあれは
    /// 移植のときに入った独自の振る舞いで、しかも SrcDir は進んで SrcSpeed だけ
    /// 止まる非対称だった（tests/.../NullNewBullet.fs の但し書き）。
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
///   // 読む段（弾幕 1 本 につき 1 回）。この段では aim は読まれない
///   let loadEnv =
///     { Rand = rand; Rank = rank
///       AimDir = 0.0f; EnemyAimDir = 0.0f
///       SpawnAimDir = 0.0f; SpawnEnemyAimDir = 0.0f }
///   let script = Runner.load loadEnv (readXmlString xml)
///   let mutable run = Runner.newRoot script
///
///   // 毎コマ
///   let env = { loadEnv with AimDir = ...; EnemyAimDir = ... }
///   let f = Runner.stepWith script env run { run.Body with Pos = myPos }
///   myPos <- myPos + f.Delta
///   run <- f.Run
///   for child in f.Spawned do ...
///
/// **この例は tests/FsBulletML2.Core.Tests/ApiUsageExample.fs で実際に動かして
/// ある。** ここはコメントなのでコンパイルされず、段階 4 で load に rootEnv が
/// 増えたときも古い形（引数 1 つ）のまま残っていた。**例を直したらあちらも、
/// あちらが赤くなったらここも。**
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
      { Bullet = RecOps.expandBulletRefOnceRec rec'
        Action = RecOps.expandActionRefOnceRec rec' }
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
      |> RecOps.getAction
      |> List.filter (function
        | RecActionElm.Action (attrs, _) ->
            match attrs.actionLabel with
            | Some label -> (ActionLabel.text label).StartsWith "top"
            | None -> false
        | _ -> false)
      |> List.map (RecOps.convertRefActionElm rec')
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
  let inline private toFrame (r: StepResult) : Frame =
    let mutable vanished = false
    let mutable spawned = []
    for effect in r.Effects do
      match effect with
      | Vanished -> vanished <- true
      | Spawn child -> spawned <- BulletRun child :: spawned
    { Run = BulletRun r.State
      Delta = r.Delta
      Spawned = List.rev spawned
      Vanished = vanished
      Finished = r.Finished
      Retired = r.Retired }

  [<CompiledName "Step">]
  let step (script: BulletmlScript) (env: Env) (run: BulletRun) : Frame =
    toFrame (Step.step script.Resolvers env run.State)

  /// 物理量を入れ替えてから 1 コマ進める。**フロントはふつうこちらを使う。**
  ///
  /// `step script env (run.WithBody body)` と答えは同じだが、**中間の
  /// BulletRun を作らない。**
  ///
  /// なぜ分けたか。フロントは弾の位置を自分で持っていて、毎コマ入れ直す
  /// （旧 stateOfBullet の規約）。それを WithBody で書くと 1 弾 1 コマ ごとに
  /// BulletRun が 1 個 余分に出る。**弾の数に比例するので、弾幕では効く。**
  ///
  /// 実測（同じプロセスで旧 API と並べた 5way / 60 コマ）:
  ///   WithBody 経由  6,211 KB   旧 API 比 +13.2%
  ///   こちら         5,722 KB   旧 API 比  +4.3%
  [<CompiledName "StepWith">]
  let stepWith (script: BulletmlScript) (env: Env) (run: BulletRun) (b: Body) : Frame =
    let st = run.State
    let st =
      { st with
          Pos = b.Pos
          Speed = b.Speed
          Dir = b.Dir
          Accel = b.Accel
          Kind = b.Kind
          IsBullet = b.IsBullet
          HasFired = b.HasFired }
    toFrame (Step.step script.Resolvers env st)
