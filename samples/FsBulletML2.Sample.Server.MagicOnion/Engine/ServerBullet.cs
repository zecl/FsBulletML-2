using System;
using System.Collections.Generic;
using FsBulletML2;
using FsBulletML2.Front;
using BulletType = FsBulletML2.DTD.BulletType;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// サーバー側 の弾 1 発。Unity の <c>BulletSim</c> から、
    /// 描くことと当たり判定 を抜いたもの。
    /// 1/100 と Y の反転 はここに在る。 エンジンが返すのは差分で、
    /// 位置と係数はフロントの持ち物（<c>Driver</c> の但し書き）——
    /// サーバーがフロントの立場に立っているので、ここが持つ。
    public sealed class ServerBullet
    {
        readonly RoomEnv env;

        public ServerBullet(RoomEnv env, int id, bool isRoot, BulletType kind)
        {
            this.env = env;
            Id = id;
            IsRoot = isRoot;
            BulletType = kind;
            IsBullet = !isRoot;
        }

        public int Id { get; }

        /// <summary>撃つ側（＝敵 の本体）。盤面 の外へ出ても捨てない</summary>
        public bool IsRoot { get; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Dir { get; set; }
        public float Speed { get; set; }
        public float AccelerationX { get; set; }
        public float AccelerationY { get; set; }

        public bool IsBullet { get; private set; }

        public bool Used { get; private set; } = true;

        /// <summary>
        /// 敵の弾か自機の弾か。既定値を入れておくこと。
        ///
        /// F# の判別共用体は参照型なので、入れ忘れた既定は 0 ではなく null。
        /// 型は合うのでコンパイルは通り、走らせて初めて落ちる。
        /// </summary>
        public BulletType BulletType { get; }

        BulletRun? run;

        /// <summary>根から始める。撃たれた弾ではない</summary>
        public void SetScript(BulletmlScript script)
        {
            run = script != null
                ? (IsBullet ? Runner.NewShot(BulletType, script) : Runner.NewRoot(BulletType, script))
                : (BulletRun?)null;
        }

        /// <summary>撃たれた弾を、エンジンから受け取った実行状態で始める</summary>
        public void SetRun(BulletRun r)
        {
            run = r;
        }

        /// <summary>
        /// 1 コマ 進める。撃たれた弾は <paramref name="spawn"/> へ渡す。
        ///
        /// 走らせ直すかどうかは、ここで決まる。 エンジンの但し書きに
        /// 「呼ぶか呼ばないかで乱数の並びが変わる」と在る ——
        /// サーバーが権威 を持つので、その決めごとをサーバー側 に固定した。
        /// 全 top が終わったら最初から走らせ直す（＝弾幕が輪 になる）。
        public void Step(Action<ServerBullet, BulletRun> spawn)
        {
            if (!run.HasValue)
            {
                return;
            }

            var rn = run.Value;

            // 物理量はフロントが持っている。毎コマ 入れ直す。
            // 名前付き引数で書く —— 位置ずれは落ちるが、値の取り違えは落ちない
            var motion = new Motion(
                pos: new Domain.Vec2(X, Y),
                speed: Speed,
                dir: Dir,
                accel: new Domain.Vec2(AccelerationX, AccelerationY));

            var f = Driver.Step(env, RoomEnv.Space, RoomEnv.Origin, rn, motion);

            var after = f.Run.Motion;
            Speed = after.Speed;
            Dir = after.Dir;
            AccelerationX = after.Accel.X;
            AccelerationY = after.Accel.Y;
            X += f.Delta.X / 100f;
            Y -= f.Delta.Y / 100f;

            foreach (var child in f.Spawned)
            {
                spawn(this, child);
            }

            if (f.Vanished || f.Retired)
            {
                Used = false;
            }

            // 走らせ直しの Env は、位置を更新したあとの自分から組む
            run = f.Finished ? Driver.Restart(env, f.Run) : f.Run;
        }

        /// <summary>撃たれた弾を、親の位置から作る</summary>
        public static ServerBullet Shoot(RoomEnv env, int id, ServerBullet parent, BulletRun child)
        {
            var sim = new ServerBullet(env, id, isRoot: false, parent.BulletType);
            sim.SetRun(child);

            var motion = child.Motion;
            sim.X = motion.Pos.X;
            sim.Y = motion.Pos.Y;
            sim.Dir = motion.Dir;
            sim.Speed = motion.Speed;
            return sim;
        }
    }
}
