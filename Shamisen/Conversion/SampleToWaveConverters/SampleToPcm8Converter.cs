using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using Shamisen.Utils.Intrinsics;
using Shamisen.Filters;

#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif
namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to 8-bit PCM.
    /// </summary>
    /// <seealso cref="SampleToWaveConverterBase" />
    public sealed class SampleToPcm8Converter : SampleToWaveConverterBase
    {
        private const float Multiplier = 128.0f;
        private Memory<sbyte> dsmLastOutput;
        private Memory<float> dsmAccumulator;
        private int dsmChannelPointer = 0;
        private Memory<float> readBuffer;
        private const int BufferMax = 1024; //The bufferMax is fixed to 1024 regardless of the destination type because the buffer is float.
        private const float Minimum = -128.0f;
        private const float Maximum = 127.0f;

        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm8Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="accuracyNeeded">Turns on <see cref="AccuracyMode"/> when <c>true</c>.</param>
        public SampleToPcm8Converter(IReadableAudioSource<float, SampleFormat> source, bool accuracyNeeded = true)
            : base(source, new WaveFormat(source.Format.SampleRate, 8, source.Format.Channels, AudioEncoding.LinearPcm))
        {
            if (accuracyNeeded)
            {
                dsmAccumulator = new float[source.Format.Channels];
                dsmLastOutput = new sbyte[source.Format.Channels];
            }
            AccuracyMode = accuracyNeeded;
            readBuffer = new float[ActualBufferMax];
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SampleToPcm8Converter"/> does the 8-bit Delta-Sigma modulation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the accuracy mode is turned on; otherwise, <c>false</c>.
        /// </value>
        public bool AccuracyMode { get; }

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => sizeof(byte);

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<byte> buffer)
        {
            var cursor = buffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= readBuffer.Length ? readBuffer : readBuffer.Slice(0, cursor.Length);
                var rr = Source.Read(reader.Span);
                if (rr.IsEndOfStream && buffer.Length == cursor.Length) return rr;
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                var u = rr.Length;
                var wrote = reader.Span.Slice(0, u);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                if (AccuracyMode)
                {
                    ProcessAccurate(wrote, dest);
                }
                else
                {
                    ProcessNormal(wrote, dest);
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        #region Accurate

        private void ProcessAccurate(Span<float> wrote, Span<byte> dest)
        {
            var channels = Format.Channels;
            if (dsmAccumulator.Length < channels || dsmLastOutput.Length < channels)
                throw new InvalidOperationException("Channels must be smaller than or equals to dsmAccumulator's length!");
            switch (channels)
            {
                case 1:
                    ProcessAccurateMonaural(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span);
                    break;
                case 2:
                    ProcessAccurateStereoStandard(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span);
                    break;
                case 3:
                    ProcessAccurate3ChannelsStandard(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span);
                    break;
                case 4:
                    ProcessAccurate4ChannelsStandard(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span);
                    break;
                default:
                    ProcessAccurateOrdinal(wrote, dest);
                    break;
            }
        }

        private static void ProcessAccurateMonaural(Span<float> wrote, Span<byte> dest, Span<float> accSpan, Span<sbyte> loSpan)
        {
            var dsmAcc = MemoryMarshal.GetReference(accSpan);
            var dsmPrev = (float)MemoryMarshal.GetReference(loSpan);
            ref var rWrote = ref MemoryMarshal.GetReference(wrote);
            ref var rDest = ref MemoryMarshal.GetReference(dest);
            nint nLength = dest.Length;
            var mul = new Vector4(Multiplier);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul.X * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                dsmAcc = Clamp(dsmAcc);
                dsmPrev = FastMath.Round(dsmAcc);
                var v = (sbyte)dsmPrev;
                Unsafe.Add(ref rDest, i) = (byte)(v ^ 128);
            }
            MemoryMarshal.GetReference(accSpan) = dsmAcc;
            MemoryMarshal.GetReference(loSpan) = (sbyte)dsmPrev;
        }

        private static void ProcessAccurateStereoStandard(Span<float> wrote, Span<byte> dest, Span<float> accSpan, Span<sbyte> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<sbyte, (sbyte x, sbyte y)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector2(dsmLastOut.x, dsmLastOut.y);
            ref var rWrote = ref Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<byte, (byte x, byte y)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 2;
            var min = new Vector2(Minimum);
            var max = new Vector2(Maximum);
            var mul = new Vector2(Multiplier);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = VectorUtils.Round(dsmAcc);
                dsmPrev = Vector2.Clamp(v, min, max);
                Unsafe.Add(ref rDest, i) = ((byte)(dsmPrev.X + 128), (byte)(dsmPrev.Y + 128));
            }
            Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<sbyte, (sbyte x, sbyte y)>(ref MemoryMarshal.GetReference(loSpan)) = ((sbyte)dsmPrev.X, (sbyte)dsmPrev.Y);
        }

        private static void ProcessAccurate3ChannelsStandard(Span<float> wrote, Span<byte> dest, Span<float> accSpan, Span<sbyte> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<sbyte, (sbyte x, sbyte y, sbyte z)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector3(dsmLastOut.x, dsmLastOut.y, dsmLastOut.z);
            ref var rWrote = ref Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<byte, (byte x, byte y, byte z)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 3;
            var min = new Vector3(Minimum);
            var max = new Vector3(Maximum);
            var mul = new Vector3(Multiplier);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = Vector3.Clamp(dsmAcc, min, max);
                dsmPrev = VectorUtils.Round(v);
                Unsafe.Add(ref rDest, i) = ((byte)(dsmPrev.X + 128), (byte)(dsmPrev.Y + 128), (byte)(dsmPrev.Z + 128));
            }
            Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<sbyte, (sbyte x, sbyte y, sbyte z)>(ref MemoryMarshal.GetReference(loSpan)) = ((sbyte)dsmPrev.X, (sbyte)dsmPrev.Y, (sbyte)dsmPrev.Z);
        }

        private static void ProcessAccurate4ChannelsStandard(Span<float> wrote, Span<byte> dest, Span<float> accSpan, Span<sbyte> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<sbyte, (sbyte x, sbyte y, sbyte z, sbyte w)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector4(dsmLastOut.x, dsmLastOut.y, dsmLastOut.z, dsmLastOut.w);
            ref var rWrote = ref Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<byte, (byte x, byte y, byte z, byte w)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 4;
            var min = new Vector4(Minimum);
            var max = new Vector4(Maximum);
            var mul = new Vector4(Multiplier);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = Vector4.Clamp(dsmAcc, min, max);
                dsmPrev = VectorUtils.Round(v);
                Unsafe.Add(ref rDest, i) = ((byte)(dsmPrev.X + 128), (byte)(dsmPrev.Y + 128), (byte)(dsmPrev.Z + 128), (byte)(dsmPrev.W + 128));
            }
            Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<sbyte, (sbyte x, sbyte y, sbyte z, sbyte w)>(ref MemoryMarshal.GetReference(loSpan)) = ((sbyte)dsmPrev.X, (sbyte)dsmPrev.Y, (sbyte)dsmPrev.Z, (sbyte)dsmPrev.W);
        }
        private static nint ProcessAccurateDirectGenericStandard(Span<float> wrote, Span<byte> dest, Span<float> dsmAcc, Span<sbyte> dsmLast, int dsmChannelPointer)
        {
            ref var acc = ref MemoryMarshal.GetReference(dsmAcc);
            ref var dlo = ref MemoryMarshal.GetReference(dsmLast);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            var channels = dsmAcc.Length;
            nint ch = dsmChannelPointer % channels;
            nint i = 0, length = wrote.Length;
            const float Mul = Multiplier;
            for (; i < length; i++)
            {
                var a = Unsafe.Add(ref acc, ch);
                var diff = Mul * Unsafe.Add(ref src, i) - Unsafe.Add(ref dlo, ch);
                a += diff;
                var v = ConvertScaled(a);
                Unsafe.Add(ref dlo, ch) = v;
                Unsafe.Add(ref acc, ch) = a;
                var h = ++ch < channels;
                var hh = -Unsafe.As<bool, byte>(ref h);
                Unsafe.Add(ref dst, i) = (byte)(v + 128);
                ch &= hh;
            }
            return ch;
        }
        private void ProcessAccurateOrdinal(Span<float> wrote, Span<byte> dest) => dsmChannelPointer = (int)ProcessAccurateDirectGenericStandard(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span, dsmChannelPointer);

        #endregion

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void ProcessNormal(Span<float> wrote, Span<byte> dest)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (Avx2.IsSupported)
                {
                    ProcessNormalAvx2A(wrote, dest);
                }
#endif
                ProcessNormalStandard(wrote, dest);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> wrote, Span<byte> dest)
        {
            var mul = new Vector<uint>(0x0380_0000u);
            var sign = Vector.AsVectorSingle(new Vector<int>(int.MinValue));
            var reciprocalEpsilon = new Vector<float>(16777216f);
            var min = new Vector<float>(-1.0f);
            var max = new Vector<float>(127.0f / 128.0f);
            var mask = new Vector<byte>(0x80);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - Vector<float>.Count * 4 + 1;
            for (; i < olen; i += Vector<float>.Count * 4)
            {
                var v0_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 0 * Vector<float>.Count));
                var v1_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 1 * Vector<float>.Count));
                var v2_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 2 * Vector<float>.Count));
                var v3_ns = Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + 3 * Vector<float>.Count));
                v0_ns = Vector.Min(max, v0_ns);
                v1_ns = Vector.Min(max, v1_ns);
                v2_ns = Vector.Min(max, v2_ns);
                v3_ns = Vector.Min(max, v3_ns);
                v0_ns = Vector.Max(min, v0_ns);
                v1_ns = Vector.Max(min, v1_ns);
                v2_ns = Vector.Max(min, v2_ns);
                v3_ns = Vector.Max(min, v3_ns);
                v0_ns = Vector.AsVectorSingle(Vector.AsVectorUInt32(v0_ns) + mul);
                v1_ns = Vector.AsVectorSingle(Vector.AsVectorUInt32(v1_ns) + mul);
                v2_ns = Vector.AsVectorSingle(Vector.AsVectorUInt32(v2_ns) + mul);
                v3_ns = Vector.AsVectorSingle(Vector.AsVectorUInt32(v3_ns) + mul);
                v0_ns = VectorUtils.RoundInLoop(v0_ns, sign, reciprocalEpsilon);
                v1_ns = VectorUtils.RoundInLoop(v1_ns, sign, reciprocalEpsilon);
                v2_ns = VectorUtils.RoundInLoop(v2_ns, sign, reciprocalEpsilon);
                v3_ns = VectorUtils.RoundInLoop(v3_ns, sign, reciprocalEpsilon);
                v0_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v0_ns));
                v1_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v1_ns));
                v2_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v2_ns));
                v3_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v3_ns));
                var v0_nh = Vector.AsVectorUInt16(Vector.Narrow(Vector.AsVectorInt32(v0_ns), Vector.AsVectorInt32(v1_ns)));
                var v2_nh = Vector.AsVectorUInt16(Vector.Narrow(Vector.AsVectorInt32(v2_ns), Vector.AsVectorInt32(v3_ns)));
                var v0_nb = Vector.Narrow(v0_nh, v2_nh);
                v0_nb = Vector.Xor(v0_nb, mask);
                Unsafe.As<byte, Vector<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector<byte>.Count)) = v0_nb;
            }
            for (; i < length; i++)
            {
                var v = Unsafe.Add(ref src, i);
                Unsafe.Add(ref dst, i) = (byte)((byte)Math.Round(Math.Min(sbyte.MaxValue, Math.Max(v * 128, sbyte.MinValue))) + 128);
            }
        }

        #region X86
#if NETCOREAPP3_1_OR_GREATER

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalAvx2A(Span<float> wrote, Span<byte> dest)
        {
            var sign = Vector256.Create((byte)128).AsUInt32();
            var expOffset = Vector256.Create(0x0380_0000u);
            var min = Vector256.Create(-1.0f);
            var max = Vector256.Create(127.0f / 128.0f);
            var perm = Vector256.Create(0, 4, 1, 5, 2, 6, 3, 7);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(dest.Length, wrote.Length);
            var olen = length - 8 * 8 + 1;
            for (; i < olen; i += 8 * 8)
            {
                var ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8))).AsUInt32();
                var ymm1 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 1 * 8))).AsUInt32();
                var ymm2 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 2 * 8))).AsUInt32();
                var ymm3 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 3 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Max(min, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Max(min, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Max(min, ymm3.AsSingle()).AsUInt32();
                //Floating-point multiplication is replaced with Integer addition.
                ymm0 = Avx2.Add(expOffset, ymm0).AsUInt32();
                ymm1 = Avx2.Add(expOffset, ymm1).AsUInt32();
                ymm2 = Avx2.Add(expOffset, ymm2).AsUInt32();
                ymm3 = Avx2.Add(expOffset, ymm3).AsUInt32();
                ymm0 = Avx.ConvertToVector256Int32(ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.ConvertToVector256Int32(ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.ConvertToVector256Int32(ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.ConvertToVector256Int32(ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                ymm2 = Avx2.PackSignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt16(), ymm2.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                ymm0 = Avx2.Xor(sign, ymm0);
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<byte>.Count)) = ymm0.AsByte();
                ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 4 * 8))).AsUInt32();
                ymm1 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 5 * 8))).AsUInt32();
                ymm2 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 6 * 8))).AsUInt32();
                ymm3 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 7 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.Max(min, ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.Max(min, ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.Max(min, ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx2.Add(expOffset, ymm0).AsUInt32();
                ymm1 = Avx2.Add(expOffset, ymm1).AsUInt32();
                ymm2 = Avx2.Add(expOffset, ymm2).AsUInt32();
                ymm3 = Avx2.Add(expOffset, ymm3).AsUInt32();
                ymm0 = Avx.ConvertToVector256Int32(ymm0.AsSingle()).AsUInt32();
                ymm1 = Avx.ConvertToVector256Int32(ymm1.AsSingle()).AsUInt32();
                ymm2 = Avx.ConvertToVector256Int32(ymm2.AsSingle()).AsUInt32();
                ymm3 = Avx.ConvertToVector256Int32(ymm3.AsSingle()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm1.AsInt32()).AsUInt32();
                ymm2 = Avx2.PackSignedSaturate(ymm2.AsInt32(), ymm3.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt16(), ymm2.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                ymm0 = Avx2.Xor(sign, ymm0);
                Unsafe.As<byte, Vector256<byte>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<byte>.Count)) = ymm0.AsByte();
            }
            olen = length - 8 + 1;
            for (; i < olen; i += 8)
            {
                var ymm0 = Avx.Min(max, Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref src, i + 0 * 8))).AsUInt32();
                ymm0 = Avx.Max(min, ymm0.AsSingle()).AsUInt32();
                ymm0 = Avx2.Add(expOffset, ymm0);
                ymm0 = Avx.ConvertToVector256Int32(ymm0.AsSingle()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt32(), ymm0.AsInt32()).AsUInt32();
                ymm0 = Avx2.PackSignedSaturate(ymm0.AsInt16(), ymm0.AsInt16()).AsUInt32();
                ymm0 = Avx2.PermuteVar8x32(ymm0.AsInt32(), perm).AsUInt32();
                ymm0 = Avx2.Xor(sign, ymm0);
                Unsafe.As<byte, long>(ref Unsafe.Add(ref dst, i)) = ymm0.AsInt64().GetElement(0);
            }
            for (; i < length; i++)
            {
                var xmm0 = Sse.MinScalar(max.GetLower(), Vector128.CreateScalarUnsafe(Unsafe.Add(ref src, i))).AsUInt32();
                xmm0 = Sse.MaxScalar(min.GetLower(), xmm0.AsSingle()).AsUInt32();
                xmm0 = Sse2.Add(expOffset.GetLower(), xmm0);
                xmm0 = Sse2.ConvertToVector128Int32(xmm0.AsSingle()).AsUInt32();
                xmm0 = Sse2.PackSignedSaturate(xmm0.AsInt32(), xmm0.AsInt32()).AsUInt32();
                xmm0 = Sse2.PackSignedSaturate(xmm0.AsInt16(), xmm0.AsInt16()).AsUInt32();
                xmm0 = Sse2.Xor(sign.GetLower(), xmm0);
                Unsafe.Add(ref dst, i) = xmm0.AsByte().GetElement(0);
            }
        }
#endif
        #endregion
        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -1 and 1, and then converts to <see cref="byte"/>.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static sbyte Convert(float srcval)
        {
            srcval *= 128.0f;
            return ConvertScaled(srcval);
        }

        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -32768.0f and 32767.0f, and then converts to <see cref="byte"/>.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static sbyte ConvertScaled(float srcval)
        {
            srcval = Clamp(srcval);
            return ConvertScaledClamped(srcval);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static float Clamp(float srcval)
        {
            srcval = FastMath.Max(srcval, -128.0f);
            srcval = FastMath.Min(srcval, 127.0f);
            return srcval;
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static sbyte ConvertScaledClamped(float srcval)
        {
            srcval = FastMath.Round(srcval);
            return (sbyte)srcval;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Source.Dispose();
                }
                dsmLastOutput = default;
                dsmAccumulator = default;
                readBuffer = default;
            }
            disposedValue = true;
        }
    }
}
