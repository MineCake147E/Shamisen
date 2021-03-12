using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Composing;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Defines a base infrastructure of a content of RF64 chunk.
    /// </summary>
    public interface IRf64Content : IComposable
    {
        /// <summary>
        /// Gets the size of this <see cref="IRf64Content"/>.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        ulong Size { get; }
    }
}
