namespace FsBulletML2.MonoGame

open System
open Microsoft.Xna.Framework
open Microsoft.FSharp.Core.Operators.Unchecked
open FsBulletML2
open FsBulletML2.Domain

/// このフロントが `Env` を組むところ。**4 本 の aim を入れる場所はここだけ。**
///
/// 以前は `BaseBullet` の中の private な `let` と `member private` に散って
/// いた。散っていると、`AimDir` に `SpawnAimDir` を入れるような取り違えを
/// 門で当てられない（型はどれも float32 なので通ってしまう）。
///
/// **式はこのフロント固有。** Unity2D の同じ関数とは 2 つ 違う。
///
///     Y の符号     こちらは -(py - y)。Unity2D は反転しない（座標系が逆）
///     Spawn の元    こちらは原点（撃った弾を原点に作る）。
///                  Unity2D は撃った側と同じ場所に作るので AimDir と同値
///
/// **だから Core へは畳めない。** 畳むと片方の座標系を強制することになる。
///
/// **公開にしてある。** internal にすると門（FsBulletML2.MonoGame.Tests）が
/// InternalsVisibleTo 越しに見ることになり、**「公開だけで書けているか」を
/// 測るというフロント側の門の役目が消える**（Core.Tests がまさにその状態で、
/// TraceApi.fs に「強制が掛かるのはフロントを移したとき」と書いてある）。
/// このフロントを使うゲームが自前で Env を組みたいときにも要る。
module FrontEnv =

  /// 自機を狙う向き。旧 GetAimDir の式そのまま。
  /// 旧はエンジンが IBulletmlObject.GetAimDir を呼び返していたが、
  /// いまは Env を組むためにフロントが自分で呼ぶ
  let aimAtPlayer (x: float32) (y: float32) =
    float32 (Math.Atan2(float (BulletMLManager.GetPlayerPosX() - x),
                        float -(BulletMLManager.GetPlayerPosY() - y)))

  /// 産まれる弾の位置から見た向き。旧 GetSpawnAimDir。
  ///
  /// 産まれた弾がどこに出るかは BaseBullet.Spawn が決めていて、位置を
  /// 入れずに作るので原点。同じ式に原点を入れる。**片方だけ直すと軌跡が割れる**
  let spawnAimAtPlayer () = aimAtPlayer 0.0f 0.0f

  /// 旧 GetSpawnEnemyAimDir。産まれたばかりの弾は TargetEnemy を持たないので、
  /// 原点にいちばん近い敵をその場で選ぶ。
  ///
  /// **弾が覚えている相手を使う EnemyAimDirAt とはここが違う** ——
  /// あちらは一度 選んだ相手を持ち回る（毎コマ 選び直すと軌跡が変わる）
  let spawnAimAtEnemy () =
    if ((Manager.enemies) :> seq<_>) |> Seq.length <= 0 then 0.0f
    else
      let mutable md = Single.MaxValue
      let mutable target = defaultof<IBullet>
      for enemy in Manager.enemies do
        let d = Vector2.Distance (Vector2(0.0f, 0.0f), Vector2(enemy.X, enemy.Y))
        if md > d then
          target <- enemy
          md <- d
      float32 (Math.Atan2(float (target.X - 0.0f), -1.0 * float (target.Y - 0.0f)))

  /// このコマの Env を、いまの位置から組む。旧 BulletRunner.envOfGlobal の写し。
  ///
  /// **組む位置が変わると aim がずれる**ので、呼ぶ側は step の直前
  /// （差分を足す前）に組むこと。
  ///
  /// `enemyAimAt` だけ引数で受けるのは、**弾が覚えている相手に依る**ため
  /// （BaseBullet.EnemyAimDirAt が TargetEnemy を持ち回る）。
  /// 残り 3 本 はグローバルと位置だけで決まるので、ここに閉じている。
  let at (enemyAimAt: float32 -> float32 -> float32) (x: float32) (y: float32) : Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = aimAtPlayer x y
      EnemyAimDir = enemyAimAt x y
      SpawnAimDir = spawnAimAtPlayer ()
      SpawnEnemyAimDir = spawnAimAtEnemy () }

  /// aim を読まないと分かっているコマの Env。aim 4 本 を 0 に。
  ///
  /// 使ってよい条件は BulletRun.HasNoScript の但し書きにある。
  /// **`at` と欄が 1 つ でもずれたら、片方だけ直したということ**
  let noAim () : Env =
    { Rand = BulletMLManager.GetRandom
      Rank = BulletMLManager.GetRank ()
      AimDir = 0.0f
      EnemyAimDir = 0.0f
      SpawnAimDir = 0.0f
      SpawnEnemyAimDir = 0.0f }
