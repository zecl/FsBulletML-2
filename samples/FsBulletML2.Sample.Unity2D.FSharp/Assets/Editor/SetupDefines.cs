using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// Scripting Define Symbols を Unity 自身に書かせる。
///
/// 走らせ方:
public static class SetupDefines
{
    const string Symbol = "DISABLE_TYPEMANAGER_ILPP";

    public static void Run()
    {
        var failures = 0;
        try
        {
            // Editor で走るコードにも効かせたいので、Editor が使う対象へ入れる
            var targets = new[] { NamedBuildTarget.Standalone };
            foreach (var t in targets)
            {
                var current = PlayerSettings.GetScriptingDefineSymbols(t);
                if (current != null && current.Contains(Symbol))
                {
                    Debug.Log("[SetupDefines] " + t.TargetName + ": 既に入っている");
                    continue;
                }

                var next = string.IsNullOrEmpty(current) ? Symbol : current + ";" + Symbol;
                PlayerSettings.SetScriptingDefineSymbols(t, next);
                Debug.Log("[SetupDefines] " + t.TargetName + ": 足した -> " + next);
            }

            AssetDatabase.SaveAssets();

            // 入ったことを読み返して確かめる。 書いたことと入ったことは別
            var after = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
            Debug.Log("[SetupDefines] いまの define: " + (string.IsNullOrEmpty(after) ? "（空）" : after));
            if (after == null || !after.Contains(Symbol))
            {
                Debug.LogError("[SetupDefines] 書いたのに入っていない");
                failures++;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SetupDefines] 落ちた: " + e);
            failures++;
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
