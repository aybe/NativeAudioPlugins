using System;

namespace Wipeout
{
    internal struct NativeFilterState : IDisposable
    {
        public UnsafeBuffer<float> Source;

        public UnsafeBuffer<float> Target;

        public UnsafeBuffer<UnsafeBuffer<float>> Coefficients;

        public UnsafeBuffer<UnsafeBuffer<float>> Delays;

        public UnsafeBuffer<UnsafeBuffer<int>> Taps;

        public UnsafeBuffer<UnsafeBuffer<int>> Positions;

        public void Dispose()
        {
            Source.Dispose();
            Target.Dispose();
            Coefficients.Dispose();
            Delays.Dispose();
            Taps.Dispose();
            Positions.Dispose();
        }
    }
}