using System;

// COMPILE-ONLY stub of Entities Graphics. Not a substitute for the Unity Editor.
//
// **アセンブリ名が本物と一致していることが要。** 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
namespace Unity.Rendering
{
    public struct MaterialMeshInfo
    {
        public static MaterialMeshInfo FromRenderMeshArrayIndices(int materialIndex, int meshIndex)
            => new MaterialMeshInfo();
    }

    /// <summary>
    /// <b>本物は struct（ISharedComponentData）。</b>class にすると、焼いた dll の
    /// field signature が参照型になり、Unity で TypeManager が
    /// 「Expected reference type but got type kind 17」で落ちる。
    /// **値型か参照型かは焼き込まれる。**
    /// </summary>
    public struct RenderMeshArray
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
