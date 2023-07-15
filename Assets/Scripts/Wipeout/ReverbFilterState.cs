using System;
using Unity.Mathematics;

namespace Wipeout
{
    [Serializable]
    internal sealed class ReverbFilterState
    {
        public float2[] Coefficients;

        public float2[] Delays;

        public float[] Buffer = new float[44100 * 2];

        public int Position;
    }
}