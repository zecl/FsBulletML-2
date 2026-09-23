namespace FsBulletML2.Unity2D
open FsBulletML2
open UnityEngine

/// フロントの弾。IBulletmlObject を継承しない。
/// MonoGame の IBullet と同じ形。片方だけ直すと、どちらが正か言えなくなる。
type IDefaultBullet =
  abstract Update : unit -> unit
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
  /// フロントの印。エンジンは見ない。
  /// 同梱サンプルが「使い終わったら Destroy」の判定に自分で立てて読んでいる。
  abstract BulletRoot : bool with get,set
  abstract BulletType : BulletType with get,set
  abstract ShootingDirection : ShootingDirection with get,set
  abstract Init : unit -> unit
  abstract Vanish : unit -> unit
  /// 弾幕を割り当てる。根から始めるときは run を None にする
  /// 根から始める。実行状態は Core に作らせる
  abstract SetScript : BulletmlScript option -> unit
  /// 撃たれた弾を、エンジンから受け取った実行状態で始める。
  /// 弾幕を渡す口が無い —— 実行状態が親のものを持っている
  abstract SetRun : BulletRun -> unit
  abstract Script : BulletmlScript option with get
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish
  abstract Finished : bool with get

/// 弾幕を読む段に渡すもの。MonoGame 側の BulletmlLoad と同じ理由でここに置く
/// —— 何を渡すかはフロントの決めごとなので Core には置かない。
[<AutoOpen>]
module BulletmlLoad =

  /// 1 個 だけ作って使い回す（MonoGame 側の同名と同じ理由）
  let loadRand : unit -> float32 = BulletMLManager.GetRandom

  let loadRank () : float32 = BulletMLManager.GetRank ()

  // aim を読まないコマの Env は FsBulletML2.Front の FrontEnv.noAim に移した
