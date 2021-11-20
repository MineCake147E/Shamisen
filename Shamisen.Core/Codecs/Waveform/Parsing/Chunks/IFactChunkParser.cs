using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Waveform.Parsing.Chunks
{
    /// <summary>
    /// Defines a base infrastructure of a "fact" chunk parser.
    /// </summary>
    /// <seealso cref="IChunkParserWithSingleData{T}" />
    public interface IFactChunkParser : IChunkParserWithSingleData<ulong>
    {
    }
}
