namespace FsBulletML2

module Domain =

    /// 位置・加速度・差分。参照型に戻すと弾の数に比例してヒープを踏む。
    [<Struct>]
    type Vec2 = { X: float32; Y: float32 }

    /// 撃つ側の位置から見た狙いの向き。フロントは ToPlayer と ToEnemy の両方を入れる。
    /// SpawnAim と混ぜると計算は通るが、基準の位置が違い軌跡でしか見えない。
    [<Struct>]
    type Aim = { ToPlayer: float32; ToEnemy: float32 }

    /// 産まれる弾の位置から見た狙いの向き。direction type="aim" に撃つ側の Aim を使うと答えが違う。
    [<Struct>]
    type SpawnAim = { ToPlayer: float32; ToEnemy: float32 }

    /// そのフレーム・その弾の環境。フレーム共通ではない。
    /// 参照型に戻すと組む回数だけヒープを踏む。関数を持っていても struct のままでよい。
    [<Struct>]
    type Env =
        {
            Rand: unit -> float32
            Rank: float32
            Aim: Aim
            Spawn: SpawnAim
        }

    /// 実行位置。term をここで評価しない。タイミングを変えると $rand の回数が変わり値が動く。
    type internal Progress =
        | PAction of done_: bool * loop: Action list option * children: Progress list
        | PWait of started: bool * left: float32
        | PRepeat of num: int * done_: bool * child: Progress
        | PAccel of started: bool * left: float32 * dx: float32 * dy: float32
        | PChangeDir of started: bool * done_: bool * left: float32 * delta: float32
        | PChangeSpeed of started: bool * done_: bool * left: float32 * delta: float32
        | PFire of done_: bool
        | PVanish of done_: bool
        | PNoop

    module Progress =

        /// Script から実行位置を組む。初期化の入口はこの 1 本だけ。
        /// `| _ -> PNoop` で受けると、来ない腕と PNoop でよい腕が混ざる。
        let rec internal initial (script: Action) : Progress =
            match script with
            | Action.Action(_, children) -> PAction(false, None, children |> List.map initial)
            | Action.Repeat(_, body) -> PRepeat(0, false, initialActionElm body)
            | Action.Wait _ -> PWait(false, 0.0f)
            | Action.Vanish -> PVanish false
            | Action.Fire _ -> PFire false
            | Action.Accel _ -> PAccel(false, 0.0f, 0.0f, 0.0f)
            | Action.ChangeDirection _ -> PChangeDir(false, false, 0.0f, 0.0f)
            | Action.ChangeSpeed _ -> PChangeSpeed(false, false, 0.0f, 0.0f)
            // 展開していない ActionRef / FireRef はここ。参照自身は状態を持たない。
            // 輪を解いた並びは親の PAction.loop が持つ（Step.action 参照）
            | Action.ActionRef _ -> PNoop
            | Action.FireRef _ -> PNoop

        /// repeat / bullet の子（action か actionRef）から実行位置を組む
        and internal initialActionElm (script: ActionElm) : Progress =
            match script with
            | ActionElm.Action(_, children) -> PAction(false, None, children |> List.map initial)
            | ActionElm.ActionRef _ -> PNoop

    /// この top が、これから何コマ 何も起こさないか（v4.9.2）
    type internal Quiet =
        /// 進行中のものが無い（この枝では何も分からない）
        | QNone
        /// `wait` で止まっていて、残り L コマ
        | QWait of float32
        /// 進行中の変化が在る。 止まって見えても `Speed` / `Dir` / `Accel` が
        /// 毎コマ 動くので、静かではない
        | QBusy

    /// 一定の割合で変わる先読み。changeDirection は入れない。Dir が動くと sin になり一定でなくなる。
    type internal Linear =
        /// 分からない（乗せない）
        | LNone
        /// あと `Frames` コマ、毎コマ `Speed += SpeedStep` と
        /// `Accel += (AccelX, AccelY)` が起きる
        | LStep of frames: int * speedStep: float32 * accelX: float32 * accelY: float32


    /// 直前の fire の値。sequence の累積がここに乗る
    type FireContext =
        {
            SrcDir: float32
            SrcSpeed: float32
            SpeedInit: bool
        }

    module FireContext =
        let zero =
            {
                SrcDir = 0.0f
                SrcSpeed = 0.0f
                SpeedInit = false
            }

    type internal BulletState =
        {
            Pos: Vec2
            Speed: float32
            Dir: float32
            Accel: Vec2
            Kind: BulletType
            /// 撃たれた弾か。根の敵は false
            IsBullet: bool
            /// 自分も子を撃ったか。旧の BulletRoot
            HasFired: bool
            /// top* の台本・実行位置・fire の累積。ばらすと添字がずれる。actionRef が残るので ActionElm。
            Tops: (ActionElm * Progress * FireContext) list
        }

    type internal Effect =
        | Spawn of BulletState
        | Vanished

    /// 1 コマの結果。Delta は絶対座標ではない。呼ぶ側が足す。
    type internal StepResult =
        {
            State: BulletState
            Effects: Effect list
            Delta: Vec2
            /// 全 top が終わった。旧の RunResult.Processed
            Finished: bool
            /// タスク完了で回収される。旧の Used <- false
            Retired: bool
        }
