using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

using Shamisen.Utils;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Converts samples to 32-bit PCM.
    /// </summary>
    /// <seealso cref="SampleToWaveConverterBase" />
    public sealed class SampleToPcm32Converter : SampleToWaveConverterBase
    {
        private const float Multiplier = 2147483648.0f;
        private const int ActualBytesPerSample = sizeof(int);
        private const int BufferMax = 1024;
        private int ActualBufferMax => BufferMax - (BufferMax % Source.Format.Channels);

        private Memory<int> dsmLastOutput;
        private Memory<float> dsmAccumulator;
        private int dsmChannelPointer = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleToPcm32Converter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="accuracyNeeded">Turns on <see cref="AccuracyMode"/> when <c>true</c>.</param>
        /// <param name="endianness">The destination endianness.</param>
        public SampleToPcm32Converter(IReadableAudioSource<float, SampleFormat> source, bool accuracyNeeded = true, Endianness endianness = Endianness.Little)
            : base(source, new WaveFormat(source.Format.SampleRate, 32, source.Format.Channels, AudioEncoding.LinearPcm))
        {
            if (accuracyNeeded)
            {
                dsmAccumulator = new float[source.Format.Channels];
                dsmLastOutput = new int[source.Format.Channels];
            }
            AccuracyMode = accuracyNeeded;
            Endianness = endianness;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SampleToPcm32Converter"/> does the 32-bit Delta-Sigma modulation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the accuracy mode is turned on; otherwise, <c>false</c>.
        /// </value>
        public bool AccuracyMode { get; }

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
            var cursor = MemoryMarshal.Cast<byte, int>(buffer);
            while (cursor.Length > 0)
            {
                var reader = MemoryMarshal.Cast<int, float>(cursor);
                var rr = Source.Read(reader);
                if (rr.IsEndOfStream && buffer.Length == cursor.Length) return rr;
                if (rr.HasNoData) return buffer.Length - cursor.Length;
                var u = rr.Length;
                var wrote = reader.Slice(0, u);
                var dest = MemoryMarshal.Cast<float, int>(wrote);
                if (wrote.Length != dest.Length)
                {
                    new InvalidOperationException(
                        $"The {nameof(wrote)}'s length and {nameof(dest)}'s length are not equal! This is a bug!").Throw();
                }

                if (AccuracyMode)
                {
                    dsmChannelPointer = ProcessAccurate(wrote, dsmAccumulator.Span, dsmLastOutput.Span, dsmChannelPointer, IsEndiannessConversionRequired);
                }
                else
                {
                    ConvertSampleToPcm32(MemoryMarshal.Cast<float, int>(wrote), wrote, IsEndiannessConversionRequired);
                }
                cursor = cursor.Slice(dest.Length);
                if (u != reader.Length) return buffer.Length - cursor.Length;  //The Source doesn't fill whole reader so return here.
            }
            return buffer.Length;
        }

        #region Accurate

        private static int ProcessAccurate(Span<float> wrote, Span<float> dsmAcc, Span<int> dsmLast, int dsmChannelPointer, bool convertEndianness)
        {
            var ch = ProcessAccurateDirectGenericStandard(wrote, dsmAcc, dsmLast, dsmChannelPointer);
            if (convertEndianness)
            {
                MemoryMarshal.Cast<float, int>(wrote).ReverseEndianness();
            }
            return (int)ch;
        }

        private static nint ProcessAccurateDirectGenericStandard(Span<float> wrote, Span<float> dsmAcc, Span<int> dsmLast, int dsmChannelPointer)
        {
            ref var acc = ref MemoryMarshal.GetReference(dsmAcc);
            ref var dlo = ref MemoryMarshal.GetReference(dsmLast);
            ref var src = ref MemoryMarshal.GetReference(wrote);
            ref var dst = ref Unsafe.As<float, int>(ref src);
            var channels = dsmAcc.Length;
            nint ch = dsmChannelPointer % channels;
            nint i = 0, length = wrote.Length;
            const float Mul = Multiplier;
            for (; i < length; i++)
            {
                var a = Unsafe.Add(ref acc, ch);
                var diff = Mul * Unsafe.Add(ref src, i) - Unsafe.Add(ref dlo, ch);
                a += diff;
                var v = ConvertFloatToInt32Clamped(a);
                Unsafe.Add(ref dst, i) = v;
                Unsafe.Add(ref dlo, ch) = v;
                Unsafe.Add(ref acc, ch) = a;
                var h = ++ch < channels;
                var hh = -Unsafe.As<bool, byte>(ref h);
                ch &= hh;
            }
            return ch;
        }

        #endregion

        /// <summary>
        /// Clamps the specified <paramref name="value"/> between -2147483648.0f and 2147483648.0f, and then converts to <see cref="int"/>.<br/>
        /// Note that the value &gt;= 1.0f will be converted to <see cref="int.MaxValue"/> instead of 0 or 2147483520 in order to avoid overflows and quantization noises.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int ConvertFloatToInt32Clamped(float value)
        {
            value = FastMath.Max(value, -2147483648.0f);
            var g = value < 2147483648.0f;
            var h = -Unsafe.As<bool, byte>(ref g);
            var res = (int)FastMath.Round(value);
            res &= h;
            res |= (int)((uint)~h >> 1);
            return res;
        }

        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -1 and 1, and then converts to <see cref="int"/>.<br/>
        /// Note that the value >= 1.0f will be converted to <see cref="int.MaxValue"/> instead of 0 or 2147483520 in order to avoid overflows and quantization noises.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        public static int ConvertSampleToPcm32(float srcval)
        {
            srcval *= Multiplier;
            return ConvertFloatToInt32Clamped(srcval);
        }

        #region Normal
        /// <summary>
        /// Converts <see cref="float"/> values to <see cref="int"/> values.
        /// </summary>
        /// <param name="destination">The place to store converted values.</param>
        /// <param name="source">The original values to convert.</param>
        /// <param name="convertEndianness">The value which indicates whether <see cref="ConvertSampleToPcm32(Span{int}, ReadOnlySpan{float}, bool)"/> should perform endianness conversion.</param>
        public static void ConvertSampleToPcm32(Span<int> destination, ReadOnlySpan<float> source, bool convertEndianness)
        {
            if (!convertEndianness)
            {
                if (Avx.IsSupported)
                {
                    ProcessNormalDirectAvx(destination, source);
                    return;
                }
                ProcessNormalDirectStandard(destination, source);
            }
            else
            {
                if (Avx2.IsSupported)
                {
                    ProcessNormalReversedAvx2(destination, source);
                    return;
                }
                ProcessNormalDirectStandard(destination, source);
                destination.ReverseEndianness();
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalDirectAvx(Span<int> destination, ReadOnlySpan<float> source)
        {
            ref var src = ref MemoryMarshal.GetReference(source);
            ref var dst = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(source.Length, destination.Length);
            var max = Vector256.Create(2147483648.0f);
            var subs = Vector256.Create(int.MaxValue).AsSingle();
            var olen = length - 4 * Vector256<float>.Count + 1;
            for (; i < olen; i += 4 * Vector256<float>.Count)
            {
                var ymm0 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 0 * Vector256<float>.Count));
                var ymm1 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 1 * Vector256<float>.Count));
                var ymm2 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 2 * Vector256<float>.Count));
                var ymm3 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 3 * Vector256<float>.Count));
                var ymm4 = Avx.Compare(max, ymm0, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm5 = Avx.Compare(max, ymm1, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm6 = Avx.Compare(max, ymm2, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm7 = Avx.Compare(max, ymm3, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                ymm0 = Avx.BlendVariable(ymm0, subs, ymm4);
                ymm1 = Avx.BlendVariable(ymm1, subs, ymm5);
                ymm2 = Avx.BlendVariable(ymm2, subs, ymm6);
                ymm3 = Avx.BlendVariable(ymm3, subs, ymm7);
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<int>.Count)) = ymm0;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<int>.Count)) = ymm1;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 2 * Vector256<int>.Count)) = ymm2;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 3 * Vector256<int>.Count)) = ymm3;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = ConvertSampleToPcm32(Unsafe.Add(ref src, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalDirectStandard(Span<int> destination, ReadOnlySpan<float> source)
        {
            ref var src = ref MemoryMarshal.GetReference(source);
            ref var dst = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(source.Length, destination.Length);
            var max = new Vector<float>(2147483648.0f);
            var subs = Vector.AsVectorSingle(new Vector<int>(int.MaxValue));
            var sign = Vector.AsVectorSingle(new Vector<int>(int.MinValue));
            var reciprocalEpsilon = new Vector<float>(16777216f);
            var olen = length - 4 * Vector<float>.Count + 1;
            for (; i < olen; i += 4 * Vector<float>.Count)
            {
                var v0_ns = max * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 0 * Vector<float>.Count));
                var v1_ns = max * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 1 * Vector<float>.Count));
                var v2_ns = max * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 2 * Vector<float>.Count));
                var v3_ns = max * Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 3 * Vector<float>.Count));
                var v4_ns = Vector.LessThanOrEqual(max, v0_ns);
                var v5_ns = Vector.LessThanOrEqual(max, v1_ns);
                var v6_ns = Vector.LessThanOrEqual(max, v2_ns);
                var v7_ns = Vector.LessThanOrEqual(max, v3_ns);
                v0_ns = VectorUtils.RoundInLoop(v0_ns, sign, reciprocalEpsilon);
                v1_ns = VectorUtils.RoundInLoop(v1_ns, sign, reciprocalEpsilon);
                v2_ns = VectorUtils.RoundInLoop(v2_ns, sign, reciprocalEpsilon);
                v3_ns = VectorUtils.RoundInLoop(v3_ns, sign, reciprocalEpsilon);
                v0_ns = Vector.ConvertToInt32(v0_ns).AsSingle();
                v1_ns = Vector.ConvertToInt32(v1_ns).AsSingle();
                v2_ns = Vector.ConvertToInt32(v2_ns).AsSingle();
                v3_ns = Vector.ConvertToInt32(v3_ns).AsSingle();
                v0_ns = VectorUtils.Blend(v4_ns, v0_ns, subs);
                v1_ns = VectorUtils.Blend(v5_ns, v1_ns, subs);
                v2_ns = VectorUtils.Blend(v6_ns, v2_ns, subs);
                v3_ns = VectorUtils.Blend(v7_ns, v3_ns, subs);
                Unsafe.As<int, Vector<float>>(ref Unsafe.Add(ref dst, i + 0 * Vector<int>.Count)) = v0_ns;
                Unsafe.As<int, Vector<float>>(ref Unsafe.Add(ref dst, i + 1 * Vector<int>.Count)) = v1_ns;
                Unsafe.As<int, Vector<float>>(ref Unsafe.Add(ref dst, i + 2 * Vector<int>.Count)) = v2_ns;
                Unsafe.As<int, Vector<float>>(ref Unsafe.Add(ref dst, i + 3 * Vector<int>.Count)) = v3_ns;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = ConvertSampleToPcm32(Unsafe.Add(ref src, i));
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessNormalReversedAvx2(Span<int> destination, ReadOnlySpan<float> source)
        {
            ref var src = ref MemoryMarshal.GetReference(source);
            ref var dst = ref MemoryMarshal.GetReference(destination);
            nint i = 0, length = MathI.Min(source.Length, destination.Length);
            var max = Vector256.Create(2147483648.0f);
            var subs = Vector256.Create(int.MaxValue).AsSingle();
            var mask256 = Vector256.Create(3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12, 3, 2, 1, 0, 7, 6, 5, 4, 11, 10, 9, 8, 15, 14, 13, 12).AsByte();
            var olen = length - 4 * Vector256<float>.Count + 1;
            for (; i < olen; i += 4 * Vector256<float>.Count)
            {
                var ymm0 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 0 * Vector256<float>.Count));
                var ymm1 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 1 * Vector256<float>.Count));
                var ymm2 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 2 * Vector256<float>.Count));
                var ymm3 = max * Unsafe.As<float, Vector256<float>>(ref Unsafe.Add(ref Unsafe.Add(ref src, i), 3 * Vector256<float>.Count));
                var ymm4 = Avx.Compare(max, ymm0, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm5 = Avx.Compare(max, ymm1, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm6 = Avx.Compare(max, ymm2, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                var ymm7 = Avx.Compare(max, ymm3, FloatComparisonMode.OrderedLessThanOrEqualSignaling);
                ymm0 = Avx.ConvertToVector256Int32(ymm0).AsSingle();
                ymm1 = Avx.ConvertToVector256Int32(ymm1).AsSingle();
                ymm2 = Avx.ConvertToVector256Int32(ymm2).AsSingle();
                ymm3 = Avx.ConvertToVector256Int32(ymm3).AsSingle();
                ymm0 = Avx.BlendVariable(ymm0, subs, ymm4);
                ymm1 = Avx.BlendVariable(ymm1, subs, ymm5);
                ymm2 = Avx.BlendVariable(ymm2, subs, ymm6);
                ymm3 = Avx.BlendVariable(ymm3, subs, ymm7);
                ymm0 = Avx2.Shuffle(ymm0.AsByte(), mask256).AsSingle();
                ymm1 = Avx2.Shuffle(ymm1.AsByte(), mask256).AsSingle();
                ymm2 = Avx2.Shuffle(ymm2.AsByte(), mask256).AsSingle();
                ymm3 = Avx2.Shuffle(ymm3.AsByte(), mask256).AsSingle();
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 0 * Vector256<int>.Count)) = ymm0;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 1 * Vector256<int>.Count)) = ymm1;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 2 * Vector256<int>.Count)) = ymm2;
                Unsafe.As<int, Vector256<float>>(ref Unsafe.Add(ref dst, i + 3 * Vector256<int>.Count)) = ymm3;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref dst, i) = BinaryPrimitives.ReverseEndianness(ConvertSampleToPcm32(Unsafe.Add(ref src, i)));
            }
        }
        #endregion

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
            }
            disposedValue = true;
        }
    }
}
