using FsBulletML2;
using FsBulletML2.MonoGame;
using Microsoft.FSharp.Core;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    /// <summary>
    /// 弾幕を読む段（<c>Runner.Load</c>）に渡すもの。
    ///
    /// 読む段は <c>Env</c> を取らない —— 木を組むときに読むのは乱数と
    /// ランクだけで、aim 4 本 は撃つ弾ごとの位置がまだ無いので読まれない。
    public static class FrontLoad
    {
        public static readonly FSharpFunc<Unit, float> Rand =
            FuncConvert.FromFunc<float>(BulletmlLoad.loadRand);

        public static float Rank() => BulletmlLoad.loadRank();
    }
}
