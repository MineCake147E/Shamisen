using System.Runtime.CompilerServices;

using Shamisen.Utils;
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

namespace Shamisen
{
    /// <summary>
    /// Contains some mathematical functions for Floating-Point numbers.
    /// </summary>
    public static class FastMath
    {
        /// <returns>
        /// Parameter x or y, whichever is larger.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Max(float, float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Max(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.MaxNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse.MaxScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Max(x, y);
            }
        }

        /// <returns>
        /// Parameter x or y, whichever is larger.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="double.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Max(double, double)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double Max(double x, double y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.Create(x);
                    var s1 = Vector64.Create(y);
                    return AdvSimd.MaxNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse2.MaxScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Max(x, y);
            }
        }

        /// <returns>
        /// Parameter x or y, whichever is smaller.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Min(float, float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Min(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.MinNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse.MinScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Min(x, y);
            }
        }

        /// <returns>
        /// Parameter x or y, whichever is smaller.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="double.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        /// <inheritdoc cref="Math.Min(double, double)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double Min(double x, double y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.Create(x);
                    var s1 = Vector64.Create(y);
                    return AdvSimd.MinNumberScalar(s0, s1).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse2.MinScalar(xmm0, xmm1).GetElement(0);
                }
#endif
                return Math.Min(x, y);
            }
        }

        /// <summary>
        /// Returns the smaller of two single-precision floating-point numbers.
        /// This one assumes both <paramref name="x"/> and <paramref name="y"/> to be positive.
        /// </summary>
        /// <returns>
        /// Parameter x or y, whichever is larger.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// This one assumes both <paramref name="x"/> and <paramref name="y"/> to be positive.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float MaxUnsignedInputs(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.Max(s0.AsUInt32(), s1.AsUInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse41.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse41.Max(xmm0.AsUInt32(), xmm1.AsUInt32()).AsSingle().GetElement(0);
                }
#endif
                return BinaryExtensions.UInt32BitsToSingle(MathI.Max(BinaryExtensions.SingleToUInt32Bits(x), BinaryExtensions.SingleToUInt32Bits(y)));
            }
        }
        /// <summary>
        /// Returns the smaller of two single-precision floating-point numbers.
        /// This one assumes both <paramref name="x"/> and <paramref name="y"/> to be positive.
        /// </summary>
        /// <returns>
        /// Parameter x or y, whichever is smaller.
        /// If <paramref name="x"/>, or <paramref name="y"/>, or both <paramref name="x"/> and <paramref name="y"/> are equal to <see cref="float.NaN"/>,
        /// the result might depend on CPUs.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float MinUnsignedInputs(float x, float y)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    var s0 = Vector64.CreateScalarUnsafe(x);
                    var s1 = Vector64.CreateScalarUnsafe(y);
                    return AdvSimd.Min(s0.AsUInt32(), s1.AsUInt32()).AsSingle().GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Sse41.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    return Sse41.Min(xmm0.AsUInt32(), xmm1.AsUInt32()).AsSingle().GetElement(0);
                }
#endif
                return BinaryExtensions.UInt32BitsToSingle(MathI.Min(BinaryExtensions.SingleToUInt32Bits(x), BinaryExtensions.SingleToUInt32Bits(y)));
            }
        }

        /// <summary>
        /// Rounds a single-precision floating-point value to the nearest integral value,
        /// and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="x">A single-precision floating-point number to be rounded.</param>
        /// <returns>The integer nearest <paramref name="x"/>. If the fractional component of <paramref name="x"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a <see cref="float"/> instead of an integral type.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Round(float x)
        {
            unchecked
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                return MathF.Round(x);
#else
                return (float)Math.Round(x);
#endif
            }
        }
    }
}
