using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 門をまとめて 1 回 で回す。
///
/// 走らせ方:
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
    /// Assets に置いた dll が、参照している相手を全部 見つけられるか。
    ///
    /// このサンプルは F# を Unity の外でビルドして dll を Assets へ手で置く。
    static int CheckAssemblies()
    {
        var failures = 0;

        // 目盛り合わせ。これが先（当てにならない道具で測っても意味がない）
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

        // 0 件 を緑にしない。 dll が 1 つ も読み込まれていなければ
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
    /// シーンが開けて、欠けた component が無いか。
    ///
    /// 古い Unity にしか無かった component（GUILayer など）がシーンに
    /// 残っていると、開くたびに Console へ赤が出る ——
    /// <c>Component GUI Layer ... is no longer available.</c>
    /// 動きには障らないが、本物のエラーが埋もれる。
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
                        // Unity の偽 null ではなく本当の null
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
