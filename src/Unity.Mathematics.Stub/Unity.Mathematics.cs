using System;

// COMPILE-ONLY stub of Unity.Mathematics. Not a substitute for the Unity Editor.
//
// **アセンブリ名が本物と一致していることが要。** 1 本 に詰めると、焼いた dll が
// 「この型は UnityEngine に在る」と主張したまま Unity へ渡り、CS7069 で落ちる。
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
namespace Unity.Mathematics
{
    public struct float3
    {
        public float x, y, z;
        public float3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct quaternion
    {
        public static readonly quaternion identity = new quaternion();
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
        public static readonly float4x4 identity = new float4x4();
        public static float4x4 TRS(float3 translation, quaternion rotation, float3 scale) => identity;
    }
}
