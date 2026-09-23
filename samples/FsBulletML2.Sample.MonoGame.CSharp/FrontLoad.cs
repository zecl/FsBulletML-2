using FsBulletML2;
using FsBulletML2.MonoGame;
using Microsoft.FSharp.Core;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    /// <summary>
    /// 弾幕を読む段（<c>Runner.Load</c>）に渡すもの。
    /// </summary>
    public static class FrontLoad
    {
        public static readonly FSharpFunc<Unit, float> Rand =
            FuncConvert.FromFunc<float>(BulletmlLoad.loadRand);

        public static float Rank() => BulletmlLoad.loadRank();
    }
}
