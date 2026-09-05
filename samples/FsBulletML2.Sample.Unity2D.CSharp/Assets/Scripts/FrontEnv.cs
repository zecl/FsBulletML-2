using System;
using UnityEngine;
using FsBulletML2;
using Microsoft.FSharp.Core;
// Domain は F# の module（C# からは型）なので using できない。別名を張る
using Env = FsBulletML2.Domain.Env;

/// <summary>
/// このサンプルが <c>Env</c> を組むところ。<b>4 本 の aim を入れる場所はここだけ。</b>
///
/// 以前は BaseBullet と BulletSim にそれぞれ GetAimDir / GetSpawnAimDir /
/// GetEnemyAimDir / GetSpawnEnemyAimDir が生えていて、エンジンが呼び返していた。
/// 新 API はフロントが Env を組んで渡すので、呼び返しは無い。
///
/// 散らしておくと <c>Aim</c> に <c>Spawn</c> を入れるような取り違えを
/// 門で当てられない（型はどれも float なので通ってしまう）。
/// 同梱の FsBulletML2.MonoGame.FrontEnv と同じ理由でここに集めてある。
///
/// <b>式はこのフロント固有。</b> MonoGame 版とは 2 つ 違う。
/// <list type="bullet">
/// <item>Y の符号   こちらは反転しない（Unity は上が正）。MonoGame は -(py - y)</item>
/// <item>Spawn の元 こちらは撃った側と同じ場所に作るので Aim と同値。
///                  MonoGame は原点に作るので別式</item>
/// </list>
/// </summary>
public static class FrontEnv
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
    public static float Rank => BulletMLManager.GetRank();

    /// <summary>
    /// 自機を狙う向き。旧 GetAimDir の式そのまま。
    /// 旧はエンジンが呼び返していたが、いまは Env を組むためにフロントが自分で呼ぶ。
    /// </summary>
    public static float AimAtPlayer(float x, float y)
    {
        return Mathf.Atan2(
            BulletMLManager.GetPlayerPosX() - x,
            BulletMLManager.GetPlayerPosY() - y);
    }

    /// <summary>
    /// このコマの <c>Env</c> を、いまの位置から組む。旧 BulletRunner.envOfGlobal の写し。
    ///
    /// <b>組む位置が変わると aim がずれる</b>ので、呼ぶ側は step の直前
    /// （差分を足す前）に組むこと。
    ///
    /// <paramref name="enemyAim"/> だけ引数で受けるのは、<b>狙う相手の選び方が
    /// 弾の種類で違う</b>ため —— ECS の弾は BulletEcsRuntime.Enemy を見て、
    /// GameObject の弾は一度 選んだ相手を持ち回る。残り 3 本 はグローバルと
    /// 位置だけで決まるので、ここに閉じている。
    ///
    /// <b>Spawn 側は撃った側と同じ値。</b> 産まれた弾は撃った側と同じ場所に作る
    /// （BulletEntityFactory.SpawnChild が parent.X / parent.Y をそのまま渡す）。
    /// <b>片方だけ直すと軌跡が割れる。</b>
    /// </summary>
    public static Env At(float x, float y, float enemyAim)
    {
        var aim = AimAtPlayer(x, y);
        // Aim と SpawnAim は別の型。値は同じでも、入れ替えるとコンパイルで落ちる
        // （3 番目 と 4 番目 を入れ替えて CS1503 になることを確かめてある）
        return new Env(
            RandFunc,
            BulletMLManager.GetRank(),
            new FsBulletML2.Domain.Aim(toPlayer: aim, toEnemy: enemyAim),
            new FsBulletML2.Domain.SpawnAim(toPlayer: aim, toEnemy: enemyAim));
    }

    /// <summary>
    /// aim を読まないと分かっているコマの <c>Env</c>。aim 4 本 を 0 に。
    ///
    /// 使ってよい条件は <c>BulletRun.HasNoScript</c> の但し書きにある。
    /// <b><c>At</c> と欄が 1 つ でもずれたら、片方だけ直したということ。</b>
    /// </summary>
    public static Env NoAim()
    {
        return new Env(
            RandFunc,
            BulletMLManager.GetRank(),
            new FsBulletML2.Domain.Aim(toPlayer: 0f, toEnemy: 0f),
            new FsBulletML2.Domain.SpawnAim(toPlayer: 0f, toEnemy: 0f));
    }

}
