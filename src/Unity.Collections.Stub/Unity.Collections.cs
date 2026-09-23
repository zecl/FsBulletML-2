using System;

// COMPILE-ONLY stub。Editor の代わりにはならない。
// 同じ namespace が 2 アセンブリに分かれる。namespace で置き場を決めると取り違える。
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
