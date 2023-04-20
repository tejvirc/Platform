namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    using Gds;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Definition of the IGdsCommunicator interface.
    /// </summary>
    public interface IGdsCommunicator : ICommunicator
    {
        /// <summary>
        ///     Event when a data message is received from GDS device.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <summary>
        ///     Send a data message to GDS device.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="token">The cancellation token</param>
        void SendMessage(GdsSerializableMessage message, CancellationToken token);
    }
}