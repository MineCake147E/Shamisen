using System;
using System.Collections.Generic;
using System.Text;

using Shamisen.Codecs.Waveform.Parsing;
using Shamisen.Codecs.Waveform.Parsing.Chunks;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform.Formats.LinearPcm
{
    /// <summary>
    /// Parses fmt chunks for PCM-Like files.
    /// </summary>
    public sealed class PcmFormatChunkParser : IFormatChunkParser
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PcmFormatChunkParser" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="encoding">The encoding.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public PcmFormatChunkParser(IChunkReader source, AudioEncoding encoding)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ChunkId = source.ChunkId;
            ChunkLength = source.TotalSize;
            //Data for formatTag is already consumed by IChunkParserFactory.
            ushort nCh = source.ReadUInt16LittleEndian();
            uint nSampleRate = source.ReadUInt32LittleEndian();
            uint nByteRate = source.ReadUInt32LittleEndian();
            ushort nBlockAlign = source.ReadUInt16LittleEndian();
            ushort nBitDepth = source.ReadUInt16LittleEndian();
            var standardWaveFormat = new StandardWaveFormat(encoding, nCh, nSampleRate, nByteRate, nBlockAlign, nBitDepth);
            if (source.RemainingBytes >= 2)
            {
                ushort cbSize = source.ReadUInt16LittleEndian();
                if (cbSize > 0)  //When the format is LPCM, the cbSize must be 0 or 22
                {
                    ushort validBitsPerSample = source.ReadUInt16LittleEndian();
                    var channelMask = (Speakers)source.ReadUInt32LittleEndian();
                    var guid = source.ReadStruct<Guid>();
                    if (source.RemainingBytes > 0)
                    {
                        byte[]? bytes = new byte[source.RemainingBytes];
                        source.ReadAll(bytes.AsSpan());
                        Data = new ExtensibleWaveFormat(standardWaveFormat, cbSize, validBitsPerSample, channelMask, guid, bytes.AsMemory());
                        return;
                    }
                    else
                    {
                        Data = new ExtensibleWaveFormat(standardWaveFormat, cbSize, validBitsPerSample, channelMask, guid, ReadOnlyMemory<byte>.Empty);
                        return;
                    }
                }
            }
            Data = standardWaveFormat;
        }

        /// <summary>
        /// Gets a value indicating whether the format requires a fact chunk.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the format requires a fact chunk; otherwise, <c>false</c>.
        /// </value>
        public bool RequireFactChunk { get; }

        /// <summary>
        /// Gets the stored data.<br />
        /// The data must be parsed at initialization.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public IWaveFormat Data { get; }

        /// <summary>
        /// Gets the chunk identifier.
        /// </summary>
        /// <value>
        /// The chunk identifier.
        /// </value>
        public ChunkId ChunkId { get; }

        /// <summary>
        /// Gets the length of this chunk excluding header 8 bytes.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public ulong ChunkLength { get; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The source chunk reader.
        /// </value>
        public IChunkReader Source { get; }

        /// <summary>
        /// Gets the waveform chunk parser.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        /// <param name="rf64ChunkSizeContainer">The container of informations about the size of certain chunks.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IWaveformChunkParser GetWaveformChunkParser(IChunkReader chunkReader, IRf64ChunkSizeContainer rf64ChunkSizeContainer) => Data.BitDepth switch
        {
            //OffsetSByte
            8 or 16 or 24 or 32 or 64 => new ByteAlignedPcmWaveformChunkParser(chunkReader, Data),
            _ => throw new NotSupportedException($"{Data.BitDepth}-bit Linear PCM is not (currently) supported!"),
        };

        /// <summary>
        /// Parses the fact chunk.
        /// </summary>
        /// <param name="chunkReader">The chunk reader.</param>
        public void ParseFactChunk(IChunkReader chunkReader) => chunkReader.Skip(chunkReader.RemainingBytes);

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
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
