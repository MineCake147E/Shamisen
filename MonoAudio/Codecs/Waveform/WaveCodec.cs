using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;

namespace MonoAudio.Codecs.Waveform
{
    /// <summary>
    /// Encodes and decodes some ".wav" files in certain formats.
    /// </summary>
    public sealed class WaveCodec : IDecoder
    {
        private const int ChunkIdRiff = 0x4646_4952;

        /// <summary>Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataReader" />.</summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        public async Task<IWaveSource> CreateDecoderAsync(DataReader<byte> dataReader)
        {
            var (success, decoder) = await TryCreateDecoderAsync(dataReader);
            return success ? decoder : throw new ArgumentException($"The {nameof(dataReader)}'s data is invalid!", nameof(dataReader));
        }

        /// <summary>Determines whether the data from <paramref name="dataReader" /> can be decoded by this decoder asynchronously.</summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>The whole verification task which returns the value below:<br /><c>true</c> if the data from <paramref name="dataReader" /> can be supported by this decoder, otherwise, <c>false</c>.</returns>
        public async Task<bool> DetermineDecodabilityAsync(DataReader<byte> dataReader) => await CheckHeadersAsync(dataReader);

        private async Task<bool> CheckHeadersAsync(DataReader<byte> dataReader)
        {
            uint ReadUInt32(ReadOnlyMemory<byte> region) => BinaryPrimitives.ReadUInt32LittleEndian(region.Span);
            var space = new byte[4];
            var memory = space.AsMemory();
            var length = await dataReader.ReadAsync(memory);
            if (length < 4) return false;
            var riff = ReadUInt32(memory);
            return riff == ChunkIdRiff;
        }

        /// <summary>Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataReader" />.</summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.</returns>
        public async Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(DataReader<byte> dataReader)
        {
            if (await CheckHeadersAsync(dataReader))
            {
                try
                {
                    var decoder = new WaveDecoder(dataReader.Synchronized());
                }
                catch (Exception)
                {
                    return (false, null);
                }
            }
            else
            {
                return (false, null);
            }
            //TODO: Implementation
            throw new NotImplementedException();
        }
    }
}
