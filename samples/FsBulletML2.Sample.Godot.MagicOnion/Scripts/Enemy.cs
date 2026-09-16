using System;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 敵 の絵。撃たない。
    ///
    /// サーバーが言った弾 の出どころ へ置く。
    /// <see cref="RoomInfo.OriginX"/> / <see cref="RoomInfo.OriginY"/> がそれ。
    public sealed partial class Enemy : Sprite2D
    {
        IDisposable roomSub;

        public override void _Ready()
        {
            // 他 の sample と同じ絵。 `Assets/Sprites/enemy1.png`（32x32）
            Texture = Sprites.Enemy;
            ZIndex = 5;
            Visible = false;

            roomSub = DanmakuState.Room.Subscribe(OnRoom);
        }

        void OnRoom(RoomInfo info)
        {
            if (info == null)
            {
                Visible = false;
                return;
            }

            var space = new Space(info, GetViewportRect().Size);
            Position = space.ToScreen(info.OriginX, info.OriginY);
            Scale = Vector2.One * (space.Scale / BulletField.PixelsPerUnit);
            Visible = true;
        }

        public override void _ExitTree() => roomSub?.Dispose();
    }
}
