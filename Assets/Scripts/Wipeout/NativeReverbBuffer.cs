using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Wipeout
{
    internal unsafe struct NativeReverbBuffer : IDisposable
        // for Burst
    {
        private readonly int Count;

        private readonly float* Items;

        private int Index;

        public readonly ref float this[in int index] => ref Items[(Index + index) & Count];

        public NativeReverbBuffer(int length)
        {
            if (!Mathf.IsPowerOfTwo(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be a power of two.");
            }

            var count = length * sizeof(float);
            var align = UnsafeUtility.AlignOf<float>();
            var items = (float*)UnsafeUtility.Malloc(count, align, Allocator.Persistent);

            UnsafeUtility.MemClear(items, length);

            Count = length - 1;
            Items = items;
            Index = 0;
        }

        public void Advance()
        {
            Index = (Index + 2) & Count;
        }

        public readonly void Dispose()
        {
            UnsafeUtility.Free(Items, Allocator.Persistent);
        }
    }
}