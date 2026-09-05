using UnityEngine;
using System;
using FsBulletML2;
using R3;
using R3.Triggers;

public class Player : MonoBehaviour
{
    public float speed = 5;
    public GameObject Bullet;
    public GameObject BombType;
    public bool isBomb = true;

    public readonly ReactiveProperty<int> DamageRp = new(0);
    public readonly ReactiveProperty<Vector2> PositionRp = new(Vector2.zero);
    public int Damage => DamageRp.Value;

    // 旧は BulletmlTask（弾幕と実行状態が 1 つ の型）を 3 本 持ち回っていた。
    // 新 API では読み込んだ弾幕（BulletmlScript）だけを持ち、実行位置は
    // 撃つたびに Runner.newRoot で作る —— **1 本 の弾幕から何発でも撃てる。**
    // 旧は同じ task を撃つ弾ぜんぶで共有していて、状態が混ざる形だった
    private static BulletmlScript b2wayLeftBulletScript;
    private static BulletmlScript b2wayRightBulletScript;
    private static BulletmlScript homingScript;

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

    void Awake()
    {
        // **Init が先。** 読む段の Env（FrontEnv.Load）は BulletMLManager から
        // rand と rank を引くので、口を差し込む前に読むと NullReference になる
        FsBulletML2.BulletMLManager.Init(new BulletFunctions());
        // Bullets の PlayerBullet は Bulletml（DTD の木）を直に持っている。
        // Enemy 側は BulletmlInfo（名前つき）なので .Script(env) を呼ぶが、
        // **どちらも Runner.Load を通る**（BulletmlInfo.Script はその包み）
        b2wayLeftBulletScript = Runner.Load(CSharpWorld.RandFunc, CSharpWorld.LoadRank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayLeftBullet);
        b2wayRightBulletScript = Runner.Load(CSharpWorld.RandFunc, CSharpWorld.LoadRank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.b2wayRightBullet);
        homingScript = Runner.Load(CSharpWorld.RandFunc, CSharpWorld.LoadRank, FsBulletML2.Bullets.Dsl.PlayerBullet.PlayerBullet.homing);
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

        update
            .Where(_ => Input.GetKey(KeyCode.Z))
            .Subscribe(_ =>
            {
                Shoot2WayLeftBullet();
                Shoot2WayRightBullet();
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

    private void Shoot2WayLeftBullet()
    {
        var position = this.transform.position + new Vector3(-0.1f, 0.1f, 0);
        BulletEntityFactory.SpawnPlayer(position, Player.b2wayLeftBulletScript);
    }

    private void Shoot2WayRightBullet()
    {
        var position = this.transform.position + new Vector3(0.1f, 0.1f, 0);
        BulletEntityFactory.SpawnPlayer(position, Player.b2wayRightBulletScript);
    }

    private void ShootHomingBullet()
    {
        BulletEntityFactory.SpawnPlayer(this.transform.position, Player.homingScript);
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
