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

        /// <summary>
        /// 盤面 を 0..65535 に割った位置。<b>float ではない。</b>
        ///
        /// <b>float32 は MessagePack で 5 バイト固定</b>（0xca ＋ 4）。
        /// ushort は 3 バイト で乗るので、x / y / 向き の 3 本 で
        /// **弾 1 発 が 19.9 -> 13.8 バイト**（実測。<c>--measure-wire</c>）。
        ///
        /// 刻み は盤面 4.8 x 6.4 に対して 0.0001 未満。
        /// **描く前 に <see cref="Wire"/> で戻す** —— 戻す式 は
        /// <see cref="RoomInfo"/> の盤面 から決まるので、client は
        /// 相変わらず物理量 を持たない。
        /// </summary>
        [Key(1)] public ushort X;
        [Key(2)] public ushort Y;

        /// <summary>向き。<b>1/100 度</b>。真上が 0 で時計回り（BulletML の決めごとそのまま）</summary>
        [Key(3)] public ushort Dir;

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

    /// <summary>
    /// 配る形 と 描く値 のあいだ の換算。<b>両側 が同じこれを使う。</b>
    ///
    /// <b>2 か所 に書かない。</b> 丸め方 が 1 ビット でもずれると、
    /// 弾が半 ピクセル ずれた場所 に出る —— ビルドは通るし落ちもしないので、
    /// 目 でしか分からない類 の割れ方 になる。
    /// </summary>
    public static class Wire
    {
        /// <summary>向き の刻み。<b>1 度 を 100 に割る</b></summary>
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

        /// <summary>度 を 1/100 度 へ。<b>0..360 に畳んでから</b></summary>
        public static ushort ToDir(float degrees)
        {
            float d = degrees % 360f;
            if (d < 0f) { d += 360f; }
            int v = (int)(d * DirScale);
            return (ushort)(v >= 36000 ? 0 : v);
        }

        public static float FromDir(ushort d) => d / DirScale;
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

        /// <summary>
        /// 何コマ に 1 回 配るか。<b>1 なら毎コマ。</b>
        ///
        /// <b>これを配らないと、client は間引きと抜けを見分けられない。</b>
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
        /// 乱数の種。<b>同じ種 と同じ弾幕 なら、同じ並びが出る。</b>
        /// 0 なら サーバーが時計から採る（＝毎回 違う）
        /// </summary>
        [Key(1)] public int Seed { get; set; }
    }
}
