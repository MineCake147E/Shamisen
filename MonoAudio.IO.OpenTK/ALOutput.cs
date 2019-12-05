using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MonoAudio.Formats;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace MonoAudio.IO
{
    /// <summary>
    /// Provides an <see cref="AL"/> output.
    /// </summary>
    public sealed partial class ALOutput : ISoundOut
    {
        private int[] bufferPointers;
        private const int BUFNUM = 8;
        private static readonly TimeSpan MinimumSleep = TimeSpan.FromMilliseconds(1);
        private CancellationTokenSource cancellationTokenSource;
        private bool bufferCreationNeeded = true;
        private byte[] inbuf;
        private ALFormat format;
        private ContextHandle contextHandle;
        private bool disposedValue = false;
        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(false);
        private Task fillTask;
        private IntPtr device;
        private IWaveFormat sourceFormat;
        private volatile bool running = false;
        private IWaveSource Source { get; set; }

        /// <summary>
        /// Gets the value which indicates how long does the <see cref="AL"/> takes while delivering the audio data to the hardware.
        /// </summary>
        public TimeSpan Latency { get; }

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState { get; private set; }

        private static TimeSpan DefaultLatency { get; } = TimeSpan.FromMilliseconds(8);

        private int src;

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
        public ALOutput(TimeSpan latency) : this(string.Empty, latency)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALOutput"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="latency">The latency.</param>
        public ALOutput(ALDevice device, TimeSpan latency) : this(device.Name, latency) { }

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
            Latency = latency;
#if DEBUG
            var version = AL.Get(ALGetString.Version); CheckErrors();
            var vendor = AL.Get(ALGetString.Vendor); CheckErrors();
            var renderer = AL.Get(ALGetString.Renderer); CheckErrors();
            Debug.WriteLine(version);
            Debug.WriteLine(vendor);
            Debug.WriteLine(renderer);
#endif
        }

        /// <summary>
        /// Initializes the <see cref="ISoundOut"/> for playing a <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source to play.</param>
        public void Initialize(IWaveSource source)
        {
            Source = source;
            sourceFormat = source.Format;
            bufferCreationNeeded = true;
            if (!(cancellationTokenSource is null))
            {
                cancellationTokenSource.Cancel();
            }
            cancellationTokenSource = new CancellationTokenSource();
        }

        private static ALFormat GetALFormat(IWaveFormat wf)
        {
            if (wf.Channels == 1)
            {
                if (wf.Encoding == AudioEncoding.Pcm)
                {
                    if (wf.BitDepth == 8) return ALFormat.Mono8;
                    else if (wf.BitDepth == 16) return ALFormat.Mono16;
                }
                else if (wf.Encoding == AudioEncoding.IeeeFloat)
                {
                    if (wf.BitDepth == 32) return ALFormat.MonoFloat32Ext;
                }
            }
            else if (wf.Channels == 2)
            {
                if (wf.Encoding == AudioEncoding.Pcm)
                {
                    if (wf.BitDepth == 8) return ALFormat.Stereo8;
                    else if (wf.BitDepth == 16) return ALFormat.Stereo16;
                }
                else if (wf.Encoding == AudioEncoding.IeeeFloat)
                {
                    if (wf.BitDepth == 32) return ALFormat.StereoFloat32Ext;
                }
            }
            throw new ArgumentException($"The format '{wf.ToString()}' is not supported.");
        }

        [DebuggerNonUserCode]
        private void CheckErrors()
        {
            var error = AL.GetError();
            if (error != ALError.NoError)
            {
                throw new InvalidOperationException($"{nameof(ALOutput)} detected an error occurred on OpenAL:" + AL.GetErrorString(error));
            }
        }

        private static PlaybackState ConvertState(ALSourceState aLSourceState)
            => aLSourceState switch
            {
                ALSourceState.Initial => PlaybackState.Playing,

                ALSourceState.Playing => PlaybackState.Playing,

                ALSourceState.Paused => PlaybackState.Paused,

                ALSourceState.Stopped => PlaybackState.Stopped,

                _ => PlaybackState.Stopped,
            };

        private void FillBuffer(CancellationToken token)
        {
            if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers); CheckErrors(); }
            if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
            bufferPointers = AL.GenBuffers(BUFNUM); CheckErrors();
            src = AL.GenSource(); CheckErrors();
            inbuf = new byte[sourceFormat.GetBufferSizeRequired(Latency)];
            format = GetALFormat(sourceFormat);
            foreach (var item in bufferPointers)
            {
                AL.BufferData(item, format, inbuf, inbuf.Length, sourceFormat.SampleRate); CheckErrors();
            }
            AL.SourceQueueBuffers(src, BUFNUM, bufferPointers); CheckErrors();
            AL.Source(src, ALSourcef.Gain, 1); CheckErrors();
            AL.Source(src, ALSource3f.Position, 0, 0, 0); CheckErrors();
            AL.SourcePlay(src); CheckErrors();
            PlaybackState = PlaybackState.Playing;
            try
            {
                while (running)
                {
                    token.ThrowIfCancellationRequested();
                    fillFlag.Wait();
                    token.ThrowIfCancellationRequested();
                    AL.GetSource(src, ALGetSourcei.BuffersProcessed, out int bp); CheckErrors();
                    _ = FillBuffer(bp);
                    var alState = ConvertState(AL.GetSourceState(src));
                    if (PlaybackState == PlaybackState.Playing && alState == PlaybackState.Stopped)
                    {
                        AL.SourcePlay(src); CheckErrors();
                    }
                    CheckErrors();
                    if (bp < 1) Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(MinimumSleep.TotalMilliseconds, Latency.TotalMilliseconds / BUFNUM)));
                }
            }
            finally
            {
                AL.SourcePause(src);
            }
        }

        private int FillBuffer(int bp)
        {
            while (bp > 0)
            {
                var buffer = AL.SourceUnqueueBuffer(src); CheckErrors();
                AL.GetBuffer(buffer, ALGetBufferi.Size, out int size);
                CheckErrors();
                AL.GetBuffer(buffer, ALGetBufferi.Bits, out int bits);
                CheckErrors();
                if (bits == 0)
                {
                    throw new DivideByZeroException("The bit depth of ALBuffer cannot be 0!");
                }
                else
                {
                    if (size > 0)
                    {
                        int cnt = Source.Read(inbuf.AsSpan().Slice(0, size));
                        AL.BufferData(buffer, format, inbuf, cnt, sourceFormat.SampleRate); CheckErrors();
                    }
                    AL.SourceQueueBuffer(src, buffer); CheckErrors();
                }
                bp--;
            }

            return bp;
        }

        #region Playback Controls

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot pause without playing!");
            PlaybackState = PlaybackState.Paused;
            if (AL.IsSource(src)) AL.SourcePause(src);
            fillFlag.Reset();
        }

        /// <summary>
        /// Starts the audio playback.
        /// Use <see cref="Resume"/> instead while the playback is <see cref="PlaybackState.Paused"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Cannot start playback without stopping or initializing!
        /// </exception>
        public void Play()
        {
            if (PlaybackState == PlaybackState.Paused)
            {
                Resume();
            }
            else
            {
                if (PlaybackState != PlaybackState.Stopped) throw new InvalidOperationException($"Cannot start playback without stopping or initializing!");
                if (AL.IsSource(src)) AL.SourcePlay(src);
                PlaybackState = PlaybackState.Playing;
                fillFlag.Set();
                fillTask ??= Task.Run(() => FillBuffer(cancellationTokenSource.Token), cancellationTokenSource.Token);
            }
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException($"Cannot resume without pausing!");
            if (AL.IsSource(src)) AL.SourcePlay(src);
            PlaybackState = PlaybackState.Playing;
            fillFlag.Set();
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            if (AL.IsSource(src)) AL.SourceStop(src);
            PlaybackState = PlaybackState.Stopped;
            running = false;
            fillFlag.Set();
            cancellationTokenSource.Cancel();
            fillTask.Dispose();
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
                if (PlaybackState != PlaybackState.Stopped)
                {
                    PlaybackState = PlaybackState.Stopped;
                    running = false;
                    fillFlag.Set();
                    cancellationTokenSource.Cancel();
                    fillTask.Dispose();
                }
                if (disposing)
                {
                    Source.Dispose();
                }

                if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers); CheckErrors(); }
                if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                _ = Alc.CloseDevice(device);
                Alc.DestroyContext(contextHandle);
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
