using System;
using System.Diagnostics.CodeAnalysis;
using Wipeout.Formats.Audio.Sony;

// ReSharper disable RedundantIfElseBlock

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public sealed class SpuReverbFilter16Backup
{
    public int Step
        ;
    private SpuReverbBuffer<int> Buffer { get; } = new(131072 * 4);

    public SpuReverbPreset Reverb { get; init; }

    [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
    [SuppressMessage("Style", "IDE0054:Use compound assignment", Justification = "<Pending>")]
    public void Process(in short sourceL, in short sourceR, out short targetL, out short targetR)
    {
        var dAPF1   = 8 * Step * Reverb.dAPF1;
        var dAPF2   = 8 * Step * Reverb.dAPF2;
        var vIIR    = Reverb.vIIR;
        var vCOMB1  = Reverb.vCOMB1;
        var vCOMB2  = Reverb.vCOMB2;
        var vCOMB3  = Reverb.vCOMB3;
        var vCOMB4  = Reverb.vCOMB4;
        var vWALL   = Reverb.vWALL;
        var vAPF1   = Reverb.vAPF1;
        var vAPF2   = Reverb.vAPF2;
        var mLSAME  = 8 * Step * Reverb.mLSAME;
        var mRSAME  = 8 * Step * Reverb.mRSAME;
        var mLCOMB1 = 8 * Step * Reverb.mLCOMB1;
        var mRCOMB1 = 8 * Step * Reverb.mRCOMB1;
        var mLCOMB2 = 8 * Step * Reverb.mLCOMB2;
        var mRCOMB2 = 8 * Step * Reverb.mRCOMB2;
        var dLSAME  = 8 * Step * Reverb.dLSAME;
        var dRSAME  = 8 * Step * Reverb.dRSAME;
        var mLDIFF  = 8 * Step * Reverb.mLDIFF;
        var mRDIFF  = 8 * Step * Reverb.mRDIFF;
        var mLCOMB3 = 8 * Step * Reverb.mLCOMB3;
        var mRCOMB3 = 8 * Step * Reverb.mRCOMB3;
        var mLCOMB4 = 8 * Step * Reverb.mLCOMB4;
        var mRCOMB4 = 8 * Step * Reverb.mRCOMB4;
        var dLDIFF  = 8 * Step * Reverb.dLDIFF;
        var dRDIFF  = 8 * Step * Reverb.dRDIFF;
        var mLAPF1  = 8 * Step * Reverb.mLAPF1;
        var mRAPF1  = 8 * Step * Reverb.mRAPF1;
        var mLAPF2  = 8 * Step * Reverb.mLAPF2;
        var mRAPF2  = 8 * Step * Reverb.mRAPF2;
        var vLIN    = Reverb.vLIN;
        var vRIN    = Reverb.vRIN;
        
        var inL = sourceL;
        var inR = sourceR;

        var Lin = vLIN * inL / 0x8000;
        var Rin = vRIN * inR / 0x8000;

        var l1 = Buffer[mLSAME - 1];
        var r1 = Buffer[mRSAME - 1];

        Buffer[mLSAME] = Clamp((Lin + Buffer[dLSAME] * vWALL / 0x8000 - l1) * vIIR / 0x8000 + l1);
        Buffer[mRSAME] = Clamp((Rin + Buffer[dRSAME] * vWALL / 0x8000 - r1) * vIIR / 0x8000 + r1);

        var l2 = Buffer[mLDIFF - 1];
        var r2 = Buffer[mRDIFF - 1];

        Buffer[mLDIFF] = Clamp((Lin + Buffer[dRDIFF] * vWALL / 0x8000 - l2) * vIIR / 0x8000 + l2);
        Buffer[mRDIFF] = Clamp((Rin + Buffer[dLDIFF] * vWALL / 0x8000 - r2) * vIIR / 0x8000 + r2);

        var Lout =
            vCOMB1 * Buffer[mLCOMB1] / 0x8000 +
            vCOMB2 * Buffer[mLCOMB2] / 0x8000 +
            vCOMB3 * Buffer[mLCOMB3] / 0x8000 +
            vCOMB4 * Buffer[mLCOMB4] / 0x8000;

        var Rout =
            vCOMB1 * Buffer[mRCOMB1] / 0x8000 +
            vCOMB2 * Buffer[mRCOMB2] / 0x8000 +
            vCOMB3 * Buffer[mRCOMB3] / 0x8000 +
            vCOMB4 * Buffer[mRCOMB4] / 0x8000;

        Lout = Lout - vAPF1 * Buffer[mLAPF1 - dAPF1] / 0x8000;
        Rout = Rout - vAPF1 * Buffer[mRAPF1 - dAPF1] / 0x8000;

        Buffer[mLAPF1] = Clamp(Lout);
        Buffer[mRAPF1] = Clamp(Rout);

        Lout = Lout * vAPF1 / 0x8000 + Buffer[mLAPF1 - dAPF1];
        Rout = Rout * vAPF1 / 0x8000 + Buffer[mRAPF1 - dAPF1];

        Lout = Lout - vAPF2 * Buffer[mLAPF2 - dAPF2] / 0x8000;
        Rout = Rout - vAPF2 * Buffer[mRAPF2 - dAPF2] / 0x8000;

        Buffer[mLAPF2] = Clamp(Lout);
        Buffer[mRAPF2] = Clamp(Rout);

        Lout = Lout * vAPF2 / 0x8000 + Buffer[mLAPF2 - dAPF2];
        Rout = Rout * vAPF2 / 0x8000 + Buffer[mRAPF2 - dAPF2];

        const short vLOUT = short.MaxValue;
        const short vROUT = short.MaxValue;

        const bool mix = false;

        if (mix)
        {
            targetL = Clamp(Lin * vLIN / 0x8000 + Lout * vLOUT / 0x8000);
            targetR = Clamp(Rin * vRIN / 0x8000 + Rout * vROUT / 0x8000);
        }
        else
        {
            targetL = Clamp(Lout * vLOUT / 0x8000);
            targetR = Clamp(Rout * vROUT / 0x8000);
        }

        Buffer.Advance();
    }

    private static short Clamp(int value)
    {
        var clamp1 = Math.Clamp(value, short.MinValue, short.MaxValue);
        var clamp2 = (short)clamp1;

        return clamp2;
    }
}