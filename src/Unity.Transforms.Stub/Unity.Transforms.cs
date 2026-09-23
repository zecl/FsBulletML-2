using System;

// COMPILE-ONLY stub。Editor の代わりにはならない。
// アセンブリ名は本物と一致させる。1 本に詰めると CS7069。詳しくは UnityEngine.cs の頭。
namespace Unity.Transforms
{
    public struct LocalTransform : Unity.Entities.IComponentData
    {
        public Unity.Mathematics.float3 Position;
        public Unity.Mathematics.quaternion Rotation;
        public float Scale;
        public static LocalTransform FromPositionRotationScale(
            Unity.Mathematics.float3 position,
            Unity.Mathematics.quaternion rotation,
            float scale)
            => new LocalTransform { Position = position, Rotation = rotation, Scale = scale };
    }

    /// <summary>
    /// Entities Graphics が見る行列。このサンプルは System を持てないので自分で書く。
    /// </summary>
    public struct LocalToWorld : Unity.Entities.IComponentData
    {
        public Unity.Mathematics.float4x4 Value;
    }

    /// <summary>
    /// LocalToWorld を作る本物の System。印としてしか使わない。型の名前だけ要る。
    /// </summary>
    public partial class TransformSystemGroup : Unity.Entities.SystemBase { }
}
