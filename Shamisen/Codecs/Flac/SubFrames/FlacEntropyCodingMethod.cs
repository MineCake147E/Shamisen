using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.SubFrames
{
    /// <summary>
    /// Defines some methods for entropy coding in FLAC.
    /// </summary>
    public enum FlacEntropyCodingMethod : byte
    {
        /// <summary>
        /// The partitioned rice
        /// </summary>
        PartitionedRice = 0b00,

        /// <summary>
        /// The partitioned rice2
        /// </summary>
        PartitionedRice2 = 0b01,

        /// <summary>
        /// The reserved10
        /// </summary>
        Reserved10 = 0b10,

        /// <summary>
        /// The reserved11
        /// </summary>
        Reserved11 = 0b11,
    }
}
