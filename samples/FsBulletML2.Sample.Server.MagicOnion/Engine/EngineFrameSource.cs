using System;
using System.Collections.Generic;
using FsBulletML2;
using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using BulletType = FsBulletML2.DTD.BulletType;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 弾幕エンジンを 1 部屋 ぶん走らせる。
    /// </summary>
    public sealed class EngineFrameSource : IFrameSource
    {
        /// <summary>
        /// 抱える弾の上限。
        /// 超えたぶんは捨てる。
        /// </summary>
        public const int MaxBullets = 4000;

        /// <summary>
        /// 自機 の弾 が 1 回 の撃ちで出る位置。Unity2D サンプルと同じ。
        /// 撃つ口 は 1 本 だが、同梱の 2way は左右 2 発 出る。
        /// </summary>
        static readonly (float Dx, float Dy)[] ShotOffsets =
        {
            (-0.1f, 0.1f),
            (0.1f, 0.1f),
        };

        readonly RoomEnv env;
        readonly BulletmlScript script;

        /// <summary>
        /// 自機 の弾幕。
        /// </summary>
        readonly BulletmlScript[] shotScripts;

        readonly List<ServerBullet> live = new List<ServerBullet>();
        readonly List<ServerBullet> born = new List<ServerBullet>();

        int nextId;
        ServerBullet root;

        /// <summary>このコマ の当たり。Step の頭 で 0 に戻す</summary>
        int playerHits;
        int enemyHits;

        public EngineFrameSource(BulletmlInfo info, int seed)
        {
            env = new RoomEnv(
                seed,
                // ランク 0。 同梱サンプル（BulletFunctions.GetRank）と同じ
                rank: 0f,
                enemyX: Field.OriginX,
                enemyY: Field.OriginY,
                // 自機 の既定。client が SetPlayer を送ってくるまでここを狙う
                playerX: (Field.MinX + Field.MaxX) * 0.5f,
                playerY: Field.MinY + 0.8f);

            script = Runner.Load(env.LoadRand, env.Rank, info.Bulletml);

            // 自機 の弾。同梱 の PlayerBullet は Bulletml（DTD の木）を直に持つので
            // BulletmlInfo を経ずに Runner.Load へ渡す
            shotScripts = new[]
            {
                Runner.Load(env.LoadRand, env.Rank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet),
                Runner.Load(env.LoadRand, env.Rank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayRightBullet),
            };

            Info = new RoomInfo
            {
                Name = info.Name,
                Fps = RoomLoop.Fps,
                Space = SpaceDto.YUp,
                MinX = Field.MinX,
                MaxX = Field.MaxX,
                MinY = Field.MinY,
                MaxY = Field.MaxY,
                OriginX = Field.OriginX,
                OriginY = Field.OriginY,
            };

            SpawnRoot();
        }

        public RoomInfo Info { get; }

        public void SetPlayer(float x, float y) => env.SetPlayer(x, y);

        /// <summary>自機の弾を撃つ。1 回の呼びで 2 発。撃たれた弾として始める。</summary>
        public void Shoot(float x, float y)
        {
            for (int i = 0; i < ShotOffsets.Length; i++)
            {
                if (live.Count + born.Count >= MaxBullets)
                {
                    return;
                }

                var shot = new ServerBullet(env, nextId++, isRoot: false, BulletType.Player)
                {
                    X = x + ShotOffsets[i].Dx,
                    Y = y + ShotOffsets[i].Dy,
                };

                // BulletType と IsBullet は SetScript の前 に決まっている。
                // Core へは毎コマ 渡らないので、後 から変えても効かない
                shot.SetScript(shotScripts[i % shotScripts.Length]);
                born.Add(shot);
            }
        }

        public FrameSnapshot Step()
        {
            playerHits = 0;
            enemyHits = 0;

            // `born` をここで空 にしない。
            for (int i = 0; i < live.Count; i++)
            {
                live[i].Step(Spawn);
            }

            Sweep();

            for (int i = 0; i < born.Count && live.Count < MaxBullets; i++)
            {
                live.Add(born[i]);
            }

            born.Clear();

            // 撃つ側 が消えたら建て直す。 建て直さないと、
            // 弾が出きったところで部屋 が永久に空 になる
            if (root == null || !root.Used)
            {
                SpawnRoot();
            }

            var dtos = new BulletDto[live.Count];
            for (int i = 0; i < live.Count; i++)
            {
                var b = live[i];
                // 配る形 は整数。
                dtos[i] = new BulletDto
                {
                    Id = b.Id,
                    X = Wire.ToGrid(b.X, Field.MinX, Field.MaxX),
                    Y = Wire.ToGrid(b.Y, Field.MinY, Field.MaxY),
                    Dir = Wire.ToDir(b.Dir),
                    Kind = (byte)(Equals(b.BulletType, BulletType.Player) ? 1 : 0),
                };
            }

            return new FrameSnapshot(dtos, playerHits, enemyHits);
        }

        void Spawn(ServerBullet parent, BulletRun child)
        {
            born.Add(ServerBullet.Shoot(env, nextId++, parent, child));
        }

        /// <summary>
        /// 死んだ弾を落とす。撃つ側は盤面の外でも落とさない。落とすと弾幕が頭へ戻る。
        /// </summary>
        void Sweep()
        {
            // 自機 を 1 度 も知らされていないなら、被弾 は数えない。
            // 既定の位置で判定すると、誰も居ない場所で弾が消え続ける
            bool judgePlayer = env.PlayerKnown;
            float px = env.PlayerX;
            float py = env.PlayerY;

            int w = 0;
            for (int i = 0; i < live.Count; i++)
            {
                var b = live[i];
                bool dead = !b.Used || (!b.IsRoot && Field.IsOutside(b.X, b.Y));

                // 撃つ側 は当たらない。 根 は弾ではなく、弾を出す口
                if (!dead && !b.IsRoot)
                {
                    if (Equals(b.BulletType, BulletType.Player))
                    {
                        if (Near(b.X, b.Y, Field.OriginX, Field.OriginY, Field.BulletRadius + Field.EnemyRadius))
                        {
                            enemyHits++;
                            dead = true;
                        }
                    }
                    else if (judgePlayer
                             && Near(b.X, b.Y, px, py, Field.BulletRadius + Field.PlayerRadius))
                    {
                        playerHits++;
                        dead = true;
                    }
                }

                if (dead)
                {
                    if (ReferenceEquals(b, root))
                    {
                        root = null;
                    }

                    continue;
                }

                live[w++] = b;
            }

            live.RemoveRange(w, live.Count - w);
        }

        /// <summary>2 点 が重なっているか。平方根 を取らない</summary>
        static bool Near(float x, float y, float tx, float ty, float reach)
        {
            float dx = x - tx;
            float dy = y - ty;
            return (dx * dx) + (dy * dy) <= reach * reach;
        }

        void SpawnRoot()
        {
            root = new ServerBullet(env, nextId++, isRoot: true, BulletType.Enemy)
            {
                X = Field.OriginX,
                Y = Field.OriginY,
            };

            // 読み直さない。
            root.SetScript(script);
            live.Add(root);
        }
    }

    /// <summary>同梱弾幕を走らせる工場</summary>
    public sealed class EngineFrameSourceFactory : IFrameSourceFactory
    {
        public IFrameSource Create(JoinRequest request)
        {
            var info = Catalog.Resolve(request?.Bulletml);
            int seed = request == null || request.Seed == 0
                ? Environment.TickCount
                : request.Seed;
            return new EngineFrameSource(info, seed);
        }

        public string[] List() => Catalog.Names();
    }
}
