using UnityEditor;
using UnityEngine;
using Unity.Entities;
using R3;
using FsBulletML2;
using FsBulletML2.Sample.Unity2D.FSharp;

/// <summary>
/// 敵が実際に弾を撃つところまでを、Play せずに見る。
/// </summary>
public static class EnemyShootCheck
{
    /// <summary>
    /// 弾幕を読むのに要る口。BulletSmokeCheck と同じ形で、
    /// 乱数を固定する（撃った本数を比べるわけではないが、走行を揃える）。
    /// </summary>
    class FixedManager : IBulletMLManager
    {
        public float GetRandom() { return 0.5f; }
        public float GetRank() { return 0f; }
        public float GetPlayerPosX() { return 2.4f; }
        public float GetPlayerPosY() { return -1.0f; }
    }

    public static void Run()
    {
        var failures = 0;
        World world = null;
        var madeWorld = false;
        GameObject tickerGo = null;
        GameObject enemyGo = null;

        try
        {
            world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                world = new World("EnemyShootCheck");
                World.DefaultGameObjectInjectionWorld = world;
                madeWorld = true;
            }

            // 弾を作れる状態にする。見た目は要らない（null で既定のまま）
            BulletEntityFactory.Configure(null, null);
            BulletEntityFactory.DestroyAllEnemy();

            // 弾幕を読む口。Enemy.Start より先（読む段の Env が引く）
            BulletMLManager.Init(new FixedManager());

            tickerGo = new GameObject("FrameTicker");
            var ticker = tickerGo.AddComponent<FrameTicker>();

            enemyGo = new GameObject("Enemy");
            var enemy = enemyGo.AddComponent<Enemy>();
            enemy.Awake();
            enemy.Start();

            var before = BulletEntityFactory.EnemyCount;

            // 撃つ合図は「弾幕を入れ替えた次のコマ」。 1 回 で足りるが、
            // 数コマ 回して確かめる（1 回 目 で出なければ配線が切れている）
            for (var i = 0; i < 5; i++)
            {
                ticker.Update();
            }

            var after = BulletEntityFactory.EnemyCount;
            Debug.Log("[EnemyShootCheck] 敵の弾: " + before + " -> " + after);

            // 0 件 を緑にしない。 撃っていないなら、Start の購読か
            // 撃つ合図の条件が切れている
            if (after <= before)
            {
                Debug.LogError("[EnemyShootCheck] コマを回しても敵の弾が増えない。"
                    + "Enemy.Start の購読か、撃つ合図の条件が切れている");
                failures++;
            }

            // 弾幕の名前が入っているか。番号の購読（Start の 2 番）が見る唯一の出口
            if (string.IsNullOrEmpty(enemy.BulletName))
            {
                Debug.LogError("[EnemyShootCheck] 弾幕の名前が空。番号の購読が届いていない");
                failures++;
            }
            else
            {
                Debug.Log("[EnemyShootCheck] 弾幕の名前: " + enemy.BulletName);
            }

            // 番号を動かすと名前が変わるか。Next は番号を動かすだけという
            // 作りなので、ここが変わらなければ購読が切れている
            var firstName = enemy.BulletName;
            enemy.Next();
            if (enemy.BulletName == firstName)
            {
                Debug.LogError("[EnemyShootCheck] Next で名前が変わらない（" + firstName + " のまま）。"
                    + "番号の購読が切れている");
                failures++;
            }
            else
            {
                Debug.Log("[EnemyShootCheck] Next で名前が変わった: " + firstName + " -> " + enemy.BulletName);
            }

            // ライフが入り直しているか。ApplyPattern が MaxLife から入れる
            if (enemy.Life <= 0)
            {
                Debug.LogError("[EnemyShootCheck] ライフが " + enemy.Life + "。"
                    + "ApplyPattern が届いていないか MaxLife が 0");
                failures++;
            }
            else
            {
                Debug.Log("[EnemyShootCheck] ライフ: " + enemy.Life);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[EnemyShootCheck] 落ちた: " + e);
            failures++;
        }
        finally
        {
            if (enemyGo != null) Object.DestroyImmediate(enemyGo);
            if (tickerGo != null) Object.DestroyImmediate(tickerGo);
            if (madeWorld && world != null)
            {
                World.DefaultGameObjectInjectionWorld = null;
                world.Dispose();
            }
        }

        LastFailures = failures;
        if (Application.isBatchMode && !SuppressExit)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }

    /// <summary>集約門から呼ぶときは Exit を抑える。見よ <see cref="SampleChecks"/></summary>
    public static bool SuppressExit;

    /// <summary>直前の走行で見つかった食い違いの数</summary>
    public static int LastFailures;
}
