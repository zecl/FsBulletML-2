using UnityEngine;
using System;
using R3;
using R3.Triggers;

/// <summary>
/// 自機。
/// </summary>
public class Player : MonoBehaviour
{
    public float speed = 5;
    public GameObject Bullet;
    public GameObject BombType;
    public bool isBomb = true;

    public readonly ReactiveProperty<int> DamageRp = new(0);
    public readonly ReactiveProperty<Vector2> PositionRp = new(Vector2.zero);
    public int Damage => DamageRp.Value;

    public float X
    {
        get => PositionRp.Value.x;
        set
        {
            var p = PositionRp.Value;
            p.x = value;
            PositionRp.Value = p;
        }
    }

    public float Y
    {
        get => PositionRp.Value.y;
        set
        {
            var p = PositionRp.Value;
            p.y = value;
            PositionRp.Value = p;
        }
    }

    void Start()
    {
        PositionRp.Value = new Vector2(transform.position.x, transform.position.y);

        PositionRp
            .Subscribe(p =>
            {
                var pos = transform.position;
                pos.x = p.x;
                pos.y = p.y;
                transform.position = pos;
            })
            .AddTo(this);

        DamageRp
            .Skip(1)
            .Subscribe(_ =>
            {
                if (isBomb) Bomb.GenerateBomb(BombType, transform.position);
            })
            .AddTo(this);

        var update = Observable.EveryUpdate(destroyCancellationToken);

        update
            .Select(_ => (x: Input.GetAxisRaw("Horizontal"), y: Input.GetAxisRaw("Vertical")))
            .Where(v => v.x != 0f || v.y != 0f)
            .Subscribe(v => ApplyMove(v.x, v.y));

        // 撃つのを頼むだけ。
        // 押しっぱなしで毎コマ 送る（元 と同じ間合い）—— 溜まりすぎたぶんはサーバーが捨てて、捨てた数を数えている
        update
            .Where(_ => Input.GetKey(KeyCode.Z))
            .Subscribe(_ =>
            {
                var p = PositionRp.Value;
                if (DanmakuClient.Instance != null)
                {
                    DanmakuClient.Instance.Shoot(p.x, p.y);
                }
            });

        this.OnTriggerEnter2DAsObservable()
            .Subscribe(_ => HitByEnemyBullet())
            .AddTo(this);
    }

    void ApplyMove(float x, float y)
    {
        var p = PositionRp.Value;
        var mx = p.x + x / 100f * speed;
        if (mx >= 0.4f && mx <= 4.4f)
        {
            p.x = mx;
        }
        var my = p.y + y / 100f * speed;
        if (my > -6.0f && my <= -0.4f)
        {
            p.y = my;
        }
        PositionRp.Value = p;
    }

    public void HitByEnemyBullet()
    {
        DamageRp.Value += 1;
    }

    void OnDestroy()
    {
        DamageRp.Dispose();
        PositionRp.Dispose();
    }
}
