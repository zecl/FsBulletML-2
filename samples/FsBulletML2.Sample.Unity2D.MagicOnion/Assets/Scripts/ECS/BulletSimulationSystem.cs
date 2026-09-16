using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 降ってきた並びを、描く形へ写す。この system は当たり判定 を持たない。
///
/// 判定 はサーバーの仕事。 ここでやらない理由は 3 つ ある。
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