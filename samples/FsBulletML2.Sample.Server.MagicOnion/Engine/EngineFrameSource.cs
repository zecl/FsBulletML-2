using System;
using System.Collections.Generic;
using FsBulletML2;
using FsBulletML2.Sample.MagicOnion.Shared;
using FsBulletML2.Sample.Server.MagicOnion.Rooms;
using BulletType = FsBulletML2.DTD.BulletType;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 弾幕エンジンを 1 部屋 ぶん走らせる。<b>この class だけが F# を呼ぶ。</b>
    ///
    /// <b>1 本 の Task からしか呼ばれない</b>のが不変条件（<see cref="Room"/>）。
    /// 排他を持たないのはそのため。
    /// </summary>
    public sealed class EngineFrameSource : IFrameSource
    {
        /// <summary>
        /// 抱える弾の上限。<b>超えたぶんは捨てる。</b>
        ///
        /// エンジンは撃った弾を値で返しきり、<b>フロントが「撃つのを断る」口 は
        /// 無い</b>（<c>Frame.Spawned</c> の但し書き）。捨てるのはこちらの都合。
        /// </summary>
        public const int MaxBullets = 4000;

        /// <summary>
        /// 自機 の弾 が 1 回 の撃ちで出る位置。<b>Unity2D サンプルと同じ。</b>
        /// 撃つ口 は 1 本 だが、同梱の 2way は**左右 2 発** 出る。
        /// </summary>
        static readonly (float Dx, float Dy)[] ShotOffsets =
        {
            (-0.1f, 0.1f),
            (0.1f, 0.1f),
        };

        readonly RoomEnv env;
        readonly BulletmlScript script;

        /// <summary>
        /// 自機 の弾幕。<b>1 本 につき 1 回 だけ読む。</b>
        /// 撃つたびに読み直すと、木 を組む段でまた乱数 を引いて並びが変わる
        /// （Unity2D サンプルの <c>Player.Awake</c> も 1 回 だけ読んでいた）。
        /// </summary>
        readonly BulletmlScript[] shotScripts;

        readonly List<ServerBullet> live = new List<ServerBullet>();
        readonly List<ServerBullet> born = new List<ServerBullet>();

        int nextId;
        ServerBullet root;

        /// <summary>このコマ の当たり。<b>Step の頭 で 0 に戻す</b></summary>
        int playerHits;
        int enemyHits;

        public EngineFrameSource(BulletmlInfo info, int seed)
        {
            env = new RoomEnv(
                seed,
                // **ランク 0。** 同梱サンプル（BulletFunctions.GetRank）と同じ
                rank: 0f,
                enemyX: Field.OriginX,
                enemyY: Field.OriginY,
                // 自機 の既定。client が SetPlayer を送ってくるまでここを狙う
                playerX: (Field.MinX + Field.MaxX) * 0.5f,
                playerY: Field.MinY + 0.8f);

            script = Runner.Load(env.LoadRand, env.Rank, info.Bulletml);

            // 自機 の弾。**同梱 の PlayerBullet は Bulletml（DTD の木）を直に持つ**ので
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

        /// <summary>
        /// 自機 の弾 を撃つ。<b>1 回 の呼びで 2 発。</b>
        ///
        /// <b>撃たれた弾（<c>NewShot</c>）として始める。</b> 根（<c>NewRoot</c>）
        /// との違いは <c>Frame.Retired</c> だけだが、自機 の弾は撃ったあと
        /// 退場してよい側 なので、こちらが正しい。
        ///
        /// **上限 に当たっていたら撃たない。** 撃ってから捨てると、
        /// 弾が 1 コマ だけ出て消える形になる。
        /// </summary>
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

                // **BulletType と IsBullet は SetScript の前 に決まっている。**
                // Core へは毎コマ 渡らないので、後 から変えても効かない
                shot.SetScript(shotScripts[i % shotScripts.Length]);
                born.Add(shot);
            }
        }

        public FrameSnapshot Step()
        {
            playerHits = 0;
            enemyHits = 0;

            // **`born` をここで空 にしない。** 撃ち（`Shoot`）はコマの頭 で、
            // つまり **この関数に入る前** に積まれる。頭 で払うと、
            // 撃った弾が 1 発 も出ない（実際にそう書いて踏んだ）。
            // 払うのは並びに載せ終えた後。

            // **添字 で回す。** 産まれた弾はこのコマでは回さない
            // （エンジンの決めと同じ。産まれた弾は次のコマから）
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

            // **撃つ側 が消えたら建て直す。** 建て直さないと、
            // 弾が出きったところで部屋 が永久に空 になる
            if (root == null || !root.Used)
            {
                SpawnRoot();
            }

            var dtos = new BulletDto[live.Count];
            for (int i = 0; i < live.Count; i++)
            {
                var b = live[i];
                // **配る形 は整数。** 盤面 を 0..65535 に割る（Wire）——
                // float32 は MessagePack で 5 バイト固定 なので、
                // x / y / 向き の 3 本 で 弾 1 発 19.9 -> 13.8 バイト
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
        /// 死んだ弾を落とす。<b>撃つ側 は盤面 の外 でも落とさない</b> ——
        /// 落とすと、その場で建て直しが走って弾幕が頭 へ戻る。
        ///
        /// <b>当たり判定 をここに相乗りさせてある。</b> 弾を落とす判断は
        /// 「使い切った / 外へ出た / 当たった」の 3 つ しか無く、どれも
        /// 同じ 1 本 の走査で決まる。**判定 のためにループを足さない** ——
        /// 足すと弾数 に比例した走査が 2 本 になる。
        /// </summary>
        void Sweep()
        {
            // **自機 を 1 度 も知らされていないなら、被弾 は数えない。**
            // 既定の位置で判定すると、誰も居ない場所で弾が消え続ける
            bool judgePlayer = env.PlayerKnown;
            float px = env.PlayerX;
            float py = env.PlayerY;

            int w = 0;
            for (int i = 0; i < live.Count; i++)
            {
                var b = live[i];
                bool dead = !b.Used || (!b.IsRoot && Field.IsOutside(b.X, b.Y));

                // **撃つ側 は当たらない。** 根 は弾ではなく、弾を出す口
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

        /// <summary>2 点 が重なっているか。<b>平方根 を取らない</b></summary>
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

            // **読み直さない。** Unity2D サンプルの Enemy は台本を読み直して
            // 建て直すが、読む段でまた乱数 を引くので並びが変わる。
            // サーバーが権威 を持つので、**台本は 1 本 のまま使い回す**
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
