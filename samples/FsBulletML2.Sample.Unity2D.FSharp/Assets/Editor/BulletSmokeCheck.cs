using UnityEditor;
using UnityEngine;
using FsBulletML2;
using FsBulletML2.Sample.Unity2D.FSharp;

/// <summary>
/// 弾が出ることを機械に言わせる。
/// </summary>
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
    /// 弾幕を 1 本 走らせる。
    ///
    /// <paramref name="type"/> に null を渡すと BulletType を入れずに
    /// 走らせる —— 既定値が消えたときに落ちる経路。
    static int Fire(string label, BulletmlScript script, BulletKind kind,
                    FsBulletML2.DTD.BulletType type, float x, float y)
    {
        var failures = 0;

        var root = new BulletSim();
        root.Kind = kind;
        if (type != null)
        {
            root.BulletType = type;
        }
        root.Init();
        root.X = x;
        root.Y = y;
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
            var sim = new BulletSim();
            sim.Kind = parent.Kind;
            // 親の種別を引き継ぐ。BulletEntityFactory.SpawnChild と同じ形
            sim.BulletType = parent.BulletType;
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

        Debug.Log(string.Format(
            "[BulletSmokeCheck] {0}: 60 コマ で 撃った {1} 発 / 場に {2} 本 / 動いた {3} 本",
            label, born, live.Count, moved));

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

            // 敵の弾幕。
            var info = FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA;
            failures += Fire(
                info.Name + "（BulletType は既定値のまま）",
                Runner.Load(EcsEnv.RandFunc, BulletMLManager.GetRank(), info.Bulletml),
                BulletKind.Enemy,
                null,
                2.4f, -0.5f);

            // 自機の弾。Player.Awake と同じ通り道。
            // 敵の弾とは通る枝が違う（BulletType.Player の分岐）
            failures += Fire(
                "自機の 2way（左）",
                Runner.Load(EcsEnv.RandFunc, BulletMLManager.GetRank(), FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet),
                BulletKind.Player,
                FsBulletML2.DTD.BulletType.Player,
                2.4f, -5.0f);

            failures += Fire(
                "自機のホーミング",
                Runner.Load(EcsEnv.RandFunc, BulletMLManager.GetRank(), FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.homing),
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
