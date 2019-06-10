using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

namespace MonoAudio.SoundOut
{
    public sealed class AudioGraphOutput : ISoundOut
    {
        private bool disposedValue = false;
        private readonly AudioFrameInputNode frameInputNode;

        public PlaybackState PlaybackState { get; private set; }

        internal AudioGraph AudioGraph { get; }

        public IWaveSource Source { get; private set; }

        private readonly uint sampleSize;
        private readonly int sampleCap;

        private AudioGraphOutput(AudioGraph audioGraph)
        {
            AudioGraph = audioGraph ?? throw new ArgumentNullException(nameof(audioGraph));
            AudioEncodingProperties nodeEncodingProperties = audioGraph.EncodingProperties;
            nodeEncodingProperties.ChannelCount = 1;
            frameInputNode = audioGraph.CreateFrameInputNode(nodeEncodingProperties);

            // Initialize the Frame Input Node in the stopped state
            frameInputNode.Stop();

            // Hook up an event handler so we can start generating samples when needed
            // This event is triggered when the node is required to provide data
            frameInputNode.QuantumStarted += Node_QuantumStarted;
            sampleSize = sizeof(float) * AudioGraph.EncodingProperties.ChannelCount;
            sampleCap = int.MaxValue - (int)(int.MaxValue % sampleSize);
        }

        private void Node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            uint numSamplesNeeded = (uint)args.RequiredSamples;

            if (numSamplesNeeded != 0)
            {
                AudioFrame audioData = GenerateAudioData(numSamplesNeeded);
                frameInputNode.AddFrame(audioData);
            }
        }

        private unsafe AudioFrame GenerateAudioData(uint samples)
        {
            uint samplesMCh = samples * AudioGraph.EncodingProperties.ChannelCount;
            uint bufferSize = sizeof(float) * samplesMCh;
            AudioFrame frame = new AudioFrame(bufferSize);

            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out byte* dataInBytes, out uint capacityInBytes);
                long u = bufferSize;
                do
                {
                    var read = FillBuffer(u > sampleCap ? sampleCap : (int)u, dataInBytes);
                    dataInBytes += read;
                    u -= read;
                } while (u > 0);
            }

            return frame;
        }

        private unsafe int FillBuffer(int bufferSize, byte* dataInBytes)
        {
            var span = new Span<byte>(dataInBytes, bufferSize);
            return Source.Read(span);
        }

        /// <summary>
        /// Creates the audio graph output.<br/>
        /// IMPORTANT: Only 32-bit IEEEFloat format is supported!
        /// </summary>
        /// <param name="ChannelCount">The number of channels. Default: 2(Stereo)</param>
        /// <param name="SampleRate">The sample rate. Default: 192000Hz</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">AudioGraph creation error: " + result.Status.ToString()</exception>
        public static async Task<AudioGraphOutput> CreateAudioGraphOutput(uint ChannelCount = 2, uint SampleRate = 192000)
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media)
            {
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.ClosestToDesired,
                EncodingProperties = new AudioEncodingProperties()
                {
                    BitsPerSample = 32,
                    ChannelCount = ChannelCount,
                    SampleRate = SampleRate
                }
            };

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
            {
                throw new Exception("AudioGraph creation error: " + result.Status.ToString(), result.ExtendedError);
            }

            return new AudioGraphOutput(result.Graph);
        }

        /// <summary>
        /// Initializes the <see cref="T:MonoAudio.SoundOut.ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        /// <exception cref="System.ArgumentException">Only 32-bit IEEEFloat format is supported! - source</exception>
        public void Initialize(IWaveSource source)
        {
            if (source.Format.WaveFormatTag != AudioEncoding.IeeeFloat || source.Format.BitsPerSample != 32) throw new ArgumentException("Only 32-bit IEEEFloat format is supported!", nameof(source));
            Source = source;
        }

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause()
        {
            frameInputNode.Stop();
            AudioGraph.Stop();
            PlaybackState = PlaybackState.Paused;
        }

        /// <summary>
        ///
        /// </summary>
        public void Play()
        {
            AudioGraph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            AudioGraph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            frameInputNode.Stop();
            AudioGraph.Stop();
            PlaybackState = PlaybackState.Stopped;
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
                    // Release managed objects.
                    Stop();
                    AudioGraph.Dispose();
                    frameInputNode.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
