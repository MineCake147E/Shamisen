using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Defines a base infrastructure of an audio device enumerator.
    /// </summary>
    public interface IAudioDeviceEnumerator
    {
        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow"/>.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow"/> kind to enumerate devices of.</param>
        /// <returns>The <see cref="IEnumerable{T}"/> of audio devices.</returns>
        IEnumerable<IAudioDevice> EnumerateDevices(DataFlow dataFlow);

        /// <summary>
        /// Enumerates devices of specified <paramref name="dataFlow"/> asynchronously.
        /// </summary>
        /// <param name="dataFlow">The <see cref="DataFlow"/> kind to enumerate devices of.</param>
        /// <returns>The <see cref="IAsyncEnumerable{T}"/> of audio devices.</returns>
        IAsyncEnumerable<IAudioDevice> EnumerateDevicesAsync(DataFlow dataFlow);
    }
}