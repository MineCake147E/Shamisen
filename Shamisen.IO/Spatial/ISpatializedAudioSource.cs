using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISpatializedAudioSource
    {
        /// <summary>
        /// Gets the value indicating whether the audio source should be spatialized or not.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        bool Spatialize { get; }

        /// <summary>
        /// Gets the value indicating whether the position is relative to the listener or absolute in the world space.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        bool IsRelative { get; }

        /// <summary>
        /// Gets the distance at witch the listener would experience the reference gain, in meters.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        float ReferenceDistance { get; }

        /// <summary>
        /// Gets the value which controls how the audio source attenuates with distance.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        float RollOffFactor { get; }

        /// <summary>
        /// Gets the mask indicating which temporal properties have been updated at the time of the last <see cref="IReadSupport{TSample}.Read(Span{TSample})"/> call.
        /// </summary>
        TemporalPropertiesUpdatedMasks UpdatedTemporalPropertiesMask => unchecked((TemporalPropertiesUpdatedMasks)~0);

        /// <summary>
        /// Gets the temporal properties of the audio source at the time of the last <see cref="IReadSupport{TSample}.Read(Span{TSample})"/> call.
        /// </summary>
        SpatialAudioSourceTemporalProperties TemporalProperties { get; }
    }
}
