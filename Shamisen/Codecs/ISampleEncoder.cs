using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an audio encoder that handles <see cref="ISampleSource"/>.
    /// </summary>
    public interface ISampleEncoder<TEncodingOptions> where TEncodingOptions : struct
    {
        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="ISampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        bool IsEncodable(ISampleSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncSampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        ValueTask<bool> IsEncodableAsync(IAsyncSampleSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="source">The <see cref="ISampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        void Encode(ISampleSource source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncSampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>The whole encoding task.</returns>
        ValueTask EncodeAsync(IAsyncSampleSource source, IDataSink<byte> sink, TEncodingOptions options = default);
    }
}
