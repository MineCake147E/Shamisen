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

        public void Initialize(IWaveSource source)
        {
            if (source.Format.AudioEncoding != AudioEncoding.IeeeFloat || source.Format.BitsPerSample != 32) throw new ArgumentException("Only 32-bit IEEEFloat format is supported!", nameof(source));
            Source = source;
        }

        public void Pause()
        {
            frameInputNode.Stop();
            AudioGraph.Stop();
            PlaybackState = PlaybackState.Paused;
        }

        public void Play()
        {
            AudioGraph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        public void Resume()
        {
            AudioGraph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            frameInputNode.Stop();
            AudioGraph.Stop();
            PlaybackState = PlaybackState.Stopped;
        }

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    Stop();
                    AudioGraph.Dispose();
                    frameInputNode.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~AudioGraphOutput()
        // {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
