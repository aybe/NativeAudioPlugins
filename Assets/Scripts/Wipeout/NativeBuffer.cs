using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wipeout
{
    public struct NativeBuffer
    {
        public static NativeBuffer<T> Create<T>(T[] array) where T : unmanaged
        {
            return new NativeBuffer<T>(array);
        }
    }
    public unsafe struct NativeBuffer<T> : IDisposable where T : unmanaged
    {
        public readonly int Length;

        private T* Pointer;

        public NativeBuffer(T[] array) : this(array.Length)
        {
            fixed (T* source = array)
            {
                UnsafeUtility.MemCpy(Pointer, source, Length * sizeof(T));
            }
        }

        public NativeBuffer(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Length = length;

            var sizeOfT  = sizeof(T);
            var size     = Length * sizeOfT;
            var alignOfT = UnsafeUtility.AlignOf<T>();
            var malloc   = UnsafeUtility.Malloc(size, alignOfT, Allocator.Persistent);

            if (malloc == null)
            {
                throw new OutOfMemoryException();
            }

            UnsafeUtility.MemClear(malloc, size);

            Pointer = (T*)malloc;
        }

        public void Dispose()
        {
            if (Pointer == null)
            {
                return;
            }

            UnsafeUtility.Free(Pointer, Allocator.Persistent);

            Pointer = null;
        }

        public ref T this[int index]
        {
            get
            {
                if (Pointer == null)
                {
                    throw new InvalidOperationException();
                }

                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }

                return ref *(Pointer + index * sizeof(T));
            }
        }

        public readonly override string ToString()
        {
            return $"{nameof(Length)}: {Length}";
        }
    }
}