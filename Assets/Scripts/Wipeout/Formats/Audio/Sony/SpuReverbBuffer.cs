using System;

namespace Wipeout.Formats.Audio.Sony
{
    internal sealed class SpuReverbBuffer<T>
    {
        private readonly int Count;

        private readonly T[] Items;

        private int Index;

        public SpuReverbBuffer(in int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            Items = new T[length];

            Count = Items.Length;
        }

        public ref T this[in int index]
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
    }
}