namespace FsBulletML2.Unity2D
open FsBulletML2
open UnityEngine

/// フロントの弾。**IBulletmlObject を継承しない。**
///
/// 旧はここが `inherit IBulletmlObject` していて、エンジンがフロントを
/// 呼び返すための 19 メンバ を実装させられていた。新 API は値の受け渡し
/// だけなので、ここに残るのはフロント自身が要るものだけ。
///
/// MonoGame 側の IBullet と同じ形にしてある。**片方だけ直すと、同じ規約を
/// 2 通り に書いた状態になって、あとから読む人がどちらが正かを判断できない。**
type IDefaultBullet =
  abstract Update : unit -> unit
  abstract TargetEnemy : IDefaultBullet with get,set
  abstract Pos : Vector3 with get,set
  abstract X : float32 with get,set
  abstract Y : float32 with get,set
  abstract Radius : float32 with get,set
  abstract Root : bool with get,set
  abstract Speed : float32 with get,set
  abstract Dir : float32 with get,set
  abstract AccelerationX : float32 with get,set
  abstract AccelerationY : float32 with get,set
  abstract Used : bool with get,set
  abstract IsBullet : bool with get,set
  /// **フロントの印。エンジンは見ない。**
  ///
  /// 旧はエンジンが GetNewBullet の中で立てていて、Retired の判定に使って
  /// いた。いまエンジン側の同じ概念は Body.HasFired が持つ（BulletRun の中）。
  /// ここに残っているのは、同梱サンプルが「撃たれた弾を、使い終わったら
  /// Destroy する」判定に自分で立てて自分で読んでいるため
  abstract BulletRoot : bool with get,set
  abstract BulletType : BulletType with get,set
  abstract ShootingDirection : ShootingDirection with get,set
  abstract Init : unit -> unit
  abstract Vanish : unit -> unit
  /// 弾幕を割り当てる。根から始めるときは run を None にする
  abstract SetScript : BulletmlScript option * BulletRun option -> unit
  abstract Script : BulletmlScript option with get
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish
  abstract Finished : bool with get

/// 弾幕を読む段の Env。MonoGame 側の BulletmlLoad と同じ理由でここに置く
/// —— **何を渡すかはフロントの決めごと**なので Core には置かない。
[<AutoOpen>]
module BulletmlLoad =

  let loadEnv () : FsBulletML2.Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = 0.0f
      EnemyAimDir = 0.0f
      SpawnAimDir = 0.0f
      SpawnEnemyAimDir = 0.0f }
