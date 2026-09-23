using System.Threading.Tasks;
using MagicOnion;

namespace FsBulletML2.Sample.MagicOnion.Shared
{
    /// <summary>
    /// 部屋との繋がり。
    /// </summary>
    public interface IDanmakuHub : IStreamingHub<IDanmakuHub, IDanmakuHubReceiver>
    {
        /// <summary>部屋に入って、走り始める。空間の決めごとが返る</summary>
        ValueTask<RoomInfo> JoinAsync(JoinRequest request);

        /// <summary>部屋から出る</summary>
        ValueTask LeaveAsync();

        /// <summary>
        /// 自機の位置を知らせる。
        /// </summary>
        ValueTask SetPlayerAsync(float x, float y);

        /// <summary>
        /// 自機の弾を撃つ。
        /// </summary>
        ValueTask ShootAsync(float x, float y);
    }

    /// <summary>サーバーから降ってくるもの</summary>
    public interface IDanmakuHubReceiver
    {
        /// <summary>1 コマ ぶんの並び</summary>
        void OnFrame(FrameDto frame);
    }

    /// <summary>
    /// 走らせられる弾幕の名前を引く。Hub と別にしてあるのは、
    /// 部屋に入る前に要るから。
    /// </summary>
    public interface IDanmakuService : IService<IDanmakuService>
    {
        UnaryResult<string[]> ListAsync();
    }
}
