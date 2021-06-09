using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Defines a base infrastructure of a FLAC sub-frame.
    /// </summary>
    public interface IFlacSubFrame : IReadSupport<int>, IDisposable
    {
        /// <summary>
        /// Gets the number of wasted LSBs.
        /// </summary>
        int WastedBits { get; }

        /// <summary>
        /// Gets the type of the sub-frame.
        /// </summary>
        byte SubFrameType { get; }
    }
}
