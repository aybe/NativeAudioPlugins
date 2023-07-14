using System;
using System.Collections;
using System.Collections.Generic;

namespace Wipeout
{
    internal sealed unsafe class UnsafeBufferEnumerator<T> : IEnumerator<T> where T : unmanaged
    {
        private readonly int Count;

        private readonly T* Items;

        private int Index = -1;

        public UnsafeBufferEnumerator(int count, T* items)
        {
            Items = items;
            Count = count;
        }

        public bool MoveNext()
        {
            return ++Index < Count;
        }

        public void Reset()
        {
            Index = -1;
        }

        public T Current => *(Items + Index);

        object IEnumerator.Current => Current;

        void IDisposable.Dispose()
        {
        }
    }
}