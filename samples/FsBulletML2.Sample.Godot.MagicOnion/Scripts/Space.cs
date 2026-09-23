using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 盤面 の値 を、画面 の座標 へ移す。
    /// </summary>
    public sealed class Space
    {
        readonly float minX;
        readonly float minY;
        readonly float maxY;
        readonly bool yUp;

        /// <summary>盤面 1 に対して画面 何 px か。弾 の大きさ もこれ で決める</summary>
        public float Scale { get; }

        public Vector2 Offset { get; }

        public Space(RoomInfo room, Vector2 viewport)
        {
            minX = room.MinX;
            minY = room.MinY;
            maxY = room.MaxY;
            yUp = room.Space == SpaceDto.YUp;

            float w = room.MaxX - room.MinX;
            float h = room.MaxY - room.MinY;

            // letterbox。 縦 と横 で別 の倍率 を掛けない
            Scale = Mathf.Min(viewport.X / w, viewport.Y / h);
            Offset = new Vector2(
                (viewport.X - (w * Scale)) * 0.5f,
                (viewport.Y - (h * Scale)) * 0.5f);
        }

        /// <summary>
        /// 盤面 の座標 -> 画面。
        /// </summary>
        public Vector2 ToScreen(float x, float y)
        {
            float sx = Offset.X + ((x - minX) * Scale);
            float sy = yUp
                ? Offset.Y + ((maxY - y) * Scale)
                : Offset.Y + ((y - minY) * Scale);
            return new Vector2(sx, sy);
        }

        /// <summary>画面 -> 盤面。自機 の位置 を送るときに使う</summary>
        public Vector2 ToField(Vector2 screen)
        {
            float x = minX + ((screen.X - Offset.X) / Scale);
            float y = yUp
                ? maxY - ((screen.Y - Offset.Y) / Scale)
                : minY + ((screen.Y - Offset.Y) / Scale);
            return new Vector2(x, y);
        }

        /// <summary>
        /// 配ってきた目盛り（0..65535）を盤面 の値 へ戻してから画面 へ。
        /// 2 か所 に書くと、丸め方 が 1 ビット ずれた瞬間 に 弾 が半 ピクセル ずれた場所 に出る —— ビルド は通るし落ちもしない。
        /// </summary>
        public Vector2 ToScreen(in BulletDto b, RoomInfo room) => ToScreen(
            Wire.FromGrid(b.X, room.MinX, room.MaxX),
            Wire.FromGrid(b.Y, room.MinY, room.MaxY));
    }
}
