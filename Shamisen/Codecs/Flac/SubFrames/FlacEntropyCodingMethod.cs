using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.SubFrames
{
    public enum FlacEntropyCodingMethod : byte
    {
        PartitionedRice = 0b00,
        PartitionedRice2 = 0b01,
        Reserved10 = 0b10,
        Reserved11 = 0b11,
    }
}
