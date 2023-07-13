using System;

namespace Wipeout
{
    public class NativeBox<T> : IDisposable where T : INativeObject
    {
        private T _data;

        internal NativeBox(T data)
        {
            _data = data;
        }

        public ref T Data => ref _data;
        public bool Allocated => _data.Allocated;

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~NativeBox()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            if (!_data.Allocated) return;
            _data.ReleaseResources();
        }
    }
}