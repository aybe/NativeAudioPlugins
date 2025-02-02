// ReSharper disable IdentifierTypo

using System;
using System.Collections.Generic;
using System.Linq;

namespace Wipeout.Formats.Audio.Extensions
{
    /// <summary>
    ///     https://fiiir.com/
    /// </summary>
    public sealed class Filter
    {
        public Filter(IReadOnlyCollection<double> coefficients)
        {
            Coefficients = coefficients.ToArray();
            DelayLine    = new double[coefficients.Count];
            Position     = 0;
        }

        private readonly double[] Coefficients;

        private readonly double[] DelayLine;

        private int Position;

        public double Process(double sample)
        {
            var delays = DelayLine;

            delays[Position] = sample;

            var result = 0.0d;
            var offset = Position;
            var source = Coefficients;
            var length = source.Length;

            for (var i = 0; i < length; i++)
            {
                var input = source[i];
                var delay = delays[offset];
                var value = input * delay;

                result = result + value;

                offset--;

                if (offset < 0)
                {
                    offset = length - 1;
                }
            }

            Position++;

            if (Position >= length)
            {
                Position = 0;
            }

            return result;
        }

        public static int[] HalfBandTaps(int count)
        {
            var taps = Enumerable.Range(0, count).Where(i => i % 2 == 1 || i == count / 2).ToArray();

            return taps;
        }

        public static double[] LowPass(double fs, double fc, double bw, FilterWindow wt)
        {
            if (fs < 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(fs), fc, null);
            }

            if (fc >= fs * 0.5)
            {
                throw new ArgumentOutOfRangeException(nameof(fc), fc, null);
            }

            if (bw < fs * 0.01 || bw > fs * 0.49)
            {
                throw new ArgumentOutOfRangeException(nameof(bw), bw, null);
            }

            var k = wt switch
            {
                FilterWindow.Blackman => 4.6,
                FilterWindow.Hamming  => 3.1,
                _                     => throw new ArgumentOutOfRangeException(nameof(wt), wt, null)
            };

            Func<int, int, double> w = wt switch
            {
                FilterWindow.Blackman => Blackman,
                FilterWindow.Hamming  => Hamming,
                _                     => throw new ArgumentOutOfRangeException(nameof(wt), wt, null)
            };

            var f = fc / fs;
            var b = bw / fs;
            var n = (int)Math.Ceiling(k / b);

            if (n % 2 == 0)
            {
                n++;
            }

            var h = new double[n];

            for (var i = 0; i < n; i++)
            {
                var d = Sinc(n, i, f);
                var e = w(n, i);
                var g = d * e;
                h[i] = g;
            }

            var sum = h.Sum();

            for (var i = 0; i < n; i++)
            {
                h[i] /= sum;
            }

            return h;
        }

        private static double Sinc(int n, int i, double d)
        {
            if (i == n / 2)
            {
                return 1;
            }

            var x = Math.PI * (2.0 * d * (i - (n - 1) / 2.0));
            var y = Math.Sin(x);
            var z = y / x;

            return z;
        }

        private static double Blackman(int n, int i)
        {
            const double a0 = 0.42;
            const double a1 = 0.50;
            const double a2 = 0.08;

            var w = a0 -
                    a1 * Math.Cos(2.0 * Math.PI * i / (n - 1)) +
                    a2 * Math.Cos(4.0 * Math.PI * i / (n - 1));

            return w;
        }

        private static double Hamming(int n, int i)
        {
            var x = Math.Cos(2.0 * Math.PI * i / (n - 1));
            var w = 0.54 - 0.46 * x;

            return w;
        }
    }
}
