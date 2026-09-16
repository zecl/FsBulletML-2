using Unity.Entities;

/// <summary>
/// 弾 1 発 の状態。ここが、写した元 との違いの芯。
///
/// 元（<c>FsBulletML2.Sample.Unity2D.CSharp</c>）では、この class が
/// 弾幕エンジンを毎コマ 呼んで自分で進んでいた ——
/// <c>Driver.Step</c> / <c>Runner.NewRoot</c> / <c>BulletRun</c> / <c>Motion</c>。
public sealed class BulletSim : IComponentData
{
    public Entity Entity;

    /// <summary>サーバーが付けた番号。コマをまたいで同じ弾は同じ番号</summary>
    public int Id;

    public BulletKind Kind;

    public float X;
    public float Y;

    /// <summary>向き。度。真上 が 0 で時計回り</summary>
    public float Dir;

    /// <summary>まだ居るか。このコマの並びに出てこなかったら false</summary>
    public bool Used = true;
}
