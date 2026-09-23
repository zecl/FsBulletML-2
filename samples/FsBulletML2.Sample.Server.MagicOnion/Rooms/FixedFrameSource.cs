using System;
using FsBulletML2.Sample.MagicOnion.Shared;

namespace FsBulletML2.Sample.Server.MagicOnion.Rooms
{
    /// <summary>
    /// 弾幕エンジンを 1 度 も呼ばない並び。配線だけを確かめるためのもの（E1.0）。
    ///
    /// 弾は 8 発。輪 になって回り、盤面の外へは出ない。
    public sealed class FixedFrameSource : IFrameSource
    {
        public const string RoomName = "(fixed)";

        const int Count = 8;
        const float Radius = 1.6f;

        readonly float centerX;
        readonly float centerY;
        int frame;

        public FixedFrameSource()
        {
            Info = new RoomInfo
            {
                Name = RoomName,
                Fps = RoomLoop.Fps,
                Space = SpaceDto.YUp,
                MinX = Field.MinX,
                MaxX = Field.MaxX,
                MinY = Field.MinY,
                MaxY = Field.MaxY,
                OriginX = Field.OriginX,
                OriginY = Field.OriginY,
            };

            centerX = (Field.MinX + Field.MaxX) * 0.5f;
            centerY = (Field.MinY + Field.MaxY) * 0.5f;
        }

        public RoomInfo Info { get; }

        /// <summary>読まない。 固定の並びは自機を狙わない</summary>
        public void SetPlayer(float x, float y)
        {
        }

        /// <summary>撃たない。 ここは配線だけを見る並び</summary>
        public void Shoot(float x, float y)
        {
        }

        public FrameSnapshot Step()
        {
            frame++;

            var bullets = new BulletDto[Count];
            for (int i = 0; i < Count; i++)
            {
                // 1 秒 で 1 周。度 で持ち回る（口 が度 なので、ここで
                // ラジアンへ落とすと単位が 2 つ になる）
                float deg = (360f / Count * i) + (frame * 360f / RoomLoop.Fps);
                float rad = deg * (float)Math.PI / 180f;

                bullets[i] = new BulletDto
                {
                    Id = i,
                    // 真上が 0 で時計回り。x が sin、y が cos
                    X = Wire.ToGrid(centerX + (Radius * (float)Math.Sin(rad)), Field.MinX, Field.MaxX),
                    Y = Wire.ToGrid(centerY + (Radius * (float)Math.Cos(rad)), Field.MinY, Field.MaxY),
                    Dir = Wire.ToDir(deg),
                    Kind = 0,
                };
            }

            // 当たり判定 はしない。 ここは配線だけを見る並び
            return new FrameSnapshot(bullets, 0, 0);
        }
    }

    /// <summary>
    /// 盤面。Unity2D サンプルの <c>BulletEntityFactory</c> と同じ値。
    /// 写した側 が古びるので、変えたら両方（写しは 1 か所 だけ）。
    /// </summary>
    public static class Field
    {
        public const float MinX = 0f;
        public const float MaxX = 4.8f;
        public const float MinY = -6.4f;
        public const float MaxY = 0f;

        /// <summary>
        /// 敵の居場所。
        /// サーバーが決めて <c>RoomInfo</c> で配り、client が敵 をそこへ置く —— 逆にすると、弾の出どころ と敵の絵 がずれる（実際に 0.4 ずれていた）。
        /// </summary>
        public const float OriginX = 2.41f;
        public const float OriginY = -0.8f;

        /// <summary>
        /// 当たり判定 の大きさ。
        /// 写した側 が古びるので、絵 を替えたらここも替える。
        /// </summary>
        public const float BulletRadius = 0.1f;

        public const float PlayerRadius = 0.15f;

        public const float EnemyRadius = 0.25f;

        public static bool IsOutside(float x, float y)
            => x < MinX || x > MaxX || y < MinY || y > MaxY;
    }
}
