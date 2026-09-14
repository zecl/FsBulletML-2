using UnityEditor;
using UnityEngine;

/// <summary>
/// batchmode で「焼けること」を確かめる口。
///
/// <b>中身はほとんど無い。それでよい。</b>
/// Unity は script が 1 本 でも焼けなければ <c>-executeMethod</c> を走らせず、
/// 0 以外 を返す。**だから「この 1 行 が出た」＝「全部 焼けた」になる。**
///
///     Unity.exe -batchmode -quit -nographics -projectPath &lt;ここ&gt; ^
///               -logFile &lt;log&gt; -executeMethod CompileCheck.Run
///
/// <b>headless の双子（<c>FsBulletML2.Sample.Unity2D.MagicOnion.Compile</c>）
/// では足りないところを、ここが埋める。</b> 双子 が見ているのは
/// <c>Assets/Scripts/</c> だけで、**<c>Assets/Editor/</c> は 1 本 も通らない**
/// —— 実際に、ここを足すまで <c>EcsCostCheck.cs</c> の壊れが出なかった。
///
/// <b>自分で赤 を数えようとしない。</b> 1 度 <c>CompilationPipeline</c> の
/// 口 で数えようとして、その口 が この版 に無くて**この file 自体が赤 になった**。
/// 数えるのは Unity の仕事。
/// </summary>
public static class CompileCheck
{
    public static void Run()
    {
        Debug.Log("[CompileCheck] 全部 焼けた");
        EditorApplication.Exit(0);
    }
}
