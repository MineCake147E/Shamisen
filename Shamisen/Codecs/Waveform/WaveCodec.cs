using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

//using Shamisen.Codecs.Waveform.Chunks;
using Shamisen.Data;
using Shamisen.Data.Binary;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Encodes and decodes some ".wav" files in certain formats.
    /// </summary>
    public sealed class WaveCodec : IDecoder
    {
        private const uint RiffSubChunkIdWave = 0x5741_5645;
        /*
        private async ValueTask<(bool, RiffChunk?)> CheckHeadersAsync(IDataSource dataReader)
        {
        }
        */

        /// <summary>
        /// Determines whether the data from <paramref name="dataSource" /> can be decoded by this decoder asynchronously.<br/>
        /// The actual decoding stream must be opened after seeking the source <see cref="Stream"/> to head.
        /// </summary>
        /// <param name="dataSource">The <see cref="IDataSource{TSample}" /> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br /><c>true</c> if the data from <paramref name="dataSource" /> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        public async ValueTask<bool> DetermineDecodabilityAsync(IDataSource<byte> dataSource) => throw new NotImplementedException();

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataSource" />.
        /// </summary>
        /// <param name="dataSource">The <see cref="IDataSource{TSample}" /> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource" />.
        /// </returns>
        public async ValueTask<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(IDataSource<byte> dataSource) => throw new NotImplementedException();/*var v = await CheckHeadersAsync(dataSource);
            if (v.Item1)
            {
                try
                {
                    //var decoder = new WaveDecoder(dataSource, v.Item2.Value);
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
            //TODO: Implementation*/
    }
}
