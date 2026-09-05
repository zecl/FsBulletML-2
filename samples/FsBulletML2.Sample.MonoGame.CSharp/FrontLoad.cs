using FsBulletML2;
using FsBulletML2.MonoGame;
using Microsoft.FSharp.Core;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    /// <summary>
    /// 弾幕を読む段（<c>Runner.Load</c>）に渡すもの。
    ///
    /// <b>読む段は <c>Env</c> を取らない</b> —— 木を組むときに読むのは乱数と
    /// ランクだけで、aim 4 本 は撃つ弾ごとの位置がまだ無いので読まれない。
    ///
    /// <b>関数値は 1 個 だけ作って使い回す。</b> 呼ぶたびに
    /// <c>FuncConvert</c> するとその回数だけヒープを踏む
    /// （同じ理由で Unity C# サンプルの <c>FrontEnv.RandFunc</c> も static）。
    /// F# の <c>BulletmlLoad.loadRand</c> は C# からはメソッドに見えるので、
    /// ここで 1 度 だけ関数値に包む。
    /// </summary>
    public static class FrontLoad
    {
        public static readonly FSharpFunc<Unit, float> Rand =
            FuncConvert.FromFunc<float>(BulletmlLoad.loadRand);

        public static float Rank() => BulletmlLoad.loadRank();
    }
}
