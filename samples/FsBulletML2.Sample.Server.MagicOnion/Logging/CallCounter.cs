using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// 口 が何回 呼ばれたかを数える。サーバーに 1 つ（singleton）。
    ///
    /// なぜ 1 行 ずつ出さないか。 <c>SetPlayerAsync</c> は毎コマ、
    /// <c>ShootAsync</c> は押しっぱなしで毎コマ 来る。2 本 で 120 行/秒。
    /// 1 行 ずつ出すと、見たい「入った・出た・落ちた」が流れて消える。
    /// だから高頻度の口 は数えて、状況 の行 に <c>毎秒 何回</c> で混ぜる。
    /// </summary>
    public sealed class CallCounter
    {
        readonly ConcurrentDictionary<string, Box> counts =
            new ConcurrentDictionary<string, Box>(StringComparer.Ordinal);

        public void Hit(string method)
            => Interlocked.Increment(ref counts.GetOrAdd(method, _ => new Box()).Value);

        /// <summary>数を取り出して 0 に戻す。読むのは状況 の行 だけ</summary>
        public List<KeyValuePair<string, int>> Drain()
        {
            var drained = new List<KeyValuePair<string, int>>(counts.Count);
            foreach (var pair in counts)
            {
                int n = Interlocked.Exchange(ref pair.Value.Value, 0);
                if (n > 0)
                {
                    drained.Add(new KeyValuePair<string, int>(pair.Key, n));
                }
            }

            drained.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return drained;
        }

        sealed class Box
        {
            public int Value;
        }
    }

    /// <summary>
    /// いま何本 繋がっているか。部屋 に入っていない接続も数える ——
    /// 繋がったのに入ってこない形（口 の食い違い）は、
    /// ここと部屋 の人数 が食い違うことで見える。
    /// </summary>
    public sealed class ConnectionCounter
    {
        int live;
        int total;

        public int Live => Volatile.Read(ref live);

        /// <summary>立ち上げてから繋がった延べ本数</summary>
        public int Total => Volatile.Read(ref total);

        public int Connected()
        {
            Interlocked.Increment(ref total);
            return Interlocked.Increment(ref live);
        }

        public int Disconnected() => Interlocked.Decrement(ref live);
    }

    /// <summary>アクセスログ の出し方</summary>
    public sealed class AccessLogOptions
    {
        /// <summary>
        /// 高頻度の口 も 1 行 ずつ出す。既定は false。
        /// <c>--all-calls</c> で true になる
        /// </summary>
        public bool AllCalls { get; set; }

        /// <summary>状況 の行 の間隔。0 なら出さない</summary>
        public TimeSpan StatusInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>数えるだけにする口。1 行 ずつ出すと流れて消えるもの</summary>
        public static bool IsNoisy(string method)
            => method == "SetPlayerAsync" || method == "ShootAsync";
    }
}