namespace Aristocrat.Mgam.Client.Routing
{
    using System;
    using System.Net;

    /// <summary>
    ///     Error event arguments.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="endPoint">The end-point of the client.</param>
        /// <param name="exception">The error exception.</param>
        public ErrorEventArgs(IPEndPoint endPoint, Exception exception)
        {
            EndPoint = endPoint;
            Exception = exception;
        }

        /// <summary>
        ///     Gets the end-point of the client.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        ///     Gets the error exception.
        /// </summary>
        public Exception Exception { get; }
    }
}
