using System;
using FsBulletML2;
using FsBulletML2.Front;
using Microsoft.FSharp.Core;

namespace FsBulletML2.Sample.Server.MagicOnion.Engine
{
    /// <summary>
    /// 部屋 1 つ から見た世界。<b>static を 1 つ も持たない。</b>
    ///
    /// <b>ここが、部屋 を 2 つ 同時に走らせられるかどうかの分かれ目。</b>
    /// 同梱のフロント 4 本（MonoGame 2 本 / Unity2D 2 本）は、
    /// 乱数・ランク・自機 を <c>BulletMLManager</c> から引いている ——
    /// あれは <c>static let mutable</c> が 1 つ で、部屋 が混ざる。
    ///
    /// <b>ただしそれはフロントの選びであって、エンジンの要求ではない。</b>
    /// <c>Driver.Step</c> は <c>IFrontEnv</c> を引数で受け取るので、
    /// 部屋ごとに 1 個 持てば済む。**この class は 1 度 も
    /// <c>BulletMLManager</c> を通らない**（門 が数えている）。
    /// </summary>
    public sealed class RoomEnv : IFrontEnv
    {
        /// <summary>
        /// <c>Env.Rand</c> に入れる F# の関数値。<b>1 個 だけ作って使い回す。</b>
        /// 毎コマ <c>FuncConvert</c> すると弾数 × コマ数 だけヒープを踏む
        /// （実測 48 B / 回）。
        /// </summary>
        readonly FSharpFunc<Unit, float> rand;

        readonly Random random;

        float playerX;
        float playerY;

        public RoomEnv(int seed, float rank, float enemyX, float enemyY, float playerX, float playerY)
        {
            random = new Random(seed);
            // **同梱サンプルと同じ刻み**（小数 4 桁 に丸める）。
            // 揃えないと、同じ弾幕でも並びが微妙に違うものが出る
            rand = FuncConvert.FromFunc<float>(
                () => (float)(Math.Round(random.NextDouble() * 10000) / 10000));
            Rank = rank;
            EnemyX = enemyX;
            EnemyY = enemyY;
            this.playerX = playerX;
            this.playerY = playerY;
        }

        /// <summary>Y は上向き。<b>Unity2D サンプルと同じ空間</b>で走らせる</summary>
        public const Space Space = FsBulletML2.Front.Space.YUp;

        /// <summary>産まれた弾は撃った側と同じ場所。<b>Unity2D サンプルと同じ</b></summary>
        public const SpawnOrigin Origin = FsBulletML2.Front.SpawnOrigin.AtShooter;

        public float Rank { get; }

        public float EnemyX { get; }

        public float EnemyY { get; }

        /// <summary>読む段（<c>Runner.Load</c>）にも同じ乱数を渡す</summary>
        public FSharpFunc<Unit, float> LoadRand => rand;

        /// <summary>
        /// client が自機の位置を 1 度 でも送ってきたか。
        ///
        /// <b>送ってくるまで当たり判定 をしない。</b> 既定の位置のまま判定すると、
        /// 誰も居ない場所で弾が消え続ける ——
        /// client からは「弾が途中で消える」としか見えない。
        /// </summary>
        public bool PlayerKnown { get; private set; }

        public float PlayerX => playerX;

        public float PlayerY => playerY;

        public void SetPlayer(float x, float y)
        {
            playerX = x;
            playerY = y;
            PlayerKnown = true;
        }

        FSharpFunc<Unit, float> IFrontEnv.Rand => rand;

        float IFrontEnv.Rank => Rank;

        float IFrontEnv.PlayerX => playerX;

        float IFrontEnv.PlayerY => playerY;

        /// <summary>
        /// <b>敵は 1 体 しか居ない</b>ので一覧を持たない。
        /// 口 が一覧を要求しないのはこのため（<c>IFrontEnv</c> の但し書き）。
        /// </summary>
        public bool TryTargetFrom(float x, float y, out float ex, out float ey)
        {
            ex = EnemyX;
            ey = EnemyY;
            return true;
        }

        /// <summary><b>撃った側と同じ相手。</b> このフロントは撃った側と同じ場所に弾を作る</summary>
        public bool TrySpawnTargetFrom(float x, float y, out float ex, out float ey)
            => TryTargetFrom(x, y, out ex, out ey);
    }
}
