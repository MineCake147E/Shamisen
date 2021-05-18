using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Buffers
{
    /// <summary>
    /// Handles resizing of <see cref="System.Array"/>s.
    /// </summary>
    public sealed class PooledArrayResizer<T> : IDisposable where T : unmanaged
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledArrayResizer{T}"/> class.
        /// </summary>
        /// <param name="initialSize">The initial size.</param>
        public PooledArrayResizer(int initialSize)
        {
            Array = ArrayPool<T>.Shared.Rent(initialSize);
        }

        /// <summary>
        /// Gets the array.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
#pragma warning disable CA1819 // Properties should not return arrays

        public T[]? Array
#pragma warning restore CA1819 // Properties should not return arrays
        {
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            get;
            [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
            private set;
        }

        /// <summary>
        /// Resizes this <see cref="PooledArrayResizer{T}"/> to specified size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        /// <returns>The resized array.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public T[] Resize(int newSize)
        {
            if (Array is null) throw new ObjectDisposedException(nameof(PooledArrayResizer<T>));
            ArrayPool<T>.Shared.Return(Array, false);
            return Array = ArrayPool<T>.Shared.Rent(newSize);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                if (Array is { })
                {
                    ArrayPool<T>.Shared.Return(Array);
                    Array = null;
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PooledArrayResizer{T}"/> class.
        /// </summary>
        ~PooledArrayResizer()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
