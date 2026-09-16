namespace FsBulletML2

module Domain =

  /// [<Struct>] にしてある。 float32 が 2 つ だけで、位置・加速度・
  /// 1 コマの差分と、弾 1 個 × 1 コマ ごとに何度も作られるところに居る。
  /// 参照型のままだと弾の数に比例してヒープを踏む。
  [<Struct>]
  type Vec2 = { X : float32; Y : float32 }

  /// 撃つ側の位置から見た狙いの向き。2 本 とも、その弾自身の位置が基準。
  ///
  /// 弾が自機のものなら `ToEnemy`、敵のものなら `ToPlayer` が使われる。
  /// どちらを使うかはエンジンが決めるので、フロントは両方 入れて渡す。
  ///
  /// `SpawnAim` とは別の型にしてある。 値はどちらも float32 2 本 で、
  /// 混ぜても計算は通ってしまうが、基準にしている位置が違う。
  /// 取り違えると軌跡でしか見えないので、型で止める。
  [<Struct>]
  type Aim = { ToPlayer : float32; ToEnemy : float32 }

  /// これから産まれる弾の位置から見た狙いの向き。`Aim` とは基準が違う。
  ///
  /// `<bullet><direction type="aim">` は撃たれた弾を基準にするので、
  /// 撃つ側の `Aim` では答えが違う。産まれる弾がどこに出るかはフロントが
  /// 決めていて Core からは分からないので、フロントがこの欄に入れて渡す。
  [<Struct>]
  type SpawnAim = { ToPlayer : float32; ToEnemy : float32 }

  /// そのフレーム・その弾ぶんの環境。フレーム共通ではない。
  ///
  /// `Aim` は自機の位置だけでなくその弾自身の位置から決まるが、
  /// 1 コマの中では撃つ側の位置が動かないので、値で持てる。
  /// 最寄りの敵を探すのは呼ぶ側の仕事で、ここには結果だけ来る。
  ///
  /// [<Struct>] にしてある（1 走行で 17,820 回 組まれる）。
  /// 中に関数（`Rand`）が居ても struct にできる。
  [<Struct>]
  type Env =
    { Rand : unit -> float32
      Rank : float32
      Aim : Aim
      Spawn : SpawnAim }

  /// 実行位置。Script と同じ形の別の木。
  ///
  /// `started` は旧の first、`left` は term に当たる。
  /// term をここで評価しない —— 旧が命令の初回に評価しており、
  /// タイミングを変えると `$rand` を読む回数が変わって値が動く。
  type internal Progress =
    | PAction      of done_: bool * loop: Action list option * children: Progress list
    | PWait        of started: bool * left: float32
    | PRepeat      of num: int * done_: bool * child: Progress
    | PAccel       of started: bool * left: float32 * dx: float32 * dy: float32
    | PChangeDir   of started: bool * done_: bool * left: float32 * delta: float32
    | PChangeSpeed of started: bool * done_: bool * left: float32 * delta: float32
    | PFire        of done_: bool
    | PVanish      of done_: bool
    | PNoop

  module Progress =

    /// Script から実行位置を組む。初期化の入口はこの 1 本だけ。
    ///
    /// 命令の 10 通りを漏れなく書く（`| _ -> PNoop` で受けない）。
    /// 受けていたころ、そこに「本当に PNoop でよいもの」と
    /// 「そもそも命令の位置に来ないもの」が混ざっていた。
    let rec internal initial (script: Action) : Progress =
      match script with
      | Action.Action (_, children) ->
          PAction (false, None, children |> List.map initial)
      | Action.Repeat (_, body) ->
          PRepeat (0, false, initialActionElm body)
      | Action.Wait _ -> PWait (false, 0.0f)
      | Action.Vanish -> PVanish false
      | Action.Fire _ -> PFire false
      | Action.Accel _ -> PAccel (false, 0.0f, 0.0f, 0.0f)
      | Action.ChangeDirection _ -> PChangeDir (false, false, 0.0f, 0.0f)
      | Action.ChangeSpeed _ -> PChangeSpeed (false, false, 0.0f, 0.0f)
      // 展開していない ActionRef / FireRef はここ。参照自身は状態を持たない。
      // 輪を解いた並びは親の PAction.loop が持つ（Step.action 参照）
      | Action.ActionRef _ -> PNoop
      | Action.FireRef _ -> PNoop

    /// repeat / bullet の子（action か actionRef）から実行位置を組む
    and internal initialActionElm (script: ActionElm) : Progress =
      match script with
      | ActionElm.Action (_, children) ->
          PAction (false, None, children |> List.map initial)
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

  /// この top が、これから何コマ 一定の割合で変わるか（v4.9.3）
  ///
  /// `accel` / `changeSpeed` が進行中 の弾は止まっていないので `Quiet` では
  /// 拾えないが、毎コマ 同じ量 が足されるので先が読める。
  ///
  /// `changeDirection` は入らない。 `Dir` が動くと `sin dir` になり、
  /// 一定の割合では変わらない。
  type internal Linear =
    /// 分からない（乗せない）
    | LNone
    /// あと `Frames` コマ、毎コマ `Speed += SpeedStep` と
    /// `Accel += (AccelX, AccelY)` が起きる
    | LStep of frames: int * speedStep: float32 * accelX: float32 * accelY: float32


  /// 直前の fire の値。sequence の累積がここに乗る
  type FireContext =
    { SrcDir : float32
      SrcSpeed : float32
      SpeedInit : bool }

  module FireContext =
    let zero = { SrcDir = 0.0f; SrcSpeed = 0.0f; SpeedInit = false }

  type internal BulletState =
    { Pos : Vec2
      Speed : float32
      Dir : float32
      Accel : Vec2
      Kind : BulletType
      /// 撃たれた弾か。根の敵は false
      IsBullet : bool
      /// 自分も子を撃ったか。旧の BulletRoot
      HasFired : bool
      /// top* は 1 本ずつ独立に回る。スクリプト・実行位置・fire の累積を
      /// 組で持つので、添字の対応が構造で保証される。
      ///
      /// 台本は top* の action。展開を止めた actionRef が残ることがあるので
      /// `ActionElm`（action か actionRef）で持つ。
      Tops : (ActionElm * Progress * FireContext) list }

  type internal Effect =
    | Spawn of BulletState
    | Vanished

  /// 1 コマの結果。`Delta` が差分であることを名前で言うのが要点で、
  /// 呼ぶ側が足す規約が型に出ていなかったのが元の作りだった
  type internal StepResult =
    { State : BulletState
      Effects : Effect list
      Delta : Vec2
      /// 全 top が終わった。旧の RunResult.Processed
      Finished : bool
      /// タスク完了で回収される。旧の Used <- false
      Retired : bool }
