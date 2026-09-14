using FsBulletML2.Sample.MagicOnion.Shared;

namespace FsBulletML2.Sample.Server.MagicOnion.Rooms
{
    /// <summary>
    /// 1 コマ ぶんの並びを作るもの。<b>部屋 1 つ につき 1 個。</b>
    ///
    /// <b>この口 を割ってある理由。</b> 配線（Hub と送信の輪）と
    /// 弾幕エンジンは、別々に壊れる。混ぜて書くと、絵が出ないときに
    /// どちらが悪いのか分からない ——
    /// <see cref="FixedFrameSource"/> に差し替えて、まだ出なければ配線。
    /// </summary>
    public interface IFrameSource
    {
        /// <summary>この部屋の空間の決めごと。<b>入った client に 1 度 だけ配る</b></summary>
        RoomInfo Info { get; }

        /// <summary>自機の位置を知らせる。<c>aim</c> と当たり判定 がこれを読む</summary>
        void SetPlayer(float x, float y);

        /// <summary>
        /// 自機の弾を撃つ。<b>呼ばれるのは輪 の中</b>（<see cref="Room"/> が
        /// 溜めておいて、コマの頭 で渡す）。
        /// </summary>
        void Shoot(float x, float y);

        /// <summary>1 コマ 進めて、いま在る弾 全部 と、このコマで起きた当たりを返す</summary>
        FrameSnapshot Step();
    }

    /// <summary>
    /// 1 コマ 進めた結果。
    ///
    /// <b>当たりの数を並びと一緒に返す理由。</b> 当たった弾はその場で
    /// 並びから落ちるので、<b>並びだけを見ても「消えた」としか分からない</b> ——
    /// 盤面の外へ出たのか、当たったのかが区別できない。
    /// </summary>
    public readonly struct FrameSnapshot
    {
        public FrameSnapshot(BulletDto[] bullets, int playerHits, int enemyHits)
        {
            Bullets = bullets;
            PlayerHits = playerHits;
            EnemyHits = enemyHits;
        }

        public BulletDto[] Bullets { get; }

        /// <summary>このコマで自機に当たった敵弾の数</summary>
        public int PlayerHits { get; }

        /// <summary>このコマで敵に当たった自機弾の数</summary>
        public int EnemyHits { get; }
    }
}