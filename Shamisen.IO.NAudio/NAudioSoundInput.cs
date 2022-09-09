using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio;
using NAudio.Wave;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides a functionality of recording audio data from <see cref="IWaveIn"/>.
    /// </summary>
    public sealed class NAudioSoundInput : ISoundIn
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public event EventHandler<StoppedEventArgs>? Stopped;
        /// <inheritdoc/>
        public event DataAvailableEventHandler? DataAvailable;

        /// <summary>
        /// Initializes a new instance of <see cref="NAudioSoundInput"/>.
        /// </summary>
        /// <param name="waveIn"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NAudioSoundInput(IWaveIn waveIn)
        {
            ArgumentNullException.ThrowIfNull(waveIn);
            WaveIn = waveIn;
            Format = WaveIn.WaveFormat.AsShamisenWaveFormat();
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;
            RecordingState = RecordingState.Stopped;
        }

        /// <summary>
        /// The value which indicates whether to suppress raising <see cref="Stopped"/> when <see cref="WaveIn_RecordingStopped(object?, NAudio.Wave.StoppedEventArgs)"/> is called.
        /// </summary>
        private volatile bool suppressStopped = false;
        private void WaveIn_RecordingStopped(object? sender, NAudio.Wave.StoppedEventArgs e)
        {
            if (suppressStopped) return;
            RecordingState = RecordingState.Stopped;
            var se = new StoppedEventArgs(e.Exception);
            Stopped?.Invoke(this, se);
        }
        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            var dae = new DataAvailableEventArgs(e.Buffer.AsSpan(0, e.BytesRecorded));
            DataAvailable?.Invoke(this, dae);
        }

        /// <inheritdoc/>
        public RecordingState RecordingState { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="IWaveIn"/>.
        /// </summary>
        private IWaveIn? WaveIn { get; set; }

        /// <inheritdoc/>
        public IWaveFormat Format { get; }

        /// <inheritdoc/>
        public void Start()
        {
            WaveIn?.StartRecording();
            RecordingState = RecordingState.Recording;
        }
        /// <inheritdoc/>
        public void Stop()
        {
            try
            {
                suppressStopped = true;
                WaveIn?.StopRecording();
                RecordingState = RecordingState.Stopped;
                suppressStopped = false;
            }
            finally
            {
                suppressStopped = false;
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
                WaveIn?.Dispose();
                WaveIn = null;
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~NAudioSoundInput()
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
