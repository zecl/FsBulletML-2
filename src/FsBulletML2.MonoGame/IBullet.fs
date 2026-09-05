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
  /// 根から始める。実行状態は Core に作らせる
  abstract SetScript : BulletmlScript option -> unit
  /// 撃たれた弾を、エンジンから受け取った実行状態で始める。
  /// **弾幕を渡す口が無い** —— 実行状態が親のものを持っている
  abstract SetRun : BulletRun -> unit
  /// いま走らせている弾幕。撃たれた弾へ引き継ぐために読む
  abstract Script : BulletmlScript option with get
  /// 直前のコマで全 top が終わったか。旧 BulletmlTask.Finish。
  /// サンプルの敵が「弾幕が終わったから撃ち直す」を判定するのに読む
  abstract Finished : bool with get

/// 弾幕を読む段に渡すもの。**Env ではない。**
///
/// `Runner.load` が要るのは乱数とランクの 2 つ だけ（木を組む段が wait の
/// term をその場で引く）。**何を渡すかはフロントの決めごと**なので Core には
/// 置かない —— 置くとまたグローバルから引く形に戻る。
///
/// この同梱フロントは旧と同じく BulletMLManager から取る。
[<AutoOpen>]
module BulletmlLoad =

  /// **1 個 だけ作って使い回す。** C# から渡すときに毎回 FuncConvert すると
  /// 弾数 × コマ数 だけヒープを踏む（同じ罠を Unity C# サンプルが踏んで
  /// いて、あちらは自前で static readonly に置いている）
  let loadRand : unit -> float32 = BulletMLManager.GetRandom

  let loadRank () : float32 = BulletMLManager.GetRank ()

  // aim を読まないコマの Env は FsBulletML2.Front の FrontEnv.noAim に移した。
  // **写しを 2 つ 置いて突き合わせる門を建てていたが、写しが 1 つ になった** ——
  // 検証を別の層へ移したのではなく、守る対象が消えた
