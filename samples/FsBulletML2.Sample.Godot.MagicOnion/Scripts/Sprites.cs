using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 絵 の置き場。
    /// </summary>
    public static class Sprites
    {
        public static Texture2D Player => GD.Load<Texture2D>("res://Sprites/player.png");

        public static Texture2D Enemy => GD.Load<Texture2D>("res://Sprites/enemy1.png");

        /// <summary>敵 の弾（<c>Kind</c> 0）</summary>
        public static Texture2D EnemyBullet => GD.Load<Texture2D>("res://Sprites/g_bullet_s.png");

        /// <summary>自機 の弾（<c>Kind</c> 1）</summary>
        public static Texture2D PlayerBullet => GD.Load<Texture2D>("res://Sprites/p_bullet_s.png");
    }
}
