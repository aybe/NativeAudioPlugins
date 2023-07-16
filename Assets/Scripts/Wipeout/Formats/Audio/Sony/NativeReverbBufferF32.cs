using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Wipeout.Formats.Audio.Sony
{
    [Obsolete]
    internal unsafe struct NativeReverbBufferF32 : IDisposable
        // for Burst
    {
        private readonly int Count;

        private readonly float* Items;

        private int Index;

        public readonly ref float this[in int index] => ref Items[((Index + index) % Count + Count) % Count];

        public NativeReverbBufferF32(int length)
        {
            if (!Mathf.IsPowerOfTwo(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be a power of two.");
            }

            var size = length * sizeof(float);
            var pack = UnsafeUtility.AlignOf<float>();
            var data = (float*)UnsafeUtility.Malloc(size, pack, Allocator.Persistent);

            UnsafeUtility.MemClear(data, length);

            Count = length;
            Items = data;
            Index = 0;
        }

        public void Advance(int count = 2)
        {
            Index = (Index + count) % Count;
        }

        public readonly void Dispose()
        {
            UnsafeUtility.Free(Items, Allocator.Persistent);
        }
    }
}