using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Flac.Parsing
{
    /// <summary>
    /// Parses ".flac" files from <see cref="IReadableDataSource{TSample}"/>.
    /// https://xiph.org/flac/format.html
    /// </summary>
    /// <seealso cref="Shamisen.IWaveSource" />
    public sealed class FlacParser : IWaveSource
    {
        private bool disposedValue;
        private readonly FlacStreamInfoBlock streamInfoBlock;

        private Memory<FlacSeekPoint> seekPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacParser"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public FlacParser(IReadableDataSource<byte> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            var fLaC = source.ReadUInt32LittleEndian();
            if (fLaC != BinaryExtensions.ConvertToBigEndian(0x664C_6143))
                throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
            var streamInfoHeader = ReadMetadataHeader(source);
            if (streamInfoHeader.MetadataBlockType != FlacMetadataBlockType.StreamInfo)
                throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
            var streamInfo = streamInfoBlock = FlacStreamInfoBlock.ToReadableValue(Read<FlacStreamInfoBlock>(source));
            Format = new WaveFormat((int)streamInfo.SampleRate, streamInfo.BitDepth, streamInfo.Channels, AudioEncoding.LinearPcm);
            var currentHeader = streamInfoHeader;
            while (!currentHeader.IsLastMetadataBlock)
            {
                currentHeader = ReadMetadataHeader(source);
                switch (currentHeader.MetadataBlockType)
                {
                    case FlacMetadataBlockType.Application:
                    case FlacMetadataBlockType.VorbisComment:
                    case FlacMetadataBlockType.CueSheet:
                    case FlacMetadataBlockType.Picture:
                    case FlacMetadataBlockType.Padding: //Defined but currently not supported, or unnecessary data like Padding
#pragma warning disable S907
                        goto case default;
#pragma warning restore S907
                    case FlacMetadataBlockType.SeekTable when seekPoints.IsEmpty:
                        ReadSeekTable(currentHeader, source);
                        break;
                    case FlacMetadataBlockType.SeekTable when !seekPoints.IsEmpty:
                    case FlacMetadataBlockType.StreamInfo:
                    case FlacMetadataBlockType.Invalid: //Invalid chunks here
                        throw new ArgumentException("The FLAC Stream is invalid!", nameof(source));
                    default:    //Reserved
                        source.SkipWithFallback(currentHeader.Size);
                        break;
                }
            }
            FindNextFrame();
        }

        private void FindNextFrame()
        {
            ushort q = default;
            while ((q & 0b1111_1111_1111_1100u) != 0b1111_1111_1111_1000u) q = Source.ReadUInt16BigEndian();
        }

        private void ReadSeekTable(FlacMetadataBlockHeader header, IReadableDataSource<byte> source)
        {
            var h = new byte[header.Size];
            source.ReadAll(h);
            seekPoints = MemoryMarshal.Cast<byte, FlacSeekPoint>(h).ToArray();
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static FlacMetadataBlockHeader ReadMetadataHeader(IReadableDataSource<byte> source)
        {
            Span<byte> buffer = stackalloc byte[4];
            source.ReadAll(buffer);
            return MemoryMarshal.Read<FlacMetadataBlockHeader>(buffer);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static T Read<T>(IReadableDataSource<byte> source) where T : unmanaged
        {
            Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
            source.ReadAll(buffer);
            return MemoryMarshal.Read<T>(buffer);
        }

        private IReadableDataSource<byte> Source { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public IWaveFormat Format { get; }

        ulong? IAudioSource<byte, IWaveFormat>.Length { get; }

        ulong? IAudioSource<byte, IWaveFormat>.TotalLength { get; }

        ulong? IAudioSource<byte, IWaveFormat>.Position { get; }

        ISkipSupport? IAudioSource<byte, IWaveFormat>.SkipSupport { get; }

        ISeekSupport? IAudioSource<byte, IWaveFormat>.SeekSupport { get; }

        public ReadResult Read(Span<byte> buffer) => throw new NotImplementedException();

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
