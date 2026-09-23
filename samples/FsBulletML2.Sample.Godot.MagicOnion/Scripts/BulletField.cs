using System;
using FsBulletML2.Sample.MagicOnion.Shared;
using Godot;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// 降ってきた弾 を描く。進めない・消さない・当たりを見ない。
    ///
    /// Unity 版 の <c>BulletSimulationSystem</c> が「描くだけ」になったのと同じ形。
    public sealed partial class BulletField : Node2D
    {
        /// <summary>
        /// 絵 の 1 px が盤面 のいくつ か。Unity の既定（Pixels Per Unit 100）と同じ。
        /// 揃えてあるので、同じ弾幕 が Unity 版 と同じ大きさ で出る。
        /// </summary>
        public const float PixelsPerUnit = 100f;

        Layer enemy;
        Layer player;
        Space space;
        RoomInfo room;
        Rect2 screen;
        IDisposable roomSub;
        IDisposable frameSub;

        public override void _Ready()
        {
            enemy = new Layer(Sprites.EnemyBullet);
            player = new Layer(Sprites.PlayerBullet);
            AddChild(enemy.Node);
            AddChild(player.Node);

            roomSub = DanmakuState.Room.Subscribe(OnRoom);
            frameSub = DanmakuState.Frames.Subscribe(OnFrame);
        }

        void OnRoom(RoomInfo info)
        {
            room = info;
            if (info == null)
            {
                enemy.Hide();
                player.Hide();
                return;
            }

            screen = GetViewportRect();
            space = new Space(info, screen.Size);

            float zoom = space.Scale / PixelsPerUnit;
            enemy.Resize(zoom);
            player.Resize(zoom);

            // 前 の部屋 の弾 を残さない。 残すと、次 のコマ が来るまで
            // 古い弾幕 が乗ったまま に見える
            enemy.Hide();
            player.Hide();
        }

        void OnFrame(FrameDto frame)
        {
            var bullets = frame.Bullets;
            if (bullets == null || space == null || room == null)
            {
                enemy.Hide();
                player.Hide();
                DanmakuState.Drawn.Value = 0;
                DanmakuState.OnScreen.Value = 0;
                return;
            }

            enemy.Begin(bullets.Length);
            player.Begin(bullets.Length);

            int visible = 0;

            for (int i = 0; i < bullets.Length; i++)
            {
                ref var b = ref bullets[i];
                var at = space.ToScreen(in b, room);

                // 向き を当てる。
                float rot = Wire.FromDir(b.Dir);

                // 0 = 敵 の弾 / 1 = 自機 の弾
                var layer = b.Kind == 1 ? player : enemy;
                layer.Put(at, rot);

                if (screen.HasPoint(at))
                {
                    visible++;
                }
            }

            enemy.End();
            player.End();

            // 届いた数 と 描いた数 を並べて出す。
            DanmakuState.Drawn.Value = bullets.Length;
            DanmakuState.OnScreen.Value = visible;
        }

        public override void _ExitTree()
        {
            roomSub?.Dispose();
            frameSub?.Dispose();
        }

        /// <summary>
        /// 同じ絵 の弾 をまとめて 1 枚 に。
        /// </summary>
        sealed class Layer
        {
            const int InitialCapacity = 1024;

            readonly QuadMesh quad;
            readonly Vector2 texture;
            int count;

            public MultiMeshInstance2D Node { get; }

            public Layer(Texture2D tex)
            {
                texture = tex.GetSize();
                quad = new QuadMesh { Size = texture };

                Node = new MultiMeshInstance2D
                {
                    Texture = tex,
                    Multimesh = new MultiMesh
                    {
                        TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
                        Mesh = quad,
                        InstanceCount = InitialCapacity,
                        VisibleInstanceCount = 0,
                    },
                };
            }

            /// <summary>盤面 に対する大きさ を決める。px を直書き しない</summary>
            public void Resize(float zoom) => quad.Size = texture * zoom;

            public void Begin(int upperBound)
            {
                count = 0;
                Grow(upperBound);
            }

            public void Put(Vector2 at, float rotation)
            {
                Node.Multimesh.SetInstanceTransform2D(count, new Transform2D(rotation, at));
                count++;
            }

            public void End() => Node.Multimesh.VisibleInstanceCount = count;

            public void Hide() => Node.Multimesh.VisibleInstanceCount = 0;

            /// <summary>
            /// 足りなければ倍 にする。
            /// </summary>
            void Grow(int need)
            {
                int capacity = Node.Multimesh.InstanceCount;
                if (need <= capacity)
                {
                    return;
                }

                while (capacity < need)
                {
                    capacity *= 2;
                }

                // `InstanceCount` を書くと中身 が捨てられる。 直後 の輪 が
                // 全部 書き直すので問題 にならないが、順番 を入れ替えないこと
                Node.Multimesh.InstanceCount = capacity;
                Node.Multimesh.VisibleInstanceCount = 0;
            }
        }
    }
}
