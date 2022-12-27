using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Formats
{
    /// <summary>
    /// Defines a base structure of PCM audio formats.<br/>
    /// </summary>
    public interface IPcmAudioFormat
    {
        /// <summary>
        /// Gets the value indicates how the samples are encoded.
        /// </summary>
        /// <value>
        /// The sample encoding.
        /// </value>
        AudioEncoding Encoding { get; }
    }
}
