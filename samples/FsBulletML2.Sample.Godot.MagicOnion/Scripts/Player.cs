using System;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 自機。
    /// </summary>
    public sealed partial class Player : Sprite2D
    {
        /// <summary>盤面 の単位 / 秒。Unity 版 の speed 5（＝ 3.0 単位/秒）に合わせてある</summary>
        public float Speed { get; set; } = 3.0f;

        /// <summary>盤面 の縁 から空ける割合。幅 に対して</summary>
        public float Margin { get; set; } = 0.08f;

        public DanmakuClient Client { get; set; }

        /// <summary>
        /// 最初 に置く場所。
        /// </summary>
        public Vector2? Start { get; set; }

        /// <summary>いまの位置。盤面 の値（画面 の px ではない）</summary>
        public Vector2 Field { get; private set; }

        Space space;
        RoomInfo room;
        IDisposable roomSub;
        float minX, maxX, minY, maxY;
        bool placed;

        public override void _Ready()
        {
            // 他 の sample と同じ絵。 `Assets/Sprites/player.png`（24x24）
            Texture = Sprites.Player;
            ZIndex = 10;
            Visible = false;

            roomSub = DanmakuState.Room.Subscribe(OnRoom);
        }

        void OnRoom(RoomInfo info)
        {
            room = info;
            if (info == null)
            {
                Visible = false;
                return;
            }

            space = new Space(info, GetViewportRect().Size);

            float m = (info.MaxX - info.MinX) * Margin;
            minX = info.MinX + m;
            maxX = info.MaxX - m;

            // 敵 より下（YUp）／上（YDown）。 盤面 の真ん中 を
            // どちら が使うかは、弾 の出どころ が決めている
            if (info.Space == SpaceDto.YUp)
            {
                minY = info.MinY + m;
                maxY = info.OriginY - m;
            }
            else
            {
                minY = info.OriginY + m;
                maxY = info.MaxY - m;
            }

            // 部屋 に入り直した（弾幕 を替えた）ときは動かさない。
            // 置き直すと、撃ちながら替えた人 の手元 が飛ぶ
            if (!placed)
            {
                placed = true;

                // `--player` は縛りの外 へも置ける。
                Field = Start ?? new Vector2(
                    (info.MinX + info.MaxX) * 0.5f,
                    info.Space == SpaceDto.YUp ? info.MinY + 0.8f : info.MaxY - 0.8f);
            }

            // Unity の Pixels Per Unit 100 に揃える。 揃えてあるので、
            // 同じ盤面 なら Unity 版 と同じ大きさ で出る
            Scale = Vector2.One * (space.Scale / BulletField.PixelsPerUnit);
            Apply();
            Visible = true;
        }

        public override void _Process(double delta)
        {
            if (space == null || room == null)
            {
                return;
            }

            float dx = 0f;
            float dy = 0f;
            if (Input.IsPhysicalKeyPressed(Key.Left)) { dx -= 1f; }
            if (Input.IsPhysicalKeyPressed(Key.Right)) { dx += 1f; }
            if (Input.IsPhysicalKeyPressed(Key.Up)) { dy += 1f; }
            if (Input.IsPhysicalKeyPressed(Key.Down)) { dy -= 1f; }

            // Y の向き はサーバーが言う。 `YDown` の部屋 では上 キー が
            // 盤面 の値 を減らす側 になる
            if (room.Space == SpaceDto.YDown)
            {
                dy = -dy;
            }

            if (dx != 0f || dy != 0f)
            {
                // 斜め を速くしない。 揃えないと、斜め だけ 1.41 倍 で動く
                var move = new Vector2(dx, dy).Normalized() * Speed * (float)delta;
                Field = new Vector2(
                    Mathf.Clamp(Field.X + move.X, minX, maxX),
                    Mathf.Clamp(Field.Y + move.Y, minY, maxY));
                Apply();
            }

            // 押しっぱなし で毎コマ 送る（Unity 版 と同じ間合い）。
            // 溜まりすぎたぶん はサーバーが捨てて、捨てた数 を数えている
            if (Input.IsPhysicalKeyPressed(Key.Z))
            {
                Client?.Shoot(Field.X, Field.Y);
            }
        }

        void Apply() => Position = space.ToScreen(Field.X, Field.Y);

        public override void _ExitTree() => roomSub?.Dispose();
    }
}
