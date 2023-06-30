using System;

namespace Wipeout.Formats.Audio.Extensions
{
    public static class Converters
    {
        public static int BytesToSamples(in int bytes, in int bits, in int channels)
        {
            var samples = bytes * 8 / bits / channels;

            return samples;
        }

        public static long BytesToSamples(in long bytes, in int bits, in int channels)
        {
            var samples = bytes * 8 / bits / channels;

            return samples;
        }

        public static int SamplesToBytes(in int samples, in int bits, in int channels)
        {
            var bytes = samples * bits * channels / 8;

            return bytes;
        }

        public static long SamplesToBytes(in long samples, in int bits, in int channels)
        {
            var bytes = samples * bits * channels / 8;

            return bytes;
        }

        public static short To16Bit(in float sample)
        {
            var clamp = Math.Clamp(sample, -1.0f, +1.0f);

            var value = (short)(clamp * 32767.0f);

            return value;
        }

        public static float To32Bit(in short sample)
        {
            var value = sample / 32768.0f;

            return value;
        }

        public static double DecibelToLinear(double db)
        {
            return Math.Pow(10.0, db / 20.0);
        }

        public static double LinearToDecibel(double linear)
        {
            return 20.0 * Math.Log10(linear);
        }
    }
}