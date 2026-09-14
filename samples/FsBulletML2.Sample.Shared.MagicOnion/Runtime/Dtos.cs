using MessagePack;

namespace FsBulletML2.Sample.MagicOnion.Shared
{
    /// <summary>
    /// 弾 1 発。<b>この struct が、client が知ってよいものの全部。</b>
    ///
    /// BulletML の語（<c>changeDirection</c> / <c>accel</c> / <c>term</c>）は
    /// 1 つ も出てこない。出した瞬間に client が運動則を持つことになり、
    /// サーバーと 2 か所 に同じものが在る形になる。
    ///
    /// <b>位置は <see cref="RoomInfo"/> が宣言した空間の値</b>で、
    /// client はそれを描く座標へ移すだけ。掛ける係数を client が決めない。
    /// </summary>
    [MessagePackObject]
    public struct BulletDto
    {
        /// <summary>部屋の中で一意。<b>コマをまたいで同じ弾は同じ番号。</b>
        /// 消えた弾は番号ごと消える（次の並びに出てこない）</summary>
        [Key(0)] public int Id;

        [Key(1)] public float X;
        [Key(2)] public float Y;

        /// <summary>向き。度。真上が 0 で時計回り（BulletML の決めごとそのまま）</summary>
        [Key(3)] public float Dir;

        /// <summary>0 = 敵の弾 / 1 = 自機の弾。<b>絵を選ぶためだけ</b></summary>
        [Key(4)] public byte Kind;
    }

    /// <summary>
    /// 1 コマ ぶん。<b>形 A（毎コマ の並び）</b>——
    /// いま在る弾 全部 を毎回 送る。差分ではない。
    ///
    /// 差分にするのは E1.5（帯域を締める段）で、<b>先に測ってから</b>。
    /// </summary>
    [MessagePackObject]
    public class FrameDto
    {
        /// <summary>部屋が始まってから何コマ 目 か。<b>抜けを数えるための番号</b></summary>
        [Key(0)] public int Frame { get; set; }

        [Key(1)] public BulletDto[] Bullets { get; set; }

        /// <summary>
        /// このコマで自機に当たった弾の数。<b>当たり判定 はサーバーが持つ。</b>
        ///
        /// 当たった弾はその場で並びから消えるので、
        /// <b>「同じ弾を 2 回 数えた」が起きない</b> ——
        /// client 側 で判定していたときは、重なっているあいだ毎コマ 数えないように
        /// 「もう数えた弾」を覚えておく必要があった。
        /// </summary>
        [Key(2)] public ushort PlayerHits { get; set; }

        /// <summary>このコマで敵に当たった自機弾の数</summary>
        [Key(3)] public ushort EnemyHits { get; set; }
    }

    /// <summary>Y がどちらを向いているか。<b>サーバーが決めて配る</b></summary>
    public enum SpaceDto : byte
    {
        YUp = 0,
        YDown = 1,
    }

    /// <summary>
    /// 部屋に入ったときに 1 度 だけ返るもの。
    ///
    /// <b>ここに空間の決めごとを載せる理由。</b> 弾の向きを決める式（aim）は
    /// 座標系の上で計算されるので、サーバーと client が別の空間を持つと
    /// 軌跡が割れる。サーバーが自分の空間で走らせ、その名前を配る。
    /// client がやるのは描くときの変換だけで、<b>物理量には触らない。</b>
    /// </summary>
    [MessagePackObject]
    public class RoomInfo
    {
        [Key(0)] public string Name { get; set; }

        /// <summary>1 秒 に何コマ 進めるか</summary>
        [Key(1)] public int Fps { get; set; }

        [Key(2)] public SpaceDto Space { get; set; }

        /// <summary>盤面。この外へ出た弾はサーバーが捨てる</summary>
        [Key(3)] public float MinX { get; set; }
        [Key(4)] public float MaxX { get; set; }
        [Key(5)] public float MinY { get; set; }
        [Key(6)] public float MaxY { get; set; }

        /// <summary>弾を撃ち始める場所（＝敵の居場所）</summary>
        [Key(7)] public float OriginX { get; set; }
        [Key(8)] public float OriginY { get; set; }
    }

    /// <summary>部屋に入るときの注文</summary>
    [MessagePackObject]
    public class JoinRequest
    {
        /// <summary>走らせる弾幕の名前。空 なら サーバーの既定</summary>
        [Key(0)] public string Bulletml { get; set; }

        /// <summary>
        /// 乱数の種。<b>同じ種 と同じ弾幕 なら、同じ並びが出る。</b>
        /// 0 なら サーバーが時計から採る（＝毎回 違う）
        /// </summary>
        [Key(1)] public int Seed { get; set; }
    }
}
