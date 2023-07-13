using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Wipeout
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct BufferHandler<T> : IDisposable where T : unmanaged
    {
        public int Length { get; }

        public T* Pointer { get; private set; }

        public readonly bool Allocated => (IntPtr)Pointer != IntPtr.Zero;

        public BufferHandler(int length)
        {
            Length = length;

            var i = sizeof(T);

            var size = Length * i;

            var alignOf = UnsafeUtility.AlignOf<T>();

            var malloc = UnsafeUtility.Malloc(size, alignOf, Allocator.Persistent);

            Pointer = (T*)malloc;
        }

        public void Dispose()
        {
            if (!Allocated)
            {
                return;
            }

            UnsafeUtility.Free(Pointer, Allocator.Persistent);

            Pointer = (T*)IntPtr.Zero;
        }

        public void CopyTo(T[] managedArray)
        {
            if (!Allocated)
            {
                throw new ObjectDisposedException("Cannot copy. Buffer has been disposed");
            }

            var length = Math.Min(managedArray.Length, Length);

            var gcHandle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);

            UnsafeUtility.MemCpy((void*)gcHandle.AddrOfPinnedObject(), Pointer, length * sizeof(T));

            gcHandle.Free();

            fixed (T* pt = managedArray)
            {
                // UnsafeUtility.MemCpy(Pointer, pt, length * sizeof(T)); // todo
            }
        }

        public void CopyTo(BufferHandler<T> buffer)
        {
            if (!Allocated)
            {
                throw new ObjectDisposedException("Cannot copy. Source buffer has been disposed");
            }

            if (!buffer.Allocated)
            {
                throw new ObjectDisposedException("Cannot copy. Dest buffer has been disposed");
            }

            var length = Math.Min(Length, buffer.Length);

            UnsafeUtility.MemCpy(Pointer, buffer.Pointer, length * sizeof(T));
        } // use pointers to access and set the data in the buffer

        public T this[int index]
        {
            get
            {
                CheckAndThrow(index);
                return *(T*)((long)Pointer + index * sizeof(T));
            }

            set
            {
                CheckAndThrow(index);
                *(T*)((long)Pointer + index * sizeof(T)) = value;
            }
        }

        // utility method to validate an index in the buffer
        private void CheckAndThrow(int index)
        {
            if (!Allocated) throw new ObjectDisposedException("Buffer is disposed");
            if (index >= Length || index < 0)
                throw new IndexOutOfRangeException($"index:{index} out of range:0-{Length}");
        }
    }
}