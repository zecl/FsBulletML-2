using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace FsBulletML2.Sample.Client.MagicOnion
{
    /// <summary>
    /// 数えるだけの client。<b>絵 は出さない。</b>
    ///
    /// <b>これが在る理由は 2 つ。</b>
    /// 1 つ 目 は、Unity を立てずに配線を確かめられること。
    /// 2 つ 目 は、<b>この repo で「F# を 1 本 も参照しない client」が
    /// 実際に焼けることを、門 が数えられる形で置いておく</b>こと ——
    /// Unity 側 は dll を消し忘れても絵 が出てしまうので、目では出ない。
    /// </summary>
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            string host = Arg(args, "--host") ?? "http://localhost:5170";
            string bulletml = Arg(args, "--bulletml") ?? "";
            int seed = int.TryParse(Arg(args, "--seed"), out var s) ? s : 0;
            int frames = int.TryParse(Arg(args, "--frames"), out var f) ? f : 180;
            bool shoot = Array.IndexOf(args, "--shoot") >= 0;

            // **h2c（暗号化しない HTTP/2）を通す。** 既定では http:// の
            // HTTP/2 が閉じていて、繋ぎに行った瞬間に落ちる
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            using var channel = GrpcChannel.ForAddress(host);

            var receiver = new Receiver();
            var hub = await StreamingHubClient
                .ConnectAsync<IDanmakuHub, IDanmakuHubReceiver>(channel, receiver)
                .ConfigureAwait(false);

            try
            {
                var info = await hub.JoinAsync(new JoinRequest { Bulletml = bulletml, Seed = seed })
                    .ConfigureAwait(false);

                Console.WriteLine(
                    "部屋 {0} / {1} コマ毎秒 / 空間 {2} / 盤面 x[{3}, {4}] y[{5}, {6}]",
                    info.Name, info.Fps, info.Space,
                    F(info.MinX), F(info.MaxX), F(info.MinY), F(info.MaxY));

                // 自機 を 1 度 置く。**置かないと aim の狙う先が既定のまま**、
                // かつ**サーバーは被弾 を数えない**（送られていない位置で判定しない）
                float px = info.OriginX;
                float py = info.MinY + 0.5f;

                // **--player で置き場を動かせる。** 当たり判定 が効いているかは、
                // 弾の出どころ へ置いて数が増えることで見る
                var placed = Arg(args, "--player");
                if (placed != null)
                {
                    var xy = placed.Split(',');
                    if (xy.Length == 2
                        && float.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var ax)
                        && float.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var ay))
                    {
                        px = ax;
                        py = ay;
                    }
                }

                Console.WriteLine("自機 ({0}, {1})", F(px), F(py));
                await hub.SetPlayerAsync(px, py).ConfigureAwait(false);

                // **撃ちながら数える。** 自機 の弾もサーバーが作るので、
                // 弾数 が増えることがそのまま「撃てた」の印 になる
                using var shooting = shoot ? StartShooting(hub, px, py) : null;

                await receiver.WaitAsync(frames, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                await hub.LeaveAsync().ConfigureAwait(false);
            }
            finally
            {
                await hub.DisposeAsync().ConfigureAwait(false);
            }

            return receiver.Report(frames);
        }

        /// <summary>
        /// 60 回/秒 で撃ち続ける。<b>Unity の Z 押しっぱなしと同じ間合い。</b>
        /// 待たないで投げる —— 待つと撃つ間隔 が往復 の遅れになる。
        /// </summary>
        static IDisposable StartShooting(IDanmakuHub hub, float x, float y)
        {
            var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        _ = hub.ShootAsync(x, y);
                        await Task.Delay(16, cts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
            return cts;
        }

        static string F(float v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        static string Arg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.Ordinal))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        /// <summary>
        /// 降ってきたコマを数える。
        ///
        /// <b>「何コマ 来たか」だけでは足りない。</b> 番号の飛びと、
        /// 弾数 の山 と中央値 まで出す —— 帯域 を概算するとき
        /// （ロードマップ E1.x の測ること 2）に要るのが後者だから。
        /// </summary>
        sealed class Receiver : IDanmakuHubReceiver
        {
            readonly TaskCompletionSource<bool> done =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            readonly object gate = new object();
            readonly System.Collections.Generic.List<int> counts = new System.Collections.Generic.List<int>();

            int want;
            int last = -1;
            int gaps;
            int playerHits;
            int enemyHits;

            public void OnFrame(FrameDto frame)
            {
                lock (gate)
                {
                    // **番号の飛びを数える。** 落ちたコマは「遅い」ではなく
                    // 「来ていない」ので、遅さとは別に見えないといけない
                    if (last >= 0 && frame.Frame != last + 1)
                    {
                        gaps += frame.Frame - last - 1;
                    }

                    last = frame.Frame;
                    counts.Add(frame.Bullets?.Length ?? 0);

                    // **当たりはサーバーが数えて降ろす。** client は判定を持たない
                    playerHits += frame.PlayerHits;
                    enemyHits += frame.EnemyHits;

                    if (want > 0 && counts.Count >= want)
                    {
                        done.TrySetResult(true);
                    }
                }
            }

            public async Task WaitAsync(int frames, TimeSpan timeout)
            {
                lock (gate)
                {
                    want = frames;
                    if (counts.Count >= want)
                    {
                        done.TrySetResult(true);
                    }
                }

                var finished = await Task.WhenAny(done.Task, Task.Delay(timeout)).ConfigureAwait(false);
                if (finished != done.Task)
                {
                    Console.Error.WriteLine("{0} 秒 待って {1} コマ しか来なかった", timeout.TotalSeconds, Count);
                }
            }

            int Count
            {
                get
                {
                    lock (gate)
                    {
                        return counts.Count;
                    }
                }
            }

            /// <summary>数を出す。<b>足りなければ 1 を返す</b>（門 が読む）</summary>
            public int Report(int want)
            {
                int[] snapshot;
                int missed;
                lock (gate)
                {
                    snapshot = counts.ToArray();
                    missed = gaps;
                }

                if (snapshot.Length == 0)
                {
                    Console.Error.WriteLine("1 コマ も来なかった");
                    return 1;
                }

                var sorted = (int[])snapshot.Clone();
                Array.Sort(sorted);

                long sum = 0;
                foreach (var c in snapshot)
                {
                    sum += c;
                }

                Console.WriteLine(
                    "{0} コマ 受けた（飛び {1}）。弾数 中央値 {2} / 山 {3} / 平均 {4:0.0}",
                    snapshot.Length, missed,
                    sorted[sorted.Length / 2], sorted[sorted.Length - 1],
                    (double)sum / snapshot.Length);

                int hitPlayer;
                int hitEnemy;
                lock (gate)
                {
                    hitPlayer = playerHits;
                    hitEnemy = enemyHits;
                }

                Console.WriteLine("当たり 敵へ {0} 発 / 自機へ {1} 発", hitEnemy, hitPlayer);

                // **弾 1 発 11 バイト の概算**（ロードマップの但し書きと同じ仮定）。
                // これは概算であって測定ではない
                double bytesPerSec = (double)sum / snapshot.Length * 11 * 60;
                Console.WriteLine(
                    "概算 {0:0.0} KB/秒 = {1:0.00} Mbps（弾 1 発 11 バイト・60 コマ毎秒 と仮定）",
                    bytesPerSec / 1024, bytesPerSec * 8 / 1_000_000);

                if (snapshot.Length < want)
                {
                    Console.Error.WriteLine("{0} コマ 欲しかったが {1} コマ", want, snapshot.Length);
                    return 1;
                }

                return 0;
            }
        }
    }
}
