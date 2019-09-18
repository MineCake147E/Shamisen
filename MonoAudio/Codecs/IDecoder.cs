using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonoAudio.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an audio decoder.
    /// </summary>
    public interface IDecoder
    {
        /// <summary>
        /// Determines whether the data from <paramref name="readFunc"/> can be decoded by this decoder.
        /// </summary>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns><c>true</c> if the data from <paramref name="readFunc"/> can be supported by this decoder, otherwise, <c>false</c>.</returns>
        bool IsDecodable(ReadFunc<byte> readFunc);

        /// <summary>
        /// Determines whether the data from <paramref name="readFunc"/> can be decoded by this decoder.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns><c>true</c> if the data from <paramref name="readFunc"/> can be supported by this decoder, otherwise, <c>false</c>.</returns>
        bool IsDecodable<TParam>(TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc);

        /// <summary>
        /// Determines whether the data from <paramref name="readAsyncFunc"/> can be decoded by this decoder asynchronously.
        /// </summary>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="readAsyncFunc"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DetermineDecodabilityAsync(ReadAsyncFunc<byte> readAsyncFunc);

        /// <summary>
        /// Determines whether the data from <paramref name="readAsyncFunc"/> can be decoded by this decoder asynchronously.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}" /> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="readAsyncFunc"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        Task<bool> DetermineDecodabilityAsync<TParam>(TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc);

        /// <summary>
        /// Creates a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        IWaveSource CreateDecoder(ReadFunc<byte> readFunc);

        /// <summary>
        /// Creates a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        IWaveSource CreateDecoder<TParam>(TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc);

        /// <summary>
        /// Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IAsyncWaveSource"/>.</returns>
        Task<IAsyncWaveSource> CreateDecoderAsync(ReadAsyncFunc<byte> readAsyncFunc);

        /// <summary>
        /// Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IAsyncWaveSource"/>.</returns>
        Task<IAsyncWaveSource> CreateDecoderAsync<TParam>(TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc);

        /// <summary>
        /// Tries to create a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        (bool success, IWaveSource decoder) TryCreateDecoder(ReadFunc<byte> readFunc);

        /// <summary>
        /// Tries to create a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        (bool success, IWaveSource decoder) TryCreateDecoder<TParam>(TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc);

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        Task<(bool success, IAsyncWaveSource decoder)> TryCreateDecoderAsync(ReadAsyncFunc<byte> readAsyncFunc);

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        Task<(bool success, IAsyncWaveSource decoder)> TryCreateDecoderAsync<TParam>(TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc);
    }
}
