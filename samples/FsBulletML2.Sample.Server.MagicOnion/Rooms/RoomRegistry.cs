using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Logging;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Rooms
{
    /// <summary>
    /// 走っている部屋 の台帳。<b>サーバーに 1 つ（singleton）。</b>
    ///
    /// <b>部屋 を 2 つ 同時に走らせられることが、この帯 の山場（E1.4）。</b>
    /// 同梱のフロント 4 本 は <c>BulletMLManager</c>（static mutable）から
    /// 乱数・ランク・自機 を引いていて、あれを通すと部屋 が混ざる。
    /// **ここは 1 度 も通さない** —— エンジンの口（<c>IFrontEnv</c>）は
    /// 引数で受け取る形なので、部屋ごとに 1 個 持てる。
    /// </summary>
    public sealed class RoomRegistry : IAsyncDisposable
    {
        readonly Dictionary<string, Room> rooms = new Dictionary<string, Room>(StringComparer.Ordinal);
        readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        readonly IFrameSourceFactory factory;
        readonly ILoggerFactory loggers;
        readonly WireMeter meter;
        readonly RoomOptions options;

        public RoomRegistry(
            IFrameSourceFactory factory, ILoggerFactory loggers, WireMeter meter, RoomOptions options)
        {
            this.factory = factory;
            this.loggers = loggers;
            this.meter = meter;
            this.options = options;
        }

        /// <summary>
        /// 部屋 に入る。無ければ建てる。
        ///
        /// <b>建てると同時に走り始める。</b> 「入った人が居るのに止まっている」
        /// 状態を作らない —— その状態は client からは配線の故障と見分けが付かない。
        /// </summary>
        public async ValueTask<Room> EnterAsync(
            string key, JoinRequest request, IGroup<IDanmakuHubReceiver> group)
        {
            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!rooms.TryGetValue(key, out var room))
                {
                    room = new Room(
                        key, factory.Create(request), group, loggers.CreateLogger<Room>(), meter, options);
                    rooms.Add(key, room);
                }

                room.Enter();
                return room;
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>
        /// 部屋 から出る。<b>最後の 1 人 が出たら畳む。</b>
        ///
        /// 畳まないと、誰も見ていない弾幕を 60 コマ/秒 で回し続ける。
        /// </summary>
        public async ValueTask LeaveAsync(string key)
        {
            Room closing = null;

            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                if (rooms.TryGetValue(key, out var room) && room.Leave() <= 0)
                {
                    rooms.Remove(key);
                    closing = room;
                }
            }
            finally
            {
                gate.Release();
            }

            if (closing != null)
            {
                await closing.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// いま走っている部屋 の写し。<b>状況 の行 が 1 秒 に 1 回 読む。</b>
        ///
        /// 写しを返すのは、読む側 が台帳 を握ったまま長く回らないため。
        /// </summary>
        public async ValueTask<Room[]> SnapshotAsync()
        {
            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                var snapshot = new Room[rooms.Count];
                rooms.Values.CopyTo(snapshot, 0);
                return snapshot;
            }
            finally
            {
                gate.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                foreach (var room in rooms.Values)
                {
                    await room.DisposeAsync().ConfigureAwait(false);
                }

                rooms.Clear();
            }
            finally
            {
                gate.Release();
            }

            gate.Dispose();
        }
    }

    /// <summary>
    /// 注文から <see cref="IFrameSource"/> を作るもの。
    /// <b>差し替える口。</b> E1.0 は固定の並び、E1.1 からエンジン。
    /// </summary>
    public interface IFrameSourceFactory
    {
        IFrameSource Create(JoinRequest request);

        /// <summary>走らせられる弾幕の名前</summary>
        string[] List();
    }

    /// <summary>配線を確かめるための工場。<b>エンジンを呼ばない</b></summary>
    public sealed class FixedFrameSourceFactory : IFrameSourceFactory
    {
        public IFrameSource Create(JoinRequest request) => new FixedFrameSource();

        public string[] List() => new[] { FixedFrameSource.RoomName };
    }
}
