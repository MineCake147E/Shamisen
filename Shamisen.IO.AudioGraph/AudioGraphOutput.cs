using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using DivideSharp;

using Shamisen.Utils;

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
        private UInt32Divisor sampleSizeDivisor;

        private AudioGraphOutput(AudioGraph audioGraph, AudioDeviceOutputNode deviceOutputNode, IWaveSource source, int desiredSamplesPerQuantum)
        {
            PlaybackState = PlaybackState.Stopped;
            ArgumentNullException.ThrowIfNull(audioGraph);
            Graph = audioGraph;
            Source = source;
            var nodeEncodingProperties = audioGraph.EncodingProperties;
            sampleSize = sizeof(float) * nodeEncodingProperties.ChannelCount;
            sampleCap = int.MaxValue - (int)(int.MaxValue % sampleSize);
            frameInputNode = audioGraph.CreateFrameInputNode(nodeEncodingProperties);
            frameInputNode.AddOutgoingConnection(deviceOutputNode);
            frameInputNode.Stop();
            frameInputNode.QuantumStarted += Node_QuantumStarted;
            frameInputNode.AudioFrameCompleted += FrameInputNode_AudioFrameCompleted;
            UsedFrameBuffer = [];
            EnqueueFrame(new AudioFrame(sampleSize * (uint)desiredSamplesPerQuantum));
            sampleSizeDivisor = new(sampleSize);
        }

        private ConcurrentBag<(AudioFrame frame, AudioBuffer buffer, IMemoryBufferReference reference, Pointer<byte> dataInBytes, uint capacityInBytes)> UsedFrameBuffer { get; }

        private void FrameInputNode_AudioFrameCompleted(AudioFrameInputNode sender, AudioFrameCompletedEventArgs args) => EnqueueFrame(args.Frame);

        private unsafe void EnqueueFrame(AudioFrame frame)
        {
            // These operations take long time so we do them asynchronously to avoid blocking the audio rendering thread.
            var buffer = frame.LockBuffer(AudioBufferAccessMode.Write);
            var reference = buffer.CreateReference();
            reference.As<IMemoryBufferByteAccess>().GetBuffer(out var dataInBytes, out var capacityInBytes);
            UsedFrameBuffer.Add((frame, buffer, reference, Pointer.Create(dataInBytes), capacityInBytes));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private unsafe void Node_QuantumStarted(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            var source = Source;
            ObjectDisposedException.ThrowIf(source is null, this);
            var localSampleSize = sampleSize;
            var divisor = sampleSizeDivisor;
            var sampleCap1 = sampleCap;
            var requiredSamples = (uint)args.RequiredSamples;
            long remainingSamplesToFill = requiredSamples;
            while (remainingSamplesToFill > 0)
            {
                var bufferSize = localSampleSize * (uint)remainingSamplesToFill;
                AudioFrame? frame = null;
                AudioBuffer? buffer = null;
                IMemoryBufferReference? reference = null;
                byte* dataInBytes = null;
                uint capacityInBytes = 0;
                if (UsedFrameBuffer.TryTake(out var f))
                {
                    frame = f.frame;
                    buffer = f.buffer;
                    reference = f.reference;
                    dataInBytes = f.dataInBytes;
                    capacityInBytes = f.capacityInBytes;
                }
                if (frame is null)
                {
                    frame = new AudioFrame(bufferSize);
                    buffer = frame.LockBuffer(AudioBufferAccessMode.Write);
                    reference = buffer.CreateReference();
                    reference.As<IMemoryBufferByteAccess>().GetBuffer(out dataInBytes, out capacityInBytes);
                }
                using (buffer)
                using (reference)
                {
                    // Get the buffer from the AudioFrame
                    var remainder = divisor.DivRem(capacityInBytes, out var samples);
                    long u = capacityInBytes - remainder;
                    while (u > 0)
                    {
                        var span = new Span<byte>(dataInBytes, u > sampleCap1 ? sampleCap1 : (int)u);
                        var read = source.Read(span);
                        dataInBytes += read.Length;
                        u -= read.Length;
                        if (read.HasNoData) break;
                    }
                    remainingSamplesToFill -= samples;
                }
                frameInputNode.AddFrame(frame);
            }
        }

        /// <summary>
        /// Creates the audio graph output.<br/>
        /// IMPORTANT: Only 32-bit IEEEFloat format is supported!
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="category">The <see cref="AudioRenderCategory"/>.</param>
        /// <returns></returns>
        /// <exception cref="Exception">AudioGraph creation error</exception>
        public static Task<AudioGraphOutput> CreateLowestLatencyAudioGraphOutputAsync(IWaveSource source, AudioRenderCategory category)
        {
            var format = source.Format;
            return format.Encoding != AudioEncoding.IeeeFloat || format.BitDepth != 32
                ? throw new ArgumentException("Only 32-bit IEEEFloat format is supported!", nameof(source))
                : SetupGraphAsync(source, category, Math.Max(format.SampleRate / 1000, 128), format, QuantumSizeSelectionMode.LowestLatency);
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
                : SetupGraphAsync(source, category, Math.Max(format.SampleRate / 200, 128), format, QuantumSizeSelectionMode.ClosestToDesired);
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
                MaxPlaybackSpeedFactor = 1,
                QuantumSizeSelectionMode = sizeSelectionMode,
                EncodingProperties = CreateEncodingPropertiesForFormat(format)
            };
            if (sizeSelectionMode == QuantumSizeSelectionMode.ClosestToDesired)
                settings.DesiredSamplesPerQuantum = desiredSamplesPerQuantum;
            return await CreateGraphAsync(settings, source, desiredSamplesPerQuantum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static AudioEncodingProperties CreateEncodingPropertiesForFormat(IWaveFormat format) => new()
        {
            BitsPerSample = 32,
            ChannelCount = (uint)format.Channels,
            SampleRate = (uint)format.SampleRate,
            Subtype = "Float"
        };

        private static async Task<AudioGraphOutput> CreateGraphAsync(AudioGraphSettings settings, IWaveSource source, int desiredSamplesPerQuantum)
        {
            var result = await AudioGraph.CreateAsync(settings);
            if (result.Status != AudioGraphCreationStatus.Success)
                throw new InvalidOperationException("AudioGraph creation error: " + result.Status.ToString(), result.ExtendedError);
            var deviceOutputNodeResult = await result.Graph.CreateDeviceOutputNodeAsync();
            return deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success
                ? throw new InvalidOperationException("AudioGraph creation error: " + deviceOutputNodeResult.Status.ToString(), deviceOutputNodeResult.ExtendedError)
                : new AudioGraphOutput(result.Graph, deviceOutputNodeResult.DeviceOutputNode, source, desiredSamplesPerQuantum);
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
            if (PlaybackState != PlaybackState.Playing) return;
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
                    frameInputNode.Dispose();
                    Graph.Dispose();
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
