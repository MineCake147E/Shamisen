using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Audio.OpenAL;

namespace Shamisen.IO
{
    /// <summary>
    /// Enumerates the AL devices.
    /// </summary>
    public sealed class OpenALDeviceEnumerator : IAudioDeviceEnumerator
    {
        /// <summary>
        /// The instance
        /// </summary>
        public static readonly OpenALDeviceEnumerator Instance = new OpenALDeviceEnumerator(false);

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

        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" />.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="IEnumerable{T}" /> of audio devices.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
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
                    break;
            }
        }

        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" /> asynchronously.
        /// </summary>
        /// <param name="dataFlow">The <see cref="T:Shamisen.IO.DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="IAsyncEnumerable{T}" /> of audio devices.
        /// </returns>
#pragma warning disable CS1998

        public async IAsyncEnumerable<IAudioDevice> EnumerateDevicesAsync(DataFlow dataFlow)
#pragma warning restore CS1998
        {
            foreach (var item in EnumerateDevices(dataFlow))
            {
                yield return item;
            }
        }
    }
}
