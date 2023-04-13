using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Conversion.SampleToWaveConverters
{
    /// <summary>
    /// Defines some options for <see cref="SampleToPcm16Converter"/> and its close relatives.
    /// </summary>
    [Flags]
    public enum SampleToLinearPcmConversionPolicy
    {
        /// <summary>
        /// The normal conversion.
        /// </summary>
        None = 0,
        /// <summary>
        /// The conversion will be noise-shaped.
        /// </summary>
        DeltaSigmaModulation = 1,
        /// <summary>
        /// The conversion will be allowed to utilize Fused-Multiply-Add for better performance.
        /// </summary>
        AllowFusedMultiplyAdd = 2,
        /// <summary>
        /// The output must be BIG-ENDIAN if the system is little-endian.<br/>
        /// The output must be little-endian if the system is BIG-ENDIAN.
        /// </summary>
        ReversedOutput = 4,
    }
}
