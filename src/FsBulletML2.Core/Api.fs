namespace FsBulletML2

open FsBulletML2.Domain

/// 弾の物理量。数だけを持ち、立場はエンジン側に置く。
/// 前のコマの答えをそのまま返さなくてよい。フロントが動かした結果を入れてよい。
[<Struct>]
type Motion =
    {
        Pos: Vec2
        Speed: float32
        Dir: float32
        Accel: Vec2
    }

module Motion =

    let zero =
        {
            Pos = { X = 0.0f; Y = 0.0f }
            Speed = 0.0f
            Dir = 0.0f
            Accel = { X = 0.0f; Y = 0.0f }
        }

/// 読み込んだ弾幕。中身は不透明。1 本につき 1 個作り、出た弾全部で使い回す。
[<Sealed>]
type BulletmlScript internal (resolvers: Step.Resolvers, shootingDirection: ShootingDirection, rootState: BulletState) =

    member internal _.Resolvers = resolvers

    /// bulletml の type。弾の見た目や向きの基準にフロントが使う
    member _.ShootingDirection = shootingDirection

    member internal _.RootState = rootState

/// 1 体の実行状態。中身は不透明。物理量だけ Motion / WithMotion で出し入れする。
[<Struct>]
type BulletRun internal (script: BulletmlScript, state: BulletState) =

    member internal _.State = state

    /// 走らせている弾幕。撃たれた弾は親のものを引き継ぐ。
    member _.Script = script

    /// 台本が 1 本も無い。このコマは aim を読まない。Env を組む前に見てよい。
    member _.HasNoScript = List.isEmpty state.Tops

    member _.Motion: Motion =
        {
            Pos = state.Pos
            Speed = state.Speed
            Dir = state.Dir
            Accel = state.Accel
        }

    /// 1 コマの差分がもう変わらないならその値。台本が残っていれば ValueNone。
    /// Speed / Dir / Accel を外から書き換えたら使ってはいけない。Pos は関係ない。
    member _.ConstantDelta: Vec2 voption =
        if List.isEmpty state.Tops then
            let speed = float state.Speed
            let dir = float state.Dir

            ValueSome
                {
                    X = state.Accel.X + float32 (System.Math.Sin dir * speed)
                    Y = state.Accel.Y + float32 (-System.Math.Cos dir * speed)
                }
        else
            ValueNone

    /// あと何コマ何も起こさないか。0 は分からない。
    /// Motion を外から書き換えたら使ってはいけない。Finished のコマには使えない。
    member _.QuietFrames: int =
        if List.isEmpty state.Tops then
            0
        else
            let mutable m = System.Single.MaxValue
            let mutable all = true

            for (_, prog, _) in state.Tops do
                match Step.quietOf prog with
                | QWait l ->
                    if l < m then
                        m <- l
                | QNone
                | QBusy -> all <- false

            if all && m >= 1.0f then int m else 0

    /// (残りコマ, Speed の増分, Accel.X の増分, Accel.Y の増分)。ValueNone は分からない。
    /// 差分の式は返さない。呼ぶ側が Step の出口と同じ式を毎コマ計算しないとビットがずれる。
    member _.LinearPlan: struct (int * float32 * float32 * float32) voption =
        match state.Tops with
        | [ (_, prog, _) ] ->
            match Step.linearOf prog with
            | LStep(f, s, ax, ay) -> ValueSome(struct (f, s, ax, ay))
            | LNone -> ValueNone
        | _ -> ValueNone

    /// 線形の n コマを飛ばした姿。LinearPlan より大きい n を渡さないこと。
    member _.SkipLinear(n: int) : BulletRun =
        if n <= 0 then
            BulletRun(script, state)
        else
            match state.Tops with
            | [ (a, prog, fc) ] ->
                match Step.linearOf prog with
                | LStep(_, s, ax, ay) ->
                    let mutable speed = state.Speed
                    let mutable cx = state.Accel.X
                    let mutable cy = state.Accel.Y
                    // 1 回 で speed + step * n としない。 素の道は毎コマ 足すので、
                    // 同じ順で足さないと下の桁がずれる（minusOnes と同じ）
                    for _ in 1..n do
                        speed <- speed + s
                        cx <- cx + ax
                        cy <- cy + ay

                    BulletRun(
                        script,
                        { state with
                            Speed = speed
                            Accel = { X = cx; Y = cy }
                            Tops = [ a, Step.skipLinear n prog, fc ]
                        }
                    )
                | LNone -> BulletRun(script, state)
            | _ -> BulletRun(script, state)

    /// 静かな n コマを飛ばした姿。速い道は wait を減らさないので、入れる前にここを 1 度通す。
    /// QuietFrames より大きい n を渡すと、残りが負に回ってその弾だけ早く動き出す。
    member _.SkipQuiet(n: int) : BulletRun =
        if n <= 0 then
            BulletRun(script, state)
        else
            BulletRun(
                script,
                { state with
                    Tops = state.Tops |> List.map (fun (a, p, fc) -> a, Step.skipQuiet n p, fc)
                }
            )

    /// この弾の立場。根を作るときに決まり、撃たれた弾は親から継ぐ。あとから差し替えない。
    member _.Kind: BulletType = state.Kind

    /// 物理量を差し替える。台本と実行位置はそのまま持ち越す
    member _.WithMotion(m: Motion) =
        BulletRun(
            script,
            { state with
                Pos = m.Pos
                Speed = m.Speed
                Dir = m.Dir
                Accel = m.Accel
            }
        )

/// 1 コマの結果。参照型に戻すと弾数に比例してヒープを踏む。
[<Struct>]
type Frame =
    {
        /// 次のコマへ持ち越す実行状態
        Run: BulletRun
        /// 座標の差分。呼ぶ側が足す。絶対値ではない
        Delta: Vec2
        /// このコマで撃たれた弾。このコマでは回さない。
        /// 撃つのを断る口は無い。捨てるかどうかはフロントの仕事。
        Spawned: BulletRun list
        /// vanish された。フロントはこの弾を消す
        Vanished: bool
        /// 全 top が終わった。旧の RunResult.Processed
        Finished: bool
        /// 終わったうえで、撃たれた弾でありかつ自分も撃った。回収してよい。
        /// 旧の Used <- false
        Retired: bool
    }

/// 畳みの前後を同じ順で対にする。揃わない枝は降りない。同じ物は対にしない。
module private FoldOrigin =

    let private link (a: obj) (b: obj) =
        if not (obj.ReferenceEquals(a, b)) then
            NodeOrigin.pair b a

    let walk (read: Bulletml) (folded: Bulletml) =
        let rec cmd (a: Action) (b: Action) =
            link (box a) (box b)

            match a, b with
            | Action.Action(_, xs), Action.Action(_, ys) when xs.Length = ys.Length -> List.iter2 cmd xs ys
            | Action.Repeat(_, x), Action.Repeat(_, y) -> elm x y
            | Action.Fire(_, _, _, x), Action.Fire(_, _, _, y) -> bul x y
            | _ -> ()

        and elm (a: ActionElm) (b: ActionElm) =
            link (box a) (box b)

            match a, b with
            | ActionElm.Action(_, xs), ActionElm.Action(_, ys) when xs.Length = ys.Length -> List.iter2 cmd xs ys
            | _ -> ()

        and bul (a: BulletElm) (b: BulletElm) =
            match a, b with
            | BulletElm.Bullet(_, _, _, xs), BulletElm.Bullet(_, _, _, ys) when xs.Length = ys.Length ->
                List.iter2 elm xs ys
            | _ -> ()

        let top (a: BulletmlElm) (b: BulletmlElm) =
            // 根の要素も対にする。子だけ降りると鎖がここで止まる。
            link (box a) (box b)

            match a, b with
            | BulletmlElm.Bullet(_, _, _, xs), BulletmlElm.Bullet(_, _, _, ys) when xs.Length = ys.Length ->
                List.iter2 elm xs ys
            | BulletmlElm.Fire(_, _, _, x), BulletmlElm.Fire(_, _, _, y) -> bul x y
            | BulletmlElm.Action(_, xs), BulletmlElm.Action(_, ys) when xs.Length = ys.Length -> List.iter2 cmd xs ys
            | _ -> ()

        match read, folded with
        | Bulletml(_, xs), Bulletml(_, ys) when xs.Length = ys.Length -> List.iter2 top xs ys
        | _ -> ()

module Runner =

    /// 弾幕を読む。1 本につき 1 回。乱数とランクを省くと wait の term がグローバルから引かれる。
    /// Env は取らない。この段は aim を読まない。
    [<CompiledName "Load">]
    let load (rand: unit -> float32) (rank: float32) (bulletml: Bulletml) : BulletmlScript =
        // 木を組む段のための Env。aim は読まれないので 0 でよい
        let rootEnv: Env =
            {
                Rand = rand
                Rank = rank
                Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
                Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f }
            }

        let rec' = BulletmlRead.foldConstants bulletml
        // 読んだ木と畳んだ木の対。繋がないときは歩かない（NodeOrigin.enabled）
        if NodeOrigin.enabled then
            FoldOrigin.walk bulletml rec'

        let resolvers: Step.Resolvers =
            {
                Bullet = BulletmlOps.expandBulletRefOnce rec'
                Action = BulletmlOps.expandActionRefOnce rec'
            }
        // 根は bulletml しかない（Bulletml の腕が 1 つ）
        let shootingDirection =
            match rec' with
            | Bulletml.Bulletml(attrs, _) ->
                match attrs.bulletmlType with
                | Some x -> x
                | None -> ShootingDirection.BulletVertical
        // top* の並びは旧の toProcessable と同じ選び方（label が top で始まる action）
        let scripts =
            rec'
            |> BulletmlOps.getAction
            |> List.filter (function
                | ActionElm.Action(attrs, _) ->
                    match attrs.actionLabel with
                    | Some label -> (ActionLabel.text label).StartsWith "top"
                    | None -> false
                | _ -> false)
            |> List.map (BulletmlOps.convertRefActionElm rec')

        let rootState =
            {
                Pos = { X = 0.0f; Y = 0.0f }
                Speed = 0.0f
                Dir = 0.0f
                Accel = { X = 0.0f; Y = 0.0f }
                Kind = BulletType.Enemy
                IsBullet = false
                HasFired = false
                Tops =
                    scripts
                    |> List.map (fun s -> s, Step.rootProgressActionElm rootEnv s, FireContext.zero)
            }

        BulletmlScript(resolvers, shootingDirection, rootState)

    /// 撃たれた弾ではない根。Player は敵を、Enemy は自機を狙う。
    /// kind はここで 1 回だけ決まる。撃たれた弾は親から継ぐ。
    [<CompiledName "NewRoot">]
    let newRoot (kind: BulletType) (script: BulletmlScript) : BulletRun =
        BulletRun(
            script,
            { script.RootState with
                Kind = kind
                IsBullet = false
            }
        )

    /// 撃たれた弾として根から始める。newRoot との違いは Frame.Retired だけ。
    [<CompiledName "NewShot">]
    let newShot (kind: BulletType) (script: BulletmlScript) : BulletRun =
        BulletRun(
            script,
            { script.RootState with
                Kind = kind
                IsBullet = true
            }
        )

    /// 全 top が終わった弾を最初から走らせ直す。Core からは自動で呼ばない。
    /// 呼ぶと wait / changeDirection / changeSpeed の term を引き直すので、乱数の並びが変わる。
    [<CompiledName "Restart">]
    let restart (env: Env) (run: BulletRun) : BulletRun =
        let st = run.State

        BulletRun(
            run.Script,
            { st with
                Tops = st.Tops |> List.map (fun (s, _, fc) -> s, Step.resetChildActionElm env s, fc)
            }
        )

    /// 1 コマ進める
    let inline private toFrame (script: BulletmlScript) (r: StepResult) : Frame =
        let mutable vanished = false
        let mutable spawned = []

        for effect in r.Effects do
            match effect with
            | Vanished -> vanished <- true
            // 撃たれた弾は親の弾幕を引き継ぐ。引き継がないと bulletRef / actionRef を誰も解けない。
            | Spawn child -> spawned <- BulletRun(script, child) :: spawned

        {
            Run = BulletRun(script, r.State)
            Delta = r.Delta
            Spawned = List.rev spawned
            Vanished = vanished
            Finished = r.Finished
            Retired = r.Retired
        }

    [<CompiledName "Step">]
    let step (env: Env) (run: BulletRun) : Frame =
        let script = run.Script
        toFrame script (Step.step script.Resolvers env run.State)

    /// 物理量を入れ替えてから 1 コマ進める。WithMotion 経由だと弾の数だけ中間の BulletRun が出る。
    [<CompiledName "StepWith">]
    let stepWith (env: Env) (run: BulletRun) (m: Motion) : Frame =
        let script = run.Script
        let st = run.State

        let st =
            { st with
                Pos = m.Pos
                Speed = m.Speed
                Dir = m.Dir
                Accel = m.Accel
            }

        toFrame script (Step.step script.Resolvers env st)
