using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Composing
{
    /// <summary>
    /// Represents a kind of Flac subframes.
    /// </summary>
    public enum FlacSubFrameKind : byte
    {
        /// <summary>
        /// The CONSTANT subframe.
        /// </summary>
        Constant = 0,
        /// <summary>
        /// The VERBATIM subframe.
        /// </summary>
        Verbatim = 1,
        /// <summary>
        /// The FIXED subframe.
        /// </summary>
        Fixed = 0b1000,
        /// <summary>
        /// The LPC subframe.
        /// </summary>
        LinearPrediction = 0b111111,
        /// <summary>
        /// The unknown subframe.
        /// </summary>
        Unknown = byte.MaxValue
    }
}
