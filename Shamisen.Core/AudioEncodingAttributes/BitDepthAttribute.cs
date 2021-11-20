using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen
{
    /// <summary>
    /// Specifies the Bit Depth of an audio encoding format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class BitDepthAttribute : AudioEncodingAttribute
    {
    }

    /// <summary>
    /// Specifies the audio encoding format supports only fixed bit depth.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class FixedBitDepthAttribute : BitDepthAttribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedBitDepthAttribute"/> class.
        /// </summary>
        public FixedBitDepthAttribute()
        {
        }
    }
}
