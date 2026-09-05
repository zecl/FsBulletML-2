using System;

// COMPILE-ONLY stub of the Unity ECS packages (Entities / Collections /
// Mathematics / Transforms / Entities Graphics).
//
// **これは Editor の代わりにはならない。** 型と面の形を写しただけで、
// エンティティは 1 つ も作られないし、何も描かれない。
// 「Unity を持っていない機械でも sln がビルドできる」ためだけに在る。
//
// **本番の門は `-p:UseRealUnity=true` のビルドと、Unity での実行のほう**
// （samples/FsBulletML2.Sample.Unity2D.FSharp/Assets/Editor/BulletSmokeCheck.cs）。
// ここが通ることは、動くことを 1 つ も保証しない。
//
// 足す面は「サンプルが実際に呼ぶもの」に限る。広げると本物とずれても
// 気づけなくなる。

namespace Unity.Mathematics
{
    public struct float3
    {
        public float x, y, z;
        public float3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct quaternion
    {
        public static quaternion identity => new quaternion();
        public static quaternion AxisAngle(float3 axis, float angle) => identity;
    }

    /// <summary>Unity.Mathematics の関数群。使うぶんだけ</summary>
    public static class math
    {
        public static float max(float a, float b) => System.Math.Max(a, b);
        public static int max(int a, int b) => System.Math.Max(a, b);
        public static float min(float a, float b) => System.Math.Min(a, b);
        public static int min(int a, int b) => System.Math.Min(a, b);
        public static float abs(float a) => System.Math.Abs(a);
    }

    public struct float4x4
    {
        public static float4x4 identity => new float4x4();
        public static float4x4 TRS(float3 translation, quaternion rotation, float3 scale) => identity;
    }
}

namespace Unity.Collections
{
    public enum Allocator
    {
        Invalid = 0,
        None = 1,
        Temp = 2,
        TempJob = 3,
        Persistent = 4,
    }

    public struct NativeArray<T> : IDisposable
    {
        public int Length => 0;
        public T this[int index] { get => default; set { } }
        public void Dispose() { }
    }
}

namespace Unity.Entities
{
    public struct Entity : IEquatable<Entity>
    {
        public static Entity Null => new Entity();
        public bool Equals(Entity other) => true;
        public override bool Equals(object obj) => obj is Entity e && Equals(e);
        public override int GetHashCode() => 0;
        public static bool operator ==(Entity a, Entity b) => a.Equals(b);
        public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    }

    /// <summary>ECS の component であることの印。中身は空</summary>
    public interface IComponentData { }

    public struct ComponentType
    {
        public static ComponentType ReadWrite<T>() => new ComponentType();
        public static ComponentType ReadOnly<T>() => new ComponentType();
        public static ComponentType ReadWrite(Type type) => new ComponentType();
        public static ComponentType ReadOnly(Type type) => new ComponentType();
    }

    public struct EntityQuery : IDisposable
    {
        public Unity.Collections.NativeArray<Entity> ToEntityArray(Unity.Collections.Allocator allocator)
            => new Unity.Collections.NativeArray<Entity>();
        public void Dispose() { }
    }

    /// <summary>
    /// ECS の System。**C# サンプルだけが使う** —— F# サンプルは
    /// `SystemBase` を避けて `FrameTicker` から回している
    /// （理由は samples/FsBulletML2.Sample.Unity2D.FSharp の BulletEcsDriver.fs）。
    ///
    /// 本物は Roslyn の生成器が partial の相方を足すが、compile を通すだけなら
    /// 空の基底で足りる
    /// </summary>
    public abstract partial class SystemBase
    {
        public EntityManager EntityManager => new EntityManager();
        protected virtual void OnCreate() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnDestroy() { }
        protected EntityQuery GetEntityQuery(params ComponentType[] componentTypes) => new EntityQuery();
        protected EntityQuery GetEntityQuery(params Type[] componentTypes) => new EntityQuery();
    }

    public partial class SimulationSystemGroup : SystemBase { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UpdateInGroupAttribute : Attribute
    {
        public UpdateInGroupAttribute(Type groupType) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UpdateBeforeAttribute : Attribute
    {
        public UpdateBeforeAttribute(Type systemType) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UpdateAfterAttribute : Attribute
    {
        public UpdateAfterAttribute(Type systemType) { }
    }

    public struct EntityManager
    {
        public Entity CreateEntity() => Entity.Null;
        public void DestroyEntity(Entity entity) { }
        public bool Exists(Entity entity) => false;
        public bool HasComponent<T>(Entity entity) => false;
        public bool AddComponentData<T>(Entity entity, T componentData) => false;
        public void AddComponentObject(Entity entity, object componentData) { }
        public T GetComponentData<T>(Entity entity) => default;
        public void SetComponentData<T>(Entity entity, T componentData) { }
        public T GetComponentObject<T>(Entity entity) => default;
        public EntityQuery CreateEntityQuery(params ComponentType[] componentTypes) => new EntityQuery();
        public EntityQuery CreateEntityQuery(params Type[] componentTypes) => new EntityQuery();
    }

    /// <summary>
    /// component の型を覚えているところ。**dll で配ると自動登録が掛からない**ので、
    /// サンプルは実行時に GetOrCreateTypeIndex で足す。
    /// </summary>
    public static class TypeManager
    {
        public static void Initialize() { }
        public static int GetOrCreateTypeIndex(Type type) => 0;
    }

    public class World : IDisposable
    {
        public World(string name) { Name = name; }
        public string Name { get; }
        public EntityManager EntityManager => new EntityManager();
        public static World DefaultGameObjectInjectionWorld { get; set; }
        public void Dispose() { }
    }
}

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

namespace Unity.Rendering
{
    public struct MaterialMeshInfo
    {
        public static MaterialMeshInfo FromRenderMeshArrayIndices(int materialIndex, int meshIndex)
            => new MaterialMeshInfo();
    }

    public class RenderMeshArray
    {
        public RenderMeshArray(UnityEngine.Material[] materials, UnityEngine.Mesh[] meshes) { }
    }

    public struct RenderMeshDescription
    {
        public RenderMeshDescription(
            UnityEngine.Rendering.ShadowCastingMode shadowCastingMode,
            bool receiveShadows,
            UnityEngine.MotionVectorGenerationMode motionVectorGenerationMode,
            int layer) { }
    }

    public static class RenderMeshUtility
    {
        public static void AddComponents(
            Unity.Entities.Entity entity,
            Unity.Entities.EntityManager entityManager,
            in RenderMeshDescription renderMeshDescription,
            RenderMeshArray renderMeshArray,
            MaterialMeshInfo materialMeshInfo) { }
    }
}

namespace UnityEngine.Rendering.Universal
{
    // COMPILE-ONLY stub. URP へ移したあとカメラを組み直す面だけ。

    public enum CameraRenderType
    {
        Base = 0,
        Overlay = 1,
    }

    public class UniversalAdditionalCameraData : UnityEngine.MonoBehaviour
    {
        public CameraRenderType renderType { get; set; }
        public bool renderPostProcessing { get; set; }
        public System.Collections.Generic.List<UnityEngine.Camera> cameraStack { get; }
            = new System.Collections.Generic.List<UnityEngine.Camera>();
        public void SetRenderer(int index) { }
    }

    public static class CameraExtensions
    {
        public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this UnityEngine.Camera camera)
            => new UniversalAdditionalCameraData();
    }
}
