using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming

namespace Wipeout.Reverb
{
    [BurstCompile]
    internal class TempSpuReverbBurstFilter : IDisposable
    {
        private readonly unsafe float* Coefficients;

        private readonly unsafe float* DelayLine;

        private int Length;

        private int Position;

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        [BurstCompile]
        [SuppressMessage("ReSharper", "InlineTemporaryVariable")]
        [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
        [SuppressMessage("Style", "IDE0054:Use compound assignment")]
        private unsafe float Process(float sample)
        {
            var delays = DelayLine;
            var result = 0.0f;
            var offset = Position;
            var source = Coefficients;
            var length = Length;

            delays[Position] = sample;

            for (var i = 0; i < length; i++)
            {
                var input = source[i];
                var delay = delays[offset];
                var value = input * delay;

                result = result + value;

                offset--;

                if (offset < 0)
                {
                    offset = length - 1;
                }
            }

            Position++;

            if (Position >= length)
            {
                Position = 0;
            }

            return result;
        }

        [BurstCompile]
        public unsafe void Process(float* sourceBuffer, float* targetBuffer, int bufferLength)
        {
            for (var i = 0; i < bufferLength; i++)
            {
                var sourceSample = sourceBuffer[i];
                var targetSample = Process(sourceSample);
                targetBuffer[i] = targetSample;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        public static void Create(float[] coefficients)
        {
            unsafe
            {
                var size    = coefficients.Length * sizeof(float);
                var alignOf = UnsafeUtility.AlignOf<float>();
                var malloc  = UnsafeUtility.Malloc(size, alignOf, Allocator.Persistent);

                var span = new Span<float>(malloc, coefficients.Length);

                coefficients.CopyTo(span);
            }
        }

        ~TempSpuReverbBurstFilter()
        {
            ReleaseUnmanagedResources();
        }
    }
}