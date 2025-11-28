using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Mathematics;

namespace Shamisen.IO.Spatial
{
    /// <summary>
    /// Provides a way to represent a spatial sound listener.
    /// </summary>
    public interface ISpatialSoundListener
    {
        /// <summary>
        /// Gets the current position of the audio source in meters.<br/>
        /// Positive X values are to the right of the listener and negative X values are to the left.<br/>
        /// Positive Y values are above the listener and negative Y values are below.<br/>
        /// Positive Z values are behind of the listener and negative Z values are in front.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets the current reference gain (volume) of the audio source.<br/>
        /// Binding Implementations that have no native support for this value should emulate the effect themselves.
        /// </summary>
        float Gain { get; }

        /// <summary>
        /// Gets the current velocity of the audio source in meters per second.<br/>
        /// Some implementations, like OpenAL bindings, may use this value to apply Doppler effect.<br/>
        /// Binding Implementations that have no native support for this value may ignore this value.<br/>
        /// Positive X values are to the right of the listener and negative X values are to the left.<br/>
        /// Positive Y values are above the listener and negative Y values are below.<br/>
        /// Positive Z values are behind of the listener and negative Z values are in front.
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Gets the current orientation of the listener.<br/>
        /// </summary>
        OrientationVectorPair Orientation { get; }
    }
}
