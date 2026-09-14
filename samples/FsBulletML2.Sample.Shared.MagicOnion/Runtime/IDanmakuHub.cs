using System.Threading.Tasks;
using MagicOnion;

namespace FsBulletML2.Sample.MagicOnion.Shared
{
    /// <summary>
    /// 部屋との繋がり。<b>client が知るのはこの口 と <see cref="BulletDto"/> だけ。</b>
    ///
    /// <b>F# も BulletML も出てこない。</b> 出てこないことは目では確かめられない
    /// ので、門 で数える（<c>guard-client-has-no-fsharp.ps1</c>）。
    /// </summary>
    public interface IDanmakuHub : IStreamingHub<IDanmakuHub, IDanmakuHubReceiver>
    {
        /// <summary>部屋に入って、走り始める。空間の決めごとが返る</summary>
        Task<RoomInfo> JoinAsync(JoinRequest request);

        /// <summary>部屋から出る</summary>
        Task LeaveAsync();

        /// <summary>
        /// 自機の位置を知らせる。<b><c>aim</c> はこれを読む。</b>
        ///
        /// 送らなければ、サーバーが持っている既定の位置が使われる
        /// （＝自機を狙う弾幕でも、狙う先が動かない）。
        /// </summary>
        Task SetPlayerAsync(float x, float y);

        /// <summary>
        /// 自機の弾を撃つ。<b>撃つのもサーバー。</b>
        ///
        /// <b>client 側 で撃つ形にしない。</b> そうすると
        /// 「敵の弾はサーバー、自機の弾はローカル」の 2 系統 になり、
        /// **client がまた運動則を持つ**（この帯 が潰そうとしている形）。
        ///
        /// 渡すのは撃つ位置だけ。<b>何発 出るか・どう飛ぶかはサーバーが決める</b>
        /// （同梱の 2way は、この 1 回 で 2 発 出る）。
        ///
        /// <b>待たないで呼んでよい。</b> 待つと自機の動きが往復の遅れに
        /// 引きずられる。撃てたかどうかは、次のコマの並びに出る。
        /// </summary>
        Task ShootAsync(float x, float y);
    }

    /// <summary>サーバーから降ってくるもの</summary>
    public interface IDanmakuHubReceiver
    {
        /// <summary>1 コマ ぶんの並び</summary>
        void OnFrame(FrameDto frame);
    }

    /// <summary>
    /// 走らせられる弾幕の名前を引く。<b>Hub と別にしてあるのは、
    /// 部屋に入る前に要るから。</b>
    /// </summary>
    public interface IDanmakuService : IService<IDanmakuService>
    {
        UnaryResult<string[]> ListAsync();
    }
}
