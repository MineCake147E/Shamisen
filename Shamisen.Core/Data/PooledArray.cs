using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Represents a pooled <see cref="System.Array"/>.
    /// </summary>
    /// <typeparam name="T">The contents of array.</typeparam>
    public sealed class PooledArray<T> : IDisposable
    {
        private T[]? array;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledArray{T}"/> class.
        /// </summary>
        /// <param name="size">The desired size.</param>
        public PooledArray(int size)
        {
            array = ArrayPool<T>.Shared.Rent(size);
            Length = size;
        }

        /// <summary>
        /// Gets the array.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        public T[] Array => array ?? throw new ObjectDisposedException(nameof(PooledArray<T>));

        /// <summary>
        /// Gets the initially desired length of the <see cref="Array"/>.
        /// </summary>
        /// <value>
        /// The initially desired length of the <see cref="Array"/>.
        /// </value>
        public int Length { get; }

        /// <summary>
        /// Gets the span of the <see cref="Array"/> with <see cref="Length"/>.
        /// </summary>
        /// <value>
        /// The span.
        /// </value>
        public Span<T> Span => Array.AsSpan(0, Length);

        /// <summary>
        /// Gets the memory of the <see cref="Array"/> with <see cref="Length"/>.
        /// </summary>
        /// <value>
        /// The memory.
        /// </value>
        public Memory<T> Memory => Array.AsMemory(0, Length);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                if (array is { } a)
                {
                    ArrayPool<T>.Shared.Return(a);
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        /// <returns></returns>
        ~PooledArray()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
