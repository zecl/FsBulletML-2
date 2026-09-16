using System.Threading;
using FsBulletML2.Sample.MagicOnion.Shared;
using MessagePack;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// 配ったコマの実際の長さを数える。
    ///
    /// なぜ要るか。 状況 の行 はこれまで「弾 1 発 20.0 バイト」の
    /// 概算 で Mbps を出していた。締めた効果 を測るには足りない ——
    /// 量子化 は 1 発 の大きさ を動かすので、固定の係数 を掛けている限り
    /// 数字 が 1 ビット も動かない。
    public sealed class WireMeter
    {
        long bytes;
        long frames;

        public WireMeter(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }

        public void Add(FrameDto frame)
        {
            if (!Enabled)
            {
                return;
            }

            // 配るのと同じ形 で焼く。 DTO を別の形で数えると、
            // 属性（Key の並び）の効き が測れない
            int len = MessagePackSerializer.Serialize(frame).Length;
            Interlocked.Add(ref bytes, len);
            Interlocked.Increment(ref frames);
        }

        /// <summary>数を取り出して 0 に戻す</summary>
        public void Drain(out long drainedBytes, out long drainedFrames)
        {
            drainedBytes = Interlocked.Exchange(ref bytes, 0);
            drainedFrames = Interlocked.Exchange(ref frames, 0);
        }
    }
}