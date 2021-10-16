using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides an <see cref="AL"/> output.
    /// </summary>
    public sealed partial class OpenALOutput : ISoundOut
    {
#if DEBUG
        private const double MinLengthInMilliseconds = 16;
        private const int BUFNUM = 4;
#else
        private const double MinLengthInMilliseconds = 1;
        private const int BUFNUM = 32;
#endif
        private int[] bufferPointers;

        private static readonly TimeSpan MinimumSleep = TimeSpan.FromMilliseconds(1);
        private CancellationTokenSource? cancellationTokenSource;
        private bool bufferCreationNeeded = true;
        private byte[]? inbuf;
        private ALFormat format;
        private ALContext contextHandle;
        private bool disposedValue = false;
        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(false);
        private Task? fillTask;
        private ALDevice device;
        private IWaveFormat? sourceFormat;
        private volatile bool running = false;
        private IWaveSource? Source { get; set; }

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
        /// Initializes a new instance of the <see cref="OpenALOutput"/> class.
        /// </summary>
        public OpenALOutput() : this(DefaultLatency)

        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALOutput"/> class with the specified <paramref name="latency"/>.
        /// </summary>
        /// <param name="latency">
        /// The value which indicates how long can <see cref="OpenALOutput"/> take between buffering and actual audio output.
        /// </param>
        public OpenALOutput(TimeSpan latency) : this(string.Empty, latency)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALOutput"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="latency">The latency.</param>
        public OpenALOutput(OpenALDevice device, TimeSpan latency) : this(device.Name, latency) { }

        [Obsolete("The Device name gets no longer supported!")]
        internal OpenALOutput(string name) : this(name, DefaultLatency)
        {
        }

        private OpenALOutput(string name, TimeSpan latency)
        {
            device = ALC.OpenDevice(name);
            unsafe
            {
                contextHandle = ALC.CreateContext(device, (int*)null);
            }
            ALC.GetInteger(device, AlcGetInteger.AttributesSize, 1, out int asize);
            int[] attr = new int[asize];
            ALC.GetInteger(device, AlcGetInteger.AllAttributes, asize, attr);
            var sAttr = MemoryMarshal.Cast<int, (AlcContextAttributes Key, int Value)>(attr.AsSpan());
            int sampleRate = int.MinValue;
            foreach (var (Key, Value) in sAttr)
            {
                switch (Key)
                {
                    case AlcContextAttributes.Frequency:
                        sampleRate = Math.Max(sampleRate, Value);
                        break;
                    default:
                        continue;
                }
                break;
            }
            if (sampleRate < 0) throw new ArgumentException($"The device {name} is not supported!", nameof(name));
            Console.WriteLine($"SampleRate:{sampleRate}");
            Latency = latency / BUFNUM;
            Latency = Latency.TotalMilliseconds < MinLengthInMilliseconds ? TimeSpan.FromMilliseconds(MinLengthInMilliseconds) : Latency;
            bufferPointers = AL.GenBuffers(BUFNUM); CheckErrors();
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
            PlaybackState = PlaybackState.Stopped;
        }

        private static ALFormat GetALFormat(IWaveFormat wf)
            => (wf.Channels, wf.Encoding, wf.BitDepth) switch
            {
                (1, AudioEncoding.LinearPcm, 8) => ALFormat.Mono8,
                (1, AudioEncoding.LinearPcm, 16) => ALFormat.Mono16,
                (2, AudioEncoding.LinearPcm, 8) => ALFormat.Stereo8,
                (2, AudioEncoding.LinearPcm, 16) => ALFormat.Stereo16,
                (1, AudioEncoding.IeeeFloat, 32) when AL.IsExtensionPresent("AL_FORMAT_MONO_FLOAT32") => ALFormat.MonoFloat32Ext,
                (1, AudioEncoding.IeeeFloat, 64) when AL.IsExtensionPresent("AL_FORMAT_MONO_DOUBLE_EXT") => ALFormat.MonoDoubleExt,
                (2, AudioEncoding.IeeeFloat, 32) when AL.IsExtensionPresent("AL_FORMAT_STEREO_FLOAT32") => ALFormat.StereoFloat32Ext,
                (2, AudioEncoding.IeeeFloat, 64) when AL.IsExtensionPresent("AL_FORMAT_STEREO_DOUBLE_EXT") => ALFormat.StereoDoubleExt,
                _ => throw new ArgumentException($"The format '{wf}' is not supported."),
            };

        [DebuggerNonUserCode]
        private void CheckErrors()
        {
            var error = AL.GetError();
            if (error != ALError.NoError)
            {
                throw new InvalidOperationException($"{nameof(OpenALOutput)} detected an error occurred on OpenAL:" + AL.GetErrorString(error));
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

        private async ValueTask FillBufferAsync(CancellationToken token)
        {
            if (Source is null) throw new Exception("");
            await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
            {
                try
                {
                    if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers); CheckErrors(); }
                    if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                    bufferPointers = AL.GenBuffers(BUFNUM); CheckErrors();
                    src = AL.GenSource(); CheckErrors();
                    var sf = sourceFormat ?? throw new ArgumentNullException();

                    inbuf = new byte[sf.GetBufferSizeRequired(Latency)];
                    format = GetALFormat(sf);
                    foreach (int item in bufferPointers)
                    {
                        var cnt = Source.Read(inbuf.AsSpan());
                        AL.BufferData(item, format, inbuf.AsSpan(0, cnt.Length), sf.SampleRate); CheckErrors();
                    }
                    if (AL.IsExtensionPresent("AL_SOFT_direct_channels_remix"))
                    {
                        AL.Source(src, (ALSourcei)0x1033, 2); CheckErrors();
                    }
                    else if (AL.IsExtensionPresent("AL_DIRECT_CHANNELS_SOFT"))
                    {
                        AL.Source(src, (ALSourcei)0x1033, 1); CheckErrors();
                    }

                    AL.Source(src, ALSourceb.SourceRelative, true); CheckErrors();
                    AL.SourceQueueBuffers(src, BUFNUM, bufferPointers); CheckErrors();
                    AL.Source(src, ALSourcef.Gain, 1); CheckErrors();
                    AL.Source(src, ALSource3f.Position, 0, 0, 0); CheckErrors();
                    AL.SourcePlay(src); CheckErrors();
                    PlaybackState = PlaybackState.Playing;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    throw;
                }
            });
            try
            {
                while (running)
                {
                    token.ThrowIfCancellationRequested();
                    fillFlag.Wait(token);
                    token.ThrowIfCancellationRequested();
                    int bp;
                    using (_ = await OpenALContextManager.WaitForContextAsync(contextHandle))
                    {
                        AL.GetSource(src, ALGetSourcei.BuffersProcessed, out bp); CheckErrors();
                        _ = FillBuffer(bp);
                        var alState = ConvertState(AL.GetSourceState(src));
                        if (PlaybackState == PlaybackState.Playing && alState == PlaybackState.Stopped)
                        {
                            AL.SourcePlay(src); CheckErrors();
                        }
                        CheckErrors();
                    }
                    if (bp < 1) await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(MinimumSleep.TotalMilliseconds, Latency.TotalMilliseconds / BUFNUM)), token);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                await OpenALContextManager.RunWithContextAsync(contextHandle, () => AL.SourcePause(src));
            }
        }

        private int FillBuffer(int bp)
        {
            if (Source is null || sourceFormat is null) throw new Exception("");
            while (bp > 0)
            {
                int buffer = AL.SourceUnqueueBuffer(src); CheckErrors();
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
                        var cnt = Source.Read(inbuf.AsSpan().Slice(0, size));
                        AL.BufferData(buffer, format, inbuf.AsSpan(0, cnt.Length), sourceFormat.SampleRate); CheckErrors();
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
            _ = Task.Run(async () => await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
                {
                    if (AL.IsSource(src)) AL.SourcePause(src);
                    fillFlag.Reset();
                })).ConfigureAwait(false);
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
                _ = Task.Run(async () =>
                {
                    running = true;
                    await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
                    {
                        if (AL.IsSource(src)) AL.SourcePlay(src);
                        PlaybackState = PlaybackState.Playing;
                        fillFlag.Set();
                        if (cancellationTokenSource is null) throw new InvalidOperationException();
                        fillTask ??= Task.Run(async () => await FillBufferAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
                        fillTask.ConfigureAwait(false);
                    });
                }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException($"Cannot resume without pausing!");
            _ = Task.Run(async () => await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
                {
                    if (AL.IsSource(src)) AL.SourcePlay(src);
                    PlaybackState = PlaybackState.Playing;
                    fillFlag.Set();
                })).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            _ = Task.Run(StopInternalAsync).ConfigureAwait(false);
        }

        private async Task StopInternalAsync() => await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
        {
            if (AL.IsSource(src)) AL.SourceStop(src);
            PlaybackState = PlaybackState.Stopped;
            running = false;
            fillFlag.Set();
            cancellationTokenSource?.Cancel();
            fillTask?.Dispose();
        });

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
                    cancellationTokenSource?.Cancel();
                    fillTask?.Dispose();
                }
                if (disposing)
                {
                    Source?.Dispose();
                }

                if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers); CheckErrors(); }
                if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                _ = ALC.CloseDevice(device);
                ALC.DestroyContext(contextHandle);

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
