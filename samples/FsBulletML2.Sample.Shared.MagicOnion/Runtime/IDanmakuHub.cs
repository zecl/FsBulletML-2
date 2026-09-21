using System.Threading.Tasks;
using MagicOnion;

namespace FsBulletML2.Sample.MagicOnion.Shared
{
    /// <summary>
    /// 部屋との繋がり。client が知るのはこの口 と <see cref="BulletDto"/> だけ。
    ///
    /// F# も BulletML も出てこない。 出てこないことは目では確かめられない
    /// ので、門 で数える（<c>guard-client-has-no-fsharp.ps1</c>）。
    /// </summary>
    public interface IDanmakuHub : IStreamingHub<IDanmakuHub, IDanmakuHubReceiver>
    {
        /// <summary>部屋に入って、走り始める。空間の決めごとが返る</summary>
        ValueTask<RoomInfo> JoinAsync(JoinRequest request);

        /// <summary>部屋から出る</summary>
        ValueTask LeaveAsync();

        /// <summary>
        /// 自機の位置を知らせる。<c>aim</c> はこれを読む。
        ///
        /// 送らなければ、サーバーが持っている既定の位置が使われる
        /// （＝自機を狙う弾幕でも、狙う先が動かない）。
        /// </summary>
        ValueTask SetPlayerAsync(float x, float y);

        /// <summary>
        /// 自機の弾を撃つ。撃つのもサーバー。
        ///
        /// client 側 で撃つ形にしない。 そうすると
        /// 「敵の弾はサーバー、自機の弾はローカル」の 2 系統 になり、
        /// client がまた運動則を持つ（この帯 が潰そうとしている形）。
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
