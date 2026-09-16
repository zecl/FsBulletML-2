using System;

// COMPILE-ONLY stub of Entities Graphics. Not a substitute for the Unity Editor.
//
// アセンブリ名が本物と一致していることが要。 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
//
// signature も既定値も本物どおりに写すこと。 省いた引数の値は呼び手の IL に
// 焼き込まれるので、既定値が違うと描き方が変わる。
//
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
namespace Unity.Rendering
{
    /// <summary>RenderMeshArray の中で material と mesh の組を指す</summary>
    public struct MaterialMeshIndex
    {
        public int MaterialIndex;
        public int MeshIndex;
        public int SubMeshIndex;
    }

    public struct MaterialMeshInfo
    {
        public static MaterialMeshInfo FromRenderMeshArrayIndices(
            int materialIndexInRenderMeshArray,
            int meshIndexInRenderMeshArray,
            ushort submeshIndex = 0)
            => new MaterialMeshInfo();
    }

    /// <summary>
    /// 本物は struct（ISharedComponentData）。class にすると、焼いた dll の
    /// field signature が参照型になり、Unity で TypeManager が
    /// 「Expected reference type but got type kind 17」で落ちる。
    /// 値型か参照型かは焼き込まれる。
    /// </summary>
    public struct RenderMeshArray
    {
        public RenderMeshArray(
            UnityEngine.Material[] materials,
            UnityEngine.Mesh[] meshes,
            MaterialMeshIndex[] materialMeshIndices = null) { }
    }

    public struct RenderMeshDescription
    {
        public RenderMeshDescription(
            UnityEngine.Rendering.ShadowCastingMode shadowCastingMode,
            bool receiveShadows = false,
            UnityEngine.MotionVectorGenerationMode motionVectorGenerationMode =
                UnityEngine.MotionVectorGenerationMode.Camera,
            int layer = 0,
            uint renderingLayerMask = 4294967295,
            UnityEngine.Rendering.LightProbeUsage lightProbeUsage =
                UnityEngine.Rendering.LightProbeUsage.Off,
            bool staticShadowCaster = false,
            int rendererPriority = -1,
            float smallMeshCullingThreshold = 0f) { }
    }

    public static class RenderMeshUtility
    {
        public static void AddComponents(
            Unity.Entities.Entity entity,
            Unity.Entities.EntityManager entityManager,
            in RenderMeshDescription renderMeshDescription,
            RenderMeshArray renderMeshArray,
            MaterialMeshInfo materialMeshInfo = default) { }
    }
}
