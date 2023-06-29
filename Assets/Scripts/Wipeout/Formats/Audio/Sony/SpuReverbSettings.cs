using System;
using System.Diagnostics.CodeAnalysis;

namespace Wipeout.Formats.Audio.Sony
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    internal /*readonly*/ struct SpuReverbSettings
    {
        public SpuReverbSettings(SpuReverbPreset preset, int sampleRate)
        {
            if (sampleRate <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, null);
            }

            dAPF1   = GetOffset(preset.dAPF1);
            dAPF2   = GetOffset(preset.dAPF2);
            vIIR    = GetCenter(preset.vIIR);
            vCOMB1  = GetCenter(preset.vCOMB1);
            vCOMB2  = GetCenter(preset.vCOMB2);
            vCOMB3  = GetCenter(preset.vCOMB3);
            vCOMB4  = GetCenter(preset.vCOMB4);
            vWALL   = GetCenter(preset.vWALL);
            vAPF1   = GetCenter(preset.vAPF1);
            vAPF2   = GetCenter(preset.vAPF2);
            mLSAME  = GetOffset(preset.mLSAME);
            mRSAME  = GetOffset(preset.mRSAME);
            mLCOMB1 = GetOffset(preset.mLCOMB1);
            mRCOMB1 = GetOffset(preset.mRCOMB1);
            mLCOMB2 = GetOffset(preset.mLCOMB2);
            mRCOMB2 = GetOffset(preset.mRCOMB2);
            dLSAME  = GetOffset(preset.dLSAME);
            dRSAME  = GetOffset(preset.dRSAME);
            mLDIFF  = GetOffset(preset.mLDIFF);
            mRDIFF  = GetOffset(preset.mRDIFF);
            mLCOMB3 = GetOffset(preset.mLCOMB3);
            mRCOMB3 = GetOffset(preset.mRCOMB3);
            mLCOMB4 = GetOffset(preset.mLCOMB4);
            mRCOMB4 = GetOffset(preset.mRCOMB4);
            dLDIFF  = GetOffset(preset.dLDIFF);
            dRDIFF  = GetOffset(preset.dRDIFF);
            mLAPF1  = GetOffset(preset.mLAPF1);
            mRAPF1  = GetOffset(preset.mRAPF1);
            mLAPF2  = GetOffset(preset.mLAPF2);
            mRAPF2  = GetOffset(preset.mRAPF2);
            vLIN    = GetVolume(preset.vLIN);
            vRIN    = GetVolume(preset.vRIN);

            float GetCenter(short value)
            {
                const float div = 1.0f / 22050.0f;

                var dt1 = GetVolume(value);
                var rc1 = 1.0f / (2.0f * MathF.PI * (div / dt1 - div));

                var dt2 = 1.0f / sampleRate;
                var rc2 = 1.0f / (2.0f * MathF.PI * rc1);

                var ctr = dt2 / (rc2 + dt2);

                return ctr;
            }

            int GetOffset(short value)
            {
                return (int)(value * 8 * sampleRate / 22050);
            }

            static float GetVolume(short value)
            {
                return value / 32768.0f;
            }
        }

        #region New region

        public int dAPF1;

        public int dAPF2;

        public int mLSAME;

        public int mRSAME;

        public int mLCOMB1;

        public int mRCOMB1;

        public int mLCOMB2;

        public int mRCOMB2;

        public int dLSAME;

        public int dRSAME;

        public int mLDIFF;

        public int mRDIFF;

        public int mLCOMB3;

        public int mRCOMB3;

        public int mLCOMB4;

        public int mRCOMB4;

        public int dLDIFF;

        public int dRDIFF;

        public int mLAPF1;

        public int mRAPF1;

        public int mLAPF2;

        public int mRAPF2;

        #endregion

        #region New region

        public float vIIR;

        public float vCOMB1;

        public float vCOMB2;

        public float vCOMB3;

        public float vCOMB4;

        public float vWALL;

        public float vAPF1;

        public float vAPF2;

        public float vLIN;

        public float vRIN;

        #endregion
    }
}
