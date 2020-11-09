using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using MonoAudio.Codecs.Waveform.Parsing.Chunks;
using MonoAudio.Codecs.Waveform.Rf64;
using MonoAudio.Codecs.Waveform.Riff;
using MonoAudio.Data;
using MonoAudio.Data.Binary;
using MonoAudio.Extensions;

namespace MonoAudio.Codecs.Waveform.Parsing
{
    /// <summary>
    /// Parses ".wav" files from <see cref="IChunkReader"/>
    /// </summary>
    /// <seealso cref="IWaveParser" />
    public sealed class SimpleWaveParser : IRf64Parser
    {
        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek { get; }

        /// <summary>
        /// Gets the chunk parser factory.
        /// </summary>
        /// <value>
        /// The chunk parser factory.
        /// </value>
        public IChunkParserFactory ChunkParserFactory { get; }

        /// <summary>
        /// Gets the source chunk reader.
        /// </summary>
        /// <value>
        /// The chunk reader.
        /// </value>
        public IChunkReader ChunkReader => Rf64ChunkReader;

        private Rf64ChunkReader Rf64ChunkReader { get; }

        /// <summary>
        /// Gets the chunk size table.
        /// </summary>
        /// <value>
        /// The chunk size table.
        /// </value>
        public IReadOnlyList<ChunkSizeTableEntry> ChunkSizeTable { get; }

        /// <summary>
        /// Gets the size of the data chunk.
        /// </summary>
        /// <value>
        /// The size of the data chunk.
        /// </value>
        public ulong DataSize { get => dataSize == 0 ? WaveChunkParser.ChunkLength : dataSize; private set => dataSize = value; }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// Negative value Means Infinity.
        /// </summary>
        public long Length { get => unchecked((long)DataSize); }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Gets the size of the RF64 chunk.
        /// </summary>
        /// <value>
        /// The size of the RF64 chunk.
        /// </value>
        public ulong RiffSize { get; }

        /// <summary>
        /// Gets the sample count in the fact chunk.
        /// </summary>
        /// <value>
        /// The sample count in the fact chunk.
        /// </value>
        public ulong SampleCount { get; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public IEnumerable<object> Metadata { get => metadataList; }

        private List<object> metadataList;

        private IFormatChunkParser FormatChunkParser { get; set; }

        private IWaveformChunkParser WaveChunkParser { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWaveParser" /> class.
        /// </summary>
        /// <param name="chunkParserFactory">The chunk parser factory.</param>
        /// <param name="dataSource">The data source for whole WAVE file.</param>
        /// <exception cref="ArgumentNullException">chunkParserFactory
        /// or
        /// chunkReader</exception>
        /// <exception cref="NotSupportedException">The \"{chars}\" file is not supported!</exception>
        /// <exception cref="InvalidDataException">
        /// The first chunk wasn't \"ds64\"!
        /// or
        /// "fmt " chunk not found!
        /// or
        /// "fact" chunk not found!
        /// or
        /// The "wavl" and "slnt" chunks are not currently supported!
        /// </exception>
        public SimpleWaveParser(IChunkParserFactory chunkParserFactory, IDataSource<byte> dataSource)
        {
            //All preparation must be done in this constructor.
            ChunkParserFactory = chunkParserFactory ?? throw new ArgumentNullException(nameof(chunkParserFactory));
            if (dataSource is null) throw new ArgumentNullException(nameof(dataSource));
            Rf64ChunkReader mainReader;
            Rf64ChunkReader = mainReader = new Rf64ChunkReader(dataSource, this, out var setter);
            var result = mainReader.TryReadUInt32LittleEndian(out var read);
            result.ThrowWhenInsufficient(1, $"initializing {nameof(SimpleWaveParser)}");
            if ((RiffSubChunkId)read != RiffSubChunkId.Wave)
            {
                char[] chars = ConvertChunkNameToChars(read);
                throw new NotSupportedException($"The \"{chars}\" file is not supported!");
            }
            metadataList = new List<object>();
            //Reading ds64 chunk when the file is real RF64
            switch (mainReader.ChunkId)
            {
                case ChunkId.Rf64:
                case ChunkId.Bw64:
                    var ds64 = mainReader.ReadSubChunk();
                    if (ds64.ChunkId != ChunkId.Rf64DataSize) throw new InvalidDataException("The first chunk wasn't \"ds64\"!");
                    ulong riffSize, dataSize, sampleCountOrDummySize;
                    uint tableLength;
                    riffSize = ds64.ReadUInt64LittleEndian();
                    dataSize = ds64.ReadUInt64LittleEndian();
                    sampleCountOrDummySize = ds64.ReadUInt64LittleEndian();
                    tableLength = ds64.ReadUInt32LittleEndian();
                    if (tableLength > 0)
                    {
                        var table = new ChunkSizeTableEntry[tableLength];
                        var span = MemoryMarshal.Cast<ChunkSizeTableEntry, byte>(table.AsSpan());
                        ds64.ReadAll(span);
                        ChunkSizeTable = new List<ChunkSizeTableEntry>(table);
                    }
                    RiffSize = riffSize;
                    setter.Invoke(riffSize);
                    DataSize = dataSize;
                    SampleCount = sampleCountOrDummySize;
                    break;
                default:    //RIFF
                    break;
            }
            //Parsing other header chunks
            //Find and parse "fmt " chunk
            IFormatChunkParser fmt = null;
            do
            {
                var reader = ReadNextSubChunk();
                switch (reader.ChunkId)
                {
                    case ChunkId.Format:
                        fmt = FormatChunkParser = chunkParserFactory.GetFormatChunkParser(reader);
                        Format = fmt.Data;
                        break;
                    case ChunkId.Fact:
                    case ChunkId.WaveList:
                    case ChunkId.Silent:
                    case ChunkId.Data:
                        throw new InvalidDataException("\"fmt \" chunk not found!");
                    default:
                        ParseMetadataChunk(chunkParserFactory, reader);
                        break;
                }
            } while (fmt is null);
            //Find and parse "fact" chunk if needed
            if (fmt.RequireFactChunk)
            {
                do
                {
                    var reader = ReadNextSubChunk();
                    switch (reader.ChunkId)
                    {
                        case ChunkId.Fact:
                            fmt.ParseFactChunk(reader);
                            reader.Dispose();
                            break;
                        case ChunkId.WaveList:
                        case ChunkId.Silent:
                        case ChunkId.Data:
                            throw new InvalidDataException("\"fact\" chunk not found!");
                        default:
                            ParseMetadataChunk(chunkParserFactory, reader);
                            continue;
                    }
                    break;
                } while (true);
            }
            //Find "data" chunk
            do
            {
                var reader = ReadNextSubChunk();
                switch (reader.ChunkId)
                {
                    case ChunkId.WaveList:
                    case ChunkId.Silent:
                        throw new InvalidDataException("The \"wavl\" and \"slnt\" chunks are not currently supported!");
                    case ChunkId.Data:
                        WaveChunkParser = fmt.GetWaveformChunkParser(reader, this);
                        break;
                    default:
                        ParseMetadataChunk(chunkParserFactory, reader);
                        continue;
                }
                break;
            } while (true);
        }

        private void ParseMetadataChunk(IChunkParserFactory chunkParserFactory, Rf64ChunkReader reader)
        {
            var parser = chunkParserFactory.GetChunkParser(reader.ChunkId, reader);
            if (parser is IChunkParserWithSingleData<object> pwd)
            {
                metadataList.Add(pwd.Data);
            }
            parser.Dispose();
        }

        private Rf64ChunkReader ReadNextSubChunk() => Rf64ChunkReader.ReadSubChunk() as Rf64ChunkReader;

        private static char[] ConvertChunkNameToChars(uint read)
        {
            var buffer = new byte[sizeof(int)];
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(), read);
            var chars = Encoding.UTF8.GetChars(buffer);
            return chars;
        }

        /// <summary>
        /// Gets the size for next chunk with specified <paramref name="id" />.<br />
        /// This function must not be called more than once for single chunk.
        /// </summary>
        /// <param name="id">The chunk identifier.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">When the parser is nor parsing RF64 data.</exception>
        public ulong GetSizeForNextChunk(ChunkId id)
        {
            switch (id)
            {
                case ChunkId.Rf64:
                case ChunkId.Bw64:
                    return RiffSize;
                case ChunkId.Data:
                    return DataSize;
                default:
                    if (!(ChunkSizeTable is null)) return ChunkSizeTable.First(a => a.Id == id).ChunkSize;
                    throw new InvalidOperationException($"Cannot find data size for {ConvertChunkNameToChars((uint)id)} chunk!");
            }
        }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<byte> buffer) => WaveChunkParser.Read(buffer);

        #region IDisposable Support

        private bool disposedValue;
        private ulong dataSize;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                metadataList = null;
                //
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
