using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 降ってきた並びを、描く形へ写す。<b>この system は当たり判定 を持たない。</b>
///
/// <b>判定 はサーバーの仕事。</b> ここでやらない理由は 3 つ ある。
///
/// 1. <b>弾を消せない。</b> 当たった弾をここで消しても、
///    次のコマでサーバーが同じ番号 を送ってきて復活する。
///    消せない判定 は「数えるだけ」になり、**同じ弾を 2 回 数えない工夫**
///    （重なっているあいだ覚えておく表）が要る —— それが丸ごと消えた。
/// 2. <b>ただ で乗る。</b> サーバーは弾を落とす走査を毎コマ 回している。
///    判定 をそこへ相乗りさせると、**増えるループは 0 本。**
/// 3. <b>2 人 目 が来たときに割れない。</b> client が判定すると、
///    2 人 居たときにどちらの言い分を採るかが決まらない。
///
/// 当たった数は <c>FrameDto</c> に載って降りてくる（<see cref="DanmakuClient"/>）。
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class BulletSimulationSystem : SystemBase
{
    EntityQuery _query;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(
            ComponentType.ReadWrite<BulletSim>(),
            ComponentType.ReadWrite<LocalTransform>());
    }

    protected override void OnUpdate()
    {
        if (!BulletEntityFactory.IsReady)
        {
            return;
        }

        var em = EntityManager;
        NativeArray<Entity> entities = _query.ToEntityArray(Allocator.Temp);
        try
        {
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                if (!em.Exists(entity))
                {
                    continue;
                }

                var sim = em.GetComponentObject<BulletSim>(entity);

                var transform = em.GetComponentData<LocalTransform>(entity);
                transform.Position = new float3(sim.X, sim.Y, 0f);
                transform.Rotation = quaternion.AxisAngle(new float3(0f, 0f, 1f), -sim.Dir);
                em.SetComponentData(entity, transform);
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}