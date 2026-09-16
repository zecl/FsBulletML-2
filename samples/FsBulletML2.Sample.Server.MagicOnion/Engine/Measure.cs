using System;
using System.Globalization;
using System.Linq;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 同梱弾幕 を 1 本 ずつ走らせて、1 コマ あたりの弾数を数える。
    ///
    /// 網 を通さない。 通すと 176 本 × 実時間 で 15 分 かかるうえ、
    /// 測っているものに送信の値段 が混ざる。ここで欲しいのは
    /// 「弾が何発 出るか」だけなので、<see cref="EngineFrameSource"/> を
    /// その場で回す。
    public static class Measure
    {
        public static int Run(int frames, int seed, int top)
        {
            var names = Catalog.Names();
            Console.WriteLine("同梱 {0} 本 を {1} コマ ずつ（種 {2}）", names.Length, frames, seed);

            var rows = new (string Name, int Max, int Median, double Mean)[names.Length];

            for (int i = 0; i < names.Length; i++)
            {
                var info = Catalog.At(i);
                var source = new EngineFrameSource(info, seed);

                var counts = new int[frames];
                for (int f = 0; f < frames; f++)
                {
                    counts[f] = source.Step().Bullets.Length;
                }

                var sorted = (int[])counts.Clone();
                Array.Sort(sorted);

                rows[i] = (info.Name, sorted[frames - 1], sorted[frames / 2], counts.Average());
            }

            // 山 で並べる。 帯域 の天井 を決めるのは山 のほう
            foreach (var r in rows.OrderByDescending(x => x.Max).Take(top))
            {
                Console.WriteLine(
                    "  山 {0,5}  中央値 {1,5}  平均 {2,7:0.0}   {3}",
                    r.Max, r.Median, r.Mean, r.Name);
            }

            int worst = rows.Max(x => x.Max);
            double medianOfMedians = rows.Select(x => x.Median).OrderBy(x => x).ElementAt(rows.Length / 2);

            Console.WriteLine();
            Console.WriteLine("いちばん多い弾幕の山 {0} 発 / 弾幕ごとの中央値 の中央値 {1} 発",
                worst, medianOfMedians.ToString(CultureInfo.InvariantCulture));

            // 概算。仮定 を一緒に出す（これだけ見て帯域 を語らせない）
            foreach (var bytes in new[] { 11, 20 })
            {
                Console.WriteLine(
                    "  弾 1 発 {0} バイト と仮定: 山 {1:0.0} KB/秒 = {2:0.00} Mbps / 中央値 {3:0.0} KB/秒",
                    bytes,
                    worst * bytes * 60 / 1024.0,
                    worst * bytes * 60 * 8 / 1_000_000.0,
                    medianOfMedians * bytes * 60 / 1024.0);
            }

            // 仮定を実測に置き換える。 上 は「弾 1 発 ◯ バイト」を掛けただけで、
            // 焼いた長さ ではない。いちばん重い 1 本 を焼き直して数える
            var heaviest = rows.OrderByDescending(x => x.Max).First();
            MeasureBytes(heaviest.Name, frames, seed);

            return 0;
        }

        /// <summary>
        /// 1 コマ ぶんを MessagePack で焼いて、長さ を数える。
        ///
        /// MagicOnion が実際に流す長さ とは少しずれる（Hub の
        /// 呼び出し 1 件 ぶんの包み が外 に付く）。ここで見たいのは
        /// 「弾 1 発 が何バイト で乗るか」なので、包み は勘定に入れない。
        /// </summary>
        static void MeasureBytes(string name, int frames, int seed)
        {
            var source = new EngineFrameSource(Catalog.Resolve(name), seed);

            int peakBullets = 0;
            int peakBytes = 0;
            long sumBytes = 0;
            long sumBullets = 0;

            for (int f = 0; f < frames; f++)
            {
                var bullets = source.Step().Bullets;
                var dto = new FsBulletML2.Sample.MagicOnion.Shared.FrameDto { Frame = f + 1, Bullets = bullets };
                int len = MessagePack.MessagePackSerializer.Serialize(dto).Length;

                sumBytes += len;
                sumBullets += bullets.Length;

                if (bullets.Length > peakBullets)
                {
                    peakBullets = bullets.Length;
                    peakBytes = len;
                }
            }

            Console.WriteLine();
            Console.WriteLine("焼いた長さ（実測）—— {0}", name);
            Console.WriteLine(
                "  山 の 1 コマ {0} 発 {1} バイト（弾 1 発 {2:0.0} バイト）",
                peakBullets, peakBytes, (double)peakBytes / peakBullets);
            Console.WriteLine(
                "  {0} コマ 通しで 弾 1 発 {1:0.0} バイト。山 を 60 コマ毎秒 で流すと {2:0.0} KB/秒 = {3:0.00} Mbps",
                frames,
                (double)sumBytes / sumBullets,
                peakBytes * 60 / 1024.0,
                peakBytes * 60 * 8 / 1_000_000.0);
        }
    }
}
