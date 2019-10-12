using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.IO
{
    /// <summary>
    /// Enumerates the AL devices.
    /// </summary>
    public sealed class ALDeviceEnumerator : IAudioDeviceEnumerator
    {
        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow" />.
        /// </summary>
        /// <param name="dataFlow">The <see cref="T:MonoAudio.IO.DataFlow" /> kind to enumerate devices of.</param>
        /// <returns>
        /// The <see cref="T:System.Collections.Generic.IEnumerable`1" /> of audio devices.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// </exception>
        /// <exception cref="NotImplementedException"></exception>
        public IEnumerable<IAudioDevice> EnumerateDevices(DataFlow dataFlow)
        {
            switch (dataFlow)
            {
                case DataFlow.None:
                    throw new NotSupportedException();
                case DataFlow.Render:

                    break;
                case DataFlow.Capture:
                    throw new NotSupportedException();
                default:
                    throw new NotSupportedException();
            }
            //TODO: Implementation
            throw new NotImplementedException();
        }
    }
}
