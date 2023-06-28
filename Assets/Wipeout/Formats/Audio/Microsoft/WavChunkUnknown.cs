using System.IO;
using Wipeout.Extensions;

namespace Wipeout.Formats.Audio.Microsoft
{
    public sealed class WavChunkUnknown : WavChunk
    {
        public WavChunkUnknown(Stream reader)
            : base(reader)
        {
            Data = reader.ReadBytes((int)ChunkSize);
        }

        public byte[] Data { get; }
    }
}