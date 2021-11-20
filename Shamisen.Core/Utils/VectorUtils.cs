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
            unchecked
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
            unchecked
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
        #region FastUnrolledDotProduct

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
            unchecked
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
        }
        #endregion
        #region FastDotMultipleChannels

        #region Stereo
        /// <summary>
        /// Returns the dot product of 4 frames x 2 channels tensor at <paramref name="head"/> and specified <paramref name="coeffs"/>.
        /// </summary>
        /// <param name="head">The location of frames to read.</param>
        /// <param name="coeffs">The coefficients to multiply.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector2 FastDotMultiple2Channels(ref Vector2 head, Vector4 coeffs)
        {
            unchecked
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
            unchecked
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
            unchecked
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
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    return FastDotMultiple4ChannelsSse(ref head, coeffs);
                }
#endif
                return FastDotMultiple4ChannelsStandard(ref head, coeffs);
            }
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
        #region Blend
        /// <summary>
        ///  Creates a new single-precision vector with elements selected between two specified single-precision source vectors based on an integral mask vector.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector<float> Blend(Vector<int> condition, Vector<float> left, Vector<float> right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (Vector<int>.Count == 4 && Sse41.IsSupported)
                {
                    return Sse41.BlendVariable(left.AsVector128(), right.AsVector128(), condition.AsVector128().AsSingle()).AsVector();
                }
                if (Vector<int>.Count == 8 && Avx.IsSupported)
                {
                    return Avx.BlendVariable(left.AsVector256(), right.AsVector256(), condition.AsVector256().AsSingle()).AsVector();
                }
#endif
#if NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER
                if (Vector<int>.Count == 4 && Sse41.IsSupported)
                {
                    var xmm0 = Unsafe.As<Vector<float>, Vector128<float>>(ref left);
                    var xmm1 = Unsafe.As<Vector<float>, Vector128<float>>(ref right);
                    var xmm2 = Unsafe.As<Vector<int>, Vector128<int>>(ref condition);
                    var xmm3 = Sse41.BlendVariable(xmm0.AsSingle(), xmm1.AsSingle(), xmm2.AsSingle());
                    return Unsafe.As<Vector128<float>, Vector<float>>(ref xmm3);
                }
                if (Vector<int>.Count == 8 && Avx.IsSupported)
                {
                    var ymm0 = Unsafe.As<Vector<float>, Vector256<float>>(ref left);
                    var ymm1 = Unsafe.As<Vector<float>, Vector256<float>>(ref right);
                    var ymm2 = Unsafe.As<Vector<int>, Vector256<int>>(ref condition);
                    var ymm3 = Avx.BlendVariable(ymm0.AsSingle(), ymm1.AsSingle(), ymm2.AsSingle());
                    return Unsafe.As<Vector256<float>, Vector<float>>(ref ymm3);
                }
#endif
                return Vector.ConditionalSelect(condition, left, right);
            }
        }
        /// <summary>
        ///  Creates a new 32-bit integer vector with elements selected between two specified 32-bit integer source vectors based on an integral mask vector.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector<int> Blend(Vector<int> condition, Vector<int> left, Vector<int> right)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (Vector<int>.Count == 4 && Sse41.IsSupported)
                {
                    return Sse41.BlendVariable(left.AsVector128().AsSingle(), right.AsVector128().AsSingle(), condition.AsVector128().AsSingle()).AsInt32().AsVector();
                }
                if (Vector<int>.Count == 8 && Avx.IsSupported)
                {
                    return Avx.BlendVariable(left.AsVector256().AsSingle(), right.AsVector256().AsSingle(), condition.AsVector256().AsSingle()).AsInt32().AsVector();
                }
#endif
#if NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER
                if (Vector<int>.Count == 4 && Sse41.IsSupported)
                {
                    var xmm0 = Unsafe.As<Vector<int>, Vector128<int>>(ref left);
                    var xmm1 = Unsafe.As<Vector<int>, Vector128<int>>(ref right);
                    var xmm2 = Unsafe.As<Vector<int>, Vector128<int>>(ref condition);
                    var xmm3 = Sse41.BlendVariable(xmm0.AsSingle(), xmm1.AsSingle(), xmm2.AsSingle()).AsInt32();
                    return Unsafe.As<Vector128<int>, Vector<int>>(ref xmm3);
                }
                if (Vector<int>.Count == 8 && Avx.IsSupported)
                {
                    var ymm0 = Unsafe.As<Vector<int>, Vector256<int>>(ref left);
                    var ymm1 = Unsafe.As<Vector<int>, Vector256<int>>(ref right);
                    var ymm2 = Unsafe.As<Vector<int>, Vector256<int>>(ref condition);
                    var ymm3 = Avx.BlendVariable(ymm0.AsSingle(), ymm1.AsSingle(), ymm2.AsSingle()).AsInt32();
                    return Unsafe.As<Vector256<int>, Vector<int>>(ref ymm3);
                }
#endif
                return Vector.ConditionalSelect(condition, left, right);
            }
        }
        #endregion
        #region Round
        /// <summary>
        /// Rounds a vector of single-precision floating-point value to the nearest integral values,
        /// and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="values">A vector of single-precision floating-point numbers to be rounded.</param>
        /// <returns>The integer <see cref="Vector{T}"/> nearest <paramref name="values"/>. If the fractional component of <paramref name="values"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a floating-point <see cref="Vector{T}"/> instead of an integral <see cref="Vector{T}"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector<float> Round(Vector<float> values)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (Vector<float>.Count == 4 && AdvSimd.IsSupported)
                {
                    return AdvSimd.RoundToNearest(values.AsVector128()).AsVector();
                }
                if (Vector<float>.Count == 8 && Avx.IsSupported)
                {
                    return Avx.RoundToNearestInteger(values.AsVector256()).AsVector();
                }
                if (Vector<float>.Count == 4 && Sse41.IsSupported)
                {
                    return Sse41.RoundToNearestInteger(values.AsVector128()).AsVector();
                }
#endif
#if NETCOREAPP3_1
                if (Vector<float>.Count == 8 && Avx.IsSupported)
                {
                    var ymm0 = Unsafe.As<Vector<float>, Vector256<float>>(ref values);
                    ymm0 = Avx.RoundToNearestInteger(ymm0);
                    return Unsafe.As<Vector256<float>, Vector<float>>(ref ymm0);
                }
                if (Vector<float>.Count == 4 && Sse41.IsSupported)
                {
                    var xmm0 = Unsafe.As<Vector<float>, Vector128<float>>(ref values);
                    xmm0 = Sse41.RoundToNearestInteger(xmm0);
                    return Unsafe.As<Vector128<float>, Vector<float>>(ref xmm0);
                }
#endif
                var v = values;
                var sign = Vector.AsVectorSingle(new Vector<int>(int.MinValue));
                var reciprocalEpsilon = new Vector<float>(16777216f);
                //round hack: if we add 16777216f and subtract 16777216f, the non-integer part is rounded to the nearest even numbers.
                var s = Vector.BitwiseAnd(sign, v);
                var a = Vector.BitwiseOr(reciprocalEpsilon, s);
                v += a;
                v -= a;
                return Vector.BitwiseOr(v, s);
            }
        }

        /// <summary>
        /// Rounds a vector of single-precision floating-point value to the nearest integral values,
        /// and rounds midpoint values to the nearest even number.<br/>
        /// This one is suitable for processing inside loops.
        /// </summary>
        /// <param name="values">A vector of single-precision floating-point numbers to be rounded.</param>
        /// <param name="sign">A broadcast vector with only sign bits set. If you pass wrong value, this function won't work as intended.</param>
        /// <param name="reciprocalEpsilon">A broadcast vector represents 16777216f. If you pass wrong value, this function won't work as intended.</param>
        /// <returns>The integer <see cref="Vector{T}"/> nearest <paramref name="values"/>. If the fractional component of <paramref name="values"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a floating-point <see cref="Vector{T}"/> instead of an integral <see cref="Vector{T}"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector<float> RoundInLoop(Vector<float> values, Vector<float> sign, Vector<float> reciprocalEpsilon)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (Vector<float>.Count == 4 && AdvSimd.IsSupported)
                {
                    return AdvSimd.RoundToNearest(values.AsVector128()).AsVector();
                }
                if (Vector<float>.Count == 8 && Avx.IsSupported)
                {
                    return Avx.RoundToNearestInteger(values.AsVector256()).AsVector();
                }
                if (Vector<float>.Count == 4 && Sse41.IsSupported)
                {
                    return Sse41.RoundToNearestInteger(values.AsVector128()).AsVector();
                }
#endif
#if NETCOREAPP3_1
                if (Vector<float>.Count == 8 && Avx.IsSupported)
                {
                    var ymm0 = Unsafe.As<Vector<float>, Vector256<float>>(ref values);
                    ymm0 = Avx.RoundToNearestInteger(ymm0);
                    return Unsafe.As<Vector256<float>, Vector<float>>(ref ymm0);
                }
                if (Vector<float>.Count == 4 && Sse41.IsSupported)
                {
                    var xmm0 = Unsafe.As<Vector<float>, Vector128<float>>(ref values);
                    xmm0 = Sse41.RoundToNearestInteger(xmm0);
                    return Unsafe.As<Vector128<float>, Vector<float>>(ref xmm0);
                }
#endif
                var v = values;
                //round hack: if we add 16777216f and subtract 16777216f, the non-integer part is rounded to the nearest even numbers.
                var s = Vector.BitwiseAnd(sign, v);
                var a = Vector.BitwiseOr(reciprocalEpsilon, s);
                v += a;
                v -= a;
                return Vector.BitwiseOr(v, s);
            }
        }

        /// <summary>
        /// Rounds a vector of single-precision floating-point value to the nearest integral values,
        /// and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="values">A vector of single-precision floating-point numbers to be rounded.</param>
        /// <returns>The integer <see cref="Vector4"/> nearest <paramref name="values"/>. If the fractional component of <paramref name="values"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a floating-point <see cref="Vector4"/> instead of an integral <see cref="Vector{T}"/>.</returns>
        public static Vector4 Round(Vector4 values)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.RoundToNearest(values.AsVector128()).AsVector4();
                }
                if (Sse41.IsSupported)
                {
                    return Sse41.RoundToNearestInteger(values.AsVector128()).AsVector4();
                }
#endif
#if NETCOREAPP3_1
                if (Sse41.IsSupported)
                {
                    var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref values);
                    xmm0 = Sse41.RoundToNearestInteger(xmm0);
                    return Unsafe.As<Vector128<float>, Vector4>(ref xmm0);
                }
#endif
                var s0 = values.X;
                var s1 = values.Y;
                var s2 = values.Z;
                var s3 = values.W;
                s0 = FastMath.Round(s0);
                s1 = FastMath.Round(s1);
                s2 = FastMath.Round(s2);
                s3 = FastMath.Round(s3);
                return new(s0, s1, s2, s3);
            }
        }

        /// <summary>
        /// Rounds a vector of single-precision floating-point value to the nearest integral values,
        /// and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="values">A vector of single-precision floating-point numbers to be rounded.</param>
        /// <returns>The integer <see cref="Vector2"/> nearest <paramref name="values"/>. If the fractional component of <paramref name="values"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a floating-point <see cref="Vector2"/> instead of an integral <see cref="Vector{T}"/>.</returns>
        public static Vector2 Round(Vector2 values)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.RoundToNearest(values.AsVector128()).AsVector2();
                }
                if (Sse41.IsSupported)
                {
                    return Sse41.RoundToNearestInteger(values.AsVector128()).AsVector2();
                }
#endif
#if NETCOREAPP3_1
                if (Sse41.IsSupported)
                {
                    var t = new Vector4(values, 0f, 0f);
                    var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref t);
                    xmm0 = Sse41.RoundToNearestInteger(xmm0);
                    return Unsafe.As<Vector128<float>, Vector2>(ref xmm0);
                }
#endif
                var s0 = values.X;
                var s1 = values.Y;
                s0 = FastMath.Round(s0);
                s1 = FastMath.Round(s1);
                return new(s0, s1);
            }
        }
        /// <summary>
        /// Rounds a vector of single-precision floating-point value to the nearest integral values,
        /// and rounds midpoint values to the nearest even number.
        /// </summary>
        /// <param name="values">A vector of single-precision floating-point numbers to be rounded.</param>
        /// <returns>The integer <see cref="Vector3"/> nearest <paramref name="values"/>. If the fractional component of <paramref name="values"/> is halfway between two
        /// integers, one of which is even and the other odd, then the even number is returned.
        /// Note that this method returns a floating-point <see cref="Vector3"/> instead of an integral <see cref="Vector{T}"/>.</returns>
        public static Vector3 Round(Vector3 values)
        {
            unchecked
            {
#if NET5_0_OR_GREATER
                if (AdvSimd.IsSupported)
                {
                    return AdvSimd.RoundToNearest(values.AsVector128()).AsVector3();
                }
                if (Sse41.IsSupported)
                {
                    return Sse41.RoundToNearestInteger(values.AsVector128()).AsVector3();
                }
#endif
#if NETCOREAPP3_1
                if (Sse41.IsSupported)
                {
                    var t = new Vector4(values, 0f);
                    var xmm0 = Unsafe.As<Vector4, Vector128<float>>(ref t);
                    xmm0 = Sse41.RoundToNearestInteger(xmm0);
                    return Unsafe.As<Vector128<float>, Vector3>(ref xmm0);
                }
#endif
                var s0 = values.X;
                var s1 = values.Y;
                var s2 = values.Z;
                s0 = FastMath.Round(s0);
                s1 = FastMath.Round(s1);
                s2 = FastMath.Round(s2);
                return new(s0, s1, s2);
            }
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
        #region CreateVector4
        /// <inheritdoc cref="Vector4(float, float, float, float)"/>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 CreateVector4(float x, float y, float z, float w)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Sse.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    var xmm2 = Vector128.CreateScalarUnsafe(z);
                    var xmm3 = Vector128.CreateScalarUnsafe(w);
                    xmm2 = Sse.UnpackLow(xmm2, xmm3);
                    xmm0 = Sse.UnpackLow(xmm0, xmm1);
                    xmm0 = Sse.MoveLowToHigh(xmm0, xmm2);
                    return xmm0.AsVector4();
                }
#endif
                return new Vector4(x, y, z, w);
            }
        }
        /// <summary>
        /// Creates a vector whose elements have the specified values.
        /// </summary>
        /// <param name="x">The value to assign to the <see cref="Vector4.X"/> field.</param>
        /// <param name="y">The value to assign to the <see cref="Vector4.Y"/> field.</param>
        /// <param name="z">The value to assign to the <see cref="Vector4.Z"/> field.</param>
        /// <param name="w">The value to assign to the <see cref="Vector4.W"/> field.</param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 ConvertAndCreateVector4(int x, int y, int z, int w)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Sse2.IsSupported)
                {
                    var xmm0 = Vector128.CreateScalarUnsafe(x);
                    var xmm1 = Vector128.CreateScalarUnsafe(y);
                    var xmm2 = Vector128.CreateScalarUnsafe(z);
                    var xmm3 = Vector128.CreateScalarUnsafe(w);
                    xmm2 = Sse2.UnpackLow(xmm2, xmm3);
                    xmm0 = Sse2.UnpackLow(xmm0, xmm1);
                    xmm0 = Sse2.UnpackLow(xmm0.AsInt64(), xmm2.AsInt64()).AsInt32();
                    return Sse2.ConvertToVector128Single(xmm0.AsInt32()).AsVector4();
                }
#endif
                return new Vector4(x, y, z, w);
            }
        }
        #endregion
        #region AsVector128
#if NETCOREAPP3_1
        /// <summary>
        /// Reinterprets a <see cref="Vector4"/> as a new <see cref="Vector128{T}"/>.
        /// </summary>
        /// <param name="value">The vector to reinterpret.</param>
        /// <returns><paramref name="value"/> reinterpreted as a new <see cref="Vector128{T}"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector128<float> AsVector128(this Vector4 value) => Unsafe.As<Vector4, Vector128<float>>(ref value);
#endif
        #endregion
        #region AsVector4
#if NETCOREAPP3_1
        /// <summary>
        /// Reinterprets a <see cref="Vector128{T}"/> as a new <see cref="Vector4"/>.
        /// </summary>
        /// <param name="value">The vector to reinterpret.</param>
        /// <returns><paramref name="value"/> reinterpreted as a new <see cref="Vector4"/>.</returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static Vector4 AsVector4(this Vector128<float> value) => Unsafe.As<Vector128<float>, Vector4>(ref value);
#endif
        #endregion
    }
}
