using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an audio encoder.
    /// </summary>
    public interface IEncoder
    {
        /// <summary>
        /// Determines whether the <paramref name="format"/> can be encoded by this codec.
        /// </summary>
        /// <param name="format">The input data's format.</param>
        /// <returns><c>true</c> if the <paramref name="format"/> can be supported by this encoder, otherwise, <c>false</c>.</returns>
        bool IsEncodable(WaveFormat format);

        /// <summary>
        /// Encodes <paramref name="source"/> with the given <paramref name="writeAction"/>.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource"/> to read the data to encode from.</param>
        /// <param name="writeAction">The destination.</param>
        void Encode(IWaveSource source, WriteAction<byte> writeAction);

        /// <summary>
        /// Encodes <paramref name="source"/> with the given <paramref name="writeAction"/>.
        /// </summary>
        /// <param name="source">The <see cref="IWaveSource"/> to read the data to encode from.</param>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="writeAction"/> with.</param>
        /// <param name="writeAction">The destination.</param>
        void Encode<TParam>(IWaveSource source, TParam parameter, WriteWithParameterAction<byte, TParam> writeAction);

        /// <summary>
        /// Encodes <paramref name="source"/> with the given <paramref name="writeAsyncFunc"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource"/> to read the data to encode from.</param>
        /// <param name="writeAsyncFunc">The destination.</param>
        /// <returns>The whole encoding task.</returns>
        Task EncodeAsync(IAsyncWaveSource source, WriteAsyncFunc<byte> writeAsyncFunc);

        /// <summary>
        /// Encodes <paramref name="source"/> with the given <paramref name="writeAsyncFunc"/> asynchronously.
        /// </summary>
        /// <param name="source">The <see cref="IAsyncWaveSource"/> to read the data to encode from.</param>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="writeAsyncFunc"/> with.</param>
        /// <param name="writeAsyncFunc">The destination.</param>
        /// <returns>The whole encoding task.</returns>
        Task EncodeAsync<TParam>(IAsyncWaveSource source, TParam parameter, WriteAsyncFunc<byte> writeAsyncFunc);
    }
}
