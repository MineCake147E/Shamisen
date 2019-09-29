using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;

namespace MonoAudio.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an audio decoder.
    /// </summary>
    public interface IDecoder
    {
        /// <summary>
        /// Determines whether the data from <paramref name="dataReader"/> can be decoded by this decoder asynchronously.
        /// </summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="dataReader"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DetermineDecodabilityAsync(DataReader<byte> dataReader);

        /// <summary>
        /// Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        Task<IWaveSource> CreateDecoderAsync(DataReader<byte> dataReader);

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataReader"/>.
        /// </summary>
        /// <param name="dataReader">The <see cref="DataReader{TSample}"/>(which the TSample is <see cref="byte"/>) to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(DataReader<byte> dataReader);
    }
}
