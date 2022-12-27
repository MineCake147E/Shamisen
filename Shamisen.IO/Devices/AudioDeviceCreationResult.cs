using System.Diagnostics.CodeAnalysis;

namespace Shamisen.IO.Devices
{
    /// <summary>
    /// Represents the result of either <see cref="IAudioOutputDevice{TSoundOut, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}.CreateSoundOut(TAudioDeviceConfiguration)"/> or <see cref="IAudioInputDevice{TSoundIn, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}.CreateSoundIn(TAudioDeviceConfiguration)"/>.
    /// </summary>
    /// <typeparam name="TSoundDevice">The type of device.</typeparam>
    public readonly struct AudioDeviceCreationResult<TSoundDevice> where TSoundDevice : class, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AudioDeviceCreationResult{TSoundDevice}"/>.
        /// </summary>
        /// <param name="soundDevice">The created sound device.</param>
        public AudioDeviceCreationResult(TSoundDevice? soundDevice)
        {
            SoundDevice = soundDevice;
        }

        /// <summary>
        /// The created <typeparamref name="TSoundDevice"/>, or <see langword="null"/> if not success.
        /// </summary>
        public TSoundDevice? SoundDevice { get; }

        /// <summary>
        /// Gets the value which indicates whether either <see cref="IAudioOutputDevice{TSoundOut, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}.CreateSoundOut(TAudioDeviceConfiguration)"/> or <see cref="IAudioInputDevice{TSoundIn, TAudioDeviceConfiguration, TAudioDeviceConfigurationBuilder}.CreateSoundIn(TAudioDeviceConfiguration)"/> is success or not.
        /// </summary>
        [MemberNotNullWhen(true, nameof(SoundDevice))]
        public bool IsSuccess => SoundDevice is not null;
    }
}
