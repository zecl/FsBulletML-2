using System;
using System.Threading.Tasks;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 投げっぱなし にする口。
    /// 捨てると落ちても誰 も気づかない —— 網 が切れた・口 が 404 を返した、が画面 にも log にも 1 行 も出ずに 「なんとなく止まっている」になる。
    /// </summary>
    public static class Forget
    {
        /// <summary>
        /// `ValueTask` に `ContinueWith` は無い。同期で終わっていたら `AsTask` しない。
        /// </summary>
        public static void Fire(this ValueTask task)
        {
            if (task.IsCompletedSuccessfully)
            {
                return;
            }

            task.AsTask().Fire();
        }

        public static void Fire(this Task task)
        {
            if (task == null)
            {
                return;
            }

            // `OnlyOnFaulted` で繋ぐ。 成功 したときに継続 を作らない
            task.ContinueWith(
                static t =>
                {
                    var ex = t.Exception?.GetBaseException();

                    // 解いたのは失敗 ではない。 場面 を抜けるたび に
                    // 赤 が出ると、本物 の失敗 が埋もれる
                    if (ex is OperationCanceledException)
                    {
                        return;
                    }

                    GD.PushError($"[Danmaku] {ex?.Message}");
                },
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
