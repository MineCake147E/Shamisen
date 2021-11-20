using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen
{
    /// <summary>
    /// Defines a base infrastructure of a readable data source.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="IDataSource{TSample}" />
    /// <seealso cref="IReadSupport{TSample}" />
    public interface IReadableDataSource<TSample> : IDataSource<TSample>, IReadSupport<TSample>
        where TSample : unmanaged
    {
    }

    /// <summary>
    /// Defines a base infrastructure of an asynchronously readable data source.
    /// </summary>
    /// <typeparam name="TSample">The type of the sample.</typeparam>
    /// <seealso cref="IDataSource{TSample}" />
    /// <seealso cref="IReadSupport{TSample}" />
    public interface IAsyncReadableDataSource<TSample> : IDataSource<TSample>, IAsyncReadSupport<TSample>
        where TSample : unmanaged
    {
    }
}
