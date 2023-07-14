namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;

    /// <summary>Additional information for report events.</summary>
    /// <seealso cref="EventArgs" />
    public class ReportEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.ReportEventArgs class.</summary>
        /// <param name="buffer">The buffer.</param>
        public ReportEventArgs(byte[] buffer)
            : this(new ReadOnlySpan<byte>(buffer))
        {
        }

        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.ReportEventArgs class.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">Number of.</param>
        public ReportEventArgs(byte[] buffer, int offset, int count)
            : this(new ReadOnlySpan<byte>(buffer, offset, count))
        {
        }

        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.ReportEventArgs class.</summary>
        /// <param name="buffer">The buffer.</param>
        public ReportEventArgs(ReadOnlySpan<byte> buffer)
        {
            ReportId = buffer[0];
            Buffer = buffer[1..].ToArray();
        }

        /// <summary>Gets the identifier of the report.</summary>
        /// <value>The identifier of the report.</value>
        public byte ReportId { get; }

        /// <summary>Gets the buffer.</summary>
        /// <value>The buffer.</value>
        public byte[] Buffer { get; }
    }
}