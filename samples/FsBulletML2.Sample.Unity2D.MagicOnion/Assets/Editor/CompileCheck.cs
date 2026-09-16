using UnityEditor;
using UnityEngine;

/// <summary>
/// batchmode で「焼けること」を確かめる口。
///
/// 中身はほとんど無い。それでよい。
/// Unity は script が 1 本 でも焼けなければ <c>-executeMethod</c> を走らせず、
/// 0 以外 を返す。だから「この 1 行 が出た」＝「全部 焼けた」になる。
///     Unity.exe -batchmode -quit -nographics -projectPath &lt;ここ&gt; ^
///               -logFile &lt;log&gt; -executeMethod CompileCheck.Run
/// headless の双子（<c>FsBulletML2.Sample.Unity2D.MagicOnion.Compile</c>）
public static class CompileCheck
{
    public static void Run()
    {
        Debug.Log("[CompileCheck] 全部 焼けた");
        EditorApplication.Exit(0);
    }
}
