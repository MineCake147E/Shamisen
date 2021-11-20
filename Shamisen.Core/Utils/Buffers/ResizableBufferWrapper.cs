using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Shamisen.Utils
{
    /// <summary>
    /// Provides a Buffer wrapper.
    /// </summary>
    public abstract class ResizableBufferWrapper<T> : IDisposable where T : unmanaged
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizableBufferWrapper{T}"/> class.
        /// </summary>
        protected ResizableBufferWrapper()
        {
        }

        /// <summary>
        /// Gets the current size in bytes.
        /// </summary>
        /// <value>
        /// The current size in bytes.
        /// </value>
        public int CurrentSizeInBytes => ActualBuffer.Length;

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        protected abstract Span<byte> ActualBuffer { get; }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        public Span<T> Buffer => MemoryMarshal.Cast<byte, T>(ActualBuffer);

        /// <summary>
        /// Resizes the buffer to the specified new size.
        /// </summary>
        /// <param name="newSize">The new size in <typeparamref name="T"/>.</param>
        public void Resize(int newSize)
        {
            unsafe
            {
                var newSizeInBytes = sizeof(T) * newSize;
                ResizeInternal(newSizeInBytes);
                if (CurrentSizeInBytes < newSizeInBytes) throw new InvalidProgramException("Failed to allocate memory!");
            }
        }

        /// <summary>
        /// Resizes the buffer to the specified new size.
        /// </summary>
        /// <param name="newSize">The new size in bytes.</param>
        protected abstract void ResizeInternal(int newSize);

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void DisposeInternal(bool disposing)
        {
            if (!disposedValue)
            {
                Dispose(disposing);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ResizableBufferWrapper{T}"/> class.
        /// </summary>
        ~ResizableBufferWrapper() => DisposeInternal(false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            DisposeInternal(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
