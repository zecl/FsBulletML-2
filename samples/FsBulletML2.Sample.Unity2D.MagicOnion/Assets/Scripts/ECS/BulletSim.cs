using Unity.Entities;

/// <summary>
/// 弾 1 発 の状態。<b>ここが、写した元 との違いの芯。</b>
///
/// 元（<c>FsBulletML2.Sample.Unity2D.CSharp</c>）では、この class が
/// 弾幕エンジンを毎コマ 呼んで自分で進んでいた ——
/// <c>Driver.Step</c> / <c>Runner.NewRoot</c> / <c>BulletRun</c> / <c>Motion</c>。
///
/// <b>いまは受け取るだけ。</b> 進めるのはサーバーで、ここに残ったのは
/// 「どこに居て、どちらを向いているか」だけ。
/// BulletML の語 も F# の型 も 1 つ も出てこない。
///
/// <b>それでも managed component のまま置いてある。</b> 中身が数だけなので
/// <c>struct</c> の <c>IComponentData</c> にもできるが、
/// <c>BulletSimulationSystem</c> が <c>GetComponentObject</c> で引いていて、
/// **そこを替えると触る面 がもう 1 つ 増える。** 速さで困ったら替える（E1.5）。
/// </summary>
// sealed にしておく。**TypeManager は sealed でない class の中を辿らない**
public sealed class BulletSim : IComponentData
{
    public Entity Entity;

    /// <summary>サーバーが付けた番号。<b>コマをまたいで同じ弾は同じ番号</b></summary>
    public int Id;

    public BulletKind Kind;

    public float X;
    public float Y;

    /// <summary>向き。度。真上 が 0 で時計回り</summary>
    public float Dir;

    /// <summary>まだ居るか。<b>このコマの並びに出てこなかったら false</b></summary>
    public bool Used = true;
}
