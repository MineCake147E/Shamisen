using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics.Arm;
#endif

using Shamisen.Filters;
using Shamisen.Optimization;
using Shamisen.Utils;

using System.Buffers;

namespace Shamisen.Conversion.Resampling.Sample
{
    /// <summary>
    /// Performs up-sampling using Catmull-Rom Spline interpolation.
    ///
    /// </summary>
    /// <seealso cref="ResamplerBase" />
    public sealed partial class SplineResampler : ResamplerBase
    {
        private const int CacheThreshold = 512;
        private ResizableBufferWrapper<float> bufferWrapper;
        private int conversionGradient = 0;
        private int framesReserved = 1;
        private int rearrangedCoeffsIndex = 0;
        private int rearrangedCoeffsDirection = 0;


        /// <summary>
        /// The pre calculated Catmull-Rom coefficients.<br/>
        /// X: The coefficient for value1 ((-xP3 + 2 * xP2 - x) * 0.5f)<br/>
        /// Y: The coefficient for value2 (((3 * xP3) - (5 * xP2) + 2) * 0.5f)<br/>
        /// Z: The coefficient for value3 ((-(3 * xP3) + 4 * xP2 + x) * 0.5f)<br/>
        /// W: The coefficient for value4 ((xP3 - xP2) * 0.5f)<br/>
        /// </summary>
        private Vector4[] preCalculatedCatmullRomCoefficients;

        private float[][] sampleCache;
        private int samplesRemaining = 0;

        private bool isEndOfStream = false;

        private ResampleStrategy Strategy { get; }
        private X86Intrinsics X86Intrinsics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SplineResampler"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationSampleRate">The destination sample rate.</param>
        public SplineResampler(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate) :
            this(source, destinationSampleRate, IntrinsicsUtils.X86Intrinsics)
        {

        }

        internal SplineResampler(IReadableAudioSource<float, SampleFormat> source, int destinationSampleRate, X86Intrinsics x86Intrinsics)
            : base(source.Format.SampleRate > destinationSampleRate
                ? new BiQuadFilter(source, BiQuadParameter.CreateLPFParameter(source.Format.SampleRate, destinationSampleRate * 0.5, 0.70710678118654752440084436210485))
                : source, destinationSampleRate)
        {
            X86Intrinsics = x86Intrinsics;
            bufferWrapper = new ResizablePooledBufferWrapper<float>(1);
            sampleCache = new float[3][];
            for (int i = 0; i < sampleCache.Length; i++)
            {
                sampleCache[i] = new float[Channels];
            }
            (Strategy, preCalculatedCatmullRomCoefficients, rearrangedCoeffsIndex, rearrangedCoeffsDirection) = GenerateCatmullRomCoefficents(source.Format.SampleRate, destinationSampleRate, RateMulInverse, RateMul, GradientIncrement);
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static (ResampleStrategy, Vector4[], int rearrangedCoeffsIndex, int rearrangedCoeffsDirection) GenerateCatmullRomCoefficents(int sourceSampleRate, int destinationSampleRate, float rateMulInverse, int rateMul, int acc)
        {
            if (sourceSampleRate < destinationSampleRate)
            {
                if (rateMul <= CacheThreshold)
                {
                    var coeffs = new Vector4[rateMul];
                    GenerateCoeffs(coeffs, rateMulInverse);
                    //CachedDirect couldn't benefit from sequentialization currently.
                    return (ResampleStrategy.CachedDirect, coeffs, 0, 1);
                }
                else if (rateMul <= CacheThreshold * 2)
                {
                    if ((rateMul & 1) > 0)
                    {
                        var coeffs = new Vector4[rateMul / 2 + 1];
                        GenerateCoeffs(coeffs, rateMulInverse);
                        RearrangeCoefficentsCachedWrappedOdd(coeffs, rateMul, acc);
                        return (ResampleStrategy.CachedWrappedOdd, coeffs, 0, 1);
                    }
                    else
                    {
                        var coeffs = new Vector4[(rateMul / 2)];
                        GenerateCoeffs(coeffs, rateMulInverse);
                        var h = RearrangeCoefficentsCachedWrappedEven(coeffs, rateMul, acc);
                        return (ResampleStrategy.CachedWrappedEven, coeffs, h.initialPosition, h.initialDirection);
                    }
                }
            }
            return (ResampleStrategy.Direct, Array.Empty<Vector4>(), 0, 1);
        }

        /// <summary>
        /// [WIP] Rearranges coefficients for <see cref="ResampleStrategy.CachedWrappedOdd"/>.
        ///
        /// </summary>
        /// <param name="coeffs"></param>
        /// <param name="rateMul"></param>
        /// <param name="acc"></param>
        private static void RearrangeCoefficentsCachedWrappedOdd(Span<Vector4> coeffs, int rateMul, int acc)
        {
            if (acc == 1) return;

            static Vector4 GetCatmullRomCoefficentsOdd(ref Vector4 coeffs, int i, int rateMul)
            {
                int x = i;
                if (i <= rateMul >> 1) return Unsafe.Add(ref coeffs, x);
                x = rateMul - i;
                var q = Unsafe.Add(ref coeffs, x);
                return VectorUtils.ReverseElements(q);
            }
            var g = ArrayPool<float>.Shared.Rent(4 * coeffs.Length);
            var gs = MemoryMarshal.Cast<float, Vector4>(g.AsSpan(0, 4 * coeffs.Length));
            coeffs.CopyTo(gs);
            ref var vd = ref MemoryMarshal.GetReference(gs);
            var h = 0;
            for (int i = 0; i < coeffs.Length; i++)
            {
                coeffs[i] = GetCatmullRomCoefficentsOdd(ref vd, h, rateMul);
                h += acc;
                bool j = h >= rateMul;
                int f = Unsafe.As<bool, byte>(ref j);
                h -= -f & rateMul;
            }
            g.AsSpan().FastFill(default);
            ArrayPool<float>.Shared.Return(g);
        }

        /// <summary>
        /// [WIP] Rearranges coefficients for <see cref="ResampleStrategy.CachedWrappedEven"/>.
        ///
        /// </summary>
        /// <param name="coeffs"></param>
        /// <param name="rateMul"></param>
        /// <param name="acc"></param>
        private static (int initialPosition, int initialDirection) RearrangeCoefficentsCachedWrappedEven(Span<Vector4> coeffs, int rateMul, int acc)
        {
            if (acc == 1) return (0, 1);

            static Vector4 GetCatmullRomCoefficentsEven(ref Vector4 coeffs, int i, int rateMul)
            {
                int x = i;
                if (i < rateMul >> 1) return Unsafe.Add(ref coeffs, x);
                x = rateMul - i - 1;
                var q = Unsafe.Add(ref coeffs, x);
                return VectorUtils.ReverseElements(q);
            }
            var g = ArrayPool<float>.Shared.Rent(4 * coeffs.Length);
            var gs = MemoryMarshal.Cast<float, Vector4>(g.AsSpan(0, 4 * coeffs.Length));
            coeffs.CopyTo(gs);
            ref var vd = ref MemoryMarshal.GetReference(gs);
            var inverse = MathI.ModularMultiplicativeInverse(acc, rateMul);
            var pred = (rateMul - 1) * inverse % rateMul;
            var wpred = (pred + 1) >> 1;
            var h = wpred * acc % rateMul;
            for (int i = 0; i < coeffs.Length; i++)
            {
                coeffs[i] = GetCatmullRomCoefficentsEven(ref vd, h, rateMul);
                h += acc;
                bool j = h >= rateMul;
                int f = Unsafe.As<bool, byte>(ref j);
                h -= -f & rateMul;
            }
            g.AsSpan().FastFill(default);
            ArrayPool<float>.Shared.Return(g);
            return (pred - wpred, -1);
        }
        #region GenerateCoeffs


        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static void GenerateCoeffs(Span<Vector4> coeffs, float rateMulInverse)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (Sse2.IsSupported)
            {
                GenerateCoeffsSse2(coeffs, rateMulInverse);
                return;
            }
#endif
            GenerateCoeffsStandard(coeffs, rateMulInverse);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// SSE2 vectorized path
        /// x1.1 faster than Standard on Core i7-4790 at 3.90GHz
        /// </summary>
        /// <param name="coeffs"></param>
        /// <param name="rateMulInverse"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void GenerateCoeffsSse2(Span<Vector4> coeffs, float rateMulInverse)
        {
            var xmm0 = Vector128.Create(-0.5f, 1.5f, -1.5f, 0.5f);
            var xmm1 = Vector128.Create(1.0f, -2.5f, 2.0f, -0.5f);
            var xmm2 = Vector128.Create(-0.5f, 0.0f, 0.5f, 0.0f);
            var xmm3 = Vector128.Create(0.0f, 1.0f, 0.0f, 0.0f);
            var xmm4 = Vector128.Create(rateMulInverse);
            Vector128<int> xmm5;
            var xmm6 = Vector128.Create(0, 1, 2, 3);
            ref var rdi = ref Unsafe.As<Vector4, Vector128<float>>(ref MemoryMarshal.GetReference(coeffs));
            nint i = 0, length = coeffs.Length;
            if (length >= 8)
            {
                xmm5 = Vector128.Create(4);
                for (; i < length - 3; i += 4)
                {
                    Vector128<float> xmm12, xmm13, xmm14, xmm15;
                    xmm12 = xmm13 = xmm14 = xmm15 = xmm0;
                    var xmm7 = Sse2.ConvertToVector128Single(xmm6);
                    xmm7 = Sse.Multiply(xmm7, xmm4);
                    var xmm8 = Sse.Shuffle(xmm7, xmm7, 0b00_00_00_00);
                    var xmm9 = Sse.Shuffle(xmm7, xmm7, 0b01_01_01_01);
                    var xmm10 = Sse.Shuffle(xmm7, xmm7, 0b10_10_10_10);
                    var xmm11 = Sse.Shuffle(xmm7, xmm7, 0b11_11_11_11);
                    xmm12 = Sse.Multiply(xmm8, xmm12);
                    xmm12 = Sse.Add(xmm12, xmm1);
                    xmm13 = Sse.Multiply(xmm9, xmm13);
                    xmm13 = Sse.Add(xmm13, xmm1);
                    xmm14 = Sse.Multiply(xmm10, xmm14);
                    xmm14 = Sse.Add(xmm14, xmm1);
                    xmm15 = Sse.Multiply(xmm11, xmm15);
                    xmm15 = Sse.Add(xmm15, xmm1);
                    xmm12 = Sse.Multiply(xmm8, xmm12);
                    xmm12 = Sse.Add(xmm12, xmm2);
                    xmm13 = Sse.Multiply(xmm9, xmm13);
                    xmm13 = Sse.Add(xmm13, xmm2);
                    xmm14 = Sse.Multiply(xmm10, xmm14);
                    xmm14 = Sse.Add(xmm14, xmm2);
                    xmm15 = Sse.Multiply(xmm11, xmm15);
                    xmm15 = Sse.Add(xmm15, xmm2);
                    xmm12 = Sse.Multiply(xmm8, xmm12);
                    xmm12 = Sse.Add(xmm12, xmm3);
                    xmm13 = Sse.Multiply(xmm9, xmm13);
                    xmm13 = Sse.Add(xmm13, xmm3);
                    xmm14 = Sse.Multiply(xmm10, xmm14);
                    xmm14 = Sse.Add(xmm14, xmm3);
                    xmm15 = Sse.Multiply(xmm11, xmm15);
                    xmm15 = Sse.Add(xmm15, xmm3);
                    xmm6 = Sse2.Add(xmm5, xmm6);
                    Unsafe.Add(ref rdi, i) = xmm12;
                    Unsafe.Add(ref rdi, i + 1) = xmm13;
                    Unsafe.Add(ref rdi, i + 2) = xmm14;
                    Unsafe.Add(ref rdi, i + 3) = xmm15;
                }
            }
            xmm5 = Vector128.CreateScalarUnsafe(1);
            for (; i < length; i++)
            {
                Vector128<float> xmm12;
                xmm12 = xmm0;
                var xmm7 = Sse2.ConvertToVector128Single(xmm6);
                xmm7 = Sse.MultiplyScalar(xmm7, xmm4);
                var xmm8 = Sse.Shuffle(xmm7, xmm7, 0b00_00_00_00);
                xmm12 = Sse.Multiply(xmm8, xmm12);
                xmm12 = Sse.Add(xmm12, xmm1);
                xmm12 = Sse.Multiply(xmm8, xmm12);
                xmm12 = Sse.Add(xmm12, xmm2);
                xmm12 = Sse.Multiply(xmm8, xmm12);
                xmm12 = Sse.Add(xmm12, xmm3);
                Unsafe.Add(ref rdi, i) = xmm12;
                xmm6 = Sse2.Add(xmm5, xmm6);
            }
        }
        /// <summary>
        /// FMA vectorized path
        /// x1.5 faster than Sse2 on Core i7-4790 at 3.59GHz(AVX DOWNCLOCK!?)
        /// </summary>
        /// <param name="coeffs"></param>
        /// <param name="rateMulInverse"></param>
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void GenerateCoeffsFma128(Span<Vector4> coeffs, float rateMulInverse)
        {
            var xmm0 = Vector128.Create(-0.5f, 1.5f, -1.5f, 0.5f);
            var xmm1 = Vector128.Create(1.0f, -2.5f, 2.0f, -0.5f);
            var xmm2 = Vector128.Create(-0.5f, 0.0f, 0.5f, 0.0f);
            var xmm3 = Vector128.Create(0.0f, 1.0f, 0.0f, 0.0f);
            var xmm4 = Vector128.Create(rateMulInverse);
            Vector128<int> xmm5;
            var xmm6 = Vector128.Create(0, 1, 2, 3);
            ref var rdi = ref Unsafe.As<Vector4, Vector128<float>>(ref MemoryMarshal.GetReference(coeffs));
            nint i = 0, length = coeffs.Length;
            if (length >= 8)
            {
                xmm5 = Vector128.Create(4);
                for (; i < length - 3; i += 4)
                {
                    Vector128<float> xmm12, xmm13, xmm14, xmm15;
                    xmm12 = xmm13 = xmm14 = xmm15 = xmm0;
                    var xmm7 = Sse2.ConvertToVector128Single(xmm6);
                    xmm7 = Sse.Multiply(xmm7, xmm4);
                    var xmm8 = Sse.Shuffle(xmm7, xmm7, 0b00_00_00_00);
                    xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm1);
                    var xmm9 = Sse.Shuffle(xmm7, xmm7, 0b01_01_01_01);
                    xmm13 = Fma.MultiplyAdd(xmm13, xmm9, xmm1);
                    var xmm10 = Sse.Shuffle(xmm7, xmm7, 0b10_10_10_10);
                    xmm14 = Fma.MultiplyAdd(xmm14, xmm10, xmm1);
                    var xmm11 = Sse.Shuffle(xmm7, xmm7, 0b11_11_11_11);
                    xmm15 = Fma.MultiplyAdd(xmm15, xmm11, xmm1);
                    xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm2);
                    xmm13 = Fma.MultiplyAdd(xmm13, xmm9, xmm2);
                    xmm14 = Fma.MultiplyAdd(xmm14, xmm10, xmm2);
                    xmm15 = Fma.MultiplyAdd(xmm15, xmm11, xmm2);
                    xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm3);
                    xmm13 = Fma.MultiplyAdd(xmm13, xmm9, xmm3);
                    xmm14 = Fma.MultiplyAdd(xmm14, xmm10, xmm3);
                    xmm15 = Fma.MultiplyAdd(xmm15, xmm11, xmm3);
                    xmm6 = Sse2.Add(xmm5, xmm6);
                    Unsafe.Add(ref rdi, i) = xmm12;
                    Unsafe.Add(ref rdi, i + 1) = xmm13;
                    Unsafe.Add(ref rdi, i + 2) = xmm14;
                    Unsafe.Add(ref rdi, i + 3) = xmm15;
                }
            }
            xmm5 = Vector128.CreateScalarUnsafe(1);
            for (; i < length; i++)
            {
                Vector128<float> xmm12;
                xmm12 = xmm0;
                var xmm7 = Sse2.ConvertToVector128Single(xmm6);
                xmm7 = Sse.MultiplyScalar(xmm7, xmm4);
                var xmm8 = Sse.Shuffle(xmm7, xmm7, 0b00_00_00_00);
                xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm1);
                xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm2);
                xmm12 = Fma.MultiplyAdd(xmm12, xmm8, xmm3);
                Unsafe.Add(ref rdi, i) = xmm12;
                xmm6 = Sse2.Add(xmm5, xmm6);
            }
        }
#endif
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void GenerateCoeffsStandard(Span<Vector4> coeffs, float rateMulInverse)
        {
            var c0 = new Vector4(-0.5f, 1.5f, -1.5f, 0.5f);
            var c1 = new Vector4(1.0f, -2.5f, 2.0f, -0.5f);
            var c2 = new Vector4(-0.5f, 0.0f, 0.5f, 0.0f);
            var c3 = new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            ref var rdi = ref MemoryMarshal.GetReference(coeffs);
            nint i = 0, length = coeffs.Length;
            for (; i < length; i++)
            {
                var x = i * rateMulInverse;
                var vx = new Vector4(x);
                var y = c0;
                y *= vx;
                y += c1;
                y *= vx;
                y += c2;
                y *= vx;
                y += c3;
                Unsafe.Add(ref rdi, i) = y;
            }
        }
        #endregion


        #region Misc

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Abs(int value)
        {
            //TODO: get outside
            int mask = value >> 31;
            return (uint)((value + mask) ^ mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector4 CalculateCatmullRomCoeffs(float x)
        {
            //Horner's Method for Catmull-Rom Coeffs
            var vx = new Vector4(x);
            var y = new Vector4(-0.5f, 1.5f, -1.5f, 0.5f);
            y *= vx;
            y += new Vector4(1.0f, -2.5f, 2.0f, -0.5f);
            y *= vx;
            y += new Vector4(-0.5f, 0.0f, 0.5f, 0.0f);
            y *= vx;
            y += new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AdvanceConversionGradient(ref int conversionGradient)
        {
            unchecked
            {
                conversionGradient += GradientIncrement;
                var t = IndexIncrement;
                if (conversionGradient >= RateMul)
                {
                    t++;
                }
                return t;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int AdvanceConversionGradient(ref int conversionGradient, int rad, int ram)
        {
            unchecked
            {
                conversionGradient += rad;
                int l = 0;
                while (conversionGradient >= ram)
                {
                    conversionGradient -= ram;
                    l++;
                }
                return l;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckBuffer(int channels, int sampleLengthOut)
        {
            //Internal buffer length check and some boundary-sample copies
            //Reserving some samples ahead
            //Row 1: #=to read $=not to read %=read and copied to $s
            //$$$############################%%%%
            // ^ process head                 ^process tail
            int v = sampleLengthOut * RateDiv + conversionGradient;
            var h = RateMulDivisor.DivRem((uint)v, out var b);
            int samplesRequired = (int)b + 3 + (h > 0 ? 1 : 0);
            int internalBufferLengthRequired = samplesRequired * channels;
            return internalBufferLengthRequired;
        }

        private void ExpandBuffer(int internalBufferLengthRequired)
        {
            int lengthReserved = framesReserved * Channels;
            Span<float> a = stackalloc float[lengthReserved];

            if (bufferWrapper.Buffer.Length > lengthReserved) bufferWrapper.Buffer.Slice(0, lengthReserved).CopyTo(a);
            bufferWrapper.Resize(internalBufferLengthRequired);
            a.CopyTo(bufferWrapper.Buffer);
        }
        #endregion
        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The length of the data written.</returns>
        public override ReadResult Read(Span<float> buffer)
        {
            int channels = Channels;
            if (buffer.Length < channels) throw new InvalidOperationException($"The length of buffer is less than {channels}!");
            if (isEndOfStream) return ReadResult.EndOfStream;

            #region Initialize and Read

            //Align the length of the buffer.
            buffer = buffer.SliceAlign(Format.Channels);

            int SampleLengthOut = (int)((uint)buffer.Length / ChannelsDivisor);
            int internalBufferLengthRequired = CheckBuffer(channels, SampleLengthOut);
            if (internalBufferLengthRequired > bufferWrapper.Buffer.Length)
            {
                ExpandBuffer(internalBufferLengthRequired);
            }
            //Resampling start
            var srcBuffer = bufferWrapper.Buffer.Slice(0, internalBufferLengthRequired);
            int lengthReserved = channels * framesReserved;
            var readBuffer = srcBuffer.Slice(lengthReserved).SliceAlign(ChannelsDivisor);
            var rr = Source.Read(readBuffer);

            #endregion Initialize and Read

            if (rr.HasData || readBuffer.Length == 0)
            {
                return Process(buffer, channels, SampleLengthOut, srcBuffer, lengthReserved, readBuffer, rr);
            }
            else
            {
                if (rr.IsEndOfStream)
                {
                    isEndOfStream = true;
                    readBuffer.FastFill(0);
                    //Process the last block with silence connected behind it.
                    return Process(buffer, channels, SampleLengthOut, srcBuffer, lengthReserved, readBuffer, readBuffer.Length);
                }
                else
                {
                    return ReadResult.WaitingForSource;
                }
            }
        }

        private ReadResult Process(Span<float> buffer, int channels, int sampleLengthOut, Span<float> srcBuffer, int lengthReserved, Span<float> readBuffer, ReadResult rr)
        {
            if (rr.Length < readBuffer.Length)   //The input result was not as long as the buffer we gave
            {
                int v = sampleLengthOut * RateDiv + conversionGradient;
                var h = RateMulDivisor.DivRem((uint)v, out var b);
                int readSamples = rr.Length + lengthReserved;
                srcBuffer = srcBuffer.SliceWhile(readSamples).SliceAlign(ChannelsDivisor);
                var framesAvailable = (int)((uint)readSamples / ChannelsDivisor);
                var bA = framesAvailable - 3 - (h > 0 ? 1 : 0);
                var vA = h + bA * RateMul;
                var outLenFrames = (int)((uint)vA / RateDivDivisor);
                buffer = buffer.SliceWhile(outLenFrames * channels).SliceAlign(ChannelsDivisor);
            }
            int lastInputSampleIndex = -1;

            switch (Strategy)
            {
                case ResampleStrategy.Direct:
                    lastInputSampleIndex = ResampleDirect(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedDirect:
                    lastInputSampleIndex = ResampleCachedDirect(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedOdd:
                    lastInputSampleIndex = ResampleCachedWrappedOdd(buffer, channels, srcBuffer);
                    break;
                case ResampleStrategy.CachedWrappedEven:
                    lastInputSampleIndex = ResampleCachedWrappedEven(buffer, channels, srcBuffer);
                    break;
            }
            Span<float> reservingRegion = srcBuffer.Slice(lastInputSampleIndex * channels);
            reservingRegion.CopyTo(srcBuffer);
            framesReserved = (int)((uint)reservingRegion.Length / ChannelsDivisor);
#if false   //For Test purpose only
                int lisi_tail = lastInputSampleIndex + 3 - (conversionGradient < RateDiv ? 1 : 0);
                Console.WriteLine($"inputLength:{srcBuffer.Length}, " +
                    $"lastInputSampleIndex: {lisi_tail}(value:{srcBuffer[lisi_tail]}), " +
                    $"nextFirstSampleIndex: {lastInputSampleIndex}(value:{srcBuffer[lastInputSampleIndex]}), " +
                    $"conversionGradient: {conversionGradient}, " +
                    $"framesReserved:{framesReserved}");
#endif

            return buffer.Length;
        }

        #region Resample
        [MethodImpl(OptimizationUtils.AggressiveOptimizationIfPossible)]
        private int ResampleCachedDirectMonaural(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
#if NETCOREAPP3_1_OR_GREATER
            return ResampleCachedDirectMonauralX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
#endif
            return ResampleCachedDirectMonauralStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        private static int ResampleCachedDirectMonauralStandard(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
            nint i = 0;
            nint length = buffer.Length;
            nint isx = 0;
            nint psx = x;
            ref var src = ref MemoryMarshal.GetReference(srcBuffer);
            ref var dst = ref MemoryMarshal.GetReference(buffer);
            Vector4 values = Unsafe.As<float, Vector4>(ref src);
            if (facc > 0)
            {
                for (i = 0; i < length; i++)
                {
                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);

                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                    psx += acc;
                    bool h = psx >= ram;
                    int y = Unsafe.As<bool, byte>(ref h);
                    isx += facc;
                    isx += y;
                    psx -= -y & ram;
                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                }
            }
            else
            {
                if (acc == 1)
                {
                    switch (ram)
                    {
                        case 2:
                            {
                                var c0 = Unsafe.Add(ref coeffPtr, 0);
                                var c1 = Unsafe.Add(ref coeffPtr, 1);
                                Vector4 values2 = default;
                                if (psx > 0)
                                {
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    var cutmullCoeffs = c1;
                                    Unsafe.Add(ref dst, i++) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx = 0;
                                    isx++;

                                }
                                for (; i < length - 3; i += 4)
                                {
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    values2 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx + 1));
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                                    Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                                    Unsafe.Add(ref dst, i + 2) = VectorUtils.FastDotProduct(values2, c0);
                                    Unsafe.Add(ref dst, i + 3) = VectorUtils.FastDotProduct(values2, c1);
                                    isx += 2;
                                }
                                for (; i < length - 1; i += 2)
                                {
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                                    Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                                    isx++;
                                }
                                for (; i < length; i++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 2)
                                    {
                                        psx -= 2;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                            }
                            break;
                        case 4:
                            {
                                var c0 = coeffPtr;
                                var c1 = Unsafe.Add(ref coeffPtr, 1);
                                var c2 = Unsafe.Add(ref coeffPtr, 2);
                                var c3 = Unsafe.Add(ref coeffPtr, 3);
                                for (i = 0; psx != 0 && psx < 4; i++, psx++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 4)
                                    {
                                        psx -= 4;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                                for (; i < length - 3; i += 4)
                                {
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, c0);
                                    Unsafe.Add(ref dst, i + 1) = VectorUtils.FastDotProduct(values, c1);
                                    Unsafe.Add(ref dst, i + 2) = VectorUtils.FastDotProduct(values, c2);
                                    Unsafe.Add(ref dst, i + 3) = VectorUtils.FastDotProduct(values, c3);
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }
                                for (; i < length; i++)
                                {
                                    var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                    Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                    psx++;
                                    if (psx >= 4)
                                    {
                                        psx -= ram;
                                        isx++;
                                        values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                    }
                                }
                            }
                            break;
                        default:
                            for (i = 0; i < length; i++)
                            {
                                var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                                Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                                psx++;
                                if (psx >= ram)
                                {
                                    psx -= ram;
                                    isx++;
                                    values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                                }

                            }
                            break;
                    }
                }
                else
                {
                    for (i = 0; i < length; i++)
                    {
                        var cutmullCoeffs = Unsafe.Add(ref coeffPtr, psx);
                        Unsafe.Add(ref dst, i) = VectorUtils.FastDotProduct(values, cutmullCoeffs);
                        psx += acc;
                        if (psx >= ram)
                        {
                            psx -= ram;
                            isx++;
                            values = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref src, isx));
                        }
                    }
                }
            }
            x = (int)psx;
            return (int)isx;
        }

        private int ResampleCachedDirect2Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc)
        {
#if NETCOREAPP3_1_OR_GREATER
            return ResampleCachedDirect2ChannelsX86(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
#endif
            return ResampleCachedDirect2ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        }
        private int ResampleCachedDirect3Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc) => ResampleCachedDirect3ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        private int ResampleCachedDirect4Channels(Span<float> buffer, Span<float> srcBuffer, ref Vector4 coeffPtr, ref int x, int ram, int acc, int facc) => ResampleCachedDirect4ChannelsStandard(buffer, srcBuffer, ref coeffPtr, ref x, ram, acc, facc);
        #endregion


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void ActualDispose(bool disposing)
        {
            if (disposing)
            {
            }
            Source.Dispose();
            bufferWrapper.Dispose();
            //sampleCache = null;
        }

    }
}
