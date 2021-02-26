using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Data
{
    /// <summary>
    /// Contains some utility functions for <see cref="IDataSource{TSample}"/>.
    /// </summary>
    public static class DataSourceUtils
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreloadDataBuffer{TSample}"/> class.
        /// </summary>
        /// <param name="source">The data source.</param>
        /// <param name="initialBlockSize">
        /// The size of initial buffer in Frames(independent on the number of channel and the type of sample).<br/>
        /// The buffer is automatically extended if the internal buffer is smaller than the size of reading buffers.
        /// </param>
        /// <param name="internalBufferNumber">The number of internal buffer.</param>
        /// <param name="allowWaitForRead">The value which indicates whether the <see cref="PreloadDataBuffer{TSample}"/> should wait for another sample block or not.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="initialBlockSize"/> should be larger than or equals to 2048.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="internalBufferNumber"/> should be larger than or equals to 16.</exception>
        public static IDataSource<T> Preload<T>(this IDataSource<T> source, int initialBlockSize, int internalBufferNumber = 16, bool allowWaitForRead = false)
            where T : unmanaged
            => new PreloadDataBuffer<T>(source, initialBlockSize, internalBufferNumber, allowWaitForRead);
    }
}
