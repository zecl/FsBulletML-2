using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using FsBulletML2;
using Microsoft.FSharp.Core;
using R3;
using R3.Triggers;
using BulletType = FsBulletML2.DTD.BulletType;

public class Enemy : BaseBullet
{
    public GameObject BombType;

    private static List<BulletmlInfo> bullets;
    public readonly ReactiveProperty<int> BulletIndexRp = new(0);
    public readonly ReactiveProperty<int> LifeRp = new(2000);
    public readonly ReactiveProperty<string> BulletNameRp = new("");
    private BulletmlInfo BulletmlInfo { get; set; }
    private BulletSim RootSim { get; set; }
    public int MaxLife = 2000;
    public bool isBomb = true;

    public int BulletIndex => BulletIndexRp.Value;
    public string BulletName => BulletNameRp.Value;
    public int Life
    {
        get => LifeRp.Value;
        set => LifeRp.Value = value;
    }

    public Enemy()
        : base()
    {
        this.BulletType = BulletType.Enemy;
        this.IsBullet = false;
        this.Used = true;
    }

    void Start()
    {
        bullets = FsBulletML2.Bullets.Dsl.All.bullets.ToList();
        var update = Observable.EveryUpdate(destroyCancellationToken);

        BulletIndexRp
            .Subscribe(_ => ApplyPattern())
            .AddTo(this);

        BulletIndexRp
            .Select(i => bullets[i].Name)
            .Subscribe(name => BulletNameRp.Value = name)
            .AddTo(this);

        var shootOnPattern = BulletIndexRp
            .Select(_ => update.Take(1));

        var shootOnFinish = update
            .Where(_ => IsFinish());

        shootOnPattern
            .Switch()
            .Merge(shootOnFinish)
            .Subscribe(_ => Shoot());

        LifeRp
            .Pairwise()
            .Where(p => p.Current == p.Previous - 1)
            .Subscribe(_ =>
            {
                if (isBomb) Bomb.GenerateBomb(BombType, transform.position);
            })
            .AddTo(this);

        LifeRp
            .Where(x => x <= 0)
            .Subscribe(_ => Next())
            .AddTo(this);

        update
            .Where(_ => Input.GetKeyDown(KeyCode.Return))
            .Subscribe(_ => Next());

        this.OnTriggerEnter2DAsObservable()
            .Subscribe(_ => HitByPlayerBullet())
            .AddTo(this);
    }

    public void HitByPlayerBullet()
    {
        LifeRp.Value -= 1;
    }

    public override GameObject GetBulletPrefubInstance()
    {
        return null;
    }

    public void Shoot()
    {
        if (this.Used)
        {
            // 弾幕は撃つたびに読み直す。読む段の Env は aim を読まない
            // （撃つ弾ごとの位置がまだ無い）ので FrontEnv.Load を渡す
            var script = this.BulletmlInfo.Script(FrontEnv.Load());
            this.RootSim = BulletEntityFactory.SpawnEnemy(this.transform.position, script, root: true);
        }
    }

    /// <summary>
    /// 撃った弾幕がひと回りしたか。旧は BulletmlTask.Finish を見ていた。
    /// 新 API では Frame.Finished を弾が控えている（BulletSim.Finished）。
    /// </summary>
    private bool IsFinish()
    {
        if (this.RootSim == null)
        {
            return false;
        }

        if (this.RootSim.Script == null)
        {
            return false;
        }

        if (this.RootSim.Finished)
        {
            BulletEntityFactory.Destroy(this.RootSim);
            this.RootSim = null;
            return true;
        }

        return false;
    }

    public void Next()
    {
        var n = bullets.Count;
        BulletIndexRp.Value = (BulletIndexRp.Value + 1) % n;
    }

    public void Prev()
    {
        var n = bullets.Count;
        BulletIndexRp.Value = (BulletIndexRp.Value + n - 1) % n;
    }

    private void ApplyPattern()
    {
        DestroyEnemyBullet();
        var bulletmlInfo = bullets[BulletIndexRp.Value];
        BulletmlInfo = bulletmlInfo;
        LifeRp.Value = MaxLife;
    }

    private void DestroyEnemyBullet()
    {
        BulletEntityFactory.DestroyAllEnemy();
        this.RootSim = null;
    }

    void OnDestroy()
    {
        BulletIndexRp.Dispose();
        BulletNameRp.Dispose();
        LifeRp.Dispose();
    }
}
