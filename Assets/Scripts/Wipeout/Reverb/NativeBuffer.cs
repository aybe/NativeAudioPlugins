using System;
using UnityEditor;

namespace Wipeout.Reverb
{
    public readonly struct NativeBuffer<T> : IDisposable where T : unmanaged
    {
        public readonly int Length;

        public NativeBuffer(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Length = length;
        }
        public void Dispose()
        {
        }
        
    }
}