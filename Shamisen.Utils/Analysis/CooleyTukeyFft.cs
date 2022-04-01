using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Numerics;
using Shamisen.Utils;
using Shamisen.Utils.Numerics;

namespace Shamisen.Analysis
{
    /// <summary>
    /// Contains functionality of Fast Fourier Transform.
    /// </summary>
    public static partial class CooleyTukeyFft
    {
        private const double Sqrt2 = 1.41421356237309504880168872420969807856967187537694807317667973799073247846210703885038753432764157273501384623091229702492483605585073721264412149709993583141322266592750559275579995050115278206057147010955997160597027453459686;
        private const double SqrtHalf = 0.5 * Sqrt2;
        private const float Sqrt2F = (float)Sqrt2;
        private const float SqrtHalfF = (float)SqrtHalf;

        internal const double Tau = 2 * Math.PI;

        /// <summary>
        /// Contains (1 &lt;&lt; n) root of unity.
        /// </summary>
        private static readonly ReadOnlyMemory<Complex> PowerRootsOfUnity = new Complex[]
{
            /*      1 */new(1, 0),
            /*      2 */new(-1, 0),
            /*      4 */new(0, 1),
            /*      8 */new(0.70710678118654757, 0.70710678118654757),
            /*     16 */new(0.92387953251128674, 0.38268343236508978),
            /*     32 */new(0.98078528040323043, 0.19509032201612828),
            /*     64 */new(0.99518472667219693, 0.0980171403295606),
            /*    128 */new(0.99879545620517241, 0.049067674327418015),
            /*    256 */new(0.99969881869620425, 0.024541228522912288),
            /*    512 */new(0.9999247018391445, 0.012271538285719925),
            /*   1024 */new(0.99998117528260111, 0.0061358846491544753),
            /*   2048 */new(0.99999529380957619, 0.0030679567629659761),
            /*   4096 */new(0.99999882345170188, 0.0015339801862847657),
            /*   8192 */new(0.99999970586288223, 0.00076699031874270449),
            /*  16384 */new(0.99999992646571789, 0.00038349518757139556),
            /*  32768 */new(0.99999998161642933, 0.00019174759731070332),
            /*  65536 */new(0.99999999540410733, 9.5873799095977345E-05),
            /* 131072 */new(0.99999999885102686, 4.7936899603066881E-05),
            /* 262144 */new(0.99999999971275666, 2.3968449808418219E-05),
            /* 524288 */new(0.99999999992818922, 1.1984224905069707E-05),
            /*1048576 */new(0.99999999999551181, 2.9960562263346608E-06),
            /*2097152 */new(0.999999999998878, 1.4980281131690111E-06)
};

        #region Common Caches
        private static ReadOnlySpan<byte> OmegasForwardOrder8Internal => new byte[] { 0, 0, 128, 63, 0, 0, 0, 0, 243, 4, 53, 63, 243, 4, 53, 191, 0, 0, 0, 0, 0, 0, 128, 191, 243, 4, 53, 191, 243, 4, 53, 191 };
        internal static ReadOnlySpan<ComplexF> OmegasForwardOrder8 => MemoryMarshal.Cast<byte, ComplexF>(OmegasForwardOrder8Internal);
        private static ReadOnlySpan<byte> OmegasBackwardOrder8Internal => new byte[] { 0, 0, 128, 63, 0, 0, 0, 0, 243, 4, 53, 63, 243, 4, 53, 63, 0, 0, 0, 0, 0, 0, 128, 63, 243, 4, 53, 191, 243, 4, 53, 63 };
        internal static ReadOnlySpan<ComplexF> OmegasBackwardOrder8 => MemoryMarshal.Cast<byte, ComplexF>(OmegasBackwardOrder8Internal);
        #endregion

        /// <summary>
        /// Transforms the specified span using Cooley-Tukey algorithm.
        /// </summary>
        /// <param name="span">The buffer.</param>
        /// <param name="mode">The FFT's Mode.</param>
        /// <exception cref="ArgumentException">The length of span must be power of 2! - span</exception>
        public static void FFT(Span<Complex> span, FftMode mode = FftMode.Forward)
        {
            if (!MathI.IsPowerOfTwo(span.Length))
            {
#if DEBUG
                throw new ArgumentException("The length of span must be power of 2!", nameof(span));
#else
                span = span.SliceWhile((int)MathI.ExtractHighestSetBit((uint)span.Length));
#endif
            }
            ReverseInternal(span);
            Perform(span, mode);
            if (mode == FftMode.Forward)
            {
                var scale = 1.0 / span.Length;
                var ds = MemoryMarshal.Cast<Complex, double>(span);
                ds.FastScalarMultiply(scale);
            }
        }

        /// <summary>
        /// Transforms the specified span using Cooley-Tukey algorithm.
        /// </summary>
        /// <param name="span">The buffer.</param>
        /// <param name="mode">The FFT's Mode.</param>
        /// <exception cref="ArgumentException">The length of span must be power of 2! - span</exception>
        public static void FFT(Span<ComplexF> span, FftMode mode = FftMode.Forward)
        {
            if (!MathI.IsPowerOfTwo(span.Length))
            {
#if DEBUG
                throw new ArgumentException("The length of span must be power of 2!", nameof(span));
#else
                span = span.SliceWhile((int)MathI.ExtractHighestSetBit((uint)span.Length));
#endif
            }
            ReverseInternal(span);
            Perform(span, mode);
            if (mode == FftMode.Forward)
            {
                var scale = BinaryExtensions.UInt32BitsToSingle(0x30000000u + ((uint)MathI.LeadingZeroCount((uint)span.Length) << 23));
                var ds = MemoryMarshal.Cast<ComplexF, float>(span);
                ds.FastScalarMultiply(scale);
            }
        }

        /// <summary>
        /// Transforms the specified buffer using Cooley-Tukey algorithm.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public static void FFT(Complex[] buffer, int offset, int count) => FFT(new Span<Complex>(buffer, offset, count));

        /// <summary>
        /// Transforms the specified buffer using Cooley-Tukey algorithm.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public static void FFT(ComplexF[] buffer, int offset, int count) => FFT(new Span<ComplexF>(buffer, offset, count));

        /// <summary>
        /// Performs Bit-Reversal permutation of the <paramref name="span"/>.
        /// </summary>
        /// <param name="span">The in/out span.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static void Reverse<T>(Span<T> span)
        {
            if (!MathI.IsPowerOfTwo(span.Length)) throw new ArgumentException("The length of span must be a power of 2!", nameof(span));
            ReverseInternal(span);
        }

        /// <summary>
        /// Calculates the cache of the powers of root of unity.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="index">The index.</param>
        /// <param name="omegas">The omegas.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void CalculateCache(FftMode mode, int index, Span<Complex> omegas)
        {
            //O(n*log(n))
            omegas[0] = GetValueToMultiply(0);
            var max = index - 1;
            for (var g = 0; g < max; g++)
            {
                var mask = 0x1 << g;
                var omegaM = GetValueToMultiply(index - g);
                omegaM = mode == FftMode.Forward ? Complex.Conjugate(omegaM) : omegaM;
                var prev = omegas.Slice(0, mask);
                var newRegion = omegas.Slice(mask, mask);
                ComplexUtils.MultiplyAll(newRegion, prev, omegaM);
            }
        }

        /// <summary>
        /// Calculates the cache of the powers of root of unity.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="index">The index.</param>
        /// <param name="omegas">The omegas.</param>
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void CalculateCache(FftMode mode, int index, Span<ComplexF> omegas)
        {
            //O(n*log(n))
            omegas[0] = (ComplexF)GetValueToMultiply(0);
            var max = index - 1;
            for (var g = 0; g < max; g++)
            {
                var mask = 0x1 << g;
                var omegaM = (ComplexF)GetValueToMultiply(index - g);
                omegaM = mode == FftMode.Forward ? ComplexF.Conjugate(omegaM) : omegaM;
                var prev = omegas.Slice(0, mask);
                var newRegion = omegas.Slice(mask, mask);
                ComplexUtils.MultiplyAll(newRegion, prev, omegaM);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Complex GetValueToMultiply(int pos)
            => pos >= PowerRootsOfUnity.Length ? Complex.FromPolarCoordinates(1, Tau * 1.0 / (1 << pos))
         : Unsafe.Add(ref MemoryMarshal.GetReference(PowerRootsOfUnity.Span), pos);

        /// <summary>
        /// Performs forward transform to the specified span.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="mode">The FFT's Mode.</param>
#if NET5_0_OR_GREATER

        [SkipLocalsInit]
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void Perform(Span<Complex> span, FftMode mode)
        {
            if (span.Length < 2) return;
            Perform2(span);
            if (span.Length < 4) return;
            switch (mode)
            {
                case FftMode.Forward:
                    Perform4Forward(span);
                    break;
                case FftMode.Backward:
                    Perform4Backward(span);
                    break;
            }
            var pool = ArrayPool<byte>.Shared;
            var region = pool.Rent(Unsafe.SizeOf<Complex>() * span.Length / 2);
            var buffer = MemoryMarshal.Cast<byte, Complex>(region.AsSpan());
            var omegas = buffer.Slice(0, span.Length / 2);
            CalculateCache(mode, MathI.LogBase2((uint)span.Length), omegas);
            ref var omH = ref MemoryMarshal.GetReference(omegas);
            var oMask = omegas.Length - 1;
            var index = 3;
            var m = 8;
            Span<Complex> cache = stackalloc Complex[span.Length / 8];
            ref var cacheH = ref MemoryMarshal.GetReference(cache);
            for (; m <= span.Length / 8; m <<= 1)
            {
                var mHalf = m >> 1;
                var step = span.Length >> index;
                var jj = 0;
                for (var gg = 0; gg < mHalf; gg++)
                {
                    cache[gg] = omegas[jj];
                    jj += step;
                }
                var i = 0;
                ref var rA = ref span[0];
                ref var rB = ref Unsafe.Add(ref rA, mHalf);
                for (var k = 0; k < span.Length; k += m)
                {
                    i = 0;
                    for (var j = 0; j < mHalf; j++)
                    {
                        var t = Unsafe.Add(ref cacheH, i) * rB;
                        var u = rA;
                        rA = u + t;
                        rB = u - t;
                        i++;
                        rA = ref Unsafe.Add(ref rA, 1);
                        rB = ref Unsafe.Add(ref rB, 1);
                    }
                    rA = ref Unsafe.Add(ref rA, mHalf);
                    rB = ref Unsafe.Add(ref rB, mHalf);
                }
                index++;
            }
            for (; m <= span.Length; m <<= 1)
            {
                Complex t, u;
                var mHalf = m >> 1;
                var step = span.Length >> index;
                var i = 0;
                ref var rA = ref span[0];
                ref var rB = ref Unsafe.Add(ref rA, mHalf);
                for (var k = 0; k < span.Length; k += m)
                {
                    i = 0;
                    for (var j = 0; j < mHalf; j++)
                    {
                        t = Unsafe.Add(ref omH, i) * rB;
                        u = rA;
                        rA = u + t;
                        rB = u - t;
                        i += step;
                        rA = ref Unsafe.Add(ref rA, 1);
                        rB = ref Unsafe.Add(ref rB, 1);
                    }
                    rA = ref Unsafe.Add(ref rA, mHalf);
                    rB = ref Unsafe.Add(ref rB, mHalf);
                }
                index++;
            }
            pool.Return(region);
        }

        /// <summary>
        /// Performs forward transform to the specified span.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="mode">The FFT's Mode.</param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void Perform(Span<ComplexF> span, FftMode mode)
        {
            if (span.Length < 2) return;
            Perform2(span);
            if (span.Length < 4) return;
            Perform4(span, mode);
            if (span.Length < 8) return;
            PerformLarge(span, mode);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ExpandCache(Span<ComplexF> span, FftMode mode)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.ExpandCacheX86(span, mode);
                    return;
                }
#endif
                Fallback.ExpandCacheFallback(span, mode);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void PerformSingleOperation(ref ComplexF pA, ref ComplexF pB, in ComplexF om)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.PerformSingleOperationX86(ref pA, ref pB, om);
                    return;
                }
#endif
                Fallback.PerformSingleOperationFallback(ref pA, ref pB, om);
            }
        }

        private static void PerformLarge(Span<ComplexF> span, FftMode mode)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.PerformLargeX86(span, mode);
                    return;
                }
#endif
                Fallback.PerformLargeFallback(span, mode);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void Perform2(Span<Complex> span)
        {
            var sD = MemoryMarshal.Cast<Complex, (Complex, Complex)>(span);
            for (var k = 0; k < sD.Length; k++)
            {
                ref var sA = ref sD[k];
                var t = sA.Item2;
                var u = sA.Item1;
                sA.Item2 = u - t;
                sA.Item1 = u + t;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void Perform2(Span<ComplexF> span)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.Perform2X86(span);
                    return;
                }
#endif
                Fallback.Perform2Fallback(span);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        internal static void Perform4(Span<ComplexF> span, FftMode mode)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.Perform4X86(span, mode);
                    return;
                }
#endif
                Fallback.Perform4Fallback(span, mode);
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void Perform4Backward(Span<Complex> span)
        {
            var sQ = MemoryMarshal.Cast<Complex, (Complex s0, Complex s1, Complex s2, Complex s3)>(span);
            for (var k = 0; k < sQ.Length; k++)
            {
                ref var sA = ref sQ[k];
                var v = new Complex(-sA.s3.Imaginary, sA.s3.Real);
                var t = sA.s2;
                var w = sA.s1;
                var u = sA.s0;
                sA.s0 = u + t;
                sA.s1 = w + v;
                sA.s2 = u - t;
                sA.s3 = w - v;
            }
        }

        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private static void Perform4Forward(Span<Complex> span)
        {
            var sQ = MemoryMarshal.Cast<Complex, (Complex, Complex, Complex, Complex)>(span);
            for (var k = 0; k < sQ.Length; k++)
            {
                ref var sA = ref sQ[k];
                var v = new Complex(sA.Item4.Imaginary, -sA.Item4.Real);
                var t = sA.Item3;
                var w = sA.Item2;
                var u = sA.Item1;
                sA.Item1 = u + t;
                sA.Item2 = w + v;
                sA.Item3 = u - t;
                sA.Item4 = w - v;
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ReverseInternal<T>(Span<T> span)
        {
            //O(sqrt(n)) permutation
            var length = (int)MathI.ExtractHighestSetBit((uint)span.Length);
            var bits = MathI.LogBase2((uint)length);
            var shift = 32 - bits;
            ref var x9 = ref MemoryMarshal.GetReference(span);
            for (var i = length >> (bits >> 1); i < length; i++)
            {
                var shifted = (uint)i << shift;
                var d = MathI.TrailingZeroCount((uint)i);
                var q = MathI.LeadingZeroCount(shifted);
                if (q > d) continue;
                ref var x1 = ref Unsafe.Add(ref x9, i);
                var index = (int)MathI.ReverseBitOrder(shifted);
                ref var x2 = ref Unsafe.Add(ref x9, index);
                var v = x1;
                if (index >= i) continue;
                x1 = x2;
                x2 = v;
            }
        }
    }
}
