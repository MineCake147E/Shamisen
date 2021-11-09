using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Silk.NET.OpenAL;

using ALCApi = Silk.NET.OpenAL.ALContext;
using AlcContextAttributes = Silk.NET.OpenAL.ContextAttributes;
using ALContext = Silk.NET.OpenAL.Context;
using ALError = Silk.NET.OpenAL.AudioError;
using ALFormat = Silk.NET.OpenAL.BufferFormat;
using ALGetBufferi = Silk.NET.OpenAL.GetBufferInteger;
using ALGetSourcei = Silk.NET.OpenAL.GetSourceInteger;
using ALSource3f = Silk.NET.OpenAL.SourceVector3;
using ALSourceb = Silk.NET.OpenAL.SourceBoolean;
using ALSourcef = Silk.NET.OpenAL.SourceFloat;
using ALSourcei = Silk.NET.OpenAL.SourceInteger;
using ALSourceState = Silk.NET.OpenAL.SourceState;
using OpenALApi = Silk.NET.OpenAL.AL;

namespace Shamisen.IO
{
    /// <summary>
    /// Provides an <see cref="OpenALApi"/> output.
    /// </summary>
    public sealed class OpenALOutput : ISoundOut
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
        private uint[]? bufferPointers;

        private static readonly TimeSpan MinimumSleep = TimeSpan.FromMilliseconds(1);
        private CancellationTokenSource? cancellationTokenSource;
        private bool bufferCreationNeeded = true;
        private bool disposedValue;
        private nint device;
        private nint contextHandle;
        private byte[]? inbuf;
        private ALFormat format;
        private ManualResetEventSlim fillFlag = new(false);
        private Task? fillTask;
        private IWaveFormat? sourceFormat;
        private volatile bool running = false;
        private uint src;
        private OpenALContextManager OpenALContextManager { get; }
        private OpenALApi AL => OpenALContextManager.AL;
        private ALCApi ALC => OpenALContextManager.ALC;
        private IWaveSource? Source { get; set; }

        /// <summary>
        /// Gets the value which indicates how long does the <see cref="AL"/> takes while delivering the audio data to the hardware.
        /// </summary>
        public TimeSpan Latency { get; }
        /// <summary>
        /// Initializes a new instance of <see cref="OpenALOutput"/>.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="latency">The latency.</param>
        /// <param name="useOpenALSoft">The value which indicates whether the <see cref="OpenALOutput"/> should be using OpenALSoft library.</param>
        public OpenALOutput(OpenALDevice device, TimeSpan latency, bool useOpenALSoft = false) : this(device.Name, latency, useOpenALSoft)
        {
        }

        private OpenALOutput(string name, TimeSpan latency, bool useOpenALSoft)
        {
            OpenALContextManager = new(useOpenALSoft);
            unsafe
            {
                device = (nint)ALC.OpenDevice(name);
                contextHandle = ALC.CreateContextHandle((Device*)device, (int*)null);
            }
            var sampleRate = OpenALDevice.QueryMaximumSamplingRate(device, ALC);
            if (sampleRate < 0) throw new ArgumentException($"The device {name} is not supported!", nameof(name));
            Console.WriteLine($"SampleRate:{sampleRate}");
            Latency = latency * ReciprocalNumberOfBuffers;
            Latency = Latency.TotalMilliseconds < MinLengthInMilliseconds ? TimeSpan.FromMilliseconds(MinLengthInMilliseconds) : Latency;
            bufferPointers = AL.GenBuffers(NumberOfBuffers); CheckErrors();
        }

        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; private set; }

        /// <inheritdoc/>
        public void Initialize(IWaveSource source)
        {
            Source = source;
            sourceFormat = source.Format;
            bufferCreationNeeded = true;
            if (cancellationTokenSource is not null)
                cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            PlaybackState = PlaybackState.Stopped;
        }

        #region FillBuffer

        private async ValueTask FillBufferAsync(CancellationToken token)
        {
            if (Source is null) throw new Exception("");
            try
            {
                await OpenALContextManager.RunWithContextAsync(contextHandle, () =>
                {
                    if (bufferPointers != null) { AL.DeleteBuffers(bufferPointers); CheckErrors(); }
                    if (AL.IsSource(src)) { AL.DeleteSource(src); CheckErrors(); }
                    bufferPointers = AL.GenBuffers(NumberOfBuffers); CheckErrors();
                    src = AL.GenSource(); CheckErrors();
                    var sf = sourceFormat ?? throw new NullReferenceException("");

                    inbuf = new byte[sf.GetBufferSizeRequired(Latency)];
                    format = OpenALDevice.ConvertToALFormat(sf);
                    foreach (var item in bufferPointers)
                    {
                        var cnt = Source.Read(inbuf.AsSpan());
                        AL.BufferData(item, format, inbuf, sf.SampleRate);
                        CheckErrors();
                    }
                    if (AL.IsExtensionPresent("AL_SOFT_direct_channels_remix"))
                    {
                        AL.SetSourceProperty(src, (ALSourcei)0x1033, 2); CheckErrors();
                    }
                    else if (AL.IsExtensionPresent("AL_DIRECT_CHANNELS_SOFT"))
                    {
                        AL.SetSourceProperty(src, (ALSourcei)0x1033, 1); CheckErrors();
                    }
                    AL.SetSourceProperty(src, ALSourcei.SourceType, (int)SourceType.Streaming);
                    AL.SetSourceProperty(src, ALSourceb.SourceRelative, true); CheckErrors();
                    AL.SourceQueueBuffers(src, bufferPointers); CheckErrors();
                    AL.SetSourceProperty(src, ALSourcef.Gain, 1); CheckErrors();
                    AL.SetSourceProperty(src, ALSource3f.Position, 0, 0, 0); CheckErrors();
                    AL.SourcePlay(src); CheckErrors();
                    PlaybackState = PlaybackState.Playing;
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                throw;
            }
            try
            {
                while (running)
                {
                    token.ThrowIfCancellationRequested();
                    fillFlag.Wait(token);
                    token.ThrowIfCancellationRequested();
                    int bp;
                    using (var handle = await OpenALContextManager.WaitForContextAsync(contextHandle))
                    {
                        AL.GetSourceProperty(src, ALGetSourcei.BuffersProcessed, out bp); CheckErrors();
                        _ = FillBuffer(bp);
                        AL.GetSourceProperty(src, ALGetSourcei.SourceState, out var state);
                        var alState = ConvertState((ALSourceState)state);
                        if (PlaybackState == PlaybackState.Playing && alState == PlaybackState.Stopped)
                        {
                            AL.SourcePlay(src); CheckErrors();
                        }
                        CheckErrors();
                    }
                    if (bp < 1) Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max(MinimumSleep.TotalMilliseconds, Latency.TotalMilliseconds * ReciprocalNumberOfBuffers)));
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
                unsafe
                {
                    uint buffer;
                    AL.SourceUnqueueBuffers(src, 1, &buffer);
                    CheckErrors();
                    AL.GetBufferProperty(buffer, ALGetBufferi.Size, out var size);
                    CheckErrors();
                    AL.GetBufferProperty(buffer, ALGetBufferi.Bits, out var bits);
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
                            fixed (byte* j = inbuf)
                            {
                                AL.BufferData(buffer, format, j, cnt.Length, sourceFormat.SampleRate);
                            }
                            CheckErrors();
                        }
                        AL.SourceQueueBuffers(src, 1, &buffer); CheckErrors();
                    }
                    bp--;
                }
            }

            return bp;
        }
        #endregion
        private static PlaybackState ConvertState(ALSourceState aLSourceState)
            => aLSourceState switch
            {
                ALSourceState.Initial => PlaybackState.Playing,

                ALSourceState.Playing => PlaybackState.Playing,

                ALSourceState.Paused => PlaybackState.Paused,

                ALSourceState.Stopped => PlaybackState.Stopped,

                _ => PlaybackState.Stopped,
            };

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
            if (PlaybackState == PlaybackState.Playing) return;
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
        [DebuggerNonUserCode]
        private void CheckErrors()
        {
            var error = AL.GetError();
            if (error != ALError.NoError)
                throw new InvalidOperationException($"{nameof(OpenALOutput)} detected an error occurred on OpenAL:" + error.ToString());
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

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
