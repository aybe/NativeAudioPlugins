using UnityEngine;
using UnityEngine.Serialization;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    internal class SpuReverbFilter16 : MonoBehaviour
    {
        public enum bw
        {
            k441  = 441,
            k882  = 882,
            k1323 = 1323,
            k1764 = 1764,
            k2205 = 2205,
            k4410 = 4410
        }

        [Range(0.0f, 1.0f)] public float Dry = 1.0f;

        [Range(0.0f, 1.0f)]    public float Wet = 1.0f;
        [Range(-10.0f, 10.0f)] public float Vol = 1.5f;

        [Range(1, 8)] public int Step = 2;

        public bool ApplyFilter = true;

        [Space] [Range(441, 21609)] public float LowPass = 11025;

        public bw TransitionBandwidth = bw.k441;

        public FilterWindow FilterWindow = FilterWindow.Blackman;

        private SpuReverbFilter16Backup Backup;

        private Filter[] Filters;

        private void Update()
        {
            Backup.Step = Step;
        }

        private void OnEnable()
        {
            OnValidate();

            Backup = new SpuReverbFilter16Backup { Reverb = SpuReverbPreset.Hall };
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
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

                Backup.Process(l1, r1, out var l2, out var r2);

                var l3 = sourceL * 0.5f * Dry + l2 / 32767.0f * 0.5f * Wet;
                var r3 = sourceR * 0.5f * Dry + r2 / 32767.0f * 0.5f * Wet;

                data[offsetL] = l3*Vol;
                data[offsetR] = r3*Vol;
            }
        }

        private void OnValidate()
        {
            var coefficients = Filter.LowPass(44100, LowPass, (double)TransitionBandwidth, FilterWindow);

            Filters = new[]
            {
                new Filter(coefficients),
                new Filter(coefficients)
            };

            Debug.Log($"{LowPass}, {TransitionBandwidth}, {FilterWindow}, {coefficients.Length}");
        }
    }
}