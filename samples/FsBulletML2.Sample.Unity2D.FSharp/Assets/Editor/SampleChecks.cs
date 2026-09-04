using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// <b>門をまとめて 1 回 で回す。</b>
///
/// 走らせ方:
/// <code>
/// Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; -logFile &lt;log&gt; ^
///           -executeMethod SampleChecks.Run
/// </code>
///
/// <b>なぜまとめるか。</b> Unity の起動は 1 回 で数分 かかる。門が 4 本 に
/// なった時点で、1 本 ずつ回すのは現実的でなくなった ——
/// <b>回さない門は門ではない。</b>
///
/// まとめるために、各門は <c>SuppressExit</c> で <c>EditorApplication.Exit</c> を
/// 抑えられるようにしてある。あれはプロセスを即座に終わらせるので、
/// 抑えないと <b>2 本 目 以降 が 1 度 も走らない</b>（そして
/// 「1 本 目 が緑だった」だけを見て全部 緑だと読んでしまう）。
///
/// 走る順に意味がある ——
/// <list type="number">
///   <item>シーン: 他の門が GameObject を作る前に見る</item>
///   <item>BulletSmokeCheck: エンジンだけ。World も MonoBehaviour も要らない</item>
///   <item>EcsEntityCheck: component の型が TypeManager に登録されているか</item>
///   <item>R3WiringCheck: 発火源から購読へ届くか</item>
///   <item>EnemyShootCheck: Awake から弾が出るまで通しで</item>
/// </list>
/// 前が倒れると後ろも倒れる並びにしてある。<b>いちばん手前の赤を読めばよい。</b>
/// </summary>
public static class SampleChecks
{
    const string ScenePath = "Assets/Senes/FsBulletML2.Sample.Unity2D.unity";

    public static void Run()
    {
        var failures = 0;

        failures += CheckScene();
        failures += CheckAssemblies();

        BulletSmokeCheck.SuppressExit = true;
        BulletSmokeCheck.Run();
        failures += BulletSmokeCheck.LastFailures;

        EcsEntityCheck.SuppressExit = true;
        EcsEntityCheck.Run();
        failures += EcsEntityCheck.LastFailures;

        R3WiringCheck.SuppressExit = true;
        R3WiringCheck.Run();
        failures += R3WiringCheck.LastFailures;

        EnemyShootCheck.SuppressExit = true;
        EnemyShootCheck.Run();
        failures += EnemyShootCheck.LastFailures;

        Debug.Log("[SampleChecks] 食い違いの合計: " + failures + " 件"
            + "（scene / smoke " + BulletSmokeCheck.LastFailures
            + " / ecs " + EcsEntityCheck.LastFailures
            + " / wiring " + R3WiringCheck.LastFailures
            + " / shoot " + EnemyShootCheck.LastFailures + "）");

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }

    /// <summary>
    /// <b>Assets に置いた dll が、参照している相手を全部 見つけられるか。</b>
    ///
    /// このサンプルは F# を Unity の外でビルドして dll を Assets へ手で置く。
    /// <b>参照が増えたことは、置き忘れても何も言ってくれない</b> ——
    /// CLR は型を実際に使うまでアセンブリを読まないので、
    /// <b>使い始めた行が走った瞬間に FileNotFoundException</b> になる。
    /// 弾幕の途中で落ちるので、原因が読みにくい。
    ///
    /// 実際にこれが起きうる形がある。弾幕を書く CE（FsBulletML2.Dsl）を
    /// 別アセンブリに分けたので、<b>FsBulletML2.Bullets がそれを使い始めると
    /// 参照が 1 つ 増える</b>。分けた時点では 1 つ も使っていないため、
    /// Bullets.dll のマニフェストにその名前はまだ入っていない。
    ///
    /// <b>門そのものの目盛りも毎回 合わせる。</b> 在るはずのない名前を 1 つ
    /// 混ぜて、それが「解決できない」と出ることを確かめる ——
    /// 出なければ、この検査は何も見ていないことになる。
    /// </summary>
    static int CheckAssemblies()
    {
        var failures = 0;

        // 目盛り合わせ。**これが先**（当てにならない道具で測っても意味がない）
        if (Resolves("FsBulletML2.ThisMustNotExist"))
        {
            Debug.LogError("[SampleChecks] 在るはずのない名前が解決できてしまった。この検査は当てにならない");
            return failures + 1;
        }

        var loaded = System.AppDomain.CurrentDomain.GetAssemblies();
        var checkedRefs = 0;
        foreach (var asm in loaded)
        {
            var name = asm.GetName().Name;
            if (!name.StartsWith("FsBulletML2"))
            {
                continue;
            }

            foreach (var reference in asm.GetReferencedAssemblies())
            {
                checkedRefs++;
                if (!Resolves(reference.FullName))
                {
                    Debug.LogError("[SampleChecks] " + name + " が参照する "
                        + reference.Name + " を読めない。Assets に置き忘れている疑い");
                    failures++;
                }
            }
        }

        Debug.Log("[SampleChecks] アセンブリ参照: " + checkedRefs + " 本 を辿って読めた");

        // **0 件 を緑にしない。** dll が 1 つ も読み込まれていなければ
        // 「全部 読めた」と同じ見た目になる
        if (checkedRefs <= 0)
        {
            Debug.LogError("[SampleChecks] FsBulletML2 のアセンブリが 1 つ も見つからない");
            failures++;
        }

        return failures;
    }

    static bool Resolves(string name)
    {
        try
        {
            return System.Reflection.Assembly.Load(name) != null;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// <b>シーンが開けて、欠けた component が無いか。</b>
    ///
    /// 古い Unity にしか無かった component（GUILayer など）がシーンに
    /// 残っていると、開くたびに Console へ赤が出る ——
    /// <c>Component GUI Layer ... is no longer available.</c>
    /// 動きには障らないが、<b>本物のエラーが埋もれる</b>。
    ///
    /// <b>0 件 を緑にしない。</b> シーンが開けなかったときも
    /// 「欠けた component 0 件」になってしまうので、
    /// GameObject を 1 個 以上 数えられたことも併せて見る。
    /// </summary>
    static int CheckScene()
    {
        var failures = 0;
        try
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[SampleChecks] シーンを開けない: " + ScenePath);
                return failures + 1;
            }

            var objects = 0;
            var missing = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    objects++;
                    foreach (var c in t.GetComponents<Component>())
                    {
                        // 型が引けない component は null で返る。
                        // **Unity の偽 null ではなく本当の null**
                        if (ReferenceEquals(c, null))
                        {
                            missing++;
                            Debug.LogError("[SampleChecks] 欠けた component: " + t.name);
                        }
                    }
                }
            }

            Debug.Log("[SampleChecks] シーン: GameObject " + objects + " 個、欠けた component " + missing + " 個");

            if (objects <= 0)
            {
                Debug.LogError("[SampleChecks] シーンに GameObject が 1 個 も無い。開けていない疑い");
                failures++;
            }
            if (missing > 0)
            {
                failures++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SampleChecks] シーンの検査で落ちた: " + e);
            failures++;
        }

        return failures;
    }
}
