using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an audio encoder that handles <see cref="IReadableAudioSource{TSample, TFormat}"/> and <see cref="ISampleSource"/>.
    /// </summary>
    public interface IEncoder<TSample, TFormat, TEncodingOptions> : ISampleEncoder<TEncodingOptions>
        where TEncodingOptions : struct
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        bool IsEncodable(IReadableAudioSource<TSample, TFormat> source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        ValueTask<bool> IsEncodableAsync(IAsyncReadableAudioSource<TSample, TFormat> source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="source">The <see cref="IReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        void Encode(IReadableAudioSource<TSample, TFormat> source, IDataSink<byte> sink, TEncodingOptions options = default);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>The whole encoding task.</returns>
        ValueTask EncodeAsync(IAsyncReadableAudioSource<TSample, TFormat> source, IDataSink<byte> sink, TEncodingOptions options = default);
    }
}
