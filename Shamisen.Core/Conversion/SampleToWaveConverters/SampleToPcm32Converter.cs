using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
                    ProcessNormal(wrote, IsEndiannessConversionRequired);
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
                var v = ConvertScaled(a);
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

        #region Normal
        private static void ProcessNormal(Span<float> wrote, bool convertEndianness) => ProcessNormalStandard(wrote, convertEndianness);

        private static void ProcessNormalStandard(Span<float> wrote, bool convertEndianness)
        {
            ProcessNormalDirectStandard(wrote);
            if (convertEndianness)
            {
                MemoryMarshal.Cast<float, int>(wrote).ReverseEndianness();
            }
        }

        internal static void ProcessNormalDirectStandard(Span<float> wrote)
        {
            ref var src = ref MemoryMarshal.GetReference(wrote);
            ref var dst = ref Unsafe.As<float, int>(ref src);
            nint i = 0, length = wrote.Length;
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
                v0_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v0_ns));
                v1_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v1_ns));
                v2_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v2_ns));
                v3_ns = Vector.AsVectorSingle(Vector.ConvertToInt32(v3_ns));
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
                Unsafe.Add(ref dst, i) = Convert(Unsafe.Add(ref src, i));
            }
        }
        #endregion
        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -1 and 1, and then converts to <see cref="int"/>.<br/>
        /// Note that the value >= 1.0f will be converted to <see cref="int.MaxValue"/> instead of 0 or 2147483520 in order to avoid overflows and quantization noises.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static int Convert(float srcval)
        {
            srcval *= Multiplier;
            return ConvertScaled(srcval);
        }

        /// <summary>
        /// Clamps the specified <paramref name="srcval"/> between -2147483648.0f and 2147483648.0f, and then converts to <see cref="int"/>.<br/>
        /// Note that the value >= 1.0f will be converted to <see cref="int.MaxValue"/> instead of 0 or 2147483520 in order to avoid overflows and quantization noises.
        /// </summary>
        /// <param name="srcval"></param>
        /// <returns></returns>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static int ConvertScaled(float srcval)
        {
            srcval = FastMath.Max(srcval, -2147483648.0f);
            var g = srcval < 2147483648.0f;
            var h = -Unsafe.As<bool, byte>(ref g);
            var res = (int)FastMath.Round(srcval);
            res &= h;
            res |= (int)((uint)~h >> 1);
            return res;
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
            }
            disposedValue = true;
        }
    }
}
