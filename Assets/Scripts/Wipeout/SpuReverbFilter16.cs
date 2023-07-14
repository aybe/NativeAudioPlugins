#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Serialization;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [BurstCompile]
    public class SpuReverbFilter16 : MonoBehaviour
    {
        private unsafe void FilterUnsafeStaticBurst(float[] data, int channels)
        {
            var sampleCount = data.Length / channels;

            for (var i = 0; i < channels; i++)
            {
                var fState = FiltersManaged[i];
                var hCount = fState.Coefficients.Length;
                var tCount = fState.Taps.Length;
                var zCount = fState.DelayLine.Length;

                fixed (float* pData = data)
                fixed (float* hArray = fState.Coefficients)
                fixed (float* zArray = fState.DelayLine)
                fixed (int* tArray = fState.Taps)
                fixed (int* zState = &fState.Position)
                {
                    FilterWithPointersBurst(
                        pData, i, channels, sampleCount, zState, hArray, hCount, zArray, zCount, tArray, tCount);
                }
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private static unsafe void FilterWithPointersBurst(
            float* data, int dataChannel, int dataChannels,
            int sampleCount, int* position,
            float* hArray, int hCount, float* zArray, int zCount, int* tArray, int tCount
        )
        {
            if (zCount != hCount * 2)
            {
                return;
            }

            var sample = &data[dataChannel];

            for (var j = 0; j < sampleCount; j++)
            {
                var index1 = *position;
                var index2 = *position + hCount;

                zArray[index1] = zArray[index2] = *sample;

                var filter = 0.0f;

                for (var pos = 0; pos < tCount; pos++)
                {
                    var tap = tArray[pos];

                    filter += hArray[tap] * zArray[index2 - tap];
                }

                index1++;

                if (index1 >= hCount)
                {
                    index1 = 0;
                }

                *position = index1;

                *sample = filter;

                sample += dataChannels;
            }
        }

        private void FilterUnsafeStaticBurstNew(float[] data, int channels)
        {
            ref var fs = ref NativeFilterState;

            Marshal.Copy(data, 0, fs.Source, data.Length);

            Tests.Convolve(ref fs, data.Length / channels, channels);

            Marshal.Copy(fs.Target, data, 0, data.Length);
        }

        #region Fields

        [FormerlySerializedAs("FilterEnabled")]
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
                SpuReverbType.Old => ProcessAudio,
                SpuReverbType.BurstOld => FilterUnsafeStaticBurst,
                SpuReverbType.BurstNew => FilterUnsafeStaticBurstNew,
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

            Debug.Log($"{LowPass}, {Quality}, {Window}, {coefficients.Length}");
        }

        private void ProcessAudio(float[] data, int channels)
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
    }
}