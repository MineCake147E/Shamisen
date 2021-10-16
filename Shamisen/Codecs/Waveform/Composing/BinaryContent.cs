using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Waveform.Composing
{
    /// <summary>
    /// Represents a binary content like <see cref="int"/> and <see cref="float"/>.
    /// </summary>
    /// <seealso cref="IRf64Content" />
    public readonly partial struct BinaryContent : IRf64Content
    {
        /// <summary>
        /// The binary object
        /// </summary>
        public readonly Memory<byte> BinaryObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryContent"/> struct.
        /// </summary>
        /// <param name="binaryObject">The binary object.</param>
        public BinaryContent(Memory<byte> binaryObject)
        {
            BinaryObject = binaryObject;
        }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public ulong Size => (ulong)BinaryObject.Length;

        /// <summary>
        /// Writes this <see cref="IComparable"/> instance to <see cref="IDataSink{TSample}"/>.
        /// </summary>
        /// <param name="sink">The sink.</param>
        public void WriteTo(IDataSink<byte> sink) => sink.Write(BinaryObject.Span);
    }
}
