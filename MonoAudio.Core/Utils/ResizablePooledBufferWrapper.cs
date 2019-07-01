using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Utils
{
    /// <summary>
    /// Provides a Buffer wrapper.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Utils.ResizableBufferWrapper{T}" />
    public sealed class ResizablePooledBufferWrapper<T> : ResizableBufferWrapper<T> where T : unmanaged
    {
        private ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResizablePooledBufferWrapper{T}"/> class.
        /// </summary>
        /// <param name="initialSize">The initial size.</param>
        public ResizablePooledBufferWrapper(int initialSize)
        {
            unsafe
            {
                array = Pool.Rent(initialSize * sizeof(T));
            }
        }

        private byte[] array;

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        protected override Span<byte> ActualBuffer => new Span<byte>(array);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (array != null) Pool.Return(array);
            array = null;
            Pool = null;
        }

        /// <summary>
        /// Resizes the buffer to the specified new size.
        /// </summary>
        /// <param name="newSize">The new size in bytes.</param>
        protected override void ResizeInternal(int newSize)
        {
            ActualBuffer.FastFill(0);
            Pool.Return(array);
            array = Pool.Rent(newSize);
            ActualBuffer.FastFill(0);
        }
    }
}
