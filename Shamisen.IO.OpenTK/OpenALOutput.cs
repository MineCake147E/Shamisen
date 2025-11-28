using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using ALCContextAttribute = OpenTK.Audio.OpenAL.ALC.ContextAttribute;
using ALAll = OpenTK.Audio.OpenAL.All;
using ALErrorCode = OpenTK.Audio.OpenAL.ErrorCode;
using ALStringName = OpenTK.Audio.OpenAL.StringName;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Provides a simple <see cref="AL"/> output.
    /// </summary>
    public sealed partial class OpenALOutput : ISoundOut
    {
#if DEBUG
        private const double MinLengthInMilliseconds = 16;
        private const int BUFNUM = 4;
#else
        private const double MinLengthInMilliseconds = 4;
        private const int BUFNUM = 16;
#endif
        private const int NumberOfBuffers = BUFNUM;
        private const double ReciprocalNumberOfBuffers = 1.0 / NumberOfBuffers;
        private int[] bufferPointers;

        private static readonly TimeSpan MinimumSleep = TimeSpan.FromMilliseconds(1);
        private CancellationTokenSource? cancellationTokenSource;
        private bool bufferCreationNeeded = true;
        private byte[]? inbuf;
        private Format format;
        private ALCContext contextHandle;
        private bool disposedValue = false;
        private ManualResetEventSlim fillFlag = new(false);
        private Task? fillTask;
        private ALCDevice device;
        private IWaveFormat? sourceFormat;
        private volatile bool running = false;
        private Dictionary<string, bool> ExtensionCache { get; } = new();

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
        public OpenALOutput(OpenALOutputDevice device, TimeSpan latency) : this(device.Name, latency) { }

        private OpenALOutput(string name, TimeSpan latency)
        {
            device = ALC.OpenDevice(name);
            if (device == ALCDevice.Null) throw new ArgumentException($"The device {name} is not found!", nameof(name));
            unsafe
            {
                contextHandle = ALC.CreateContext(device, (int*)null);
            }
            int sampleRate = -1;
            var sAttr = OpenALContextManager.GetContextAttributes(device);
            foreach (var entry in sAttr)
            {
                switch (entry.Key)
                {
                    case ALCContextAttribute.Frequency:
                        sampleRate = MathI.Max(sampleRate, entry.Value);
                        break;
                    default:
                        continue;
                }
                break;
            }
            sAttr.Clear();
            if (sampleRate < 0) throw new ArgumentException($"The device {name} is not supported!", nameof(name));
            Console.WriteLine($"SampleRate:{sampleRate}");
            Latency = latency * ReciprocalNumberOfBuffers;
            Latency = Latency.TotalMilliseconds < MinLengthInMilliseconds ? TimeSpan.FromMilliseconds(MinLengthInMilliseconds) : Latency;
            bufferPointers = [];
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
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            PlaybackState = PlaybackState.Stopped;
        }

        private bool IsExtensionPresent(string name)
        {
            if (ExtensionCache.TryGetValue(name, out var val)) return val;
            var res = AL.IsExtensionPresent(name);
            ExtensionCache[name] = res;
            return res;
        }

        private Format GetALFormat(IWaveFormat wf)
            => (wf.Channels, wf.Encoding, wf.BitDepth) switch
            {
                (1, AudioEncoding.LinearPcm, 8) => Format.FormatMono8,
                (1, AudioEncoding.LinearPcm, 16) => Format.FormatMono16,
                (2, AudioEncoding.LinearPcm, 8) => Format.FormatStereo8,
                (2, AudioEncoding.LinearPcm, 16) => Format.FormatStereo16,
                (1, AudioEncoding.IeeeFloat, 32) when IsExtensionPresent("AL_FORMAT_MONO_FLOAT32") => Format.FormatMonoFloat32,
                (1, AudioEncoding.IeeeFloat, 64) when IsExtensionPresent("AL_FORMAT_MONO_DOUBLE_EXT") => Format.FormatMonoDoubleExt,
                (2, AudioEncoding.IeeeFloat, 32) when IsExtensionPresent("AL_FORMAT_STEREO_FLOAT32") => Format.FormatStereoFloat32,
                (2, AudioEncoding.IeeeFloat, 64) when IsExtensionPresent("AL_FORMAT_STEREO_DOUBLE_EXT") => Format.FormatStereoDoubleExt,
                _ => throw new ArgumentException($"The format '{wf}' is not supported."),
            };

        [DebuggerNonUserCode]
        private void CheckErrors()
        {
            var error = AL.GetError();
            if (error != ALErrorCode.NoError)
                throw new InvalidOperationException($"{nameof(OpenALOutput)} detected an error occurred on OpenAL:" + AL.GetString((ALStringName)error));
        }

        private static PlaybackState ConvertState(SourceState aLSourceState)
            => aLSourceState switch
            {
                SourceState.Initial => PlaybackState.Playing,

                SourceState.Playing => PlaybackState.Playing,

                SourceState.Paused => PlaybackState.Paused,

                SourceState.Stopped => PlaybackState.Stopped,

                _ => PlaybackState.Stopped,
            };

        private async ValueTask FillBufferAsync(CancellationToken token)
        {
            if (Source is null) throw new Exception("");
            await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
            {
                try
                {
                    if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers.Length, bufferPointers); CheckErrors(); }
                    if (bufferPointers is null || bufferPointers.Length < NumberOfBuffers)
                    {
                        bufferPointers = new int[NumberOfBuffers];
                    }
                    if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                    AL.GenBuffers(bufferPointers.Length, bufferPointers); CheckErrors();
                    src = AL.GenSource(); CheckErrors();
                    var sf = sourceFormat ?? throw new NullReferenceException();

                    inbuf = new byte[sf.GetBufferSizeRequired(Latency)];
                    format = sf.ConvertToFormat();
                    foreach (var item in bufferPointers)
                    {
                        var cnt = Source.Read(inbuf.AsSpan());
                        AL.BufferData<byte>(item, format, inbuf.AsSpan(0, cnt.Length), cnt.Length, sf.SampleRate); CheckErrors();
                    }

                    if (IsExtensionPresent("AL_SOFT_direct_channels_remix"))
                    {
                        AL.Sourcei(src, SourcePNameI.DirectChannelsSoft, 2); CheckErrors();
                    }
                    else if (IsExtensionPresent("AL_DIRECT_CHANNELS_SOFT"))
                    {
                        AL.Sourcei(src, SourcePNameI.DirectChannelsSoft, 1); CheckErrors();
                    }
                    AL.Sourcei(src, (SourcePNameI)(int)SourcePNameB.SourceRelative, (int)ALAll.True); CheckErrors();
                    AL.SourceQueueBuffers(src, bufferPointers.Length, MemoryMarshal.Cast<int, uint>(bufferPointers)); CheckErrors();
                    AL.Sourcef(src, SourcePNameF.Gain, 1); CheckErrors();
                    AL.Source3f(src, SourcePName3F.Position, 0, 0, 0); CheckErrors();
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
                    if (token.IsCancellationRequested) return;
                    token.ThrowIfCancellationRequested();
                    fillFlag.Wait(token);
                    int bp;
                    if (token.IsCancellationRequested) return;
                    token.ThrowIfCancellationRequested();
                    using (_ = await OpenALContextManager.WaitForContextAsync(contextHandle))
                    {
                        AL.GetSourcei(src, SourceGetPNameI.BuffersProcessed, out bp); CheckErrors();
                        _ = FillBuffer(bp, inbuf);
                        var alState = ConvertState((SourceState)AL.GetSourcei(src, SourceGetPNameI.SourceState));
                        if (PlaybackState == PlaybackState.Playing && alState == PlaybackState.Stopped)
                        {
                            AL.SourcePlay(src); CheckErrors();
                        }
                        CheckErrors();
                    }
                    if (bp < 1) await Task.Delay(TimeSpan.FromMilliseconds(Math.Max(MinimumSleep.TotalMilliseconds, Latency.TotalMilliseconds * ReciprocalNumberOfBuffers)), token);
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

        private int FillBuffer(int bp, Span<byte> span)
        {
            if (Source is null || sourceFormat is null) throw new Exception("");
            while (bp > 0)
            {
                var buffer = 0;
                AL.SourceUnqueueBuffers(src, 1, ref buffer); CheckErrors();
                AL.GetBufferi(buffer, BufferGetPNameI.Size, out var size);
                CheckErrors();
                AL.GetBufferi(buffer, BufferGetPNameI.Bits, out var bits);
                CheckErrors();
                if (bits == 0)
                {
                    throw new DivideByZeroException("The bit depth of ALBuffer cannot be 0!");
                }
                else
                {
                    if (size > 0)
                    {
                        var cnt = Source.Read(span);
                        AL.BufferData<byte>(buffer, format, span.Slice(0, cnt.Length), cnt.Length, sourceFormat.SampleRate); CheckErrors();
                    }
                    AL.SourceQueueBuffers(src, 1, ref Unsafe.As<int, uint>(ref buffer)); CheckErrors();
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
            if (PlaybackState != PlaybackState.Playing) return;
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
            switch (PlaybackState)
            {
                case PlaybackState.Playing:
                    return;
                case PlaybackState.Paused:
                    Resume();
                    break;
                default:
                    if (PlaybackState != PlaybackState.Stopped) throw new InvalidOperationException("Cannot start playback without stopping or initializing!");
                    _ = Task.Run(async () => await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
                        {
                            running = true;
                            if (AL.IsSource(src)) AL.SourcePlay(src);
                            PlaybackState = PlaybackState.Playing;
                            fillFlag.Set();
                            if (cancellationTokenSource is null) throw new InvalidOperationException();
                            fillTask ??= Task.Run(async () => await FillBufferAsync(cancellationTokenSource.Token), cancellationTokenSource.Token);
                            fillTask.ConfigureAwait(false);
                        })).ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException("Cannot resume without pausing!");
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
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException("Cannot stop without playing!");
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
                    Source?.Dispose();

                if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers.Length, bufferPointers); CheckErrors(); }
                if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                ALC.DestroyContext(contextHandle);
                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}
