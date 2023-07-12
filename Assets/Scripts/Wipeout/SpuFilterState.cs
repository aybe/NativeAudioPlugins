using System;
using System.Linq;

namespace Wipeout
{
    [Serializable]
    internal sealed class SpuFilterState
    {
        public float[] Input;
        public float[] Delay;
        public int     Index;

        public SpuFilterState(float[] input)
        {
            Input = input.Concat(input).ToArray();
            Delay = new float[input.Length];
            Index = 0;
        }
    }
}