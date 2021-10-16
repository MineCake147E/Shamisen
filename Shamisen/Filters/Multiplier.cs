using System;

using Shamisen.Filters.Mixing;
using Shamisen.Synthesis;
using Shamisen.Utils;

namespace Shamisen.Filters
{
    /// <summary>
    /// Multiplies the instantaneous values of the two sources.<br/>
    /// It can be used to gradually modify the volume of a <see cref="ISampleSource"/>.<br/>
    /// If you put two <see cref="SinusoidSource"/> with different frequencies, the output will be a mix of two new sinusoid with two different frequencies, one at sum, and the other at difference.
    /// </summary>
    public sealed class Multiplier : ISampleSource
    {
        private bool disposedValue;
        /// <summary>
        /// TODO: Docs
        /// </summary>
        /// <param name="itemA"></param>
        /// <param name="itemB"></param>
        /// <exception cref="ArgumentNullException">Either <paramref name="itemA"/> or <paramref name="itemB"/> were <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="itemA"/>'s <see cref="IAudioSource{TSample, TFormat}.Format"/> is not same as <paramref name="itemB"/>'s <see cref="IAudioSource{TSample, TFormat}.Format"/>.</exception>
        public Multiplier(ISourceBufferPair itemA, ISourceBufferPair itemB)
        {
            ItemA = itemA ?? throw new ArgumentNullException(nameof(itemA));
            ItemB = itemB ?? throw new ArgumentNullException(nameof(itemB));
            if (!ItemA.Format.Equals(ItemB.Format)) throw new ArgumentException($"The itemA's Format is not same as itemB's Format!");
            Format = ItemA.Format;
        }

        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        public ulong? Length => ModifierUtils.NullOrMax(ItemA.Length, ItemB.Length);

        /// <inheritdoc/>
        public ulong? TotalLength => ModifierUtils.NullOrMax(ItemA.TotalLength, ItemB.TotalLength);

        /// <inheritdoc/>
        public ulong? Position => null;
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport { get; }
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport { get; }

        /// <summary>
        /// Gets the item A.
        /// </summary>
        /// <value>
        /// The item A.
        /// </value>
        public ISourceBufferPair ItemA { get; private set; }

        /// <summary>
        /// Gets the item B.
        /// </summary>
        /// <value>
        /// The item B.
        /// </value>
        public ISourceBufferPair ItemB { get; private set; }
        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            ItemA.CheckBuffer(buffer.Length);
            ItemB.CheckBuffer(buffer.Length);
            var mA = ItemA.Buffer.SliceWhile(buffer.Length);
            var rA = ItemA.Read(mA.Span);
            var mB = ItemB.Buffer.SliceWhile(buffer.Length);
            var rB = ItemB.Read(mB.Span);
            //The region without data will be treated as 0.
            if (rA.IsEndOfStream)
            {
                buffer.FastFill(0);
                var g = ItemA;
                ItemA = ItemB;
                ItemB = g;
                return rB;
            }
            else if (rA.HasNoData)
            {
                buffer.FastFill(0);
                return rB;
            }
            else if (rB.HasNoData)
            {
                buffer.FastFill(0);
                return rA;
            }
            else
            {
                AudioUtils.FastMultiply(buffer, mA.Span.SliceWhileIfLongerThan(rA.Length), mB.Span.SliceWhileIfLongerThan(rB.Length));
                return MathI.Max(rA, rB);
            }
        }

        /// <inheritdoc/>
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
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
