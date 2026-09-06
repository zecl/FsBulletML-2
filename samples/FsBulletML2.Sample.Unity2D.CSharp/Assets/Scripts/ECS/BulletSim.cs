using Unity.Entities;
using UnityEngine;
using FsBulletML2;
using FsBulletML2.Front;
using BulletType = FsBulletML2.DTD.BulletType;

/// <summary>
/// Managed BulletML object stored on an ECS entity. X/Y are plain floats (no Transform).
/// Not Burst-compatible: it holds a managed BulletmlScript.
///
/// <b>旧 API（Processable.IBulletmlObject）の実装をやめた。</b>
/// 旧はエンジンがここを呼び返すので 19 メンバ を実装させられていた。
/// 新 API は値の受け渡しだけなので、残るのはフロント自身が要るものだけ ——
/// 物理量の箱と、走らせている弾幕（Script）とその実行位置（Run）。
///
/// 旧の <c>Task option</c> は 2 つ に割れた。
/// <list type="bullet">
/// <item><c>Script</c>  読み込んだ弾幕。撃たれた弾も親と同じものを使い回す</item>
/// <item><c>Run</c>     この弾 1 体 の実行位置。コマごとに受け取って持ち歩く</item>
/// </list>
/// </summary>
// sealed にしておく。**TypeManager は sealed でない class の中を辿らない**
// ので、他の managed component から参照されたときに中が見えなくなる
// （F# 版の BulletSim も [<Sealed>]）
public sealed class BulletSim : IComponentData
{
    public Entity Entity;
    public bool Root;
    public BulletKind Kind;

    public float X { get; set; }
    public float Y { get; set; }
    public float AccelerationX { get; set; }
    public float AccelerationY { get; set; }
    /// <summary>自分も子を撃ったか。旧 BulletRoot。<b>フロントの印で、エンジンは見ない</b></summary>
    public bool BulletRoot { get; set; }

    /// <summary>
    /// 敵の弾か自機の弾か。<b>既定値を入れておくこと。</b>
    ///
    /// F# の判別共用体は参照型なので、<c>new BulletSim()</c> の既定は
    /// <b>0 ではなく null</b>。入れ忘れたまま Runner.StepWith に渡すと、
    /// エンジンが match したところで NullReferenceException になる。
    /// <b>コンパイルは通る</b>ので、走らせるまで出ない。
    /// 同梱フロント（FsBulletML2.Unity2D.DefaultBullet）も同じ既定を入れている。
    /// </summary>
    public BulletType BulletType { get; set; } = BulletType.Enemy;

    public float Dir { get; set; }
    public bool IsBullet { get; set; }

    /// <summary>
    /// <c>&lt;bulletml type&gt;</c>。走らせた弾は Script.ShootingDirection で
    /// 上書きされるので、この既定が出るのはまだ 1 度 も走らせていない弾だけ。
    /// <b>BulletType と同じ理由で既定値を入れる</b>（DU は null になりうる）。
    /// Core の既定（Runner.load）と揃えてある。
    /// </summary>
    public DTD.ShootingDirection ShootingDirection { get; set; } = DTD.ShootingDirection.BulletVertical;

    public float Speed { get; set; }
    public bool Used { get; set; }

    /// <summary>走らせている弾幕。撃たれた弾は親と同じものを引き継ぐ</summary>
    /// <summary>走らせている弾幕。<b>実行状態の中に居る</b>（<c>BulletRun.Script</c>）。</summary>
    public BulletmlScript Script => Run.HasValue ? Run.Value.Script : null;

    /// <summary>
    /// この弾 1 体 の実行位置。<b>台本が無いあいだは null。</b>
    ///
    /// <c>BulletRun</c> は値型なので既定値（中身が null）を作れてしまう。
    /// Nullable で持って「まだ持っていない」と区別する。
    /// </summary>
    public BulletRun? Run { get; private set; }

    /// <summary>直前のコマで全 top が終わったか。旧 BulletmlTask.Finish</summary>
    public bool Finished { get; private set; }

    /// <summary>
    /// 弾幕を割り当てる。根から始めるときは <paramref name="run"/> を null にする。
    /// 撃たれた弾には、親から受け取った実行位置をそのまま渡す。
    ///
    /// <b>根の立場（狙う先と、撃たれた弾か）はここで 1 回 だけ決まる。</b>
    /// Core へは毎コマ渡らないので、BulletType と IsBullet はこれを呼ぶ前に
    /// 立てておくこと。    /// </summary>
    public void SetScript(BulletmlScript script)
    {
        Finished = false;
        Run = script != null
            ? (IsBullet ? Runner.NewShot(BulletType, script) : Runner.NewRoot(BulletType, script))
            : (BulletRun?)null;
    }

    /// <summary>
    /// 撃たれた弾を、エンジンから受け取った実行状態で始める。
    /// <b>弾幕を渡す口が無い</b> —— <c>BulletRun</c> が親のものを持っている。
    /// </summary>
    public void SetRun(BulletRun run)
    {
        Finished = false;
        Run = run;
    }

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

        // 旧はここで task.Init(envOfGlobal this) を呼んで木を歩き直していた。
        // この時点の位置（撃たれた直後は、まだ親の位置へ移す前）で組むところも旧のまま
        if (Run.HasValue)
        {
            Run = Driver.Restart(front, Run.Value);
        }
    }

    public void Vanish()
    {
        Used = false;
    }

    /// <summary>
    /// この弾から見た世界。<b>敵は 1 体 しか居ない</b>ので一覧を持たない。
    /// </summary>
    readonly EcsEnv front = new EcsEnv();

    /// <summary>
    /// 1 コマ進める。<b>座標は呼ぶ側が足す</b>（Frame.Delta は差分）。
    ///
    /// 撃たれた弾は <paramref name="spawn"/> へ渡す。旧はエンジンが
    /// GetNewBullet を呼び返して実体を要求していたが、いまは値で受け取るので
    /// <b>フロントが自分の都合で実体を作る</b>（弾プールが尽きたら捨ててよい）。
    /// </summary>
    public void Step(System.Action<BulletSim, BulletRun> spawn)
    {
        if (!Run.HasValue)
        {
            return;
        }

        var rn = Run.Value;
        ShootingDirection = Script.ShootingDirection;
        // 物理量はフロントが持っている。毎コマ入れ直す（旧 stateOfBullet）。
        // **名前付き引数で書く理由は BaseBullet.RunTask の但し書き**
        // （位置ずれは落ちるが、値の取り違えは落ちない）
        var motion = new FsBulletML2.Motion(
            pos: new FsBulletML2.Domain.Vec2(X, Y),
            speed: Speed,
            dir: Dir,
            accel: new FsBulletML2.Domain.Vec2(AccelerationX, AccelerationY));

        // Env を組む位置も、台本が無い弾の枝も Driver が持っている
        var f = Driver.Step(front, CSharpEnv.Space, CSharpEnv.Origin, rn, motion);
        var after = f.Run.Motion;
        Speed = after.Speed;
        Dir = after.Dir;
        AccelerationX = after.Accel.X;
        AccelerationY = after.Accel.Y;
        X = X + (f.Delta.X / 100f);
        Y = Y - (f.Delta.Y / 100f);
        Finished = f.Finished;

        foreach (var child in f.Spawned)
        {
            BulletRoot = true;
            spawn(this, child);
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
        // （旧 BulletSim が座標を足したあとで envOfGlobal を呼ぶのと同じ順）
        Run = f.Finished
            ? Driver.Restart(front, f.Run)
            : f.Run;
    }
}
