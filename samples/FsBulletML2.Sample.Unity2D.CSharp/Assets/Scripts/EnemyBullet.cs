// Leftover GameObject bullet. Spawned shots are ECS entities (BulletSim).
// Kept so the prefab still compiles if opened; nothing instantiates this at runtime.
using UnityEngine;
using FsBulletML2;
using R3;
using R3.Triggers;
using BulletType = FsBulletML2.DTD.BulletType;

public class EnemyBullet : BaseBullet
{
    public EnemyBullet()
        : base()
    {
        this.Init();
        this.Root = true;

        this.IsBullet = true;
        this.BulletRoot = true;
        this.BulletType = BulletType.Enemy;
    }

    void Start()
    {
        Observable.EveryUpdate(destroyCancellationToken)
            .Subscribe(_ =>
            {
                if (!this.Root && this.BulletRoot && !this.Used)
                {
                    InstanceManager.Destroy(gameObject);
                    return;
                }

                var p = this.transform.position;
                if (p.x < 0 || p.x > 4.8 || p.y < -6.4 || p.y > 0)
                {
                    this.Used = false;
                    InstanceManager.Destroy(gameObject);
                }
            });

        this.OnTriggerEnter2DAsObservable()
            .Where(col => col.gameObject.tag == "Player")
            .Subscribe(_ =>
            {
                if (!this.Root)
                {
                    InstanceManager.Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    public override GameObject GetBulletPrefubInstance()
    {
        return null;
    }
}
