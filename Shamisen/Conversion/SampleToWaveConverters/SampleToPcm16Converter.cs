
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using System.Text;

using Shamisen.Optimization;

using System.Runtime.CompilerServices;
using System.Numerics;

using Shamisen.Utils;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif

namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to 16-bit PCM.
    /// </summary>
    /// <seealso cref="SampleToWaveConverterBase" />
    public sealed partial class SampleToPcm16Converter : SampleToWaveConverterBase
    {
        private const float Multiplier = 32768.0f;
        private const float MultiplierInv = 1.0f / Multiplier;
        private const int ActualBytesPerSample = sizeof(short);
        private const int BufferMax = 4096;
        private int ActualBufferMax => BufferMax * Source.Format.Channels;

        private Memory<short> dsmLastOutput;
        private Memory<float> dsmAccumulator;
        private int dsmChannelPointer = 0;
        private Memory<float> readBuffer;

        private readonly bool enableIntrinsics;
        private readonly X86Intrinsics enabledX86Intrinsics;
        private readonly ArmIntrinsics enabledArmIntrinsics;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm16Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="doDeltaSigmaModulation">Turns on <see cref="DoDeltaSigmaModulation"/> when <c>true</c>.</param>
        /// <param name="endianness">The destination endianness.</param>
        public SampleToPcm16Converter(IReadableAudioSource<float, SampleFormat> source, bool doDeltaSigmaModulation = true, Endianness endianness = Endianness.Little)
            : this(source, true, IntrinsicsUtils.X86Intrinsics, IntrinsicsUtils.ArmIntrinsics, doDeltaSigmaModulation, endianness)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm16Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="enableIntrinsics"></param>
        /// <param name="enabledX86Intrinsics"></param>
        /// <param name="enabledArmIntrinsics"></param>
        /// <param name="doDeltaSigmaModulation">Turns on <see cref="DoDeltaSigmaModulation"/> when <c>true</c>.</param>
        /// <param name="endianness">The destination endianness.</param>
        internal SampleToPcm16Converter(IReadableAudioSource<float, SampleFormat> source, bool enableIntrinsics, X86Intrinsics enabledX86Intrinsics, ArmIntrinsics enabledArmIntrinsics, bool doDeltaSigmaModulation = true, Endianness endianness = Endianness.Little)
             : base(source, new WaveFormat(source.Format.SampleRate, 16, source.Format.Channels, AudioEncoding.LinearPcm))
        {
            if (doDeltaSigmaModulation)
            {
                dsmAccumulator = new float[source.Format.Channels];
                dsmLastOutput = new short[source.Format.Channels];
            }
            DoDeltaSigmaModulation = doDeltaSigmaModulation;
            Endianness = endianness;
            readBuffer = new float[ActualBufferMax];
            this.enableIntrinsics = enableIntrinsics;
            this.enabledX86Intrinsics = enabledX86Intrinsics;
            this.enabledArmIntrinsics = enabledArmIntrinsics;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SampleToPcm16Converter"/> does the 16-bit Delta-Sigma modulation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the accuracy mode is turned on; otherwise, <c>false</c>.
        /// </value>
        public bool DoDeltaSigmaModulation { get; }

        /// <summary>
        /// Gets the endianness.
        /// </summary>
        /// <value>
        /// The endianness.
        /// </value>
        public Endianness Endianness { get; }

        private bool IsEndiannessConversionRequired => Endianness != EndiannessExtensions.EnvironmentEndianness;

        /// <summary>
        /// Gets the bytes consumed per sample.
        /// </summary>
        /// <value>
        /// The bytes consumed per sample.
        /// </value>
        protected override int BytesPerSample => ActualBytesPerSample;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>
        /// The length of the data written.
        /// </returns>
        public override ReadResult Read(Span<byte> buffer)
        {
            var channels = Format.Channels;
            var outBuffer = MemoryMarshal.Cast<byte, short>(buffer).SliceAlign(channels);
            var cursor = outBuffer;
            while (cursor.Length > 0)
            {
                var reader = cursor.Length >= readBuffer.Length ? readBuffer : readBuffer.Slice(0, cursor.Length);
                var rr = Source.Read(reader.Span);
                if (rr.IsEndOfStream && outBuffer.Length == cursor.Length) return rr;
                if (rr.HasNoData) return (outBuffer.Length - cursor.Length) * sizeof(ushort);
                var u = rr.Length;
                var wrote = reader.Span.Slice(0, u).SliceAlign(channels);
                var dest = cursor.Slice(0, wrote.Length);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                if (DoDeltaSigmaModulation)
                {
                    ProcessAccurate(wrote, dest);
                }
                else
                {
                    ProcessNormal(wrote, dest);
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return (outBuffer.Length - cursor.Length) * sizeof(ushort);  //The Source doesn't fill whole reader so return here.
            }
            return outBuffer.Length * sizeof(ushort);
        }

        #region Accurate

        private void ProcessAccurate(Span<float> wrote, Span<short> dest) =>
            //

            ProcessAccurateStandard(wrote, dest);

        private void ProcessAccurateStandard(Span<float> wrote, Span<short> dest)
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
            if (IsEndiannessConversionRequired)
            {
                dest.ReverseEndianness();
            }
        }

        private static void ProcessAccurateMonaural(Span<float> wrote, Span<short> dest, Span<float> accSpan, Span<short> loSpan)
        {
            var dsmAcc = MemoryMarshal.GetReference(accSpan);
            var dsmPrev = MemoryMarshal.GetReference(loSpan);
            ref var rWrote = ref MemoryMarshal.GetReference(wrote);
            ref var rDest = ref MemoryMarshal.GetReference(dest);
            nint nLength = dest.Length;
            var mul = new Vector4(Multiplier);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul.X * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = dsmPrev = ConvertScaled(dsmAcc);
                Unsafe.Add(ref rDest, i) = v;
            }
            MemoryMarshal.GetReference(accSpan) = dsmAcc;
            MemoryMarshal.GetReference(loSpan) = dsmPrev;
        }

        private static void ProcessAccurateStereoStandard(Span<float> wrote, Span<short> dest, Span<float> accSpan, Span<short> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<short, (short x, short y)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector2(dsmLastOut.x, dsmLastOut.y);
            ref var rWrote = ref Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<short, (short x, short y)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 2;
            var min = new Vector2(-32768.0f);
            var max = new Vector2(32767.0f);
            var mul = new Vector2(32768.0f);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = VectorUtils.Round(dsmAcc);
                dsmPrev = Vector2.Clamp(v, min, max);
                Unsafe.Add(ref rDest, i) = ((short)dsmPrev.X, (short)dsmPrev.Y);
            }
            Unsafe.As<float, Vector2>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<short, (short x, short y)>(ref MemoryMarshal.GetReference(loSpan)) = ((short)dsmPrev.X, (short)dsmPrev.Y);
        }

        private static void ProcessAccurate3ChannelsStandard(Span<float> wrote, Span<short> dest, Span<float> accSpan, Span<short> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<short, (short x, short y, short z)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector3(dsmLastOut.x, dsmLastOut.y, dsmLastOut.z);
            ref var rWrote = ref Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<short, (short x, short y, short z)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 3;
            var min = new Vector3(-32768.0f);
            var max = new Vector3(32767.0f);
            var mul = new Vector3(32768.0f);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = Vector3.Clamp(dsmAcc, min, max);
                dsmPrev = VectorUtils.Round(v);
                Unsafe.Add(ref rDest, i) = ((short)dsmPrev.X, (short)dsmPrev.Y, (short)dsmPrev.Z);
            }
            Unsafe.As<float, Vector3>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<short, (short x, short y, short z)>(ref MemoryMarshal.GetReference(loSpan)) = ((short)dsmPrev.X, (short)dsmPrev.Y, (short)dsmPrev.Z);
        }

        private static void ProcessAccurate4ChannelsStandard(Span<float> wrote, Span<short> dest, Span<float> accSpan, Span<short> loSpan)
        {
            var dsmAcc = Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(accSpan));
            var dsmLastOut = Unsafe.As<short, (short x, short y, short z, short w)>(ref MemoryMarshal.GetReference(loSpan));
            var dsmPrev = new Vector4(dsmLastOut.x, dsmLastOut.y, dsmLastOut.z, dsmLastOut.w);
            ref var rWrote = ref Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(wrote));
            ref var rDest = ref Unsafe.As<short, (short x, short y, short z, short w)>(ref MemoryMarshal.GetReference(dest));
            var nLength = (nint)dest.Length / 4;
            var min = new Vector4(-32768.0f);
            var max = new Vector4(32767.0f);
            var mul = new Vector4(32768.0f);
            for (nint i = 0; i < nLength; i++)
            {
                var diff = mul * Unsafe.Add(ref rWrote, i) - dsmPrev;
                dsmAcc += diff;
                var v = Vector4.Clamp(dsmAcc, min, max);
                dsmPrev = VectorUtils.Round(v);
                Unsafe.Add(ref rDest, i) = ((short)dsmPrev.X, (short)dsmPrev.Y, (short)dsmPrev.Z, (short)dsmPrev.W);
            }
            Unsafe.As<float, Vector4>(ref MemoryMarshal.GetReference(accSpan)) = dsmAcc;
            Unsafe.As<short, (short x, short y, short z, short w)>(ref MemoryMarshal.GetReference(loSpan)) = ((short)dsmPrev.X, (short)dsmPrev.Y, (short)dsmPrev.Z, (short)dsmPrev.W);
        }
        private static nint ProcessAccurateDirectGenericStandard(Span<float> wrote, Span<short> dest, Span<float> dsmAcc, Span<short> dsmLast, int dsmChannelPointer)
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
                Unsafe.Add(ref dst, i) = v;
                ch &= hh;
            }
            return ch;
        }
        private void ProcessAccurateOrdinal(Span<float> wrote, Span<short> dest) => dsmChannelPointer = (int)ProcessAccurateDirectGenericStandard(wrote, dest, dsmAccumulator.Span, dsmLastOutput.Span, dsmChannelPointer);

        #endregion Accurate

        #region Normal

        private void ProcessNormal(Span<float> wrote, Span<short> dest)
        {
            if (IsEndiannessConversionRequired)
            {
#if NETCOREAPP3_1_OR_GREATER
                if (enableIntrinsics)
                {
                    if (Avx2.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Avx2))
                    {
                        ProcessReversedAvx2(wrote, dest);
                        return;
                    }
                    if (Ssse3.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Ssse3))
                    {
                        ProcessReversedSsse3(wrote, dest);
                        return;
                    }
                }
#endif
                ProcessReversedStandard(wrote, dest);
            }
            else
            {
#if NETCOREAPP3_1_OR_GREATER
                if (enableIntrinsics)
                {
                    if (Avx2.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Avx2))
                    {
                        ProcessNormalAvx2(wrote, dest);
                        return;
                    }
                    if (Sse2.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse2))
                    {
                        ProcessNormalSse2(wrote, dest);
                        return;
                    }
                }
#endif
                ProcessNormalStandard(wrote, dest);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessReversedStandard(Span<float> wrote, Span<short> dest)
        {
            ProcessNormalStandard(wrote, dest);
            dest.ReverseEndianness();
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalStandard(Span<float> wrote, Span<short> dest)
        {
            var max = new Vector<float>(32767.0f);
            var min = new Vector<float>(-32768.0f);
            var mul = new Vector<float>(32768.0f);
            var sign = Vector.AsVectorSingle(new Vector<int>(int.MinValue));
            var reciprocalEpsilon = new Vector<float>(16777216f);
            ref var dst = ref MemoryMarshal.GetReference(dest);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            nint i = 0, length = MathI.Min(wrote.Length, dest.Length);
            var olen = length - Vector<float>.Count * 4 + 1;
            for (; i < olen; i += Vector<float>.Count * 4)
            {
                var v0_ns = mul * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + Vector<float>.Count * 0));
                var v1_ns = mul * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + Vector<float>.Count * 1));
                var v2_ns = mul * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + Vector<float>.Count * 2));
                var v3_ns = mul * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref src, i + Vector<float>.Count * 3));
                v0_ns = VectorUtils.RoundInLoop(v0_ns, sign, reciprocalEpsilon);
                v1_ns = VectorUtils.RoundInLoop(v1_ns, sign, reciprocalEpsilon);
                v2_ns = VectorUtils.RoundInLoop(v2_ns, sign, reciprocalEpsilon);
                v3_ns = VectorUtils.RoundInLoop(v3_ns, sign, reciprocalEpsilon);
                v0_ns = Vector.Min(v0_ns, max);
                v1_ns = Vector.Min(v1_ns, max);
                v2_ns = Vector.Min(v2_ns, max);
                v3_ns = Vector.Min(v3_ns, max);
                v0_ns = Vector.Max(v0_ns, min);
                v1_ns = Vector.Max(v1_ns, min);
                v2_ns = Vector.Max(v2_ns, min);
                v3_ns = Vector.Max(v3_ns, min);
                var v0_ns2 = Vector.ConvertToInt32(v0_ns);
                var v1_ns2 = Vector.ConvertToInt32(v1_ns);
                var v2_ns2 = Vector.ConvertToInt32(v2_ns);
                var v3_ns2 = Vector.ConvertToInt32(v3_ns);
                var v4_nh = Vector.Narrow(v0_ns2, v1_ns2);
                var v5_nh = Vector.Narrow(v2_ns2, v3_ns2);
                Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref dst, i + Vector<short>.Count * 0)) = v4_nh;
                Unsafe.As<short, Vector<short>>(ref Unsafe.Add(ref dst, i + Vector<short>.Count * 1)) = v5_nh;
            }
            for (; i < length; i++)
            {
                var s0 = mul[0] * Unsafe.Add(ref src, i);
                s0 = FastMath.Min(s0, max[0]);
                s0 = FastMath.Max(s0, min[0]);
                s0 = FastMath.Round(s0);
                var x0 = (int)s0;
                Unsafe.Add(ref dst, i) = (short)x0;
            }
        }

        #endregion Normal

        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -1 and 1, and then converts to <see cref="short"/>.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static short Convert(float srcval)
        {
            srcval *= Multiplier;
            return ConvertScaled(srcval);
        }

        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -32768.0f and 32767.0f, and then converts to <see cref="short"/>.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static short ConvertScaled(float srcval)
        {
            srcval = FastMath.Round(srcval);
            srcval = FastMath.Max(srcval, -32768.0f);
            srcval = FastMath.Min(srcval, 32767.0f);
            return (short)srcval;
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
