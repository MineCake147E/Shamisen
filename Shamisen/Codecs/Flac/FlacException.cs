using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shamisen.Codecs.Flac
{
    /// <summary>
    /// Represents a FLAC exception.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class FlacException : Exception
    {
        public FlacException()
        {
        }

        public FlacException(string message) : base(message)
        {
        }

        public FlacException(string message, Exception inner) : base(message, inner)
        {
        }

        protected FlacException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
