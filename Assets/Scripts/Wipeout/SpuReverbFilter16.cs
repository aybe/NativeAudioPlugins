#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private SpuFilterState[] States;

        [SerializeField]
        private SpuReverbType ReverbType;

        private Filter[] Filters;

        private SpuReverbFilter16Backup Reverb;

        private SpuReverbHandler ReverbHandler;

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

        private void OnAudioFilterRead(float[] data, int channels)
        {
            ReverbHandler(data, channels);
        }

        private void OnValidate()
        {
            CreateFilters();

            ReverbHandler = ReverbType switch
            {
                SpuReverbType.Old => ProcessAudio,
                SpuReverbType.New => NewMethod,
                SpuReverbType.New2 => NewMethod2,
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

            States = new[]
            {
                new SpuFilterState(Array.ConvertAll(coefficients, Convert.ToSingle)),
                new SpuFilterState(Array.ConvertAll(coefficients, Convert.ToSingle))
            };
        }

        private void NewMethod(float[] data, int channels)
        {
            var sampleCount = data.Length / channels;
            var sampleIndex = 0;

            for (var j = 0; j < sampleCount; j++)
            {
                for (var i = 0; i < channels; i++)
                {
                    var f = States[i];
                    var z = f.Delay;
                    var n = z.Length;
                    var h = f.Input;

                    ref var sample = ref data[sampleIndex];

                    sample = fir_double_h(sample, n, h, z, ref f.Index);

                    sampleIndex++;
                }
            }
        }

        private unsafe void NewMethod2(float[] data, int channels)
        {
            var sampleCount = data.Length / channels;
            var sampleIndex = 0;

            for (var j = 0; j < sampleCount; j++)
            {
                for (var i = 0; i < channels; i++)
                {
                    var f = States[i];
                    var z = f.Delay;
                    var n = z.Length;
                    var h = f.Input;

                    fixed (float* ph = h)
                    fixed (float* pz = z)
                    fixed (int* pi = &f.Index)
                    {
                        ref var sample = ref data[sampleIndex];

                        sample = fir_double_h(sample, n, ph, pz, pi);

                        sampleIndex++;
                    }
                }
            }
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static float fir_double_h(float input, int ntaps, float[] h, float[] z, ref int p_state)
            /****************************************************************************
            * fir_double_h: This uses doubled coefficients (supplied by caller) so that 
            * the filter calculation always operates on a flat buffer.
            *****************************************************************************/
        {
            var state = p_state;

            /* store input at the beginning of the delay line */
            z[state] = input;

            /* calculate the filter */
            var accum = 0.0f;
            for (var i = 0; i < ntaps; i++)
            {
                accum += h[ntaps - state + i] * z[i];
            }

            /* decrement state, wrapping if below zero */
            if (--state < 0)
            {
                state += ntaps;
            }

            p_state = state; /* return new state to caller */

            return accum;
        }

        [BurstCompile(CompileSynchronously = true)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static unsafe float fir_double_h(float input, int ntaps, float* h, float* z, int* p_state)
        /****************************************************************************
        * fir_double_h: This uses doubled coefficients (supplied by caller) so that 
        * the filter calculation always operates on a flat buffer.
        *****************************************************************************/
        {
            var state = *p_state;

            /* store input at the beginning of the delay line */
            z[state] = input;

            /* calculate the filter */
            var accum = 0.0f;
            for (var i = 0; i < ntaps; i++)
            {
                accum += h[ntaps - state + i] * z[i];
            }

            /* decrement state, wrapping if below zero */
            if (--state < 0)
            {
                state += ntaps;
            }

            *p_state = state; /* return new state to caller */

            return accum;
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
    }

    [Serializable]
    internal sealed class SpuFilterState
    {
        public float[] Input;
        public float[] Delay;
        public int     Index;

        public SpuFilterState(float[] input)
        {
            Input = input.Concat(input).ToArray();
            Delay = new float[input.Length];
            Index = 0;
        }
    }

    internal delegate void SpuReverbHandler(float[] data, int channels);

    internal enum SpuReverbType
    {
        Old,
        New,
        New2
    }
}