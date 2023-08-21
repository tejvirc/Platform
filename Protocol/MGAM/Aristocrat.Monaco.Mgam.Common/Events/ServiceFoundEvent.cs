namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using System.Net;
    using Kernel;

    /// <summary>
    ///     Published when VLT Service is located.
    /// </summary>
    public class ServiceFoundEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceFoundEvent"/> class.
        /// </summary>
        public ServiceFoundEvent(IPEndPoint endPoint)
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
