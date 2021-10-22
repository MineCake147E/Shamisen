using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#if NETCOREAPP3_1_OR_GREATER

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0_OR_GREATER

using System.Runtime.Intrinsics.Arm;

#endif

using System.Text;

using Shamisen.Optimization;

using System.Runtime.InteropServices;

namespace Shamisen.Filters
{
    /// <summary>
    /// Filters audio signal with single Digital BiQuad Filter.
    /// </summary>
    public sealed partial class BiQuadFilter : ISampleFilter
    {
        private bool disposedValue = false; //
        private readonly bool enableIntrinsics;
        private readonly X86Intrinsics enabledX86Intrinsics;
        private readonly ArmIntrinsics enabledArmIntrinsics;

        /// <summary>
        /// Gets the value which indicates whether the <see cref="BiQuadFilter"/> can alter results for speed, or not.
        /// </summary>
        public bool AllowFusedMultiplyAddForPerformance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiQuadFilter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public BiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter parameter) : this(source, parameter, true)
        {
        }

        internal BiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter parameter, bool enableIntrinsics)
            : this(source, parameter, enableIntrinsics, IntrinsicsUtils.X86Intrinsics, IntrinsicsUtils.ArmIntrinsics)
        {
        }

        internal BiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter parameter, bool enableIntrinsics, X86Intrinsics enabledX86Intrinsics, ArmIntrinsics enabledArmIntrinsics)
        {
            Source = source?.EnsureBlocks() ?? throw new ArgumentNullException(nameof(source));
            Parameter = parameter;
            internalStates = new Vector2[Format.Channels];
            internalStates.AsSpan().Fill(new Vector2(0, 0));
            this.enableIntrinsics = enableIntrinsics;
            this.enabledX86Intrinsics = enabledX86Intrinsics;
            this.enabledArmIntrinsics = enabledArmIntrinsics;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public IReadableAudioSource<float, SampleFormat> Source { get; }

        /// <summary>
        /// Gets the parameter.
        /// </summary>
        /// <value>
        /// The parameter.
        /// </value>
        public BiQuadParameter Parameter { get; }

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format => Source.Format;

        private Vector2[] internalStates;

        /// <summary>
        /// Gets the remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The remaining length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? Length => Source.Length;

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength => Source.TotalLength;

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? Position => Source.Position;

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport => Source.SkipSupport;

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport => Source.SeekSupport;

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<float> buffer)
        {
            var channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            var rr = Source.Read(buffer);
            if (rr.HasNoData) return rr;
            var len = rr.Length;
            buffer = buffer.Slice(0, len);
            switch (channels)
            {
                case 1:
                    ProcessMonaural(buffer);
                    break;
                case 2:
                    ProcessStereo(buffer);
                    break;
                case 3:
                    ProcessTriple(buffer, Parameter, internalStates);
                    break;
                case 4:
                    ProcessQuadruple(buffer, Parameter, internalStates);
                    break;
                default:
                    ProcessMultiple(buffer);
                    break;
            }
            return len;
        }

        private static void ProcessQuadruple(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            var factorB = parameter.B;
            var factorA = parameter.A;
            var ist = states;
            var iStateLFE = ist[3];
            var iStateC = ist[2];
            var iStateR = ist[1];
            var iStateL = ist[0];
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var fBX = new Vector4(factorB.X);
            var fBY = new Vector4(factorB.Y);
            var fBZ = new Vector4(factorB.Z);
            var fAX = new Vector4(factorA.X);
            var fAY = new Vector4(factorA.Y);
            var iSX = new Vector4(iStateL.X, iStateR.X, iStateC.X, iStateLFE.X);
            var iSY = new Vector4(iStateL.Y, iStateR.Y, iStateC.Y, iStateLFE.Y);
            nint i = 0, length = buffer.Length;
            var olen = length - 7;
            for (; i < olen; i += 8)
            {
                var input = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i));
                var s0 = fBX * input;
                s0 += iSX;
                iSX = fBY * input;
                iSY = iSX + iSY;
                iSX = fBZ * input;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i)) = s0;
                input = fAX * s0;
                iSY += input;
                s0 = fAY * s0;
                s0 = iSX + s0;
                iSX = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4));
                input = fBX * iSX;
                iSY = input + iSY;
                input = fBY * iSX;
                s0 = input + s0;
                input = fBZ * iSX;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i + 4)) = iSY;
                iSX = fAX * iSY;
                iSX = s0 + iSX;
                s0 = fAY * iSY;
                iSY = input + s0;
            }
            if (i < length)
            {
                var s0 = Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i));
                fBX *= s0;
                fBX += iSX;
                Unsafe.As<float, Vector4>(ref Unsafe.Add(ref rdi, i)) = fBX;
                fBZ *= s0;
                fAY *= fBX;
                s0 = fBY * s0;
                s0 += iSY;
                iSY = fBZ + fAY;
                fAX *= fBX;
                iSX = s0 + fAX;
            }
            iStateL = new Vector2(iSX.X, iSY.X);
            iStateR = new Vector2(iSX.Y, iSY.Y);
            iStateC = new Vector2(iSX.Z, iSY.Z);
            iStateLFE = new Vector2(iSX.W, iSY.W);
            states[0] = iStateL;
            states[1] = iStateR;
            states[2] = iStateC;
            states[2] = iStateLFE;
        }

        private static void ProcessTriple(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            var factorB = parameter.B;
            var factorA = parameter.A;
            var ist = states;
            var iStateC = ist[2];
            var iStateL = ist[0];
            var iStateR = ist[1];
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var fBX = new Vector3(factorB.X);
            var fBY = new Vector3(factorB.Y);
            var fBZ = new Vector3(factorB.Z);
            var fAX = new Vector3(factorA.X);
            var fAY = new Vector3(factorA.Y);
            var iSX = new Vector3(iStateL.X, iStateR.X, iStateC.X);
            var iSY = new Vector3(iStateL.Y, iStateR.Y, iStateC.Y);
            nint i = 0, length = buffer.Length;
            var olen = length - 5;
            for (; i < olen; i += 6)
            {
                var input = Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i));
                var s0 = fBX * input;
                s0 += iSX;
                iSX = fBY * input;
                iSY = iSX + iSY;
                iSX = fBZ * input;
                Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i)) = s0;
                input = fAX * s0;
                iSY += input;
                s0 = fAY * s0;
                s0 = iSX + s0;
                iSX = Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i + 3));
                input = fBX * iSX;
                iSY = input + iSY;
                input = fBY * iSX;
                s0 = input + s0;
                input = fBZ * iSX;
                Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i + 3)) = iSY;
                iSX = fAX * iSY;
                iSX = s0 + iSX;
                s0 = fAY * iSY;
                iSY = input + s0;
            }
            if (i < length)
            {
                var s0 = Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i));
                fBX *= s0;
                fBX += iSX;
                Unsafe.As<float, Vector3>(ref Unsafe.Add(ref rdi, i)) = fBX;
                fBZ *= s0;
                fAY *= fBX;
                s0 = fBY * s0;
                s0 += iSY;
                iSY = fBZ + fAY;
                fAX *= fBX;
                iSX = s0 + fAX;
            }
            iStateL = new Vector2(iSX.X, iSY.X);
            iStateR = new Vector2(iSX.Y, iSY.Y);
            iStateC = new Vector2(iSX.Z, iSY.Z);
            states[0] = iStateL;
            states[1] = iStateR;
            states[2] = iStateC;
        }

        private void ProcessStereo(Span<float> buffer) => ProcessStereoStandard(buffer, Parameter, internalStates);

        private static void ProcessStereoStandard(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            var factorB = parameter.B;
            var factorA = parameter.A;
            var ist = states;
            var iStateR = ist[1];
            var iStateL = ist[0];
            ref var rdi = ref MemoryMarshal.GetReference(buffer);
            var fBX = new Vector2(factorB.X);
            var fBY = new Vector2(factorB.Y);
            var fBZ = new Vector2(factorB.Z);
            var fAX = new Vector2(factorA.X);
            var fAY = new Vector2(factorA.Y);
            var iSX = new Vector2(iStateL.X, iStateR.X);
            var iSY = new Vector2(iStateL.Y, iStateR.Y);
            nint i = 0, length = buffer.Length;
            var olen = length - 3;
            for (; i < olen; i += 4)
            {
                var input = Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i));
                var s0 = fBX * input;
                s0 += iSX;
                iSX = fBY * input;
                iSY = iSX + iSY;
                iSX = fBZ * input;
                Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i)) = s0;
                input = fAX * s0;
                iSY += input;
                s0 = fAY * s0;
                s0 = iSX + s0;
                iSX = Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i + 2));
                input = fBX * iSX;
                iSY = input + iSY;
                input = fBY * iSX;
                s0 = input + s0;
                input = fBZ * iSX;
                Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i + 2)) = iSY;
                iSX = fAX * iSY;
                iSX = s0 + iSX;
                s0 = fAY * iSY;
                iSY = input + s0;
            }
            if (i < length)
            {
                var s0 = Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i));
                fBX *= s0;
                fBX += iSX;
                Unsafe.As<float, Vector2>(ref Unsafe.Add(ref rdi, i)) = fBX;
                fBZ *= s0;
                fAY *= fBX;
                s0 = fBY * s0;
                s0 += iSY;
                iSY = fBZ + fAY;
                fAX *= fBX;
                iSX = s0 + fAX;
            }
            iStateL = new Vector2(iSX.X, iSY.X);
            iStateR = new Vector2(iSX.Y, iSY.Y);
            states[0] = iStateL;
            states[1] = iStateR;
        }

        private void ProcessMonaural(Span<float> buffer) => ProcessMonauralStandard(buffer, Parameter, internalStates);

        internal static void ProcessMonauralStandard(Span<float> buffer, BiQuadParameter parameter, Span<Vector2> states)
        {
            unsafe
            {
                var factorB = parameter.B;
                var factorA = parameter.A;
                var iState = states[0];
                ref var rdi = ref MemoryMarshal.GetReference(buffer);
                var fBX = factorB.X;
                var fBY = factorB.Y;
                var fBZ = factorB.Z;
                var fAX = factorA.X;
                var fAY = factorA.Y;
                var iSX = iState.X;
                var iSY = iState.Y;
                //LLVM had me to do everything in scalar
                nint i, length = buffer.Length;
                for (i = 0; i < length - 1; i += 2)
                {
                    var input = Unsafe.Add(ref rdi, i);
                    var s0 = fBX * input;
                    s0 += iSX;
                    iSX = fBY * input;
                    iSY = iSX + iSY;
                    iSX = fBZ * input;
                    Unsafe.Add(ref rdi, i) = s0;
                    input = fAX * s0;
                    iSY += input;
                    s0 = fAY * s0;
                    s0 = iSX + s0;
                    iSX = Unsafe.Add(ref rdi, i + 1);
                    input = fBX * iSX;
                    iSY = input + iSY;
                    input = fBY * iSX;
                    s0 = input + s0;
                    input = fBZ * iSX;
                    Unsafe.Add(ref rdi, i + 1) = iSY;
                    iSX = fAX * iSY;
                    iSX = s0 + iSX;
                    s0 = fAY * iSY;
                    iSY = input + s0;
                }
                if (i < length)
                {
                    var s0 = Unsafe.Add(ref rdi, i);
                    fBX *= s0;
                    fBX += iSX;
                    Unsafe.Add(ref rdi, i) = fBX;
                    fBZ *= s0;
                    fAY *= fBX;
                    s0 = fBY * s0;
                    s0 += iSY;
                    iSY = fBZ + fAY;
                    fAX *= fBX;
                    iSX = s0 + fAX;
                }
                iState = new Vector2(iSX, iSY);
                states[0] = iState;
            }
        }

        private void ProcessMultiple(Span<float> buffer)
        {
            unchecked
            {
#if NETCOREAPP3_1_OR_GREATER
                if (enableIntrinsics)
                {
                    if (Avx.IsSupported)
                    {
                        ProcessMultipleAvx(buffer, Parameter, internalStates);
                        return;
                    }
                    else
                    if (Sse.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse))
                    {
                        ProcessMultipleSse(buffer);
                        return;
                    }
                }
#endif
                ProcessMultipleOrdinal(buffer);
            }
        }

        private void ProcessMultipleOrdinal(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                var factorB = Parameter.B;
                var factorA = Parameter.A;
                Span<Vector2> iState = stackalloc Vector2[internalStates.Length];
                var channels = iState.Length;
                internalStates.AsSpan().CopyTo(iState);
                for (var i = 0; i < buffer.Length - channels + 1; i += channels)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (var ch = 0; ch < iState.Length; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref iState[ch]; //Persist reference in order to decrease number of times of range check.
                        ref var v = ref Unsafe.Add(ref pos, ch);
                        var feedForward = v * factorB; //Multiply in one go
                        var sum1 = v = feedForward.X + a.X;
                        var feedBack = sum1 * factorA;  //Multiply in one go
                        var aY = a.Y;   //Needed backup
                        a = new Vector2(feedForward.Y + aY + feedBack.X, feedForward.Z + feedBack.Y);
                    }
                }
                iState.CopyTo(internalStates.AsSpan());
            }
        }

        #region IDisposable Support

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                internalStates = Array.Empty<Vector2>();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BiQuadFilter"/> class.
        /// </summary>
        ~BiQuadFilter() => Dispose(false);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
