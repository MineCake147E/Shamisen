using Silk.NET.OpenAL;

using ALCApi = Silk.NET.OpenAL.ALContext;
using OpenALApi = Silk.NET.OpenAL.AL;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides a base infrastructure of objects that manipulates <see cref="Silk.NET.OpenAL"/>.
    /// </summary>
    public abstract class SilkOpenALObjectBase : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of <see cref="SilkOpenALObjectBase"/>.
        /// </summary>
        /// <param name="useOpenALSoft">The value which indicates whether the <see cref="OpenALOutput"/> should be using OpenALSoft library.</param>
        protected SilkOpenALObjectBase(bool useOpenALSoft)
        {
            IsOpenALSoft = useOpenALSoft;
            AL = OpenALApi.GetApi(useOpenALSoft);
            ALC = ALCApi.GetApi(useOpenALSoft);
        }

        /// <summary>
        /// Gets the value which indicates whether the <see cref="OpenALOutput"/> is using OpenALSoft library.
        /// </summary>
        public bool IsOpenALSoft { get; }
        /// <summary>
        /// The AL object.
        /// </summary>
        protected internal OpenALApi AL { get; }
        /// <summary>
        /// The ALC object.
        /// </summary>
        protected internal ALCApi ALC { get; }

        private void Dispose(bool disposing)
        {
            ActualDispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                AL.Dispose();
                ALC.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void ActualDispose(bool disposing);

        /// <inheritdoc/>
        ~SilkOpenALObjectBase()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}