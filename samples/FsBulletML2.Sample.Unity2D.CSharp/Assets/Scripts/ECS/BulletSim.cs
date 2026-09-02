using Unity.Entities;
using UnityEngine;
using FsBulletML2;
using Microsoft.FSharp.Core;
using BulletType = FsBulletML2.DTD.BulletType;

/// <summary>
/// Managed BulletML object stored on an ECS entity. X/Y are plain floats (no Transform).
/// Not Burst-compatible: IBulletmlObject is a class with FSharpOption tasks.
/// </summary>
public class BulletSim : IComponentData, Processable.IBulletmlObject
{
    public Entity Entity;
    public bool Root;
    public BulletKind Kind;

    public float X { get; set; }
    public float Y { get; set; }
    public float AccelerationX { get; set; }
    public float AccelerationY { get; set; }
    public bool BulletRoot { get; set; }
    public BulletType BulletType { get; set; }
    public float Dir { get; set; }
    public bool IsBullet { get; set; }
    public DTD.ShootingDirection ShootingDirection { get; set; }
    public float Speed { get; set; }
    public FSharpOption<Processable.BulletmlTask> Task { get; set; }
    public bool Used { get; set; }

    public void Init()
    {
        Root = false;
        Used = true;
        BulletRoot = false;
        AccelerationX = 0;
        AccelerationY = 0;
        Speed = 0;
        Dir = 0;
        IsBullet = true;

        Monad.OptionExtentions.Action<Processable.BulletmlTask>(Task,
            x =>
            {
                x.Init(BulletRunner.envOfGlobal(this));
                return;
            },
            _ =>
            {
                return;
            });
    }

    public void Vanish()
    {
        Used = false;
    }

    public float GetAimDir()
    {
        return Mathf.Atan2(
            Processable.BulletMLManager.GetPlayerPosX() - X,
            Processable.BulletMLManager.GetPlayerPosY() - Y);
    }

    // 産まれる弾の位置から見た向き。GetNewBullet は
    // BulletEntityFactory.SpawnChild(this) で作り、あちらは parent.X / parent.Y
    // をそのまま渡すので、産まれた弾は撃った側と同じ場所に居る。
    // よって撃った側と同じ値になる
    public float GetSpawnAimDir()
    {
        return GetAimDir();
    }

    public float GetSpawnEnemyAimDir()
    {
        return GetEnemyAimDir();
    }

    public float GetEnemyAimDir()
    {
        var enemy = BulletEcsRuntime.Enemy;
        if (enemy == null)
        {
            return 0f;
        }

        var p = enemy.transform.position;
        return Mathf.Atan2(p.x - X, p.y - Y);
    }

    public Processable.IBulletmlObject GetNewBullet()
    {
        BulletRoot = true;
        return BulletEntityFactory.SpawnChild(this);
    }

    public void SetTask(FSharpOption<Processable.BulletmlTask> bulletmlTask)
    {
        Task = bulletmlTask;
    }
}
