using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Composing
{
    /// <summary>
    /// Defines a base infrastructure of a composable object.
    /// </summary>
    public interface IComposable
    {
        /// <summary>
        /// Writes this <see cref="IComposable"/> instance to <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <param name="sink">The sink.</param>
        void WriteTo(IDataSink<byte> sink);
    }

    /// <summary>
    /// Defines a base infrastructure of an asynchronously composable object.
    /// </summary>
    public interface IAsyncComposable
    {
        /// <summary>
        /// Writes this <see cref="IComparable"/> instance to <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <param name="sink">The sink.</param>
        ValueTask WriteTo(IDataSink<byte> sink);
    }
}
