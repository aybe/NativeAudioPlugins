using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [BurstCompile]
    public class SpuReverbFilter16 : MonoBehaviour
    {
        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private static unsafe void TestVectorization2(
            float2* source, float2* target, int samples, float2* h, int taps, float2* z, ref int state)
        {
            for (var i = 0; i < samples; i++)
            {
                z[state] = z[state + taps] = source[i];

                var sample = float2.zero;

                for (var j = 0; j < taps; j++)
                {
                    sample = math.mad(h[j], z[state + j], sample);
                }

                --state;

                if (state < 0)
                {
                    state += taps;
                }

                target[i] = sample;
            }
        }

        #region Fields

        public bool ApplyFilter = true;

        public bool ApplyReverb = true;

        [Range(441, 21609)]
        public float LowPass = 11025;

        [Range(0.0f, 1.0f)]
        public float MixDry = 1.0f;

        [Range(0.0f, 1.0f)]
        public float MixWet = 1.0f;

        [Range(0.0f, 2.0f)]
        public float OutVol = 1.5f;

        public SpuReverbQuality Quality = SpuReverbQuality.Highest;

        public FilterWindow Window = FilterWindow.Blackman;

        [SerializeField]
        private SpuReverbType ReverbType;

        private NativeFilter2 Filter2 = null!;

        private Filter[] Filters;

        private NativeFilter[] NativeFilters;

        private SpuReverbFilter16Backup Reverb;

        private SpuReverbHandler ReverbHandler;

        private NativeFilterState NativeFilterState;

        #endregion

        #region Methods

        private void OnEnable()
        {
            if (AudioSettings.outputSampleRate != 44100 || AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
            {
                Debug.LogWarning("PSX reverb requires a sample rate of 44100Hz and stereo speakers, disabling.");
                enabled = false;
            }

            OnValidate();

            Reverb = new SpuReverbFilter16Backup(SpuReverbPreset.Hall); // this is the EXACT preset they've used

            NativeFilters = new[]
            {
                new NativeFilter(FilterState.CreateHalfBand()),
                new NativeFilter(FilterState.CreateHalfBand())
            };

            Filter2 = new NativeFilter2(FilterState.CreateHalfBand(), 2);

            var f = FilterState.CreateHalfBand();

            NativeFilterState = new NativeFilterState
            {
                Source       = UnsafeBufferUtility.Allocate(new float[44100 * 2]),
                Target       = UnsafeBufferUtility.Allocate(new float[44100 * 2]),
                Coefficients = UnsafeBufferUtility.Allocate(new[] { f.Coefficients, f.Coefficients }),
                Delays       = UnsafeBufferUtility.Allocate(new[] { f.DelayLine, f.DelayLine }),
                Taps         = UnsafeBufferUtility.Allocate(new[] { f.Taps, f.Taps }),
                Positions    = UnsafeBufferUtility.Allocate(new[] { new[] { 0 }, new[] { 0 } })
            };

            VectorizedInit();
        }

        private void OnDisable()
        {
            if (!enabled)
            {
                return;
            }

            Filter2.Dispose();

            foreach (var nativeFilter in NativeFilters)
            {
                nativeFilter.Dispose();
            }

            NativeFilterState.Dispose();
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            ReverbHandler?.Invoke(data, channels);
        }

        private void OnValidate()
        {
            CreateFilters();

            if (!Enum.IsDefined(typeof(SpuReverbType), ReverbType))
            {
                ReverbType = SpuReverbType.Off;
            }

            ReverbHandler = ReverbType switch
            {
                SpuReverbType.Off => null,
                SpuReverbType.Managed => FilterManaged,
                SpuReverbType.Burst => FilterBurst,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void CreateFilters()
        {
            // initially, the PSX reverb only works at a sample rate of 22050Hz
            // but it turns out that it's possible to get it working at 44100Hz
            // problem #1: we must use that sample rate, fix: let OS do the SRC
            // problem #2: high hiss, fix: process input with a solid LP filter 

            // it is really close to the real thing and most users won't notice

            // have it 100% exact is IMPOSSIBLE: there are other things at play

            var coefficients = Filter.LowPass(44100, LowPass, (double)Quality, Window);

            Filters = new[]
            {
                new Filter(coefficients),
                new Filter(coefficients)
            };

            //Debug.Log($"{LowPass}, {Quality}, {Window}, {coefficients.Length}");
        }

        private void FilterManaged(float[] data, int channels)
        {
            // we always do the processing to avoid delay in chain

            var samples = data.Length / channels;

            for (var i = 0; i < samples; i++)
            {
                var offsetL = i * channels + 0;
                var offsetR = i * channels + 1;

                var sourceL = data[offsetL];
                var sourceR = data[offsetR];

                var filterL = sourceL;
                var filterR = sourceR;

                if (ApplyFilter)
                {
                    filterL = (float)Filters[0].Process(filterL);
                    filterR = (float)Filters[1].Process(filterR);
                }

                var l1 = (short)(filterL * 32767.0f);
                var r1 = (short)(filterR * 32767.0f);

                var l2 = l1;
                var r2 = r1;

                if (ApplyReverb)
                {
                    Reverb.Process(l1, r1, out l2, out r2);
                }

                var l3 = sourceL * 0.5f * MixDry + l2 / 32768.0f * 0.5f * MixWet;
                var r3 = sourceR * 0.5f * MixDry + r2 / 32768.0f * 0.5f * MixWet;

                data[offsetL] = l3 * OutVol;
                data[offsetR] = r3 * OutVol;
            }
        }

        #endregion

        #region Vectorized

        [SerializeField]
        private ReverbFilterState RFS = new();

        private void VectorizedInit()
        {
            var f = FilterState.CreateHalfBand();

            var h = f.Coefficients;

            h = h.Where((_, t) => t % 2 == 1 || t == h.Length / 2).ToArray();

            RFS.Coefficients = h.Select(s => new float2(s)).ToArray();
            RFS.Delays       = new float2[RFS.Coefficients.Length * 2];
        }


        private unsafe void FilterBurst(float[] data, int channels)
        {
            var length = data.Length;

            Assert.AreEqual(0, length % 2);

            fixed (float* source = data)
            fixed (float* target = RFS.Buffer)
            fixed (float2* h = RFS.Coefficients)
            fixed (float2* z = RFS.Delays)
            {
                var samples = length / channels;

                TestVectorization2((float2*)source, (float2*)target, samples, h, RFS.Coefficients.Length, z,
                    ref RFS.Position);

                UnsafeUtility.MemCpy(source, target, length * sizeof(float));
            }
        }

        #endregion
    }
}