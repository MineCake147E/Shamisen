using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSCore.CoreAudioAPI;
using CSCore.SoundOut;

using Shamisen.IO.Devices;

using CCDataFlow = CSCore.CoreAudioAPI.DataFlow;

namespace Shamisen.IO
{
    /// <summary>
    /// Enumerates the devices available on <see cref="CSCore"/>.
    /// </summary>
    /// <seealso cref="IAudioDeviceEnumerator" />
    public sealed class CSCoreDeviceEnumerator : IAudioDeviceEnumerator
    {
        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" /> asynchronously.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="IAsyncEnumerable{T}" /> of audio devices.
        /// </returns>
#pragma warning disable CS1998 //

        public async IAsyncEnumerable<IAudioDevice> EnumerableDevicesAsync(DataFlow dataFlow)
#pragma warning restore CS1998 //
        {
            foreach (var item in EnumerateDevices(dataFlow))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" />.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="IEnumerable{T}" /> of audio devices.
        /// </returns>
        public IEnumerable<IAudioDevice> EnumerateDevices(DataFlow dataFlow)
        {
            CCDataFlow ccDataFlow = default;
#pragma warning disable S3265 // Non-flags enums should not be used in bitwise operations
            if ((dataFlow & DataFlow.Capture) > 0) ccDataFlow |= CCDataFlow.Capture;
            if ((dataFlow & DataFlow.Render) > 0) ccDataFlow |= CCDataFlow.Render;
#pragma warning restore S3265 // Non-flags enums should not be used in bitwise operations
            if ((dataFlow & DataFlow.Render) > 0)
            {
                foreach (var item in WaveOutDevice.EnumerateDevices())
                {
                    yield return new CSCoreWaveOutDevice(item);
                }
            }
            foreach (var item in MMDeviceEnumerator.EnumerateDevices(ccDataFlow, DeviceState.Active))
            {
                yield return new CSCoreMMDevice(item);
            }
        }
    }
}
