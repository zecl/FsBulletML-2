using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using FsBulletML2;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class BulletSimulationSystem : SystemBase
{
    EntityQuery _query;
    readonly HashSet<Entity> _overlappingPlayer = new HashSet<Entity>();
    readonly List<Entity> _overlapScratch = new List<Entity>();

    protected override void OnCreate()
    {
        _query = GetEntityQuery(
            ComponentType.ReadWrite<BulletSim>(),
            ComponentType.ReadWrite<LocalTransform>(),
            ComponentType.ReadOnly<BulletTag>());
    }

    protected override void OnUpdate()
    {
        if (!BulletEntityFactory.IsReady)
        {
            return;
        }

        var player = BulletEcsRuntime.Player;
        var enemy = BulletEcsRuntime.Enemy;
        var playerPos = player != null ? player.transform.position : Vector3.zero;
        var enemyPos = enemy != null ? enemy.transform.position : Vector3.zero;
        var playerRadius = BulletEcsRuntime.PlayerRadius;
        var enemyRadius = BulletEcsRuntime.EnemyRadius;

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
                var tag = em.GetComponentData<BulletTag>(entity);

                RunSim(sim);

                var transform = em.GetComponentData<LocalTransform>(entity);
                transform.Position = new float3(sim.X, sim.Y, 0f);
                transform.Rotation = quaternion.AxisAngle(new float3(0f, 0f, 1f), -sim.Dir);
                em.SetComponentData(entity, transform);

                bool dead = !sim.Used || BulletEntityFactory.IsOffScreen(sim.X, sim.Y);
                if (dead)
                {
                    _overlappingPlayer.Remove(entity);
                    BulletEntityFactory.Destroy(entity);
                    continue;
                }

                if (tag.Kind == BulletKind.Enemy && player != null)
                {
                    bool hit = Overlaps(sim.X, sim.Y, tag.Radius, playerPos, playerRadius);
                    if (hit)
                    {
                        if (_overlappingPlayer.Add(entity))
                        {
                            player.HitByEnemyBullet();
                        }

                        if (!sim.Root)
                        {
                            _overlappingPlayer.Remove(entity);
                            BulletEntityFactory.Destroy(entity);
                        }
                    }
                    else
                    {
                        _overlappingPlayer.Remove(entity);
                    }
                }
                else if (tag.Kind == BulletKind.Player && enemy != null)
                {
                    if (Overlaps(sim.X, sim.Y, tag.Radius, enemyPos, enemyRadius))
                    {
                        enemy.HitByPlayerBullet();
                        BulletEntityFactory.Destroy(entity);
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }

        if (_overlappingPlayer.Count > 0)
        {
            _overlapScratch.Clear();
            foreach (var e in _overlappingPlayer)
            {
                if (!em.Exists(e))
                {
                    _overlapScratch.Add(e);
                }
            }

            for (int i = 0; i < _overlapScratch.Count; i++)
            {
                _overlappingPlayer.Remove(_overlapScratch[i]);
            }
        }
    }

    /// <summary>
    /// 1 コマ進める。撃たれた弾は <see cref="BulletEntityFactory.SpawnChild"/> で
    /// 実体にする。
    ///
    /// <b>旧 API は「撃つのを断る」口を持っていた</b>（GetNewBullet が null を
    /// 返すと fire の累積を巻き戻す）。新 API はエンジンが撃った弾を値で返しきる
    /// ので、捨てるかどうかはこちらの都合で決める（Frame.Spawned の但し書き）。
    /// </summary>
    static void RunSim(BulletSim sim)
    {
        sim.Step(SpawnChild);
    }

    static void SpawnChild(BulletSim parent, BulletRun child)
    {
        BulletEntityFactory.SpawnChild(parent, child);
    }

    static bool Overlaps(float x, float y, float radius, Vector3 target, float targetRadius)
    {
        float dx = x - target.x;
        float dy = y - target.y;
        float rr = radius + targetRadius;
        return (dx * dx) + (dy * dy) <= rr * rr;
    }
}
