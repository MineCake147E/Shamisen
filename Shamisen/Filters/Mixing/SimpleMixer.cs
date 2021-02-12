using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Mixes down two signal into one signal.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    public sealed class SimpleMixer : ISampleSource
    {
        private bool disposedValue = false;

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemA"></param>
        /// <param name="itemB"></param>
        public SimpleMixer(IMixerItem itemA, IMixerItem itemB)
        {
            ItemA = itemA ?? throw new ArgumentNullException(nameof(itemA));
            ItemB = itemB ?? throw new ArgumentNullException(nameof(itemB));
            if (!ItemA.Source.Format.Equals(ItemB.Source.Format)) throw new ArgumentException($"The itemA's Format is not same as itemB's Format!");
            Format = ItemA.Source.Format;
        }

        /// <summary>
        /// Gets the format of the audio data.
        /// </summary>
        /// <value>
        /// The format of the audio data.
        /// </value>
        public SampleFormat Format { get; }

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample, TFormat}"/> in frames.
        /// </value>
        public ulong? Length => ModifierUtils.NullOrMax(ItemA.Source.Length, ItemB.Source.Length);

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => ModifierUtils.NullOrMax(ItemA.Source.TotalLength, ItemB.Source.TotalLength);

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.<br/>
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample, TFormat}"/> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample, TFormat}" /> in frames.
        /// </value>
        public ulong? Position => null;

        /// <summary>
        /// Gets the item A.
        /// </summary>
        /// <value>
        /// The item A.
        /// </value>
        public IMixerItem ItemA { get; private set; }

        /// <summary>
        /// Gets the item B.
        /// </summary>
        /// <value>
        /// The item B.
        /// </value>
        public IMixerItem ItemB { get; private set; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => null;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => null;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public ReadResult Read(Span<float> buffer)
        {
            ItemB.CheckBuffer(buffer.Length);
            var rA = ItemA.Read(buffer);
            if (rA.IsEndOfStream)
            {
                var rA2 = ItemB.Read(buffer);
                if (rA2.HasNoData) return rA2;
                buffer.SliceWhile(rA2.Length).FastScalarMultiply(ItemB.Volume);
                var g = ItemA;
                ItemA = ItemB;
                ItemB = g;
                return rA2;
            }
            else if (rA.HasNoData)
            {
                var rA2 = ItemB.Read(buffer);
                buffer.FastScalarMultiply(ItemB.Volume);
                return rA2;
            }
            else if (rA.Length < buffer.Length)
            {
                buffer.SliceWhile(rA.Length).FastScalarMultiply(ItemA.Volume);
                buffer.Slice(rA.Length).FastFill(0);
                var memory = ItemB.Buffer.SliceWhile(buffer.Length);
                var rB = ItemB.Read(memory.Span);
                SpanExtensions.FastMix(memory.Span, buffer, ItemB.Volume);
                return rB;
            }
            else
            {
                buffer.FastScalarMultiply(ItemA.Volume);
                var memory = ItemB.Buffer.SliceWhile(buffer.Length);
                var rB = ItemB.Read(memory.Span);
                SpanExtensions.FastMix(memory.Span, buffer, ItemB.Volume);
                return MathI.Max(rA, rB);
            }
        }

        #region IDisposable Support

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
                    //
                }

                //
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
