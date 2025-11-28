using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.ALC;

using StringName = OpenTK.Audio.OpenAL.ALC.StringName;

namespace Shamisen.IO.OpenTK.OpenAL
{
    /// <summary>
    /// Enumerates the AL devices.
    /// </summary>
    public sealed class OpenALCDeviceEnumerator : IAudioOutputDeviceEnumerator<OpenALOutputDevice, OpenALOutput, OpenALOutputConfiguration, OpenALOutputConfigurationBuilder>
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly OpenALCDeviceEnumerator Instance = new(false);

        private OpenALCDeviceEnumerator(bool q)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenALCDeviceEnumerator"/> class.<br/>
        /// Obsolete: Use <see cref="Instance"/> instead.
        /// </summary>
        [Obsolete("Use Instance instead.")]
        public OpenALCDeviceEnumerator()
        {
        }

        /// <inheritdoc/>
        public IEnumerable<OpenALOutputDevice> EnumerateDevices()
        {
            var flag = false;
            var list = new List<string>();
            if (ALC.IsExtensionPresent(ALCDevice.Null, "ALC_ENUMERATE_ALL_EXT"))
            {
                flag |= true;
                unsafe
                {
                    OpenALUtils.FromALStringList(ALC.GetString_(ALCDevice.Null, StringName.AllDevicesSpecifier), list);
                }
            }
            if (ALC.IsExtensionPresent(ALCDevice.Null, "ALC_ENUMERATION_EXT"))
            {
                flag |= true;
                unsafe
                {
                    OpenALUtils.FromALStringList(ALC.GetString_(ALCDevice.Null, StringName.DeviceSpecifier), list);
                }
            }
            foreach (var item in list)
            {
                if (OpenALOutputDevice.TryCreateDevice(item, out var device))
                {
                    yield return device;
                }
            }
            if (!flag) throw new NotSupportedException($"Device Enumeration is not supported on this device!");
        }
        /// <inheritdoc/>
        public IAsyncEnumerable<OpenALOutputDevice> EnumerateDevicesAsync() => (IAsyncEnumerable<OpenALOutputDevice>)EnumerateDevices();
    }
}
