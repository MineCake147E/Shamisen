using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MonoAudio.Filters
{
    /// <summary>
    /// Provides a function of filtering with Digital BiQuad Filter.
    /// </summary>
    public sealed class BiQuadFilter : IAudioFilter<float, SampleFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BiQuadFilter"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parameter">The parameter.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public BiQuadFilter(IReadableAudioSource<float, SampleFormat> source, BiQuadParameter parameter)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Parameter = parameter;
            internalStates = new Vector2[Format.Channels];
            internalStates.AsSpan().Fill(new Vector2(0, 0));
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
        /// Gets a value indicating whether this instance can seek.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can seek; otherwise, <c>false</c>.
        /// </value>
        public bool CanSeek => throw new NotImplementedException();

        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public SampleFormat Format => Source.Format;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public long Position { get => Source.Position; set => Source.Position = value; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public long Length => Source.Length;

        private Vector2[] internalStates;

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
                    ProcessDouble(buffer);
                    break;
                case 3:
                    ProcessTriple(buffer);
                    break;
                case 4:
                    ProcessQuadruple(buffer);
                    break;
                default:
                    ProcessOrdinal(buffer, channels);
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
                Vector2 iStateL = internalStates[0];
                Vector2 iStateR = internalStates[1];
                Vector2 iStateC = internalStates[2];
                Vector2 iStateLFE = internalStates[3];
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
                    iStateL = new Vector2(feedForwardL.Y + feedBackL.X + aYL, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + feedBackR.X + aYR, feedForwardR.Z + feedBackR.Y);
                    iStateC = new Vector2(feedForwardC.Y + feedBackC.X + aYC, feedForwardC.Z + feedBackC.Y);
                    iStateLFE = new Vector2(feedForwardLFE.Y + feedBackLFE.X + aYLFE, feedForwardLFE.Z + feedBackLFE.Y);
                }
                internalStates[0] = iStateL;
                internalStates[1] = iStateR;
                internalStates[2] = iStateC;
                internalStates[3] = iStateLFE;
            }
        }

        private void ProcessTriple(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2 iStateL = internalStates[0];
                Vector2 iStateR = internalStates[1];
                Vector2 iStateC = internalStates[2];
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
                    iStateL = new Vector2(feedForwardL.Y + feedBackL.X + aYL, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + feedBackR.X + aYR, feedForwardR.Z + feedBackR.Y);
                    iStateC = new Vector2(feedForwardC.Y + feedBackC.X + aYC, feedForwardC.Z + feedBackC.Y);
                }
                internalStates[0] = iStateL;
                internalStates[1] = iStateR;
                internalStates[2] = iStateC;
            }
        }

        private void ProcessDouble(Span<float> buffer)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Vector2 iStateL = internalStates[0];
                Vector2 iStateR = internalStates[1];
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
                    iStateL = new Vector2(feedForwardL.Y + feedBackL.X + aYL, feedForwardL.Z + feedBackL.Y);
                    iStateR = new Vector2(feedForwardR.Y + feedBackR.X + aYR, feedForwardR.Z + feedBackR.Y);
                }
                internalStates[0] = iStateL;
                internalStates[1] = iStateR;
            }
        }

        private void ProcessMonaural(Span<float> buffer)
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
                    iState = new Vector2(feedForward.Y + feedBack.X + aY, feedForward.Z + feedBack.Y);
                }
                internalStates[0] = iState;
            }
        }

        private void ProcessOrdinal(Span<float> buffer, int channels)
        {
            unsafe
            {
                //Factor localization greatly improved performance
                Vector3 factorB = Parameter.B;
                Vector2 factorA = Parameter.A;
                Span<Vector2> iState = stackalloc Vector2[internalStates.Length];
                internalStates.AsSpan().CopyTo(iState);
                for (int i = 0; i < buffer.Length; i += channels)
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
                        a = new Vector2(feedForward.Y + feedBack.X + aY, feedForward.Z + feedBack.Y);
                    }
                }
                iState.CopyTo(internalStates.AsSpan());
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; //

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
                Source.Dispose();
                internalStates = null;
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
