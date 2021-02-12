using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Audio;

using Shamisen.Conversion.Resampling.Sample;
using Shamisen.Conversion.SampleToWaveConverters;
using Shamisen.Conversion.WaveToSampleConverters;

namespace Shamisen.IO.MonoGame
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="ISoundOut" />
    public sealed class ShamisenSoundEffectInstance : ISoundOut
    {
        private bool disposedValue = false;

        private DynamicSoundEffectInstance soundEffectInstance;

        /// <summary>
        /// Gets the latency of the playback.
        /// </summary>
        /// <value>
        /// The latency.
        /// </value>
        public TimeSpan Latency { get; }

        private const int TargetBufferCount = 4;
        private int currentBufferIndex = 0;

        private byte[][] buffers;

        /// <summary>
        /// Gets the state of the playback.
        /// </summary>
        /// <value>
        /// The state of the playback.
        /// </value>
        public PlaybackState PlaybackState
        {
            get
            {
                switch (soundEffectInstance.State)
                {
                    case SoundState.Playing:
                        return PlaybackState.Playing;
                    case SoundState.Paused:
                        return PlaybackState.Paused;
                    case SoundState.Stopped:
                        return PlaybackState.Stopped;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private IWaveSource waveSource;

        /// <summary>
        /// Initializes the <see cref="ISoundOut" /> for playing a <paramref name="source" />.
        /// </summary>
        /// <param name="source">The source to play.</param>
        /// <exception cref="NotSupportedException">
        /// The specified number of channels is not supported!
        /// or
        /// The format \"{actualSource.Format.BitDepth}bit Linear PCM\" is not supported!
        /// or
        /// The format \"{actualSource.Format.BitDepth}bit IEEE 754 Floating Point PCM\" is not supported!
        /// or
        /// The format \"{actualSource.Format.BitDepth}bit {actualSource.Format.Encoding.ToString()}\" is not supported!
        /// </exception>
        public void Initialize(IWaveSource source)
        {
            AudioChannels channels;
            switch (source.Format.Channels)
            {
                case 1:
                    channels = AudioChannels.Mono;
                    break;
                case 2:
                    channels = AudioChannels.Stereo;
                    break;
                default:
                    throw new NotSupportedException($"The specified number of channels is not supported!");
            }
            IWaveSource actualSource = source;
            ISampleSource sampleSource;
            //Needed to be 16bit PCM : https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Audio/DynamicSoundEffectInstance.cs#L230
            switch (actualSource.Format.Encoding)
            {
                case AudioEncoding.LinearPcm:
                    switch (actualSource.Format.BitDepth)
                    {
                        case 16:
                            sampleSource = new Pcm16ToSampleConverter(actualSource);
                            break;
                        case 8:
                            sampleSource = new Pcm8ToSampleConverter(actualSource);
                            break;
                        case 24:
                            sampleSource = new Pcm24ToSampleConverter(actualSource);
                            break;
                        case 32:
                            sampleSource = new Pcm32ToSampleConverter(actualSource);
                            break;
                        default:
                            throw new NotSupportedException($"The format \"{actualSource.Format.BitDepth}bit Linear PCM\" is not supported!");
                    }
                    break;
                case AudioEncoding.IeeeFloat:
                    switch (actualSource.Format.BitDepth)
                    {
                        case 32:
                            sampleSource = new Float32ToSampleConverter(actualSource);
                            break;
                        default:
                            throw new NotSupportedException($"The format \"{actualSource.Format.BitDepth}bit IEEE 754 Floating Point PCM\" is not supported!");
                    }
                    break;
                default:
                    throw new NotSupportedException($"The format \"{actualSource.Format.BitDepth}bit {actualSource.Format.Encoding}\" is not supported!");
            }
            //Sample rate is capped between 8k~48k : https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Audio/DynamicSoundEffectInstance.cs#L83
            ValidateSampleRate(source, ref actualSource, ref sampleSource);
            soundEffectInstance = new DynamicSoundEffectInstance(actualSource.Format.SampleRate, channels);
            soundEffectInstance.BufferNeeded += SoundEffectInstance_BufferNeeded;
            var l = soundEffectInstance.GetSampleSizeInBytes(Latency);
            buffers = new byte[TargetBufferCount * 3][];
            for (int i = 0; i < buffers.Length; i++)
            {
                buffers[i] = new byte[l];
                buffers[i].AsSpan().FastFill(0);
            }
        }

        private volatile bool isProcessing = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShamisenSoundEffectInstance"/> class.
        /// </summary>
        /// <param name="latency">The latency.</param>
        public ShamisenSoundEffectInstance(TimeSpan latency)
        {
            if (latency.TotalMilliseconds < 4) latency = TimeSpan.FromMilliseconds(4);
            Latency = latency;
        }

        private void SoundEffectInstance_BufferNeeded(object sender, EventArgs e)
        {
            if (isProcessing) return;
            isProcessing = true;
            while (soundEffectInstance.PendingBufferCount < TargetBufferCount)
            {
                var buf = buffers[currentBufferIndex];
                currentBufferIndex = ++currentBufferIndex % buffers.Length;
                var len = waveSource.Read(buf);
                if (len.HasData)
                {
                    soundEffectInstance.SubmitBuffer(buf, 0, len.Length);
                }
            }
            isProcessing = false;
        }

        private void ValidateSampleRate(IWaveSource source, ref IWaveSource actualSource, ref ISampleSource sampleSource)
        {
            if (sampleSource.Format.SampleRate < 24000)
            {
                sampleSource = new SplineResampler(sampleSource, 24000);
            }
            else if (sampleSource.Format.SampleRate > 48000)
            {
                sampleSource = new SplineResampler(sampleSource, 48000);
            }
            else if (actualSource.Format.Encoding == AudioEncoding.LinearPcm && actualSource.Format.BitDepth == 16)
            {
                sampleSource?.Dispose();
                sampleSource = null;
                waveSource = source;
                return;
            }
            actualSource = new SampleToPcm16Converter(sampleSource, true, Endianness.Little);
            waveSource = actualSource;
        }

        /// <summary>
        /// Pauses the audio playback.
        /// </summary>
        public void Pause() => soundEffectInstance?.Pause();

        /// <summary>
        /// Starts the audio playback.
        /// </summary>
        public void Play() => soundEffectInstance?.Play();

        /// <summary>
        /// Resumes the audio playback.
        /// </summary>
        public void Resume() => soundEffectInstance?.Resume();

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void Stop() => soundEffectInstance?.Stop();

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //
                }
                soundEffectInstance?.Dispose();
                soundEffectInstance = null;
                //

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
