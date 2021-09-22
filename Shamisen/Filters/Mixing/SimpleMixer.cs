using System;

using Shamisen.Utils;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Mixes down two signals into one signal.
    /// </summary>
    /// <seealso cref="ISampleSource" />
    public sealed class SimpleMixer : ISampleSource
    {
        private bool disposedValue = false;

        /// <summary>
        /// TODO: Docs
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

        /// <inheritdoc/>
        public SampleFormat Format { get; }

        /// <inheritdoc/>
        public ulong? Length => ModifierUtils.NullOrMax(ItemA.Source.Length, ItemB.Source.Length);

        /// <inheritdoc/>
        public ulong? TotalLength => ModifierUtils.NullOrMax(ItemA.Source.TotalLength, ItemB.Source.TotalLength);

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => null;

        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => null;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            ItemA.CheckBuffer(buffer.Length);
            ItemB.CheckBuffer(buffer.Length);
            var mA = ItemA.Buffer.SliceWhile(buffer.Length);
            var rA = ItemA.Read(mA.Span);
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
                var memory = ItemB.Buffer.SliceWhile(buffer.Length);
                var rB = ItemB.Read(memory.Span);
                AudioUtils.FastMix(buffer, mA.Span.SliceWhileIfLongerThan(rA.Length), ItemA.Volume, memory.Span.SliceWhileIfLongerThan(rB.Length), ItemB.Volume);
                return MathI.Max(rA, rB);
            }
            else
            {
                var memory = ItemB.Buffer.SliceWhile(buffer.Length);
                var rB = ItemB.Read(memory.Span);
                AudioUtils.FastMix(buffer, mA.Span, ItemA.Volume, memory.Span.SliceWhileIfLongerThan(rB.Length), ItemB.Volume);
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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
