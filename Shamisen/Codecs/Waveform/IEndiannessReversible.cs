using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.Codecs.Waveform
{
    /// <summary>
    /// Defines a base infrastructure of a chunk of data that is bi-endianed.
    /// </summary>
    /// <typeparam name="TImplementation">The type of an implementation of this interface.</typeparam>
    public interface IEndiannessReversible<out TImplementation> where TImplementation : unmanaged
    {
        /// <summary>
        /// Reverses endianness for all fields, and returns a new value.
        /// </summary>
        /// <returns></returns>
        TImplementation ReverseEndianness();
    }
}
