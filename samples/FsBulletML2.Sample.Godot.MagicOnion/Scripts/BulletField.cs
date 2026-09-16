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

                // 向き を当てる。 弾 の絵 は 11x23 の縦長 で、先 が上 ——
                // 回さないと全部 が真上 を向いたまま 横 へ飛ぶ。
                //
                // 口 の ToDir は「度」と書いてあるが、サーバーはエンジン の
                // ラジアン をそのまま渡している。 FromDir の戻りはラジアン。
                // DegToRad すると全部 真上 に潰れて、Unity と見え方が割れる。
                // Godot は Y が下向き なので、正 の回転 が画面 で時計回り ——
                // 裏返す必要 は無い。
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

            // 届いた数 と 描いた数 を並べて出す。 絵 がまばら に見えたとき、
            // 「client が落とした」「重なっている」「盤面 の外 へ出ている」の
            // 3 通り は目 では割れない
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
        ///
        /// <c>InstanceCount</c> を毎コマ 動かさない。 あれ は確保 し直しなので、
        /// 弾数 が上下 するたび に払うことになる —— 上限 を確保 して
        /// <c>VisibleInstanceCount</c> で切る。足りなくなったら倍 にする（減らさない）。
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
            /// 足りなければ倍 にする。減らさない。
            /// 弾数 は 1 秒 のうち に何度 も上下 するので、縮めると縮めた端 から
            /// また確保 することになる。
            ///
            /// 上限 は「そのコマ の弾 全部」で取る。 敵 と自機 の割合 は
            /// コマ ごとに動くので、片方 に寄ったときに足りなくなる形 を作らない。
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
