using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Shamisen.Formats;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents the support status of certain format in certain device.
    /// </summary>
    public enum FormatSupportStatus : byte
    {
        /// <summary>
        /// The <see cref="IWaveFormat"/> is not supported by both device and binding.
        /// </summary>
        NotSupported,
        /// <summary>
        /// The <see cref="IWaveFormat"/> is supported by the binding, but requires either down-sampling, re-quantization, or both, which may result in lower quality.
        /// </summary>
        SupportedByBinding,
        /// <summary>
        /// The <see cref="IWaveFormat"/> is supported by the hardware and does not require any down-sampling or re-quantization.
        /// </summary>
        SupportedByHardware
    }
}
