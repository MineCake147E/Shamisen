using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MonoAudio.Data;

namespace MonoAudio.Codecs
{
    /// <summary>
    /// Provides some utility functions for <see cref="IEncoder"/> and <see cref="IDecoder"/>.
    /// </summary>
    public static class CodecUtils
    {
        #region Utility functions for IDecoder

        /// <summary>
        /// Creates a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        public static async Task<IWaveSource> CreateDecoderAsync(this IDecoder codec, ReadFunc<byte> readFunc)
        {
            var reader = new DataReaderOrdinal<byte>(readFunc);
            return await codec.CreateDecoderAsync(reader);
        }

        /// <summary>
        /// Creates a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        public static async Task<IWaveSource> CreateDecoderAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc)
        {
            var reader = new ParametrizedDataReader<byte, TParam>(readFunc, parameter);
            return await codec.CreateDecoderAsync(reader);
        }

        /// <summary>
        /// Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        public static async Task<IWaveSource> CreateDecoderAsync(this IDecoder codec, ReadAsyncFunc<byte> readAsyncFunc)
        {
            var reader = new AsyncDataReader<byte>(readAsyncFunc);
            return await codec.CreateDecoderAsync(reader);
        }

        /// <summary>
        /// Creates a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>The decoding <see cref="IWaveSource"/>.</returns>
        public static async Task<IWaveSource> CreateDecoderAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc)
        {
            var reader = new ParametrizedAsyncDataReader<byte, TParam>(readAsyncFunc, parameter);
            return await codec.CreateDecoderAsync(reader);
        }

        /// <summary>
        /// Determines whether the data from <paramref name="readFunc"/> can be decoded by this decoder.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns><c>true</c> if the data from <paramref name="readFunc"/> can be supported by this decoder, otherwise, <c>false</c>.</returns>
        public static async Task<bool> DetermineDecodabilityAsync(this IDecoder codec, ReadFunc<byte> readFunc)
        {
            using (var reader = new DataReaderOrdinal<byte>(readFunc))
            {
                return await codec.DetermineDecodabilityAsync(reader);
            }
        }

        /// <summary>
        /// Determines whether the data from <paramref name="readFunc"/> can be decoded by this decoder.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns><c>true</c> if the data from <paramref name="readFunc"/> can be supported by this decoder, otherwise, <c>false</c>.</returns>
        public static async Task<bool> DetermineDecodabilityAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc)
        {
            using (var reader = new ParametrizedDataReader<byte, TParam>(readFunc, parameter))
            {
                return await codec.DetermineDecodabilityAsync(reader);
            }
        }

        /// <summary>
        /// Determines whether the data from <paramref name="readAsyncFunc"/> can be decoded by this decoder asynchronously.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="readAsyncFunc"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> DetermineDecodabilityAsync(this IDecoder codec, ReadAsyncFunc<byte> readAsyncFunc)
        {
            using (var reader = new AsyncDataReader<byte>(readAsyncFunc))
            {
                return await codec.DetermineDecodabilityAsync(reader);
            }
        }

        /// <summary>
        /// Determines whether the data from <paramref name="readAsyncFunc"/> can be decoded by this decoder asynchronously.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The <typeparamref name="TParam"/> value to call the <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}" /> to read the data from.</param>
        /// <returns>
        /// The whole verification task which returns the value below:<br/>
        /// <c>true</c> if the data from <paramref name="readAsyncFunc"/> can be supported by this decoder, otherwise, <c>false</c>.
        /// </returns>
        public static async Task<bool> DetermineDecodabilityAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc)
        {
            using (var reader = new ParametrizedAsyncDataReader<byte, TParam>(readAsyncFunc, parameter))
            {
                return await codec.DetermineDecodabilityAsync(reader);
            }
        }

        /// <summary>
        /// Tries to create a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readFunc">The <see cref="ReadFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        public static async Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(this IDecoder codec, ReadFunc<byte> readFunc)
        {
            var reader = new DataReaderOrdinal<byte>(readFunc);
            return await codec.TryCreateDecoderAsync(reader);
        }

        /// <summary>
        /// Tries to create a decoder that decodes the data read from <paramref name="readFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readFunc"/> with.</param>
        /// <param name="readFunc">The <see cref="ReadWithParameterFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        public static async Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterFunc<byte, TParam> readFunc)
        {
            var reader = new ParametrizedDataReader<byte, TParam>(readFunc, parameter);
            return await codec.TryCreateDecoderAsync(reader);
        }

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadAsyncFunc{TSample}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        public static async Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync(this IDecoder codec, ReadAsyncFunc<byte> readAsyncFunc)
        {
            var reader = new AsyncDataReader<byte>(readAsyncFunc);
            return await codec.TryCreateDecoderAsync(reader);
        }

        /// <summary>
        /// Tries to create a decoder that asynchronously decodes the data asynchronously read from <paramref name="readAsyncFunc"/>.
        /// </summary>
        /// <typeparam name="TParam">The type of <paramref name="parameter"/> to call <paramref name="readAsyncFunc"/> with.</typeparam>
        /// <param name="codec">The calling <see cref="IDecoder"/> instance.</param>
        /// <param name="parameter">The value of <typeparamref name="TParam"/> to call <paramref name="readAsyncFunc"/> with.</param>
        /// <param name="readAsyncFunc">The <see cref="ReadWithParameterAsyncFunc{TSample, TParam}"/> to read the data from.</param>
        /// <returns>
        /// success: The value which indicates whether the data is decodable, and the decoder is created.
        /// decoder: The decoding <see cref="IWaveSource"/>.
        /// </returns>
        public static async Task<(bool success, IWaveSource decoder)> TryCreateDecoderAsync<TParam>(this IDecoder codec, TParam parameter, ReadWithParameterAsyncFunc<byte, TParam> readAsyncFunc)
        {
            var reader = new ParametrizedAsyncDataReader<byte, TParam>(readAsyncFunc, parameter);
            return await codec.TryCreateDecoderAsync(reader);
        }

        #endregion Utility functions for IDecoder
    }
}
