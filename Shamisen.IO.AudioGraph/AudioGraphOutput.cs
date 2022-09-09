using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;
using Windows.Media.MediaProperties;
using Windows.Media.Render;

using WinRT;

namespace Shamisen.IO.WinRt
{
    /// <summary>
    /// Provides an audio output to <see cref="AudioGraph"/>.
    /// </summary>
    public sealed class AudioGraphOutput : ISoundOut
    {
        private bool disposedValue = false;
        private readonly AudioFrameInputNode frameInputNode;

        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; private set; }

        internal AudioGraph Graph { get; }

        /// <inheritdoc/>
        public IWaveSource? Source { get; private set; }

        private readonly uint sampleSize;
        private readonly int sampleCap;

        private AudioGraphOutput(AudioGraph audioGraph, AudioDeviceOutputNode deviceOutputNode, IWaveSource source)
        {
            PlaybackState = PlaybackState.Stopped;
            ArgumentNullException.ThrowIfNull(audioGraph);
            Graph = audioGraph;
            Source = source;
            var nodeEncodingProperties = audioGraph.EncodingProperties;
            frameInputNode = audioGraph.CreateFrameInputNode(nodeEncodingProperties);
            frameInputNode.AddOutgoingConnection(deviceOutputNode);
            frameInputNode.Stop();
            frameInputNode.QuantumStarted += Node_QuantumStarted;
            frameInputNode.AudioFrameCompleted += FrameInputNode_AudioFrameCompleted;
            sampleSize = sizeof(float) * Graph.EncodingProperties.ChannelCount;
            sampleCap = int.MaxValue - (int)(int.MaxValue % sampleSize);
            UsedFrameBuffer = new ConcurrentQueue<(AudioFrame frame, int length)>();
            UsedFrames = new SortedList<int, Queue<AudioFrame>>();
        }

        private ConcurrentQueue<(AudioFrame frame, int length)> UsedFrameBuffer { get; }

        private SortedList<int, Queue<AudioFrame>> UsedFrames { get; }

        private void FrameInputNode_AudioFrameCompleted(AudioFrameInputNode sender, AudioFrameCompletedEventArgs args)
        {
            var frame = args.Frame;
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            {
                UsedFrameBuffer.Enqueue((frame, (int)buffer.Capacity));
            }
        }

        private void Node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            var oldMode = GCSettings.LatencyMode;
            try
            {
                //GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                var numSamplesNeeded = (uint)args.RequiredSamples;

                if (numSamplesNeeded != 0)
                {
                    var audioData = GenerateAudioData(numSamplesNeeded);
                    frameInputNode.AddFrame(audioData);
                }
            }
            finally
            {
                GCSettings.LatencyMode = oldMode;
            }
        }

        private unsafe AudioFrame GenerateAudioData(uint samples)
        {
            var samplesMCh = samples * Graph.EncodingProperties.ChannelCount;
            var bufferSize = sizeof(float) * samplesMCh;
            while (UsedFrameBuffer.TryDequeue(out var item))
            {
                if (UsedFrames.TryGetValue(item.length, out var queue))
                    queue.Enqueue(item.frame);
                else
                {
                    var newQueue = new Queue<AudioFrame>();
                    newQueue.Enqueue(item.frame);
                    UsedFrames.Add(item.length, newQueue);
                }
            }
            var sel = UsedFrames.Where(a => a.Key >= bufferSize && a.Value.Count > 0);
            var frame = sel.Any() ? sel.First().Value.Dequeue() : new AudioFrame(bufferSize);
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.ReadWrite))
            using (var reference = buffer.CreateReference())
            {
                // Get the buffer from the AudioFrame
                reference.As<IMemoryBufferByteAccess>().GetBuffer(out var dataInBytes, out var capacityInBytes);
                long u = bufferSize;
                do
                {
                    var read = FillBuffer(u > sampleCap ? sampleCap : (int)u, dataInBytes);
                    dataInBytes += read.Length;
                    u -= read.Length;
                } while (u > 0);
            }

            return frame;
        }

        private unsafe ReadResult FillBuffer(int bufferSize, byte* dataInBytes)
        {
            if (Source is null) throw new ObjectDisposedException(nameof(AudioGraphOutput));
            var span = new Span<byte>(dataInBytes, bufferSize);
            return Source.Read(span);
        }

        /// <summary>
        /// Creates the audio graph output.<br/>
        /// IMPORTANT: Only 32-bit IEEEFloat format is supported!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="category">The <see cref="AudioRenderCategory"/>.</param>
        /// <returns></returns>
        /// <exception cref="Exception">AudioGraph creation error</exception>
        public static Task<AudioGraphOutput> CreateAudioGraphOutputAsync(IWaveSource source, AudioRenderCategory category)
        {
            var format = source.Format;
            return format.Encoding != AudioEncoding.IeeeFloat || format.BitDepth != 32
                ? throw new ArgumentException("Only 32-bit IEEEFloat format is supported!", nameof(source))
                : SetupGraphAsync(source, category, 0, format, QuantumSizeSelectionMode.LowestLatency);
        }

        /// <summary>
        /// Creates the audio graph output.<br/>
        /// IMPORTANT: Only 32-bit IEEEFloat format is supported!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="category">The <see cref="AudioRenderCategory"/>.</param>
        /// <param name="desiredSamplesPerQuantum">The value set to <see cref="AudioGraphSettings.DesiredSamplesPerQuantum"/>.</param>
        /// <returns></returns>
        /// <exception cref="Exception">AudioGraph creation error</exception>
        public static Task<AudioGraphOutput> CreateAudioGraphOutputAsync(IWaveSource source, AudioRenderCategory category, int desiredSamplesPerQuantum)
        {
            var format = source.Format;
            return format.Encoding != AudioEncoding.IeeeFloat || format.BitDepth != 32
                ? throw new ArgumentException("Only 32-bit IEEEFloat format is supported!", nameof(source))
                : SetupGraphAsync(source, category, desiredSamplesPerQuantum, format, QuantumSizeSelectionMode.ClosestToDesired);
        }

        private static async Task<AudioGraphOutput> SetupGraphAsync(IWaveSource source, AudioRenderCategory category, int desiredSamplesPerQuantum, IWaveFormat format, QuantumSizeSelectionMode sizeSelectionMode)
        {
            var settings = new AudioGraphSettings(category)
            {
                QuantumSizeSelectionMode = sizeSelectionMode,
                EncodingProperties = CreateEncodingPropertiesForFormat(format)
            };
            if (sizeSelectionMode == QuantumSizeSelectionMode.ClosestToDesired)
                settings.DesiredSamplesPerQuantum = desiredSamplesPerQuantum;
            return await CreateGraphAsync(settings, source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AudioEncodingProperties CreateEncodingPropertiesForFormat(IWaveFormat format) => new()
        {
            BitsPerSample = 32,
            ChannelCount = (uint)format.Channels,
            SampleRate = (uint)format.SampleRate,
            Subtype = "Float"
        };

        private static async Task<AudioGraphOutput> CreateGraphAsync(AudioGraphSettings settings, IWaveSource source)
        {
            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
                throw new InvalidOperationException("AudioGraph creation error: " + result.Status.ToString(), result.ExtendedError);
            var deviceOutputNodeResult = await result.Graph.CreateDeviceOutputNodeAsync();
            return deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success
                ? throw new InvalidOperationException("AudioGraph creation error: " + deviceOutputNodeResult.Status.ToString(), deviceOutputNodeResult.ExtendedError)
                : new AudioGraphOutput(result.Graph, deviceOutputNodeResult.DeviceOutputNode, source);
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (PlaybackState != PlaybackState.Playing)
#if DEBUG
                throw new InvalidOperationException($"Cannot pause without playing!");
#else
                return;
#endif
            frameInputNode.Stop();
            Graph.Stop();
            PlaybackState = PlaybackState.Paused;
        }

        /// <inheritdoc/>
        public void Play()
        {
            if (PlaybackState != PlaybackState.Stopped)
            {
                Resume();
                return;
            }
            Graph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused)
#if DEBUG
                throw new InvalidOperationException($"Cannot resume without pausing!");
#else
                return;
#endif
            Graph.Start();
            frameInputNode.Start();
            PlaybackState = PlaybackState.Playing;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            frameInputNode.Stop();
            Graph.Stop();
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
                    Graph.Dispose();
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
