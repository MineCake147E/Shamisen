using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Formats;

using System.Numerics;

#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace Shamisen.Conversion.WaveToSampleConverters
{
    /// <summary>
    /// Converts 32-bit Ranged Linear PCM to IEEE 754 Single Precision Floating-Point Sample.
    /// </summary>
    public sealed partial class RangedPcm32ToSampleConverter : IAudioConverter<int, Int32RangedLinearPcmSampleFormat, float, SampleFormat>, ISampleSource
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangedPcm32ToSampleConverter"/> class.
        /// </summary>
        /// <param name="source"></param>
        public RangedPcm32ToSampleConverter(IReadableAudioSource<int, Int32RangedLinearPcmSampleFormat> source)
        {
            Source = source;
            Format = new(source.Format.Channels, source.Format.SampleRate);
        }

        /// <inheritdoc/>
        public IReadableAudioSource<int, Int32RangedLinearPcmSampleFormat> Source { get; }
        /// <inheritdoc/>
        public SampleFormat Format { get; }
        /// <inheritdoc/>
        public ulong? Length => Source.Length;
        /// <inheritdoc/>
        public ulong? TotalLength => Source.TotalLength;
        /// <inheritdoc/>
        public ulong? Position => Source.Position;
        /// <inheritdoc/>
        public ISkipSupport? SkipSupport => Source.SkipSupport;
        /// <inheritdoc/>
        public ISeekSupport? SeekSupport => Source.SeekSupport;

        /// <inheritdoc/>
        public ReadResult Read(Span<float> buffer)
        {
            //Since sizeof(float) is the same as sizeof(int), no external buffer is required.
            var rr = Source.Read(MemoryMarshal.Cast<float, int>(buffer));
            if (rr.HasNoData)
            {
                return rr;
            }
            buffer = buffer.SliceWhile(rr.Length / 4);
            Process(buffer, Source.Format.EffectiveBitDepth);
            return buffer.Length;
        }

        private static void Process(Span<float> buffer, int effectiveBitDepth)
        {
            if (effectiveBitDepth >= 32)
            {
                Pcm32ToSampleConverter.ProcessNormal(buffer);
            }
            else
            {
#if NETCOREAPP3_1_OR_GREATER
                if (X86.IsSupported)
                {
                    X86.Process(buffer, effectiveBitDepth);
                    return;
                }
#endif
                ProcessEMoreThan24Standard(buffer, effectiveBitDepth);
            }
        }

        [MethodImpl(OptimizationUtils.InlineAndOptimizeIfPossible)]
        internal static void ProcessEMoreThan24Standard(Span<float> buffer, int effectiveBitDepth)
        {
            Vector<float> mul = new(2.0f / (1 << effectiveBitDepth));
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            ref var rsi = ref Unsafe.As<float, int>(ref rdi);
            nint i, length = buffer.Length;
            var size = Vector<float>.Count;
            for (i = 0; i < length - 8 * Vector<float>.Count + 1; i += 8 * Vector<float>.Count)
            {
                var v0_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 0)));
                var v1_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 1)));
                var v2_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 2)));
                var v3_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 3)));
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 0)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 1)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 2)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 3)) = v3_ns;
                v0_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 4)));
                v1_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 5)));
                v2_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 6)));
                v3_ns = Vector.ConvertToSingle(Unsafe.As<float, Vector<int>>(ref Unsafe.Add(ref rdi, i + size * 7)));
                v0_ns *= mul;
                v1_ns *= mul;
                v2_ns *= mul;
                v3_ns *= mul;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 4)) = v0_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 5)) = v1_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 6)) = v2_ns;
                Unsafe.As<float, Vector<float>>(ref Unsafe.Add(ref rdi, i + size * 7)) = v3_ns;
            }
            for (; i < length; i++)
            {
                Unsafe.Add(ref rdi, i) = Unsafe.Add(ref rsi, i) * mul[0];
            }
        }
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                disposedValue = true;
            }
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
