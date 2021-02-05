using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Contains some utility functions about <see cref="IAudioSource{TSample, TFormat}"/>
    /// </summary>
    public static class AudioSourceExtensions
    {
        /// <summary>
        /// Reads the audio to the specified buffer asynchronously if possible.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public static async ValueTask<ReadResult> ReadAsAsync<TSample, TFormat>(this IReadableAudioSource<TSample, TFormat> source, Memory<TSample> destination)
            where TSample : unmanaged
            where TFormat : IAudioFormat<TSample>
            => source is IAsyncReadableAudioSource<TSample, TFormat> asource
                ? await asource.ReadAsync(destination)
                : source.Read(destination.Span);
    }
}
