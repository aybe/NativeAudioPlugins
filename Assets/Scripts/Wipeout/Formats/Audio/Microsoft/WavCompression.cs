using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Wipeout.Formats.Audio.Microsoft
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum WavCompression : ushort
    {
        Unknown   = 0x0000,
        PCM       = 0x0001,
        IEEEFloat = 0x0003
    }
}