namespace Wipeout.Reverb.Obsolete
{
    public interface INativeObject
    {
        public bool Allocated { get; }
        internal void ReleaseResources();
    }
}