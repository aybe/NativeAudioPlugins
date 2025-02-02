using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Wipeout.Formats.Audio.Extensions;

namespace Wipeout
{
    [Serializable]
    [NoReorder]
    public sealed class FilterState
    {
        public float[] Coefficients = null!;

        public float[] DelayLine = null!;

        public int[] Taps = null!;

        public int Position;

        public static FilterState CreateHalfBand(double fs = 44100.0d, double bw = 441.0d, FilterWindow fw = FilterWindow.Blackman)
        {
            if (fs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fs), fs, null);
            }

            if (bw < fs * 0.01d || bw > fs * 0.49d)
            {
                throw new ArgumentOutOfRangeException(nameof(bw), bw, null);
            }

            if (!Enum.IsDefined(typeof(FilterWindow), fw))
            {
                throw new InvalidEnumArgumentException(nameof(fw), (int)fw, typeof(FilterWindow));
            }

            var fc = fs / 4.0d;

            var lp = Filter.LowPass(fs, fc, bw, fw);

            var hb = Filter.HalfBandTaps(lp.Length);

            return new FilterState
            {
                Coefficients = Array.ConvertAll(lp, Convert.ToSingle),
                DelayLine    = new float[lp.Length * 2],
                Position     = 0,
                Taps         = hb
            };
        }
    }
}