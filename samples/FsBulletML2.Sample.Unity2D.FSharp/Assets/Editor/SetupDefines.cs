using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>
/// <b>Scripting Define Symbols を Unity 自身に書かせる。</b>
///
/// 走らせ方:
/// <code>
/// Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; -logFile &lt;log&gt; ^
///           -executeMethod SetupDefines.Run
/// </code>
///
/// <b>ProjectSettings.asset を手で書き換えないこと。</b>
/// 一度 やって YAML を壊し、Unity が起動しなくなった（ログ 22 行 で終了）。
/// あの形は map で、インデントも版で変わりうる。**書ける側に書かせる。**
///
/// --- なぜ DISABLE_TYPEMANAGER_ILPP が要るか
///
/// Entities は ILPostProcessor が各アセンブリに
/// <c>Unity.Entities.CodeGeneratedRegistry.AssemblyTypeRegistry</c> を埋め込み、
/// TypeManager がそれを集めて component の型を登録する。
/// <b>その加工は Unity がコンパイルしたアセンブリにしか掛からない。</b>
/// このサンプルは F# を外でビルドして dll を Assets へ置くので掛からず、こうなる ——
///
///     ArgumentException: Unknown Type: ...BulletSim
///
/// この define を入れると TypeManager はリフレクションで全アセンブリを走査する
/// 形に変わり、<b>Unity.Entities を参照している dll なら拾われる。</b>
///
/// 代償は起動時の走査。サンプルなので許容している。
///
/// <b>効いたかは <see cref="EcsEntityCheck"/> が見る。</b>
/// この設定を入れても入れなくても、コンパイルは通ってしまう。
/// </summary>
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

            // **入ったことを読み返して確かめる。** 書いたことと入ったことは別
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
