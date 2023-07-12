using System;
using System.Collections.Generic;
using System.Linq;

namespace Wipeout
{
    [Serializable]
    internal sealed class FilterStateOld
    {
        public float[] Input;

        public float[] Delay;

        public int Count;

        public int Index;

        public FilterStateOld(IReadOnlyCollection<float> filter)
        {
            Input = filter.Concat(filter).ToArray();
            Delay = new float[filter.Count];
            Count = filter.Count;
        }
    }
}