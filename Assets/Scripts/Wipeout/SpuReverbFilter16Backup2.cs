using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Mathematics;
using Wipeout.Formats.Audio.Sony;

namespace Wipeout
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [NoReorder]
    public sealed class SpuReverbFilter16Backup2
    {
        public SpuReverbFilter16Backup2(SpuReverbPreset reverb)
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

        private readonly int   dAPF1;
        private readonly int   dAPF2;
        private readonly float vIIR;
        private readonly float vCOMB1;
        private readonly float vCOMB2;
        private readonly float vCOMB3;
        private readonly float vCOMB4;
        private readonly float vWALL;
        private readonly float vAPF1;
        private readonly float vAPF2;
        private readonly int   mLSAME;
        private readonly int   mRSAME;
        private readonly int   mLCOMB1;
        private readonly int   mRCOMB1;
        private readonly int   mLCOMB2;
        private readonly int   mRCOMB2;
        private readonly int   dLSAME;
        private readonly int   dRSAME;
        private readonly int   mLDIFF;
        private readonly int   mRDIFF;
        private readonly int   mLCOMB3;
        private readonly int   mRCOMB3;
        private readonly int   mLCOMB4;
        private readonly int   mRCOMB4;
        private readonly int   dLDIFF;
        private readonly int   dRDIFF;
        private readonly int   mLAPF1;
        private readonly int   mRAPF1;
        private readonly int   mLAPF2;
        private readonly int   mRAPF2;
        private readonly float vLIN;
        private readonly float vRIN;
        private const    float vLOUT = 1.0f;
        private const    float vROUT = 1.0f;

        private readonly SpuReverbBuffer<float> Buffer = new(524288);

        [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
        [SuppressMessage("Style", "IDE0054:Use compound assignment")]
        public void Process(in float sourceL, in float sourceR, out float targetL, out float targetR)
        {
            const int div = 0x8000;

            var LIn = vLIN * sourceL /* / div*/;
            var RIn = vRIN * sourceR /* / div*/;

            var L1 = Buffer[mLSAME - 1];
            var R1 = Buffer[mRSAME - 1];

            Buffer[mLSAME] = Clamp((LIn + Buffer[dLSAME] * vWALL /* / div*/ - L1) * vIIR /* / div*/ + L1);
            Buffer[mRSAME] = Clamp((RIn + Buffer[dRSAME] * vWALL /* / div*/ - R1) * vIIR /* / div*/ + R1);

            var L2 = Buffer[mLDIFF - 1];
            var R2 = Buffer[mRDIFF - 1];

            Buffer[mLDIFF] = Clamp((LIn + Buffer[dRDIFF] * vWALL /* / div*/ - L2) * vIIR /* / div*/ + L2);
            Buffer[mRDIFF] = Clamp((RIn + Buffer[dLDIFF] * vWALL /* / div*/ - R2) * vIIR /* / div*/ + R2);

            var LOut = vCOMB1 * Buffer[mLCOMB1] /* / div*/ +
                       vCOMB2 * Buffer[mLCOMB2] /* / div*/ +
                       vCOMB3 * Buffer[mLCOMB3] /* / div*/ +
                       vCOMB4 * Buffer[mLCOMB4] /* / div*/;

            var ROut = vCOMB1 * Buffer[mRCOMB1] /* / div*/ +
                       vCOMB2 * Buffer[mRCOMB2] /* / div*/ +
                       vCOMB3 * Buffer[mRCOMB3] /* / div*/ +
                       vCOMB4 * Buffer[mRCOMB4] /* / div*/;

            LOut = LOut - vAPF1 * Buffer[mLAPF1 - dAPF1] /* / div*/;
            ROut = ROut - vAPF1 * Buffer[mRAPF1 - dAPF1] /* / div*/;

            Buffer[mLAPF1] = Clamp(LOut);
            Buffer[mRAPF1] = Clamp(ROut);

            LOut = LOut * vAPF1 /* / div*/ + Buffer[mLAPF1 - dAPF1];
            ROut = ROut * vAPF1 /* / div*/ + Buffer[mRAPF1 - dAPF1];

            LOut = LOut - vAPF2 * Buffer[mLAPF2 - dAPF2] /* / div*/;
            ROut = ROut - vAPF2 * Buffer[mRAPF2 - dAPF2] /* / div*/;

            Buffer[mLAPF2] = Clamp(LOut);
            Buffer[mRAPF2] = Clamp(ROut);

            LOut = LOut * vAPF2 /* / div*/ + Buffer[mLAPF2 - dAPF2];
            ROut = ROut * vAPF2 /* / div*/ + Buffer[mRAPF2 - dAPF2];

            targetL = Clamp(LOut /* * vLOUT*/ /* / div*/);
            targetR = Clamp(ROut /* * vROUT*/ /* / div*/);

            Buffer.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(in float value)
        {
            const float minValue = -1.0f;
            const float maxValue = +1.0f;

            var clamp = math.clamp(value, minValue, maxValue);

            return clamp;
        }
    }
}