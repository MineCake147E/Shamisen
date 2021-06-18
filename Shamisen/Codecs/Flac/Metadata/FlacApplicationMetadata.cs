using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents an application metadata in FLAC files.
    /// </summary>
    public readonly struct FlacApplicationMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacApplicationMetadata"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="data">The data.</param>
        public FlacApplicationMetadata(VectorB4 id, ReadOnlyMemory<byte> data)
        {
            Id = id;
            Data = data;
        }

        /// <summary>
        /// Gets the registered application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public VectorB4 Id { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public ReadOnlyMemory<byte> Data { get; }
    }
}
