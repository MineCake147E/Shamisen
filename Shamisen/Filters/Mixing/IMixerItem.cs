
using Shamisen.Pipeline;

namespace Shamisen.Filters.Mixing
{
    /// <summary>
    /// Defines a base infrastructure of mixer item.
    /// </summary>
    public interface IMixerItem : ISourceBufferPair
    {
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
    }
}
