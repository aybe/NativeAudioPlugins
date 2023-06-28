﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Wipeout.Formats.Audio.Sony
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public readonly struct SpuReverbPreset
    {
        public readonly short dAPF1;
        public readonly short dAPF2;

        public readonly short vIIR;

        public readonly short vCOMB1;
        public readonly short vCOMB2;
        public readonly short vCOMB3;
        public readonly short vCOMB4;

        public readonly short vWALL;

        public readonly short vAPF1;
        public readonly short vAPF2;

        public readonly short mLSAME;
        public readonly short mRSAME;

        public readonly short mLCOMB1;
        public readonly short mRCOMB1;

        public readonly short mLCOMB2;
        public readonly short mRCOMB2;

        public readonly short dLSAME;
        public readonly short dRSAME;

        public readonly short mLDIFF;
        public readonly short mRDIFF;

        public readonly short mLCOMB3;
        public readonly short mRCOMB3;

        public readonly short mLCOMB4;
        public readonly short mRCOMB4;

        public readonly short dLDIFF;
        public readonly short dRDIFF;

        public readonly short mLAPF1;
        public readonly short mRAPF1;

        public readonly short mLAPF2;
        public readonly short mRAPF2;

        public readonly short vLIN;
        public readonly short vRIN;

        private static SpuReverbPreset Parse(Span<ushort> span)
        {
            var reverbs = MemoryMarshal.Cast<ushort, SpuReverbPreset>(span);

            var reverb1 = reverbs[0];

            return reverb1;
        }

        public static SpuReverbPreset Off { get; } =
            Parse(new ushort[]
            {
                0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                0x0000, 0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
                0x0000, 0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001, 0x0001,
                0x0000, 0x0000, 0x0001, 0x0001, 0x0001, 0x0001, 0x0000, 0x0000
            });

        public static SpuReverbPreset Room { get; } =
            Parse(new ushort[]
            {
                0x007D, 0x005B, 0x6D80, 0x54B8, 0xBED0, 0x0000, 0x0000, 0xBA80,
                0x5800, 0x5300, 0x04D6, 0x0333, 0x03F0, 0x0227, 0x0374, 0x01EF,
                0x0334, 0x01B5, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                0x0000, 0x0000, 0x01B4, 0x0136, 0x00B8, 0x005C, 0x8000, 0x8000
            });

        public static SpuReverbPreset StudioA { get; } =
            Parse(new ushort[]
            {
                0x0033, 0x0025, 0x70F0, 0x4FA8, 0xBCE0, 0x4410, 0xC0F0, 0x9C00,
                0x5280, 0x4EC0, 0x03E4, 0x031B, 0x03A4, 0x02AF, 0x0372, 0x0266,
                0x031C, 0x025D, 0x025C, 0x018E, 0x022F, 0x0135, 0x01D2, 0x00B7,
                0x018F, 0x00B5, 0x00B4, 0x0080, 0x004C, 0x0026, 0x8000, 0x8000
            });

        public static SpuReverbPreset StudioB { get; } =
            Parse(new ushort[]
            {
                0x00B1, 0x007F, 0x70F0, 0x4FA8, 0xBCE0, 0x4510, 0xBEF0, 0xB4C0,
                0x5280, 0x4EC0, 0x0904, 0x076B, 0x0824, 0x065F, 0x07A2, 0x0616,
                0x076C, 0x05ED, 0x05EC, 0x042E, 0x050F, 0x0305, 0x0462, 0x02B7,
                0x042F, 0x0265, 0x0264, 0x01B2, 0x0100, 0x0080, 0x8000, 0x8000
            });

        public static SpuReverbPreset StudioC { get; } =
            Parse(new ushort[]
            {
                0x00E3, 0x00A9, 0x6F60, 0x4FA8, 0xBCE0, 0x4510, 0xBEF0, 0xA680,
                0x5680, 0x52C0, 0x0DFB, 0x0B58, 0x0D09, 0x0A3C, 0x0BD9, 0x0973,
                0x0B59, 0x08DA, 0x08D9, 0x05E9, 0x07EC, 0x04B0, 0x06EF, 0x03D2,
                0x05EA, 0x031D, 0x031C, 0x0238, 0x0154, 0x00AA, 0x8000, 0x8000
            });

        public static SpuReverbPreset Hall { get; } =
            Parse(new ushort[]
            {
                0x01A5, 0x0139, 0x6000, 0x5000, 0x4C00, 0xB800, 0xBC00, 0xC000,
                0x6000, 0x5C00, 0x15BA, 0x11BB, 0x14C2, 0x10BD, 0x11BC, 0x0DC1,
                0x11C0, 0x0DC3, 0x0DC0, 0x09C1, 0x0BC4, 0x07C1, 0x0A00, 0x06CD,
                0x09C2, 0x05C1, 0x05C0, 0x041A, 0x0274, 0x013A, 0x8000, 0x8000
            });

        public static SpuReverbPreset Pipe { get; } =
            Parse(new ushort[]
            {
                0x0017, 0x0013, 0x70F0, 0x4FA8, 0xBCE0, 0x4510, 0xBEF0, 0x8500,
                0x5F80, 0x54C0, 0x0371, 0x02AF, 0x02E5, 0x01DF, 0x02B0, 0x01D7,
                0x0358, 0x026A, 0x01D6, 0x011E, 0x012D, 0x00B1, 0x011F, 0x0059,
                0x01A0, 0x00E3, 0x0058, 0x0040, 0x0028, 0x0014, 0x8000, 0x8000
            });

        public static SpuReverbPreset Space { get; } =
            Parse(new ushort[]
            {
                0x033D, 0x0231, 0x7E00, 0x5000, 0xB400, 0xB000, 0x4C00, 0xB000,
                0x6000, 0x5400, 0x1ED6, 0x1A31, 0x1D14, 0x183B, 0x1BC2, 0x16B2,
                0x1A32, 0x15EF, 0x15EE, 0x1055, 0x1334, 0x0F2D, 0x11F6, 0x0C5D,
                0x1056, 0x0AE1, 0x0AE0, 0x07A2, 0x0464, 0x0232, 0x8000, 0x8000
            });

        public static SpuReverbPreset Echo { get; } =
            Parse(new ushort[]
            {
                0x0001, 0x0001, 0x7FFF, 0x7FFF, 0x0000, 0x0000, 0x0000, 0x8100,
                0x0000, 0x0000, 0x1FFF, 0x0FFF, 0x1005, 0x0005, 0x0000, 0x0000,
                0x1005, 0x0005, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                0x0000, 0x0000, 0x1004, 0x1002, 0x0004, 0x0002, 0x8000, 0x8000
            });

        public static SpuReverbPreset Delay { get; } =
            Parse(new ushort[]
            {
                0x0001, 0x0001, 0x7FFF, 0x7FFF, 0x0000, 0x0000, 0x0000, 0x0000,
                0x0000, 0x0000, 0x1FFF, 0x0FFF, 0x1005, 0x0005, 0x0000, 0x0000,
                0x1005, 0x0005, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                0x0000, 0x0000, 0x1004, 0x1002, 0x0004, 0x0002, 0x8000, 0x8000
            });
    }
}