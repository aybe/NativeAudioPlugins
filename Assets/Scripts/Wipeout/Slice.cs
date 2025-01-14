﻿using System;

namespace Wipeout
{
    public readonly ref struct Slice<T> where T : unmanaged
    {
        public readonly unsafe T*  Source;
        public readonly        int Length;
        public readonly        int Stride;

        public unsafe Slice(T* source, int length, int stride = 0)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (stride < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stride));
            }

            Source = source;
            Length = length;
            Stride = stride;
        }

        public unsafe ref T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }

                return ref *(Source + index * Stride);
            }
        }

        public override string ToString()
        {
            return $"{nameof(Length)}: {Length}, {nameof(Stride)}: {Stride}";
        }
    }
}