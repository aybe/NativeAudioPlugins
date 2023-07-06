using System;
using System.Linq;

namespace Wipeout
{
    [Serializable]
    internal sealed class FilterState2
    {
        public float[] Coefficients;
        public float[] Delay;
        public int     Index;
        public int     Taps;

        public FilterState2(float[] coefficients)
        {
            Coefficients = coefficients.Concat(coefficients).ToArray();
            Delay        = new float[coefficients.Length];
            Taps         = coefficients.Length;
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}";
        }
    }
}