using System;

// COMPILE-ONLY stub of Universal Render Pipeline. Not a substitute for the Unity Editor.
//
// **アセンブリ名が本物と一致していることが要。** 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
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
