using System;
using System.Collections.Generic;
using System.Text;

using MonoAudio.Codecs.Waveform.Formats.LinearPcm;
using MonoAudio.Codecs.Waveform.Parsing.Chunks;
using MonoAudio.Data.Binary;

namespace MonoAudio.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Constructs simple chunk parsers and discards any metadata chunks.
    /// </summary>
    public class SimpleChunkParserFactory : IChunkParserFactory
    {
        /// <summary>
        /// Gets the chunk parser for the specified <paramref name="id" />.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="chunkReader">The source chunk reader.</param>
        /// <returns></returns>
        public virtual IChunkParser GetChunkParser(ChunkId id, IChunkReader chunkReader) => new JunkChunkParser(id, chunkReader.TotalSize, chunkReader);

        /// <summary>
        /// Gets the format chunk parser.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        /// <returns></returns>
        public IFormatChunkParser GetFormatChunkParser(IChunkReader chunkReader)
        {
            var audioEncoding = (AudioEncoding)chunkReader.ReadUInt16LittleEndian();
            switch (audioEncoding)
            {
                case AudioEncoding.Extensible:
                case AudioEncoding.LinearPcm:
                case AudioEncoding.IeeeFloat:
                case AudioEncoding.Alaw:
                case AudioEncoding.Mulaw:
                    return new PcmFormatChunkParser(chunkReader, audioEncoding);
                default:
                    throw new NotSupportedException($"The {audioEncoding} format is not supported!");
            }
        }
    }
}
