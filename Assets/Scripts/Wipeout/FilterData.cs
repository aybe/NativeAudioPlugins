using System;
using System.Linq;

namespace Wipeout
{
    [Serializable]
    internal unsafe struct FilterData : IDisposable
    {
        public int     DataLength;
        public int     DataChannels;
        public float*  Data;
        public float** FiltersCoefficients;
        public float** FiltersDelay;
        public int*    FiltersPositions;
        public int*    FiltersTaps;

        public void Dispose()
        {
        }

        public static void Create(float[] filter)
        {
            var h = filter.Concat(filter).ToArray();
            var z = new float[h.Length];
            var n = new int[h.Length];
        }
    }
}