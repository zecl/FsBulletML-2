namespace FsBulletML2.MonoGame

open System.Collections.Generic
open FsBulletML2
open FsBulletML2.Front

/// このフロントが `FsBulletML2.Front` の口に答えるところ。
///
/// **式そのものはここに無い。** aim 4 本 は `Aiming.toward` の
/// `Space` 違いで、Unity2D の同じ関数と 1 ビット しか違わなかった
/// （格子 11 点 × 4 変数 = 14,641 組 でビット一致。凍結は
/// `tests/FsBulletML2.Front.Tests/AimingFreeze.fs`）。
/// 以前ここに「座標系が違うから Core へ畳めない」と書いてあったが、
/// **結論だけが正しくて理由が嘘だった。**
///
/// **弾 1 個 につき 1 個 作る。** 狙う相手を覚えるのは弾ごとで、
/// 使い回すと別の弾が選んだ相手を引き継いでしまう。
///
/// **公開にしてある。** internal にすると門（`FsBulletML2.MonoGame.Tests`）が
/// `InternalsVisibleTo` 越しに見ることになり、「公開だけで書けているか」を
/// 測るというフロント側の門の役目が消える。このフロントを使うゲームが
/// 自前で `Env` を組みたいときにも要る。
[<Sealed>]
type MonoGameEnv() =

  let near =
    NearestEnemy<IBullet>((fun () -> Manager.enemies :> IReadOnlyList<IBullet>),
                          (fun e -> e.X), (fun e -> e.Y))

  /// 覚えている相手を捨てる。次に聞かれたら選び直す
  member _.Forget () = near.Forget ()

  interface IFrontEnv with
    member _.Rand = BulletMLManager.GetRandom
    member _.Rank = BulletMLManager.GetRank ()
    member _.PlayerX = BulletMLManager.GetPlayerPosX ()
    member _.PlayerY = BulletMLManager.GetPlayerPosY ()

    /// 一度 選んだ相手を持ち回る。**毎コマ 選び直すと軌跡が変わる**
    member _.TryTargetFrom (x, y, ex, ey) = near.TryFrom (x, y, &ex, &ey)

    /// 産まれたばかりの弾はまだ相手を覚えていないので、産まれる位置から
    /// その場で選ぶ。**覚えない** —— 旧 GetSpawnEnemyAimDir の振る舞い
    member _.TrySpawnTargetFrom (x, y, ex, ey) = near.TryNearest (x, y, &ex, &ey)

/// このフロントの並び。**2 つ とも 1 か所 だけに書く。**
module MonoGameFront =

  /// 画面座標で Y が下向き
  let space = Space.YDown

  /// 撃った弾は位置を入れずに作るので、狙いの基準は原点
  let origin = SpawnOrigin.AtOrigin
