namespace FsBulletML2
// 旧 API（IBulletmlObject）の Obsolete 警告を、**このファイルだけ**止める。
// ここは旧 API のシムそのもので、旧の型に触るのが仕事だから。
//
// プロジェクト単位（NoWarn）で止めない。止めると、**新しく書いたコードが
// うっかり旧 API を使っても警告が出なくなる**。抑制はいつも、意図して
// 旧経路を使っているファイルの中だけに置く。
#nowarn "44"

open System
open FsBulletML2.Domain
open FsBulletML2.Processable

[<StructAttribute>]
type RunResult =
  val Processed : bool
  val X: float32
  val Y: float32
  new(processed:bool,x:float32, y: float32) =
    { Processed = processed; X = x; Y = y }

module BulletRunner =

  /// 現行のグローバルと弾から Env を組む唯一の場所。旧 API を新経路の上に
  /// 載せるための橋渡し。
  ///
  /// public にしてあるのは、これを呼べない場所（テスト側・フロントエンド側の
  /// Init 呼び出し）が自前で同じレコードを組み直さずに済むようにするため。
  /// 同じ組み方が複数箇所にコピーされると、どれか 1 つが ずれたときに
  /// 全弾幕の軌跡が静かにずれる。呼ぶ側は必ずここを通すこと。
  [<System.Obsolete("新 API（Runner.step）へ移してください。移し方は Api.fs の Runner の但し書き。 Env はフロントが組みます（同梱フロントの loadEnv / EnvAt が例）。")>]
  let envOfGlobal (bullet: IBulletmlObject) : Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = bullet.GetAimDir ()
      EnemyAimDir = bullet.GetEnemyAimDir ()
      SpawnAimDir = bullet.GetSpawnAimDir ()
      SpawnEnemyAimDir = bullet.GetSpawnEnemyAimDir () }

  /// aim を読まないと分かっているコマ用の Env。
  ///
  /// envOfGlobal の aim 4 本 は、フロントエンドの実装がどれも自機／敵との
  /// 差から atan2 を引く（BaseBullet.GetAimDir、DefaultBullet.GetAimDir、
  /// テストの FakeBullet も同じ式の写し）。弾 1 個 × 1 コマ ごとに 4 回 で、
  /// 同梱ベンチの 5way / 10Way では全体の 2 割 を占める。撃ち終わって
  /// 飛んでいるだけの弾が大半なので、その大半が払い損になっている。
  ///
  /// 「読まない」と言えるのは、生きている top が 1 本 も無いとき。Step.step が
  /// env を触るのは top を回すループの中だけで、ループの外（差分の計算・
  /// FireContext の積み直し・Finished の判定）は env を見ない。
  ///
  /// この前提は StepTop の「終わった top しか無いコマは、aim を読まない」で
  /// 門にしてある。step がループの外で env を読むようになったら、
  /// そちらが赤くなる。**前提が消えたことに気づかないまま速い経路を通ると、
  /// aim が黙って 0 になる**ので、門を外さないこと。
  let private envWithoutAim () : Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = 0.0f
      EnemyAimDir = 0.0f
      SpawnAimDir = 0.0f
      SpawnEnemyAimDir = 0.0f }

  /// 角度を 0 〜 2π に丸める。式そのものは Step.calcDir にあり、ここはその別名。
  /// Step.fs はこのファイルより前に compile されるので、逆向き（Step 側が
  /// BulletRunner を指す）にはできない
  let internal calcDir : float32 -> float32 = Step.calcDir

  /// 新経路の Resolvers を組む。BulletmlTask が持ち回る ResolveBulletRef /
  /// ResolveActionRef（RecBulletml を返す関数）をそのまま Step.Resolvers の
  /// 形に包むだけ。Step.Resolvers は Processable.fs より後で compile される
  /// ため、この変換は BulletmlTask 側には置けない
  let internal resolversOf (task: BulletmlTask) : Step.Resolvers =
    { Bullet = task.ResolveBulletRef
      Action = task.ResolveActionRef }

  /// bulletml から根の top* スクリプトと、それぞれの根の Progress を組む。
  /// 木を組む段（旧の toProcessable、消した convertRecBulletmlEx）の写しで、
  /// wait の term だけを document 順にまとめて引く（Step.rootProgress、
  /// 設計文書 5.6）。accel / changeDirection / changeSpeed はこの段では
  /// 引かない。
  ///
  /// 2 か所から呼ばれる。
  ///   convertBulletmlTask  弾オブジェクトへ渡す task を最初に組むとき
  ///   BulletmlTask.Init    Original が Some のとき（旧の Init の
  ///                        `this.Tasks <- toProcessable x` の写し）。
  ///                        Processable.fs はこのファイルより前に compile
  ///                        されるので、BulletmlTask のコンストラクタへ
  ///                        関数として注入して呼んでもらう
  ///
  /// 旧の toProcessable 同様、Init(env) の env 引数は使わない。木を組む段の
  /// Env はグローバルを直に読む（IntermediateParser.fs の RecBulletml.Wait の
  /// 腕がそうだったのと同じ理由。撃つ弾ごとの位置がまだ無いので AimDir /
  /// EnemyAimDir は 0 に固定する）
  let private buildRootTops (bulletml: Bulletml) : (RecActionElm * Progress) list =
    let recBulletml = IntermediateParser.convertRecBulletml bulletml
    let scripts =
      recBulletml
      |> RecOps.getAction
      |> List.filter (function
        | RecActionElm.Action (attrs, _) ->
          match attrs.actionLabel with
          | Some label -> (ActionLabel.text label).StartsWith("top")
          | _ -> false
        | RecActionElm.ActionRef _ -> false)
      |> List.map (RecOps.convertRefActionElm recBulletml)
    let rootEnv : Env =
      { Rand = BulletMLManager.GetRandom
        Rank = BulletMLManager.GetRank ()
        AimDir = 0.f
        EnemyAimDir = 0.f
        SpawnAimDir = 0.f
        SpawnEnemyAimDir = 0.f }
    scripts |> List.map (fun s -> s, Step.rootProgressActionElm rootEnv s)

  /// 弾オブジェクトの現在の物理量と、task が持ち回っている Tops から
  /// BulletState を組む。Tops（実行位置と fire の累積。旧の pa.finish / term /
  /// bulletmlTask.FireData に当たる）だけが弾オブジェクト側に対応が無いので
  /// task から読み、それ以外は毎フレーム弾から読み直す。旧の runWithEnv も
  /// bullet を直に読み書きしていてコピーを別に持たなかったので、それと
  /// 同じに保つ（前フレームの書き戻しと今フレームの読み出しの間に、
  /// フロント側が bullet の物理量を動かすことがあり得るため）
  let internal stateOfBullet (bullet: IBulletmlObject) (task: BulletmlTask) : BulletState =
    { task.State with
        Pos = { X = bullet.X; Y = bullet.Y }
        Speed = bullet.Speed
        Dir = bullet.Dir
        Accel = { X = bullet.AccelerationX; Y = bullet.AccelerationY }
        Kind = bullet.BulletType
        IsBullet = bullet.IsBullet
        HasFired = bullet.BulletRoot }

  /// Spawn を受けて、撃たれた弾へ状態を書き込む。旧の createTask 以降、
  /// fireCommand が newBullet へ書く一連の代入の写し。方向・速さの解決は
  /// Step.fire が済ませているので、ここではその結果（child）を弾オブジェクトへ
  /// 移すだけ。
  ///
  /// bullet 側の direction が aim 系のときも、産まれる弾の位置から見た向きは
  /// env.SpawnAimDir として Step.fire に渡っているので、ここで実体を見て
  /// 仕上げる必要は無い（IBulletmlObject.GetSpawnAimDir / Step.fire 参照）。
  /// 「撃つ」を値として返しきるために、この後付けの解決を外してある。
  ///
  /// GetNewBullet() が null を返したかどうかを呼ぶ側（applyToBullet）へ返す。
  /// 旧 fireCommand は null なら createTask を呼ばず、bullet 側の speed も
  /// fire 側の SrcSpeed / SpeedInit も一切 更新せずに終える
  /// （変換そのもの——getValue の呼び出し——は Step.fire がもう済ませてしまって
  /// いるので、乱数の消費まではここでは巻き戻せない。巻き戻せるのは
  /// SrcSpeed / SpeedInit という「値」だけ。呼ぶ側 applyToBullet 参照）
  let private applySpawn (parent: IBulletmlObject) (task: BulletmlTask) (childIn: BulletState) : bool =
    let newBullet = parent.GetNewBullet ()
    if isNull (box newBullet) then false
    else
      newBullet.Init ()
      let child = childIn
      let scripts = child.Tops |> List.map (fun (s, _, _) -> s)
      let childTask = new BulletmlTask(Step.resetChildActionElm, buildRootTops, scripts, child)
      // 輪を解く入口は、撃たれた弾の task にも引き継ぐ。
      // 引き継がないと、弾の中に残った bulletRef / actionRef を誰も解けない
      childTask.ResolveBulletRef <- task.ResolveBulletRef
      childTask.ResolveActionRef <- task.ResolveActionRef
      // <bulletml type> も同じ理由で引き継ぐ。撃たれた弾の task は
      // convertBulletmlTask を通らないので、ここで渡さないと未設定のまま残る
      childTask.ShootingDirection <- task.ShootingDirection
      newBullet.Task <- Some childTask
      newBullet.X <- child.Pos.X
      newBullet.Y <- child.Pos.Y
      newBullet.Dir <- child.Dir
      newBullet.Speed <- child.Speed
      true

  /// Step.step の結果を弾オブジェクトへ書き戻す。ここが旧 API と新経路の継ぎ目
  let internal applyToBullet (bullet: IBulletmlObject) (task: BulletmlTask) (r: StepResult) =
    // 旧 fireCommand は GetNewBullet() の直前に fire 側の direction（SrcDir）を
    // 確定させたあと、null なら SrcSpeed / SpeedInit を更新せずに終える
    // （BulletRunner.fs の fireCommand 参照。GetNewBullet の後ろにしか
    // speed の解決が無い）。新の Step.fire は GetNewBullet の結果を待てず
    // （まだ弾オブジェクトが無い。値は emit した Spawn として後で渡ってくる）
    // 常に SrcSpeed / SpeedInit も一緒に確定させてしまうので、ここで
    // GetNewBullet が実際に null だったとわかった時点で、SrcDir だけを残して
    // SrcSpeed / SpeedInit をコマ開始時点の値へ戻す。
    //
    // 1 コマに複数回 fire があり、そのうち一部だけが null になる場合は
    // 対応していない（コマ開始時点まで丸ごと戻す）。旧はその場合でも
    // fire ごとに独立して判定するが、そこまでは写していない——GetNewBullet
    // が null を返すのはサンプルの未実装（GetBulletPrefubInstance の既定）
    // でしか届かない経路で、1 コマに複数回 fire しつつ一部だけ null という
    // 組み合わせはさらに稀だと判断した
    let preFc =
      match task.State.Tops with
      | (_, _, fc0) :: _ -> Some fc0
      | [] -> None
    bullet.Dir <- r.State.Dir
    bullet.Speed <- r.State.Speed
    bullet.AccelerationX <- r.State.Accel.X
    bullet.AccelerationY <- r.State.Accel.Y
    task.State <- r.State
    let mutable anySpawnFailed = false
    for effect in r.Effects do
      match effect with
      | Vanished -> bullet.Vanish ()
      | Spawn child -> if not (applySpawn bullet task child) then anySpawnFailed <- true
    if anySpawnFailed then
      match preFc, task.State.Tops with
      | Some pre, ((_, _, curFc) :: _) ->
          let reverted = { pre with SrcDir = curFc.SrcDir }
          task.State <- { task.State with Tops = task.State.Tops |> List.map (fun (s, p, _) -> s, p, reverted) }
      | _ -> ()
    // 旧の「bullet.IsBullet && bullet.BulletRoot なら Used <- false」と同じ。
    // Retired は finished && st.IsBullet && st.HasFired で、この 2 つを畳んだもの
    if r.Retired then bullet.Used <- false
    task.Finish <- r.Finished

  [<CompiledName "Run">]
  [<System.Obsolete("新 API（Runner.step）へ移してください。移し方は Api.fs の Runner の但し書き。")>]
  let run (bullet:IBulletmlObject) =
    match bullet.Task with
    | None ->
        // 返すのは差分。呼ぶ側は足すので、ここで絶対値を返すと座標が膨らむ
        // （膨らむ量は呼ぶ側の係数しだい。同梱では MonoGame が 1 倍、
        //  Unity2D と C# サンプルが 1/100）。
        // 呼ぶ側 4 経路とも Task を先に見ているのでここへは届かないが、
        // ガードを 1 つでも外したら届くので、届いても壊れない形にしておく
        RunResult(true, 0.f, 0.f)
    | Some task ->
      // 未設定は null になりうるので、そのときは弾の値をそのままにする
      if not (isNull (box task.ShootingDirection)) then
        bullet.ShootingDirection <- task.ShootingDirection
      let st = stateOfBullet bullet task
      // 生きている top が 1 本 でもあれば、そのコマは aim を読みうる。
      // 1 本 も無ければ Step.step はループの中へ入らないので読まない
      // （envWithoutAim の但し書きと、それを支える StepTop の門を見ること）
      let env =
        if st.Tops |> List.exists (fun (_, p, _) -> not (Step.isDone p)) then
          envOfGlobal bullet
        else
          envWithoutAim ()
      let r = Step.step (resolversOf task) env st
      applyToBullet bullet task r
      RunResult(r.Finished, r.Delta.X, r.Delta.Y)

  [<CompiledName "ConvertBulletmlTask">]
  [<System.Obsolete("新 API（Runner.load）へ移してください。移し方は Api.fs の Runner の但し書き。")>]
  let convertBulletmlTask bulletml =
    if bulletml :> obj = null then
      let emptyState : BulletState =
        { Pos = { X = 0.f; Y = 0.f }
          Speed = 0.f
          Dir = 0.f
          Accel = { X = 0.f; Y = 0.f }
          Kind = BulletType.Enemy
          IsBullet = false
          HasFired = false
          Tops = [] }
      BulletmlTask(Step.resetChildActionElm, buildRootTops, [], emptyState)
    else
    let recBulletml = IntermediateParser.convertRecBulletml bulletml

    // 根は bulletml しかない（RecBulletml の腕が 1 つ）。
    // 以前はここに `| _ -> failwith "根が bulletml ではない"` があった
    let shootingDirection =
      match recBulletml with
      | RecBulletml.Bulletml(attrs,_) ->
        match attrs.bulletmlType with
        | Some x -> x
        | None -> ShootingDirection.BulletVertical

    let tops = buildRootTops bulletml
    let scripts = tops |> List.map fst
    let initialState : BulletState =
      { Pos = { X = 0.f; Y = 0.f }
        Speed = 0.f
        Dir = 0.f
        Accel = { X = 0.f; Y = 0.f }
        Kind = BulletType.Enemy
        IsBullet = false
        HasFired = false
        Tops = tops |> List.map (fun (s, p) -> s, p, FireContext.zero) }

    let bulletmlTask = new BulletmlTask(Step.resetChildActionElm, buildRootTops, scripts, initialState)
    bulletmlTask.ResolveBulletRef <- RecOps.expandBulletRefOnceRec recBulletml
    bulletmlTask.ResolveActionRef <- RecOps.expandActionRefOnceRec recBulletml
    bulletmlTask.ShootingDirection <- shootingDirection
    bulletmlTask

  [<CompiledName "ConvertBulletmlTaskOption">]
  [<System.Obsolete("新 API（Runner.load）へ移してください。移し方は Api.fs の Runner の但し書き。")>]
  let convertBulletmlTaskOption bulletml =
    convertBulletmlTask bulletml |> Some
