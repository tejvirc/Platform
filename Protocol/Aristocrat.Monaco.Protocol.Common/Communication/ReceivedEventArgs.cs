namespace Aristocrat.Monaco.Protocol.Common
{
    using System;

    /// <summary>
    ///     Received event arguments.
    /// </summary>
    public class ReceivedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes and instance of the <see cref="ReceivedEventArgs" /> class.
        /// </summary>
        /// <param name="payload">The end-point of the client.</param>
        public ReceivedEventArgs(Packet payload)
        {
            Payload = payload;
        }

        /// <summary>
        ///     Gets the payload from the host.
        /// </summary>
        public Packet Payload { get; }
    }
}