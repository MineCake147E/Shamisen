using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Shamisen.Data
{
    /// <summary>
    /// Provides some extensions for <see cref="IDataSource{TSample}"/>s.
    /// </summary>
    public static class DataUtils
    {
        /// <summary>
        /// Skips this data source the specified number of elements to skip.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="numberOfElementsToSkip">The number of elements to skip.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void SkipWithFallback<TSample>(this IDataSource<TSample> dataSource, ulong numberOfElementsToSkip) where TSample : unmanaged
        {
            if (dataSource is ISkippableDataSource<byte> src)
            {
                src.Skip(numberOfElementsToSkip);
            }
            else
            {
                Span<TSample> buffer = new TSample[Math.Min(numberOfElementsToSkip, 2048)];
                ulong h = numberOfElementsToSkip;
                while (h > 0)
                {
                    var result = dataSource.Read(buffer);
                    if (result.IsEndOfStream) return;
                    h -= (uint)result.Length;
                }
            }
        }
    }
}
