using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using FsBulletML2.Sample.Unity2D.FSharp;

/// <summary>
/// Entity を実際に作って、component の型が登録されているかを見る。
///
/// 走らせ方:
public static class EcsEntityCheck
{
    public static void Run()
    {
        var failures = 0;
        World world = null;
        var madeWorld = false;
        try
        {
            world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                world = new World("EcsEntityCheck");
                World.DefaultGameObjectInjectionWorld = world;
                madeWorld = true;
            }

            var em = world.EntityManager;

            // 1. component の型が引けるか。ここが Unknown Type で落ちていた
            var simType = ComponentType.ReadWrite<BulletSim>();
            var tagType = ComponentType.ReadWrite<BulletTag>();
            Debug.Log("[EcsEntityCheck] component の型を引けた: BulletSim / BulletTag");

            // 2. Entity を作って付けられるか
            BulletEntityFactory.Configure(null, null);
            var sim = BulletEntityFactory.Spawn(BulletKind.Enemy, 1f, -1f, false);
            if (sim == null || sim.Entity == Entity.Null)
            {
                Debug.LogError("[EcsEntityCheck] Spawn が Entity を返さなかった");
                failures++;
            }
            else
            {
                Debug.Log("[EcsEntityCheck] Spawn できた: entity=" + sim.Entity.Index);
            }

            // 3. Driver と同じ形で query を組めるか
            var query = em.CreateEntityQuery(
                ComponentType.ReadWrite<BulletSim>(),
                ComponentType.ReadWrite<LocalTransform>(),
                ComponentType.ReadOnly<BulletTag>());
            var entities = query.ToEntityArray(Allocator.Temp);
            var found = entities.Length;
            entities.Dispose();
            query.Dispose();

            Debug.Log("[EcsEntityCheck] query が引いた Entity は " + found + " 個");

            // 0 件 を緑にしない。 作ったのに引けないなら、component が
            // 付いていないか query の組み方が違う
            if (found <= 0)
            {
                Debug.LogError("[EcsEntityCheck] Entity を作ったのに query が 1 個 も引けない");
                failures++;
            }

            BulletEntityFactory.DestroySim(sim);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[EcsEntityCheck] 落ちた: " + e);
            failures++;
        }
        finally
        {
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
