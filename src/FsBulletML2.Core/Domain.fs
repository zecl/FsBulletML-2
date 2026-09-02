namespace FsBulletML2

module Domain =

  type Vec2 = { X : float32; Y : float32 }

  /// そのフレーム・その弾ぶんの環境。フレーム共通ではない。
  ///
  /// AimDir は自機の位置だけでなくその弾自身の位置から決まるが、
  /// 1 コマの中では撃つ側の位置が動かないので、値で持てる。
  /// 最寄りの敵を探すのは呼ぶ側の仕事で、ここには結果だけ来る。
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
      /// Core からは分からないので、IBulletmlObject.GetSpawnAimDir に訊く。
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
    | PAction      of done_: bool * loop: RecCommand list option * children: Progress list
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
    /// 「Bulletml / Bullet / BulletRef / NotCommand」（そもそも命令の位置に
    /// 来ないもの）が混ざっていた。型が分かれたので、前者だけが残る
    let rec internal initial (script: RecCommand) : Progress =
      match script with
      | RecCommand.Action (_, children) ->
          PAction (false, None, children |> List.map initial)
      | RecCommand.Repeat (_, body) ->
          PRepeat (0, false, initialActionElm body)
      | RecCommand.Wait _ -> PWait (false, 0.0f)
      | RecCommand.Vanish -> PVanish false
      | RecCommand.Fire _ -> PFire false
      | RecCommand.Accel _ -> PAccel (false, 0.0f, 0.0f, 0.0f)
      | RecCommand.ChangeDirection _ -> PChangeDir (false, false, 0.0f, 0.0f)
      | RecCommand.ChangeSpeed _ -> PChangeSpeed (false, false, 0.0f, 0.0f)
      // 展開していない ActionRef / FireRef はここ。参照自身は状態を持たない。
      // 輪を解いた並びは、その actionRef 自身ではなく親の PAction.loop が持つ
      // （Step.action 参照）
      | RecCommand.ActionRef _ -> PNoop
      | RecCommand.FireRef _ -> PNoop

    /// repeat / bullet の子（action か actionRef）から実行位置を組む
    and internal initialActionElm (script: RecActionElm) : Progress =
      match script with
      | RecActionElm.Action (_, children) ->
          PAction (false, None, children |> List.map initial)
      | RecActionElm.ActionRef _ -> PNoop

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
      /// RecActionElm（action か actionRef）で持つ
      Tops : (RecActionElm * Progress * FireContext) list }

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
