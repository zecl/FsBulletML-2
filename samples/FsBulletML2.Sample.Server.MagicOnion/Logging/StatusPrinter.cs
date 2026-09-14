using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Logging
{
    /// <summary>
    /// いま何が起きているかを 1 秒 に 1 行 出す。
    ///
    /// <b>配れているかは、ここでしか見えない。</b> 降りるほう（<c>OnFrame</c>）は
    /// filter を通らないので、アクセスログ には 1 行 も出ない ——
    /// 「繋がっているのに絵 が動かない」を割るのはこの行 の <c>コマ</c> の伸び。
    ///
    /// <b>静かなときは黙る。</b> 誰も居ないのに毎秒 1 行 出すと、
    /// 出入り の行 がすぐ画面の外 へ流れる。
    /// </summary>
    public sealed class StatusPrinter : BackgroundService
    {
        readonly RoomRegistry registry;
        readonly CallCounter counter;
        readonly ConnectionCounter connections;
        readonly AccessLogOptions options;
        readonly ILogger logger;

        public StatusPrinter(
            RoomRegistry registry,
            CallCounter counter,
            ConnectionCounter connections,
            AccessLogOptions options,
            ILoggerFactory loggers)
        {
            this.registry = registry;
            this.counter = counter;
            this.connections = connections;
            this.options = options;
            logger = loggers.CreateLogger("stat");
        }

        protected override async Task ExecuteAsync(CancellationToken stopping)
        {
            if (options.StatusInterval <= TimeSpan.Zero)
            {
                return;
            }

            using var timer = new PeriodicTimer(options.StatusInterval);
            int lastFrame = 0;

            try
            {
                while (await timer.WaitForNextTickAsync(stopping).ConfigureAwait(false))
                {
                    var rooms = await registry.SnapshotAsync().ConfigureAwait(false);
                    var calls = counter.Drain();

                    // **誰も居ないなら黙る**（出入り の行 を流さない）
                    if (rooms.Length == 0 && calls.Count == 0 && connections.Live == 0)
                    {
                        lastFrame = 0;
                        continue;
                    }

                    var line = new StringBuilder();
                    line.Append("接続 ").Append(connections.Live);
                    line.Append(" / 部屋 ").Append(rooms.Length);

                    int frames = 0;
                    foreach (var room in rooms)
                    {
                        frames += room.Frame;
                        line.Append("  |  ").Append(room.Label);
                        line.Append(" 人 ").Append(room.Members);
                        line.Append(" コマ ").Append(room.Frame);
                        line.Append(" 弾 ").Append(room.LastBullets);
                        line.Append(" 当たり ").Append(room.EnemyHits).Append('/').Append(room.PlayerHits);

                        if (room.DroppedShots > 0)
                        {
                            line.Append(" 撃ち捨て ").Append(room.DroppedShots);
                        }

                        // **弾 1 発 20.0 バイト は実測**（MessagePack で焼いた長さ）。
                        // ただしこの行 に出るのは概算 —— 実際に流れた量 ではない
                        double mbps = room.LastBullets * 20.0 * room.Info.Fps * 8 / 1_000_000.0;
                        line.Append(" 概算 ").Append(mbps.ToString("0.0")).Append("Mbps");
                    }

                    if (calls.Count > 0)
                    {
                        line.Append("  |  呼び");
                        double seconds = options.StatusInterval.TotalSeconds;
                        foreach (var call in calls)
                        {
                            line.Append(' ').Append(Short(call.Key));
                            line.Append(' ').Append((call.Value / seconds).ToString("0")).Append("/s");
                        }
                    }

                    // **コマ が伸びていないことを黙って見逃さない。**
                    // 人 が居るのに止まっているなら、輪 が落ちている
                    if (rooms.Length > 0 && frames == lastFrame)
                    {
                        line.Append("  ← コマ が進んでいない");
                    }

                    lastFrame = frames;
                    logger.LogInformation("{Line}", line.ToString());
                }
            }
            catch (OperationCanceledException)
            {
                // 畳んだ。**これは失敗ではない**
            }
        }

        static string Short(string method)
        {
            int at = method.LastIndexOf('/');
            string name = at < 0 ? method : method.Substring(at + 1);
            return name.EndsWith("Async", StringComparison.Ordinal)
                ? name.Substring(0, name.Length - 5)
                : name;
        }
    }
}