using System;
using System.Threading.Tasks;

using Shamisen.Pipeline;
using Shamisen.Utils;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Defines a base infrastructure of mixer item.
    /// </summary>
    public interface IMixerItem : ISampleSource
    {
        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        ISampleSource Source { get; }

        /// <summary>
        /// Gets or sets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        float Volume { get; set; }

        /// <summary>
        /// Gets the source properties.
        /// </summary>
        /// <value>
        /// The source properties.
        /// </value>
        AudioSourceProperties<float, SampleFormat> SourceProperties { get; }

        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>
        /// The buffer.
        /// </value>
        Memory<float> Buffer { get; }

        /// <summary>
        /// Checks and stretches the buffer.
        /// </summary>
        /// <param name="length">The length.</param>
        void CheckBuffer(int length);
    }
}
