using UnityEngine;
using Wipeout.Extensions;
using Wipeout.Formats.Audio.Extensions;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    internal class SpuReverbFilter : MonoBehaviour
    {
        private Filter[] Filters;

        private SpuReverb Reverb;

        private void OnEnable()
        {
            var fs = AudioSettings.outputSampleRate;
            var lp = Filter.LowPass(fs, 11025.0d, fs * 0.01d, FilterWindow.Blackman);

            Filters = Arrays.Create(2, () => new Filter(lp));

            Reverb = new SpuReverb(SpuReverbPreset.Hall, fs);
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

                var filterL = (float)Filters[0].Process(sourceL);
                var filterR = (float)Filters[1].Process(sourceR);

                filterL = sourceL;
                filterR = sourceR;

                Reverb.Process(filterL, filterR, out var targetL, out var targetR);

                data[offsetL] = sourceL * 0.5f + targetL * 0.5f;
                data[offsetR] = sourceR * 0.5f + targetR * 0.5f;
            }
        }
    }
}