namespace FsBulletML2.Unity2D

open System.Collections.Generic
open FsBulletML2
open FsBulletML2.Front

/// このフロントが `FsBulletML2.Front` の口に答えるところ。
///
/// 式そのものはここに無い。 aim 4 本 は `Aiming.toward` の
/// `Space` 違いで、MonoGame の同じ関数と 1 ビット しか違わなかった
/// （凍結は `tests/FsBulletML2.Front.Tests/AimingFreeze.fs`）。
///
/// 弾 1 個 につき 1 個 作る。 狙う相手を覚えるのは弾ごとで、
/// 使い回すと別の弾が選んだ相手を引き継いでしまう。
[<Sealed>]
type Unity2DEnv() =

  let near =
    NearestEnemy<IDefaultBullet>((fun () -> Manager.enemies :> IReadOnlyList<IDefaultBullet>),
                                 (fun e -> e.X), (fun e -> e.Y))

  /// 覚えている相手を捨てる。次に聞かれたら選び直す
  member _.Forget () = near.Forget ()

  interface IFrontEnv with
    member _.Rand = BulletMLManager.GetRandom
    member _.Rank = BulletMLManager.GetRank ()
    member _.PlayerX = BulletMLManager.GetPlayerPosX ()
    member _.PlayerY = BulletMLManager.GetPlayerPosY ()

    /// 一度 選んだ相手を持ち回る。毎コマ 選び直すと軌跡が変わる
    member _.TryTargetFrom (x, y, ex, ey) = near.TryFrom (x, y, &ex, &ey)

    /// MonoGame と違って、覚えている相手をそのまま使う。
    /// このフロントは撃った側と同じ場所に弾を作るので、産まれる弾から見た
    /// 相手も撃った側と同じ。旧 GetSpawnEnemyAimDir の但し書きをそのまま写した
    member _.TrySpawnTargetFrom (x, y, ex, ey) = near.TryFrom (x, y, &ex, &ey)

/// このフロントの並び。2 つ とも 1 か所 だけに書く。
module Unity2DFront =

  /// Unity は Y が上向き
  let space = Space.YUp

  /// 撃った弾は撃った側と同じ場所に作る
  let origin = SpawnOrigin.AtShooter
