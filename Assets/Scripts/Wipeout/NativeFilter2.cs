using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wipeout
{
    public unsafe class NativeFilter2 : IDisposable
    {
        private readonly int ArraysCount;

        public readonly float** Coefficients;
        public readonly int*    CoefficientsLength;
        public readonly float** DelayLine;
        public readonly int**   DelayLinePosition;
        public readonly int**   Taps;
        public readonly int*    TapsLength;

        public NativeFilter2(FilterState state, int count)
        {
            var coefficients       = state.Coefficients;
            var coefficientsLength = Repeat(coefficients.Length, count);
            var delayLine          = state.DelayLine;
            var delayLinePosition  = new[] { 0 };
            var taps               = state.Taps;
            var tapsLength         = Repeat(taps.Length, count);

            ArraysCount        = count;
            Coefficients       = Alloc(coefficients, count);
            CoefficientsLength = Alloc(coefficientsLength);
            DelayLine          = Alloc(delayLine, count);
            DelayLinePosition  = Alloc(delayLinePosition, count);
            Taps               = Alloc(taps, count);
            TapsLength         = Alloc(tapsLength);
        }

        public void Dispose()
        {
            for (var i = 0; i < ArraysCount; i++)
            {
                Free(Coefficients[i]);
                Free(DelayLine[i]);
                Free(DelayLinePosition[i]);
                Free(Taps[i]);
            }

            Free(Coefficients);
            Free(DelayLine);
            Free(DelayLinePosition);
            Free(Taps);
            Free(CoefficientsLength);
            Free(TapsLength);
        }

        private static T[] Repeat<T>(T value, int count)
        {
            return Enumerable.Repeat(value, count).ToArray();
        }

        private static T** Alloc<T>(T[] array, int count) where T : unmanaged
        {
            var ptrSizeOf = UnsafeUtility.SizeOf<IntPtr>();
            var ptrSize   = count * ptrSizeOf;
            var ptrAlign  = UnsafeUtility.AlignOf<IntPtr>();
            var ptr       = UnsafeUtility.Malloc(ptrSize, ptrAlign, Allocator.Persistent);

            var arrSizeOf = sizeof(T);
            var arrSize   = array.Length * arrSizeOf;
            var arrAlign  = UnsafeUtility.AlignOf<T>();
            var arr       = (T**)ptr;

            for (var i = 0; i < count; i++)
            {
                var arrPtr = UnsafeUtility.Malloc(arrSize, arrAlign, Allocator.Persistent);
                var arrSpn = new Span<T>(arrPtr, array.Length);

                array.CopyTo(arrSpn);

                arr[i] = (T*)arrPtr;
            }

            return arr;
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

        private static void Free(void* ptr)
        {
            UnsafeUtility.Free(ptr, Allocator.Persistent);
        }
    }
}