using System;
using System.Collections.Generic;
using System.Text;
using MonoAudio.Formats;

namespace MonoAudio.IO
{
    /// <summary>
    /// Defines a base structure of audio output device.
    /// </summary>
    public interface IAudioOutputDevice<TSoundOut> : IAudioDevice where TSoundOut : ISoundOut
    {
        /// <summary>
        /// Indicates whether the audio output device supports a particular stream format.
        /// </summary>
        /// <param name="format">The format to judge the availability.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns>The value which indicates how the <see cref="IWaveFormat"/> can be supported by <see cref="MonoAudio"/>.</returns>
        FormatSupportStatus CheckSupportStatus(IWaveFormat format, IOExclusivity mode = IOExclusivity.Shared);

        /// <summary>
        /// Creates the <see cref="ISoundOut"/> that outputs audio to this device.
        /// </summary>
        /// <param name="latency">The desired latency for output.</param>
        /// <returns>The <typeparamref name="TSoundOut"/> instance.</returns>
        TSoundOut CreateSoundOut(TimeSpan latency = default);

        /// <summary>
        /// Creates the <see cref="ISoundOut"/> that outputs audio to this device with the specified <paramref name="mode"/>.
        /// </summary>
        /// <param name="latency">The latency.</param>
        /// <param name="mode">The share mode.</param>
        /// <returns></returns>
        TSoundOut CreateSoundOut(TimeSpan latency, IOExclusivity mode);
    }
}
