using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Rooms
{
    /// <summary>コマの速さ。<b>部屋ごとではなく、サーバー全体で 1 つ</b></summary>
    public static class RoomLoop
    {
        public const int Fps = 60;
    }

    /// <summary>
    /// 走っている部屋 1 つ。<b>弾幕エンジンを持つのはここだけ。</b>
    ///
    /// <b>輪 を回すのは Hub ではなく、この側。</b> Hub は接続 1 本 の寿命で
    /// 作り直されるので、そこに輪 を置くと「誰かが切れた瞬間にコマが飛ぶ」。
    ///
    /// <b>進めるのは 1 本 の Task だけ。</b> <see cref="IFrameSource"/> の中身は
    /// 排他を持たない（エンジンの実行状態は素の field）ので、
    /// ここが唯一の呼び手であることが不変条件。自機の位置だけは
    /// 別のスレッドから届くので、<see cref="pendingPlayer"/> に置いて
    /// 輪 の中で読む。
    /// </summary>
    public sealed class Room : IAsyncDisposable
    {
        readonly IFrameSource source;
        readonly IGroup<IDanmakuHubReceiver> group;
        readonly ILogger logger;
        readonly CancellationTokenSource stopping = new CancellationTokenSource();
        readonly Task loop;

        /// <summary>いちばん新しい自機の位置。<b>輪 の外から書いて、輪 の中で読む</b></summary>
        long pendingPlayer;
        int hasPlayer;

        /// <summary>
        /// 撃った合図。<b>位置 と違って、いちばん新しい 1 つ では足りない。</b>
        ///
        /// 位置は「いまどこか」なので古いものを捨ててよいが、
        /// 撃つのは**出来事**で、捨てると弾が 1 発 出ない。
        /// 1 コマ に 2 つ 以上 届くことが在るので並べて持つ。
        /// </summary>
        readonly ConcurrentQueue<long> pendingShots = new ConcurrentQueue<long>();

        /// <summary>
        /// 溜める上限。<b>超えたら捨てる。</b>
        /// client が壊れて撃ち続けたときに、部屋 が溺れないようにする
        /// —— **捨てた数は数える**（黙って落とさない）。
        /// </summary>
        const int MaxPendingShots = 32;

        int droppedShots;

        int members;
        int frame;

        /// <summary>いちばん新しいコマの弾数。<b>状況 の行 が読む</b></summary>
        int lastBullets;

        int playerHits;
        int enemyHits;

        public Room(string key, IFrameSource source, IGroup<IDanmakuHubReceiver> group, ILogger logger)
        {
            Key = key;
            this.source = source;
            this.group = group;
            this.logger = logger;
            loop = Task.Run(RunAsync);
        }

        public string Key { get; }

        public RoomInfo Info => source.Info;

        /// <summary>
        /// 状況 の行 に出す名前。<b><see cref="Key"/> をそのまま出さない</b> ——
        /// 弾幕 を名指ししなければ頭 が空 になって <c>#5</c> としか出ない。
        /// 種 は残す（同じ弾幕 の別の部屋 を見分けるため）。
        /// </summary>
        public string Label
        {
            get
            {
                int at = Key.LastIndexOf('#');
                string seed = at < 0 ? "" : Key.Substring(at);
                return source.Info.Name + seed;
            }
        }

        /// <summary>いま何コマ 目 か。<b>抜けを数える側 が読む</b></summary>
        public int Frame => Volatile.Read(ref frame);

        public int Members => Volatile.Read(ref members);

        /// <summary>溜めきれずに捨てた撃ちの数。<b>0 でないなら client が撃ちすぎ</b></summary>
        public int DroppedShots => Volatile.Read(ref droppedShots);

        /// <summary>いちばん新しいコマに載っていた弾の数</summary>
        public int LastBullets => Volatile.Read(ref lastBullets);

        /// <summary>入ってからの被弾 の総数</summary>
        public int PlayerHits => Volatile.Read(ref playerHits);

        /// <summary>入ってからの命中 の総数</summary>
        public int EnemyHits => Volatile.Read(ref enemyHits);

        public void Enter() => Interlocked.Increment(ref members);

        /// <summary>出た人を引く。<b>0 になっても部屋は畳まない</b> ——
        /// 畳むのは <see cref="RoomRegistry"/> の仕事</summary>
        public int Leave() => Interlocked.Decrement(ref members);

        public void SetPlayer(float x, float y)
        {
            Interlocked.Exchange(ref pendingPlayer, Pack(x, y));
            Volatile.Write(ref hasPlayer, 1);
        }

        /// <summary>撃った合図 を溜める。<b>撃つのは輪 の中</b></summary>
        public void Shoot(float x, float y)
        {
            if (pendingShots.Count >= MaxPendingShots)
            {
                Interlocked.Increment(ref droppedShots);
                return;
            }

            pendingShots.Enqueue(Pack(x, y));
        }

        /// <summary>
        /// float 2 本 を 1 つ の long に詰める。<b>1 回 の書きで済ませるため</b> ——
        /// 2 回 に割ると、x だけ新しい組が輪 に読まれうる。
        /// </summary>
        static long Pack(float x, float y)
            => ((long)(uint)BitConverter.SingleToInt32Bits(x) << 32)
               | (uint)BitConverter.SingleToInt32Bits(y);

        static void Unpack(long packed, out float x, out float y)
        {
            x = BitConverter.Int32BitsToSingle((int)(packed >> 32));
            y = BitConverter.Int32BitsToSingle((int)packed);
        }

        async Task RunAsync()
        {
            // **PeriodicTimer は遅れを繰り越さない。** 1 コマ が遅れても
            // 取り返そうとして詰めて撃たない（詰めると弾幕の見え方が変わる）
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1.0 / RoomLoop.Fps));

            try
            {
                while (await timer.WaitForNextTickAsync(stopping.Token).ConfigureAwait(false))
                {
                    if (Volatile.Read(ref hasPlayer) != 0)
                    {
                        Unpack(Interlocked.Read(ref pendingPlayer), out var px, out var py);
                        source.SetPlayer(px, py);
                    }

                    // **撃つのは進める前。** 後 にすると、撃った弾が
                    // このコマで 1 度 も進まないまま並びに出る（1 コマ 止まって見える）
                    int shots = 0;
                    while (shots < MaxPendingShots && pendingShots.TryDequeue(out var packedShot))
                    {
                        Unpack(packedShot, out var sx, out var sy);
                        source.Shoot(sx, sy);
                        shots++;
                    }

                    var snapshot = source.Step();
                    int n = Interlocked.Increment(ref frame);

                    Volatile.Write(ref lastBullets, snapshot.Bullets.Length);
                    if (snapshot.PlayerHits > 0)
                    {
                        Interlocked.Add(ref playerHits, snapshot.PlayerHits);
                    }

                    if (snapshot.EnemyHits > 0)
                    {
                        Interlocked.Add(ref enemyHits, snapshot.EnemyHits);
                    }

                    group.All.OnFrame(new FrameDto
                    {
                        Frame = n,
                        Bullets = snapshot.Bullets,
                        PlayerHits = (ushort)snapshot.PlayerHits,
                        EnemyHits = (ushort)snapshot.EnemyHits,
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // 畳んだ。**これは失敗ではない**
            }
            // **`System.` を省けない。** この repo には `FsBulletML2.Exception` が
            // 在り、ここの名前空間が `FsBulletML2.` 始まりなので、
            // 素 の `Exception` はそちらに当たる（CS0155 で落ちる）
            catch (System.Exception ex)
            {
                // **輪 が落ちたことを黙って飲まない。** 飲むと、client には
                // 「繋がっているのにコマが来ない」としか見えない
                logger.LogError(ex, "部屋 {Key} の輪 が {Frame} コマ 目 で落ちた", Key, Frame);
            }
        }

        public async ValueTask DisposeAsync()
        {
            stopping.Cancel();
            try
            {
                await loop.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            stopping.Dispose();
        }
    }
}
