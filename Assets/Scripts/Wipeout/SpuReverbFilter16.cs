using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [BurstCompile]
    public class SpuReverbFilter16 : MonoBehaviour
    {
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

        [SerializeField]
        private FilterState[] FiltersManaged;

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

            FiltersManaged = new[]
            {
                FilterState.CreateHalfBand(),
                FilterState.CreateHalfBand()
            };

            // TODO

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

            ReverbHandler = ReverbType switch
            {
                SpuReverbType.Off => null,
                SpuReverbType.Managed => Managed,
                SpuReverbType.BurstOld => BurstOld,
                SpuReverbType.BurstNew => BurstNew,
                SpuReverbType.Vectorized => Vectorized,
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

        private void Managed(float[] data, int channels)
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

        private unsafe void BurstOld(float[] data, int channels)
        {
            var sampleCount = data.Length / channels;

            fixed (float* pData = data)
            {
                for (var i = 0; i < channels; i++)
                {
                    var fState = FiltersManaged[i];
                    var hCount = fState.Coefficients.Length;
                    var tCount = fState.Taps.Length;

                    fixed (float* hArray = fState.Coefficients)
                    fixed (float* zArray = fState.DelayLine)
                    fixed (int* tArray = fState.Taps)
                    fixed (int* zState = &fState.Position)
                    {
                        BurstOld(
                            pData, i, channels, sampleCount, zState, hArray, hCount, zArray, tArray, tCount);
                    }
                }
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance, DisableSafetyChecks = true)]
        private static unsafe void BurstOld(
            float* data, int dataChannel, int dataChannels,
            int sampleCount, int* position,
            float* hArray, int hCount, float* zArray, int* tArray, int tCount
        )
        {
            var sample = &data[dataChannel];

            for (var i = 0; i < sampleCount; i++)
            {
                var index1 = *position;
                var index2 = *position + hCount;

                zArray[index1] = zArray[index2] = *sample;

                var filter = 0.0f;

                for (var j = 0; j < tCount; j++)
                {
                    var tap = tArray[j];

                    filter += hArray[tap] * zArray[index2 - tap];
                }

                index1++;

                if (index1 == hCount)
                {
                    index1 = 0;
                }

                *position = index1;

                *sample = filter;

                sample += dataChannels;
            }
        }

        private void BurstNew(float[] data, int channels)
        {
            ref var fs = ref NativeFilterState;

            // Marshal.Copy(data, 0, fs.Source, data.Length);

            Tests.ConvolveN(ref fs, data.Length / channels, channels);

            // Marshal.Copy(fs.Target, data, 0, data.Length);
        }

        #endregion

        #region Vectorized

        [SerializeField]
        private float4[] VectorizedH;

        [SerializeField]
        private float4[] VectorizedZ;

        [SerializeField]
        private int VectorizedP;

        private void VectorizedInit()
        {
            var f = FilterState.CreateHalfBand();

            var h = f.Coefficients;

            h = h.Where((_, t) => t % 2 == 1 || t == h.Length / 2).ToArray();

            var length = h.Length % 4;

            Array.Resize(ref h, h.Length + length);

            for (var i = 0; i < length; i++)
            {
                h[h.Length - length + i] = 0.0f;
            }

            h = VectorizedDuplicate(h, 2);

            var z = new float[h.Length * 2];
            
            VectorizedH = ConvertToFloat4Array(h);
            VectorizedZ = ConvertToFloat4Array(z);
        }

        private static float4[] ConvertToFloat4Array(float[] source)
        {
            Assert.AreEqual(0, source.Length % 4);
            
            var length = source.Length / 4;
            
            var result = new float4[length];

            for (var i = 0; i < length; i++)
            {
                var x = source[i * 4 + 0];
                var y = source[i * 4 + 1];
                var z = source[i * 4 + 2];
                var w = source[i * 4 + 3];
                
                result[i] = new float4(x, y, z, w);
            }

            return result;
        }

        private static T[] VectorizedDuplicate<T>(T[] source, int repeat)
        {
            var length = source.Length;
            var result = new T[length * repeat];
            var offset = 0;

            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < repeat; j++)
                {
                    result[offset++] = source[i];
                }
            }

            return result;
        }

        private void Vectorized(float[] data, int channels)
        {
            Assert.AreEqual(0, data.Length % 4);
            
            var span = MemoryMarshal.Cast<float, float4>(data);

            TestVectors.TestVectorization(span, VectorizedH, VectorizedZ, ref VectorizedP);
        }

        #endregion
    }
}