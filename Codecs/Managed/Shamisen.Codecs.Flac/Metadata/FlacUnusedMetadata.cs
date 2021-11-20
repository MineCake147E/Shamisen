using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac.Metadata
{
    /// <summary>
    /// Represents an unused metadata in FLAC files.
    /// </summary>
    public readonly struct FlacUnusedMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacUnusedMetadata"/> struct.
        /// </summary>
        /// <param name="metadataBlockType">Type of the metadata block.</param>
        /// <param name="data">The data.</param>
        public FlacUnusedMetadata(FlacMetadataBlockType metadataBlockType, ReadOnlyMemory<byte> data)
        {
            MetadataBlockType = metadataBlockType;
            Data = data;
        }

        /// <summary>
        /// Gets the type of the metadata block.
        /// </summary>
        /// <value>
        /// The type of the metadata block.
        /// </value>
        public FlacMetadataBlockType MetadataBlockType { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public ReadOnlyMemory<byte> Data { get; }
    }
}
