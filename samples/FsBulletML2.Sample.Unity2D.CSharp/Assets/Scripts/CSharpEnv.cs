using System;
using System.Collections.Generic;
using UnityEngine;
using FsBulletML2;
using FsBulletML2.Front;
using Microsoft.FSharp.Core;

/// <summary>
/// このサンプルが <c>FsBulletML2.Front</c> の口に答えるところ。
///
/// 式そのものはここに無い。 aim 4 本 は <c>Aiming.Toward</c> の
/// <c>Space</c> 違いで、MonoGame の同じ関数と 1 ビット しか違わなかった
/// （凍結は tests/FsBulletML2.Front.Tests/AimingFreeze.fs）。
public abstract class CSharpEnv : IFrontEnv
{
    /// <summary>
    /// <c>Env.Rand</c> に入れる F# の関数値。1 個 だけ作って使い回す。
    ///
    /// 中身はグローバル（BulletMLManager）を読むだけなので、いつ作っても同じ
    /// ものになる。毎コマ FuncConvert すると弾数 × コマ数 だけヒープを踏む。
    /// </summary>
    public static readonly FSharpFunc<Unit, float> RandFunc =
        FuncConvert.FromFunc<float>(() => BulletMLManager.GetRandom());

    /// <summary>
    /// 読む段（<c>Runner.Load</c>）に渡すランク。
    /// 読む段は <c>Env</c> を取らない —— 木を組むときに読むのは
    /// 乱数とランクだけで、aim 4 本 は撃つ弾ごとの位置がまだ無いので
    /// 読まれない。欄が無ければ取り違えようがない。
    /// </summary>
    public static float LoadRank => BulletMLManager.GetRank();

    /// <summary>Unity は Y が上向き。
    /// 型を完全修飾する —— UnityEngine にも Space が在るので、
    /// 短く書くと CS0104（あいまいな参照）になる。</summary>
    public const FsBulletML2.Front.Space Space = FsBulletML2.Front.Space.YUp;

    /// <summary>
    /// 産まれた弾は撃った側と同じ場所に作る
    /// （BulletEntityFactory.SpawnChild が parent.X / parent.Y をそのまま渡す）。
    /// 片方だけ直すと軌跡が割れる。
    /// </summary>
    public const FsBulletML2.Front.SpawnOrigin Origin = FsBulletML2.Front.SpawnOrigin.AtShooter;

    FSharpFunc<Unit, float> IFrontEnv.Rand => RandFunc;
    float IFrontEnv.Rank => BulletMLManager.GetRank();
    float IFrontEnv.PlayerX => BulletMLManager.GetPlayerPosX();
    float IFrontEnv.PlayerY => BulletMLManager.GetPlayerPosY();

    public abstract bool TryTargetFrom(float x, float y, out float ex, out float ey);

    /// <summary>
    /// 撃った側と同じ相手。 このフロントは撃った側と同じ場所に弾を作る。
    /// </summary>
    public bool TrySpawnTargetFrom(float x, float y, out float ex, out float ey)
        => TryTargetFrom(x, y, out ex, out ey);
}

/// <summary>
/// GameObject の弾から見た世界。弾 1 個 につき 1 個。
///
/// 敵は <c>FindGameObjectsWithTag</c> で引く。作るときではなく、
/// 相手が要るときに初めて引くので <c>NearestEnemy</c> に
/// 「一覧を返すもの」を渡している。
/// </summary>
public sealed class GameObjectEnv : CSharpEnv
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
    /// 一度 選んだ相手を持ち回る。毎コマ 選び直すと相手が入れ替わって
    /// 軌跡が変わる。
    /// </summary>
    public override bool TryTargetFrom(float x, float y, out float ex, out float ey)
        => near.TryFrom(x, y, out ex, out ey);
}

/// <summary>
/// ECS の弾から見た世界。敵は 1 体 しか居ないので一覧を持たず、
/// <c>BulletEcsRuntime.Enemy</c> をそのまま答える ——
/// 口が一覧を要求しないのはこのため。
/// </summary>
public sealed class EcsEnv : CSharpEnv
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
