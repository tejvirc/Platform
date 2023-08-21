namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;

    /// <summary>
    ///     Connect event arguments.
    /// </summary>
    internal class ConnectedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectedEventArgs"/> class.
        /// </summary>
        /// <param name="endPoint">The end-point of the client.</param>
        public ConnectedEventArgs(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        /// <summary>
        ///     Gets the end-point of the client.
        /// </summary>
        public IPEndPoint EndPoint { get; }
    }
}
