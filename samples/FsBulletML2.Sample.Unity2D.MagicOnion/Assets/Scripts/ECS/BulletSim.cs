using Unity.Entities;

/// <summary>
/// 弾 1 発 の状態。
/// </summary>
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
