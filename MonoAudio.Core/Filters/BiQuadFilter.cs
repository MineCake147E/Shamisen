using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Provides a function of filtering with Digital BiQuad Filter.
    /// </summary>
    public sealed class BiQuadFilter : IAudioFilter<float, SampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiQuadFilter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public BiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter parameter)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Parameter = parameter;
            internalStates = new Vector2[Format.Channels];
            internalStates.AsSpan().Fill(new Vector2(0, 0));
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <value>
        /// The parameter.
        /// </value>
        public BiQuadParameter Parameter { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public bool CanSeek => Source.CanSeek;

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format => Source.Format;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public long Position { get => Source.Position; set => Source.Position = value; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => Source.Length;

        private Vector2[] internalStates;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int Read(Span<float> buffer)
        {
            var len = Source.Read(buffer);
            buffer = buffer.Slice(0, len);
            unsafe
            {
                for (int i = 0; i < buffer.Length; i += Format.Channels)
                {
                    var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < internalStates.Length; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref internalStates[ch]; //Persist reference in order to decrease number of times of range check.
                        ref float v = ref span[ch];
                        var feedForward = v * Parameter.B; //Multiply in one go
                        var sum1 = v = feedForward.X + a.X;
                        var feedBack = sum1 * Parameter.A;  //Multiply in one go
                        a = new Vector2(feedForward.Y + feedBack.X + a.Y, feedForward.Z + feedBack.Y);
                    }
                }
            }
            return len;
        }

        #region IDisposable Support

        private bool disposedValue = false; //

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                Source.Dispose();
                internalStates = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BiQuadFilter"/> class.
        /// </summary>
        ~BiQuadFilter() => Dispose(false);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
