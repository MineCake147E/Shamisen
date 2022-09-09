using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Shamisen.Data
{
    /// <summary>
    /// The exception that is thrown when a reader of <see cref="IDataSource{TSample}"/> has ran out of buffered data.
    /// </summary>
    /// <seealso cref="IOException" />
    [Serializable]
    public class BufferingException : IOException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        public BufferingException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <inheritdoc/>
        public BufferingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <inheritdoc/>
        public BufferingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <inheritdoc/>
        protected BufferingException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferingException"/> class.
        /// </summary>
        /// <inheritdoc/>
        public BufferingException(string? message, int hresult) : base(message, hresult)
        {
        }
    }
}
