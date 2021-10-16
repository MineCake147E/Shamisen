using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters.BiQuad.Cascaded
{
    /// <summary>
    /// Filters audio signal with multiple Digital BiQuad Filter.
    /// </summary>
    public sealed partial class CascadedBiQuadFilter : ISampleFilter
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of <see cref="CascadedBiQuadFilter"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameters">The filters for all channels.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CascadedBiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter[] parameters)
        {
            int channels = source.Format.Channels;

            Source = source;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CascadedBiQuadFilter"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public CascadedBiQuadFilter(IReadableAudioSource<float, SampleFormat> source, CascadedBiQuadParameters parameters)
        {
            if (parameters.Channels != source.Format.Channels) throw new ArgumentException("", nameof(parameters));

            Source = source;
        }

        /// <inheritdoc/>
        public IReadableAudioSource<float, SampleFormat> Source { get; }
        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        public ulong? Length { get; }
        /// <inheritdoc/>
        public ulong? TotalLength { get; }
        /// <inheritdoc/>
        public ulong? Position { get; }
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer) => throw new NotImplementedException();

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
