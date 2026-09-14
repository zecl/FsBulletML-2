using System;

// COMPILE-ONLY stub of UnityEngine and the Unity packages the samples use.
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
//
// ## アセンブリ名は本物と 1 対 1 にすること
//
// 以前は ECS も URP も、この `AssemblyName=UnityEngine` の中に同居していた。
// **stub では通るが、Unity では通らない。** 焼いた dll が
// 「`Unity.Entities.Entity` は UnityEngine に在る」と主張したまま渡り、
// Unity の `UnityEngine.dll`（型フォワードだけの facade）には無いので
// CS7069 で落ちる。`Transform` や `Vector3` はフォワードが在るので通り、
// **ECS と URP を触った所だけが落ちる**ので、気づくのが遅れた。
//
// いまは本物と同じ名前で 1 本 ずつ在る（src/Unity.*.Stub）。
// 同梱 dll がどこから型を引いているかは
// .github/scripts/guard-shipped-refs.ps1 が見ている。

namespace UnityEngine
{
    // COMPILE-ONLY stub. Not a substitute for the Unity Editor.

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static float Distance(Vector2 a, Vector2 b)
        {
            float dx = a.x - b.x, dy = a.y - b.y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static implicit operator Vector3(Vector2 v) => new Vector3(v.x, v.y, 0f);
        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y) { this.x = x; this.y = y; this.z = 0f; }
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0f, 0f, 0f);
        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public static Quaternion identity => new Quaternion();
        public static Quaternion AngleAxis(float angle, Vector3 axis) => identity;
        public static Quaternion Euler(float x, float y, float z) => identity;
    }

    public struct Rect
    {
        public float x { get; set; }
        public float y { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public Rect(float x, float y, float width, float height)
        {
            this.x = x; this.y = y; this.width = width; this.height = height;
        }
    }

    public struct Mathf
    {
        public const float Rad2Deg = 57.29578f;
        public const float Deg2Rad = 0.0174532924f;
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Abs(float v) => Math.Abs(v);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
    }

    public enum FindObjectsInactive
    {
        Exclude,
        Include
    }

    public class Object
    {
        public string name { get; set; }
        public static void DontDestroyOnLoad(Object target) { }
        public static Object Instantiate(Object original) => original;
        public static Object Instantiate(Object original, Vector3 position, Quaternion rotation) => original;
        public static T Instantiate<T>(T original) where T : Object => original;
        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : Object => original;
        public static void Destroy(Object obj) { }
        public static void DestroyObject(Object obj) { }
        public static T FindObjectOfType<T>() where T : Object => default;
        public static T FindFirstObjectByType<T>() where T : Object => default;
        public static T FindAnyObjectByType<T>() where T : Object => default;
        public static T[] FindObjectsByType<T>() where T : Object => System.Array.Empty<T>();
        public static T[] FindObjectsByType<T>(FindObjectsInactive findObjectsInactive) where T : Object => System.Array.Empty<T>();

        /// <summary>
        /// <b>本物はここに「壊されたか」の判定が入っている。</b>
        /// Unity は壊した Object を <b>null のように振る舞う非 null 参照</b>に
        /// するので、素の参照比較では生きていると読んでしまう。
        ///
        /// 偽物は壊す仕組みを持たないので参照比較そのまま。
        /// <b>ここが通ることは、生存判定が正しいことを 1 つ も保証しない。</b>
        /// </summary>
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);

        /// <summary>
        /// <c>if (self &amp;&amp; ...)</c> と書ける Unity の癖。上の == と同じ理由で、
        /// <b>偽物では「壊されたか」を見ていない</b>
        /// </summary>
        public static implicit operator bool(Object exists) => !ReferenceEquals(exists, null);
    }

    public class GameObject : Object
    {
        public GameObject() {}
        public GameObject(string name) { this.name = name; }
        public string tag { get; set; }

        // **遅らせて作る。** 即座に作ると Transform -> Component ->
        // GameObject -> Transform で無限に降りて StackOverflow になる
        // （Transform は Component の派生）。コンパイルだけなら踏まないので、
        // フロントを実際に回す門を建てるまで誰も気づかなかった
        Transform _transform;
        public Transform transform => _transform ??= new Transform();
        public bool activeSelf { get; private set; } = true;
        public void SetActive(bool value) { activeSelf = value; }
        public T GetComponent<T>() => default;
        public Component GetComponent(Type type) => null;
        public T AddComponent<T>() where T : Component => default;
        public static GameObject Find(string name) => null;
        public static GameObject[] FindGameObjectsWithTag(string tag) => Array.Empty<GameObject>();
    }

    public class Component : Object
    {
        // **遅らせて作る**（GameObject.transform と同じ理由）。
        // 自分が Transform ならそれ自身を返す —— Unity でもそうなっている
        GameObject _gameObject;
        public GameObject gameObject => _gameObject ??= new GameObject();
        Transform _transform;
        public Transform transform => _transform ??= (this as Transform) ?? new Transform();
        public bool CompareTag(string tag) => false;
        public string tag { get => gameObject.tag; set => gameObject.tag = value; }
        public T GetComponent<T>() => default;
        public Component GetComponent(Type type) => null;
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
    }

    public class MonoBehaviour : Behaviour
    {
        public bool useGUILayout { get; set; }

        /// <summary>
        /// この component が壊されたときに取り消される印。
        /// <b>R3 の購読を切るのに使う</b> —— <c>subscription.RegisterTo(token)</c>。
        ///
        /// 本物は Unity 2022.2 以降 の MonoBehaviour が持つ。ここが偽物なので
        /// 決して取り消されないが、<b>コンパイルが通るかを見るのが目的</b>。
        /// </summary>
        public System.Threading.CancellationToken destroyCancellationToken { get; }
            = System.Threading.CancellationToken.None;
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
    }

    public class Renderer : Component
    {
        public Material material { get; set; } = new Material();
        public Material[] materials { get; set; } = System.Array.Empty<Material>();
        public Material[] sharedMaterials { get; set; } = System.Array.Empty<Material>();
        public string sortingLayerName { get; set; }
        public int sortingOrder { get; set; }
        public void GetPropertyBlock(MaterialPropertyBlock block) { }
        public void SetPropertyBlock(MaterialPropertyBlock block) { }
    }

    public class MeshRenderer : Renderer { }

    /// <summary>材質を共有したまま個別に色やテクスチャを差す口</summary>
    public class MaterialPropertyBlock
    {
        public void SetTexture(string name, Texture value) { }
        public void SetColor(string name, Color value) { }
    }

    public enum CameraClearFlags
    {
        Skybox = 1,
        SolidColor = 2,
        Depth = 3,
        Nothing = 4,
    }

    public class Camera : Behaviour
    {
        public static Camera main => null;
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public bool orthographic { get; set; }
        public float orthographicSize { get; set; }
        public float fieldOfView { get; set; }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
        public int cullingMask { get; set; }
    }

    public enum LightType
    {
        Spot = 0,
        Directional = 1,
        Point = 2,
        Area = 3,
    }

    public class Light : Behaviour
    {
        public LightType type { get; set; }
        public float intensity { get; set; }
        public Color color { get; set; }
    }

    public class Material : Object
    {
        public Material() {}
        public Material(Shader shader) {}
        public Shader shader { get; set; }
        public Vector2 mainTextureOffset { get; set; }
        public Texture mainTexture { get; set; }
        public int renderQueue { get; set; }
        public bool enableInstancing { get; set; }
        public bool doubleSidedGI { get; set; }
        public bool HasProperty(string name) => false;
        public void SetFloat(string name, float value) { }
        public void SetColor(string name, Color value) { }
        public void SetTexture(string name, Texture value) { }
        public Texture GetTexture(string name) => null;
        public Color GetColor(string name) => Color.white;
        public void SetOverrideTag(string tag, string val) { }
        public void EnableKeyword(string keyword) { }
        public void DisableKeyword(string keyword) { }
    }

    public class Shader : Object
    {
        public static Shader Find(string name) => null;
    }

    public class Texture : Object { }

    /// <summary>AnimationClip の基底。**このサンプルは 1 度 も使わない** ——
    /// それでも置くのは、`FsBulletML2.Motion` と名前がかぶるから。
    /// 本物に在る型を stub が持たないと、その衝突が Unity でだけ出る（CS0104）</summary>
    public class Motion : Object { }
    public class Texture2D : Texture
    {
        // 本物に引数なしの ctor は無い。書かないと public な既定 ctor が生えて、
        // 呼び手がそれを指してしまう
        public Texture2D(int width, int height) { }
        public static Texture2D whiteTexture => new Texture2D(1, 1);
    }

    public struct Color
    {
        public float r, g, b, a;
        public static Color white => new Color { r = 1f, g = 1f, b = 1f, a = 1f };
    }

    public struct Bounds
    {
        // 本物は auto-property。field にすると MissingFieldException
        public Vector3 center { get; set; }
        public Vector3 extents { get; set; }
    }

    public class Sprite : Object
    {
        public Vector2[] vertices => System.Array.Empty<Vector2>();
        public ushort[] triangles => System.Array.Empty<ushort>();
        public Vector2[] uv => System.Array.Empty<Vector2>();
        public Texture2D texture => Texture2D.whiteTexture;
        public Bounds bounds => new Bounds();
    }

    public class SpriteRenderer : Renderer
    {
        public Sprite sprite { get; set; }
        public Color color { get; set; }
    }

    /// <summary>描画に渡す形。Entities Graphics が RenderMeshArray に入れる</summary>
    public class Mesh : Object
    {
        public void SetVertices(Vector3[] vertices) { }
        public void SetUVs(int channel, Vector2[] uvs) { }
        public void SetTriangles(int[] triangles, int submesh) { }
        public void RecalculateBounds() { }
        public void RecalculateNormals() { }
    }

    public enum MotionVectorGenerationMode
    {
        Camera = 0,
        Object = 1,
        ForceNoMotion = 2,
    }

    public enum ParticleSystemSimulationSpace { Local, World, Custom }
    public enum ParticleSystemStopAction { None, Disable, Destroy, Callback }
    public enum ParticleSystemStopBehavior { StopEmitting, StopEmittingAndClear }

    public class ParticleSystem : Component
    {
        /// <summary>1 発 だけ出すときに渡す粒。使われる欄だけ持つ</summary>
        public struct EmitParams
        {
            public Vector3 position { get; set; }
            public bool applyShapeToPosition { get; set; }
        }

        /// <summary>
        /// <b>本物は struct。</b> 取り出して書き換えても本体に伝わらないので、
        /// Unity 側でも同じ書き方で効かないことがある。ここは compile を通すだけ
        /// </summary>
        public struct MainModule
        {
            public bool playOnAwake { get; set; }
            public bool loop { get; set; }
            public ParticleSystemSimulationSpace simulationSpace { get; set; }
            public ParticleSystemStopAction stopAction { get; set; }
        }

        public MainModule main => new MainModule();
        public bool IsAlive() => false;
        public void Emit(int count) { }
        public void Emit(EmitParams emitParams, int count) { }
        public void Play() { }
        public void Stop() { }
        public void Stop(bool withChildren) { }
        public void Stop(bool withChildren, ParticleSystemStopBehavior behavior) { }
    }

    public class ParticleSystemRenderer : Renderer { }

    public class Collider2D : Component
    {
        public Bounds bounds => new Bounds();
    }

    /// <summary>MonoBehaviour の Update が呼ばれる順。小さいほど先</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultExecutionOrder : Attribute
    {
        public DefaultExecutionOrder(int order) { }
    }

    public static class Time
    {
        public static float deltaTime => 0f;
        /// <summary>Time.timeScale の影響を受けない実時間。fps を数えるのに使う</summary>
        public static float unscaledDeltaTime => 0f;
        public static float realtimeSinceStartup => 0f;
        public static float unscaledTime => 0f;
        public static float time => 0f;
    }

    public static class Application
    {
        public static int targetFrameRate { get; set; }
        public static bool isPlaying => false;
    }

    /// <summary>
    /// fps の上限を掛けるとき、targetFrameRate より<b>先に</b>切る必要がある
    /// —— vSyncCount が 1 以上 だと Unity は targetFrameRate を無視する。
    /// </summary>
    public static class QualitySettings
    {
        public static int vSyncCount { get; set; }
    }
}

namespace UnityEngine.Rendering
{
    // COMPILE-ONLY stub. 透過で描くときに使う面だけ。

    /// <summary>RenderMeshDescription が既定値に使う。**本物と同じ並びにする**
    /// —— 省いた引数の値は呼び手の IL に焼き込まれる</summary>
    public enum LightProbeUsage
    {
        Off = 0,
        BlendProbes = 1,
        UseProxyVolume = 2,
        CustomProvided = 4,
    }

    public enum ShadowCastingMode
    {
        Off = 0,
        On = 1,
        TwoSided = 2,
        ShadowsOnly = 3,
    }

    public enum BlendMode
    {
        Zero = 0,
        One = 1,
        SrcAlpha = 5,
        OneMinusSrcAlpha = 10,
    }

    public enum RenderQueue
    {
        Geometry = 2000,
        AlphaTest = 2450,
        Transparent = 3000,
        Overlay = 4000,
    }

    /// <summary>いま使われているレンダリングパイプライン。URP へ移ったかを見る</summary>
    public class RenderPipelineAsset : Object { }

    public static class GraphicsSettings
    {
        /// <summary>本物の戻り型は RenderPipelineAsset。Object にすると
        /// 呼び手が get_defaultRenderPipeline():Object を吐き、
        /// 実行時に MissingMethodException になる</summary>
        public static RenderPipelineAsset defaultRenderPipeline { get; set; }
    }
}

namespace UnityEngine
{

    public enum RuntimeInitializeLoadType
    {
        AfterSceneLoad = 0,
        BeforeSceneLoad = 1,
        AfterAssembliesLoaded = 2,
        BeforeSplashScreen = 3,
        SubsystemRegistration = 4,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute() {}
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType) {}
    }

    public static class Input
    {
        public static float GetAxisRaw(string axisName) => 0f;
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyDown(KeyCode key) => false;
    }

    /// <summary>Transform.Translate などが取る向きの基準。
    /// **本物に在って stub に無い型は、名前の衝突を隠す** ——
    /// FsBulletML2.Front.Space と名前が同じで、Unity でだけ CS0104 になった</summary>
    public enum Space
    {
        World = 0,
        Self = 1,
    }
    public enum KeyCode
    {
        None = 0,
        Return = 13,
        Z = 122,
    }

    public static class GUI
    {
        public static void Box(Rect position, string text) { }
        public static void Label(Rect position, string text) { }
        public static bool Button(Rect position, string text) => false;
    }

    public static class Debug
    {
        public static void LogError(object message) { }
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogWarning(object message, Object context) { }
    }

    public class ExecuteInEditMode : Attribute { }
    public class SerializeField : Attribute { }
    public class RequireComponent : Attribute
    {
        public RequireComponent(Type type) { }
    }

    /// <summary>Inspector に出すスライダの範囲。値は使われない</summary>
    /// <summary>Inspector の欄に出る説明。<b>焼くだけなら何もしない</b></summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TooltipAttribute : Attribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(float min, float max) { }
    }

    public class AudioClip : Object { }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public float volume { get; set; }
        public bool loop { get; set; }
        public bool mute { get; set; }
        public bool playOnAwake { get; set; }
        public bool isPlaying => false;
        public void Play() { }
        public void Stop() { }
        public void PlayOneShot(AudioClip clip) { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }
}

// **Unity.Collections のこの 2 型 は、本物では UnityEngine.CoreModule に在る。**
// パッケージ（com.unity.collections）ではない。名乗りを間違えると、焼いた dll が
// 「Unity.Collections アセンブリに在る」と主張して Unity で解決できなくなる。
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
