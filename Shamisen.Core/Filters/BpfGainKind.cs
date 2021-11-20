using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Filters
{
    /// <summary>
    /// Represents a BPF gain kind.
    /// </summary>
    public enum BpfGainKind
    {
        /// <summary>
        /// Constant skirt gain.
        /// Peak gain = Q
        /// </summary>
        ConstSkirt,

        /// <summary>
        /// Constant 0 dB peak gain.
        /// </summary>
        ZeroDBPeakGain
    }
}
