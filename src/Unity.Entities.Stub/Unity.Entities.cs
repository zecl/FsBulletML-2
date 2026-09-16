using System;

// COMPILE-ONLY stub of Unity.Entities. Not a substitute for the Unity Editor.
//
// アセンブリ名が本物と一致していることが要。 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
//
// signature も本物どおりに写すこと。 引数の型・数・既定値は呼び手の IL に
// そのまま焼き込まれる。ここで楽な形（`Type[]` を取るなど）を足すと、
// stub では通り、Unity で MissingMethodException になる。
//
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
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

        /// <summary>本物にも在る。C# の呼び手が `typeof(X)` を渡せるのはこれのおかげ
        /// —— 無いと `Type[]` を取る口を stub に足したくなる（本物には無い）</summary>
        public static implicit operator ComponentType(Type type) => new ComponentType();
    }

    public struct EntityQuery : IDisposable
    {
        /// <summary>本物が取るのは `Allocator` ではなく `AllocatorHandle`。
        /// `Allocator` からは暗黙変換で入る</summary>
        public Unity.Collections.NativeArray<Entity> ToEntityArray(
            Unity.Collections.AllocatorManager.AllocatorHandle allocator)
            => new Unity.Collections.NativeArray<Entity>();
        public void Dispose() { }
    }

    /// <summary>
    /// 本物は SystemBase の基底。EntityManager も OnCreate も
    /// GetEntityQuery もこちらに在る。1 つ に潰すと、呼び手が
    /// <c>SystemBase::get_EntityManager</c> を吐いて本物に無い口を指す
    /// （override も宣言型が合わず、別のメソッドになる）。
    /// </summary>
    public abstract class ComponentSystemBase
    {
        public EntityManager EntityManager => new EntityManager();
        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }
        protected EntityQuery GetEntityQuery(params ComponentType[] componentTypes) => new EntityQuery();
    }

    /// <summary>
    /// ECS の System。C# サンプルだけが使う —— F# サンプルは
    /// `SystemBase` を避けて `FrameTicker` から回している
    /// （理由は samples/FsBulletML2.Sample.Unity2D.FSharp の BulletEcsDriver.fs）。
    /// </summary>
    public abstract partial class SystemBase : ComponentSystemBase
    {
        protected virtual void OnUpdate() { }
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
    }

    /// <summary>
    /// component の型を覚えているところ。dll で配ると自動登録が掛からないので、
    /// 足す面は本物の public だけ —— GetOrCreateTypeIndex は本物では
    /// internal なので置かない。stub にだけ在る面は、呼び手がそれを使ってしまう。
    /// </summary>
    public static class TypeManager
    {
        public static void Initialize() { }
    }

    /// <summary>World の役割。既定値まで本物と同じにする ——
    /// 省いた引数の値は呼び手の IL に焼き込まれる（本物の既定は 9）</summary>
    [Flags]
    public enum WorldFlags
    {
        None = 0,
        Live = 1,
        Editor = 2,
        Game = 8,
        Simulation = Live | Game,
    }

    public class World : IDisposable
    {
        public World(string name, WorldFlags flags = WorldFlags.Simulation) { Name = name; }
        public string Name { get; }
        public EntityManager EntityManager => new EntityManager();
        public static World DefaultGameObjectInjectionWorld { get; set; }
        public void Dispose() { }
    }
}
