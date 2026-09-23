using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Logging;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Rooms
{
    /// <summary>コマの速さ。部屋ごとではなく、サーバー全体で 1 つ</summary>
    public static class RoomLoop
    {
        public const int Fps = 60;
    }

    /// <summary>
    /// 部屋 の回し方。サーバー全体で 1 つ（singleton）。
    /// </summary>
    public sealed class RoomOptions
    {
        /// <summary>
        /// 何コマ に 1 回 配るか。
        /// </summary>
        public int SendEvery { get; set; } = 1;
    }

    /// <summary>
    /// 走っている部屋 1 つ。
    /// Hub は接続 1 本 の寿命で 作り直されるので、そこに輪 を置くと「誰かが切れた瞬間にコマが飛ぶ」。
    /// </summary>
    public sealed class Room : IAsyncDisposable
    {
        readonly IFrameSource source;
        readonly IGroup<IDanmakuHubReceiver> group;
        readonly ILogger logger;
        readonly WireMeter meter;
        readonly int sendEvery;

        /// <summary>
        /// あと何コマ で配るか。
        /// <c>frame % N == 0</c> は「その数列を必ず全部 通る」前提 で、 1 つ でも飛ぶと二度と踏まない（この repo で 1 度 踏んでいる）。
        /// </summary>
        int untilSend;
        readonly CancellationTokenSource stopping = new CancellationTokenSource();
        readonly Task loop;

        /// <summary>いちばん新しい自機の位置。輪 の外から書いて、輪 の中で読む</summary>
        long pendingPlayer;
        int hasPlayer;

        /// <summary>
        /// 撃った合図。
        /// 位置は「いまどこか」なので古いものを捨ててよいが、 撃つのは出来事で、捨てると弾が 1 発 出ない。
        /// </summary>
        readonly ConcurrentQueue<long> pendingShots = new ConcurrentQueue<long>();

        /// <summary>超えたら捨てる。捨てた数は数える。黙って落とさない。</summary>
        const int MaxPendingShots = 32;

        int droppedShots;

        int members;
        int frame;

        /// <summary>いちばん新しいコマの弾数。状況 の行 が読む</summary>
        int lastBullets;

        int playerHits;
        int enemyHits;

        /// <summary>配らなかったコマ の当たり。次に配るコマ に載せる</summary>
        int pendingPlayerHits;
        int pendingEnemyHits;

        public Room(
            string key,
            IFrameSource source,
            IGroup<IDanmakuHubReceiver> group,
            ILogger logger,
            WireMeter meter,
            RoomOptions options)
        {
            Key = key;
            this.source = source;
            this.group = group;
            this.logger = logger;
            this.meter = meter;
            sendEvery = Math.Max(1, options?.SendEvery ?? 1);
            untilSend = 1;

            // 配る間隔 を client へ知らせる。 知らせないと、間引いたぶんを
            // 全部「落ちた」と数えられる（コマ番号 は時刻 のままなので飛ぶ）
            source.Info.SendEvery = sendEvery;
            loop = Task.Run(RunAsync);
        }

        public string Key { get; }

        public RoomInfo Info => source.Info;

        /// <summary>
        /// 状況 の行 に出す名前。
        /// <see cref="Key"/> をそのまま出さない —— 弾幕 を名指ししなければ頭 が空 になって <c>#5</c> としか出ない。
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

        /// <summary>いま何コマ 目 か。抜けを数える側 が読む</summary>
        public int Frame => Volatile.Read(ref frame);

        public int Members => Volatile.Read(ref members);

        /// <summary>溜めきれずに捨てた撃ちの数。0 でないなら client が撃ちすぎ</summary>
        public int DroppedShots => Volatile.Read(ref droppedShots);

        /// <summary>いちばん新しいコマに載っていた弾の数</summary>
        public int LastBullets => Volatile.Read(ref lastBullets);

        /// <summary>入ってからの被弾 の総数</summary>
        public int PlayerHits => Volatile.Read(ref playerHits);

        /// <summary>入ってからの命中 の総数</summary>
        public int EnemyHits => Volatile.Read(ref enemyHits);

        public void Enter() => Interlocked.Increment(ref members);

        /// <summary>出た人を引く。0 になっても部屋は畳まない ——
        /// 畳むのは <see cref="RoomRegistry"/> の仕事</summary>
        public int Leave() => Interlocked.Decrement(ref members);

        public void SetPlayer(float x, float y)
        {
            Interlocked.Exchange(ref pendingPlayer, Pack(x, y));
            Volatile.Write(ref hasPlayer, 1);
        }

        /// <summary>撃った合図 を溜める。撃つのは輪 の中</summary>
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
        /// float 2 本 を 1 つ の long に詰める。1 回 の書きで済ませるため ——
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
            // PeriodicTimer は遅れを繰り越さない。 1 コマ が遅れても
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

                    // 撃つのは進める前。 後 にすると、撃った弾が
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

                    // 当たりは間引きで消さない。
                    // 配らないコマの当たりを 捨てると、その 1 発 が無かったことになる（client 側 で 遅れたコマを落とすときと同じ分かれ目）。
                    pendingPlayerHits += snapshot.PlayerHits;
                    pendingEnemyHits += snapshot.EnemyHits;

                    if (--untilSend > 0)
                    {
                        continue;
                    }

                    untilSend = sendEvery;

                    var dto = new FrameDto
                    {
                        Frame = n,
                        Bullets = snapshot.Bullets,
                        PlayerHits = (ushort)Math.Min(pendingPlayerHits, ushort.MaxValue),
                        EnemyHits = (ushort)Math.Min(pendingEnemyHits, ushort.MaxValue),
                    };

                    pendingPlayerHits = 0;
                    pendingEnemyHits = 0;

                    meter.Add(dto);
                    group.All.OnFrame(dto);
                }
            }
            catch (OperationCanceledException)
            {
                // 畳んだ。これは失敗ではない
            }
            // `System.` を省けない。
            // この repo には `FsBulletML2.Exception` が 在り、ここの名前空間が `FsBulletML2.` 始まりなので、 素 の `Exception` はそちらに当たる（CS0155 で落ちる）
            catch (System.Exception ex)
            {
                // 輪 が落ちたことを黙って飲まない。 飲むと、client には
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
