using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Audio.OpenAL;

using ALSource = System.UInt32;

namespace MonoAudio.IO
{
    /// <summary>
    /// Provides an <see cref="AL"/> output.
    /// </summary>
    public sealed partial class ALOutput : ISoundOut
    {
        private CancellationTokenSource cancellationTokenSource;
        private ContextHandle contextHandle;
        private bool disposedValue = false;
        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(false);
        private volatile bool running = true;
        private Task fillTask;
        private int[] bufferPointers;

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState { get; private set; }

        private static TimeSpan DefaultLatency { get; } = TimeSpan.FromMilliseconds(8);

        private ALSource Source { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALOutput"/> class.
        /// </summary>
        public ALOutput() : this(null, DefaultLatency)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALOutput"/> class with the specified <paramref name="latency"/>.
        /// </summary>
        /// <param name="latency">
        /// The value which indicates how long can <see cref="ALOutput"/> take between buffering and actual audio output.
        /// </param>
        public ALOutput(TimeSpan latency) : this(null, latency)
        {
        }

        [Obsolete("The Device name gets no longer supported!")]
        internal ALOutput(string name) : this(name, DefaultLatency)
        {
        }

        private ALOutput(string name, TimeSpan latency)
        {
            var device = Alc.OpenDevice(name);
            contextHandle = Alc.CreateContext(device, (int[])null);
            Alc.GetInteger(device, AlcGetInteger.AttributesSize, 1, out int asize);
            int[] attr = new int[asize];
            Alc.GetInteger(device, AlcGetInteger.AllAttributes, asize, attr);
            var sAttr = MemoryMarshal.Cast<int, ALAttribute<AlcContextAttributes>>(attr.AsSpan());
            int sampleRate = int.MinValue;
            foreach (var item in sAttr)
            {
                switch (item.Key)
                {
                    case AlcContextAttributes.Frequency:
                        sampleRate = Math.Max(sampleRate, item.Value);
                        break;
                    default:
                        continue;
                }
                break;
            }
            if (sampleRate < 0) throw new ArgumentException($"The device {name} is not supported!", nameof(name));
            Console.WriteLine($"SampleRate:{sampleRate}");
            Alc.MakeContextCurrent(contextHandle);
        }

        /// <summary>
        /// Initializes the <see cref="ISoundOut"/> for playing a <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to play.</param>
        public void Initialize(IWaveSource source) => throw new NotImplementedException();

        private void FillBuffer(CancellationToken token)
        {
            while (running)
            {
                token.ThrowIfCancellationRequested();
                fillFlag.Wait();
                token.ThrowIfCancellationRequested();
            }
        }

        #region Playback Controls

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot pause without playing!");
            PlaybackState = PlaybackState.Paused;
        }

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play()
        {
            if (PlaybackState != PlaybackState.Stopped) throw new InvalidOperationException($"Cannot start playback without stopping or initializing!");
            PlaybackState = PlaybackState.Playing;
            fillTask = Task.Run(() => FillBuffer(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException($"Cannot resume without pausing!");
            PlaybackState = PlaybackState.Playing;
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            PlaybackState = PlaybackState.Stopped;
        }

        #endregion Playback Controls

        #region IDisposable Support

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        #endregion IDisposable Support
    }
}
