namespace Aristocrat.Mgam.Client.Routing
{
    using System.Net;

    /// <summary>
    ///     Transport status.
    /// </summary>
    public readonly struct TransportStatus
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportStatus"/> class.
        /// </summary>
        /// <param name="transportState"><see cref="T:Aristocrat.Mgam.Client.Routing.TransportState" />.</param>
        /// <param name="broadcast">True is source is broadcast status.</param>
        /// <param name="endPoint">IP end point address.</param>
        /// <param name="connectionState"><see cref="ConnectionState" />.</param>
        /// <param name="failure"><see cref="TransportFailure" />.</param>
        public TransportStatus(TransportState transportState, bool broadcast, IPEndPoint endPoint, ConnectionState connectionState, TransportFailure failure = TransportFailure.None)
        {
            TransportState = transportState;
            IsBroadcast = broadcast;
            EndPoint = endPoint;
            ConnectionState = connectionState;
            Failure = failure;
        }

        /// <summary>
        ///     Gets the transport state.
        /// </summary>
        public TransportState TransportState { get; }

        /// <summary>
        ///     Gets the transport event.
        /// </summary>
        public TransportFailure Failure { get; }

        /// <summary>
        ///     Gets a value that indicates whether the end point is for broadcasting.
        /// </summary>
        public bool IsBroadcast { get; }

        /// <summary>
        ///     Gets the transport end point.
        /// </summary>
        public IPEndPoint EndPoint { get; }

        /// <summary>
        ///     Gets the transport end point state.
        /// </summary>
        public ConnectionState ConnectionState { get; }
    }
}
