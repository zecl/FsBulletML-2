using System.Threading;
using FsBulletML2.Sample.MagicOnion.Shared;
using MessagePack;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// 配ったコマの<b>実際の長さ</b>を数える。
    ///
    /// <b>なぜ要るか。</b> 状況 の行 はこれまで「弾 1 発 20.0 バイト」の
    /// 概算 で Mbps を出していた。**締めた効果 を測るには足りない** ——
    /// 量子化 は 1 発 の大きさ を動かすので、固定の係数 を掛けている限り
    /// 数字 が 1 ビット も動かない。
    ///
    /// <b>何を数えていないか。</b> ここで測るのは MessagePack に焼いた
    /// 本文 の長さ だけ。gRPC の枠 と HTTP/2 の頭 は入っていない。
    /// **3 つ の手 を突き合わせるための物差し**なので、
    /// 同じものを同じやり方で数えていれば足りる ——
    /// 「網 に流れた総量」としては読まないこと。
    ///
    /// <b>既定では動かない。</b> 1 コマ につき もう 1 回 焼くので、
    /// 3,373 発 の弾幕では 60 回/秒 ぶん の確保 が増える。
    /// <c>--measure-bytes</c> で入る。
    /// </summary>
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

            // **配るのと同じ形 で焼く。** DTO を別の形で数えると、
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