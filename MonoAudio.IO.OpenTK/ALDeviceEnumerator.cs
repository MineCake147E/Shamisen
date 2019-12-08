using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Audio.OpenAL;


namespace MonoAudio.IO
{
    /// <summary>
    /// Enumerates the AL devices.
    /// </summary>
    public sealed class ALDeviceEnumerator : IAudioDeviceEnumerator
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly ALDeviceEnumerator Instance = new ALDeviceEnumerator(false);

        private ALDeviceEnumerator(bool q)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ALDeviceEnumerator"/> class.<br/>
        /// Obsolete: Use <see cref="Instance"/> instead.
        /// </summary>
        [Obsolete("Use Instance instead.")]
        public ALDeviceEnumerator()
        {
        }

        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" />.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="IEnumerable{T}" /> of audio devices.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<IAudioDevice> EnumerateDevices(DataFlow dataFlow)
        {
            switch (dataFlow)
            {
                case DataFlow.None:
                    throw new InvalidOperationException($"The {nameof(dataFlow)} is not specified!");
                default:
                    if ((dataFlow & DataFlow.Capture) > 0)
                    {
                        throw new NotSupportedException($"Capturing is not currently supported!");
                    }
                    if ((dataFlow & DataFlow.Render) > 0)
                    {
                        bool flag = false;
                        if (Alc.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATE_ALL_EXT"))
                        {
                            flag |= true;
                            foreach (var item in Alc.GetString(IntPtr.Zero, AlcGetStringList.AllDevicesSpecifier))
                            {
                                yield return new ALDevice(item);
                            }
                        }
                        if (Alc.IsExtensionPresent(IntPtr.Zero, "ALC_ENUMERATION_EXT"))
                        {
                            flag |= true;
                            foreach (var item in Alc.GetString(IntPtr.Zero, AlcGetStringList.DeviceSpecifier))
                            {
                                yield return new ALDevice(item);
                            }
                        }
                        if (!flag) throw new NotSupportedException($"Device Enumeration is not supported on this device!");
                    }
                    break;
            }

        }
    }
}
