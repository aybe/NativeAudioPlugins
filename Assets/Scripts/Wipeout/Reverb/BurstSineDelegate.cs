﻿namespace Wipeout.Reverb
{
    internal delegate double BurstSineDelegate(
        ref SynthBuffer buffer, double phase, int sampleRate, float amplitude, float frequency
    );
}