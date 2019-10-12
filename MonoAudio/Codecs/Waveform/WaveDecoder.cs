using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Data;
using MonoAudio.Data.Binary;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Decodes "*.wav" files in some format.
    /// </summary>
    public sealed class WaveDecoder : IWaveSource
    {
        private ISynchronizedDataReader<byte> dataReader;

        private bool disposedValue = false;

        private const uint FccTypeWave = 0x4556_4157;

        private const uint ChunkIdFmt = 0x2074_6D66;

        private static string ConvertUInt32ToChunkIdString(uint value2convert)
        {
            var buffer = new byte[sizeof(uint)];
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(), value2convert);
            return Encoding.UTF8.GetString(buffer);
        }

        private static Guid ReadGuidLittleEndian(ISynchronizedDataReader<byte> dataReader)
        {
            byte[] ibuf = new byte[Unsafe.SizeOf<Guid>()];
            Span<byte> buffer = ibuf;
            if (buffer.Length != dataReader.Read(buffer))
                throw new ArgumentException($"The {nameof(dataReader)} ran out of data!");
            return new Guid(ibuf);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveDecoder"/> class.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <exception cref="ArgumentNullException">dataReader</exception>
        internal WaveDecoder(ISynchronizedDataReader<byte> dataReader)
        {
            this.dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            //Parsing all headers
            _ = dataReader.ReadUInt32LittleEndian();   //the whole length of this file
            var fccType = dataReader.ReadUInt32LittleEndian();
            if (fccType != FccTypeWave) throw new NotSupportedException($"The FccType {fccType} is not supported!");
            //Reading FMT chunk in LITTLE ENDIAN
            var chunkHeader = dataReader.ReadStruct<RiffChunkHeader>();
            switch (chunkHeader.ChunkId)
            {
                case ChunkId.RiffReversed:
                case ChunkId.FormatReversed:
                case ChunkId.FactReversed:
                case ChunkId.DataReversed:
                case ChunkId.WaveListReversed:
                case ChunkId.SilentReversed:
                case ChunkId.CueReversed:
                case ChunkId.PlaylistReversed:
                case ChunkId.AssociatedDataListReversed:
                case ChunkId.LabelReversed:
                case ChunkId.NoteReversed:
                case ChunkId.LabeledTextReversed:
                case ChunkId.SamplerReversed:
                case ChunkId.InstrumentReversed:
                case ChunkId.JunkLargeReversed:
                case ChunkId.JunkSmallReversed:
                    chunkHeader = chunkHeader.ReverseEndianness();
                    break;
                default:
                    break;
            }
            switch (chunkHeader.ChunkId)
            {
                case ChunkId.Riff:
                    break;
                case ChunkId.Format:
                    break;
                case ChunkId.Fact:
                    break;
                case ChunkId.Data:
                    break;
                case ChunkId.WaveList:
                    break;
                case ChunkId.Silent:
                    break;
                case ChunkId.Cue:
                    break;
                case ChunkId.Playlist:
                    break;
                case ChunkId.AssociatedDataList:
                    break;
                case ChunkId.Label:
                    break;
                case ChunkId.Note:
                    break;
                case ChunkId.LabeledText:
                    break;
                case ChunkId.Sampler:
                    break;
                case ChunkId.Instrument:
                    break;
                case ChunkId.JunkLarge:
                    break;
                case ChunkId.JunkSmall:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IAudioSource{TSample,TFormat}"/> supports seeking or not.
        /// </summary>
        public bool CanSeek => false;

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public IWaveFormat Format { get; }

        /// <summary>
        /// Gets or sets where the <see cref="IAudioSource{TSample,TFormat}"/> is.
        /// Some implementation could not support this property.
        /// </summary>
        [Obsolete("Not Supported!", true)]
        public long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <summary>
        /// Gets how long the <see cref="IAudioSource{TSample,TFormat}"/> lasts in specific types.
        /// -1 Means Infinity.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public int Read(Span<byte> buffer)
        {
            //TODO: Implementation
            throw new NotImplementedException();
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (dataReader is IDisposable disposable) disposable.Dispose();
                    dataReader = null;
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
