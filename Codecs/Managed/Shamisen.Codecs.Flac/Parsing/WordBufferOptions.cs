using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Specifies configuration options for a word buffer, including writer concurrency and channel capacity.
    /// </summary>
    /// <param name="SingleWriter">Indicates whether the buffer is restricted to a single writer. Set to <see langword="true"/> to optimize for
    /// single-writer scenarios; otherwise, <see langword="false"/> allows multiple writers.</param>
    /// <param name="ChannelCapacity">The maximum number of words that the buffer channel can hold. Specify -1 for an unbounded capacity, or a
    /// positive integer to set a fixed limit.</param>
    /// <param name="BufferPool">The <see cref="ArrayPool{T}"/> instance used for buffer management. Defaults to <see cref="ArrayPool{T}.Shared"/> if not specified.</param>
    public record struct WordBufferOptions(bool SingleWriter = false, int ChannelCapacity = -1, ArrayPool<ulong>? BufferPool = default)
    {
    }
}
