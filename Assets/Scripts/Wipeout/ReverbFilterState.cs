using System;
using Unity.Mathematics;

namespace Wipeout
{
    [Serializable]
    internal sealed class ReverbFilterState
    {
        public float2[] Coefficients;

        public float2[] Delays;

        public float2[] Buffer = new float2[44100];

        public int Position;
    }
}