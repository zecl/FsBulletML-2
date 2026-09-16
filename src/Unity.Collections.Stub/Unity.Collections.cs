using System;

// COMPILE-ONLY stub of Unity.Collections. Not a substitute for the Unity Editor.
//
// 同じ namespace が 2 つ のアセンブリに分かれている。 `Allocator` と
// `NativeArray<T>` は本物では UnityEngine.CoreModule に在り、こちらの
// パッケージ（com.unity.collections）に在るのは `AllocatorManager` のほう。
// namespace で置き場を決めると 1 つ 取り違える。
//
// 詳しい但し書きは src/UnityEngine.Stub/UnityEngine.cs の頭。
namespace Unity.Collections
{
    public static class AllocatorManager
    {
        /// <summary>`EntityQuery.ToEntityArray` が取るのはこちら。
        /// `Allocator` からは暗黙変換で入る —— 本物にも同じ変換が在るので、
        /// 呼び手は `Allocator.Temp` と書ける</summary>
        public struct AllocatorHandle
        {
            public static implicit operator AllocatorHandle(Allocator allocator)
                => new AllocatorHandle();
        }
    }
}
