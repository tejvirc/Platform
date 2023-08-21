namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using System.Net;
    using Kernel;

    /// <summary>
    ///     Published when the connection to VLT Service is lost.
    /// </summary>
    public class ConnectionLostEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionLostEvent"/> class.
        /// </summary>
        /// <param name="endPoint"></param>
        public ConnectionLostEvent(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        /// <summary>
        ///     Gets the VLT Service end point address.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[{GetType().FullName} (EndPoint: {EndPoint})]";
        }
    }
}
