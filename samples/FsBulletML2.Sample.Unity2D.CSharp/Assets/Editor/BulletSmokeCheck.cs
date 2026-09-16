using System;
using UnityEditor;
using UnityEngine;
using FsBulletML2;

/// <summary>
/// 弾が出ることを機械に言わせる。
///
/// 走らせ方（Unity を開かずに済む）:
///
/// コールバックで渡す形なので、数えるだけの関数を渡せばエンティティを
///
/// 較正した。 既存が緑のまま、この門だけが赤くなる変異:
///
/// 同じ門で当て直したときに、緑のまま通ることに気づいた。
public static class BulletSmokeCheck
{
    /// <summary>rand と rank と自機の位置を固定する。動かすと数が走行ごとに変わる</summary>
    class FixedManager : IBulletMLManager
    {
        public float GetRandom() { return 0.5f; }
        public float GetRank() { return 0.5f; }
        public float GetPlayerPosX() { return 2.4f; }
        public float GetPlayerPosY() { return -1.0f; }
    }

    /// <summary>
    /// 弾幕を 1 本 走らせて「撃った数 / 場の数 / 動いた数」を出す。
    /// 撃つか動くかが 0 なら赤にする。
    /// </summary>
    static int Fire(string label, BulletmlScript script, BulletKind kind,
                    FsBulletML2.DTD.BulletType type, float x, float y)
    {
        var failures = 0;

        var root = new BulletSim { Kind = kind, BulletType = type, X = x, Y = y };
        root.Init();
        // SetScript より前に立てる。 根の立場（狙う先と、撃たれた弾か）は
        // SetScript が 1 回 だけ読んで Core へ渡す
        root.IsBullet = false;
        root.SetScript(script);

        var live = new System.Collections.Generic.List<BulletSim> { root };
        var born = 0;

        System.Action<BulletSim, BulletRun> spawn = (parent, child) =>
        {
            born++;
            var motion = child.Motion;
            var sim = new BulletSim { Kind = parent.Kind, BulletType = parent.BulletType };
            sim.Init();
            sim.SetRun(child);
            sim.X = motion.Pos.X;
            sim.Y = motion.Pos.Y;
            sim.Dir = motion.Dir;
            sim.Speed = motion.Speed;
            live.Add(sim);
        };

        // このコマで回す顔ぶれを先に固める。産まれた弾は次のコマから
        for (int frame = 0; frame < 60; frame++)
        {
            var count = live.Count;
            for (int i = 0; i < count; i++)
            {
                if (live[i].Used)
                {
                    live[i].Step(spawn);
                }
            }
        }

        var moved = 0;
        for (int i = 1; i < live.Count; i++)
        {
            if (Mathf.Abs(live[i].X - x) > 0.0001f || Mathf.Abs(live[i].Y - y) > 0.0001f)
            {
                moved++;
            }
        }

        // エンジンが 1 コマ に何 ms 使うか。 描画も ECS も通さない値なので、
        // 「FPS が出ない」の出どころがエンジンかどうかを割るのに使う。
        // 60 FPS の予算は 16.7 ms、40 FPS なら 25 ms
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var laps = 0;
        for (int frame = 0; frame < 60; frame++)
        {
            var count = live.Count;
            for (int i = 0; i < count; i++)
            {
                if (live[i].Used)
                {
                    // 産まれた弾は数えるだけにする。ここで増やすと 2 倍 に膨らむ
                    live[i].Step((p, c) => { });
                    laps++;
                }
            }
        }
        sw.Stop();
        var msPerFrame = sw.Elapsed.TotalMilliseconds / 60.0;

        Debug.Log(string.Format(
            "[BulletSmokeCheck] {0}: 60 コマ で 撃った {1} 発 / 場に {2} 本 / 動いた {3} 本"
            + " / エンジンだけで 1 コマ {4:F3} ms（弾 {5} 本 を回した。60 FPS の予算 16.7 ms）",
            label, born, live.Count, moved, msPerFrame, live.Count));

        // 0 件 を緑にしない。 撃たない・動かないなら、受け渡しのどこかが
        // 切れている（コンパイルは通るので、ここでしか出ない）
        if (born <= 0)
        {
            Debug.LogError("[BulletSmokeCheck] " + label + ": 1 発 も撃っていない。Script か Env の受け渡しが切れている");
            failures++;
        }

        if (moved <= 0)
        {
            Debug.LogError("[BulletSmokeCheck] " + label + ": 撃った弾が 1 本 も動いていない。Frame.Delta の使い方が違う");
            failures++;
        }

        return failures;
    }

    public static void Run()
    {
        var failures = 0;
        try
        {
            BulletMLManager.Init(new FixedManager());

            // 敵の弾幕。Enemy.GetBulletml の先頭と同じもの
            var info = FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA;
            failures += Fire(
                info.Name,
                info.Script(CSharpEnv.RandFunc, CSharpEnv.LoadRank),
                BulletKind.Enemy,
                FsBulletML2.DTD.BulletType.Enemy,
                2.4f, -0.5f);

            // 自機の弾。Player.Awake と同じ通り道（Bulletml から Runner.Load）。
            // 敵の弾とは通る枝が違う（BulletType.Player の分岐、Spawn.ToEnemy）
            failures += Fire(
                "自機の 2way（左）",
                Runner.Load(CSharpEnv.RandFunc, CSharpEnv.LoadRank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet),
                BulletKind.Player,
                FsBulletML2.DTD.BulletType.Player,
                2.4f, -5.0f);

            failures += Fire(
                "自機のホーミング",
                Runner.Load(CSharpEnv.RandFunc, CSharpEnv.LoadRank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.homing),
                BulletKind.Player,
                FsBulletML2.DTD.BulletType.Player,
                2.4f, -5.0f);
        }
        // FsBulletML2 にも Exception という名前があるので、System のほうを名指す
        catch (System.Exception e)
        {
            Debug.LogError("[BulletSmokeCheck] 落ちた: " + e);
            failures++;
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
