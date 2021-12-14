using System;
using System.Collections.Generic;
using System.Text;

namespace Shamisen.IO
{
    /// <summary>
    /// Represents an event handler that holds recorded audio data.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="DataAvailableEventArgs"/> instance containing the event data.</param>
    public delegate void DataAvailableEventHandler(object sender, DataAvailableEventArgs e);

    /// <summary>
    /// Represents an event arguments that holds recorded audio data.<br/>
    /// It is a <c>struct</c> which has <c>ref</c> and <c>readonly</c> modifier because the event occurs frequently and has a <see cref="Span{T}"/> to deliver a raw buffer.<br/>
    /// <b>CAUTION! REF STRUCT! IT CANNOT BE STORED ON HEAPS!</b>
    /// </summary>
    public readonly ref struct DataAvailableEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAvailableEventArgs" /> struct.
        /// </summary>
        /// <param name="data">The data.</param>
        public DataAvailableEventArgs(Span<byte> data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public Span<byte> Data { get; }
    }
}
