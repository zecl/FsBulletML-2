using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using MagicOnion;
using MagicOnion.Server;

namespace FsBulletML2.Sample.Server.MagicOnion.Services
{
    /// <summary>走らせられる弾幕の名前を返すだけ</summary>
    public sealed class DanmakuService : ServiceBase<IDanmakuService>, IDanmakuService
    {
        readonly IFrameSourceFactory factory;

        public DanmakuService(IFrameSourceFactory factory)
        {
            this.factory = factory;
        }

        public UnaryResult<string[]> ListAsync() => UnaryResult.FromResult(factory.List());
    }
}
