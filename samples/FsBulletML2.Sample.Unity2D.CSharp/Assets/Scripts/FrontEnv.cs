using System;
using System.Collections.Generic;
using UnityEngine;
using FsBulletML2;
using FsBulletML2.Front;
using Microsoft.FSharp.Core;

/// <summary>
/// このサンプルが <c>FsBulletML2.Front</c> の口に答えるところ。
///
/// <b>式そのものはここに無い。</b> aim 4 本 は <c>Aiming.Toward</c> の
/// <c>Space</c> 違いで、MonoGame の同じ関数と 1 ビット しか違わなかった
/// （凍結は tests/FsBulletML2.Front.Tests/AimingFreeze.fs）。
///
/// <b>狙う相手の選び方だけが弾の種類で違う</b>ので、そこを派生で分ける ——
/// GameObject の弾は一度 選んだ相手を持ち回り、ECS の弾は
/// BulletEcsRuntime.Enemy を毎コマ 見る。
/// </summary>
public abstract class CSharpWorld : IWorld
{
    /// <summary>
    /// <c>Env.Rand</c> に入れる F# の関数値。<b>1 個 だけ作って使い回す。</b>
    ///
    /// 中身はグローバル（BulletMLManager）を読むだけなので、いつ作っても同じ
    /// ものになる。毎コマ FuncConvert すると弾数 × コマ数 だけヒープを踏む。
    /// </summary>
    public static readonly FSharpFunc<Unit, float> RandFunc =
        FuncConvert.FromFunc<float>(() => BulletMLManager.GetRandom());

    /// <summary>
    /// 読む段（<c>Runner.Load</c>）に渡すランク。
    /// <b>読む段は <c>Env</c> を取らない</b> —— 木を組むときに読むのは
    /// 乱数とランクだけで、aim 4 本 は撃つ弾ごとの位置がまだ無いので
    /// 読まれない。欄が無ければ取り違えようがない。
    /// </summary>
    public static float LoadRank => BulletMLManager.GetRank();

    /// <summary>Unity は Y が上向き。</summary>
    public const Space Space = FsBulletML2.Front.Space.YUp;

    /// <summary>
    /// 産まれた弾は撃った側と同じ場所に作る
    /// （BulletEntityFactory.SpawnChild が parent.X / parent.Y をそのまま渡す）。
    /// <b>片方だけ直すと軌跡が割れる。</b>
    /// </summary>
    public const SpawnOrigin Origin = FsBulletML2.Front.SpawnOrigin.AtShooter;

    FSharpFunc<Unit, float> IWorld.Rand => RandFunc;
    float IWorld.Rank => BulletMLManager.GetRank();
    float IWorld.PlayerX => BulletMLManager.GetPlayerPosX();
    float IWorld.PlayerY => BulletMLManager.GetPlayerPosY();

    public abstract bool TryTargetFrom(float x, float y, out float ex, out float ey);

    /// <summary>
    /// <b>撃った側と同じ相手。</b> このフロントは撃った側と同じ場所に弾を作る。
    /// </summary>
    public bool TrySpawnTargetFrom(float x, float y, out float ex, out float ey)
        => TryTargetFrom(x, y, out ex, out ey);
}

/// <summary>
/// GameObject の弾から見た世界。<b>弾 1 個 につき 1 個。</b>
///
/// 敵は <c>FindGameObjectsWithTag</c> で引く。<b>作るときではなく、
/// 相手が要るときに初めて引く</b>ので <c>NearestEnemy</c> に
/// 「一覧を返すもの」を渡している。
/// </summary>
public sealed class GameObjectWorld : CSharpWorld
{
    static readonly Func<IReadOnlyList<GameObject>> Enemies =
        () => GameObject.FindGameObjectsWithTag("Enemy");

    readonly NearestEnemy<GameObject> near =
        new NearestEnemy<GameObject>(
            Enemies,
            g => g.transform.position.x,
            g => g.transform.position.y);

    /// <summary>覚えている相手を捨てる。次に聞かれたら選び直す。</summary>
    public void Forget() => near.Forget();

    /// <summary>
    /// 一度 選んだ相手を持ち回る。<b>毎コマ 選び直すと相手が入れ替わって
    /// 軌跡が変わる。</b>
    /// </summary>
    public override bool TryTargetFrom(float x, float y, out float ex, out float ey)
        => near.TryFrom(x, y, out ex, out ey);
}

/// <summary>
/// ECS の弾から見た世界。<b>敵は 1 体 しか居ない</b>ので一覧を持たず、
/// <c>BulletEcsRuntime.Enemy</c> をそのまま答える ——
/// 口が一覧を要求しないのはこのため。
/// </summary>
public sealed class EcsWorld : CSharpWorld
{
    public override bool TryTargetFrom(float x, float y, out float ex, out float ey)
    {
        var enemy = BulletEcsRuntime.Enemy;
        if (enemy == null)
        {
            ex = 0f;
            ey = 0f;
            return false;
        }

        var p = enemy.transform.position;
        ex = p.x;
        ey = p.y;
        return true;
    }
}
