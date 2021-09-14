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
        #region FastDotProduct
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
                return FastDotAdvSimd64(vector0.AsVector128(), vector1.AsVector128());
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
            return g.X + g.Z + (g.Y + g.W);
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
                return FastDotAdvSimd64(vector0, vector1);
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
        internal static float FastDotAdvSimd64(Vector128<float> vector0, Vector128<float> vector1)
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
#if NET5_0_OR_GREATER
            if (AdvSimd.IsSupported)
            {
                var v0_4s = value.AsVector128();
                v0_4s = AdvSimd.ReverseElement32(v0_4s.AsUInt64()).AsSingle();
                v0_4s = AdvSimd.ExtractVector128(v0_4s, v0_4s, 8);
                return v0_4s.AsVector4();
            }
            if (Avx.IsSupported)
            {
                return Avx.Permute(value.AsVector128(), 0x1B).AsVector4();
            }
            if (Sse2.IsSupported)
            {
                return Sse2.Shuffle(value.AsVector128().AsInt32(), 0x1B).AsSingle().AsVector4();
            }
            if (Sse.IsSupported)
            {
                var xmm0 = value.AsVector128();
                xmm0 = Sse.Shuffle(xmm0, xmm0, 0b00_01_10_11);
                return xmm0.AsVector4();
            }
#elif NETCOREAPP3_1_OR_GREATER
            if (Avx.IsSupported)
            {
                var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref value);
                xmm0 = Avx.Permute(xmm0, 0x1B);
                return Unsafe.As<Vector128<float>, Vector4>(ref xmm0);
            }
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
        #region FastDotMultipleCoeffs

        #region Stereo
        /// <summary>
        /// Returns the dot product of 4 frames x 2 channels tensor at <paramref name="head"/> and specified <paramref name="coeffs"/>.
        /// </summary>
        /// <param name="head">The location of frames to read.</param>
        /// <param name="coeffs">The coefficents to multiply.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector2 FastDotMultiple2Channels(ref Vector2 head, Vector4 coeffs)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.Arm64.IsSupported)
            {
                return FastDotMultiple2ChannelsAdvSimdArm64(ref head, coeffs);
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            if (Avx.IsSupported)
            {
                return FastDotMultiple2ChannelsAvx(ref head, coeffs);
            }
            if (Sse2.IsSupported)
            {
                return FastDotMultiple2ChannelsSse2(ref head, coeffs);
            }
#endif
            return FastDotMultiple2ChannelsStandard(ref head, coeffs);
        }
#if NET5_0_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector2 FastDotMultiple2ChannelsAdvSimdArm64(ref Vector2 head, Vector4 coeffs)
        {
            var v0_4s = coeffs.AsVector128();
            var v1_4s = Unsafe.As<Vector2, Vector128<float>>(ref head);
            var v2_4s = Unsafe.As<Vector2, Vector128<float>>(ref Unsafe.Add(ref head, 2));
            var v3_4s = AdvSimd.Arm64.UnzipEven(v1_4s, v2_4s); //Sse.Shuffle(v1_4s, v2_4s, 0x88);
            v1_4s = AdvSimd.Arm64.UnzipOdd(v1_4s, v2_4s); //Sse.Shuffle(v1_4s, v2_4s, 0xdd);
            v2_4s = AdvSimd.Multiply(v3_4s, v0_4s);
            v0_4s = AdvSimd.Multiply(v1_4s, v0_4s);
            v1_4s = AdvSimd.Arm64.ZipLow(v2_4s, v0_4s); //Sse.UnpackLow(v2_4s, v0_4s);
            v0_4s = AdvSimd.Arm64.ZipHigh(v2_4s, v0_4s); //Sse.UnpackHigh(v2_4s, v0_4s);
            v0_4s = AdvSimd.Add(v1_4s, v0_4s);
            v1_4s = AdvSimd.ExtractVector128(v0_4s.AsByte(), v0_4s.AsByte(), 8).AsSingle(); //Avx.Permute(v0_4s.AsDouble(), 1).AsSingle();
            v0_4s = AdvSimd.Add(v0_4s, v1_4s);
            return v0_4s.AsVector2();
        }
#endif
#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector2 FastDotMultiple2ChannelsSse2(ref Vector2 head, Vector4 coeffs)
        {
#if NET5_0_OR_GREATER
            var xmm0 = coeffs.AsVector128();
#else
            var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref coeffs);
#endif
            var xmm1 = Unsafe.As<Vector2, Vector128<float>>(ref head);
            var xmm2 = Unsafe.As<Vector2, Vector128<float>>(ref Unsafe.Add(ref head, 2));
            var xmm3 = Sse.Shuffle(xmm1, xmm2, 0x88);
            xmm1 = Sse.Shuffle(xmm1, xmm2, 0xdd);
            xmm2 = Sse.Multiply(xmm3, xmm0);
            xmm0 = Sse.Multiply(xmm1, xmm0);
            xmm1 = Sse.UnpackLow(xmm2, xmm0);
            xmm0 = Sse.UnpackHigh(xmm2, xmm0);
            xmm0 = Sse.Add(xmm1, xmm0);
            xmm3 = Sse2.UnpackHigh(xmm0.AsDouble(), xmm0.AsDouble()).AsSingle();
            xmm0 = Sse.Add(xmm0, xmm3);
#if NET5_0_OR_GREATER
            return xmm0.AsVector2();
#else
            return Unsafe.As<Vector128<float>, Vector2>(ref xmm0);
#endif
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector2 FastDotMultiple2ChannelsAvx(ref Vector2 head, Vector4 coeffs)
        {
#if NET5_0_OR_GREATER
            var xmm0 = coeffs.AsVector128();
#else
            var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref coeffs);
#endif
            var xmm1 = Unsafe.As<Vector2, Vector128<float>>(ref head);
            var xmm2 = Unsafe.As<Vector2, Vector128<float>>(ref Unsafe.Add(ref head, 2));
            var xmm3 = Sse.Shuffle(xmm1, xmm2, 0x88);
            xmm1 = Sse.Shuffle(xmm1, xmm2, 0xdd);
            xmm2 = Sse.Multiply(xmm3, xmm0);
            xmm0 = Sse.Multiply(xmm1, xmm0);
            xmm1 = Sse.UnpackLow(xmm2, xmm0);
            xmm0 = Sse.UnpackHigh(xmm2, xmm0);
            xmm0 = Sse.Add(xmm1, xmm0);
            xmm1 = Avx.Permute(xmm0.AsDouble(), 1).AsSingle();
            xmm0 = Sse.Add(xmm0, xmm1);
#if NET5_0_OR_GREATER
            return xmm0.AsVector2();
#else
            return Unsafe.As<Vector128<float>, Vector2>(ref xmm0);
#endif
        }
#endif
        private static Vector2 FastDotMultiple2ChannelsStandard(ref Vector2 head, Vector4 coeffs)
        {
            var v0_4s = Unsafe.As<Vector2, Vector4>(ref head);
            var v1_4s = Unsafe.As<Vector2, Vector4>(ref Unsafe.Add(ref head, 2));
            var v2_4s = new Vector4(v0_4s.X, v0_4s.Z, v1_4s.X, v1_4s.Z);    //Left
            var v3_4s = new Vector4(v0_4s.Y, v0_4s.W, v1_4s.Y, v1_4s.W);    //Right
            var s0 = FastDotProduct(v2_4s, coeffs);
            var s1 = FastDotProduct(v3_4s, coeffs);
            return new Vector2(s0, s1);
        }
        #endregion

        #region 3Channels
        /// <summary>
        /// Returns the dot product of 4 frames x 3 channels at <paramref name="head"/> and specified <paramref name="coeffs"/>.
        /// </summary>
        /// <param name="head">The location of frames to read.</param>
        /// <param name="coeffs">The coefficents to multiply.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector3 FastDotMultiple3Channels(ref Vector3 head, Vector4 coeffs)
        {
#if NET5_0_OR_GREATER
            if (AdvSimd.IsSupported)
            {
                return FastDotMultiple3ChannelsAdvSimd(ref head, coeffs);
            }
#endif
#if NETCOREAPP3_1_OR_GREATER
            //if (Ssse3.IsSupported)
            //{
            //    return FastDotMultiple3ChannelsSsse3(ref head, coeffs);
            //}
            if (Sse2.IsSupported)
            {
                return FastDotMultiple3ChannelsSse2(ref head, coeffs);
            }
#endif
            return FastDotMultiple3ChannelsStandard(ref head, coeffs);
        }

#if NET5_0_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector3 FastDotMultiple3ChannelsAdvSimd(ref Vector3 head, Vector4 coeffs)
        {
            ref var rsi = ref Unsafe.As<Vector3, Vector128<float>>(ref head);
            var v0_4s = coeffs.AsVector128();
            var v1_4s = rsi;
            var v2_4s = Unsafe.Add(ref rsi, 1);
            var v3_4s = Unsafe.Add(ref rsi, 2);
            var v4_4s = AdvSimd.DuplicateSelectedScalarToVector128(v1_4s, 3);
            v4_4s = AdvSimd.ExtractVector128(v4_4s, v4_4s, 1);
            v4_4s = AdvSimd.ExtractVector128(v4_4s, v2_4s, 3);
            v2_4s = AdvSimd.ExtractVector128(v2_4s, v3_4s, 2);
            v3_4s = AdvSimd.ExtractVector128(v3_4s, v3_4s, 1);
            v1_4s = AdvSimd.MultiplyBySelectedScalar(v1_4s, v0_4s, 0);
            v2_4s = AdvSimd.MultiplyBySelectedScalar(v2_4s, v0_4s, 2);
            v3_4s = AdvSimd.MultiplyBySelectedScalar(v3_4s, v0_4s, 3);
            v4_4s = AdvSimd.MultiplyBySelectedScalar(v4_4s, v0_4s, 1);
            v1_4s = AdvSimd.Add(v1_4s, v2_4s);
            v4_4s = AdvSimd.Add(v4_4s, v3_4s);
            v4_4s = AdvSimd.Add(v4_4s, v1_4s);
            return v4_4s.AsVector3();
        }
#endif

#if NETCOREAPP3_1_OR_GREATER
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector3 FastDotMultiple3ChannelsSse2(ref Vector3 head, Vector4 coeffs)
        {
            ref var rsi = ref Unsafe.As<Vector3, Vector128<float>>(ref head);
            var xmm1 = rsi;
            var xmm2 = Unsafe.Add(ref rsi, 1);
            var xmm3 = Unsafe.Add(ref rsi, 2);
#if NET5_0_OR_GREATER
            var xmm7 = coeffs.AsVector128();
#else
            var xmm7 = Unsafe.As<Vector4, Vector128<float>>(ref coeffs);
#endif
            var xmm0 = xmm1;
            var xmm8 = Sse.Shuffle(xmm7, xmm7, 0x00);
            xmm0 = Sse.Multiply(xmm0, xmm8);
            xmm1 = Sse.Shuffle(xmm1, xmm2, 0xc7);
            xmm1 = Sse.Shuffle(xmm1, xmm2, 0xd8);
            xmm2 = Sse.Shuffle(xmm2, xmm3, 0x4e);
            xmm3 = Sse.Shuffle(xmm3, xmm3, 0xf9);
            var xmm9 = Sse.Shuffle(xmm7, xmm7, 0x55);
            xmm1 = Sse.Multiply(xmm1, xmm9);
            var xmm10 = Sse.Shuffle(xmm7, xmm7, 0xAA);
            xmm2 = Sse.Multiply(xmm2, xmm10);
            xmm0 = Sse.Add(xmm0, xmm2);
            var xmm11 = Sse.Shuffle(xmm7, xmm7, 0xFF);
            xmm3 = Sse.Multiply(xmm3, xmm11);
            xmm1 = Sse.Add(xmm1, xmm3);
            xmm0 = Sse.Add(xmm0, xmm1);
#if NET5_0_OR_GREATER
            return xmm0.AsVector3();
#else
            return Unsafe.As<Vector128<float>, Vector3>(ref xmm0);
#endif
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector3 FastDotMultiple3ChannelsSsse3(ref Vector3 head, Vector4 coeffs)
        {
            ref var rsi = ref Unsafe.As<Vector3, Vector128<int>>(ref head);
            var xmm4 = rsi;
            var xmm5 = Unsafe.Add(ref rsi, 1);
            var xmm6 = Unsafe.Add(ref rsi, 2);
#if NET5_0_OR_GREATER
            var xmm7 = coeffs.AsVector128();
#else
            var xmm7 = Unsafe.As<Vector4, Vector128<float>>(ref coeffs);
#endif
            var xmm0 = xmm4.AsSingle();
            var xmm1 = Ssse3.AlignRight(xmm5, xmm4, 12).AsSingle();
            var xmm2 = Ssse3.AlignRight(xmm6, xmm5, 8).AsSingle();
            var xmm3 = Ssse3.AlignRight(xmm6, xmm6, 4).AsSingle();
            var xmm8 = Sse.Shuffle(xmm7, xmm7, 0x00);
            xmm0 = Sse.Multiply(xmm0, xmm8);
            var xmm9 = Sse.Shuffle(xmm7, xmm7, 0b01_01_01_01);
            xmm1 = Sse.Multiply(xmm1, xmm9);
            var xmm10 = Sse.Shuffle(xmm7, xmm7, 0b10_10_10_10);
            xmm2 = Sse.Multiply(xmm2, xmm10);
            xmm0 = Sse.Add(xmm0, xmm2);
            var xmm11 = Sse.Shuffle(xmm7, xmm7, 0xFF);
            xmm3 = Sse.Multiply(xmm3, xmm11);
            xmm1 = Sse.Add(xmm1, xmm3);
            xmm0 = Sse.Add(xmm0, xmm1);
#if NET5_0_OR_GREATER
            return xmm0.AsVector3();
#else
            return Unsafe.As<Vector128<float>, Vector3>(ref xmm0);
#endif
        }
#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector3 FastDotMultiple3ChannelsStandard(ref Vector3 head, Vector4 coeffs)
        {
            var v0_4s = head;
            var v1_4s = Unsafe.Add(ref head, 1);
            var v2_4s = Unsafe.Add(ref head, 2);
            var v3_4s = Unsafe.Add(ref head, 3);
            v0_4s *= coeffs.X;
            v1_4s *= coeffs.Y;
            v2_4s *= coeffs.Z;
            v3_4s *= coeffs.W;
            v0_4s += v2_4s;
            v1_4s += v3_4s;
            return v0_4s + v1_4s;
        }
        #endregion

        #region 4Channels
        /// <summary>
        /// Returns the dot product of 4 frames x 4 channels at <paramref name="head"/> and specified <paramref name="coeffs"/>.
        /// </summary>
        /// <param name="head">The location of frames to read.</param>
        /// <param name="coeffs">The coefficents to multiply.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 FastDotMultiple4Channels(ref Vector4 head, Vector4 coeffs)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Sse.IsSupported)
            {
                return FastDotMultiple4ChannelsSse(ref head, coeffs);
            }
#endif
            return FastDotMultiple4ChannelsStandard(ref head, coeffs);
        }
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector4 FastDotMultiple4ChannelsSse(ref Vector4 head, Vector4 coeffs)
        {
            ref var rsi = ref Unsafe.As<Vector4, Vector128<float>>(ref head);
#if NET5_0_OR_GREATER
            var xmm7 = coeffs.AsVector128();
#else
            var xmm7 = Unsafe.As<Vector4, Vector128<float>>(ref coeffs);
#endif
            var xmm4 = Sse.Shuffle(xmm7, xmm7, 0b00_00_00_00);
            var xmm0 = Sse.Multiply(xmm4, rsi);
            var xmm5 = Sse.Shuffle(xmm7, xmm7, 0b01_01_01_01);
            var xmm1 = Sse.Multiply(xmm5, Unsafe.Add(ref rsi, 1));
            var xmm6 = Sse.Shuffle(xmm7, xmm7, 0b10_10_10_10);
            xmm7 = Sse.Shuffle(xmm7, xmm7, 0b11_11_11_11);
            var xmm2 = Sse.Multiply(xmm6, Unsafe.Add(ref rsi, 2));
            xmm0 = Sse.Add(xmm0, xmm2);
            var xmm3 = Sse.Multiply(xmm7, Unsafe.Add(ref rsi, 3));
            xmm1 = Sse.Add(xmm1, xmm3);
            xmm0 = Sse.Add(xmm0, xmm1);
#if NET5_0_OR_GREATER
            return xmm0.AsVector4();
#else
            return Unsafe.As<Vector128<float>, Vector4>(ref xmm0);
#endif
        }
#endif

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static Vector4 FastDotMultiple4ChannelsStandard(ref Vector4 head, Vector4 coeffs)
        {
            var v0_4s = head * coeffs.X;
            var v2_4s = Unsafe.Add(ref head, 2) * coeffs.Z;
            var v1_4s = Unsafe.Add(ref head, 1) * coeffs.Y;
            var v3_4s = Unsafe.Add(ref head, 3) * coeffs.W;
            v0_4s += v2_4s;
            v1_4s += v3_4s;
            return v0_4s + v1_4s;
        }

        #endregion

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
