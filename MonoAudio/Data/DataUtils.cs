using System;
using System.Collections.Generic;
using System.Text;

namespace MonoAudio.Data
{
    /// <summary>
    /// Provides some extensions for <see cref="DataReader{TSample}"/>s.
    /// </summary>
    public static class DataUtils
    {
        /// <summary>
        /// Returns the synchronized data reader.
        /// </summary>
        /// <typeparam name="TSample">The type of the sample.</typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <returns></returns>
        public static SynchronizedDataReader<TSample> Synchronized<TSample>(this DataReader<TSample> dataReader)
            => new SynchronizedDataReader<TSample>(dataReader);
    }
}
