using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [NoReorder]
    public readonly struct NativeReverbPreset
    {
        private static double GetConstant(double sourceRate, double targetRate, short value)
        {
            var val = value / 32768.0d;

            var dt1 = 1.0d / sourceRate;
            var fc1 = 1.0d / (2.0d * MathF.PI * (dt1 / val - dt1));

            var dt2 = 1.0d / targetRate;
            var fc2 = 1.0d / (2.0d * MathF.PI * fc1);

            var iir = dt2 / (fc2 + dt2);

            return iir;
        }

        public NativeReverbPreset(SpuReverbPreset reverb)
        {
            // note that this works "by accident" for 44100Hz
            // any other sample rate needs an adjusted preset
            // unless you're an expert in reverb, just forget

            const int hop = 8 * 2 /* 44100Hz */;

            const float vol = 1.0f / 32768.0f;

            dAPF1   = hop * reverb.dAPF1;
            dAPF2   = hop * reverb.dAPF2;
            vIIR    = vol * reverb.vIIR;
            vCOMB1  = vol * reverb.vCOMB1;
            vCOMB2  = vol * reverb.vCOMB2;
            vCOMB3  = vol * reverb.vCOMB3;
            vCOMB4  = vol * reverb.vCOMB4;
            vWALL   = vol * reverb.vWALL;
            vAPF1   = vol * reverb.vAPF1;
            vAPF2   = vol * reverb.vAPF2;
            mLSAME  = hop * reverb.mLSAME;
            mRSAME  = hop * reverb.mRSAME;
            mLCOMB1 = hop * reverb.mLCOMB1;
            mRCOMB1 = hop * reverb.mRCOMB1;
            mLCOMB2 = hop * reverb.mLCOMB2;
            mRCOMB2 = hop * reverb.mRCOMB2;
            dLSAME  = hop * reverb.dLSAME;
            dRSAME  = hop * reverb.dRSAME;
            mLDIFF  = hop * reverb.mLDIFF;
            mRDIFF  = hop * reverb.mRDIFF;
            mLCOMB3 = hop * reverb.mLCOMB3;
            mRCOMB3 = hop * reverb.mRCOMB3;
            mLCOMB4 = hop * reverb.mLCOMB4;
            mRCOMB4 = hop * reverb.mRCOMB4;
            dLDIFF  = hop * reverb.dLDIFF;
            dRDIFF  = hop * reverb.dRDIFF;
            mLAPF1  = hop * reverb.mLAPF1;
            mRAPF1  = hop * reverb.mRAPF1;
            mLAPF2  = hop * reverb.mLAPF2;
            mRAPF2  = hop * reverb.mRAPF2;
            vLIN    = vol * reverb.vLIN;
            vRIN    = vol * reverb.vRIN;
        }

        public readonly int   dAPF1;
        public readonly int   dAPF2;
        public readonly float vIIR;
        public readonly float vCOMB1;
        public readonly float vCOMB2;
        public readonly float vCOMB3;
        public readonly float vCOMB4;
        public readonly float vWALL;
        public readonly float vAPF1;
        public readonly float vAPF2;
        public readonly int   mLSAME;
        public readonly int   mRSAME;
        public readonly int   mLCOMB1;
        public readonly int   mRCOMB1;
        public readonly int   mLCOMB2;
        public readonly int   mRCOMB2;
        public readonly int   dLSAME;
        public readonly int   dRSAME;
        public readonly int   mLDIFF;
        public readonly int   mRDIFF;
        public readonly int   mLCOMB3;
        public readonly int   mRCOMB3;
        public readonly int   mLCOMB4;
        public readonly int   mRCOMB4;
        public readonly int   dLDIFF;
        public readonly int   dRDIFF;
        public readonly int   mLAPF1;
        public readonly int   mRAPF1;
        public readonly int   mLAPF2;
        public readonly int   mRAPF2;
        public readonly float vLIN;
        public readonly float vRIN;
    }
}