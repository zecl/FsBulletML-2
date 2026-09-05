namespace FsBulletML2.MonoGame
open FsBulletML2
open Microsoft.Xna.Framework

/// フロントの弾。**IBulletmlObject を継承しない。**
///
/// 旧はここが `inherit IBulletmlObject` していて、エンジンがフロントを
/// 呼び返すための 19 メンバ（GetNewBullet / Vanish / Init / Task / Used /
/// BulletRoot / 物理量の setter …）を実装させられていた。呼び返しが在ると
/// 「いつ呼ばれるか」を知らないと実装が書けない。
///
/// 新 API は値の受け渡しだけなので、ここに残るのは**フロント自身が要るもの**
/// だけになった。位置と見た目（Manager と描画が読む）、弾幕を割り当てる口、
/// そして 1 コマ進める Update。
///
/// BulletRoot が消えたのは、それがエンジン側の概念（自分も撃ったか）で
/// Body.HasFired が持つようになったため。Task が消えたのは、
/// BulletmlScript（弾幕）と BulletRun（実行状態）に割れたため。
type IBullet =
  abstract Update : unit -> unit
  abstract TargetEnemy : IBullet with get,set
  abstract Pos : Vector2 with get,set
  abstract X : float32 with get,set
  abstract Y : float32 with get,set
  abstract Radius : float32 with get,set
  abstract Speed : float32 with get,set
  abstract Dir : float32 with get,set
  abstract AccelerationX : float32 with get,set
  abstract AccelerationY : float32 with get,set
  abstract Used : bool with get,set
  abstract IsBullet : bool with get,set
  abstract BulletType : BulletType with get,set
  abstract ShootingDirection : ShootingDirection with get,set
  abstract Init : unit -> unit
  abstract Vanish : unit -> unit
  /// 弾幕を割り当てる。根から始めるときは run を None にする
  /// （Runner.newRoot が使われる）。撃たれた弾には、親が Frame.Spawned で
  /// 受け取った BulletRun をそのまま渡す
  abstract SetScript : BulletmlScript option * BulletRun option -> unit
  /// いま走らせている弾幕。撃たれた弾へ引き継ぐために読む
  abstract Script : BulletmlScript option with get
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish。
  /// サンプルの敵が「弾幕が終わったから撃ち直す」を判定するのに読む
  abstract Finished : bool with get

/// 弾幕を読む段の Env。
///
/// Runner.load が Env を要るのは、木を組む段が wait の term をその場で
/// 引くため。**何を渡すかはフロントの決めごと**なので Core には置かない
/// —— 置くとまたグローバルから引く形に戻る。
///
/// この同梱フロントは旧と同じく BulletMLManager から取る。aim は
/// この段では読まれない（撃つ弾ごとの位置がまだ無い）ので 0 でよい。
[<AutoOpen>]
module BulletmlLoad =

  let loadEnv () : FsBulletML2.Domain.Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimVec = { X = 0.0f; Y = 0.0f }
      EnemyAimVec = { X = 0.0f; Y = 0.0f }
      SpawnAimVec = { X = 0.0f; Y = 0.0f }
      SpawnEnemyAimVec = { X = 0.0f; Y = 0.0f } }

  /// aim を読まないと分かっているコマの Env。中身は loadEnv と同じだが、
  /// **意味が違うので名前を分けてある** —— あちらは「木を組む段はまだ
  /// 弾の位置が無いので 0」、こちらは「この弾はこのコマ aim を読まないので
  /// 計算しない」。片方の理由が消えたときに、もう片方まで一緒に消さないため。
  ///
  /// 使ってよい条件は BulletRun.HasNoScript の但し書きにある。
  /// 旧 BulletRunner.envWithoutAim と同じ狙いで、段階 4 で Env を組む責任が
  /// フロントへ移ったぶん、判断もフロントに来た
  let noAimEnv () : FsBulletML2.Domain.Env = loadEnv ()

  // FrontEnv.noAim も同じ形。**3 つ に増えたので、門で 3 つ が一致することを
  // 見ている**（MonoGameEnvGate）。名前を分けたまま値のずれだけ止める
