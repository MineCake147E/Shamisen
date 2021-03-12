using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs
{
    /// <summary>
    /// Defines a base infrastructure of an instance of encoder.
    /// </summary>
    public interface IComposer<TSample, TFormat> : IDisposable
        where TSample : unmanaged
        where TFormat : IAudioFormat<TSample>
    {
        /// <summary>
        /// Gets the frames written to the <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <value>
        /// The frames written.
        /// </value>
        ulong FramesWritten { get; }

        /// <summary>
        /// Gets the theoretical maximum capacity of the codec.<br/>
        /// The <see langword="null"/> means the codec doesn't have any limitation of size.
        /// </summary>
        /// <value>
        /// The theoretical maximum capacity of the file format.
        /// </value>
        BigInteger? TheoreticalMaxCapacity { get; }
    }
}
