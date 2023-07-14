using System;
using System.Collections;
using System.Collections.Generic;

namespace Wipeout
{
    public unsafe struct UnsafeBufferEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly int Count;

        private readonly T* Items;

        private int Index;

        public UnsafeBufferEnumerator(int count, T* items)
        {
            Items = items;
            Count = count;
            Index = -1;
        }

        public bool MoveNext()
        {
            return ++Index < Count;
        }

        public void Reset()
        {
            Index = -1;
        }

        public readonly T Current => *(Items + Index);

        readonly object IEnumerator.Current => Current;

        readonly void IDisposable.Dispose()
        {
        }
    }
}