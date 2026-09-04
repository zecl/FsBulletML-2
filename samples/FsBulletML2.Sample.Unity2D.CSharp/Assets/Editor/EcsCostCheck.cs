using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// <b>ECS 側が 1 コマ にいくら使うかを、Unity を Play せずに測る。</b>
///
/// 走らせ方:
/// <code>
/// Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; -logFile &lt;log&gt; ^
///           -executeMethod EcsCostCheck.Run
/// </code>
///
/// エンジン（Runner.StepWith）が 1 コマ 0.12〜0.43 ms しか使わないことは
/// <see cref="BulletSmokeCheck"/> で測ってある。<b>残りがどこに行っているか</b>を
/// 割るのがここ。60 fps の予算は 16.7 ms。
///
/// 測るのは 3 つ。どれも弾 1 発 ごとに走るもの。
/// <list type="number">
/// <item><b>生成</b>  CreateEntity + AddComponentData × 3。
///       ECS では 1 回 ごとにアーキタイプ間でエンティティが移動する（構造変更）</item>
/// <item><b>更新</b>  GetComponentObject（managed）+ SetComponentData。毎コマ 全弾</item>
/// <item><b>破棄</b>  DestroyEntity。これも構造変更</item>
/// </list>
///
/// <b>描く時間そのものは測っていない。</b> Configure を通すので描画
/// コンポーネントは付くが、GPU と culling は -nographics では動かない。
/// つまり<b>ここで出るのは CPU 側の下限</b>で、本番はこれより重い。
///
/// <b>実測（弾 600 本、この機械）</b>:
/// <list type="bullet">
/// <item>生成 1 発 0.109 ms —— 1 コマ 10 発 なら 1.09 ms</item>
/// <item>更新 0.53 ms/コマ（600 本 ぜんぶ）</item>
/// <item>破棄 1 発 0.006 ms</item>
/// </list>
/// 合わせて <b>16.7 ms の 9.7%</b>。エンジン（BulletSmokeCheck）が 3% 未満 なので、
/// <b>この 2 つ で 13%。残り 87% は測れていない側にある</b>
/// —— 描画、IMGUI（Informations.OnGUI）、物理、R3。
/// </summary>
public static class EcsCostCheck
{
    /// 場に残る弾の数。10Way が 60 コマ で 600 発 撃つので、その規模に合わせる
    const int Bullets = 600;

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
                world = new World("EcsCostCheck");
                World.DefaultGameObjectInjectionWorld = world;
                madeWorld = true;
            }

            var em = world.EntityManager;

            // **描画コンポーネントも付ける。** Configure を通さないと
            // RenderMeshUtility.AddComponents が飛ばされ、本番より軽い数が出る。
            // sprite は null でよい（既定の四角が使われる）
            BulletEntityFactory.Configure(null, null);

            // --- 1. 生成 ---
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var sims = new BulletSim[Bullets];
            for (int i = 0; i < Bullets; i++)
            {
                sims[i] = BulletEntityFactory.Spawn(BulletKind.Enemy, i * 0.001f, -1f, root: false);
            }
            sw.Stop();
            var spawnMs = sw.Elapsed.TotalMilliseconds;

            // --- 2. 更新（BulletSimulationSystem.OnUpdate と同じ触り方）---
            var query = em.CreateEntityQuery(
                ComponentType.ReadWrite<BulletSim>(),
                ComponentType.ReadWrite<LocalTransform>(),
                ComponentType.ReadOnly<BulletTag>());

            const int Frames = 60;
            sw.Restart();
            for (int f = 0; f < Frames; f++)
            {
                var entities = query.ToEntityArray(Allocator.Temp);
                try
                {
                    for (int i = 0; i < entities.Length; i++)
                    {
                        var e = entities[i];
                        var sim = em.GetComponentObject<BulletSim>(e);
                        var tag = em.GetComponentData<BulletTag>(e);
                        var tr = em.GetComponentData<LocalTransform>(e);
                        tr.Position = new float3(sim.X, sim.Y, 0f);
                        tr.Rotation = quaternion.AxisAngle(new float3(0f, 0f, 1f), -sim.Dir);
                        em.SetComponentData(e, tr);
                    }
                }
                finally
                {
                    entities.Dispose();
                }
            }
            sw.Stop();
            var updateMsPerFrame = sw.Elapsed.TotalMilliseconds / Frames;

            // --- 3. 破棄 ---
            sw.Restart();
            for (int i = 0; i < Bullets; i++)
            {
                BulletEntityFactory.Destroy(sims[i]);
            }
            sw.Stop();
            var destroyMs = sw.Elapsed.TotalMilliseconds;

            query.Dispose();

            Debug.Log(string.Format(
                "[EcsCostCheck] 弾 {0} 本\n"
                + "  生成   {1,8:F3} ms 通し（1 発 {2:F4} ms）\n"
                + "  更新   {3,8:F3} ms/コマ（毎コマ 全弾。60 fps の予算 16.7 ms）\n"
                + "  破棄   {4,8:F3} ms 通し（1 発 {5:F4} ms）\n"
                + "  ※ 描画コンポーネント（RenderMeshUtility）も付けた数。\n"
                + "  ※ ただし実際に描く時間（GPU と culling）は -nographics では測れない",
                Bullets, spawnMs, spawnMs / Bullets,
                updateMsPerFrame, destroyMs, destroyMs / Bullets));

            // **1 発 の値だけでは「効くか」が分からない。** 1 コマ に何発 撃つかを
            // 掛けて、60 fps の予算に対する割合で出す。10Way は 60 コマ で 600 発
            // ＝ 平均 10 発/コマ（ピークはもっと）
            var perFrameSpawn = spawnMs / Bullets * 10.0;
            Debug.Log(string.Format(
                "[EcsCostCheck] 1 コマ に 10 発 撃つとして: 生成 {0:F3} ms + 更新 {1:F3} ms"
                + " = {2:F3} ms（16.7 ms の {3:F1}%）",
                perFrameSpawn, updateMsPerFrame,
                perFrameSpawn + updateMsPerFrame,
                (perFrameSpawn + updateMsPerFrame) / 16.7 * 100.0));

            // **0 を緑にしない。** 1 本 も作れていなければ測れていない
            if (Bullets <= 0 || spawnMs <= 0.0)
            {
                Debug.LogError("[EcsCostCheck] 測れていない（弾を作れていないか、時計が動いていない）");
                failures++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[EcsCostCheck] 落ちた: " + e);
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

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
