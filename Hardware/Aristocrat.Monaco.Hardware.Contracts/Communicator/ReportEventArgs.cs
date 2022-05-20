namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using System;
    using System.Linq;

    /// <summary>Additional information for report events.</summary>
    /// <seealso cref="T:System.EventArgs" />
    public class ReportEventArgs : EventArgs
    {
        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.ReportEventArgs class.</summary>
        /// <param name="buffer">The buffer.</param>
        public ReportEventArgs(byte[] buffer)
            : this(buffer, 0, buffer?.Length ?? -1)
        {
        }

        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.ReportEventArgs class.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one or more arguments are outside the required range.</exception>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">Number of.</param>
        public ReportEventArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (count < 1 || buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            ReportId = buffer[offset];
            Buffer = new ArraySegment<byte>(buffer, offset + 1, count - 1).ToArray();
        }

        /// <summary>Gets the identifier of the report.</summary>
        /// <value>The identifier of the report.</value>
        public byte ReportId { get; }

        /// <summary>Gets the buffer.</summary>
        /// <value>The buffer.</value>
        public byte[] Buffer { get; }
    }
}