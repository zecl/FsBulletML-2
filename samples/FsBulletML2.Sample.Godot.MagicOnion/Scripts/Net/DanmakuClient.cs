using System;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// サーバーと繋がっている 1 本。この面 が、この sample で唯一 網 を知る。
    ///
    /// 外 へ出すのは <see cref="DanmakuState"/> だけ。
    /// 場面 の側 は この Node を 1 度 も探さない —— 購読していれば、
    /// 繋がった瞬間 に値 が流れてくる。
    public sealed partial class DanmakuClient : Node
    {
        /// <summary>h2c。証明書 は要らない</summary>
        public string Host { get; set; } = "http://127.0.0.1:5170";

        /// <summary>入る部屋 の弾幕。空 なら サーバーの既定</summary>
        public string Bulletml { get; set; } = "";

        /// <summary>乱数 の種。0 なら サーバーが時計 から採る</summary>
        public int Seed { get; set; }

        /// <summary>自機 の位置 を、何コマ に 1 回 送るか</summary>
        public int PlayerSendInterval { get; set; } = 1;

        /// <summary>自機 の居場所 を返す。場面 が差す（無ければ送らない）</summary>
        public Func<Vector2> PlayerPosition { get; set; }

        readonly Receiver receiver = new();
        CancellationTokenSource life;
        GrpcChannel channel;
        IDanmakuHub hub;
        int sendCountdown;
        int switching;

        public RoomInfo Room => DanmakuState.Room.Value;

        public bool Connected => hub != null && Room != null;

        public override void _Ready()
        {
            // 走行 をまたいで残る値 を落とす。 static なので、エディタ で
            // 再生 し直すと前 の走行 の数 が見えたまま になる
            DanmakuState.Reset();

            life = new CancellationTokenSource();
            ConnectAsync(life.Token).Fire();
        }

        /// <summary>受けたコマ を主スレッド へ汲み直す。ここ だけ が Godot の API を触る。</summary>
        public override void _Process(double delta) => Pump();

        async Task ConnectAsync(CancellationToken token)
        {
            try
            {
                // h2c（暗号化しない HTTP/2）を通す。 既定 では http:// の
                // HTTP/2 が閉じていて、繋ぎに行った瞬間 に落ちる
                // （console client と同じ 1 行）
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                channel = GrpcChannel.ForAddress(Host);

                var service = MagicOnionClient.Create<IDanmakuService>(channel);
                DanmakuState.Names.Value = await service.ListAsync();

                hub = await StreamingHubClient.ConnectAsync<IDanmakuHub, IDanmakuHubReceiver>(
                    channel, receiver, cancellationToken: token);

                // 空 なら一覧 の 0 番。 空 のまま入ると既定 の全方位弾 になり、
                // Unity / MonoGame の 1 本目 と起動 がずれる
                var room = await hub.JoinAsync(new JoinRequest
                {
                    Bulletml = string.IsNullOrEmpty(Bulletml) ? "0" : Bulletml,
                    Seed = Seed,
                });
                token.ThrowIfCancellationRequested();

                // 飛び の物差し を、配る間隔 に合わせる。 合わせないと、
                // 間引いたぶん を全部「落ちた」と数える（コマ番号 は時刻 のまま）
                receiver.Step = Math.Max(1, room.SendEvery);

                DanmakuState.Room.Value = room;
                DanmakuState.Connected.Value = true;

                GD.Print(
                    $"[Danmaku] 部屋 {room.Name} / 進める {room.Fps} コマ毎秒・" +
                    $"配る {room.Fps / Math.Max(1, room.SendEvery)} 回毎秒 / 空間 {room.Space} / " +
                    $"盤面 x[{room.MinX:0.##}, {room.MaxX:0.##}] y[{room.MinY:0.##}, {room.MaxY:0.##}] / " +
                    $"弾幕 {DanmakuState.Names.Value.Length} 本");
            }
            catch (OperationCanceledException)
            {
                // 場面 を抜けた。これは失敗 ではない
            }
            catch (Exception ex)
            {
                // 黙って繋がらないことにしない。 絵 が出ないだけ だと、
                // 配線 かエンジン か描く側 かが分からない
                DanmakuState.Error.Value = $"{Host} に繋がらない: {ex.Message}";
                GD.PushError($"[Danmaku] {DanmakuState.Error.Value}");
            }
        }

        void Pump()
        {
            if (!Connected)
            {
                return;
            }

            var frame = receiver.Take(out int dropped, out int gaps, out int playerHits, out int enemyHits);

            if (dropped > 0)
            {
                DanmakuState.Dropped.Value += dropped;
            }

            if (gaps > 0)
            {
                DanmakuState.Gaps.Value += gaps;
            }

            if (frame != null)
            {
                DanmakuState.Frame.Value = frame.Frame;
                DanmakuState.Bullets.Value = frame.Bullets?.Length ?? 0;
                DanmakuState.Frames.Publish(frame);
            }

            if (playerHits > 0)
            {
                DanmakuState.PlayerHits.Value += playerHits;
            }

            if (enemyHits > 0)
            {
                DanmakuState.EnemyHits.Value += enemyHits;
            }

            SendPlayer();
        }

        /// <summary>
        /// 自機 の位置 を送る。毎コマ 送る。
        ///
        /// <c>aim</c> が読むだけ なら間引けたが、この位置 は当たり判定 の
        /// 入力 にもなった —— 間引くと、その間 に動いたぶん が判定 に映らない。
        ///
        /// 上り は 1 コマ 8 バイト ＝ 60 コマ毎秒 で 4 kbps。
        /// 下り の 2.8 Mbps に対して測るまでもなく無視できる。
        /// </summary>
        void SendPlayer()
        {
            if (PlayerPosition == null)
            {
                return;
            }

            if (--sendCountdown > 0)
            {
                return;
            }

            sendCountdown = Math.Max(1, PlayerSendInterval);

            var p = PlayerPosition();
            hub.SetPlayerAsync(p.X, p.Y).Fire();
        }

        /// <summary>
        /// 自機 の弾 を撃つ。撃つのはサーバー。
        /// ここ が渡すのは位置 だけ で、何発 出るかも どう飛ぶかも向こう が決める。
        ///
        /// 待たない。 撃てたかどうかは次 のコマ の並び に出る。
        /// </summary>
        public void Shoot(float x, float y)
        {
            if (hub == null)
            {
                return;
            }

            hub.ShootAsync(x, y).Fire();
        }

        /// <summary>
        /// 走らせる弾幕 を替える。出て、入り直す。
        /// 部屋 は「弾幕 と 種」で決まるので、入り直すと別 の部屋 になる。
        /// </summary>
        public void Switch(string bulletml)
        {
            if (hub == null)
            {
                return;
            }

            // 出入り が重なると、部屋 に入りっぱなし が残り、
            // 途中 から替えられなくなる
            if (Interlocked.CompareExchange(ref switching, 1, 0) != 0)
            {
                return;
            }

            SwitchAsync(bulletml, life.Token).Fire();
        }

        async Task SwitchAsync(string bulletml, CancellationToken token)
        {
            try
            {
                DanmakuState.Connected.Value = false;

                await hub.LeaveAsync();
                token.ThrowIfCancellationRequested();

                // 部屋 が変わるとコマ番号 は 0 から。 前 の部屋 の番号 を
                // 残すと、次 の 1 コマ を全部「飛び」と数える
                receiver.ResetClock();

                var room = await hub.JoinAsync(new JoinRequest { Bulletml = bulletml, Seed = Seed });
                token.ThrowIfCancellationRequested();

                receiver.Step = Math.Max(1, room.SendEvery);
                DanmakuState.Room.Value = room;
                DanmakuState.Connected.Value = true;
                DanmakuState.Error.Value = "";
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                DanmakuState.Error.Value = $"弾幕 を替えられなかった: {ex.Message}";
                GD.PushError($"[Danmaku] {DanmakuState.Error.Value}");
            }
            finally
            {
                Interlocked.Exchange(ref switching, 0);
            }
        }

        public override void _ExitTree()
        {
            DanmakuState.Connected.Value = false;

            life?.Cancel();

            // 片付け は待たない。 `_ExitTree` は同期 の口 なので、
            // ここ で待つと場面 の切り替え が止まる
            DisposeAsync(hub, channel).Fire();
            hub = null;
            channel = null;
        }

        static async Task DisposeAsync(IDanmakuHub closing, GrpcChannel closingChannel)
        {
            try
            {
                if (closing != null)
                {
                    await closing.DisposeAsync();
                }

                closingChannel?.Dispose();
            }
            catch (Exception ex)
            {
                GD.PushWarning($"[Danmaku] 切るときに: {ex.Message}");
            }
        }

        /// <summary>
        /// 降ってきたコマ を 1 つ だけ持つ。溜めない。
        ///
        /// 溜めると、描く側 が遅れたぶん だけ古いコマ を順 に描くことになり、
        /// 遅れ が返ってこない。 いちばん新しい 1 つ を残して捨てる。
        /// </summary>
        sealed class Receiver : IDanmakuHubReceiver
        {
            FrameDto latest;
            int dropped;
            int gaps;
            int last = -1;
            int pendingPlayerHits;
            int pendingEnemyHits;

            /// <summary>配る間隔。1 なら毎コマ</summary>
            public int Step { get; set; } = 1;

            /// <summary>
            /// 部屋 を替えたとき、前 のコマ番号 を忘れる。
            /// 残すと、新しい部屋 の 0 コマ目 が「飛び」になる。
            /// </summary>
            public void ResetClock()
            {
                Interlocked.Exchange(ref last, -1);
                Interlocked.Exchange(ref latest, null);
                Interlocked.Exchange(ref dropped, 0);
                Interlocked.Exchange(ref gaps, 0);
            }

            public void OnFrame(FrameDto frame)
            {
                // 番号 の飛び を数える。 落ちたコマ は「遅い」ではなく
                // 「来ていない」ので、遅さ とは別 に見えないといけない
                int previousFrame = Interlocked.Exchange(ref last, frame.Frame);
                int step = Step;
                if (previousFrame >= 0 && frame.Frame != previousFrame + step)
                {
                    int missed = (frame.Frame - previousFrame - step) / step;
                    if (missed > 0)
                    {
                        Interlocked.Add(ref gaps, missed);
                    }
                }

                // Interlocked で置き換える。 ここ は Godot の主スレッド ではない
                var previous = Interlocked.Exchange(ref latest, frame);
                if (previous == null)
                {
                    return;
                }

                Interlocked.Increment(ref dropped);

                // 当たり は捨てられない。 位置 は「いま どこか」なので古いコマ を
                // 落としてよいが、当たり は出来事 で、落とすとその 1 発 が
                // 無かったことになる（サーバー側 で「撃ち」と「位置」を
                // 別 に扱ったのと同じ分かれ目）。
                Interlocked.Add(ref pendingPlayerHits, previous.PlayerHits);
                Interlocked.Add(ref pendingEnemyHits, previous.EnemyHits);
            }

            public FrameDto Take(out int droppedSinceLast, out int gapsSinceLast, out int playerHits, out int enemyHits)
            {
                droppedSinceLast = Interlocked.Exchange(ref dropped, 0);
                gapsSinceLast = Interlocked.Exchange(ref gaps, 0);
                playerHits = Interlocked.Exchange(ref pendingPlayerHits, 0);
                enemyHits = Interlocked.Exchange(ref pendingEnemyHits, 0);

                var frame = Interlocked.Exchange(ref latest, null);
                if (frame != null)
                {
                    playerHits += frame.PlayerHits;
                    enemyHits += frame.EnemyHits;
                }

                return frame;
            }
        }
    }
}
