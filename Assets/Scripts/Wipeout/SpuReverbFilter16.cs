using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        [Range(0.0f, 1.0f)]
        public float MixDry = 1.0f;

        [Range(0.0f, 1.0f)]
        public float MixWet = 1.0f;

        [Range(0.0f, 2.0f)]
        public float OutVol = 1.5f;

        [Space]
        [SerializeField]
        private SpuReverbType ReverbType;

        [Space]
        public bool ReverbProcessEnabled = true;

        public bool ReverbFilterEnabled = true;

        [Space]
        [Range(441, 21609)]
        public float ReverbLowPass = 11025;

        public SpuReverbQuality ReverbQuality = SpuReverbQuality.Highest;

        public FilterWindow ReverbWindow = FilterWindow.Blackman;

        [Space]
        [SerializeField]
        private ReverbFilterState ReverbFilterState = new();

        private readonly NativeReverb ReverbBurst = new(SpuReverbPreset.Hall);

        private SpuReverbFilter16Backup Reverb;

        private NativeReverbBuffer ReverbBuffer;

        private Filter[] ReverbFilters;

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

            var f = FilterState.CreateHalfBand();

            var h = f.Coefficients;

            h = h.Where((_, t) => t % 2 == 1 || t == h.Length / 2).ToArray();

            ReverbFilterState.Coefficients = h.Select(s => new float2(s)).ToArray();
            ReverbFilterState.Delays       = new float2[ReverbFilterState.Coefficients.Length * 2];

            ReverbBuffer = new NativeReverbBuffer(524288);
        }

        private void OnDisable()
        {
            if (enabled)
            {
                ReverbBuffer.Dispose();
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            ReverbHandler?.Invoke(data, channels);
        }

        private void OnValidate()
        {
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


            // initially, the PSX reverb only works at a sample rate of 22050Hz
            // but it turns out that it's possible to get it working at 44100Hz
            // problem #1: we must use that sample rate, fix: let OS do the SRC
            // problem #2: high hiss, fix: process input with a solid LP filter 

            // it is really close to the real thing and most users won't notice

            // have it 100% exact is IMPOSSIBLE: there are other things at play

            var coefficients = Filter.LowPass(44100, ReverbLowPass, (double)ReverbQuality, ReverbWindow);

            ReverbFilters = new[]
            {
                new Filter(coefficients),
                new Filter(coefficients)
            };

            //Debug.Log($"{ReverbLowPass}, {ReverbQuality}, {ReverbWindow}, {coefficients.Length}");
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

                if (ReverbFilterEnabled)
                {
                    filterL = (float)ReverbFilters[0].Process(filterL);
                    filterR = (float)ReverbFilters[1].Process(filterR);
                }

                var l1 = (short)(filterL * 32767.0f);
                var r1 = (short)(filterR * 32767.0f);

                var l2 = l1;
                var r2 = r1;

                if (ReverbProcessEnabled)
                {
                    Reverb.Process(l1, r1, out l2, out r2);
                }

                var l3 = sourceL * 0.5f * MixDry + l2 / 32768.0f * 0.5f * MixWet;
                var r3 = sourceR * 0.5f * MixDry + r2 / 32768.0f * 0.5f * MixWet;

                data[offsetL] = l3 * OutVol;
                data[offsetR] = r3 * OutVol;
            }
        }

        private unsafe void FilterBurst(float[] data, int channels)
        {
            var length = data.Length;

            Assert.AreEqual(0, length % 2);

            var state = ReverbFilterState;

            fixed (float* source = data)
            fixed (float* target = state.Buffer)
            fixed (float2* h = state.Coefficients)
            fixed (float2* z = state.Delays)
            {
                var samples = length / channels;
                var source2 = (float2*)source;
                var target2 = (float2*)target;

                FilterBurstImpl(source2, target2, samples, h, state.Coefficients.Length, z, ref state.Position);

                NewMethod(source2, target2, samples);
            }
        }

        [BurstCompile]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        private static unsafe void TestReverbBuffer(
            float2* source, float2* target, int length, ref NativeReverb reverb, ref NativeReverbBuffer buffer)
        {
            var dAPF1   = reverb.dAPF1;
            var dAPF2   = reverb.dAPF2;
            var vIIR    = reverb.vIIR;
            var vCOMB1  = reverb.vCOMB1;
            var vCOMB2  = reverb.vCOMB2;
            var vCOMB3  = reverb.vCOMB3;
            var vCOMB4  = reverb.vCOMB4;
            var vWALL   = reverb.vWALL;
            var vAPF1   = reverb.vAPF1;
            var vAPF2   = reverb.vAPF2;
            var mLSAME  = reverb.mLSAME;
            var mRSAME  = reverb.mRSAME;
            var mLCOMB1 = reverb.mLCOMB1;
            var mRCOMB1 = reverb.mRCOMB1;
            var mLCOMB2 = reverb.mLCOMB2;
            var mRCOMB2 = reverb.mRCOMB2;
            var dLSAME  = reverb.dLSAME;
            var dRSAME  = reverb.dRSAME;
            var mLDIFF  = reverb.mLDIFF;
            var mRDIFF  = reverb.mRDIFF;
            var mLCOMB3 = reverb.mLCOMB3;
            var mRCOMB3 = reverb.mRCOMB3;
            var mLCOMB4 = reverb.mLCOMB4;
            var mRCOMB4 = reverb.mRCOMB4;
            var dLDIFF  = reverb.dLDIFF;
            var dRDIFF  = reverb.dRDIFF;
            var mLAPF1  = reverb.mLAPF1;
            var mRAPF1  = reverb.mRAPF1;
            var mLAPF2  = reverb.mLAPF2;
            var mRAPF2  = reverb.mRAPF2;
            var vLIN    = reverb.vLIN;
            var vRIN    = reverb.vRIN;

            for (var i = 0; i < length; i++)
            {
                var LIn = vLIN * source[i].x;
                var RIn = vRIN * source[i].y;

                var L1 = buffer[mLSAME - 1];
                var R1 = buffer[mRSAME - 1];

                buffer[mLSAME] = Clamp((LIn + buffer[dLSAME] * vWALL - L1) * vIIR + L1);
                buffer[mRSAME] = Clamp((RIn + buffer[dRSAME] * vWALL - R1) * vIIR + R1);

                var L2 = buffer[mLDIFF - 1];
                var R2 = buffer[mRDIFF - 1];

                buffer[mLDIFF] = Clamp((LIn + buffer[dRDIFF] * vWALL - L2) * vIIR + L2);
                buffer[mRDIFF] = Clamp((RIn + buffer[dLDIFF] * vWALL - R2) * vIIR + R2);

                var LOut = vCOMB1 * buffer[mLCOMB1] + vCOMB2 * buffer[mLCOMB2] + vCOMB3 * buffer[mLCOMB3] + vCOMB4 * buffer[mLCOMB4];
                var ROut = vCOMB1 * buffer[mRCOMB1] + vCOMB2 * buffer[mRCOMB2] + vCOMB3 * buffer[mRCOMB3] + vCOMB4 * buffer[mRCOMB4];

                LOut = LOut - vAPF1 * buffer[mLAPF1 - dAPF1];
                ROut = ROut - vAPF1 * buffer[mRAPF1 - dAPF1];

                buffer[mLAPF1] = Clamp(LOut);
                buffer[mRAPF1] = Clamp(ROut);

                LOut = LOut * vAPF1 + buffer[mLAPF1 - dAPF1];
                ROut = ROut * vAPF1 + buffer[mRAPF1 - dAPF1];

                LOut = LOut - vAPF2 * buffer[mLAPF2 - dAPF2];
                ROut = ROut - vAPF2 * buffer[mRAPF2 - dAPF2];

                buffer[mLAPF2] = Clamp(LOut);
                buffer[mRAPF2] = Clamp(ROut);

                LOut = LOut * vAPF2 + buffer[mLAPF2 - dAPF2];
                ROut = ROut * vAPF2 + buffer[mRAPF2 - dAPF2];

                target[i].x = Clamp(LOut);
                target[i].y = Clamp(ROut);

                buffer.Advance();
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private static float Clamp(in float value)
        {
            const float minValue = -1.0f;
            const float maxValue = +1.0f;

            var clamp = math.clamp(value, minValue, maxValue);

            return clamp;
        }

        private unsafe void NewMethod(float2* source, float2* target, int samples)
        {
            //for (var i = 0; i < samples; i++)
            //{
            //    var sample = target[i];

            //    ReverbBurst.Process(sample.x, sample.y, out var l, out var r);

            //    var x = source[i].x * 0.5f * MixDry + l * 0.5f * MixWet;
            //    var y = source[i].y * 0.5f * MixDry + r * 0.5f * MixWet;

            //    source[i] = new float2(x, y);
            //}
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private static unsafe void FilterBurstImpl(
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
    }
}