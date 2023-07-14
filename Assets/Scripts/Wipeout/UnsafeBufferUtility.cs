using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wipeout
{
    public static unsafe class UnsafeBufferUtility
    {
        public static UnsafeBuffer<T> Allocate<T>(T[] array) where T : unmanaged
        {
            var length = array.Length;
            var buffer = new UnsafeBuffer<T>(length);

            fixed (T* source = array)
            {
                UnsafeUtility.MemCpy(buffer.Items, source, length * sizeof(T));
            }

            return buffer;
        }

        public static UnsafeBuffer<UnsafeBuffer<T>> Allocate<T>(T[][] arrays) where T : unmanaged
        {
            var length = arrays.Length;
            var buffer = new UnsafeBuffer<UnsafeBuffer<T>>(length);

            for (var i = 0; i < length; i++)
            {
                buffer[i] = Allocate(arrays[i]);
            }

            return buffer;
        }

        public static UnsafeBuffer<UnsafeBuffer<UnsafeBuffer<T>>> Allocate<T>(T[][][] arrays) where T : unmanaged
        {
            var length = arrays.Length;
            var buffer = new UnsafeBuffer<UnsafeBuffer<UnsafeBuffer<T>>>(length);

            for (var i = 0; i < length; i++)
            {
                buffer[i] = Allocate(arrays[i]);
            }

            return buffer;
        }

        public static UnsafeBuffer<UnsafeBuffer<T>> AllocateOld<T>(T[][] arrays) where T : unmanaged
        {
            var buffer = new UnsafeBuffer<UnsafeBuffer<T>>(arrays.Length);

            for (var i = 0; i < arrays.Length; i++)
            {
                var array = arrays[i];

                buffer[i] = new UnsafeBuffer<T>(array.Length);

                fixed (T* source = array)
                {
                    UnsafeUtility.MemCpy(buffer[i].Items, source, array.Length * sizeof(T));
                }
            }

            return buffer;
        }

        public static T* Malloc<T>(int length) where T : unmanaged
        {
            var sizeOf  = UnsafeUtility.SizeOf<T>();
            var size    = length * sizeOf;
            var alignOf = UnsafeUtility.AlignOf<T>();
            var malloc  = UnsafeUtility.Malloc(size, alignOf, Allocator.Persistent);

            return (T*)malloc;
        }
    }
}