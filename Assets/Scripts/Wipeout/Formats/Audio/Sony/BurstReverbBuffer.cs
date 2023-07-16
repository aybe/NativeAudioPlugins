using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Wipeout.Formats.Audio.Sony
{
    public unsafe struct BurstReverbBuffer : IDisposable
    {
        private readonly int Count;

        private readonly int* Items;

        private int Index;

        public BurstReverbBuffer(in int length)
        {
            if (!math.ispow2(length))
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, "Length must be a power of 2.");
            }

            var size      = length * sizeof(int);
            var alignment = UnsafeUtility.AlignOf<int>();
            var pointer   = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);

            UnsafeUtility.MemClear(pointer, size);

            Items = (int*)pointer;
            Count = length;
            Index = 0;
        }

        public ref int this[in int index]
        {
            get
            {
                var n = Index + index;
                var m = Count;
                var i = (n % m + m) % m;

                return ref Items[i];
            }
        }

        public void Advance(in int count = 2)
        {
            Index = (Index + count) % Count;
        }

        public readonly void Dispose()
        {
            UnsafeUtility.Free(Items, Allocator.Persistent);
        }
    }
}