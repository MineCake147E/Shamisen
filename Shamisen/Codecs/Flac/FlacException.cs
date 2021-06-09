using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Shamisen.Codecs.Flac.Parsing;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a FLAC exception.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class FlacException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        public FlacException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FlacException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public FlacException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="bitReader">The bit reader to dump.</param>
        public FlacException(string message, FlacBitReader bitReader) : base(message + Environment.NewLine + bitReader.Dump())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlacException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected FlacException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
