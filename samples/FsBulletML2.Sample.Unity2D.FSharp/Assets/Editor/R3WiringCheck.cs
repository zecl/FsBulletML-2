using UnityEditor;
using UnityEngine;
// **Subscribe(Action&lt;T&gt;) は拡張メソッド。** これが無いと
// Subscribe(Observer&lt;T&gt;) に解決されて CS1660 になる
using R3;
using FsBulletML2.Sample.Unity2D.FSharp;

/// <summary>
/// <b>R3 の配線が生きているかを、Play せずに見る。</b>
///
/// 走らせ方:
/// <code>
/// Unity.exe -batchmode -quit -nographics -projectPath &lt;proj&gt; -logFile &lt;log&gt; ^
///           -executeMethod R3WiringCheck.Run
/// </code>
///
/// <b>なぜ要るか。</b> このサンプルの毎コマ の仕事は、R3 化で
/// <c>Update()</c> から <c>FrameTicker.Frames</c> の購読へ移った。
/// <b>購読は「書いた」だけでは動かない</b> —— 発火源が居るか、
/// 流れが届くか、切ったら止まるか、はどれも実行時にしか分からず、
/// <b>どれが欠けてもコンパイルは通る</b>。
///
/// ここが見るのは 3 つ。
/// <list type="number">
///   <item>発火源（FrameTicker）が Observable を配れる</item>
///   <item>Update を 1 回 回すと購読へ 1 個 届く</item>
///   <item>購読を切ると届かなくなる</item>
/// </list>
///
/// <b>Play は要らない。</b> <c>FrameTicker.Update</c> は普通のメソッドなので
/// 手で呼べる。Unity の PlayerLoop に頼っていないのが、ここで効いている
/// （R3.Unity の <c>Observable.EveryUpdate</c> だったら、この門は書けない）。
/// </summary>
public static class R3WiringCheck
{
    public static void Run()
    {
        var failures = 0;
        GameObject go = null;
        try
        {
            go = new GameObject("FrameTickerCheck");
            var ticker = go.AddComponent<FrameTicker>();

            // 1. 配れるか。**シーンに居れば Play 中でなくても配る**設計
            var frames = FrameTicker.Frames;
            if (frames == null)
            {
                Debug.LogError("[R3WiringCheck] Frames が null");
                failures++;
                return;
            }

            var received = 0;
            var subscription = frames.Subscribe(_ => received++);

            // 2. 回した数だけ届くか
            ticker.Update();
            ticker.Update();
            ticker.Update();
            if (received != 3)
            {
                Debug.LogError("[R3WiringCheck] 3 回 回したのに届いたのは " + received + " 個");
                failures++;
            }
            else
            {
                Debug.Log("[R3WiringCheck] 3 回 回して 3 個 届いた");
            }

            // 3. 切ったら止まるか。**ここを見ないと、切り忘れに気づけない**
            subscription.Dispose();
            var beforeStop = received;
            ticker.Update();
            if (received != beforeStop)
            {
                Debug.LogError("[R3WiringCheck] 切ったのに届いた（" + beforeStop + " -> " + received + "）");
                failures++;
            }
            else
            {
                Debug.Log("[R3WiringCheck] 切ったあとは届かない");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[R3WiringCheck] 落ちた: " + e);
            failures++;
        }
        finally
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        LastFailures = failures;
        if (Application.isBatchMode && !SuppressExit)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }

    /// <summary>
    /// <b>集約門から呼ぶときは Exit を抑える。</b>
    /// <see cref="EditorApplication.Exit"/> はプロセスを即座に終わらせるので、
    /// 抑えないと <b>2 本 目 以降 が 1 度 も走らない</b>。
    /// 見よ <see cref="SampleChecks"/>。
    /// </summary>
    public static bool SuppressExit;

    /// <summary>直前の走行で見つかった食い違いの数</summary>
    public static int LastFailures;
}
