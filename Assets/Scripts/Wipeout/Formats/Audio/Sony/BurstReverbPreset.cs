using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Wipeout.Formats.Audio.Sony
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public readonly struct BurstReverbPreset
    {
        public BurstReverbPreset(SpuReverbPreset reverb)
        {
            // note that this works "by accident" for 44100Hz
            // any other sample rate needs an adjusted preset
            // unless you're an expert in reverb, just forget

            const int step = 8 * 2 /* 44100Hz */;

            dAPF1   = step * reverb.dAPF1;
            dAPF2   = step * reverb.dAPF2;
            vIIR    = reverb.vIIR;
            vCOMB1  = reverb.vCOMB1;
            vCOMB2  = reverb.vCOMB2;
            vCOMB3  = reverb.vCOMB3;
            vCOMB4  = reverb.vCOMB4;
            vWALL   = reverb.vWALL;
            vAPF1   = reverb.vAPF1;
            vAPF2   = reverb.vAPF2;
            mLSAME  = step * reverb.mLSAME;
            mRSAME  = step * reverb.mRSAME;
            mLCOMB1 = step * reverb.mLCOMB1;
            mRCOMB1 = step * reverb.mRCOMB1;
            mLCOMB2 = step * reverb.mLCOMB2;
            mRCOMB2 = step * reverb.mRCOMB2;
            dLSAME  = step * reverb.dLSAME;
            dRSAME  = step * reverb.dRSAME;
            mLDIFF  = step * reverb.mLDIFF;
            mRDIFF  = step * reverb.mRDIFF;
            mLCOMB3 = step * reverb.mLCOMB3;
            mRCOMB3 = step * reverb.mRCOMB3;
            mLCOMB4 = step * reverb.mLCOMB4;
            mRCOMB4 = step * reverb.mRCOMB4;
            dLDIFF  = step * reverb.dLDIFF;
            dRDIFF  = step * reverb.dRDIFF;
            mLAPF1  = step * reverb.mLAPF1;
            mRAPF1  = step * reverb.mRAPF1;
            mLAPF2  = step * reverb.mLAPF2;
            mRAPF2  = step * reverb.mRAPF2;
            vLIN    = reverb.vLIN;
            vRIN    = reverb.vRIN;
        }

        private readonly int   dAPF1;
        private readonly int   dAPF2;
        private readonly short vIIR;
        private readonly short vCOMB1;
        private readonly short vCOMB2;
        private readonly short vCOMB3;
        private readonly short vCOMB4;
        private readonly short vWALL;
        private readonly short vAPF1;
        private readonly short vAPF2;
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
        private readonly short vLIN;
        private readonly short vRIN;
        private const    short vLOUT = short.MaxValue;
        private const    short vROUT = short.MaxValue;

        [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
        [SuppressMessage("Style", "IDE0054:Use compound assignment")]
        public static void Process(
            in short sourceL, in short sourceR, out short targetL, out short targetR, ref BurstReverbPreset p, ref BurstReverbBuffer b)
        {
            const int div = 0x8000;

            var LIn = p.vLIN * sourceL / div;
            var RIn = p.vRIN * sourceR / div;

            var L1 = b[p.mLSAME - 1];
            var R1 = b[p.mRSAME - 1];

            b[p.mLSAME] = Clamp((LIn + b[p.dLSAME] * p.vWALL / div - L1) * p.vIIR / div + L1);
            b[p.mRSAME] = Clamp((RIn + b[p.dRSAME] * p.vWALL / div - R1) * p.vIIR / div + R1);

            var L2 = b[p.mLDIFF - 1];
            var R2 = b[p.mRDIFF - 1];

            b[p.mLDIFF] = Clamp((LIn + b[p.dRDIFF] * p.vWALL / div - L2) * p.vIIR / div + L2);
            b[p.mRDIFF] = Clamp((RIn + b[p.dLDIFF] * p.vWALL / div - R2) * p.vIIR / div + R2);

            var LOut = p.vCOMB1 * b[p.mLCOMB1] / div +
                       p.vCOMB2 * b[p.mLCOMB2] / div +
                       p.vCOMB3 * b[p.mLCOMB3] / div +
                       p.vCOMB4 * b[p.mLCOMB4] / div;

            var ROut = p.vCOMB1 * b[p.mRCOMB1] / div +
                       p.vCOMB2 * b[p.mRCOMB2] / div +
                       p.vCOMB3 * b[p.mRCOMB3] / div +
                       p.vCOMB4 * b[p.mRCOMB4] / div;

            LOut = LOut - p.vAPF1 * b[p.mLAPF1 - p.dAPF1] / div;
            ROut = ROut - p.vAPF1 * b[p.mRAPF1 - p.dAPF1] / div;

            b[p.mLAPF1] = Clamp(LOut);
            b[p.mRAPF1] = Clamp(ROut);

            LOut = LOut * p.vAPF1 / div + b[p.mLAPF1 - p.dAPF1];
            ROut = ROut * p.vAPF1 / div + b[p.mRAPF1 - p.dAPF1];

            LOut = LOut - p.vAPF2 * b[p.mLAPF2 - p.dAPF2] / div;
            ROut = ROut - p.vAPF2 * b[p.mRAPF2 - p.dAPF2] / div;

            b[p.mLAPF2] = Clamp(LOut);
            b[p.mRAPF2] = Clamp(ROut);

            LOut = LOut * p.vAPF2 / div + b[p.mLAPF2 - p.dAPF2];
            ROut = ROut * p.vAPF2 / div + b[p.mRAPF2 - p.dAPF2];

            targetL = Clamp(LOut * vLOUT / div);
            targetR = Clamp(ROut * vROUT / div);

            b.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short Clamp(in int value)
        {
            const short minValue = short.MinValue;
            const short maxValue = short.MaxValue;
            math.clamp(value, short.MinValue, short.MaxValue); // todo
            return value < minValue ? minValue : value > maxValue ? maxValue : (short)value;
        }
    }
}