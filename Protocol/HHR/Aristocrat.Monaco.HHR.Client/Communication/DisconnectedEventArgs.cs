namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;
    using System.Net;

    /// <summary>
    ///     Disconnect event arguments.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisconnectedEventArgs" /> class.
        /// </summary>
        /// <param name="endPoint">The end-point of the client.</param>
        public DisconnectedEventArgs(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        /// <summary>
        ///     Gets the end-point of the client.
        /// </summary>
        public IPEndPoint EndPoint { get; }
    }
}