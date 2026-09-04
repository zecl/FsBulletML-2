using UnityEditor;
using UnityEngine;
using FsBulletML2;
using FsBulletML2.Sample.Unity2D.FSharp;

/// <summary>
/// <b>弾が出ることを機械に言わせる。</b>
///
/// 走らせ方（Unity を開かずに済む）:
/// <code>
/// Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; -logFile &lt;log&gt; ^
///           -executeMethod BulletSmokeCheck.Run
/// </code>
///
/// <b>コンパイルが通ることと、弾が出ることは別。</b>
/// 同梱の C# サンプルでは、この形の門が実際に穴を 1 つ 見つけた ——
/// F# の判別共用体は参照型なので既定値が null になり、入れ忘れたまま
/// <c>Runner.StepWith</c> に渡すとエンジンが match したところで落ちる。
/// コンパイルは通るので、走らせるまで出ない。
///
/// <b>ECS の World は要らない。</b> <see cref="BulletSim.Step"/> は撃たれた弾を
/// コールバックで渡す形なので、数えるだけの関数を渡せばエンティティを
/// 作らずに回せる。ここで測りたいのはエンジンとの受け渡しであって、
/// 描画やエンティティ管理ではない。
///
/// <b>この門が守らない範囲</b>:
/// <list type="bullet">
/// <item><b>値の取り違え</b>。名前が正しくて値が違うものは、弾は出るので通る</item>
/// <item><b>描画と当たり判定</b>。World もシーンも使わないので分からない</item>
/// <item><b>BulletEcsDriver / BulletEntityFactory</b>。あちらは EntityManager を
///       触るので、World を作らないと回せない</item>
/// </list>
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
    /// <paramref name="type"/> に null を渡すと <b>BulletType を入れずに</b>
    /// 走らせる —— <b>既定値が消えたときに落ちる経路</b>。
    /// F# の判別共用体は参照型なので、既定を書き忘れると null になり、
    /// エンジンが match したところで NullReferenceException になる。
    /// **明示的に入れてしまうと、その穴は門をすり抜ける**（実際にすり抜けた）。
    /// </summary>
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
        root.SetScript(script, null);
        root.IsBullet = false;

        var live = new System.Collections.Generic.List<BulletSim> { root };
        var born = 0;

        System.Action<BulletSim, BulletRun> spawn = (parent, child) =>
        {
            born++;
            var body = child.Body;
            var sim = new BulletSim();
            sim.Kind = parent.Kind;
            // 親の種別を引き継ぐ。**BulletEntityFactory.SpawnChild と同じ形**
            sim.BulletType = parent.BulletType;
            sim.Init();
            sim.SetScript(parent.Script, child);
            sim.X = body.Pos.X;
            sim.Y = body.Pos.Y;
            sim.Dir = body.Dir;
            sim.Speed = body.Speed;
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

        // **0 件 を緑にしない。** 撃たない・動かないなら、受け渡しのどこかが
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

            // 敵の弾幕。Enemy.GetBulletml の先頭と同じもの。
            // **BulletType を渡さない** —— BulletSim の既定値が生きているかを
            // ここで見る（渡してしまうと、既定が null でも通ってしまう）
            var info = FsBulletML2.Bullets.Dsl.EnemyBullet.Sdmkun.SilverGun.b4D_boss_PENTA;
            failures += Fire(
                info.Name + "（BulletType は既定値のまま）",
                Runner.Load(FrontEnv.Load(), info.Bulletml),
                BulletKind.Enemy,
                null,
                2.4f, -0.5f);

            // 自機の弾。**Player.Awake と同じ通り道**。
            // 敵の弾とは通る枝が違う（BulletType.Player の分岐）
            failures += Fire(
                "自機の 2way（左）",
                Runner.Load(FrontEnv.Load(), FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet),
                BulletKind.Player,
                FsBulletML2.DTD.BulletType.Player,
                2.4f, -5.0f);

            failures += Fire(
                "自機のホーミング",
                Runner.Load(FrontEnv.Load(), FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.homing),
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
