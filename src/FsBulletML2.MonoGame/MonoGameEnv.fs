namespace FsBulletML2.MonoGame

open System.Collections.Generic
open FsBulletML2
open FsBulletML2.Front

/// このフロントが `FsBulletML2.Front` の口に答えるところ。式そのものはここに無い。
/// 弾 1 個 につき 1 個 作る。使い回すと別の弾が選んだ相手を引き継ぐ。
[<Sealed>]
type MonoGameEnv() =

    let near =
        NearestEnemy<IBullet>((fun () -> Manager.enemies :> IReadOnlyList<IBullet>), (fun e -> e.X), (fun e -> e.Y))

    /// 覚えている相手を捨てる。次に聞かれたら選び直す
    member _.Forget() = near.Forget()

    interface IFrontEnv with
        member _.Rand = BulletMLManager.GetRandom
        member _.Rank = BulletMLManager.GetRank()
        member _.PlayerX = BulletMLManager.GetPlayerPosX()
        member _.PlayerY = BulletMLManager.GetPlayerPosY()

        /// 一度 選んだ相手を持ち回る。毎コマ 選び直すと軌跡が変わる
        member _.TryTargetFrom(x, y, ex, ey) = near.TryFrom(x, y, &ex, &ey)

        /// 産まれたばかりの弾はまだ相手を覚えていないので、産まれる位置から
        /// その場で選ぶ。覚えない —— 旧 GetSpawnEnemyAimDir の振る舞い
        member _.TrySpawnTargetFrom(x, y, ex, ey) = near.TryNearest(x, y, &ex, &ey)

/// このフロントの並び。2 つ とも 1 か所 だけに書く。
module MonoGameFront =

    /// 画面座標で Y が下向き
    let space = Space.YDown

    /// 撃った弾は位置を入れずに作るので、狙いの基準は原点
    let origin = SpawnOrigin.AtOrigin
