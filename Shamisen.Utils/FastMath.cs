using System.Runtime.CompilerServices;

using Shamisen.Utils;
#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen;

using System.Reflection.Metadata;

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
        #region Max
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
                return BitConverter.UInt32BitsToSingle(MathI.Max(BitConverter.SingleToUInt32Bits(x), BitConverter.SingleToUInt32Bits(y)));
            }
        }
        #endregion

        #region Min
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
                return BitConverter.UInt32BitsToSingle(MathI.Min(BitConverter.SingleToUInt32Bits(x), BitConverter.SingleToUInt32Bits(y)));
            }
        }
        #endregion

        #region Round

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
        #endregion

        #region FastMultiplyAdd

        /// <summary>
        /// Returns (x * y) + z, sometimes rounded as one ternary operation.
        /// </summary>
        /// <param name="x">The number to be multiplied with y.</param>
        /// <param name="y">The number to be multiplied with x.</param>
        /// <param name="z">The number to be added to the result of x multiplied by y.</param>
        /// <returns>(<paramref name="x"/> * y) + z, sometimes rounded as one ternary operation.</returns>
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
        /// Returns (x * y) + z, sometimes rounded as one ternary operation.
        /// </summary>
        /// <param name="x">The number to be multiplied with y.</param>
        /// <param name="y">The number to be multiplied with x.</param>
        /// <param name="z">The number to be added to the result of x multiplied by y.</param>
        /// <returns>(<paramref name="x"/> * y) + z, sometimes rounded as one ternary operation.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double FastMultiplyAdd(double x, double y, double z)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return Math.FusedMultiplyAdd(x, y, z);
                }
#endif
#if NETCOREAPP3_1_OR_GREATER
                if (Fma.IsSupported)
                {
                    return Math.FusedMultiplyAdd(x, y, z);
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

        /// <summary>
        /// Returns z - (x * y), sometimes rounded as one ternary operation.
        /// </summary>
        /// <param name="x">The number to be multiplied with y.</param>
        /// <param name="y">The number to be multiplied with x.</param>
        /// <param name="z">The number to be added to the result of x multiplied by y.</param>
        /// <returns>z - (x * y), sometimes rounded as one ternary operation.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static double FastMultiplyAddNegated(double x, double y, double z)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.FusedMultiplySubtractScalar(Vector64.Create(z), Vector64.Create(x), Vector64.Create(y)).GetElement(0);
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
            var fp = x - ip;
            var res = Exp2C4;
            var ix = (long)ip + 1023;
            res = FastMultiplyAdd(res, fp, Exp2C3);
            var v = FastMultiplyAddNegated(fp, fp, fp);
            res = FastMultiplyAdd(res, fp, Exp2C2);
            ix <<= 52;
            res = FastMultiplyAdd(res, fp, Exp2C1);
            var q = BitConverter.Int64BitsToDouble(ix);
            res = FastMultiplyAdd(res, fp, Exp2C0);
            var q2 = (float)q;
            res = FastMultiplyAdd(res, v, fp);
            q2 = FastMultiplyAdd(res, q2, q2);
            return q2;
        }
        #endregion

        #region Cross-Platform Fast Logarithm
        internal const float Log2C8 = 3.6030097e-3f;
        internal const float Log2C7 = -2.0096829e-2f;
        internal const float Log2C6 = 5.2624937e-2f;
        internal const float Log2C5 = -9.0581175e-2f;
        internal const float Log2C4 = 1.2433369e-1f;
        internal const float Log2C3 = -1.5743491e-1f;
        internal const float Log2C2 = 2.0215821e-1f;
        internal const float Log2C1 = -2.7864946e-1f;
        internal const float Log2C0 = 4.4269502e-1f;

        /// <summary>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// All numbers, including <see cref="float.PositiveInfinity"/> and <see cref="float.NaN"/>, are treated as normal number, so it returns -127 for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Log2AsNormal(float x)
        {
            x = Math.Abs(x);
            var y = MathI.AndNot(BitConverter.UInt32BitsToSingle(0x7f80_0000u), x);
            const float One = 1.0f;
            x = MathI.SubtractInteger(x, One);
            y = MathI.AddInteger(One, y);
            y -= One;
            var z = Log2C8;
            var ix = BitConverter.SingleToInt32Bits(x);
            z = z * y + Log2C7;
            z = z * y + Log2C6;
            z = z * y + Log2C5;
            ix >>= 23;
            z = z * y + Log2C4;
            z = z * y + Log2C3;
            x = ix;
            z = z * y + Log2C2;
            z = z * y + Log2C1;
            var w = y - y * y;
            z = z * y + Log2C0;
            z = z * w + y;
            x += z;
            return x;
        }

        /// <summary>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// Unlike <see cref="Log2AsNormal(float)"/>, it checks for exponent if it's neither 255 nor 0, so it returns <see cref="float.NegativeInfinity"/> for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Log2(float x)
        {
            x = Math.Abs(x);
            var v = (double)x;
            const double One = 1.0;
            var p = MathI.AndNot(BitConverter.UInt64BitsToDouble(0x7ff0_0000_0000_0000ul), v);
            v = MathI.SubtractInteger(v, One);
            p = MathI.AddInteger(One, p);
            p -= One;
            var y = (float)p;
            var z = Log2C8;
            var ix = BitConverter.DoubleToInt64Bits(v);
            z = z * y + Log2C7;
            ix >>= 52;
            z = z * y + Log2C6;
            var isnan = ix == 1024;
            var nisnan = (uint)-Unsafe.As<bool, byte>(ref isnan);
            var isz = ix == -1023;
            var nisz = (uint)-Unsafe.As<bool, byte>(ref isz);
            z = z * y + Log2C5;
            ix += (int)nisz;
            z = z * y + Log2C4;
            x = ix;
            z = z * y + Log2C3;
            nisz &= 0x7F80_0000u;
            nisz |= nisnan;
            z = z * y + Log2C2;
            x = MathI.Or(x, BitConverter.UInt32BitsToSingle(nisz));
            z = z * y + Log2C1;
            var w = y - y * y;
            z = z * y + Log2C0;
            z = z * w + y;
            x += z;
            return x;
        }

        #region Experimental

        /// <summary>
        /// EXPERIMENTAL FUNCTION DO NOT USE FOR PRODUCTION PURPOSE<br/>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// All numbers, including <see cref="float.PositiveInfinity"/> and <see cref="float.NaN"/>, are treated as normal number, so it returns -127 for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float Log2AsNormalEstrin(float x)
        {
            x = Math.Abs(x);
            var y = MathI.AndNot(BitConverter.UInt32BitsToSingle(0x7f80_0000u), x);
            const float One = 1.0f;
            x = MathI.SubtractInteger(x, One);
            y = MathI.AddInteger(One, y);
            y -= One;
            var ix = BitConverter.SingleToInt32Bits(x);
            ix >>= 23;
            x = ix;
            var y2 = y * y;
            var z0 = Log2C0 + y * Log2C1;
            var z1 = Log2C2 + y * Log2C3;
            var z2 = Log2C4 + y * Log2C5;
            var z3 = Log2C6 + y * Log2C7;
            var y4 = y2 * y2;
            var y8 = y4 * y4;
            var w = y - y2;
            z0 += y2 * z1;
            z2 += y2 * z3;
            z0 += y4 * z2;
            z0 += y8 * Log2C8;
            z0 = z0 * w + y;
            x += z0;
            return x;
        }

        /// <summary>
        /// EXPERIMENTAL FUNCTION DO NOT USE FOR PRODUCTION PURPOSE<br/>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// All numbers, including <see cref="float.PositiveInfinity"/> and <see cref="float.NaN"/>, are treated as normal number, so it returns -127 for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float Log2AsNormalCake0(float x)
        {
            x = Math.Abs(x);
            var y = MathI.AndNot(BitConverter.UInt32BitsToSingle(0x7f80_0000u), x);
            const float One = 1.0f;
            x = MathI.SubtractInteger(x, One);
            y = MathI.AddInteger(One, y);
            y -= One;
            var ix = BitConverter.SingleToInt32Bits(x);
            ix >>= 23;
            x = ix;
            var y2 = y * y;
            var z0 = Log2C6 + y2 * Log2C8;
            var z1 = Log2C5 + y2 * Log2C7;
            z0 = z0 * y2 + Log2C4;
            z1 = z1 * y2 + Log2C3;
            z0 = z0 * y2 + Log2C2;
            z1 = z1 * y2 + Log2C1;
            z0 = z0 * y2 + Log2C0;
            var w = y - y2;
            z0 += z1 * y;
            z0 = z0 * w + y;
            x += z0;
            return x;
        }
        #endregion
        #endregion

        #region Fast Logarithm
        /// <summary>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// All numbers, including <see cref="float.PositiveInfinity"/> and <see cref="float.NaN"/>, are treated as normal number, so it returns -127 for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastLog2AsNormal(float x)
        {
            x = Math.Abs(x);
            var y = MathI.AndNot(BitConverter.UInt32BitsToSingle(0x7f80_0000u), x);
            const float One = 1.0f;
            x = MathI.SubtractInteger(x, One);
            y = MathI.AddInteger(One, y);
            y -= One;
            var z = Log2C8;
            var ix = BitConverter.SingleToInt32Bits(x);
            z = FastMultiplyAdd(z, y, Log2C7);
            z = FastMultiplyAdd(z, y, Log2C6);
            z = FastMultiplyAdd(z, y, Log2C5);
            ix >>= 23;
            z = FastMultiplyAdd(z, y, Log2C4);
            z = FastMultiplyAdd(z, y, Log2C3);
            x = ix;
            z = FastMultiplyAdd(z, y, Log2C2);
            z = FastMultiplyAdd(z, y, Log2C1);
            var w = FastMultiplyAddNegated(y, y, y);
            z = FastMultiplyAdd(z, y, Log2C0);
#pragma warning disable S2234 // Parameters should be passed in the correct order
            z = FastMultiplyAdd(z, w, y);
#pragma warning restore S2234 // Parameters should be passed in the correct order
            x += z;
            return x;
        }

        /// <summary>
        /// Approximates the log base 2 of <paramref name="x"/>.<br/>
        /// Negative input will result in the log2 of negated <paramref name="x"/>.<br/>
        /// Unlike <see cref="Log2AsNormal(float)"/>, it checks for exponent if it's neither 255 nor 0, so it returns <see cref="float.NegativeInfinity"/> for 0.
        /// </summary>
        /// <param name="x">The number that specifies a power.</param>
        /// <returns>
        /// Approximation of the log base 2 of <paramref name="x"/> computed with a 8th-order polynomial optimized by lolremez.<br/>
        /// </returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastLog2(float x)
        {
            x = Math.Abs(x);
            var v = (double)x;
            const double One = 1.0;
            var p = MathI.AndNot(BitConverter.UInt64BitsToDouble(0x7ff0_0000_0000_0000ul), v);
            v = MathI.SubtractInteger(v, One);
            p = MathI.AddInteger(One, p);
            p -= One;
            var y = (float)p;
            var z = Log2C8;
            var ix = BitConverter.DoubleToInt64Bits(v);
            z = FastMultiplyAdd(z, y, Log2C7);
            ix >>= 52;
            z = FastMultiplyAdd(z, y, Log2C6);
            var isnan = ix == 1024;
            var nisnan = (uint)-Unsafe.As<bool, byte>(ref isnan);
            var isz = ix == -1023;
            var nisz = (uint)-Unsafe.As<bool, byte>(ref isz);
            z = FastMultiplyAdd(z, y, Log2C5);
            ix += (int)nisz;
            z = FastMultiplyAdd(z, y, Log2C4);
            x = ix;
            z = FastMultiplyAdd(z, y, Log2C3);
            nisz &= 0x7F80_0000u;
            nisz |= nisnan;
            z = FastMultiplyAdd(z, y, Log2C2);
            x = MathI.Or(x, BitConverter.UInt32BitsToSingle(nisz));
            z = FastMultiplyAdd(z, y, Log2C1);
            var w = FastMultiplyAddNegated(y, y, y);
            z = FastMultiplyAdd(z, y, Log2C0);
#pragma warning disable S2234 // Parameters should be passed in the correct order
            z = FastMultiplyAdd(z, w, y);
#pragma warning restore S2234 // Parameters should be passed in the correct order
            x += z;
            return x;
        }
        #endregion

        #region nth Root
        // Reference: Moroz, L.; Samotyy, V.; Walczyk, C.J.; Cie ́sli  ́nski, J.L. Fast Calculation of Cube and Inverse Cube Roots Using a Magic Constant and Its Implementation on Microcontrollers. Energies 2021, 14, 1058. https://doi.org/10.3390/en14041058
        private const double CbrtC1 = 1.7523196763699390234751750023038468;
        private const double CbrtC2 = 1.2509524245066599988510127816507199;
        private const double CbrtC3 = 0.50938182920440939104272244099570921;
        private const ulong Magic = 0x553C_3014_2000_0000;

        /// <summary>
        /// Approximates the cube root of <paramref name="x"/>.<br/>
        /// </summary>
        /// <returns>
        /// Approximation of the cube root of <paramref name="x"/> computed with a method described in the article below.<br/>
        /// Moroz, L.; Samotyy, V.; Walczyk, C.J.; Cie ́sli  ́nski, J.L. Fast Calculation of Cube and Inverse Cube Roots Using a Magic Constant and Its Implementation on Microcontrollers. Energies 2021, 14, 1058. https://doi.org/10.3390/en14041058
        /// </returns>
        /// <inheritdoc cref="MathF.Cbrt(float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float Cbrt(float x)
        {
            // Reference: Moroz, L.; Samotyy, V.; Walczyk, C.J.; Cie ́sli  ́nski, J.L. Fast Calculation of Cube and Inverse Cube Roots Using a Magic Constant and Its Implementation on Microcontrollers. Energies 2021, 14, 1058. https://doi.org/10.3390/en14041058
            const double One = 1.0;
            const double OneOver3 = 0.33333333333333333333;
            var fabsx = MathF.Abs(x);
            var i = BitConverter.DoubleToUInt64Bits(fabsx);
            var absx = (double)fabsx;
            i = Magic - i / 3;
            var y = BitConverter.UInt64BitsToDouble(i);
            var q = absx * y;
            var c = y * y;
            c *= q;
            q = CbrtC2 - c * CbrtC3;
            c = CbrtC1 - c * q;
            y *= c;
            q = absx * y;
            c = y * y;
            c = One - q * c;
            c = One + OneOver3 * c;
            y *= c;
            q = absx * y;
            c = y * y;
            c = One - q * c;
            c = One + OneOver3 * c;
            y *= c;
            return (float)(y * y * x);
        }

        /// <summary>
        /// Approximates the cube root of <paramref name="x"/>.<br/>
        /// </summary>
        /// <returns>
        /// Approximation of the cube root of <paramref name="x"/> computed with a method described in the article below.<br/>
        /// Moroz, L.; Samotyy, V.; Walczyk, C.J.; Cie ́sli  ́nski, J.L. Fast Calculation of Cube and Inverse Cube Roots Using a Magic Constant and Its Implementation on Microcontrollers. Energies 2021, 14, 1058. https://doi.org/10.3390/en14041058
        /// </returns>
        /// <inheritdoc cref="MathF.Cbrt(float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastCbrt(float x)
        {
            // Reference: Moroz, L.; Samotyy, V.; Walczyk, C.J.; Cie ́sli  ́nski, J.L. Fast Calculation of Cube and Inverse Cube Roots Using a Magic Constant and Its Implementation on Microcontrollers. Energies 2021, 14, 1058. https://doi.org/10.3390/en14041058
            const double One = 1.0;
            const double OneOver3 = 0.33333333333333333333;
            var fabsx = MathF.Abs(x);
            var i = BitConverter.DoubleToUInt64Bits(fabsx);
            var absx = (double)fabsx;
            i = Magic - i / 3;
            var y = BitConverter.UInt64BitsToDouble(i);
            var q = absx * y;
            var c = y * y;
            c *= q;
            q = FastMultiplyAddNegated(c, CbrtC3, CbrtC2);
            c = FastMultiplyAddNegated(c, q, CbrtC1);
            y *= c;
            q = absx * y;
            c = y * y;
            c = FastMultiplyAddNegated(c, q, One);
            c = FastMultiplyAdd(OneOver3, c, One);
            y *= c;
            q = absx * y;
            c = y * y;
            c = FastMultiplyAddNegated(c, q, One);
            c = FastMultiplyAdd(OneOver3, c, One);
            y *= c;
            return (float)(y * y * x);
        }

        #endregion
    }
}
