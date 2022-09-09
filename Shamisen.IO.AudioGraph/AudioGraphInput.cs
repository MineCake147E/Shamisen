using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;

using WinRT;

namespace Shamisen.IO.WinRt
{
    /// <summary>
    /// Provides an audio input from <see cref="AudioGraph"/>.
    /// </summary>
    public sealed partial class AudioGraphInput : ISoundIn
    {
        private bool disposedValue;

        private AudioGraph? graph;

        private AudioFrameOutputNode? frameOutputNode;
        private IAudioInputNode? inputNode;

        /// <summary>
        /// Initializes a new instance of <see cref="AudioGraphInput"/> with specified <paramref name="audioGraph"/>.
        /// </summary>
        /// <param name="audioGraph">The audio graph.</param>
        /// <param name="inputNode">The input node.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public AudioGraphInput(AudioGraph audioGraph, IAudioInputNode inputNode)
        {
            ArgumentNullException.ThrowIfNull(audioGraph);
            graph = audioGraph;
            ArgumentNullException.ThrowIfNull(inputNode);
            this.inputNode = inputNode;
            var outputNode = audioGraph.CreateFrameOutputNode();
            inputNode.AddOutgoingConnection(outputNode);
            frameOutputNode = outputNode;
            var encodingProperties = outputNode.EncodingProperties;
            Format = new WaveFormat((int)encodingProperties.SampleRate, (int)encodingProperties.BitsPerSample, (int)encodingProperties.ChannelCount, AudioEncoding.IeeeFloat);
            audioGraph.QuantumStarted += AudioGraph_QuantumStarted;
        }

        private void AudioGraph_QuantumStarted(AudioGraph sender, object args)
        {
            using var f = frameOutputNode?.GetFrame();
            if (f is not null) ProcessFrameOutput(f);
        }

        private unsafe void ProcessFrameOutput(AudioFrame frame)
        {
            using (var buffer = frame.LockBuffer(AudioBufferAccessMode.ReadWrite))
            using (var reference = buffer.CreateReference())
            {
                reference.As<IMemoryBufferByteAccess>().GetBuffer(out var dataInBytes, out var capacityInBytes);
                DataAvailable?.Invoke(this, new(new(dataInBytes, (int)capacityInBytes)));
            }
        }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }
        /// <inheritdoc/>
        public RecordingState RecordingState { get; }

        /// <inheritdoc/>
        public event DataAvailableEventHandler? DataAvailable;
        /// <inheritdoc/>
        public event EventHandler<StoppedEventArgs>? Stopped;

        /// <inheritdoc/>
        public void Start() => frameOutputNode?.Start();
        /// <inheritdoc/>
        public void Stop() => frameOutputNode?.Stop();

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                if (frameOutputNode is not null && inputNode is not null)
                {
                    inputNode.RemoveOutgoingConnection(frameOutputNode);
                    frameOutputNode.Dispose();
                }
                frameOutputNode = null;
                inputNode = null;
                graph = null;
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~AudioGraphInput()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
