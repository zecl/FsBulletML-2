using UnityEngine;
using System;
using System.Linq;
using FsBulletML2;
using R3;
using BulletType = FsBulletML2.DTD.BulletType;

/// <summary>
/// GameObject 側の弾。
///
/// <b>旧 API（Processable.IBulletmlObject）の実装をやめた。</b>
/// 旧はエンジンが GetAimDir / GetNewBullet などを呼び返すので 19 メンバ を
/// 実装させられていた。新 API は値の受け渡しだけなので、残るのは
/// フロント自身が要るものだけ。
///
/// 実際に飛ぶ弾は ECS 側（<see cref="BulletSim"/>）。ここは Enemy が継承して
/// 自分の動きの弾幕を回すのと、prefab に付いた EnemyBullet / PlayerBullet が
/// コンパイルできるようにするために残っている。
/// </summary>
public abstract class BaseBullet : MonoBehaviour
{
    [SerializeField]
    protected GameObject bulletObject;
    [field: SerializeField]
    public bool Root { get; set; }
    private GameObject TargetEnemy;
    public abstract GameObject GetBulletPrefubInstance();
    public GameObject BulletPrefab => bulletObject;

    public BaseBullet() : base() {}

    IDisposable simSub;

    /// <summary>走らせている弾幕。撃たれた弾も親と同じものを使い回す</summary>
    public BulletmlScript Script { get; private set; }

    /// <summary>
    /// この弾 1 体 の実行位置。<b>台本が無いあいだは null。</b>
    /// <c>BulletRun</c> は値型なので既定値を作れてしまう。Nullable で
    /// 「まだ持っていない」と区別する。
    /// </summary>
    public BulletRun? Run { get; private set; }

    /// <summary>直前のコマで全 top が終わったか。旧 BulletmlTask.Finish</summary>
    public bool Finished { get; private set; }

    public float AccelerationX { get; set; }
    public float AccelerationY { get; set; }
    /// <summary>自分も子を撃ったか。旧 BulletRoot。<b>フロントの印で、エンジンは見ない</b></summary>
    public bool BulletRoot { get; set; }

    /// <summary>
    /// 敵の弾か自機の弾か。<b>既定値を入れておくこと。</b>
    ///
    /// F# の判別共用体は参照型なので、既定は <b>0 ではなく null</b>。
    /// 入れ忘れたまま Runner.StepWith に渡すと、エンジンが match した
    /// ところで NullReferenceException になる。<b>コンパイルは通る</b>ので、
    /// 走らせるまで出ない（実際に踏んだ。BulletSmokeCheck が見つけた）。
    /// </summary>
    public BulletType BulletType { get; set; } = BulletType.Enemy;

    public float Dir { get; set; }
    public bool IsBullet { get; set; }

    /// <summary>
    /// <c>&lt;bulletml type&gt;</c>。<b>BulletType と同じ理由で既定値を入れる。</b>
    /// Core の既定（Runner.load）と揃えてある。
    /// </summary>
    public DTD.ShootingDirection ShootingDirection { get; set; } = DTD.ShootingDirection.BulletVertical;

    public float Speed { get; set; }
    public bool Used { get; set; }

    protected virtual void OnEnable()
    {
        simSub = Observable.EveryUpdate(destroyCancellationToken)
            .Subscribe(_ => RunTask());
    }

    protected virtual void OnDisable()
    {
        simSub?.Dispose();
        simSub = null;
    }

    /// <summary>
    /// 弾幕を割り当てる。根から始めるときは <paramref name="run"/> を null にする。
    /// </summary>
    public void SetScript(BulletmlScript script, BulletRun? run)
    {
        Script = script;
        Finished = false;
        Run = run.HasValue
            ? run
            : (script != null ? Runner.NewRoot(script) : (BulletRun?)null);
    }

    /// <summary>
    /// 1 コマ進める。<b>座標は差分を足す</b>（Frame.Delta は差分で、絶対値ではない）。
    /// </summary>
    protected void RunTask()
    {
        if (Script == null || !Run.HasValue)
        {
            return;
        }

        var rn = Run.Value;
        ShootingDirection = Script.ShootingDirection;
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）。
        //
        // **名前付き引数で書く。** F# 側は `{ rn.Body with Pos = ... }` と
        // 欄の名前で書けるが、C# にレコードの with が無いのでコンストラクタを
        // 並べることになり、**float が 2 本 並ぶ speed / dir と、bool が
        // 2 本 並ぶ isBullet / hasFired が位置ずれしても通ってしまう。**
        // 名前を書けば位置ずれはコンパイルで落ちる（綴り違いで較正済み）。
        // ただし**値そのものを取り違えた場合は落ちない** —— `speed: Dir` は
        // 名前が正しいので通る。そこは軌跡でしか見えない
        var body = new Body(
            pos: new FsBulletML2.Domain.Vec2(X, Y),
            speed: Speed,
            dir: Dir,
            accel: new FsBulletML2.Domain.Vec2(AccelerationX, AccelerationY),
            kind: BulletType,
            isBullet: IsBullet,
            hasFired: BulletRoot);

        var f = Runner.StepWith(Script, EnvNow(rn), rn, body);
        var after = f.Run.Body;
        Speed = after.Speed;
        Dir = after.Dir;
        AccelerationX = after.Accel.X;
        AccelerationY = after.Accel.Y;
        X = X + (f.Delta.X / 100);
        Y = Y - (f.Delta.Y / 100);
        var angle = -(Dir) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1));
        Finished = f.Finished;

        // 撃たれた弾は ECS の実体にする。旧はエンジンが GetNewBullet を
        // 呼び返して実体を要求していたが、いまは値で受け取る
        foreach (var child in f.Spawned)
        {
            BulletRoot = true;
            BulletEntityFactory.SpawnFromEmitter(this, child);
        }

        if (f.Vanished)
        {
            Vanish();
        }

        if (f.Retired)
        {
            Used = false;
        }

        // 走らせ直しの Env は、位置を更新したあとの自分から組む
        // （旧 BaseBullet が座標を足したあとで envOfGlobal を呼ぶのと同じ順）
        Run = f.Finished
            ? Runner.Restart(EnvNow(f.Run), f.Run)
            : f.Run;
    }

    public float X
    {
        get { return transform.position.x; }
        set
        {
            var newPosition = this.transform.position;
            newPosition.x = value;
            this.transform.position = newPosition;
        }
    }
    public float Y
    {
        get { return transform.position.y; }
        set
        {
            var newPosition = this.transform.position;
            newPosition.y = value;
            this.transform.position = newPosition;
        }
    }

    public virtual void Init()
    {
        this.Root = false;
        this.Used = true;
        this.BulletRoot = false;
        this.AccelerationX = 0;
        this.AccelerationY = 0;
        this.Speed = 0;
        this.Dir = 0;
        this.IsBullet = true;

        // 旧はここで task.Init(envOfGlobal this) を呼んで木を歩き直していた
        if (Run.HasValue)
        {
            Run = Runner.Restart(EnvNow(Run.Value), Run.Value);
        }
    }

    public void Vanish()
    {
        this.Used = false;
    }

    /// <summary>
    /// このコマの Env。<b>台本が無い弾は aim を読まない</b>ので、
    /// そのときは Atan2 を 4 本 とも省く（BulletRun.HasNoScript の但し書き）。
    /// </summary>
    FsBulletML2.Domain.Env EnvNow(BulletRun run)
    {
        return run.HasNoScript
            ? FrontEnv.NoAim()
            : FrontEnv.At(X, Y, GetEnemyAimDir());
    }

    /// <summary>
    /// いちばん近い敵を狙う向き。旧 GetEnemyAimDir の式そのまま。
    /// <b>選んだ相手を覚えるところも旧と同じ</b> —— 毎コマ 選び直すと
    /// 相手が入れ替わって軌跡が変わる。
    /// </summary>
    public float GetEnemyAimDir()
    {
        if (this.TargetEnemy != null)
        {
            return Mathf.Atan2(this.TargetEnemy.transform.position.x - this.X, this.TargetEnemy.transform.position.y - this.Y);
        }
        else
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (!enemies.Any())
            {
                return 0;
            }
            else
            {
                GameObject near = enemies[0];
                float nearDist = Vector2.Distance(this.transform.position, near.transform.position);
                for (int i = 1; i < enemies.Length; i++)
                {
                    var d = Vector2.Distance(this.transform.position, enemies[i].transform.position);
                    if (d < nearDist)
                    {
                        nearDist = d;
                        near = enemies[i];
                    }
                }
                this.TargetEnemy = near;
                return Mathf.Atan2(this.TargetEnemy.transform.position.x - this.X, this.TargetEnemy.transform.position.y - this.Y);
            }
        }
    }
}
