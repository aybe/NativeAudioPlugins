using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Wipeout
{
    public readonly unsafe struct UnsafeBuffer<T> : IDisposable, IEnumerable<T> where T : unmanaged
    {
        public readonly int Count;

        public readonly T* Items;

        public UnsafeBuffer(int count)
            : this(count, UnsafeBufferUtility.Malloc<T>(count))
        {
        }

        public UnsafeBuffer(int count, T* items)
        {
            Count = count;
            Items = items;
        }

        public ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }

                return ref *(Items + index);
            }
        }

        public static implicit operator T*(UnsafeBuffer<T> buffer)
        {
            return buffer.Items;
        }
        
        public static implicit operator IntPtr(UnsafeBuffer<T> buffer)
        {
            return (IntPtr)buffer.Items;
        }

        public void Dispose()
        {
            if (GetType().GenericTypeArguments[0].IsGenericType)
            {
                foreach (var item in this)
                {
                    ((IDisposable)item).Dispose();
                }
            }
            else
            {
                UnsafeUtility.Free(Items, Allocator.Persistent);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new UnsafeBufferEnumerator<T>(Count, Items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"{nameof(Count)}: {Count}, {GetType().GetNiceName()}";
        }
    }
}