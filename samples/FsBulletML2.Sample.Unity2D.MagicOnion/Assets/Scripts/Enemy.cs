using System;
using UnityEngine;
using R3;
using R3.Triggers;

/// <summary>
/// 撃つ側。<b>もう何も撃たない。</b>
///
/// 元（<c>FsBulletML2.Sample.Unity2D.CSharp</c>）では、この面 が
/// 同梱の 176 本 を <c>All.bullets</c> から読んで、<c>Runner.Load</c> で
/// 台本 に起こし、<c>BulletEntityFactory.SpawnEnemy</c> で撃っていた。
///
/// <b>いまやるのは「どれを走らせるか」を頼むことだけ。</b>
/// 弾幕の一覧 も、走らせることも、サーバーが持っている。
///
/// <b>元 が継いでいた <c>BaseBullet</c> は落とした。</b> あれは
/// GameObject 側 の弾の親 で、中身は丸ごとエンジンだった。
/// </summary>
public class Enemy : MonoBehaviour
{
    // **名前を変えない。** prefab の YAML は field の名前で値を引く
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

    static string[] Names => DanmakuClient.Instance != null
        ? DanmakuClient.Instance.Names
        : Array.Empty<string>();

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

        // **繋がるのを待つ。** 一覧 はサーバーから来るので、
        // 立ち上がった時点ではまだ 0 本。名前が出るのは繋がってから
        update
            .Where(_ => BulletNameRp.Value.Length == 0 && Names.Length > 0)
            .Take(1)
            .Subscribe(_ => ApplyPattern());

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
    /// 走らせる弾幕を頼む。<b>番号 で頼む。</b>
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

        if (DanmakuClient.Instance != null)
        {
            DanmakuClient.Instance.Switch(i.ToString());
        }
    }

    void OnDestroy()
    {
        BulletIndexRp.Dispose();
        BulletNameRp.Dispose();
        LifeRp.Dispose();
    }
}
