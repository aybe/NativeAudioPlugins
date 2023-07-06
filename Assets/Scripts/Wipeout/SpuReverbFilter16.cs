using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Burst;
using UnityEngine;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [BurstCompile]
    public class SpuReverbFilter16 : MonoBehaviour
    {
        public bool FilterEnabled = true;

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
        private FilterState2[] FilterStates;

        private Filter[] Filters;

        private SpuReverbFilter16Backup Reverb;

        private void OnEnable()
        {
            if (AudioSettings.outputSampleRate != 44100 || AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
            {
                Debug.LogWarning("PSX reverb requires a sample rate of 44100Hz and stereo speakers, disabling.");
                enabled = false;
            }

            OnValidate();

            Reverb = new SpuReverbFilter16Backup(SpuReverbPreset.Hall); // this is the EXACT preset they've used
        }

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private void OnAudioFilterRead(float[] data, int channels)
        {
            {
                var sampleCount = data.Length / channels;
                var sampleIndex = 0;

                for (var i = 0; i < channels; i++)
                {
                    ref var s = ref FilterStates[i];
                    ref var n = ref s.Taps;
                    ref var h = ref s.Coefficients;
                    ref var z = ref s.Delay;
                    ref var p = ref s.Index;

                    for (var j = 0; j < sampleCount; j++)
                    {
                        ref var sample = ref data[sampleIndex++];

                        z[p] = z[p + n] = sample;
                        
                        var result = 0.0f;

                        for (var k = 0; k < n; k++)
                        {
                            result += h[k] * z[k + p];
                        }

                        if (--p < 0)
                        {
                            p += n;
                        }

                        sample = result;
                    }
                }
            }

            return;

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

                    if (FilterEnabled)
                    {
                        filterL = (float)Filters[0].Process(filterL);
                        filterR = (float)Filters[1].Process(filterR);
                    }

                    var l1 = (short)(filterL * 32767.0f);
                    var r1 = (short)(filterR * 32767.0f);

                    Reverb.Process(l1, r1, out var l2, out var r2);

                    var l3 = sourceL * 0.5f * MixDry + l2 / 32768.0f * 0.5f * MixWet;
                    var r3 = sourceR * 0.5f * MixDry + r2 / 32768.0f * 0.5f * MixWet;

                    data[offsetL] = l3 * OutVol;
                    data[offsetR] = r3 * OutVol;
                }
            }
        }

        private void OnValidate()
        {
            // initially, the PSX reverb only works at a sample rate of 22050Hz
            // but it turns out that it's possible to get it working at 44100Hz
            // problem #1: we must use that sample rate, fix: let OS do the SRC
            // problem #2: high hiss, fix: process input with a solid LP filter 

            // it is really close to the real thing and most users won't notice

            // have it 100% exact is IMPOSSIBLE: there are other things at play

            var coefficients = Filter.LowPass(44100, LowPass, (double)Quality, Window); // TODO

            Filters = new[]
            {
                new Filter(coefficients),
                new Filter(coefficients)
            };

            Debug.Log($"{LowPass}, {Quality}, {Window}, {coefficients.Length}");

            FilterStates = new[]
            {
                new FilterState2(Array.ConvertAll(coefficients, Convert.ToSingle)),
                new FilterState2(Array.ConvertAll(coefficients, Convert.ToSingle))
            };
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum SpuReverbQuality
    {
        Highest = 441,
        High    = 882,
        K1323   = 1323,
        K1764   = 1764,
        K2205   = 2205,
        K4410   = 4410
    }

    [Serializable]
    internal sealed class FilterState2
    {
        public float[] Coefficients;
        public float[] Delay;
        public int     Index;
        public int     Taps;

        public FilterState2(float[] coefficients)
        {
            Coefficients = coefficients.ToArray();
            Delay        = new float[coefficients.Length * 2];
            //Index        = coefficients.Length - 1;
            Taps         = coefficients.Length;
        }

        public override string ToString()
        {
            return $"{nameof(Index)}: {Index}";
        }
    }
}