namespace FsBulletML2.Front

open FsBulletML2
open FsBulletML2.Domain

/// このコマの `Env` を組む。**aim を入れる場所はここだけ。**
///
/// 以前は同梱の 4 つ のフロントがそれぞれ同じ形を写していた。写しは
/// 「片方だけ直す」ができるので、`Aim` と `Spawn` の食い違いが門に出ない。
module FrontEnv =

  /// aim を読まないと分かっているコマの `Env`。aim 4 本 を 0 に。
  ///
  /// 使ってよい条件は `BulletRun.HasNoScript` の但し書き。
  /// **`at` と欄が 1 つ でもずれたら、片方だけ直したということ**
  [<CompiledName "NoAim">]
  let noAim (world: IWorld) : Env =
    { Rand = world.Rand
      Rank = world.Rank
      Aim = { ToPlayer = 0.0f; ToEnemy = 0.0f }
      Spawn = { ToPlayer = 0.0f; ToEnemy = 0.0f } }

  /// いまの位置から組む。
  ///
  /// **組む位置が変わると aim がずれる**ので、呼ぶ側は step の直前
  /// （差分を足す前）に組むこと。走らせ直しの前は、差分を足した**あと**に組む
  /// （旧 `BaseBullet` が apply のあとで `envOfGlobal` を呼ぶのと同じ順）。
  [<CompiledName "At">]
  let at (world: IWorld) (space: Space) (origin: SpawnOrigin) (x: float32) (y: float32) : Env =
    let struct (sx, sy) = Aiming.spawnPoint origin x y
    let mutable tx = 0.0f
    let mutable ty = 0.0f
    let toEnemy =
      if world.TryTargetFrom(x, y, &tx, &ty) then Aiming.toward space x y tx ty else 0.0f
    let mutable stx = 0.0f
    let mutable sty = 0.0f
    let spawnToEnemy =
      if world.TrySpawnTargetFrom(sx, sy, &stx, &sty) then Aiming.toward space sx sy stx sty else 0.0f
    { Rand = world.Rand
      Rank = world.Rank
      Aim = { ToPlayer = Aiming.toward space x y world.PlayerX world.PlayerY
              ToEnemy = toEnemy }
      Spawn = { ToPlayer = Aiming.toward space sx sy world.PlayerX world.PlayerY
                ToEnemy = spawnToEnemy } }

  /// 台本が無い弾は aim を読まないので、そのときは `noAim`。
  ///
  /// **この枝を既定にしてある。** 同梱のフロントは全部 これを通していたが、
  /// 通し忘れても答えは同じで速さだけ落ちる（5way で 24%）ので、
  /// 忘れたことが門に出ない
  [<CompiledName "ForRun">]
  let forRun (world: IWorld) (space: Space) (origin: SpawnOrigin)
             (run: BulletRun) (x: float32) (y: float32) : Env =
    if run.HasNoScript then noAim world else at world space origin x y

/// 1 弾 1 コマ の回し方。
///
/// **位置と係数はフロントの持ち物**（MonoGame は 1 倍、Unity2D は 1/100 で
/// Y が反転する）なので、差分を足すのはここではやらない。ここが持つのは
/// **`Env` をどの位置で組むか**の順だけ。
module Driver =

  /// 1 コマ 進める。**差分を足す前の位置**を渡すこと
  [<CompiledName "Step">]
  let step (world: IWorld) (space: Space) (origin: SpawnOrigin)
           (run: BulletRun) (motion: Motion) : Frame =
    let env = FrontEnv.forRun world space origin run motion.Pos.X motion.Pos.Y
    Runner.stepWith env run motion

  /// 全 top が終わった弾を走らせ直す。**差分を足したあとの位置**を渡すこと。
  ///
  /// **走らせ直すかどうかはフロントの決めごと。** 同梱の 2 つ は
  /// `Finished` のコマで呼んでいるが、`restart` は wait / changeDirection /
  /// changeSpeed の term を引き直すので、呼ぶか呼ばないかで乱数の並びが変わる。
  /// ここが既定を作ると、その決めごとを黙って奪うことになる
  [<CompiledName "Restart">]
  let restart (world: IWorld) (space: Space) (origin: SpawnOrigin)
              (run: BulletRun) (x: float32) (y: float32) : BulletRun =
    Runner.restart (FrontEnv.forRun world space origin run x y) run
