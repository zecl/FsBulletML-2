using MessagePack;

namespace FsBulletML2.Sample.MagicOnion.Shared
{
    /// <summary>
    /// 弾 1 発。この struct が、client が知ってよいものの全部。
    ///
    /// BulletML の語（<c>changeDirection</c> / <c>accel</c> / <c>term</c>）は
    ///
    /// サーバーと 2 か所 に同じものが在る形になる。
    [MessagePackObject]
    public struct BulletDto
    {
        /// <summary>部屋の中で一意。コマをまたいで同じ弾は同じ番号。
        /// 消えた弾は番号ごと消える（次の並びに出てこない）</summary>
        [Key(0)] public int Id;

        /// <summary>
        /// 盤面 を 0..65535 に割った位置。float ではない。
        ///
        /// float32 は MessagePack で 5 バイト固定（0xca ＋ 4）。
        [Key(1)] public ushort X;
        [Key(2)] public ushort Y;

        /// <summary>向き。1/100 度。真上が 0 で時計回り（BulletML の決めごとそのまま）</summary>
        [Key(3)] public ushort Dir;

        /// <summary>0 = 敵の弾 / 1 = 自機の弾。絵を選ぶためだけ</summary>
        [Key(4)] public byte Kind;
    }

    /// <summary>
    /// 1 コマ ぶん。形 A（毎コマ の並び）——
    /// いま在る弾 全部 を毎回 送る。差分ではない。
    ///
    /// 差分にするのは E1.5（帯域を締める段）で、先に測ってから。
    /// </summary>
    [MessagePackObject]
    public class FrameDto
    {
        /// <summary>部屋が始まってから何コマ 目 か。抜けを数えるための番号</summary>
        [Key(0)] public int Frame { get; set; }

        [Key(1)] public BulletDto[] Bullets { get; set; }

        /// <summary>
        /// このコマで自機に当たった弾の数。当たり判定 はサーバーが持つ。
        ///
        /// 当たった弾はその場で並びから消えるので、
        /// 「同じ弾を 2 回 数えた」が起きない ——
        /// client 側 で判定していたときは、重なっているあいだ毎コマ 数えないように
        /// 「もう数えた弾」を覚えておく必要があった。
        /// </summary>
        [Key(2)] public ushort PlayerHits { get; set; }

        /// <summary>このコマで敵に当たった自機弾の数</summary>
        [Key(3)] public ushort EnemyHits { get; set; }
    }

    /// <summary>
    /// 配る形 と 描く値 のあいだ の換算。両側 が同じこれを使う。
    ///
    /// 2 か所 に書かない。 丸め方 が 1 ビット でもずれると、
    /// 弾が半 ピクセル ずれた場所 に出る —— ビルドは通るし落ちもしないので、
    /// 目 でしか分からない類 の割れ方 になる。
    /// </summary>
    public static class Wire
    {
        /// <summary>向き の刻み。1 度 を 100 に割る</summary>
        public const float DirScale = 100f;

        public static ushort ToGrid(float v, float min, float max)
        {
            float t = (v - min) / (max - min);
            if (t <= 0f) { return 0; }
            if (t >= 1f) { return ushort.MaxValue; }
            return (ushort)(t * ushort.MaxValue);
        }

        public static float FromGrid(ushort g, float min, float max)
            => min + ((max - min) * g / ushort.MaxValue);

        /// <summary>度 を 1/100 度 へ。0..360 に畳んでから</summary>
        public static ushort ToDir(float degrees)
        {
            float d = degrees % 360f;
            if (d < 0f) { d += 360f; }
            int v = (int)(d * DirScale);
            return (ushort)(v >= 36000 ? 0 : v);
        }

        public static float FromDir(ushort d) => d / DirScale;
    }

    /// <summary>Y がどちらを向いているか。サーバーが決めて配る</summary>
    public enum SpaceDto : byte
    {
        YUp = 0,
        YDown = 1,
    }

    /// <summary>
    /// 部屋に入ったときに 1 度 だけ返るもの。
    ///
    /// ここに空間の決めごとを載せる理由。 弾の向きを決める式（aim）は
    /// 座標系の上で計算されるので、サーバーと client が別の空間を持つと
    /// 軌跡が割れる。サーバーが自分の空間で走らせ、その名前を配る。
    /// client がやるのは描くときの変換だけで、物理量には触らない。
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

        /// <summary>
        /// 何コマ に 1 回 配るか。1 なら毎コマ。
        ///
        /// これを配らないと、client は間引きと抜けを見分けられない。
        /// <see cref="FrameDto.Frame"/> はコマ番号（＝時刻）のままなので、
        /// 2 なら 1, 3, 5… と飛ぶ —— 配らなければ、client は全部 を
        /// 「落ちた」と数えることになる。
        /// </summary>
        [Key(9)] public int SendEvery { get; set; } = 1;
    }

    /// <summary>部屋に入るときの注文</summary>
    [MessagePackObject]
    public class JoinRequest
    {
        /// <summary>走らせる弾幕の名前。空 なら サーバーの既定</summary>
        [Key(0)] public string Bulletml { get; set; }

        /// <summary>
        /// 乱数の種。同じ種 と同じ弾幕 なら、同じ並びが出る。
        /// 0 なら サーバーが時計から採る（＝毎回 違う）
        /// </summary>
        [Key(1)] public int Seed { get; set; }
    }
}
