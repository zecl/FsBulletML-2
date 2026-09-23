using System;

// COMPILE-ONLY stub。Editor の代わりにはならない。
// signature も既定値も本物どおり。既定値が違うと描き方が変わる。
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
    /// 本物は struct。class にすると field signature が参照型になり、Unity で落ちる。
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
