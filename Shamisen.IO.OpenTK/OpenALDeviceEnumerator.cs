using OpenTK.Audio.OpenAL;

namespace Shamisen.IO
{
    /// <summary>
    /// Enumerates the AL devices.
    /// </summary>
    public sealed class OpenALDeviceEnumerator : IAudioOutputDeviceEnumerator<OpenALDevice, OpenALOutput, OpenALOutputConfiguration, OpenALOutputConfigurationBuilder>
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly OpenALDeviceEnumerator Instance = new(false);

        private OpenALDeviceEnumerator(bool q)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALDeviceEnumerator"/> class.<br/>
        /// Obsolete: Use <see cref="Instance"/> instead.
        /// </summary>
        [Obsolete("Use Instance instead.")]
        public OpenALDeviceEnumerator()
        {
        }

        /// <inheritdoc/>
        public IEnumerable<OpenALDevice> EnumerateDevices()
        {
            var flag = false;
            if (ALC.IsExtensionPresent(ALDevice.Null, "ALC_ENUMERATE_ALL_EXT"))
            {
                flag |= true;
                foreach (var item in ALC.GetString(ALDevice.Null, AlcGetStringList.AllDevicesSpecifier))
                {
                    yield return new OpenALDevice(item);
                }
            }
            if (ALC.IsExtensionPresent(ALDevice.Null, "ALC_ENUMERATION_EXT"))
            {
                flag |= true;
                foreach (var item in ALC.GetString(ALDevice.Null, AlcGetStringList.DeviceSpecifier))
                {
                    yield return new OpenALDevice(item);
                }
            }
            if (!flag) throw new NotSupportedException($"Device Enumeration is not supported on this device!");
        }
        /// <inheritdoc/>
        public IAsyncEnumerable<OpenALDevice> EnumerateDevicesAsync() => (IAsyncEnumerable<OpenALDevice>)EnumerateDevices();
    }
}
