using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Utils.Memory
{
    /// <summary>
    /// [WIP]Represents a tuple of multiple contiguous memory regions of the same length.
    /// </summary>
    [Obsolete("WIP! DO NOT USE!")]
    public readonly ref struct MutichannelSpan<T> where T : unmanaged
    {
        private readonly ref T head;
    }
}
