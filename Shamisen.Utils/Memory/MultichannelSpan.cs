using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Memory
{
    /// <summary>
    /// [WIP]Represents a tuple of multiple contiguous memory regions of the same length.
    /// </summary>
    [Obsolete("WIP! DO NOT USE!")]
    internal readonly ref struct MultichannelSpan<T> where T : unmanaged
    {
        private readonly ref T head;
    }
}
