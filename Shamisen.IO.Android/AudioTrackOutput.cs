using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;

using Shamisen.Formats;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Encoding = Android.Media.Encoding;
using static Android.Graphics.ImageDecoder;
using System.Numerics;

namespace Shamisen.IO.Android
{
    /// <summary>
    /// Provides an <see cref="AudioTrack"/> output.
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class AudioTrackOutput : ISoundOut
    {
        private bool disposedValue = false;

        private ManualResetEventSlim? fillFlag = new ManualResetEventSlim(false);

        private AudioTrack? track;

        private IWaveSource Source { get; set; }

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState { get; private set; }

        private AudioContentType ContentType { get; }

        private AudioUsageKind UsageKind { get; }

        private byte[] buffer;

        /// <summary>
        /// Gets the value which indicates how long does the <see cref="AudioTrack"/> takes while delivering the audio data to the hardware.
        /// </summary>
        public TimeSpan Latency { get; }

        /// <summary>
        /// Gets the value which indicates how the input audio data are.
        /// </summary>
        public IWaveFormat Format => Source.Format;

        private int bufferSizeInBytes;
        private CancellationTokenSource cancellationTokenSource;
        private volatile bool running = true;

        private Task? procTask;

        /// <summary>
        /// Initializes an instance of <see cref="AudioTrackOutput"/>.
        /// </summary>
        /// <param name="usageKind">The kind of usage to set to the internal <see cref="AudioTrack"/>.</param>
        /// <param name="contentType">The kind of content to set to the internal <see cref="AudioTrack"/>.</param>
        /// <param name="latency">
        /// The value which indicates how long does the <see cref="AudioTrack"/> takes while delivering the audio data to the hardware.<br/>
        /// Must be greater than <see cref="TimeSpan.Zero"/>, otherwise, it throws <see cref="ArgumentOutOfRangeException"/>.
        /// </param>
        public AudioTrackOutput(AudioUsageKind usageKind, AudioContentType contentType, TimeSpan latency, IWaveSource source)
        {
            if (latency <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(latency), $"The {nameof(latency)} must be greater than {TimeSpan.Zero}!");
            UsageKind = usageKind;
            ContentType = contentType;
            Latency = latency;
            cancellationTokenSource = new CancellationTokenSource();
            int latencyInFrames = (int)(source.Format.SampleRate * Latency.TotalSeconds / 2.0);
            bufferSizeInBytes = latencyInFrames * source.Format.GetFrameSizeInBytes();
            buffer = new byte[bufferSizeInBytes];
            using (var attributesBuilder = new AudioAttributes.Builder())
            using (var formatBuilder = new AudioFormat.Builder())
            using (var trackBuilder = new AudioTrack.Builder())
            {
                _ = attributesBuilder
                    .SetUsage(UsageKind)
                    ?.SetContentType(ContentType);
                var attributes = attributesBuilder.Build() ?? throw new NullReferenceException();
                _ = formatBuilder
                    .SetEncoding(ConvertEncoding(source.Format))
                    ?.SetSampleRate(source.Format.SampleRate)
                    ?.SetChannelMask(ConvertChannels(source.Format));
                var format = formatBuilder.Build() ?? throw new NullReferenceException();
                _ = trackBuilder
                    .SetAudioAttributes(attributes)
                    .SetAudioFormat(format)
                    .SetBufferSizeInBytes(bufferSizeInBytes * 2);
                track = trackBuilder.Build();
            }
            Source = source;
            PlaybackState = PlaybackState.Stopped;
        }

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot pause without playing!");
            PlaybackState = PlaybackState.Paused;
            track?.Pause();
            fillFlag?.Reset();
        }

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play()
        {
            if (PlaybackState != PlaybackState.Stopped) throw new InvalidOperationException($"Cannot start playback without stopping or initializing!");
            PlaybackState = PlaybackState.Playing;
            running = true;
            track?.Play();
            fillFlag?.Set();
            procTask = Task.Run(() => Process(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()
        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException($"Cannot resume without pausing!");
            PlaybackState = PlaybackState.Playing;
            track?.Play();
            fillFlag?.Set();
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            PlaybackState = PlaybackState.Stopped;
            running = false;
            track?.Stop();
            fillFlag?.Set();
            procTask?.Dispose();
        }

        private void Process(CancellationToken token)
        {
            while (running)
            {
                if (track is null) break;
                if (PlaybackState == PlaybackState.Playing && track.PlayState != PlayState.Playing)
                {
                    track.Play();
                }
                var span = buffer.AsSpan();
                ReadResult rr = Source.Read(span);
                if (rr.HasData)
                {
                    span = span.Slice(0, rr.Length);
                    try
                    {
                        track?.Write(buffer, 0, span.Length, WriteMode.Blocking);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        throw;
                    }
                }
                token.ThrowIfCancellationRequested();
                fillFlag?.Wait();
                token.ThrowIfCancellationRequested();
            }
        }

        private static Encoding ConvertEncoding(IWaveFormat format)
        {
            switch (format.Encoding)
            {
                case AudioEncoding.LinearPcm:
                    switch (format.BitDepth)
                    {
                        case 8:
                            return Encoding.Pcm8bit;
                        case 16:
                            return Encoding.Pcm16bit;
                        default:
                            break;
                    }
                    break;
                case AudioEncoding.IeeeFloat:
                    return Encoding.PcmFloat;
                case AudioEncoding.DolbyAc3Spdif:
                    return Encoding.EAc3;
                default:
                    break;
            }
            throw new NotSupportedException($"The given format ({format.ToString()}) is not supported!");
        }

        private ChannelOut ConvertChannels(IWaveFormat format)
        {
            if(format is IChannelMaskedFormat channelMaskedFormat)
            {
                var speakers = channelMaskedFormat.ChannelCombination;
                return ConvertChannelMask(format, speakers);
            }
            else
            {

                return ConvertUnknownChannels(format);

            }
        }

        private static ChannelOut ConvertUnknownChannels(IWaveFormat format) =>
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
#pragma warning disable RCS1130
            format.Channels switch
            {
                1 => ChannelOut.Mono,
                2 => ChannelOut.Stereo,
                3 => ChannelOut.Stereo | ChannelOut.FrontCenter,
                4 => ChannelOut.Quad,
                5 => ChannelOut.Quad | ChannelOut.BackCenter,
                6 => ChannelOut.FivePointOne,
                7 => ChannelOut.FivePointOne | ChannelOut.BackCenter,
                8 => ChannelOut.C7point1Surround,
                _ => ChannelOut.Default,
            };
#pragma warning restore RCS1130
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations


        private static ChannelOut ConvertChannelMask(IWaveFormat format, StandardSpeakerChannels speakers)
        {
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
#pragma warning disable RCS1130
            switch (speakers)
            {
                case StandardSpeakerChannels.None:
                    throw new ArgumentOutOfRangeException($"The given format ({format}) is not supported!");
                case StandardSpeakerChannels.Monaural:
                    return ChannelOut.Mono;
                case StandardSpeakerChannels.FrontStereo:
                case StandardSpeakerChannels.SideStereo:    //SideStereo alone might be selected for Headphones.
                    return ChannelOut.Stereo;
                case StandardSpeakerChannels.ThreePointOne:
                    return ChannelOut.Stereo | ChannelOut.FrontCenter | ChannelOut.LowFrequency;
                case StandardSpeakerChannels.FrontFivePointOne:
                    return ChannelOut.FivePointOne;
                case StandardSpeakerChannels.SevenPointOne:
                    return ChannelOut.SevenPointOne;
                default:
                    return (ChannelOut)((uint)speakers << 2);   //Exploiting the fact that ChannelOut is designed to have 2 bits of padding at LSB compared to the KSAUDIO_CHANNEL_CONFIG for all defined channels.
            }
#pragma warning restore RCS1130
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
        }

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
                fillFlag?.Reset();
                cancellationTokenSource.Cancel();
                fillFlag?.Set();
                procTask?.Dispose();
                track?.Dispose();
                track = null;
                fillFlag?.Dispose();
                fillFlag = null;
                cancellationTokenSource.Dispose();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AudioTrackOutput"/> class.
        /// </summary>
        ~AudioTrackOutput()
        {
            Dispose(false);
        }

        #endregion IDisposable Support
    }
}
