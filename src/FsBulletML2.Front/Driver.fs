namespace FsBulletML2.Front

open FsBulletML2
open FsBulletML2.Domain

/// 1 弾 1 コマ の回し方。
///
/// 位置と係数はフロントの持ち物（MonoGame は 1 倍、Unity2D は 1/100 で
/// Y が反転する）なので、差分を足すのはここではやらない。ここが持つのは
/// `Env` をどの位置で組むかの順だけ。
module Driver =

  /// 1 コマ 進める。差分を足す前の位置を渡すこと
  [<CompiledName "Step">]
  let step (front: IFrontEnv) (space: Space) (origin: SpawnOrigin)
           (run: BulletRun) (motion: Motion) : Frame =
    let env = FrontEnv.forRun front space origin run motion.Pos.X motion.Pos.Y
    Runner.stepWith env run motion

  /// 全 top が終わった弾を走らせ直す。
  ///
  /// 位置を取らない。aim が結果に出ないから。
  /// ここが既定を作ると、その決めごとを黙って奪うことになる
  [<CompiledName "Restart">]
  let restart (front: IFrontEnv) (run: BulletRun) : BulletRun =
    Runner.restart (FrontEnv.noAim front) run
