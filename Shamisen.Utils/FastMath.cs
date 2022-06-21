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
        /// <summary>
        /// Represents the number of radians in one turn, specified by the constant, τ.
        /// </summary>
        public const float Tau = MathF.PI * 2.0f;
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
        #region FastMultiplyAdd

        /// <summary>
        /// Returns (x * y) + z, sometimes rounded as one ternary operation.
        /// </summary>
        /// <param name="x">The number to be multiplied with y.</param>
        /// <param name="y">The number to be multiplied with x.</param>
        /// <param name="z">The number to be added to the result of x multiplied by y.</param>
        /// <returns>(x * y) + z, sometimes rounded as one ternary operation.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastMultiplyAdd(float x, float y, float z)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return MathF.FusedMultiplyAdd(x, y, z);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Fma.IsSupported)
                {
                    return MathF.FusedMultiplyAdd(x, y, z);
                }
#endif
                return x * y + z;
            }
        }

        /// <summary>
        /// Returns z - (x * y), sometimes rounded as one ternary operation.
        /// </summary>
        /// <param name="x">The number to be multiplied with y.</param>
        /// <param name="y">The number to be multiplied with x.</param>
        /// <param name="z">The number to be added to the result of x multiplied by y.</param>
        /// <returns>z - (x * y), sometimes rounded as one ternary operation.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastMultiplyAddNegated(float x, float y, float z)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.FusedMultiplySubtractScalar(Vector64.CreateScalarUnsafe(z), Vector64.CreateScalarUnsafe(x), Vector64.CreateScalarUnsafe(y)).GetElement(0);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Fma.IsSupported)
                {
                    return Fma.MultiplyAddNegatedScalar(Vector128.CreateScalarUnsafe(x), Vector128.CreateScalarUnsafe(y), Vector128.CreateScalarUnsafe(z)).GetElement(0);
                }
#endif
                return z - x * y;
            }
        }
        #endregion

        #region Cross-Platform Fast Trigonometric
        internal const float S4 = 7.7656368e-2f;
        internal const float S3 = -5.9824574e-1f;
        internal const float S2 = 2.5500606f;
        internal const float S1 = -5.1677083f;
        internal const float S0 = 3.1415926f;

        /// <summary>
        /// Approximates the <see cref="MathF.Sin(float)"/> of the <paramref name="x"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in radians.</param>
        /// <returns>
        /// Approximation of the sine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Sin(float x)
        {
            const float MaxAngle = 1.0f;
            const float SignBit = -0.0f;
            var t = x * 0.15915494f;
            t = Round(t);
            x = x * 0.31830987f - 2 * t;
            var a = MathI.AndNot(SignBit, x);
            t = MaxAngle - a;
            a = Min(a, t);
            var s = MathI.And(SignBit, x);
            var s2 = a * a;
            s = MathI.Xor(s, a);
            var res = S4;
            res = res * s2 + S3;
            res = res * s2 + S2;
            res = res * s2 + S1;
            res = res * s2 + S0;
            return res * s;
        }

        /// <summary>
        /// Approximates the <see cref="MathF.Sin(float)"/> of the <paramref name="x"/> multiplied by <see cref="MathF.PI"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in half turns.</param>
        /// <returns>
        /// Approximation of the sine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float SinPi(float x)
        {
            const float MaxAngle = 1.0f;
            const float SignBit = -0.0f;
            var t = x * 0.5f;
            t = Round(t);
            x -= 2 * t;
            var a = MathI.AndNot(SignBit, x);
            t = MaxAngle - a;
            a = Min(a, t);
            var s = MathI.And(SignBit, x);
            var s2 = a * a;
            s = MathI.Xor(s, a);
            var res = S4;
            res = res * s2 + S3;
            res = res * s2 + S2;
            res = res * s2 + S1;
            res = res * s2 + S0;
            return res * s;
        }

        /// <summary>
        /// Approximates the <see cref="MathF.Cos(float)"/> of the <paramref name="x"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in radians.</param>
        /// <returns>
        /// Approximation of the cosine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Cos(float x) => Sin(x + 1.5707964f);

        /// <summary>
        /// Approximates the <see cref="MathF.Cos(float)"/> of the <paramref name="x"/> multiplied by <see cref="MathF.PI"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in half turns.</param>
        /// <returns>
        /// Approximation of the cosine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float CosPi(float x) => SinPi(x + 0.5f);
        #endregion

        #region Fast Trigonometric
        /// <summary>
        /// Approximates the <see cref="MathF.Sin(float)"/> of the <paramref name="x"/>.
        /// </summary>
        /// <param name="x">An angle, measured in radians.</param>
        /// <returns>
        /// Approximation of the sine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastSin(float x)
        {
            const float MaxAngle = 1.0f;
            const float SignBit = -0.0f;
            var t = x * 0.15915494f;
            t = Round(t);
            x = x * 0.31830987f - 2 * t;
            var a = MathI.AndNot(SignBit, x);
            t = MaxAngle - a;
            a = Min(a, t);
            var s = MathI.And(SignBit, x);
            var s2 = a * a;
            s = MathI.Xor(s, a);
            var res = S4;
            res = FastMultiplyAdd(res, s2, S3);
            res = FastMultiplyAdd(res, s2, S2);
            res = FastMultiplyAdd(res, s2, S1);
            res = FastMultiplyAdd(res, s2, S0);
            return res * s;
        }

        /// <summary>
        /// Approximates the <see cref="MathF.Sin(float)"/> of the <paramref name="x"/> multiplied by <see cref="MathF.PI"/>.
        /// </summary>
        /// <param name="x">An angle, measured in half turns.</param>
        /// <returns>
        /// Approximation of the sine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastSinPi(float x)
        {
            const float MaxAngle = 1.0f;
            const float SignBit = -0.0f;
            var t = x * 0.5f;
            t = Round(t);
            x = FastMultiplyAdd(t, -2, x);
            var a = MathI.AndNot(SignBit, x);
            t = MaxAngle - a;
            a = Min(a, t);
            var s = MathI.And(SignBit, x);
            var s2 = a * a;
            s = MathI.Xor(s, a);
            var res = S4;
            res = FastMultiplyAdd(res, s2, S3);
            res = FastMultiplyAdd(res, s2, S2);
            res = FastMultiplyAdd(res, s2, S1);
            res = FastMultiplyAdd(res, s2, S0);
            return res * s;
        }

        /// <summary>
        /// Approximates the <see cref="MathF.Cos(float)"/> of the <paramref name="x"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in radians.</param>
        /// <returns>
        /// Approximation of the cosine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastCos(float x) => FastSin(x + 1.5707964f);

        /// <summary>
        /// Approximates the <see cref="MathF.Cos(float)"/> of the <paramref name="x"/> multiplied by <see cref="MathF.PI"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">An angle, measured in half turns.</param>
        /// <returns>
        /// Approximation of the cosine of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastCosPi(float x) => FastSinPi(x + 0.5f);
        #endregion

        #region Cross-Platform Fast Exponentiation
        internal const float Exp2C4 = -2.1715025e-4f;
        internal const float Exp2C3 = -1.461238e-3f;
        internal const float Exp2C2 = -1.1139302e-2f;
        internal const float Exp2C1 = -6.6623454e-2f;
        internal const float Exp2C0 = -3.0685297e-1f;

        /// <summary>
        /// Approximates the 2.0f raised to the power <paramref name="x"/>.
        /// This function is fully implemented in C#.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the Exp2 of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Exp2(float x)
        {
            var ip = Max(MathF.Floor(x), -150.0f);
            var ix = (int)ip + 1023;
            var fp = x - ip;
            var res = Exp2C4;
            res = res * fp + Exp2C3;
            var v = 1.0f - fp;
            res = res * fp + Exp2C2;
            v *= fp;
            res = res * fp + Exp2C1;
            var q = BitConverter.Int64BitsToDouble((long)ix << 52);
            res = res * fp + Exp2C0;
            var q2 = (float)q;
            res = res * v + fp;
            q2 = q2 * res + q2;
            return q2;
        }

        #endregion

        #region Fast Exponentiation

        /// <summary>
        /// Approximates the 2.0f raised to the power <paramref name="x"/>.
        /// </summary>
        /// <param name="x">An angle, measured in radians.</param>
        /// <returns>
        /// Approximation of the Exp2 of <paramref name="x"/> computed with a fourth-order polynomial optimized by lolremez.<br/>
        /// If either <see cref="float.IsNaN(float)"/> or <see cref="float.IsInfinity(float)"/> returns <see langword="true"/> for <paramref name="x"/>, this method may return <see cref="float.NaN"/>.
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastExp2(float x)
        {
            var ip = Max(MathF.Floor(x), -150.0f);
            var ix = (int)ip + 1023;
            var fp = x - ip;
            var res = Exp2C4;
            res = FastMultiplyAdd(res, fp, Exp2C3);
            var v = FastMultiplyAddNegated(fp, fp, fp);
            res = FastMultiplyAdd(res, fp, Exp2C2);
            res = FastMultiplyAdd(res, fp, Exp2C1);
            var q = BitConverter.Int64BitsToDouble((long)ix << 52);
            res = FastMultiplyAdd(res, fp, Exp2C0);
            var q2 = (float)q;
            res = FastMultiplyAdd(res, v, fp);
            q2 = FastMultiplyAdd(res, q2, q2);
            return q2;
        }
        #endregion
    }
}
