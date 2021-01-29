using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#if NET5_0 || NETCOREAPP3_1

using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

#endif
#if NET5_0

using System.Runtime.Intrinsics.Arm;

#endif

using System.Text;

using MonoAudio.Optimization;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Provides a function of filtering with Digital BiQuad Filter.
    /// </summary>
    public sealed partial class BiQuadFilter : IAudioFilter<float, SampleFormat>
    {
        private bool disposedValue = false; //
        private readonly bool enableIntrinsics;
        private readonly X86Intrinsics enabledX86Intrinsics;
        private readonly ArmIntrinsics enabledArmIntrinsics;

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
            Source = source ?? throw new ArgumentNullException(nameof(source));
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
        public ulong? Length { get => Source.Length; }

        /// <summary>
        /// Gets the total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> continues infinitely.
        /// </summary>
        /// <value>
        /// The total length of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? TotalLength { get => Source.TotalLength; }

        /// <summary>
        /// Gets the position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.<br />
        /// The <c>null</c> means that the <see cref="IAudioSource{TSample,TFormat}" /> doesn't support this property.
        /// </summary>
        /// <value>
        /// The position of the <see cref="IAudioSource{TSample,TFormat}" /> in frames.
        /// </value>
        public ulong? Position { get => Source.Position; }

        /// <summary>
        /// Gets the skip support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The skip support.
        /// </value>
        public ISkipSupport? SkipSupport { get => Source.SkipSupport; }

        /// <summary>
        /// Gets the seek support of the <see cref="IAudioSource{TSample,TFormat}"/>.
        /// </summary>
        /// <value>
        /// The seek support.
        /// </value>
        public ISeekSupport? SeekSupport { get => Source.SeekSupport; }

        /// <summary>
        /// Reads the audio to the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ReadResult Read(Span<float> buffer)
        {
            int channels = Format.Channels;
            buffer = buffer.SliceAlign(channels);
            ReadResult rr = Source.Read(buffer);
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
                    ProcessTriple(buffer);
                    break;
                case 4:
                    ProcessQuadruple(buffer);
                    break;
                default:
                    ProcessMultiple(buffer);
                    break;
            }
            return len;
        }

        private void ProcessQuadruple(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2[] ist = internalStates;
                Vector2 iStateL = ist[0];
                Vector2 iStateR = ist[1];
                Vector2 iStateC = ist[2];
                Vector2 iStateLFE = ist[3];
                for (int i = 0; i < buffer.Length; i += 4)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float vL = ref buffer[i];
                    ref float vR = ref buffer[i + 1];
                    ref float vC = ref buffer[i + 2];
                    ref float vLFE = ref buffer[i + 3];
                    var feedForwardL = vL * factorB; //Multiply in one go
                    var feedForwardR = vR * factorB;
                    var feedForwardC = vC * factorB;
                    var feedForwardLFE = vLFE * factorB;
                    var sumL = vL = feedForwardL.X + iStateL.X;
                    var sumR = vR = feedForwardR.X + iStateR.X;
                    var sumC = vC = feedForwardC.X + iStateC.X;
                    var sumLFE = vLFE = feedForwardLFE.X + iStateLFE.X;
                    var feedBackL = sumL * factorA;  //Multiply in one go
                    var feedBackR = sumR * factorA;
                    var feedBackC = sumC * factorA;
                    var feedBackLFE = sumLFE * factorA;
                    var aYL = iStateL.Y;   //Needed backup
                    var aYR = iStateR.Y;
                    var aYC = iStateC.Y;
                    var aYLFE = iStateLFE.Y;
                    iStateL = new Vector2(feedForwardL.Y + aYL + feedBackL.X, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + aYR + feedBackR.X, feedForwardR.Z + feedBackR.Y);
                    iStateC = new Vector2(feedForwardC.Y + aYC + feedBackC.X, feedForwardC.Z + feedBackC.Y);
                    iStateLFE = new Vector2(feedForwardLFE.Y + aYLFE + feedBackLFE.X, feedForwardLFE.Z + feedBackLFE.Y);
                }
                ist[0] = iStateL;
                ist[1] = iStateR;
                ist[2] = iStateC;
                ist[3] = iStateLFE;
            }
        }

        private void ProcessTriple(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2[] ist = internalStates;
                Vector2 iStateL = ist[0];
                Vector2 iStateR = ist[1];
                Vector2 iStateC = ist[2];
                for (int i = 0; i < buffer.Length; i += 3)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float vL = ref buffer[i];
                    ref float vR = ref buffer[i + 1];
                    ref float vC = ref buffer[i + 2];
                    var feedForwardL = vL * factorB; //Multiply in one go
                    var feedForwardR = vR * factorB;
                    var feedForwardC = vC * factorB;
                    var sumL = vL = feedForwardL.X + iStateL.X;
                    var sumR = vR = feedForwardR.X + iStateR.X;
                    var sumC = vC = feedForwardC.X + iStateC.X;
                    var feedBackL = sumL * factorA;  //Multiply in one go
                    var feedBackR = sumR * factorA;
                    var feedBackC = sumC * factorA;
                    var aYL = iStateL.Y;   //Needed backup
                    var aYR = iStateR.Y;
                    var aYC = iStateC.Y;
                    iStateL = new Vector2(feedForwardL.Y + aYL + feedBackL.X, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + aYR + feedBackR.X, feedForwardR.Z + feedBackR.Y);
                    iStateC = new Vector2(feedForwardC.Y + aYC + feedBackC.X, feedForwardC.Z + feedBackC.Y);
                }
                ist[0] = iStateL;
                ist[1] = iStateR;
                ist[2] = iStateC;
            }
        }

        private void ProcessStereo(Span<float> buffer)
        {
#if NET5_0 || NETCOREAPP3_1
            if (enableIntrinsics)
            {
                if (Avx.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Avx))
                {
                    ProcessStereoAvx(buffer);
                    return;
                }
                else if (Sse.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse))
                {
                    ProcessStereoSse(buffer);
                    return;
                }
            }
#endif
            ProcessStereoOrdinal(buffer);
        }

        private void ProcessStereoOrdinal(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2[] ist = internalStates;
                Vector2 iStateL = ist[0];
                Vector2 iStateR = ist[1];
                for (int i = 0; i < buffer.Length; i += 2)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float vL = ref buffer[i];
                    ref float vR = ref buffer[i + 1];
                    var feedForwardL = vL * factorB; //Multiply in one go
                    var feedForwardR = vR * factorB;
                    var sumL = vL = feedForwardL.X + iStateL.X;
                    var sumR = vR = feedForwardR.X + iStateR.X;
                    var feedBackL = sumL * factorA;  //Multiply in one go
                    var feedBackR = sumR * factorA;
                    var aYL = iStateL.Y;   //Needed backup
                    var aYR = iStateR.Y;
                    iStateL = new Vector2(feedForwardL.Y + aYL + feedBackL.X, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + aYR + feedBackR.X, feedForwardR.Z + feedBackR.Y);
                }
                ist[0] = iStateL;
                ist[1] = iStateR;
            }
        }

        private void ProcessMonaural(Span<float> buffer)
        {
            unchecked
            {
#if NET5_0 || NETCOREAPP3_1
                if (enableIntrinsics)
                {
                    if (Avx.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Avx))
                    {
                        ProcessMonauralAvx(buffer);
                        return;
                    }
                    else if (Sse.IsSupported && enabledX86Intrinsics.HasAllFeatures(X86IntrinsicsMask.Sse))
                    {
                        ProcessMonauralSse(buffer);
                        return;
                    }
                }
#endif
                ProcessMonauralOrdinal(buffer);
            }
        }

        private void ProcessMonauralOrdinal(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2 iState = internalStates[0];
                for (int i = 0; i < buffer.Length; i++)
                {
                    //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                    //Transformed for SIMD awareness.
                    ref float v = ref buffer[i];
                    var feedForward = v * factorB; //Multiply in one go
                    var sum1 = v = feedForward.X + iState.X;
                    var feedBack = sum1 * factorA;  //Multiply in one go
                    var aY = iState.Y;   //Needed backup
                    iState = new Vector2(feedForward.Y + aY + feedBack.X, feedForward.Z + feedBack.Y);
                }
                internalStates[0] = iState;
            }
        }

        private void ProcessMultiple(Span<float> buffer)
        {
            unchecked
            {
#if NET5_0 || NETCOREAPP3_1
                if (enableIntrinsics)
                {
                    /*if (Avx.IsSupported)
                    {
                        ProcessMonauralAvx(buffer);
                        return;
                    }
                    else */
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
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Span<Vector2> iState = stackalloc Vector2[internalStates.Length];
                internalStates.AsSpan().CopyTo(iState);
                for (int i = 0; i < buffer.Length; i += iState.Length)
                {
                    ref var pos = ref buffer[i];
                    //var span = buffer.Slice(i, internalStates.Length);
                    for (int ch = 0; ch < iState.Length; ch++)
                    {
                        //Reference: https://en.wikipedia.org/wiki/Digital_biquad_filter#Transposed_Direct_form_2
                        //Transformed for SIMD awareness.
                        ref var a = ref iState[ch]; //Persist reference in order to decrease number of times of range check.
                        ref float v = ref Unsafe.Add(ref pos, ch);
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

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
