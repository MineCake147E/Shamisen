using System.Numerics;
using System.Runtime.CompilerServices;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif


namespace Shamisen.Utils
{
    /// <summary>
    /// Contains some utility functions for manipulating <see cref="Vector"/> values.
    /// </summary>
    public static class VectorUtils
    {
        #region FastDot
        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector0">The value to multiply.</param>
        /// <param name="vector1">The value to multiply.</param>
        /// <returns>The dot product of two vectors.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastDotProduct(Vector4 vector0, Vector4 vector1)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                return FastDorAdvSimd64(vector0.AsVector128(), vector1.AsVector128());
            }
            if (Avx.IsSupported)
            {
                return FastDotAvx(vector0.AsVector128(), vector1.AsVector128());
            }
            if (Sse.IsSupported)
            {
                return FastDotSse(vector0.AsVector128(), vector1.AsVector128());
            }
#endif
#if NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER
            if (Avx.IsSupported)
            {
                return FastDotAvx(Unsafe.As<Vector4, Vector128<float>>(ref vector0), Unsafe.As<Vector4, Vector128<float>>(ref vector1));
            }
            if (Sse.IsSupported)
            {
                return FastDotSse(Unsafe.As<Vector4, Vector128<float>>(ref vector0), Unsafe.As<Vector4, Vector128<float>>(ref vector1));
            }
#endif
            var g = vector0 * vector1;
            return g.X + g.Y + (g.Z + g.W);
        }
#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector0">The value to multiply.</param>
        /// <param name="vector1">The value to multiply.</param>
        /// <returns>The dot product of two vectors.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static float FastDotProduct(Vector128<float> vector0, Vector128<float> vector1)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                return FastDorAdvSimd64(vector0, vector1);
            }
#endif
            if (Avx.IsSupported)
            {
                return FastDotAvx(vector0, vector1);
            }
            if (Sse.IsSupported)
            {
                return FastDotSse(vector0, vector1);
            }
            return FastDotProduct(Unsafe.As<Vector128<float>, Vector4>(ref vector0), Unsafe.As<Vector128<float>, Vector4>(ref vector1));
        }
#endif
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float FastDotSse(Vector128<float> vector0, Vector128<float> vector1)
        {
            var xmm0 = vector0;
            var xmm1 = vector1;
            xmm0 = Sse.Multiply(xmm0, xmm1);
            xmm1 = Sse.Shuffle(xmm0, xmm0, 0b11_10_11_10);
            xmm0 = Sse.Add(xmm0, xmm1);
            xmm1 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
            xmm0 = Sse.AddScalar(xmm0, xmm1);
            return xmm0.GetElement(0);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float FastDotSse2(Vector128<float> vector0, Vector128<float> vector1)
        {
            var xmm0 = vector0;
            var xmm1 = vector1;
            xmm0 = Sse.Multiply(xmm0, xmm1);
            xmm1 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm0.AsDouble()).AsSingle();
            xmm0 = Sse.Add(xmm0, xmm1);
            xmm1 = Sse.Shuffle(xmm0, xmm0, 0b01_01_01_01);
            xmm0 = Sse.AddScalar(xmm0, xmm1);
            return xmm0.GetElement(0);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float FastDotSse41(Vector128<float> vector0, Vector128<float> vector1)
        {
            var xmm0 = vector0;
            var xmm1 = vector1;
            xmm0 = Sse.Multiply(xmm0, xmm1);
            xmm1 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm0.AsDouble()).AsSingle();
            xmm0 = Sse.Add(xmm0, xmm1);
            xmm1 = Sse3.MoveHighAndDuplicate(xmm0);
            xmm0 = Sse.AddScalar(xmm0, xmm1);
            return xmm0.GetElement(0);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float FastDotAvx(Vector128<float> vector0, Vector128<float> vector1)
        {
            var xmm0 = vector0;
            var xmm1 = vector1;
            xmm0 = Sse.Multiply(xmm0, xmm1);
            xmm1 = Avx.Permute(xmm0.AsDouble(), 1).AsSingle();
            xmm0 = Sse.Add(xmm0, xmm1);
            xmm1 = Sse3.MoveHighAndDuplicate(xmm0);
            xmm0 = Sse.AddScalar(xmm0, xmm1);
            return xmm0.GetElement(0);
        }
#endif
#if NET5_0_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static float FastDorAdvSimd64(Vector128<float> vector0, Vector128<float> vector1)
        {
            var v0 = vector0;
            var v1 = vector1;
            v0 = AdvSimd.Multiply(v0, v1);
            var v2 = AdvSimd.Arm64.AddPairwise(v0, v0);
            var v3 = AdvSimd.AddPairwise(v2.GetLower(), v2.GetUpper());
            return v3.GetElement(0);
        }
#endif

        #endregion
        #region ReverseElements
        /// <summary>
        /// Returns a new <see cref="Vector4"/> value with reversed elements of <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to reverse elements.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 ReverseElements(Vector4 value)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Sse.IsSupported)
            {
                var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref value);
                xmm0 = Sse.Shuffle(xmm0, xmm0, 0b00_01_10_11);
                return Unsafe.As<Vector128<float>, Vector4>(ref xmm0);
            }
#endif
            return new(value.W, value.Z, value.Y, value.X);
        }
        #endregion

        #region AddAsInt32
        /// <summary>
        /// Adds two <see cref="Vector4"/> values as if values are <see cref="int"/>.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>The added value.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 AddAsInt32(Vector4 left, Vector4 right)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.IsSupported)
            {
                return AdvSimd.Add(left.AsVector128().AsInt32(), right.AsVector128().AsInt32()).AsSingle().AsVector4();
            }
            if (Sse2.IsSupported)
            {
                return Sse2.Add(left.AsVector128(), right.AsVector128()).AsVector4();
            }
#elif NETCOREAPP3_1_OR_GREATER
            if (Sse2.IsSupported)
            {
                var xmm0 = Unsafe.As<Vector4, Vector128<int>>(ref left);
                var xmm1 = Unsafe.As<Vector4, Vector128<int>>(ref right);
                var xmm3 = Sse2.Add(xmm0, xmm1);
                return Unsafe.As<Vector128<int>, Vector4>(ref xmm3);
            }
#endif
#if NETCOREAPP3_1_OR_GREATER

            return new(
                BitConverter.SingleToInt32Bits(left.X) + BitConverter.SingleToInt32Bits(right.X),
                BitConverter.SingleToInt32Bits(left.Y) + BitConverter.SingleToInt32Bits(right.Y),
                BitConverter.SingleToInt32Bits(left.Z) + BitConverter.SingleToInt32Bits(right.Z),
                BitConverter.SingleToInt32Bits(left.W) + BitConverter.SingleToInt32Bits(right.W));
#else
            var l = Unsafe.As<Vector4, (int X, int Y, int Z, int W)>(ref left);
            var r = Unsafe.As<Vector4, (int X, int Y, int Z, int W)>(ref right);
            var s = (l.X + r.X, l.Y + r.Y, l.Z + r.Z, l.W + r.W);
            return Unsafe.As<(int X, int Y, int Z, int W), Vector4>(ref s);
#endif
        }
        #endregion
    }
}
