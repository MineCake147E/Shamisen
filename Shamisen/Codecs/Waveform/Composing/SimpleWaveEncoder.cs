using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Waveform.Chunks;
using Shamisen.Codecs.Waveform.Riff;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Data;
using Shamisen.Formats;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Provides functionality of encoding WAVE file format.
    /// </summary>
    /// <seealso cref="IEncoder{TSample, TFormat, TEncodingOptions}" />
#pragma warning disable S3453 // Classes should not have only "private" constructors

    public sealed partial class SimpleWaveEncoder : IWaveEncoder<SimpleWaveEncoderOptions>
#pragma warning restore S3453 // Classes should not have only "private" constructors
    {
        private const ulong ChunkHeaderSize = 8;
        private const uint MaxBufferSize = 1u << 17;

        private SimpleWaveEncoder()
        {
        }

        /// <summary>
        /// Gets the global instance of <see cref="SimpleWaveEncoder"/>.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static SimpleWaveEncoder Instance { get; } = new();

        /// <summary>
        /// Encodes <paramref name="source" /> to the specified <paramref name="sink" />.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource" /> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        public void Encode(IWaveSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default) => Compose(source, sink, options);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="source">The <see cref="IReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        public void Encode(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => Compose(source, sink, options);

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/>.
        /// </summary>
        /// <param name="source">The <see cref="ISampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        public void Encode(ISampleSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => Compose(new SampleToFloat32Converter(source), sink, options);

        /// <summary>
        /// Encodes <paramref name="source" /> to the specified <paramref name="sink" /> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource" /> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>
        /// The whole encoding task.
        /// </returns>
        public ValueTask EncodeAsync(IAsyncWaveSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => throw new NotImplementedException();

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>The whole encoding task.</returns>
        public ValueTask EncodeAsync(IAsyncReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => throw new NotImplementedException();

        /// <summary>
        /// Encodes <paramref name="source"/> to the specified <paramref name="sink"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncSampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns>The whole encoding task.</returns>
        public ValueTask EncodeAsync(IAsyncSampleSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => throw new NotImplementedException();

        /// <summary>
        /// Determines whether the <paramref name="source" /> can be encoded to <paramref name="sink" /> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource" /> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <returns>
        ///   <c>true</c> if the <paramref name="source" /> is supported by this encoder and <paramref name="sink" /> meets the requirement of encoding, otherwise, <c>false</c>.
        /// </returns>
        /// <param name="options">The encoding options.</param>
        public bool IsEncodable(IWaveSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)

            => sink.SeekSupport is { } || source.TotalLength is { };

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        public bool IsEncodable(IReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => sink.SeekSupport is { } || source.TotalLength is { };

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="ISampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        public bool IsEncodable(ISampleSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => sink.SeekSupport is { } || source.TotalLength is { };

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        public async ValueTask<bool> IsEncodableAsync(IAsyncWaveSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => sink.SeekSupport is { } || source.TotalLength is { };

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncReadableAudioSource{TSample, TFormat}"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        public ValueTask<bool> IsEncodableAsync(IAsyncReadableAudioSource<byte, IWaveFormat> source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => throw new NotImplementedException();

        /// <summary>
        /// Determines whether the <paramref name="source"/> can be encoded to <paramref name="sink"/> by this codec.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncSampleSource"/> to read the data to encode from.</param>
        /// <param name="sink">The destination.</param>
        /// <param name="options">The encoding options.</param>
        /// <returns><c>true</c> if the <paramref name="source"/> is supported by this encoder and <paramref name="sink"/> meets the requirement of encoding, otherwise, <c>false</c>.</returns>
        public ValueTask<bool> IsEncodableAsync(IAsyncSampleSource source, IDataSink<byte> sink, SimpleWaveEncoderOptions options = default)
            => throw new NotImplementedException();
    }
}
