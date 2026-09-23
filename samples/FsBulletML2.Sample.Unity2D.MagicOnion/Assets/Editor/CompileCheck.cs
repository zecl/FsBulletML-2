using UnityEditor;
using UnityEngine;

/// <summary>
/// batchmode で「焼けること」を確かめる口。
/// </summary>
public static class CompileCheck
{
    public static void Run()
    {
        Debug.Log("[CompileCheck] 全部 焼けた");
        EditorApplication.Exit(0);
    }
}
