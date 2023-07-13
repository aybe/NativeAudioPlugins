namespace Wipeout
{
    public interface INativeObject
    {
        public bool Allocated { get; }
        internal void ReleaseResources();
    }
}