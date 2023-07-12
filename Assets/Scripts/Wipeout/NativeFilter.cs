using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wipeout
{
    public readonly unsafe struct NativeFilter : IDisposable
    {
        public readonly float* Coefficients;
        public readonly int    CoefficientsLength;
        public readonly float* DelayLine;
        public readonly int*   DelayLinePosition;
        public readonly int*   Taps;
        public readonly int    TapsLength;

        public NativeFilter(FilterState state)
        {
            var coefficients = state.Coefficients;
            var delayLine    = state.DelayLine;
            var taps         = state.Taps;

            Coefficients       = Alloc(coefficients);
            CoefficientsLength = coefficients.Length;
            DelayLine          = Alloc(delayLine);
            DelayLinePosition  = Alloc(new[] { 0 });
            Taps               = Alloc(taps);
            TapsLength         = taps.Length;
        }

        private static T* Alloc<T>(T[] array) where T : unmanaged
        {
            var sizeOf = UnsafeUtility.SizeOf<T>();

            var size = array.Length * sizeOf;

            var alignment = UnsafeUtility.AlignOf<T>();

            var ptr = UnsafeUtility.Malloc(size, alignment, Allocator.Persistent);

            var source = MemoryMarshal.AsBytes(array.AsSpan());

            var target = new Span<byte>(ptr, size);

            source.CopyTo(target);

            return (T*)ptr;
        }

        private static void Free<T>(T* ptr) where T : unmanaged
        {
            UnsafeUtility.Free(ptr, Allocator.Persistent);
        }

        public void Dispose()
        {
            Free(Coefficients);
            Free(DelayLine);
            Free(Taps);
        }
    }
}