namespace FsBulletML
open System
open FsBulletML.Domain
open FsBulletML.Processable

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
  /// 同じ 4 フィールドの組み方が複数箇所にコピーされると、どれか 1 つが
  /// ずれたときに全弾幕の軌跡が静かにずれる。呼ぶ側は必ずここを通すこと。
  let envOfGlobal (bullet: IBulletmlObject) : Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = bullet.GetAimDir ()
      EnemyAimDir = bullet.GetEnemyAimDir () }

  /// 角度を 0 〜 2π に丸める。式そのものは Step.calcDir にあり、ここはその別名。
  /// Step.fs はこのファイルより前に compile されるので、逆向き（Step 側が
  /// BulletRunner を指す）にはできない
  let internal calcDir : float32 -> float32 = Step.calcDir

  /// 新経路の Resolvers を組む。BulletmlTask が持ち回る ResolveBulletRef /
  /// ResolveActionRef（RecBulletml を返す関数）をそのまま Step.Resolvers の
  /// 形に包むだけ。Step.Resolvers は Processable.fs より後で compile される
  /// ため、この変換は BulletmlTask 側には置けない
  let private resolversOf (task: BulletmlTask) : Step.Resolvers =
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
  let private buildRootTops (bulletml: Bulletml) : (RecBulletml * Progress) list =
    let recBulletml = IntermediateParser.convertRecBulletml bulletml
    let scripts =
      recBulletml
      |> IntermediateParser.getAction
      |> List.filter (function
        | RecBulletml.Action (attrs, _) ->
          match attrs.actionLabel with
          | Some label -> label.StartsWith("top")
          | _ -> false
        | _ -> false)
      |> List.map (IntermediateParser.convertRefBulletml recBulletml)
    let rootEnv : Env =
      { Rand = BulletMLManager.GetRandom
        Rank = BulletMLManager.GetRank ()
        AimDir = 0.f
        EnemyAimDir = 0.f }
    scripts |> List.map (fun s -> s, Step.rootProgress rootEnv s)

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
  /// fireCommand が newBullet へ書く一連の代入の写し。方向・速さの解決の
  /// 大半は Step.fire が既に済ませているので、ここではその結果
  /// （child）を弾オブジェクトへ移すだけ ——
  /// ただし bullet 側の direction が aim 系だった 1 か所だけは例外。
  ///
  /// 旧 createTask は GetNewBullet() が返した新しい弾オブジェクトの
  /// GetAimDir() / GetEnemyAimDir() を、撃った側の位置をコピーする前に
  /// 読んで bullet 側の aim を解決していた。Step.fire の時点では撃たれた弾の
  /// 実オブジェクトがまだ無いのでそこでは解決できず、PendingBulletAim を
  /// 立てて child.Dir に revise 済みの角度だけを残してある
  /// （Domain.BulletState 参照）。ここで newBullet を得た直後・位置を
  /// コピーする前に、newBullet 自身の GetAimDir() / GetEnemyAimDir() を
  /// 読んで仕上げる。
  ///
  /// この瞬間の newBullet の位置がどうなっているかはフロントエンドしだい
  /// （FakeBullet / BaseBullet.GetNewBullet は Init 直後で未設定＝原点のまま、
  /// Unity2D の ECS 実装 BulletEntityFactory.SpawnFromEmitter は撃った側の
  /// 位置をコンストラクタで先にコピーしている）。位置が何であるかは
  /// ここでは前提にしていない —— 「いま実際に生きている newBullet を読む。
  /// 撃った側を読まない」という 1 点だけが要る。両方とも生きているオブジェクトを
  /// 読んでいる点は同じなので、位置の値そのものが違っても解決結果は
  /// 旧の createTask と同じ形になる。
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
      let child =
        if childIn.PendingBulletAim then
          // childIn.Kind は撃った側（self.Kind）の種別 —— Step.fire が
          // child を組むときに Kind = self.Kind としているため。旧 createTask
          // はここを newBullet.BulletType（新しい弾自身の種別）で判定していた。
          // 2 つを同じものとして扱ってよいのは、GetNewBullet() の実装が
          // 例外なく撃った側の種別を新しい弾へその場でコピーしているから
          // （FakeBullet.GetNewBullet: c.BulletType <- bulletType、
          // BaseBullet.GetNewBullet: newBullet.BulletType <- this.self.BulletType、
          // BulletEntityFactory.SpawnFromEmitter: emitter.BulletType を見て
          // 新しい弾の BulletType を決める）。この前提が崩れる
          // GetNewBullet 実装がリポジトリに増えたら、ここは newBullet.BulletType
          // を読み直す形に変える必要がある
          let aim =
            if childIn.Kind = BulletType.Player then newBullet.GetEnemyAimDir ()
            else newBullet.GetAimDir ()
          { childIn with Dir = calcDir (aim + childIn.Dir); PendingBulletAim = false }
        else childIn
      let scripts = child.Tops |> List.map (fun (s, _, _) -> s)
      let childTask = new BulletmlTask(Step.resetChild, buildRootTops, scripts, child)
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
      let env = envOfGlobal bullet
      let st = stateOfBullet bullet task
      let r = Step.step (resolversOf task) env st
      applyToBullet bullet task r
      RunResult(r.Finished, r.Delta.X, r.Delta.Y)

  [<CompiledName "ConvertBulletmlTask">]
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
          Tops = []
          PendingBulletAim = false }
      BulletmlTask(Step.resetChild, buildRootTops, [], emptyState)
    else
    let recBulletml = IntermediateParser.convertRecBulletml bulletml

    let shootingDirection =
      match recBulletml with
      | RecBulletml.Bulletml(attrs,_) ->
        match attrs.bulletmlType with
        | Some x -> x
        | None -> ShootingDirection.BulletVertical
      | _ -> failwith "convertBulletmlTask: 根が bulletml ではない"

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
        Tops = tops |> List.map (fun (s, p) -> s, p, FireContext.zero)
        PendingBulletAim = false }

    let bulletmlTask = new BulletmlTask(Step.resetChild, buildRootTops, scripts, initialState)
    bulletmlTask.ResolveBulletRef <- IntermediateParser.expandBulletRefOnceRec recBulletml
    bulletmlTask.ResolveActionRef <- IntermediateParser.expandActionRefOnceRec recBulletml
    bulletmlTask.ShootingDirection <- shootingDirection
    bulletmlTask

  [<CompiledName "ConvertBulletmlTaskOption">]
  let convertBulletmlTaskOption bulletml =
    convertBulletmlTask bulletml |> Some
