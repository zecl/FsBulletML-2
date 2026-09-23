using System.Threading.Tasks;
using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Logging;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;

namespace FsBulletML2.Sample.Server.MagicOnion.Hubs
{
    /// <summary>
    /// 口 の実装。
    /// </summary>
    public sealed class DanmakuHub : StreamingHubBase<IDanmakuHub, IDanmakuHubReceiver>, IDanmakuHub
    {
        readonly RoomRegistry registry;
        readonly ConnectionCounter connections;
        readonly ILogger logger;

        string key;
        Room room;

        public DanmakuHub(RoomRegistry registry, ConnectionCounter connections, ILoggerFactory loggers)
        {
            this.registry = registry;
            this.connections = connections;
            logger = loggers.CreateLogger("conn");
        }

        /// <summary>
        /// 繋がった。ここを出すのが、いちばん手前 の切り分け ——
        /// この行 が出ないなら、届いていないのは口 ではなく網 か港。
        /// </summary>
        protected override ValueTask OnConnecting()
        {
            int live = connections.Connected();
            logger.LogInformation("繋がった  #{Conn}  （いま {Live} 本）", Id(), live);
            return default;
        }

        public async ValueTask<RoomInfo> JoinAsync(JoinRequest request)
        {
            request ??= new JoinRequest();

            // 入る前 に出る。 出さないと、前 の部屋 に入りっぱなし が残り、
            // client が連打 したときに替えられなくなる
            await LeaveAsync();

            // 部屋 の名前は「弾幕 と 種」で決まる。 同じ組なら同じ部屋 に入る
            // ＝ 同じ並びを見る（観戦）。違う種 なら別の部屋 になる
            key = $"{request.Bulletml}#{request.Seed}";

            var group = await Group.AddAsync(key);
            room = await registry.EnterAsync(key, request, group);

            logger.LogInformation(
                "部屋「{Room}」へ  人 {Members}  #{Conn}", room.Info.Name, room.Members, Id());
            return room.Info;
        }

        public async ValueTask LeaveAsync()
        {
            if (room == null)
            {
                return;
            }

            string name = room.Info.Name;
            await registry.LeaveAsync(key);
            logger.LogInformation("部屋「{Room}」から出た  #{Conn}", name, Id());

            room = null;
            key = null;
        }

        public ValueTask SetPlayerAsync(float x, float y)
        {
            room?.SetPlayer(x, y);
            return ValueTask.CompletedTask;
        }

        public ValueTask ShootAsync(float x, float y)
        {
            room?.Shoot(x, y);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 切れたときも部屋 から引く。 引かないと、落ちた client のぶんだけ
        /// 人数 が減らず、誰も見ていない部屋 が回り続ける。
        /// </summary>
        protected override async ValueTask OnDisconnected()
        {
            await LeaveAsync();

            int live = connections.Disconnected();
            logger.LogInformation("切れた  #{Conn}  （いま {Live} 本）", Id(), live);
        }

        string Id() => HubAccessLogFilter.Short(Context.ContextId);
    }
}