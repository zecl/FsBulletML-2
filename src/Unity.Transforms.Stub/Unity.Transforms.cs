using System;

// COMPILE-ONLY stub of Unity.Transforms. Not a substitute for the Unity Editor.
//
// **アセンブリ名が本物と一致していることが要。** 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
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
    /// <b>Entities Graphics が実際に見る行列。</b>
    /// 本物は <see cref="LocalTransform"/> から TransformSystemGroup が作るが、
    /// このサンプルは System を持てないので自分で書く（BulletEcsDriver）。
    /// </summary>
    public struct LocalToWorld : Unity.Entities.IComponentData
    {
        public Unity.Mathematics.float4x4 Value;
    }

    /// <summary>
    /// 上の LocalToWorld を作る本物の System。**印としてしか使わない**
    /// —— C# サンプルが `[UpdateBefore(typeof(TransformSystemGroup))]` で
    /// 順を指定するために型の名前だけ要る
    /// </summary>
    public partial class TransformSystemGroup : Unity.Entities.SystemBase { }
}
