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
        /// Determines whether the data from <paramref name="dataSource"/> can be decoded by this decoder asynchronously.<br/>
        /// The actual decoding stream must be opened after seeking the source <see cref="Stream"/> to head.
        /// </summary>
        /// <param name="dataSource">The <see cref="IDataSource{TSample}"/> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="dataSource"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        ValueTask<bool> DetermineDecodabilityAsync(IDataSource<byte> dataSource);

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="dataSource"/>.
        /// </summary>
        /// <param name="dataSource">The <see cref="IDataSource{TSample}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        ValueTask<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(IDataSource<byte> dataSource);
    }
}
