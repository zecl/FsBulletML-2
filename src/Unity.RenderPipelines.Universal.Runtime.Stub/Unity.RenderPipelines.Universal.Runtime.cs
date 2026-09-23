using System;

// COMPILE-ONLY stub。Editor の代わりにはならない。
// アセンブリ名は本物と一致させる。1 本に詰めると CS7069。詳しくは UnityEngine.cs の頭。
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
