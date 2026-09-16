using System;
using UnityEngine;
using R3;
using R3.Triggers;

/// <summary>
/// 撃つ側。もう何も撃たない。
///
/// 元（<c>FsBulletML2.Sample.Unity2D.CSharp</c>）では、この面 が
/// 同梱の 176 本 を <c>All.bullets</c> から読んで、<c>Runner.Load</c> で
/// 台本 に起こし、<c>BulletEntityFactory.SpawnEnemy</c> で撃っていた。
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

        // 繋がるのを待つ。 一覧 はサーバーから来るので、
        // 立ち上がった時点ではまだ 0 本。
        //
        // 毎コマ「まだか」を見ない。 元 は
        // `update.Where(_ => ... Names.Length > 0).Take(1)` で、
        // 建つ順 が外から見えないのが理由 だった。
        // いまは置き場（DanmakuState）が建つ順 に依らないので、購読するだけ
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
    /// 走らせる弾幕を頼む。番号 で頼む。
    /// 一覧 の順 はサーバーが返したものなので、番号 が そのまま通じる
    /// （名前は日本語の説明文 なので、そのまま送ると往復 に乗る字が増える）。
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
