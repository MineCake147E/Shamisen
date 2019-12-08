using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using MonoAudio.Formats;
using Encoding = Android.Media.Encoding;

namespace MonoAudio.IO.Android
{
    /// <summary>
    /// Provides an <see cref="AudioTrack"/> output.
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class AudioTrackOutput : ISoundOut
    {
        private const Speakers SpeakersNotSupported = Speakers.RearLowFrequency
                    | Speakers.TopFrontLeft | Speakers.TopFrontCenter | Speakers.TopFrontRight
                    | Speakers.TopRearLeft | Speakers.TopRearCenter | Speakers.TopRearRight
                    | Speakers.TopSideLeft | Speakers.TopSideCenter | Speakers.TopSideRight;

        private bool disposedValue = false;

        private ManualResetEventSlim fillFlag = new ManualResetEventSlim(false);

        private AudioTrack track;

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

        private Task procTask;

        /// <summary>
        /// Initializes an instance of <see cref="AudioTrackOutput"/>.
        /// </summary>
        /// <param name="usageKind">The kind of usage to set to the internal <see cref="AudioTrack"/>.</param>
        /// <param name="contentType">The kind of content to set to the internal <see cref="AudioTrack"/>.</param>
        /// <param name="latency">
        /// The value which indicates how long does the <see cref="AudioTrack"/> takes while delivering the audio data to the hardware.<br/>
        /// Must be greater than <see cref="TimeSpan.Zero"/>, otherwise, it throws <see cref="ArgumentOutOfRangeException"/>.
        /// </param>
        public AudioTrackOutput(AudioUsageKind usageKind, AudioContentType contentType, TimeSpan latency)
        {
            if (latency <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(latency), $"The {nameof(latency)} must be greater than {TimeSpan.Zero}!");
            UsageKind = usageKind;
            ContentType = contentType;
            Latency = latency;
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Initializes the <see cref="ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        public void Initialize(IWaveSource source)
        {
            AudioAttributes attributes;
            AudioFormat format;
            int latencyInFrames = (int)(source.Format.SampleRate * Latency.TotalSeconds);
            bufferSizeInBytes = latencyInFrames * source.Format.GetFrameSize();
            buffer = new byte[bufferSizeInBytes];
            using (var attributesBuilder = new AudioAttributes.Builder())
            using (var formatBuilder = new AudioFormat.Builder())
            using (var trackBuilder = new AudioTrack.Builder())
            {
                _ = attributesBuilder
                    .SetUsage(UsageKind)
                    .SetContentType(ContentType);
                attributes = attributesBuilder.Build();
                _ = formatBuilder
                    .SetEncoding(ConvertEncoding(source.Format))
                    .SetSampleRate(source.Format.SampleRate)
                    .SetChannelMask(ConvertChannelMask(source.Format));
                format = formatBuilder.Build();
                _ = trackBuilder
                    .SetAudioAttributes(attributes)
                    .SetAudioFormat(format)
                    .SetBufferSizeInBytes(bufferSizeInBytes);
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
            track.Pause();
            fillFlag.Reset();
        }

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play()
        {
            if (PlaybackState != PlaybackState.Stopped) throw new InvalidOperationException($"Cannot start playback without stopping or initializing!");
            PlaybackState = PlaybackState.Playing;
            running = true;
            track.Play();
            fillFlag.Set();
            procTask = Task.Run(() => Process(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume()

        {
            if (PlaybackState != PlaybackState.Paused) throw new InvalidOperationException($"Cannot resume without pausing!");
            PlaybackState = PlaybackState.Playing;
            track.Play();
            fillFlag.Set();
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop()
        {
            if (PlaybackState != PlaybackState.Playing) throw new InvalidOperationException($"Cannot stop without playing!");
            PlaybackState = PlaybackState.Stopped;
            running = false;
            track.Stop();
            fillFlag.Set();
            procTask.Dispose();
        }

        private void Process(CancellationToken token)
        {
            while (running)
            {
                if (PlaybackState == PlaybackState.Playing && track.PlayState != PlayState.Playing)
                {
                    track.Play();
                }
                var span = buffer.AsSpan();
                span = span.Slice(0, Source.Read(span));
                try
                {
                    track.Write(buffer, 0, span.Length, WriteMode.Blocking);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw;
                }
                token.ThrowIfCancellationRequested();
                fillFlag.Wait();
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

        private ChannelOut ConvertChannelMask(IWaveFormat format)
        {
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
            var speakers = format.GetChannelMasks();
            switch (speakers)
            {
                case Speakers.None:
                    throw new NotSupportedException($"The given format ({format.ToString()}) is not supported!");
                case Speakers.Monaural:
                    return ChannelOut.Mono;
                case Speakers.FrontStereo:
                    return ChannelOut.Stereo;
                case Speakers.ThreePointOne:
                    return ChannelOut.Stereo | ChannelOut.FrontCenter | ChannelOut.LowFrequency;
                case Speakers.FivePointOne:
                    return ChannelOut.FivePointOne;
                case Speakers.SevenPointOne:
                    return ChannelOut.SevenPointOne;
                default:
                    if ((speakers & SpeakersNotSupported) > 0)
                        throw new NotSupportedException($"The given format ({format.ToString()}) is not supported!");
                    ChannelOut result = ChannelOut.None;
                    if ((speakers & Speakers.FrontLeft) > 0) result |= ChannelOut.FrontLeft;
                    if ((speakers & Speakers.FrontRight) > 0) result |= ChannelOut.FrontRight;
                    if ((speakers & Speakers.FrontCenter) > 0) result |= ChannelOut.FrontCenter;
                    if ((speakers & Speakers.FrontLowFrequency) > 0) result |= ChannelOut.LowFrequency;
                    if ((speakers & Speakers.RearLeft) > 0) result |= ChannelOut.BackLeft;
                    if ((speakers & Speakers.RearRight) > 0) result |= ChannelOut.BackRight;
                    if ((speakers & Speakers.FrontLeftOfCenter) > 0) result |= ChannelOut.FrontLeftOfCenter;
                    if ((speakers & Speakers.FrontRightOfCenter) > 0) result |= ChannelOut.FrontRightOfCenter;
                    if ((speakers & Speakers.RearCenter) > 0) result |= ChannelOut.BackCenter;
                    if ((speakers & Speakers.SideLeft) > 0) result |= ChannelOut.SideLeft;
                    if ((speakers & Speakers.SideRight) > 0) result |= ChannelOut.SideRight;
                    return result;
            }
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
                fillFlag.Reset();
                cancellationTokenSource.Cancel();
                fillFlag.Set();
                procTask.Dispose();
                track.Dispose();
                track = null;
                fillFlag.Dispose();
                fillFlag = null;
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
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
