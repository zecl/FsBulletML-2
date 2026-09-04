namespace FsBulletML2

module Domain =

  /// **[<Struct>] にしてある。** float32 が 2 つ だけで、位置・加速度・
  /// 1 コマの差分と、**弾 1 個 × 1 コマ ごとに何度も作られる**ところに居る。
  /// 参照型のままだと弾の数に比例してヒープを踏む。
  ///
  /// Body を struct にした時点で 5way の確保が旧 API 比 +33% → +8% まで
  /// 落ちたが、残りはこの中の Pos / Accel が踏んでいた分。
  [<Struct>]
  type Vec2 = { X : float32; Y : float32 }

  /// そのフレーム・その弾ぶんの環境。フレーム共通ではない。
  ///
  /// AimDir は自機の位置だけでなくその弾自身の位置から決まるが、
  /// 1 コマの中では撃つ側の位置が動かないので、値で持てる。
  /// 最寄りの敵を探すのは呼ぶ側の仕事で、ここには結果だけ来る。
  ///
  /// **[<Struct>] にしてある。** 1 走行（5way / 60 コマ）でフロントがこれを
  /// 17,820 回 組む —— step の前と走らせ直しの前で、生きている弾も死んだ弾も
  /// 区別せず。参照型だとその回数だけヒープを踏む。
  ///
  /// **中に関数（Rand）が居ても struct にできる。** 関数参照そのものは
  /// ヒープに在るままだが、それを持つ**包み**を値にできる。
  /// 「関数を持つから参照型でなければならない」ではない。
  ///
  /// BulletRun.HasNoScript で aim の計算は止めたが、あれは Atan2 を
  /// 止めただけで箱は毎コマ 作っていた（時間は 24% 減ったのに確保が
  /// 1 バイト も動かなかったのがその証拠）。箱を消すのはこちら。
  [<Struct>]
  type Env =
    { Rand : unit -> float32
      Rank : float32
      AimDir : float32
      EnemyAimDir : float32
      /// これから産まれる弾の位置から見た向き。AimDir とは基準が違う。
      ///
      /// <bullet><direction type="aim"> は撃たれた弾を基準にするので、
      /// 撃つ側の AimDir では答えが違う。産まれる弾がどこに出るかは
      /// フロントエンドが決めていて（MonoGame は原点、Unity2D は撃った側）
      /// Core からは分からないので、**フロントがこの欄に入れて渡す**
      /// （旧はエンジンが `IBulletmlObject.GetSpawnAimDir` を呼び返していた）。
      SpawnAimDir : float32
      SpawnEnemyAimDir : float32 }

  /// 実行位置。Script と同じ形の別の木。
  ///
  /// started は現行の first、left は term に当たる。term をここで評価しないのは、
  /// 現行が命令の初回に評価しており、タイミングを変えると $rand を読む回数が
  /// 変わって値が動くため。
  /// PChangeDir / PChangeSpeed だけ done_ を明示で持つ。wait と accel は
  /// 「term が尽きたら二度と正にならない」ので left の符号だけで終わりが
  /// 判定できるが、changeDirection / changeSpeed は終わるフレームで
  /// term を getValue initTerm へ**戻す**（repeat の次周のため）。
  /// 戻すと left がまた正になるので、left の符号だけでは「戻した直後」と
  /// 「まだ途中」を区別できない。現行の pd.finish / ps.finish に当たる
  /// 明示のフラグが要る
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
    /// 命令の 10 通りを漏れなく書く。以前は `| _ -> PNoop` で受けていて、
    /// そこに「ActionRef と FireRef」（本当に PNoop でよいもの）と
    /// 「Bulletml / Bullet / BulletRef と、当時あった NotCommand」（そもそも
    /// 命令の位置に来ないもの）が混ざっていた。型が分かれたので、前者だけが残る
    /// —— NotCommand はその後、公開の Bulletml からも型ごと消えた
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
      // 輪を解いた並びは、その actionRef 自身ではなく親の PAction.loop が持つ
      // （Step.action 参照）
      | Action.ActionRef _ -> PNoop
      | Action.FireRef _ -> PNoop

    /// repeat / bullet の子（action か actionRef）から実行位置を組む
    and internal initialActionElm (script: ActionElm) : Progress =
      match script with
      | ActionElm.Action (_, children) ->
          PAction (false, None, children |> List.map initial)
      | ActionElm.ActionRef _ -> PNoop

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
      /// 自分も子を撃ったか。現行の BulletRoot
      HasFired : bool
      /// top* は 1 本ずつ独立に回る。スクリプト・実行位置・fire の累積を
      /// 組で持つので、添字の対応が構造で保証される。
      ///
      /// スクリプトを弾が持ち歩くのは、撃たれた弾が自分の action を
      /// 持てるようにするため。これで step の引数が状態 1 つで済む
      ///
      /// 台本は top* の action。展開を止めた actionRef が残ることがあるので
      /// ActionElm（action か actionRef）で持つ
      Tops : (ActionElm * Progress * FireContext) list }

  type internal Effect =
    | Spawn of BulletState
    | Vanished

  /// 1 コマの結果。Delta が差分であることを名前で言うのが要点で、
  /// 呼ぶ側が足す規約が型に出ていなかったのが元の作りだった
  type internal StepResult =
    { State : BulletState
      Effects : Effect list
      Delta : Vec2
      /// 全 top が終わった。現行の RunResult.Processed
      Finished : bool
      /// タスク完了で回収される。現行の Used <- false
      Retired : bool }
