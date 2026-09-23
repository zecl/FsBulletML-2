using System;
using UnityEngine;
using R3;
using R3.Triggers;

/// <summary>
/// 撃つ側。
/// </summary>
public class Enemy : MonoBehaviour
{
    // 名前を変えない。 prefab の YAML は field の名前で値を引く
    // （宣言している型 ではない）ので、名前さえ同じなら親 から移しても繋がる
    [SerializeField]
    protected GameObject bulletObject;

    public GameObject BulletPrefab => bulletObject;

    public GameObject BombType;

    public readonly ReactiveProperty<int> BulletIndexRp = new(0);
    public readonly ReactiveProperty<int> LifeRp = new(2000);
    public readonly ReactiveProperty<string> BulletNameRp = new("");

    public int MaxLife = 2000;
    public bool isBomb = true;

    public int BulletIndex => BulletIndexRp.Value;
    public string BulletName => BulletNameRp.Value;

    public int Life
    {
        get => LifeRp.Value;
        set => LifeRp.Value = value;
    }

    static string[] Names => DanmakuState.Names.Value;

    void Start()
    {
        var update = Observable.EveryUpdate(destroyCancellationToken);

        BulletIndexRp
            .Subscribe(_ => ApplyPattern())
            .AddTo(this);

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

        // 繋がるのを待つ。
        DanmakuState.Names
            .Where(names => names.Length > 0)
            .Take(1)
            .Subscribe(_ => ApplyPattern())
            .AddTo(this);

        this.OnTriggerEnter2DAsObservable()
            .Subscribe(_ => HitByPlayerBullet())
            .AddTo(this);
    }

    public void HitByPlayerBullet()
    {
        LifeRp.Value -= 1;
    }

    public void Next()
    {
        var n = Names.Length;
        if (n == 0)
        {
            return;
        }

        BulletIndexRp.Value = (BulletIndexRp.Value + 1) % n;
    }

    public void Prev()
    {
        var n = Names.Length;
        if (n == 0)
        {
            return;
        }

        BulletIndexRp.Value = (BulletIndexRp.Value + n - 1) % n;
    }

    /// <summary>
    /// 走らせる弾幕を頼む。
    /// </summary>
    void ApplyPattern()
    {
        var names = Names;
        if (names.Length == 0)
        {
            return;
        }

        int i = ((BulletIndexRp.Value % names.Length) + names.Length) % names.Length;
        BulletNameRp.Value = names[i];
        LifeRp.Value = MaxLife;

        DanmakuClient.Instance?.Switch(i.ToString());
    }

    void OnDestroy()
    {
        BulletIndexRp.Dispose();
        BulletNameRp.Dispose();
        LifeRp.Dispose();
    }
}
