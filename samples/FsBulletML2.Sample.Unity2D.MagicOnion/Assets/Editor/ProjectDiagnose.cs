using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity が何を見えていて、何が見えていないかを出す。
///
/// 「Project ウィンドウにシーンが出てこない」を追うための口。ファイルは
/// 在るのに Unity が見ていない、という状態は外から区別できないので、
/// AssetDatabase に訊く。
public static class ProjectDiagnose
{
    public static void Run()
    {
        Debug.Log("[Diagnose] dataPath = " + Application.dataPath);

        // 1. AssetDatabase が見ているシーン
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        Debug.Log("[Diagnose] t:Scene が " + sceneGuids.Length + " 件");
        foreach (var g in sceneGuids)
        {
            Debug.Log("[Diagnose]   " + g + "  " + AssetDatabase.GUIDToAssetPath(g));
        }

        // 2. 名指しで引けるか。FindAssets が 0 でも、パス指定で引けることがある
        const string ScenePath = "Assets/Senes/FsBulletML2.Sample.Unity2D.unity";
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("[Diagnose] LoadAssetAtPath(" + ScenePath + ") = "
                  + (asset == null ? "null（Unity から見えていない）" : asset.name));
        Debug.Log("[Diagnose] そのパスの GUID = '" + AssetDatabase.AssetPathToGUID(ScenePath) + "'");

        // 3. Assets 直下に何が見えているか
        var all = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/"))
            .ToArray();
        Debug.Log("[Diagnose] Assets 配下で AssetDatabase が見ているもの " + all.Length + " 件");
        foreach (var p in all.Where(p => p.Split('/').Length <= 2).OrderBy(p => p))
        {
            Debug.Log("[Diagnose]   " + p);
        }

        // 4. Senes の中身
        foreach (var p in all.Where(p => p.StartsWith("Assets/Senes")).OrderBy(p => p))
        {
            Debug.Log("[Diagnose]   Senes: " + p);
        }

        // 5. 実際に開けるか。「見えている」と「開ける」は別
        try
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[Diagnose] OpenScene 成功: name=" + scene.name
                      + " rootCount=" + scene.rootCount
                      + " isLoaded=" + scene.isLoaded);
            foreach (var go in scene.GetRootGameObjects())
            {
                Debug.Log("[Diagnose]   root: " + go.name);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Diagnose] OpenScene が落ちた: " + e.Message);
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }
}
