using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Shamisen.Data;

namespace Shamisen
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
            if (dataSource.ReadSupport is not { } source)
            {
                throw new NotSupportedException("");
            }
            else
            {
                if (dataSource.SkipSupport is { } src)
                {
                    src.Skip(numberOfElementsToSkip);
                }
                else
                {
                    var pbuf = new PooledArray<TSample>((int)Math.Min(numberOfElementsToSkip, 2048));
                    var buffer = pbuf.Span;
                    ulong h = numberOfElementsToSkip;
                    while (h > 0 && h <= numberOfElementsToSkip)
                    {
                        var result = source.Read(buffer.SliceWhile((int)Math.Min((ulong)buffer.Length, h)));
                        if (result.IsEndOfStream) return;
                        h -= (uint)result.Length;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the specified data source.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="dataSource">The data source.</param>
        /// <param name="span">The span.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static ReadResult Read<TSample>(this IDataSource<TSample> dataSource, Span<TSample> span) where TSample : unmanaged
            => dataSource.ReadSupport is { } src ? src.Read(span) : throw new NotSupportedException("Reading is not supported!");
    }
}
