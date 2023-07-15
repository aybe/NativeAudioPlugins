using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Wipeout
{
    [BurstCompile]
    public static class TestVectors
    {
        [BurstCompile(Debug = true)]
        private static unsafe float TestVectorization(float4* src, float4* tgt, int len)
        {
            var c = new float4(0, 0, 0, 0);

            for (var i = 0; i < len; i++)
            {
                var a = *src;
                var b = *tgt;
                c = math.mad(a, b, c);
                src++;
                tgt++;
            }

            return math.length(c);
        }

        // [BurstCompile(Debug = true)]
        public static void TestVectorization4(
            Span<float4> samples, Span<float4> h, Span<float4> z, ref int zState)
        {
            var taps = h.Length;

            for (var i = 0; i < samples.Length; i++)
            {
                ref var sample = ref samples[i];

                z[zState] = z[zState + taps] = sample;

                var filter = float4.zero;

                for (var j = 0; j < taps; j++)
                {
                    filter += h[j] * z[zState + j];
                }

                --zState;

                if (zState < 0)
                {
                    zState += taps;
                }

                sample = filter;
            }
        }

        public static void TestVectorization2(
            Span<float2> samples, Span<float> h, Span<float2> z, ref int zState)
        {
            var taps = h.Length;

            for (var i = 0; i < samples.Length; i++)
            {
                ref var sample = ref samples[i];

                z[zState] = z[zState + taps] = sample;

                var filter = float2.zero;

                for (var j = 0; j < taps; j++)
                {
                    filter += h[j] * z[zState + j];
                }

                --zState;

                if (zState < 0)
                {
                    zState += taps;
                }

                sample = filter;
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public static unsafe void TestVectorization2(
            float2* sampleArray, int sampleCount, float* h, int taps, float2* z, ref int state)
        {
            for (var i = 0; i < sampleCount; i++)
            {
                ref var sample = ref sampleArray[i];

                z[state] = z[state + taps] = sample;

                var filter = float2.zero;

                for (var j = 0; j < taps; j++)
                {
                    filter = math.mad(h[j], z[state + j], filter);
                }

                --state;

                if (state < 0)
                {
                    state += taps;
                }

                sample = filter;
            }
        }
    }
}