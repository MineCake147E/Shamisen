using OpenTK.Audio.OpenAL;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Represents a handle that exits the current OpenAL context when disposed.
    /// </summary>
    public struct CurrentContextHandle : IDisposable
    {
        private bool disposedValue;

        internal CurrentContextHandle(ALCContext context) : this()
        {
            Context = context;
        }

        private ALCContext Context { get; }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                OpenALContextManager.ExitContext(Context);
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose() => Dispose(disposing: true);
    }
}
