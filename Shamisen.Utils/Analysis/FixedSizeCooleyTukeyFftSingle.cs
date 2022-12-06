using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Data;
using Shamisen.Utils;

namespace Shamisen.Analysis
{
    /// <summary>
    /// Provides a way to perform FFT with fixed size for monaural signal.
    /// Useful for making real-time FFT analyzer.
    /// </summary>
    public sealed partial class FixedSizeCooleyTukeyFftSingle : IDisposable
    {
        private PooledArray<ComplexF>? cache;
        private bool disposedValue;

        /// <summary>
        /// The size of FFT.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The FFT mode.
        /// </summary>
        public FftMode Mode { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FixedSizeCooleyTukeyFftSingle"/>.
        /// </summary>
        /// <param name="size">The FFT Size. Must be a power of two and be larger than 7.</param>
        /// <param name="mode">The FFT Mode.</param>
        public FixedSizeCooleyTukeyFftSingle(int size, FftMode mode)
        {
            Mode = mode;
            var order = MathI.LogBase2((uint)size);
            size = 1 << order;
            if (size < 8) throw new ArgumentOutOfRangeException(nameof(size), $"{nameof(size)} must be larger than 7!");
            Size = size;
            if (order < 4) return;
            var omo8 = mode == FftMode.Forward ? CooleyTukeyFft.OmegasForwardOrder8 : CooleyTukeyFft.OmegasBackwardOrder8;
            PooledArray<ComplexF> a = new(size - omo8.Length);
            omo8.CopyTo(a.Span);
            var ll = omo8.Length * 2;
            var o = omo8.Length;
            var s = a.Span.SliceWhile(o);
            while (o < a.Length)
            {
                var b = a.Span.Slice(o, ll);
                var c = b.Slice(s.Length);
                s.CopyTo(c);
                CooleyTukeyFft.ExpandCache(b, mode);
                o += ll;
                ll += ll;
                s = b;
            }
            cache = a;
        }

        /// <summary>
        /// Transforms the specified span using Cooley-Tukey algorithm.
        /// </summary>
        /// <param name="destination">The buffer.</param>
        /// <exception cref="ArgumentException">The length of <paramref name="destination"/> must be the same as <see cref="Size"/>! - span</exception>
        public void PerformFft(Span<ComplexF> destination)
        {
            var size = Size;
            if (destination.Length < size) throw new ArgumentException("The length of span must be the same as Size!", nameof(destination));
            destination = destination.SliceWhileIfLongerThan(size);
            CooleyTukeyFft.ReverseInternal(destination);
            Perform(destination, Mode, cache);
            if (Mode == FftMode.Forward)
            {
                var scale = BitConverter.UInt32BitsToSingle(0x30000000u + ((uint)MathI.LeadingZeroCount((uint)destination.Length) << 23));
                var ds = MemoryMarshal.Cast<ComplexF, float>(destination);
                ds.FastScalarMultiply(scale);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void Perform(Span<ComplexF> span, FftMode mode, PooledArray<ComplexF>? cache)
        {
            if (span.Length < 2) return;
            CooleyTukeyFft.Perform2(span);
            if (span.Length < 4) return;
            CooleyTukeyFft.Perform4(span, mode);
            if (span.Length < 8 || cache is null) return;
            PerformLarge(span, cache.Span);
        }

        private static void PerformLarge(Span<ComplexF> span, ReadOnlySpan<ComplexF> cache)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.PerformLargeX86(span, cache);
                    return;
                }
#endif
                Fallback.PerformLargeFallback(span, cache);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                cache?.Dispose();
                cache = null;
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
