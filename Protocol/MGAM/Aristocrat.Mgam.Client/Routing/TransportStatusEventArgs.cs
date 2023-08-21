namespace Aristocrat.Mgam.Client.Routing
{
    using System;

    /// <summary>
    ///     Transport status event arguments. 
    /// </summary>
    public class TransportStatusEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportStatusEventArgs"/> class.
        /// </summary>
        /// <param name="status"></param>
        public TransportStatusEventArgs(TransportStatus status)
        {
            Status = status;
        }

        /// <summary>
        ///     Gets the transport status.
        /// </summary>
        public TransportStatus Status { get; }
    }
}
