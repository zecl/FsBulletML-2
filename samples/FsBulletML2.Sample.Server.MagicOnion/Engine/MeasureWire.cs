using System;
using System.Collections.Generic;
using FsBulletML2.Sample.MagicOnion.Shared;
using MessagePack;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 帯域 を締める 3 つ の手 を、同じ走行の上で並べて測る（E1.6）。
    /// </summary>
    public static class MeasureWire
    {
        public static int Run(int frames, int seed, string bulletml)
        {
            var info = Catalog.Resolve(bulletml);
            Console.WriteLine("{0} を {1} コマ（種 {2}）", info.Name, frames, seed);
            Console.WriteLine();

            var source = new EngineFrameSource(info, seed);

            // 走行を 1 度 だけ回して、コマ列 を控える。
            // 3 通り を別々に走らせると、A/B の差 に走行の違い が混ざる
            var run = new BulletDto[frames][];
            for (int f = 0; f < frames; f++)
            {
                run[f] = source.Step().Bullets;
            }

            long bullets = 0;
            foreach (var frame in run)
            {
                bullets += frame.Length;
            }

            Console.WriteLine("弾 の総数 {0}（1 コマ 平均 {1:0.0} 発）", bullets, (double)bullets / frames);
            Console.WriteLine();
            Console.WriteLine("{0,-22} {1,12} {2,12} {3,9} {4,10}", "手", "総バイト", "1 コマ", "弾 1 発", "Mbps");

            long plain = Plain(run, 1);
            Row("素（毎コマ・float）", plain, frames, bullets, 1);

            long half = Plain(run, 2);
            Row("送信レート 1/2", half, frames, bullets, 2);

            long quantized = Quantized(run, 1);
            Row("量子化（毎コマ）", quantized, frames, bullets, 1);

            long both = Quantized(run, 2);
            Row("量子化 ＋ 1/2", both, frames, bullets, 2);

            Console.WriteLine();
            Roundtrip();

            Console.WriteLine();
            Delta(run);

            Console.WriteLine();
            Console.WriteLine("素 を 1 とすると —— 送信レート {0:0.00} / 量子化 {1:0.00} / 両方 {2:0.00}",
                (double)half / plain, (double)quantized / plain, (double)both / plain);

            return 0;
        }

        static void Row(string name, long bytes, int frames, long bullets, int sendEvery)
        {
            int sent = (frames + sendEvery - 1) / sendEvery;
            double seconds = frames / (double)RoomLoopFps;
            Console.WriteLine(
                "{0,-22} {1,12} {2,12:0} {3,9:0.0} {4,10:0.00}",
                name, bytes, (double)bytes / sent, (double)bytes / (bullets / sendEvery), bytes * 8 / seconds / 1_000_000.0);
        }

        const int RoomLoopFps = 60;

        /// <summary>
        /// 量子化 する前 の形。
        /// </summary>
        static long Plain(BulletDto[][] run, int sendEvery)
        {
            long total = 0;
            for (int f = 0; f < run.Length; f += sendEvery)
            {
                var bullets = new FloatBullet[run[f].Length];
                for (int i = 0; i < bullets.Length; i++)
                {
                    var b = run[f][i];
                    bullets[i] = new FloatBullet
                    {
                        Id = b.Id,
                        X = Wire.FromGrid(b.X, Rooms.Field.MinX, Rooms.Field.MaxX),
                        Y = Wire.FromGrid(b.Y, Rooms.Field.MinY, Rooms.Field.MaxY),
                        Dir = Wire.FromDir(b.Dir),
                        Kind = b.Kind,
                    };
                }

                total += MessagePackSerializer.Serialize(
                    new FloatFrame { Frame = f + 1, Bullets = bullets }).Length;
            }

            return total;
        }

        /// <summary>
        /// x / y / 向き を整数 に丸めた形。
        /// </summary>
        static long Quantized(BulletDto[][] run, int sendEvery)
        {
            long total = 0;
            for (int f = 0; f < run.Length; f += sendEvery)
            {
                total += MessagePackSerializer.Serialize(
                    new FrameDto { Frame = f + 1, Bullets = run[f] }).Length;
            }

            return total;
        }

        /// <summary>
        /// 丸めて戻したときに、どれだけずれるかを測る。
        /// 丸め方 が 片側 にずれていてもビルドは通るし落ちもしない —— 弾が半 ピクセル ずれた場所 に出るだけなので、目 でしか分からない。
        /// </summary>
        static void Roundtrip()
        {
            float worstX = 0f, worstY = 0f, worstDir = 0f;
            const int steps = 200000;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                float x = Rooms.Field.MinX + ((Rooms.Field.MaxX - Rooms.Field.MinX) * t);
                float back = Wire.FromGrid(Wire.ToGrid(x, Rooms.Field.MinX, Rooms.Field.MaxX),
                                           Rooms.Field.MinX, Rooms.Field.MaxX);
                worstX = Math.Max(worstX, Math.Abs(back - x));

                float y = Rooms.Field.MinY + ((Rooms.Field.MaxY - Rooms.Field.MinY) * t);
                float backY = Wire.FromGrid(Wire.ToGrid(y, Rooms.Field.MinY, Rooms.Field.MaxY),
                                            Rooms.Field.MinY, Rooms.Field.MaxY);
                worstY = Math.Max(worstY, Math.Abs(backY - y));

                float deg = 360f * t;
                float backDeg = Wire.FromDir(Wire.ToDir(deg));
                // 360 は 0 に畳まれる。そこを誤差 に数えない
                float gap = Math.Abs(backDeg - deg);
                if (gap > 180f) { gap = 360f - gap; }
                worstDir = Math.Max(worstDir, gap);
            }

            Console.WriteLine(
                "丸めて戻したときのずれ（{0} 点）—— x {1:0.000000} / y {2:0.000000} / 向き {3:0.0000} 度",
                steps, worstX, worstY, worstDir);
            Console.WriteLine(
                "  弾 の当たり半径 は {0}。**ずれ は半径 の {1:0.00}%**",
                Rooms.Field.BulletRadius, worstX / Rooms.Field.BulletRadius * 100);
        }

        /// <summary>
        /// 差分 の上界 を測る。
        /// </summary>
        static void Delta(BulletDto[][] run)
        {
            var previous = new Dictionary<int, BulletDto>();
            long same = 0;
            long changed = 0;
            long born = 0;

            foreach (var frame in run)
            {
                foreach (var b in frame)
                {
                    if (!previous.TryGetValue(b.Id, out var old))
                    {
                        born++;
                    }
                    else if (old.X == b.X && old.Y == b.Y && old.Dir == b.Dir)
                    // 量子化 した後 の値 で見る。
                    {
                        same++;
                    }
                    else
                    {
                        changed++;
                    }
                }

                previous.Clear();
                foreach (var b in frame)
                {
                    previous[b.Id] = b;
                }
            }

            long all = same + changed + born;
            Console.WriteLine(
                "差分 の当たる先 —— 動いた {0:0.0}% / 据え置ける {1:0.0}% / 生まれた {2:0.0}%",
                changed * 100.0 / all, same * 100.0 / all, born * 100.0 / all);
            Console.WriteLine(
                "  据え置ける ぶん を丸ごと省けたとしても、減るのは最大 {0:0.0}%", same * 100.0 / all);
        }
    }

    /// <summary>量子化 する前 の形。測るためだけ。配っていない</summary>
    [MessagePackObject]
    public struct FloatBullet
    {
        [Key(0)] public int Id;
        [Key(1)] public float X;
        [Key(2)] public float Y;
        [Key(3)] public float Dir;
        [Key(4)] public byte Kind;
    }

    /// <summary>量子化 する前 の形。測るためだけ。配っていない</summary>
    [MessagePackObject]
    public class FloatFrame
    {
        [Key(0)] public int Frame { get; set; }
        [Key(1)] public FloatBullet[] Bullets { get; set; }
        [Key(2)] public ushort PlayerHits { get; set; }
        [Key(3)] public ushort EnemyHits { get; set; }
    }
}