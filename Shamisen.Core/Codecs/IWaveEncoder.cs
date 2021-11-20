using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an <see cref="IWaveSource"/> encoder.
    /// </summary>
    public interface IWaveEncoder<TEncodingOptions> : IEncoder<byte, IWaveFormat, TEncodingOptions>
        where TEncodingOptions : struct
    {
        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        bool IsEncodable(IWaveSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        ValueTask<bool> IsEncodableAsync(IAsyncWaveSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        void Encode(IWaveSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>The whole encoding task.</returns>
        ValueTask EncodeAsync(IAsyncWaveSource source, IDataSink<byte> sink, TEncodingOptions options = default);
    }
}
